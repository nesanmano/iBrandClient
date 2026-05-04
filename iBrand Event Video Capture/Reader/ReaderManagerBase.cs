using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;
using JetBrains.Annotations;
using Newtonsoft.Json;
using NLog;
using NomadLogic.Common.Services.StateWatcher;
using NomadLogic.Common.Util.RfidReader;
using NomadLogic.Multimedia.Video;

namespace NomadLogic.App.EventVideoCapture.Reader
{
    [PublicAPI]
    [JsonConverter(typeof(ReaderManagerBaseConverter))]
    public abstract class ReaderManagerBase : IVideoCaptureModule
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private readonly List<IProgress<VideoCaptureReport>> _observerList = new List<IProgress<VideoCaptureReport>>();
        protected VideoCaptureReport _lastMessage;
        private bool _enterDetected;

        public IRfidReader Reader { get; set; } = new MR800_RfidReader();

        public virtual void Dispose()
        {
            if (Reader != null)
            {
                LinkEvents(Reader, false);
                Reader.Dispose();
            }

            lock (_observerList)
            {
                _observerList.Clear();
            }
        }

        /// <summary>
        /// Indique si le module roule.
        /// </summary>
        [JsonIgnore]
        public bool IsRunning => Reader?.IsReady() ?? false;

        [JsonIgnore]
        public string ConfigFileName { get; set; }

        public abstract string DefaultConfigFileName { get; }

        [JsonIgnore] public bool NeedTextBoxSource => !(Reader is IRfidEvents);

        public bool Init()
        {
            if (Reader == null || !Reader.ConnectReader())
            {
                return false;
            }

            LinkEvents(Reader, true);
            
            return ExtraInit();
        }

        /// <summary>
        /// Initialisation supplémentaire des classes enfants.
        /// </summary>
        /// <returns></returns>
        protected virtual bool ExtraInit()
        {
            return true;
        }

        private void LinkEvents(IRfidReader r, bool register)
        {
            if (!(r is IRfidEvents ie))
            {
                log.Warn("Non-event based RFID reader detected. Not fully implemented yet.");
                return;
            }

            if (register)
            {
                ie.CardRead += OnCardRead;
                ie.HardWareDisconnected += OnHardWareDisconnected;
                ie.InvalidCardRead += OnInvalidCardRead;
            }
            else
            {
                ie.CardRead -= OnCardRead;
                ie.HardWareDisconnected -= OnHardWareDisconnected;
                ie.InvalidCardRead -= OnInvalidCardRead;
            }
        }

        private void OnInvalidCardRead(object sender, RfidReadEventArgs e)
        {
            // No-op
        }

        private void OnHardWareDisconnected(object sender, EventArgs e)
        {
            var mess = "RFID Reader disconnected.";
            log.Error(mess);
            RaiseUiMessageEvent(mess);
        }

        private void OnCardRead(object sender, RfidReadEventArgs e)
        {
            ValidateCardScan(e.CardUid);
        }

        /// <summary>
        /// Provoque la validation d'un scan RFID. Si la validation fonctionne, un événement 'ScanDetectedAndValidated' sera
        /// émis.
        /// Cette méthode est publique pour permettre à une configuration sans événements lié au scanner de quand même fonctionner.
        /// </summary>
        /// <param name="eCardUid">L'identifiant RFID à vérifier.</param>
        public abstract Task ValidateCardScan(string eCardUid);

        /// <summary>
        /// Délégué pour la méthode de "callback" lorsque un déclenchement arrive.
        /// </summary>
        public delegate void ScannerEventHandler(string uniqueRespondentId);

        /// <summary>
        /// Indique quand le module à détecté un scan RFID et que les vérification nécessaires on eue lieu.
        /// </summary>
        public event ScannerEventHandler ScanDetectedAndValidated;

        /// <summary>
        /// Méthode permettant aux enfants d'invoquer l'événement d'un scan complet de leur père.
        /// </summary>
        protected void RaiseScanDetectedEvent(string uniqueId)
        {
            if (log.IsDebugEnabled)
            {
                UiMessageEvent?.Invoke(this, "Scan event raised.");
            }

            ScanDetectedAndValidated?.Invoke(uniqueId);
        }
        
        public event EventHandler<string> UiMessageEvent;

        /// <summary>
        /// Méthode permettant aux enfants d'invoquer l'événement d'un envoi de message UI de leur père.
        /// </summary>
        protected void RaiseUiMessageEvent(string message)
        {
            UiMessageEvent?.Invoke(this, message);
        }

        public abstract void UpdateWatcherList(IList<ExternalStateWatcher> watcherList);

        public void RemoveObserver(IProgress<VideoCaptureReport> observer)
        {
            lock (_observerList)
            {
                _observerList.Remove(observer);
            }
        }

        public void AddObserver(IProgress<VideoCaptureReport> observer)
        {
            lock (_observerList)
            {
                _observerList.Add(observer);
            }
        }

        public void TriggerReport(VideoCaptureReport report, bool dontCopy = false)
        {
            lock (_observerList)
            {
                _lastMessage = dontCopy ? report : report.Copy();
                foreach (var progress in _observerList)
                {
                    progress.Report(_lastMessage);
                }
            }
        }

        public void SendLastReport()
        {
            TriggerReport(_lastMessage);
        }

        /// <summary>
        /// Lie un textbox pour les événements quand on fait un scan via un scanner RFID qui ne supporte pas les événements,
        /// mais qui agit comme un clavier.
        /// </summary>
        /// <param name="linkedTextBox">Le textbox à se lier pour les événements clavier.</param>
        /// <param name="removeEvents">Si mis à vrai (faux par défaut), on enlève les liens à la place.</param>
        public void InitializeTextBoxEvents(TextBox linkedTextBox, bool removeEvents = false)
        {
            if (removeEvents)
            {
                linkedTextBox.KeyDown -= OnScanTextBoxOnKeyDown;
                linkedTextBox.KeyPress -= OnScanTextBoxOnKeyPress;
                linkedTextBox.TextChanged -= OnTextChangedScanTrigger;
            }
            else
            {
                log.Trace("Scan box Key detection event enabled.");
                linkedTextBox.KeyDown += OnScanTextBoxOnKeyDown;
                linkedTextBox.KeyPress += OnScanTextBoxOnKeyPress;
                linkedTextBox.TextChanged += OnTextChangedScanTrigger;
            }
        }

        private void OnTextChangedScanTrigger(object sender, EventArgs e)
        {
            if (!(sender is TextBox txtBox)) return;

            TextAnalyse(false, txtBox);
        }

        private void TextAnalyse(bool enterPressed, TextBox txtBox)
        {
            var check = false;
            var code = txtBox.Text.Trim();
            log.Trace("Analysing scan text: {1}", code);

            if (enterPressed)
            {
                // Avec un "Enter", on tente de valider immédiatement le texte.
                check = true;
            }
            else
            {
                var visCount = code.Length;
                if (Reader.ScanTriggerOnCharacterNumber > 0 && visCount >= Reader.ScanTriggerOnCharacterNumber)
                {
                    log.Debug("Automatic code validation launched by minimal number of caracter.");
                    check = true;
                }
            }
            // Les événement de claviers sont traités ailleurs (voir plus haut: ScanTriggerOnReturn)

            if (check)
            {
                //Lance le event
                txtBox.Clear();
                ValidateCardScan(code);
            }
        }

        /// <summary>
        /// Cette méthode modifie le code avec une façon de le rendre unique, selon la nature du lecteur utilisée.
        /// Surcharger cette méthode donnera accès à une unicité au besoin.
        /// Ne fait aucune modification de base.
        /// </summary>
        /// <param name="code">Le code brut.</param>
        /// <returns>Le code modifié (au besoin).</returns>
        protected virtual string AssignUniqueIdToCode(string code)
        {
            return code;
        }

        private void OnScanTextBoxOnKeyPress(object sender, KeyPressEventArgs e)
        {
            if (!_enterDetected) return;

            // On défait le event.
            _enterDetected = false;
            e.Handled = true;
            if (!(sender is TextBox txtBox)) return;
            TextAnalyse(true, txtBox);
        }

        private void OnScanTextBoxOnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter) return;
            log.Trace("Enter key pressed.");
            _enterDetected = true;
        }


    }
}