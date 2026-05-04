using System.IO;
using NLog;
using NomadLogic.Common;

namespace NomadLogic.iBrandDesktop.Util.Social.Vimeo
{
    public class UploadVideoToVimeo
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private string _consumerKey;
        private string _consumerSecret;
        private string _token;
        private string _tokenSecret;

        /// <summary>
        /// Délégué pour une fois que le traitement d'envoi est un succès.
        /// </summary>
        public event OnUploadFinishedDelegate OnUploadFinished;
        public event ShowMessageBoxDelegate OnShowMessageBox;

        private readonly int _uploadId;
        private readonly string _fileToUpload;
        private readonly bool _deleteAfterUpload;

        /// <summary>
        /// Créé un objet d'envoi Viméo. Il faut par la suite invoquer Init() avec DoWork().
        /// </summary>
        /// <param name="fileToUpload">Le chemin complet du fichier à envoyer.</param>
        /// <param name="id">L'identifiant de l'envoi.</param>
        /// <param name="deleteAfterUpload">Indique s'il faut détruire le fichier après un téléversement complété avec succès.</param>
        public UploadVideoToVimeo(string fileToUpload, int id, bool deleteAfterUpload)
        {
            _fileToUpload = fileToUpload;
            _uploadId = id;
            _deleteAfterUpload = deleteAfterUpload;
            Success = false;
            VideoId = null;
        }

        /// <summary>
        /// Initialise l'envoi du vidéo avec des informations de compte pour l'envoi du vidéo.
        /// </summary>
        /// <param name="consumerKey">Clé d'identification de l'usager.</param>
        /// <param name="consumerSecret">Clé secrète de l'usager.</param>
        /// <param name="token">Jeton d'envoi.</param>
        /// <param name="tokenSecret">Jeton secret d'envoi.</param>
        public void Init(string consumerKey, string consumerSecret, string token, string tokenSecret)
        {
            _consumerKey = consumerKey;
            _consumerSecret = consumerSecret;
            _token = token;
            _tokenSecret = tokenSecret;
        }

        /// <summary>
        /// Indique si l'envoi a été couronné de succès.
        /// </summary>
        public bool Success { get; private set; }

        /// <summary>
        /// Si l'envoi a été un succès, cette propriété contiendra un identifiant  non-nul venant de Viméo.
        /// </summary>
        public string VideoId { get; private set; }

        /// <summary>
        /// Fait le travail d'envoi. Le résultat sera stocké dans la propriété Success.
        /// </summary>
        public void DoWork()
        {
            try
            {
                if (File.Exists(_fileToUpload))
                {
                    var vc = new VimeoClient(_consumerKey, _consumerSecret)
                    {
                        _token = _token,
                        _token_secret = _tokenSecret
                    };

                    if (vc.Login())
                    {
                        if (vc.RefreshQuota()) 
                        { 
                            var t = vc.vimeo_videos_upload_getTicket(null, null);
                            vc.PostVideo(t, _fileToUpload);
                            VideoId = vc.vimeo_videos_upload_complete(_fileToUpload, t._id);

                            if (VideoId != null)
                            {
                                Success = true;

                                OnShowMessageBox?.Invoke("File uploaded to Vimeo.", false);

                                if (_deleteAfterUpload)
                                    File.Delete(_fileToUpload);
                            }
                        }
                    }
                    else
                    {
                        log.Fatal("Can't login to vimeo");

                    }
                }
                else
                {
                    log.Warn("File does not exists: " + _fileToUpload);
                }
            }
            catch (System.Exception ex)
            {
                OnShowMessageBox?.Invoke("Error Uploading file to Vimeo.", true);

                log.Fatal(ex.Message);
                log.Fatal(ex.StackTrace);
            }

            OnUploadFinished?.Invoke(Success, _uploadId, VideoId);
        }
    }
}
