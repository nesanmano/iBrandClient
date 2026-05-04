using System;
using System.Drawing;
using System.IO;
using System.Threading;
using NLog;
using NomadLogic.Common.Services;
using NomadLogic.Common.Services.Configuration;
using NomadLogic.Common.Util;
using NomadLogic.iBrandDesktop.Util.SubmitAction.BaseClasses;

namespace NomadLogic.iBrandDesktop.Util.SubmitAction
{
    /// <summary>
    /// Classe de travail sur fil d'exécution pour créer le vidéo.
    /// </summary>
    class KiaNba2015SubmitActionWorker : ISubmitActionWorker
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private const int QUESTION_VIDEO = 5;
        private const int QUESTION_FIRST_NAME = 1;
        private const int QUESTION_LAST_NAME = 2;
        private const string FICHIER_INTRO_IMAGE = "video-in-out.jpg";
        private const string FICHIER_SONORE = "kia_soundtrack.wav";
        private const string FICHIER_OUTRO_IMAGE = "video-in-out.jpg";
        private const string FICHIER_COMPRESS = "config_compress.bin";
        private const string FICHIER_BKG_VIDEO = "bkg_final.avi";
        private const string FICHIER_OVERLAY = "overlay.png";

        private readonly KiaNba2015SubmitAction _action;
        private readonly Respondent _respondentT;
        private readonly int _respIdT;
        private readonly string _saveDir;
        private readonly string _key;
        private readonly string _secretKey;
        private readonly string _token;
        private readonly string _secretToken;

        private readonly string _pathProjectDir;
        private readonly SubmitActionWorkerPool _pool;

        /// <summary>
        /// Constructeur principal
        /// </summary>
        /// <param name="action">L'initiateur</param>
        /// <param name="resp">Le répondant à créer</param>
        /// <param name="respId">L'identifiant.</param>
        /// <param name="saveDir">Le répertoire à utiliser pour sauvegarder les trames.</param>
        /// <param name="projectDir">Répertoire du projet.</param>
        /// <param name="vimeoConsumerKey">La clé de l'usager viméo.</param>
        /// <param name="vimeoConsumerSecret">La clé secrète de l'usager viméo.</param>
        /// <param name="vimeoToken">Le jeton de téléchargement viméo.</param>
        /// <param name="vimeoTokenSecret">Le jeton secret de téléchargement viméo.</param>
        /// <param name="secondPool">Le deuxième bassin de connexion où envoyer la deuxième étape de la transformation (Vimeo).</param>
        public KiaNba2015SubmitActionWorker(KiaNba2015SubmitAction action, Respondent resp, int respId, string saveDir,
            string vimeoConsumerKey, string vimeoConsumerSecret, string vimeoToken, string vimeoTokenSecret,
            SubmitActionWorkerPool secondPool)
        {
            _action = action;
            _respondentT = resp;
            _respIdT = respId;
            _saveDir = saveDir;
            _pathProjectDir = DirectoryUtil.GetLocalIbrandTerminalDir(_action.TerminalInfo);
            _key = vimeoConsumerKey;
            _secretKey = vimeoConsumerSecret;
            _token = vimeoToken;
            _secretToken = vimeoTokenSecret;
            _pool = secondPool;
        }

        /// <summary>
        /// Obtient l'ID du répondant à traiter.
        /// </summary>
        /// <returns>L'ID du répondant.</returns>
        public int GetRespId()
        {
            return _respIdT;
        }

        /// <summary>
        /// Retourne l'instance du submit action lié à cette instance.
        /// </summary>
        /// <returns>La référence de l'action Submit lié à ce travailleur.</returns>
        public ISubmitAction GetSubmitAction()
        {
            return _action;
        }

        /// <summary>
        /// Cette méthode contient le gros du travail à faire sur le fil d'exécution.
        /// </summary>
        public void DoWork(CancellationToken? ct)
        {
            if (ct?.IsCancellationRequested ?? false) return;

            try
            {
                var videoName = _respondentT.GetAnswerText(QUESTION_VIDEO);

                var configuration = ServicesContainer.GetService<NomadConfiguration>();

                var fileName = StringUtil.GetFileName(videoName);

                var videoMovedName = //configuration.VideosCanonDir + 
                    fileName;

                if (!File.Exists(videoMovedName))
                {
                    throw new FileNotFoundException("Video file not found: " + videoMovedName);
                }
                
                var id = fileName.Substring(0, fileName.LastIndexOf(".", StringComparison.Ordinal));

                var videoFinalName = //configuration.VideosCanonDir + 
                    id + "_final.flv";

                _action.UiSynchro?.AddUiMessage("Transforming video " + fileName + " ...", 0);

                var intro = _pathProjectDir + FICHIER_INTRO_IMAGE;
                if (!File.Exists(intro)) throw new System.Exception("File not found: " + intro);
                var sound = _pathProjectDir + FICHIER_SONORE;
                if (!File.Exists(sound)) throw new System.Exception("File not found: " + sound);
                var outro = _pathProjectDir + FICHIER_OUTRO_IMAGE;
                if (!File.Exists(outro)) throw new System.Exception("File not found: " + outro);
                var compressOptions = _pathProjectDir + FICHIER_COMPRESS; // Peut ne pas exister
                var bkgVideo = _pathProjectDir + FICHIER_BKG_VIDEO;
                if (!File.Exists(bkgVideo)) throw new System.Exception("File not found: " + bkgVideo);
                var overlay = _pathProjectDir + FICHIER_OVERLAY;
                if (!File.Exists(overlay)) throw new System.Exception("File not found: " + overlay);

                if (!File.Exists(videoFinalName))
                {
                    // Transformation du overlay pour mettre le nom du répondant
                    var fName = _respondentT.GetAnswerText(QUESTION_FIRST_NAME, 1);
                    var lName = _respondentT.GetAnswerText(QUESTION_LAST_NAME, 1);

                    var overlayBitmap = new Bitmap(overlay);

                    /*overlayBitmap.AddTextToBitmap((fName + " " + lName).ToUpper().Trim(), 670, 576, true,
                        Color.White, "Arial", 26.0f, FontStyle.Bold);

                    var transformation = new TransformationAvi();

                    var timeVideo = transformation.transformation_Kia2015(
                        configuration,
                        true,
                        false,
                        _saveDir,
                        compressOptions,
                        intro,
                        outro,
                        sound,
                        videoMovedName,
                        12,
                        bkgVideo,
                        overlayBitmap,
                        videoFinalName,
                        null /*repTest + @"bkg_frames\"#1#,
                        null /*"frame_*.bmp"#1#,
                        false);
                    

                    if (timeVideo < 0)
                    {
                        throw new System.Exception("Incorrect video time. Error during transformation.");
                    }*/
                    
                }

                // Envoi vers la télé (VLC)
                //var vlc = new Vlc(videoFinalName, configuration.VlcLocation);
                //vlc.AddToPlayList();

                // Si le bassin est nul, alors on fait des transformations synchrones, et on aura besoin d'avoir accès au
                // deuxième travailleur de l'extérieur.
                NextWorkerStep = new KiaNba2015SubmitActionWorkerStep2(_action, _respondentT, _respIdT, videoFinalName,
                    _key, _secretKey, _token, _secretToken);

                // On chaîne maintenant la deuxième partie de la transformation dans le deuxième bassin.
                _pool?.AddWorker(NextWorkerStep);
            }
            catch (System.Exception ex)
            {
                log.Fatal("Error while KiaNBA2015SubmitAction_Worker#DoWork():\r\n" + ex.Message + "\r\n" + ex.StackTrace);

                _action.UiSynchro?.AddUiMessage("Error while KiaNBA2015SubmitAction_Worker#DoWork()", 1);
            }
        }

        public ISubmitActionWorker NextWorkerStep { get; set; }
        
    }
}