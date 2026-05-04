using System.Threading;
using NLog;
using NomadLogic.Common;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Services;
using NomadLogic.iBrandDesktop.Services;
using NomadLogic.iBrandDesktop.Util.Social.Vimeo;
using NomadLogic.iBrandDesktop.Util.SubmitAction.BaseClasses;

namespace NomadLogic.iBrandDesktop.Util.SubmitAction
{
    /// <summary>
    /// Classe de travail sur fil d'exécution pour envoyer sur VIMEO.
    /// </summary>
    class KiaNba2015SubmitActionWorkerStep2 : ISubmitActionWorker
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private static readonly QuestionIdentifier question_Vimeo_Id = new QuestionIdentifier(6,1,QuestionTypeEnum.Vimeo);

        private readonly KiaNba2015SubmitAction _action;
        private readonly Respondent _respondentT;
        private readonly int _respIdT;
        private readonly string _key;
        private readonly string _secretKey;
        private readonly string _token;
        private readonly string _secretToken;

        private readonly string _fileName;

        /// <summary>
        /// Constructeur principal
        /// </summary>
        /// <param name="action">L'initiateur</param>
        /// <param name="resp">Le répondant à créer</param>
        /// <param name="respId">L'identifiant.</param>
        /// <param name="fileName"></param>
        /// <param name="vimeoConsumerKey">La clé de l'usager viméo.</param>
        /// <param name="vimeoConsumerSecret">La clé secrète de l'usager viméo.</param>
        /// <param name="vimeoToken">Le jeton de téléchargement viméo.</param>
        /// <param name="vimeoTokenSecret">Le jeton secret de téléchargement viméo.</param>
        public KiaNba2015SubmitActionWorkerStep2(KiaNba2015SubmitAction action, Respondent resp, int respId, string fileName,
            string vimeoConsumerKey, string vimeoConsumerSecret, string vimeoToken, string vimeoTokenSecret)
        {
            _action = action;
            _respondentT = resp;
            _respIdT = respId;
            _key = vimeoConsumerKey;
            _secretKey = vimeoConsumerSecret;
            _token = vimeoToken;
            _secretToken = vimeoTokenSecret;
            _fileName = fileName;
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
                var db = ServicesContainer.GetService<IbrandDatabase>();
                if (!db.GetAnswersMap(_respIdT).ContainsKey(question_Vimeo_Id))
                {

                    // On envoie vers VIMEO maintenant.
                    var uploadVideo = new UploadVideoToVimeo(_fileName, _respondentT.Id, false);
                    uploadVideo.Init(_key, _secretKey, _token, _secretToken);
                    uploadVideo.DoWork();

                    var vimeoId = uploadVideo.VideoId;
                    if (uploadVideo.Success && vimeoId != null)
                    {
                        // Enregistrement de la question Viméo
                        var vimeoA = new Answer{QuestionIdentifier = question_Vimeo_Id, Response = vimeoId};
                        db.DeleteAnswer(_respIdT, vimeoA);
                        db.InsertAnswer(_respIdT, vimeoA);

                        // On finalise le répondant.
                        _action.UiSynchro?.AddUiMessage("Video transformed, thumbnail created, VIMEO sent.", 0);

                        db.SetRespondentReadyToSync(_respIdT);

                        _action.NewRespondentEventHandle?.Set();
                    }
                    else
                    {
                        log.Error("Error while VIMEO sending. Was not successful.");
                        _action.UiSynchro?.AddUiMessage("Error while VIMEO sending. Was not successful.", 1);
                    }
                }
                else
                {
                    // S'il y a déjà une réponse, on procède à valider le répondant.
                    _action.UiSynchro?.AddUiMessage("Video already sent. Validating respondant", 0);

                    db.SetRespondentReadyToSync(_respIdT);

                    _action.NewRespondentEventHandle?.Set();
                }
            }
            catch (System.Exception ex)
            {
                log.Fatal("Error while KiaNBA2015SubmitAction_WorkerStep2#DoWork():\r\n" + ex.Message + "\r\n" + ex.StackTrace);

                _action.UiSynchro?.AddUiMessage("Error while KiaNBA2015SubmitAction_WorkerStep2#DoWork()", 1);
            }
        }

        public ISubmitActionWorker NextWorkerStep { get; set; }
    }
}