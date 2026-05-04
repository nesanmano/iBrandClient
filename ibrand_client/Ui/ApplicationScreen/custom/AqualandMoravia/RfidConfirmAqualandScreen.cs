using System.Drawing;
using NLog;
using NomadLogic.Common.Ui;
using NomadLogic.Common.Util.Win32;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.custom.AqualandMoravia
{
    public class RfidConfirmAqualandScreen : BarcodeScreen
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        public RfidConfirmAqualandScreen(string backgroundImgEnSrc, string backgroundImgFrSrc, ScreenConfig sc, Rectangle? textboxLocation = null)
            : base(backgroundImgEnSrc, backgroundImgFrSrc, sc, textboxLocation) { }

        public override void SetActivated()
        {
            base.SetActivated();

            // TODO Check if already authorized

            // If not authorized, scan new Band

            // When detecting authorized, show confirm Dialog

            ShowConfirmDialog();
        }

        private void ShowConfirmDialog()
        {
            this.SafeCall(() =>
            {
                var diagResult = new MessageDialog();
            });
        }
    }
}