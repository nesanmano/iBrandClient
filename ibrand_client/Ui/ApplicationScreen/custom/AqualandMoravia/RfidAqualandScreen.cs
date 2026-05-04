using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using JetBrains.Annotations;
using NLog;
using NomadLogic.Common.Services;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Win32;
using NomadLogic.iBrandDesktop.Services;
using NomadLogic.iBrandDesktop.Services.RespondentEventDataDownload;
using NomadLogic.iBrandDesktop.Services.RespondentEventDataDownload.DownloadWorkers;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.custom.AqualandMoravia
{
    /// <summary>
    /// Cette classe fait une vérification supplémentaire pour obtenir un ID unique du système de paiement distant avec Aqualand.
    /// Ce classe est liée àu service de téléchargement des événement multimédia répondants.
    /// </summary>
    [PublicAPI]
    public class RfidAqualandScreen : RfidScannerScreen, IProgress<RespondentBundleDownloadReport>
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private RespondentEventDataDownloadService _serviceDownload;
        private string _baseBundleDir;

        public RfidAqualandScreen(string backgroundImgEnSrc, string backgroundImgFrSrc, ScreenConfig sc, Rectangle? textboxLocation = null)
            : base(backgroundImgEnSrc, backgroundImgFrSrc, sc, textboxLocation) { }

        /// <summary>
        /// Force la vérification d'un utilisateur pour poursuivre.
        /// Vrai pas défaut, mais on peut mettre à faux pour débogage. Si mis à faux, il n'y a pas d'appels distants
        /// et un utlise un format unique de EUI basé sur le code lu.
        /// </summary>
        public bool RequireCorrectCustomerId { get; set; } = true;

        public DownloadWorkerAqualand Worker { get; set; }

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
                throw new System.Exception("No worker has been configured for the Aqualand scan screen.");
            }

            _baseBundleDir = GetTerminalDirectory(DirectoryUtil.ProjectDataDirectoriesEnum.pictures, DirectoryUtil.ProjectDataDirectoriesModEnum.bundle);

            _serviceDownload = ServicesContainer.GetService<RespondentEventDataDownloadService>();
            if (_serviceDownload != null) return;
            
            // Initialiser le service au besoin
            var tmp = new RespondentEventDataDownloadService()
            {
                MaxNumberOfRetries = 3
            };
            if (!await ServicesContainer.AddService(tmp))
            {
                throw new System.Exception("Could not initialize critical service: RespondentEventDataDownloadService");
            }

            _serviceDownload = ServicesContainer.GetService<RespondentEventDataDownloadService>();
            if (!await _serviceDownload.StartLongRunning())
            {
                throw new System.Exception("Could not start critical service: RespondentEventDataDownloadService");
            }
            _serviceDownload.AddObserver(this);
        }

        protected override async Task<bool> OnValidateAction(string sequenceRead)
        {
            string respondentExternalUniqueId;

            // TODO Code custom pour ce type de requête. Trouver le véritable UEI associé au répondant.
            if (!RequireCorrectCustomerId)
            {
                // On assigne un UEI basé sur la date et heure ainsi que le code lu.
                var today = DateTime.Today.ToString("yyyyMMdd");
                respondentExternalUniqueId = $"nomad_{sequenceRead}_{today}";
            }
            else
            {
                // TODO
                throw new NotImplementedException();
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