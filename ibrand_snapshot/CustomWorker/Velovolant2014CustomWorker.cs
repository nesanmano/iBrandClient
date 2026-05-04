using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using NomadLogic.iBrand.Common.Services;
using NomadLogic.iBrand.Common.Transformation.Video.AviMaker;
using NomadLogic.iBrand.Common.Transformation.Video.Ffmpeg;
using NomadLogic.iBrand.Common.Util;

namespace iBrand_SnapShot.CustomWorker
{
    /// <summary>
    /// Classe de traitement pour le vidéo Vélovolant 2014.
    /// <p/>Cette classe doit être capable de lire un fichier de configuration dans la racine d'exécution nommé "config.txt".
    /// <br/>Ce fichier doit contenir les propriétés suivantes:
    /// <li>thumbnail_time</li>
    /// <li>gallery_dir</li>
    /// <li>save_dir</li>
    /// <li>save_frame</li>
    /// <li>log_all</li>
    /// <li>archive_dir</li>
    /// <li>asset_dir</li>
    /// </summary>
    internal class Velovolant2014CustomWorker : CustomWorker<SnapShotCustomWorkerParameter, FileInfo>
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private static readonly string noGallery = "No gallery directory found. Cannot continue video transformation.";

        private int _thumbnailTime;
        private string _galleryDir;
        public string SaveDir { get; private set; }
        private bool _saveFrame;
        private bool _log;
        private string _archiveDir;
        private string _assetDir;

        public override void DoPreparationWork()
        {
            GetTransformationProperties();
            if (!Directory.Exists(_galleryDir))
            {
                if (LoggerReference == null)
                {
                    log.Debug(noGallery);
                }
                else
                {
                    LoggerReference.LogEvent(noGallery);
                }
                throw new Exception(noGallery);
            }

            string fichierOriginal = Parameter.WorkFile.FullName;
            var videoMovToAviName = fichierOriginal + ".avi";
            var fichierFinal = fichierOriginal + ".flv";

            FfmpegUtil.MovToAvi(fichierOriginal, videoMovToAviName, Parameter.Settings.LocationFfmpeg, 1280, 720);

            // On tente de faire un thumbnail.
            if (Directory.Exists(_galleryDir))
            {
                var thumbnail = _galleryDir + DateUtil.GetCurrentTimeMilli() + "_" + StringUtil.GetFileName(fichierFinal) + ".jpg";
                if (File.Exists(thumbnail))
                {
                    File.Delete(thumbnail);
                }
                var tg = new ThumbnailsGenerator {CustomFFmpegLocation = Parameter.Settings.LocationFfmpeg};
                tg.GenerateThumbnail(videoMovToAviName, thumbnail, _thumbnailTime);
                File.Move(thumbnail, _galleryDir + StringUtil.GetFileName(thumbnail));
            }

            // On met à jour le fichier d'état (status.txt) lisible par la page admin de la gallerie.
            var contenu = new List<string>
            {
                "Last worker sent to pool: " + DateTime.Now,
                "Pool size at that time (working/total) : " + Pool.GetCurrentWorkerCount() + "/" + Pool.GetQueueSize()
            };
            try
            {
                using (var file = new StreamWriter(_galleryDir + "status.txt"))
                {
                    foreach (var line in contenu)
                    {
                        file.WriteLine(line);
                    }
                }
            }
            catch (Exception e)
            {
                log.Error(e);
            }
        }

        public override void DoWork()
        {
            // Vérification des répertoires
            if (!Directory.Exists(_assetDir))
            {
                if (LoggerReference == null)
                {
                    log.Debug("No asset directory found. Cannot continue video transformation.");
                }
                else
                {
                    LoggerReference.LogEvent("No asset directory found. Cannot continue video transformation.");
                }
                return;
            }
            if (!Directory.Exists(_archiveDir))
            {
                if (LoggerReference == null)
                {
                    log.Debug("No archive directory found. Cannot continue video transformation.");
                }
                else
                {
                    LoggerReference.LogEvent("No archive directory found. Cannot continue video transformation.");
                }
                return;
            }
            if (!Directory.Exists(_galleryDir))
            {
                if (LoggerReference == null)
                {
                    log.Debug(noGallery);
                }
                else
                {
                    LoggerReference.LogEvent(noGallery);
                }
                return;
            }

            string fichierOriginal = Parameter.WorkFile.FullName;

            var fichierFinal = fichierOriginal + ".flv";

            var conf = new NomadConfiguration {FfmpegLocation = Parameter.Settings.LocationFfmpeg};

            float dureeFilm = -1;

            TransformationAvi ta = new TransformationAvi();
            try
            {
                dureeFilm = ta.transformation_VeloVolant2014_stutter(
                    conf,
                    _assetDir + @"intro.avi", // Intro
                    _assetDir + @"outro.avi", // Outro
                    _assetDir + @"sound.wav", // Son Repondant
                    fichierOriginal, // Video Repondant
                    _assetDir + @"overlay.png", // Overlay
                    _assetDir + @"velovolant_end.jpg", // Logo final
                    fichierFinal, // fichier final
                    _saveFrame,
                    _assetDir + @"frames\",
                    _assetDir + @"config_compress.bin",
                    _log, 1, 1, 30, -1,
                    _galleryDir,
                    _thumbnailTime);
            }
            catch (Exception e)
            {
                if (LoggerReference == null)
                {
                    log.Debug(e, "Error during transformation.");
                }
                else
                {
                    LoggerReference.LogEvent("Error during transformation.", e);
                }
            }

            if (dureeFilm > 0.0f)
            {
                var newFile = _archiveDir + StringUtil.GetFileName(fichierOriginal);
                // On déplace ensuite les fichiers.
                if (File.Exists(newFile))
                {
                    File.Delete(newFile);
                }
                File.Move(fichierOriginal, newFile);

                newFile = _galleryDir + StringUtil.GetFileName(fichierFinal);
                if (File.Exists(newFile))
                {
                    File.Delete(newFile);
                }
                File.Move(fichierFinal, newFile);
            }
        }

        private void GetTransformationProperties()
        {
            var path = "config.txt";
            if (File.Exists(path))
            {
                var data = new Dictionary<string, string>();
                foreach (var row in File.ReadAllLines(path))
                {
                    var tab = row.Split('=');
                    var rest = string.Concat(tab.Skip(1).ToArray());
                    data.Add(tab[0], rest);
                }

                var temp = data["thumbnail_time"];
                if (temp != null)
                {
                    _thumbnailTime = int.Parse(temp);
                }

                temp = data["gallery_dir"];
                if (temp != null)
                {
                    _galleryDir = temp;
                }

                temp = data["save_dir"];
                if (temp != null)
                {
                    SaveDir = temp;
                }

                temp = data["save_frame"];
                if (temp != null)
                {
                    _saveFrame = bool.Parse(temp);
                }

                temp = data["log_all"];
                if (temp != null)
                {
                    _log = bool.Parse(temp);
                }

                temp = data["archive_dir"];
                if (temp != null && Directory.Exists(temp))
                {
                    _archiveDir = temp;
                }

                temp = data["asset_dir"];
                if (temp != null && Directory.Exists(temp))
                {
                    _assetDir = temp;
                }
            }
            else
            {
                if (LoggerReference == null)
                {
                    log.Info("Could not find config file for transformation VéloVolant 2014.");
                }
                else
                {
                    LoggerReference.LogEvent("Could not find config file for transformation VéloVolant 2014.");
                }
            }
        }
    }
}
