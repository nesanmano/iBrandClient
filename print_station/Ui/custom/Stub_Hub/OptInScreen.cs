using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Ui;
using NomadLogic.Common.Util;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen;
using NomadLogic.iBrandDesktop.Ui.Choice;

namespace NomadLogic.PrintStation.Ui.custom.Stub_Hub
{
    public class OptInScreen : AbstractScreen
    {
        private readonly Rectangle _backArea;
        private readonly Rectangle _closeArea;
        private readonly Rectangle _linkArea1;
        private readonly Rectangle _linkArea2;
        private readonly Rectangle _nextArea;
        private readonly Rectangle _optInArea1;
        private readonly Rectangle _optInArea2;
        private readonly Bitmap _checkmark;
        private int _optIn1Checked;
        private int _optIn2Checked;

        public OptInScreen(string backgroundImgEnSrc, string backgroundImageSrc)
            : base(backgroundImgEnSrc, backgroundImageSrc)
        {
            //PictureBox.MouseClick += MouseDownHandler;
            //PictureBox.Paint += PaintEvent;

            _checkmark = GetBitmap("checkmark.png");

            _optInArea1 = new Rectangle(new Point(229, 370), new Size(61, 61));

            _optInArea2 = new Rectangle(new Point(229, 555), new Size(61, 61));

            _backArea = new Rectangle(new Point(114, 55), new Size(116, 110));

            _closeArea = new Rectangle(new Point(1717, 43), new Size(119, 122));

            _linkArea1 = new Rectangle(new Point(826, 546), new Size(188, 56));

            _linkArea2 = new Rectangle(new Point(1065, 549), new Size(194, 56));

            _nextArea = new Rectangle(new Point(816, 745), new Size(271, 125));
        }

        [DllImport("user32.dll")]
        private static extern bool EnableWindow(IntPtr hWnd, bool enable);

        protected override Bitmap GetBitmap(string fileName)
        {
            Bitmap bitmap = null;

            var path =
                DirectoryUtil.GetLocalPrintstationMediaDir(0, DirectoryUtil.NomadLogicPrintstationDirectoriesEnum.ui) +
                fileName;

            if (File.Exists(path))
            {
                var stream = new FileStream(path, FileMode.Open);
                bitmap = new Bitmap(stream);
                bitmap.SetResolution(96f, 96f);
                stream.Close();
            }

            return bitmap;
        }

        public override void Recycle()
        {
            base.Recycle();

            _optIn1Checked = 0;
            _optIn2Checked = 0;
        }

        //public new void PaintEvent(object sender, PaintEventArgs e)
        //{
        //    base.PaintEvent(sender, e);

        //    if (_optIn1Checked == 1)
        //    {
        //        if (_checkmark != null)
        //        {
        //            e.Graphics.DrawImageUnscaled(_checkmark, new Point(229, 360));
        //        }
        //    }

        //    if (_optIn2Checked == 1)
        //    {
        //        if (_checkmark != null)
        //        {
        //            e.Graphics.DrawImageUnscaled(_checkmark, new Point(229, 545));
        //        }
        //    }
        //}

        private bool SetAnswersToQuestionnaire()
        {
            /*var questionnaire = MainForm.GetInstance().Questionnaire;

            if (_optIn2Checked == 1)
            {
                var question = questionnaire.GetQuestion(3);
                var choiceCheck = (ChoiceCheck) question.GetChoice(1);
                var answer = new Answer(-1, question.No, 1);
                choiceCheck.SetAnswer(answer);
            }
            else
            {
                var mBox = new MessageDialog();
                mBox.ShowMessage("You must agree to the StubHub Terms of Use and Privacy Policy.");
                return false;
            }

            if (_optIn1Checked == 1)
            {
                var question = questionnaire.GetQuestion(2);
                var choiceCheck = (ChoiceCheck) question.GetChoice(1);
                var answer = new Answer(-1, question.No, 1);
                choiceCheck.SetAnswer(answer);
            }*/
            return true;
        }

        public new void MouseDownHandler(object sender, MouseEventArgs e)
        {
            if (InArea(e.X, e.Y, _closeArea))
            {
                Close();
            }
            else if (InArea(e.X, e.Y, _nextArea))
            {
                if (SetAnswersToQuestionnaire())
                    Next();
            }
            else if (InArea(e.X, e.Y, _optInArea1))
            {
                if (_optIn1Checked == 0)
                {
                    _optIn1Checked = 1;
                }
                else
                {
                    _optIn1Checked = 0;
                }

                Refresh();
            }
            else if (InArea(e.X, e.Y, _optInArea2))
            {
                if (_optIn2Checked == 0)
                {
                    _optIn2Checked = 1;
                }
                else
                {
                    _optIn2Checked = 0;
                }

                Refresh();
            }
            else if (InArea(e.X, e.Y, _linkArea2))
            {
                MainFormUtil.GetInstance()
                    .MainForm.BeginInvoke(
                        new Action(() => EnableWindow(MainFormUtil.GetInstance().MainForm.Handle, false)));

                var t = new Thread(ShowUrlLink2);
                t.SetApartmentState(ApartmentState.STA);
                t.Start();

                SuspendTimer();
            }
            else if (InArea(e.X, e.Y, _linkArea1))
            {
                MainFormUtil.GetInstance()
                    .MainForm.BeginInvoke(
                        new Action(() => EnableWindow(MainFormUtil.GetInstance().MainForm.Handle, false)));

                var t = new Thread(ShowUrlLink1);
                t.SetApartmentState(ApartmentState.STA);
                t.Start();

                SuspendTimer();
            }
            else if (InArea(e.X, e.Y, _backArea))
            {
                Recycle();
                Back();
            }
        }

        private void ShowUrlLink2()
        {
            /*
            var dialog = new WebBrowserTouchDlg(false, new Size(1280, 800));
            dialog.BringToFront();
            var uri =
                new Uri("file:///" +
                        DirectoryUtil.GetLocalPrintstationMediaDir(0,
                            DirectoryUtil.NomadLogicPrintstationDirectoriesEnum.ui) + "privacy_policy.htm");
            dialog.Navigate(uri.ToString());
            dialog.OnBrowserClosed += OnBrowserClosed;
            dialog.ShowDialog();
            */

            ResumeTimer();
        }

        private void ShowUrlLink1()
        {
            /*
            var dialog = new WebBrowserTouchDlg(false, new Size(1280, 800));
            dialog.BringToFront();
            dialog.Navigate("file:///" + Constants.APP_DIR + "ui" + Path.DirectorySeparatorChar + "stub_hub" +
                            Path.DirectorySeparatorChar + "user_agreement.htm");
            dialog.OnBrowserClosed += OnBrowserClosed;
            dialog.ShowDialog();
            */
            ResumeTimer();
        }

        private void OnBrowserClosed(object sender, EventArgs e)
        {
            MainFormUtil.GetInstance()
                .MainForm.BeginInvoke(new Action(() => EnableWindow(MainFormUtil.GetInstance().MainForm.Handle, true)));
            SingleApplication.SwitchToCurrentInstance();
        }

        public override void SetActivated()
        {
            base.SetActivated();
        }
    }
}