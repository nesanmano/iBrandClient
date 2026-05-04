using System;
using System.IO;
using System.Threading;
using NLog;
using NomadLogic.Common.Services;
using NomadLogic.Common.Util;
using NomadLogic.iBrandDesktop.Services;
using NomadLogic.iBrandDesktop.Util.SubmitAction.BaseClasses;

// ReSharper disable RedundantEmptyFinallyBlock

namespace NomadLogic.iBrandDesktop.Util.SubmitAction
{
    /// <summary>
    /// Classe de SubmitAction en deux temps. Il y a deux bassins de transformations, chacun pour une étape précise avec des besoins
    /// différents. À savoir, il ne faut faire qu'une seule transformation vidéo à la fois, mais il est permis d'envoyer plusieurs vidéos
    /// vers VIMEO simultanément. Donc, il y a deux workers différents, chacun sur un pool différent. Lorsque le premier worker a terminé,
    /// il créé le deuxième worker et l'envoi sur le deuxième pool. Le deuxième worker finalise le répondant.
    /// Dans le cas où l'on transforme les répondants de façon asynchrone (la méthode TransformNotReadyToSyncRespondents),
    /// il est possible d'avoir accès à une référence sur le deuxième worker une fois que le premier à terminé via une propriété.
    /// </summary>
    class KiaNba2015SubmitAction : AbstractSubmitAction
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        private const bool START_VLC = true;

        private string _saveDir = "";
        private string _vimeoConsumerKey;
        private string _vimeoConsumerSecret;
        private string _vimeoToken;
        private string _vimeoTokenSecret;

        protected override void InstanciateSubmitActionPools()
        {
            AddSubmitActionWorkerPool("transformation", 1);
            AddSubmitActionWorkerPool("upload", 20);
        }

        /// <summary>
        /// Insère le répondant à la fin du traitement.
        /// </summary>
        /// <param name="respondent"></param>
        /// <returns></returns>
        public override bool SubmitRespondent(Respondent respondent)
        {
            try
            {
                var db = ServicesContainer.GetService<IbrandDatabase>();
                var respId = db.InsertRespondent(respondent, false);

                var ok = ProcessRespondent(respondent, respId);

                return ok;
            }
            catch (System.Exception ex)
            {
                log.Fatal(ex.Message);
                return false;
            }
        }

        protected override ISubmitActionWorker CreateWorker(Respondent resp, bool usePool = true)
        {
            throw new NotImplementedException();
        }

        public override bool ReSubmitRespondent(int respondentId)
        {
            var db = ServicesContainer.GetService<IbrandDatabase>();
            var respondent = db.GetRespondent(respondentId);

            respondent.Answers = db.GetAnswers(respondentId);

            var ok = ProcessRespondent(respondent, respondentId);
            return ok;
        }

        /// <summary>
        /// Indique si VLC doit être démarré lors de la fin du traitement.
        /// </summary>
        /// <returns></returns>
        public bool HasToStartVlc()
        {
            return START_VLC;
        }

        /// <summary>
        /// Exécute le traitement du vidéo.
        /// </summary>
        /// <param name="resp">Le répondant à traiter.</param>
        /// <param name="respId">L'identifiant de ce répondant.</param>
        private bool ProcessRespondent(Respondent resp, int respId)
        {
            var videoName = resp.GetAnswerText(5);
            
            var fileName = StringUtil.GetFileName(videoName);
            var videoOriginal = //configuration.VideosCanonDir + 
                fileName;

            if (!File.Exists(videoOriginal))
            {
                throw new FileNotFoundException("Video file not found: " + videoOriginal);
            }

            var temp = new KiaNba2015SubmitActionWorker(this, resp, respId, _saveDir,
                _vimeoConsumerKey, _vimeoConsumerSecret, _vimeoToken, _vimeoTokenSecret, _submitActionWorkerPools[1]);

            return _submitActionWorkerPools[0].AddWorker(temp);
        }

        protected override void ValidateConfigData()
        {
            var temp = _configData["save_dir"];
            if (temp != null)
            {
                _saveDir = temp;
            }

            _vimeoConsumerKey = _configData["consumer_key"];
            _vimeoConsumerSecret = _configData["consumer_secret"];
            _vimeoToken = _configData["token"];
            _vimeoTokenSecret = _configData["token_secret"];

            if (_vimeoConsumerKey == null || _vimeoConsumerSecret == null || _vimeoToken == null || _vimeoTokenSecret == null)
            {
                throw new System.Exception("VIMEO information is lacking from config.txt file in projet directory.");
            }
        }

        public override void TransformNotReadyToSyncRespondents(CancellationToken? ct)
        {
            var db = ServicesContainer.GetService<IbrandDatabase>();
            var respondentList = db.GetRespondentsBySyncStatus(Convert.ToString(ProjectId));

            UiSynchro?.SetProgressBarMinMax(0, respondentList.Count);

            foreach (var respId in respondentList)
            {
                if (ct != null && ct.Value.IsCancellationRequested) break;

                var respondent = db.GetRespondent(respId);
                respondent.Answers = db.GetAnswers(respId);
                
                var temp = new KiaNba2015SubmitActionWorker(this, respondent, respId, _saveDir,
                    _vimeoConsumerKey, _vimeoConsumerSecret, _vimeoToken, _vimeoTokenSecret, null);
                temp.DoWork(ct);

                // On lance la deuxième étape ici. NextWorkerStep aura alors été créé à l'interne lors du DoWork.
                if (ct != null && ct.Value.IsCancellationRequested) break;
                temp.NextWorkerStep?.DoWork(ct);

                UiSynchro?.AddProgressStep(1);
            }
        }
    }
}
