using System;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using JetBrains.Annotations;
using NLog;
using NomadLogic.Common.Questionnaire;
using NomadLogic.Common.Services;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Win32;
using NomadLogic.iBrandDesktop.Services.RespondentEventDataDownload;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic
{
    /// <summary>
    /// Écran qui fait une requête vers le serveur pour obtenir des éléments de gallerie, basée sur la lecture d'un identifiant (RFID ou QR).
    /// Ce classe est liée àu service de téléchargement des événement multimédia répondants.
    /// </summary>
    [PublicAPI]
    public class GalleryScanRequestScreen : RfidScannerScreen, IProgress<RespondentBundleDownloadReport>
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private RespondentEventDataDownloadService _serviceDownload;
        private string _baseBundleDir;
        
        public GalleryScanRequestScreen(string backgroundImgEnSrc, string backgroundImgFrSrc, ScreenConfig sc, Rectangle? textboxLocation = null)
            : base(backgroundImgEnSrc, backgroundImgFrSrc, sc, textboxLocation) { }
        
        /// <summary>
        /// Force la vérification d'un identifiant auprès d'un serveur distant.
        /// Sinon, utilise la valeur comme telle.
        ///  TODO Implémenter une classe pour faire cette requête.
        /// </summary>
        public bool RequireCorrectCustomerId { get; set; }

        /// <summary>
        /// L'instance du travailleur spécifique pour la requête de téléchargement.
        /// </summary>
        public IRespondentEventDataDownloadWorker Worker { get; set; }

        /// <summary>
        /// Indique que l'on veut sauvegarder la valeur du Scan lu dans le questionnaire actuel.
        /// </summary>
        public DataModelQuestionnaire SaveValueDataModel { get; set; }

        public override void Dispose()
        {
            base.Dispose();
            
            _serviceDownload?.RemoveObserver(this);
        }

        protected override async void CustomLoad()
        {
            base.CustomLoad();

            if (Worker == null)
            {
                throw new System.Exception("No worker has been configured for the gallery scan request screen.");
            }

            _baseBundleDir = DirectoryUtil.GetLocalIbrandTerminalDataDir(ProjectId, QuestionnaireId, TerminalId,
                DirectoryUtil.ProjectDataDirectoriesEnum.pictures, DirectoryUtil.ProjectDataDirectoriesModEnum.bundle);

            _serviceDownload = ServicesContainer.GetService<RespondentEventDataDownloadService>();
            if (_serviceDownload != null) return;
            
            // Initialiser le service au besoin
            var tmp = new RespondentEventDataDownloadService
            {
                MaxNumberOfRetries = 3
            };
            if (!await ServicesContainer.AddService(tmp))
            {
                throw new System.Exception("Could not initialize critical service: RespondentEventDataDownloadService");
            }

            _serviceDownload = ServicesContainer.GetService<RespondentEventDataDownloadService>();
            
            if (!_serviceDownload.IsStartedAndRunning())
            {
                throw new System.Exception("Critical service not operational: RespondentEventDataDownloadService");
            }
            
            _serviceDownload.AddObserver(this);
        }

        protected override async Task<bool> OnValidateAction(string sequenceRead)
        {
            string respondentExternalUniqueId;

            // TODO Code custom pour ce type de requête. Trouver le véritable UEI associé au répondant.
            if (!RequireCorrectCustomerId)
            {
                // TODO voir si on peut ajouter des modifications à la lecture.
                respondentExternalUniqueId = sequenceRead;
            }
            else
            {
                // TODO
                throw new NotImplementedException();
            }

            // Sauvegarde ou écrase la valeur lue dans la BD.
            if (SaveValueDataModel != null)
            {
                var saveResult = SaveValueDataModel.SaveData(GetCurrentQuestionnaire(), respondentExternalUniqueId);
                if (!saveResult)
                {
                    log.Error("Could not save data. Please check data model specifications: " + SaveValueDataModel.ToStringPublicProperties());
                }
            }

            // Lancer le processus pour le service de téléchargement.
            
            // Créer un répertoire unique pour ce répondant unique.
            var eventDir = new DirectoryInfo(Path.Combine(_baseBundleDir, respondentExternalUniqueId));
            
            var dr = new RespondentBundleRequest(eventDir, respondentExternalUniqueId, Worker, GetCurrentQuestionnaire(), 
                CallBackSuccess, CallBackCancelled, CallBackError);
            
            var result = _serviceDownload.SetRespondentToDownload(dr);
            if (result)
            {
                log.Trace("New respondent download request sent.");
                return true;
            }
            
            log.Warn("Respondent configuration was invalid.");
            
            this.SafeCall(() =>
            {
                MessageBox.Show(@"Invalid download request. Please review install condifguration.", @"iBrand", MessageBoxButtons.OK);
            });

            return false;
        }

        private void CallBackSuccess(RespondentBundleRequest rbr)
        {
            if (log.IsDebugEnabled)
            {
                log.Debug("Work finished on respondent EUI: {0}", rbr.GetRespondentUniqueId());
            }
        }

        private void CallBackCancelled(RespondentBundleRequest rbr)
        {
            if (log.IsWarnEnabled)
            {
                log.Debug("Cancellation for respondent EUI: {0}", rbr.GetRespondentUniqueId());
            }
        }

        private void CallBackError(RespondentBundleRequest rbr)
        {
            if (log.IsDebugEnabled)
            {
                log.Debug("Error during work on respondent EUI: {0}", rbr.GetRespondentUniqueId());
            }
        }

        public void Report(RespondentBundleDownloadReport value)
        {
            if (log.IsDebugEnabled)
            {
                log.Debug(value.ToStringQuickPreview());
            }
        }
    }
}