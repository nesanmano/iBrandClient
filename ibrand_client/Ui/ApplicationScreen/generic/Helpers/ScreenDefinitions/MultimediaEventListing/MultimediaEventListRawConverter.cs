using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using JetBrains.Annotations;
using NLog;
using NomadLogic.Common.Serveur.Structures;
using NomadLogic.iBrandDesktop.Services.RespondentEventDataDownload;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers.ScreenDefinitions.MultimediaEventListing
{
    public abstract class MultimediaEventListRawConverter
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        protected IReadOnlyDictionary<RequestRespondentDataDynamicValue, RespondentBundleDataGroup> _rawData;

        protected MultimediaEventItemPresentation _presentationData;
        protected DirectoryInfo _assetsDir;
        protected DirectoryInfo _workDirectory;
        protected UserControl _screenInst;
        private CancellationTokenSource _cts;
        private Task _activeTask;
        protected FileInfo _placeholderFile;

        /// <summary>
        /// Indique que la conversion est terminée. Se remet à faux si on assigne de nouvelles données brutes ou si on nettoie l'instance.
        /// </summary>
        public bool IsConversionFinished { get; private set; }

        public void SetPresentationData(MultimediaEventItemPresentation presentationData)
        {
            _presentationData = presentationData;
        }
        
        /// <summary>
        /// Vide les référence vers le dictionnaire de données.
        /// </summary>
        public void Clean()
        {
            _cts?.Cancel();
            _activeTask = null;
            _rawData = null;
            IsConversionFinished = false;
        }

        /// <summary>
        /// L'événement requis à associer avec les items quand sélectionnés.
        /// </summary>
        public EventHandler<object> SelectionChangedEventRef { get; set; }

        public void SetRawData(IReadOnlyDictionary<RequestRespondentDataDynamicValue, RespondentBundleDataGroup> rawData)
        {
            _rawData = rawData;
            IsConversionFinished = false;
        }

        // Délégué requis pour l'invocation correcte de cette méthode.
        protected delegate void StartConvertionDelegate(CancellationToken ct, [NotNull] Action<List<MultimediaEventGalleryRow>> populateAction);
        
        /// <summary>
        /// S'assure que l'exécution se fasse sur le fil UI.
        /// </summary>
        private void StartConversionInvokeCheck(CancellationToken ct, [NotNull] Action<List<MultimediaEventGalleryRow>> populateAction)
        {
            if (_screenInst.InvokeRequired)
            {
                var d = new StartConvertionDelegate(StartConversionInvokeCheck);
                _screenInst.Invoke(d, ct, populateAction);
                return;
            }

            List<MultimediaEventGalleryRow> rows = null;
            try
            {
                rows = StartConversionExec(ct);
            }
            catch (System.Exception e)
            {
                log.Error(e, "Error during conversion execution.");
            }

            log.Trace("Invoking populate action.");
            IsConversionFinished = true;
            populateAction.Invoke(rows);
        }

        /// <summary>
        /// Fait tout le travail de conversion, retourne la liste d'éléments basée sur les données brutes.
        /// </summary>
        /// <param name="ct">Jeton de cancellation.</param>
        protected abstract List<MultimediaEventGalleryRow> StartConversionExec(CancellationToken ct);

        /// <summary>
        /// Lance la conversion dans une tâche asynchrone.
        /// </summary>
        public void StartConversion([NotNull] Action<List<MultimediaEventGalleryRow>> populateAction)
        {
            if (_rawData == null)
            {
                log.Warn("No data to convert.");
                return;
            }

            if (_activeTask != null || IsConversionFinished)
            {
                log.Debug("Skipping new conversion attempt. Job has already started or finished.");
                return;
            }

            _cts = new CancellationTokenSource();

            _activeTask = Task.Run(() =>
            {
                // Démarre l'implémentation.
                StartConversionInvokeCheck(_cts.Token, populateAction);
            });
        }

        public void SetAssetsDirectory(DirectoryInfo assetsDir)
        {
            _assetsDir = assetsDir;
        }

        public void SetWorkDirectory(string workFileDirectory)
        {
            _workDirectory = new DirectoryInfo(workFileDirectory);
        }

        public void SetScreenInstance(UserControl screen)
        {
            _screenInst = screen;
        }

        public void SetPlaceHolderImageFilePath(string customErrorPlaceHolderFileName)
        {
            if (string.IsNullOrWhiteSpace(customErrorPlaceHolderFileName)) return;
            _placeholderFile = new FileInfo(customErrorPlaceHolderFileName);
        }
    }
}