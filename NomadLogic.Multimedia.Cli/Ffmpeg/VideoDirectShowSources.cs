using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using NLog;

namespace NomadLogic.Multimedia.Cli.Ffmpeg
{
    /// <summary>
    /// Liste les sources Vidéo et Audio DIrect Show détectable depuis FFMPEG.
    /// Doit s'initialiser avec un fichier contenant le résultat d'une requête FFMPEG.
    /// On peut mettre à jour les informations précises de cette liste avec une autre requête (par source).
    /// </summary>
    [PublicAPI]
    public class VideoDirectShowSources
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private readonly List<FfmpegDirectShowDeviceSource> _list = new List<FfmpegDirectShowDeviceSource>();

        protected VideoDirectShowSources()
        {

        }

        /// <summary>
        /// Liste les sources qui sont identifiées comme vidéo.
        /// </summary>
        public List<FfmpegDirectShowDeviceSource> VideoDevices
        {
            get
            {
                return _list
                    .Where(dsd => dsd.DeviceType == FfmpegDirectShowDeviceSourceType.Video)
                    .ToList();
            }
        }

        /// <summary>
        /// Liste les sources qui sont identifiées comme audio.
        /// </summary>
        public List<FfmpegDirectShowDeviceSource> AudioDevices
        {
            get
            {
                return _list
                    .Where(dsd => dsd.DeviceType == FfmpegDirectShowDeviceSourceType.Audio)
                    .ToList();
            }
        }

        [SuppressMessage("ReSharper", "RedundantJumpStatement"), SuppressMessage("ReSharper", "InvertIf")]
        public static VideoDirectShowSources FromTextFile(FileInfo arg)
        {
            /* Example de output (après la déclaration des libraries)
            [dshow @ 059f32a0] DirectShow video devices (some may be both video and audio devices)
            [dshow @ 059f32a0]  "EOS Webcam Utility"
            [dshow @ 059f32a0]     Alternative name "@device_pnp_\\?\root#eoswebcamsource#0000#{65e8773d-8f56-11d0-a3b9-00a0c9223196}\eoswebcamcamerasource"
            [dshow @ 059f32a0]  "c922 Pro Stream Webcam"
            [dshow @ 059f32a0]     Alternative name "@device_pnp_\\?\usb#vid_046d&pid_085c&mi_00#7&3b1e4642&0&0000#{65e8773d-8f56-11d0-a3b9-00a0c9223196}\global"
            [dshow @ 059f32a0] DirectShow audio devices
            [dshow @ 059f32a0]  "Microphone (C922 Pro Stream Webcam)"
            [dshow @ 059f32a0]     Alternative name "@device_cm_{33D9A762-90C8-11D0-BD43-00A0C911CE86}\wave_{9C68F08C-FDB1-4FB5-B841-A0379F0B1462}"
             */

            if (arg == null || !File.Exists(arg.FullName))
            {
                return null;
            }

            var md = new VideoDirectShowSources();

            var lines = File.ReadAllLines(arg.FullName);
            var insideVideoList = false;
            var insideAudioList = false;
            FfmpegDirectShowDeviceSource currentDevice = null;

            // On parse chaque ligne
            foreach (var line in lines)
            {
                var l = line.Trim();

                if (!l.StartsWith("[dshow")) continue;

                var trimmed = l.Substring(l.IndexOf(']')+1).TrimStart();

                if (trimmed.StartsWith("DirectShow video devices"))
                {
                    currentDevice = null;
                    insideVideoList = true;
                    insideAudioList = false;
                    continue;
                }

                if (trimmed.StartsWith("DirectShow audio devices"))
                {
                    currentDevice = null;
                    insideAudioList = true;
                    insideVideoList = false;
                    continue;
                }

                if (trimmed.StartsWith("\"")) // Un nom de devices va suivre
                {
                    if (insideVideoList)
                    {
                        currentDevice = new FfmpegDirectShowDeviceSource
                        {
                            DeviceType = FfmpegDirectShowDeviceSourceType.Video
                        };
                        md.AddDirectShowSource(currentDevice);
                    } 
                    else if (insideAudioList)
                    {
                        currentDevice = new FfmpegDirectShowDeviceSource
                        {
                            DeviceType = FfmpegDirectShowDeviceSourceType.Audio
                        };
                        md.AddDirectShowSource(currentDevice);
                    }
                    else
                    {
                        // Erreur de machine d'état
                        log.Warn("Error while parsing DirectShow device list. Device name detected, but list header detection failed. Skipping.");
                        continue;
                    }

                    var deviceName = trimmed.Trim('"'); // Enlève le premier et dernier caractère, qui devrait être double_quote
                    currentDevice.Name = deviceName;
                    continue;
                }

                const string alternativeNamePrefix = "Alternative name ";
                if (trimmed.StartsWith(alternativeNamePrefix)) // Une référence de "hook" USB va suivre
                {
                    if (currentDevice == null)
                    {
                        // Erreur de machine d'état
                        log.Warn("Error while parsing DirectShow device list. Device not initialized, but alternative name detected. Skipping.");
                        continue;
                    }
                    
                    var aName = trimmed.Substring(alternativeNamePrefix.Length).Trim('"');
                    currentDevice.AlternativeName = aName;
                    continue;
                }

                // Ligne de type inconnu...
                log.Warn("Unknown line data format. Skipping. string=" + trimmed);
            }

            return md;
        }

        private void AddDirectShowSource(FfmpegDirectShowDeviceSource currentDevice)
        {
            _list.Add(currentDevice);
        }

        /// <summary>
        /// Fait une recherche du premier candidats de cette instance pour trouver une source et canal vidéo qui correspondent
        /// aux critère de qualité nommées.
        /// Si on en trouve un, on retourne TRUE ainsi que des référence sur les objets trouvés.
        /// Le concept de "maximal minimal" est le suivant: On cherche la valeur de vitesse de trame maximale la plus petite.
        /// Par exemple, si on veut 30 FPS, et que l'on a plusieures options (comme 30, 60 et 75), on va choisir l'option 30, la plus petite des options
        /// de vitesses "maximal". Car chaque canal donne sa vitesse en format min-max=, comme 5-30, 10-60, 20-75, etc...
        /// </summary>
        /// <param name="desiredSize">Le format EXACT de la taille de trame voulue.</param>
        /// <param name="desiredMaxFps">La vitesse de trame 'maximale minimale' pour être choisie.</param>
        /// <param name="sourceCandidate">Si trouvée, la référence de source vidéo.</param>
        /// <param name="videoCanalCandidate">Si trouvée, les options du canal vidéo.</param>
        /// <param name="execLocation">Si on veut mettre à jour les options Direct Show de façon "au besoin", on doit passer l'exécutable FFMPEG ici.</param>
        /// <returns>TRUE, si on a trouvé un candidat acceptable.</returns>
        public bool GetBestVideoFeed(Size desiredSize, double desiredMaxFps, 
            out FfmpegDirectShowDeviceSource sourceCandidate, 
            out FfmpegDirectShowDeviceSourceVideoOptionData videoCanalCandidate,
            string execLocation = null)
        {
            var vidSources = VideoDevices;

            var minFpsValueFound = double.MaxValue;
            sourceCandidate = null;
            videoCanalCandidate = null;

            foreach (var v in vidSources)
            {
                if (v.Options == null)
                {
                    if (execLocation != null)
                    {
                        // Initialization "lazy" des options de cette source vidéo.
                        v.Options = FfmpegUtil.UpdateDirectShowSourceData(execLocation, v);
                        if (v.Options == null)
                        {
                            log.Error("Lazy initialisation of Direct Show source failed: " + v.Name);
                            return false;
                        }
                    }
                }

                if (!(v.Options is FfmpegDirectShowDeviceSourceVideoOptions vOpt)) continue;

                foreach (var vo in vOpt.VideoOptions)
                {
                    if (!vo.FrameSize.Equals(desiredSize)) continue;
                    var fps = vo.FpsRange[1];
                    if (!(fps >= desiredMaxFps)) continue;

                    if (!(fps < minFpsValueFound)) continue; // Candidat possible, mais précédent candidat meilleur.
                    
                    minFpsValueFound = fps;
                    sourceCandidate = v;
                    videoCanalCandidate = vo;
                }
            }
            
            return minFpsValueFound < double.MaxValue; // Indique que l'On a trouvé au moins candidat.
        }
    }
}