using System;
using System.Windows.Forms;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.custom.RadioCanada
{
    public class ThanksScreen : AbstractScreen
    {

        private readonly Timer _nextPageTimer;

        public ThanksScreen(string backgroundImageSrc, ScreenConfig sc)
            : base(backgroundImageSrc, backgroundImageSrc, sc){

            _nextPageTimer = new Timer {Interval = 3000};
            _nextPageTimer.Tick += OnNextPageTimerEventt;
        }

        public void OnNextPageTimerEventt(object sender, EventArgs e)
        {
            _nextPageTimer.Stop();

            Next();
        }

        public override void SetActivated()
        {
            base.SetActivated();

            _nextPageTimer.Start();
        }

    }
}
