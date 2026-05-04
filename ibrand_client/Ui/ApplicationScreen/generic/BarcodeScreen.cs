using System;
using System.Drawing;
using System.Windows.Forms;
using NomadLogic.iBrandDesktop.Util.BarcodeScanner;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic
{
    /// <summary>
    /// Cet écran est utilisé pour saisir un code barre, soit manuellement, soit automatiquement via les événements de lecteur optique.
    /// </summary>
    public class BarcodeScreen : ScanScreenAbstract, IBarcodeScannerListener
    {
        public BarcodeScreen(string backgroundImgEnSrc, string backgroundImgFrSrc, ScreenConfig sc, Rectangle? textboxLocation = null)
            : base(backgroundImgEnSrc, backgroundImgFrSrc, sc, textboxLocation) { }

        protected override DialogResult ExecuteFacadeCustomCode(string cardUid, object res, Type returnType)
        {
            return (res != null || !FailOnNullResult) ? DialogResult.OK : DialogResult.Cancel;
        }

        /// <inheritdoc />
        public void Scanner_Read(object sender, ScanEventArgs args)
        {
            UpdateWithScanValue(args.BarCode);
        }
        
        /// <summary>
        /// Enveloppe "AppendScanValue"
        /// </summary>
        public bool AppendBarCode
        {
            get => AppendScanValue;
            set => AppendScanValue = value;
        }

        /// <inheritdoc />
        public bool BarCodeActivated
        {
            /* Retourne toujours vrai. */
            get => true;
            set { }
        }
    }
}