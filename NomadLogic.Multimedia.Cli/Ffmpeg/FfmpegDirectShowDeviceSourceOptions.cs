using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using NLog;
using NomadLogic.Common.Util;

namespace NomadLogic.Multimedia.Cli.Ffmpeg
{
    /// <summary>
    /// Représente les options d'une source DirectShow. La nature de cette source peut varier (habituellement vidéo ou audio),
    /// mais toutes les sources ont un "Pin".
    /// L'implémentation de cette classe défini les propriétés qui sont lues.
    /// </summary>
    public abstract class FfmpegDirectShowDeviceSourceOptions
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Créer une instance valide de cet objet (une classe enfant donc) en analysant le contenu d'un fichier texte, résultat
        /// d'une requête FFMPEG.
        /// </summary>
        /// <param name="arg">Le fichier contenant les lignes issues de la commande.</param>
        /// <returns></returns>
        [SuppressMessage("ReSharper", "RedundantJumpStatement"), SuppressMessage("ReSharper", "InvertIf")]
        public static FfmpegDirectShowDeviceSourceOptions FromTextFile(FileInfo arg)
        {
            /* Example de output (après la déclaration des libraries) pour un type VIDEO
            [dshow @ 037e32a0] DirectShow video device options (from video devices)
            [dshow @ 037e32a0]  Pin "Capture" (alternative pin name "0")
            [dshow @ 037e32a0]   pixel_format=yuyv422  min s=640x480 fps=5 max s=640x480 fps=30
            [dshow @ 037e32a0]   pixel_format=yuyv422  min s=640x480 fps=5 max s=640x480 fps=30
            [dshow @ 037e32a0]   vcodec=mjpeg  min s=640x480 fps=5 max s=640x480 fps=30
            [dshow @ 037e32a0]   vcodec=mjpeg  min s=640x480 fps=5 max s=640x480 fps=30
             */

            if (arg == null || !File.Exists(arg.FullName))
            {
                return null;
            }

            FfmpegDirectShowDeviceSourceOptions md = null;

            var lines = File.ReadAllLines(arg.FullName);

            // On parse chaque ligne
            foreach (var line in lines)
            {
                var l = line.Trim();

                if (!l.StartsWith("[dshow @")) continue;

                var trimmed = l.Substring(l.IndexOf(']')+1).TrimStart();

                if (trimmed.StartsWith("DirectShow video device"))
                {
                    md = new FfmpegDirectShowDeviceSourceVideoOptions();
                    continue;
                }

                if (trimmed.StartsWith("DirectShow audio device"))
                {
                    md = new FfmpegDirectShowDeviceSourceAudioOptions();
                    continue;
                }

                if (trimmed.StartsWith("Pin")) // Un nom de "pin" va suivre, ainsi qu'une référence vers ce "pin"
                {
                    if (md == null)
                    {
                        // Erreur de machine d'état
                        log.Warn("Error while parsing DirectShow device options. Device Pin detected, but device nature detection failed. Skipping.");
                        continue;
                    }

                    var cut = trimmed.Split('"');
                    // Devrait y avoir 5 éléments
                    if (cut.Length != 5)
                    {
                        log.Warn("Could not correctly cut Pin data string. Skipping. string=" + trimmed);
                        continue;
                    }

                    md.PinName = cut[1];
                    md.PinAlternativeName = cut[3];
                    continue;
                }

                // Le reste des lignes est une concaténation de propriétés. À gérer par l'instance.
                if (md == null)
                {
                    // Erreur de machine d'état
                    log.Warn("Error while parsing DirectShow device options. Device property detected, but device nature detection failed. Skipping.");
                    continue;
                }

                md.AddOptionFromPropertyString(trimmed);
            }

            return md;
        }

        protected abstract void AddOptionFromPropertyString([NotNull] string props);

        public string PinAlternativeName { get; set; }

        public string PinName { get; set; }
    }

    /// <summary>
    /// Description des propriétés d'une source DirectShow vidéo.
    /// </summary>
    public class FfmpegDirectShowDeviceSourceVideoOptions : FfmpegDirectShowDeviceSourceOptions
    {
        private static Regex _regexVideoProps;

        protected override void AddOptionFromPropertyString(string props)
        {
            /* Examples de lignes à analyser
            pixel_format=yuyv422  min s=640x480 fps=5 max s=640x480 fps=30
            vcodec=mjpeg  min s=640x480 fps=5 max s=640x480 fps=30
            */

            if (_regexVideoProps == null)
            {
                _regexVideoProps = new Regex(@"[\w]*=([\w]*(\.[\w]*)?)", RegexOptions.Compiled);
            }
            
            var data = new FfmpegDirectShowDeviceSourceVideoOptionData();
            foreach (Match prop in _regexVideoProps.Matches(props))
            {
                var val = prop?.Value.Split('=');

                if (val == null || val.Length < 2) continue;
                
                switch (val[0])
                {
                    case "vcodec": data.VideoCodec = val[1]; break;
                    case "pixel_format": data.PixelFormat = val[1]; break;
                    case "s": data.FrameSize = StringUtil.SizeFromStringDefinition(val[1]); break;
                    case "fps": data.FpsRange.Add(double.Parse(val[1],CultureInfo.InvariantCulture)); break;
                }
            }
            VideoOptions.Add(data);
        }

        public IList<FfmpegDirectShowDeviceSourceVideoOptionData> VideoOptions { get; } = new List<FfmpegDirectShowDeviceSourceVideoOptionData>();
    }

    public class FfmpegDirectShowDeviceSourceVideoOptionData
    {
        /// <summary>
        /// Représente une liste de deux éléments à virgule flottante, où le premier élément est la vitesse maximale,
        /// et l'autre la vitesse minimale du nombre de trames par seconde.
        /// </summary>
        public IList<double> FpsRange { get; } = new List<double>();

        public string FpsMaxDesc {
            get
            {
                if (FpsRange.Count < 2) return "N/A";
                return FpsRange[1].ToString("G29", CultureInfo.InvariantCulture);
            }
        }

        /// <summary>
        /// La taille de la trame de cette option.
        /// </summary>
        public Size FrameSize { get; set; } = Size.Empty;

        public string SizeDesc {
            get
            {
                if (FrameSize == Size.Empty) return "N/A";
                return $"{FrameSize.Width}x{FrameSize.Height}";
            }
        }

        /// <summary>
        /// Le format de pixel (comme format codec vidéo). Si null, habituellement on a un nom de codec pour compenser.
        /// </summary>
        public string PixelFormat { get; set; }

        /// <summary>
        /// Le format de codec pour chaque trame, selon la nature du flux. Si null, habituellement on a un format de pixel précis
        /// pour compenser.
        /// </summary>
        public string VideoCodec { get; set; }

        public override string ToString()
        {
            // Notation G29 expliquée: https://stackoverflow.com/questions/4525854/remove-trailing-zeros
            return $"Size:{SizeDesc}, FPS:{FpsMaxDesc}, Codec:{VideoCodec ?? "N/A"}, Pixel Format:{PixelFormat ?? "N/A"}";
        }
    }

    public class FfmpegDirectShowDeviceSourceAudioOptions : FfmpegDirectShowDeviceSourceOptions
    {
        protected override void AddOptionFromPropertyString(string props)
        {
            // Pas encore besoin de faire cette étape pour les sources audio.
            throw new System.NotImplementedException();
        }
    }
}