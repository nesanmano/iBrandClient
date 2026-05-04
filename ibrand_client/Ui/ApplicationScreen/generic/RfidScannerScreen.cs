using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using NLog;
using NomadLogic.Common.Ui.Dialog;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Json;
using NomadLogic.Common.Util.RfidReader;
using NomadLogic.Common.Util.Win32;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic
{
    /// <summary>
    /// Cet écran est utilisé pour saisir un code via lecture RFID. Les détails matériel de ce lecteur est isolé dans d'autre classes et accédés via
    /// une interface.
    /// </summary>
    public class RfidScannerScreen : ScanScreenAbstract
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private IRfidReader _rfidReader;
        
        /// <summary>
        /// Nom du fichier de configuration du lecteur RFID.
        /// </summary>
        public string RfidReaderConfigFile { get; set; } = "rfid_config.json";

        public RfidScannerScreen(string backgroundImgEnSrc, string backgroundImgFrSrc, ScreenConfig sc, Rectangle? textboxLocation = null) :
            base(backgroundImgEnSrc, backgroundImgFrSrc, sc, textboxLocation)
        { }

        protected override DialogResult ExecuteFacadeCustomCode(string cardUid, object res, Type returnType)
        {
            return (res != null || !FailOnNullResult) ? DialogResult.OK : DialogResult.Cancel;
        }

        protected override void OnLoad()
        {
            base.OnLoad();

            ScanTriggerOnCharacterNumber = _rfidReader?.ScanTriggerOnCharacterNumber ?? 0;
            ScanTriggerOnReturn = _rfidReader?.ScanTriggerOnEnter ?? false;
        }
        
        /// Dès que la zone de texte reçoit ce nombre de caractère (ou plus), cela provoque une vérification du code. Cette valeur est tirée de l'instance du lecteur RFID
        /// et est en lecture seule.
        public new int ScanTriggerOnCharacterNumber
        {
            get => _rfidReader?.ScanTriggerOnCharacterNumber ?? 0;
            private set
            {
                _scanTriggerOnCharacterNumber = value;
                UpdateScanCharacterTriggerEvent();
            }
        }
        
        protected override RfidTransformationResult TransformCode(string sequenceOriginal)
        {
            return _rfidReader.ExecuteSequenceAnalisys(sequenceOriginal);
        }

        protected override void CustomLoad()
        {
            base.CustomLoad();

            // Charger le lecteur RFID. S'il n'y a pas de fichier de configuration, on assume que c'est un lecteur "léger" (qui ne requiert pas de configuration complexe).
            var configFilePath = new FileInfo(DirectoryUtil.GetLocalIbrandTerminalDir(_screenConfig.ProjectId, _screenConfig.QuestionnaireId, _screenConfig.TerminalId) + RfidReaderConfigFile);
            _rfidReader = configFilePath.Exists ? JsonUtil.ReadJsonFile<IRfidReader>(configFilePath) : new Simple_RfidReader();

            if (_rfidReader == null)
            {
                throw new System.Exception("Error during marshalling RFID reader instance. Check JSON config file format.");
            }

            if (_rfidReader.ConnectReader())
            {
                ScanTriggerOnReturn = _rfidReader.ScanTriggerOnEnter;
                ScanTriggerOnCharacterNumber = _rfidReader.ScanTriggerOnCharacterNumber;

                // Vérification de spécialisation d'instance.
                if (_rfidReader is IRfidEvents rfidEvent)
                {
                    rfidEvent.CardRead += ScanManagerOnCardRead;
                    rfidEvent.HardWareDisconnected += ScanManagerOnHardWareDisconnected;
                    rfidEvent.InvalidCardRead += ScanManagerOnInvalidCardRead;
                }
            }
            else
            {
                if (_errorBitmap == null)
                {
                    MsgBox.Show(this, Resources.iBrand.BarcodeScreen_CannotConnect, Resources.iBrand.AppName,
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {
                    _showError = true;
                }
                Refresh();
            }

            InitBitmap();
        }

        private void ScanManagerOnInvalidCardRead(object sender, RfidReadEventArgs args)
        {
            if (!_activated) return;
            log.Debug("Invalid card read.");
            this.SafeCall(() =>
            {
                if (_errorBitmap == null)
                {
                    MsgBox.Show(this, Resources.iBrand.BarcodeScreen_InvalidBarcode, Resources.iBrand.AppName,
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {
                    _showError = true;
                }
                Refresh();
            });
        }

        private void ScanManagerOnHardWareDisconnected(object sender, EventArgs eventArgs)
        {
            log.Warn("Hardware disconnected event.");

            this.SafeCall(() =>
            {
                if (_errorBitmap == null)
                {
                    MsgBox.Show(this, Resources.iBrand.BarcodeScreen_PeripheralDisconnect, Resources.iBrand.AppName,
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {
                    _showError = true;
                }
                Refresh();
            });
        }

        private void ScanManagerOnCardRead(object sender, RfidReadEventArgs args)
        {
            if (!_activated) return;
            log.Debug("Card read event : " + args?.CardUid);
            UpdateWithScanValue(args?.CardUid);
        }
        
        public override void Dispose()
        {
            base.Dispose();

            // Décharger le lecteur RFID
            if (!(_rfidReader is IRfidEvents iRfidEvents)) return;

            iRfidEvents.CardRead -= ScanManagerOnCardRead;
            iRfidEvents.HardWareDisconnected -= ScanManagerOnHardWareDisconnected;
            iRfidEvents.InvalidCardRead -= ScanManagerOnInvalidCardRead;
            iRfidEvents.Dispose();
        }
        
    }
}