using System;
using System.IO;
using System.Windows.Forms;
using iBrand_SnapShot.UI;
using NomadLogic.iBrand.Common.Util;

namespace iBrand_SnapShot
{
    /// <summary>
    /// Classe de base pour conserver les configuration du programme SnapShot, en utilisant un format JSON.
    /// </summary>
    internal class SnapShotSettings
    {
        /// <summary>
        /// Indique que l'on n'utilise pas le FTP. Donc inutile de lancer son fil d'exécution ou de tester ce dernier.
        /// </summary>
        public bool UseFtp { get; set; }

        /// <summary>
        /// L'adresse du serveur Ftp à utiliser.
        /// </summary>
        public string FtpServer { get; set; }

        /// <summary>
        /// Le nom d'usager de l'utilisateur.
        /// </summary>
        public string FtpUser { get; set; }

        /// <summary>
        /// Le mot de passe de l'usager.
        /// </summary>
        public string FtpPassword { get; set; }

        /// <summary>
        /// L'adresse du serveur de secour Ftp à utiliser.
        /// </summary>
        public string BackupFtpServer { get; set; }

        /// <summary>
        /// Le nom d'usager de secour de l'utilisateur.
        /// </summary>
        public string BackupFtpUser { get; set; }

        /// <summary>
        /// Le mot de passe de l'usager de secour.
        /// </summary>
        public string BackupFtpPassword { get; set; }

        /// <summary>
        /// Répertoire où mettre le fichier dans le serveur Ftp
        /// </summary>
        public string FtpDirectory { get; set; }

        /// <summary>
        /// Endroit où mettre le fichier une fois le téléchargement terminé.
        /// </summary>
        [System.ComponentModel.DefaultValue("./archives")]
        public string FtpArchive { get; set; }

        /// <summary>
        /// Indique si le bassin Ftp journalise à fond son comportement.
        /// </summary>
        [System.ComponentModel.DefaultValue(false)]
        public bool PoolVerboseLog { get; set; }

        /// <summary>
        /// Indique si la caméra prend des images ou des vidéos (valeur fausse pour vidéo).
        /// </summary>
        [System.ComponentModel.DefaultValue(true)]
        public bool IsCameraPicture { get; set; }

        /// <summary>
        /// Indique la durée d'un vidéo, en secondes.
        /// </summary>
        [System.ComponentModel.DefaultValue(5)]
        public int VideoDuration { get; set; }

        public bool ModifyVideoDuration(string newValue, Control ctrl, MainFormUi reference)
        {
            var error = false;

            try
            {
                var result = Convert.ToInt32(newValue);
                VideoDuration = result;
            }
            catch (Exception)
            {
                error = true;
            }

            if (error)
            {
                reference.SetStatus(ctrl, SnapShotController.NOT_READY, null, true);
                reference.LogEvent("Video duration is not valid. Must be an integer.");
            }
            else
            {
                reference.SetStatus(ctrl, SnapShotController.ORIGINAL, null, true);
            }

            return error;
        }

        /// <summary>
        /// Indique le moment, en secondes, où est prit le thumbnail du vidéo.
        /// </summary>
        [System.ComponentModel.DefaultValue(3)]
        public int VideoThumbnail { get; set; }

        public bool ModifyVideoThumbnail(string newValue, Control ctrl, MainFormUi reference)
        {
            var error = false;

            try
            {
                var result = Convert.ToInt32(newValue);
                VideoThumbnail = result;
            }
            catch (Exception)
            {
                error = true;
            }

            if (error)
            {
                reference.SetStatus(ctrl, SnapShotController.NOT_READY, null, true);
                reference.LogEvent("Video Thumbnail is not valid. Must be an integer.");
            }
            else
            {
                reference.SetStatus(ctrl, SnapShotController.ORIGINAL, null, true);
            }

            return error;
        }

        /// <summary>
        /// Indique la taille maximale, en kilo-octets, du fichier vidéo quand il passe de MOV à AVI.
        /// Ce qui veut dire que le fichier sera compressé pour ne pas dépasser cette taille. Selon la taille
        /// originale, ça peut être beaucoup ou peu.
        /// <br/>Si zéro, aucune compression ne sera faite et XVID évaluera la taille selon ses paramètres optimaux.
        /// </summary>
        [System.ComponentModel.DefaultValue(0)]
        public uint VideoFileSize { get; set; }

        public bool ModifyVideoFileSize(string newValue, Control ctrl, MainFormUi reference)
        {
            var error = false;

            try
            {
                var result = Convert.ToUInt32(newValue);
                VideoFileSize = result;
            }
            catch (Exception)
            {
                error = true;
            }

            if (error)
            {
                reference.SetStatus(ctrl, SnapShotController.NOT_READY, null, true);
                reference.LogEvent("Video file size is not valid. Must be an integer.");
            }
            else
            {
                reference.SetStatus(ctrl, SnapShotController.ORIGINAL, null, true);
            }

            return error;
        }

        /// <summary>
        /// Le nom de la classe à utiliser comme déclencheur.
        /// </summary>
        public string TriggerClassName { get; set; }

        /// <summary>
        /// Paramètres de la classes de déclencheur. Ceci est leur configuration en somme.
        /// </summary>
        public string[] TriggerParameters { get; set; }

        public bool ModifyTriggerParameters(string newValue, Control ctrl, MainFormUi reference)
        {
            var error = false;

            try
            {
                string[] result = StringUtil.SplitString(newValue, ",");
                TriggerParameters = result;
            }
            catch (Exception)
            {
                error = true;
            }

            if (error)
            {
                reference.SetStatus(ctrl, SnapShotController.NOT_READY, null, true);
                reference.LogEvent("Trigger parameters not valid. Must be an list separated by commas..");
            }
            else
            {
                reference.SetStatus(ctrl, SnapShotController.ORIGINAL, null, true);
            }

            return error;
        }

        /// <summary>
        /// Location de l'exécutable FFMPEG sur l'ordinateur local.
        /// </summary>
        [System.ComponentModel.DefaultValue("C:/Program Files (x86)/ffmpeg/bin/ffmpeg.exe")]
        public string LocationFfmpeg { get; set; }

        public bool ModifyLocationFfmpeg(string newValue, Control ctrl, MainFormUi reference)
        {
            var error = false;

            try
            {
                if (!File.Exists(newValue) || !newValue.EndsWith("ffmpeg.exe"))
                {
                    error = true;
                }
                else
                {
                    LocationFfmpeg = newValue;
                }
            }
            catch (Exception)
            {
                error = true;
            }

            if (error)
            {
                reference.SetStatus(ctrl, SnapShotController.NOT_READY, null, true);
                reference.LogEvent("FFMPEG executable not found. Must be 'ffmpeg.exe'");
            }
            else
            {
                reference.SetStatus(ctrl, SnapShotController.ORIGINAL, null, true);
            }

            return error;
        }

        /// <summary>
        /// Classe de travail personnalisé au projet pour le traitement du fichier multimédia.
        /// <br/>Est activée à chaque prise de fichier, ou peut être activée manuellement au besoin.
        /// </summary>
        public string CustomWorker { get; set; }

        /// <summary>
        /// Indique le temps authorisé pour le déclenchement des Trigger.
        /// <br/>Si vide, aucune restriction. Sinon, on utilise le format "H:m-H:m".
        /// </summary>
        public string TimeTriggerWindow { get; set; }

        /// <summary>
        /// Nombre de travailleurs simultanés pour le FTP.
        /// </summary>
        public static readonly int DEFAULT_CONCURRENT_WORKERS_FTP = 5;
        public int ConcurrentWorkersFtp { get; set; }

        public bool ModifyConcurrentWorkersFtp(string newValue, Control ctrl, MainFormUi reference)
        {
            var error = false;

            try
            {
                var result = Convert.ToInt32(newValue);
                if (result < 1)
                {
                    error = true;
                }
                else
                {
                    ConcurrentWorkersFtp = result;
                }
            }
            catch (Exception)
            {
                error = true;
            }

            if (error)
            {
                reference.SetStatus(ctrl, SnapShotController.NOT_READY, null, true);
                reference.LogEvent("Number of concurrent workers for FTP not valid. Must be an integer.");
            }
            else
            {
                reference.SetStatus(ctrl, SnapShotController.ORIGINAL, null, true);
            }

            return error;
        }

        /// <summary>
        /// Nombre de travailleurs simultanés pour les travailleurs personalisés.
        /// </summary>
        public static readonly int DefaultConcurrentWorkersCustom = 1;
        public int ConcurrentWorkersCustom { get; set; }

        public bool ModifyConcurrentWorkersCustom(string newValue, Control ctrl, MainFormUi reference)
        {
            var error = false;

            try
            {
                var result = Convert.ToInt32(newValue);
                if (result < 1)
                {
                    error = true;
                }
                else
                {
                    ConcurrentWorkersCustom = result;
                }
            }
            catch (Exception)
            {
                error = true;
            }

            if (error)
            {
                reference.SetStatus(ctrl, SnapShotController.NOT_READY, null, true);
                reference.LogEvent("Number of concurrent workers for Custom Class not valid. Must be an integer.");
            }
            else
            {
                reference.SetStatus(ctrl, SnapShotController.ORIGINAL, null, true);
            }

            return error;
        }
    }
}
