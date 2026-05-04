using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using NLog;
using NomadLogic.iBrandDesktop.Util;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.custom.KijijiHalloweenParty
{
    public class ShareScreen : AbstractScreen
    {
        private delegate void UiDelegateIbrand(bool success, Dictionary<string, string> results);

        // ReSharper disable once UnusedMember.Local
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        readonly Rectangle _nextArea;
        readonly PictureBox _picture;

        private readonly Rectangle _smsArea;
        private readonly Rectangle _printArea;

        private string _photoFileName;


        public ShareScreen(string backgroundImageSrc, ScreenConfig sc)
            : base(backgroundImageSrc, backgroundImageSrc, sc)
        {
           

            //PictureBox.MouseDown += MouseDownHandler;
            _picture = new PictureBox
            {
                Location = new Point((PictureBox.Width - 480) / 2, 118),
                Size = new Size(480, 480),
                BackColor = Color.White,
                Visible = true
            };
            //PictureBox.Paint += PaintEvent;

            Controls.Add(_picture);

            _picture.BringToFront();


            _printArea = new Rectangle(new Point(906, 266), new Size(87, 87));
            _smsArea = new Rectangle(new Point(906, 380), new Size(87, 87));


            _nextArea = new Rectangle(new Point(506, 620), new Size(277, 110));

        }

        protected override void OnTimerEvent(object sender, EventArgs e)
        {
            End(); // On ne renvoi pas au premier écran, on termine.
        }

        //protected override void MouseDownHandler(object sender, MouseEventArgs e)
        //{
        //    base.MouseDownHandler(sender, e);

        //    if (InArea(e.X, e.Y, _nextArea))
        //    {
        //        End();
        //    }
           
        //    else if (InArea(e.X, e.Y, _smsArea))
        //    {
        //        SuspendTimer();
        //        Next(1);
        //    }
        //    else if (InArea(e.X, e.Y, _printArea))
        //    {
        //        if (_screenConfig.QuestionPictureToPrintNo == null) return;
        //        SuspendTimer();
        //        PrintAndIncrement(GetTextAnswerFromQuestion(_screenConfig.QuestionPictureToPrintNo.Value, 1),
        //            _screenConfig.QuestionNumberOfPrintNo, null);
        //        ResumeTimer();
        //    }
           
        //}

        public override void Recycle()
        {
            base.Recycle();

            _picture.Image?.Dispose();
            _picture.Image = null;

            _photoFileName = null;
        }

        public override void SetActivated()
        {
            base.SetActivated();

            try
            {
                if (_photoFileName != null) return;
                if (_screenConfig.QuestionGifNo == null) return;

                _photoFileName = GetTextAnswerFromQuestion(_screenConfig.QuestionGifNo.Value, 1);
                _picture.Image?.Dispose();
                _picture.ImageLocation = _photoFileName;

               
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(WorkingArea.Container, ex.Message, @"iBrand (tm)", MessageBoxButtons.OK,
                    MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
            }
        }

        
    }
}
