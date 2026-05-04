using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Canon.Eos.Framework;
using Canon.Eos.Framework.Eventing;
using NomadLogic.Common.Extentions;
using NomadLogic.Common.Util;
using NomadLogic.iBrandDesktop.Ui.Choice;
using NomadLogic.iBrandDesktop.Util;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.custom.ParcCanada
{
    public partial class TakeSinglePictureScreen : AbstractScreen
    {

        private readonly Timer _countDownTimer;

        readonly Bitmap[] _bmpTab = new Bitmap[10];

        readonly PictureBox _panelPreview;

        readonly Bitmap _hourglassBitmap;
        readonly Bitmap _guideBitmap;

        private int _countDown = 10;

        private readonly List<string> _fileList = new List<string>();

        public TakeSinglePictureScreen(string backgroundImgEnSrc, ScreenConfig sc)
            : base(backgroundImgEnSrc, backgroundImgEnSrc, sc)
        {
            _countDownTimer = new Timer();
            _countDownTimer.Interval = 1000; //1 sec
            _countDownTimer.Tick += OnCountDownTimerEventt;

            //PictureBox.Paint += PaintEvent;

            for (var i = 0; i < 10; i++)
            {
                _bmpTab[i] = GetNoneLanguageBitmap((i + 1) + ".png");
            }

            _hourglassBitmap = GetNoneLanguageBitmap("hourglass.png");
            _guideBitmap = GetNoneLanguageBitmap("guide.png");
            _guideBitmap.SetResolution(112, 112);

            _panelPreview = new PictureBox();
            _panelPreview.Location = new Point(486, 223);
            _panelPreview.Size = new Size(950, 643);
            _panelPreview.BackColor = Color.White;

            Controls.Add(_panelPreview);

            var canonManager = CanonCameraManager.GetInstance();
            canonManager.LiveViewUpdate += HandlePictureUpdate;
            canonManager.FileCreatedOnCamera += OnFileCreatedOnCamera;
            canonManager.FileSavedOnHost += OnFileSavedOnHost;

            _panelPreview.BringToFront();

            //PictureBox.MouseDown += MouseDownHandler;
        }

        public List<string> GetPictureFiles()
        {
            return _fileList;
        }

        //protected override void MouseDownHandler(object sender, MouseEventArgs e)
        //{
        //    base.MouseDownHandler(sender, e);
        //}

        //public new void PaintEvent(object sender, PaintEventArgs e)
        //{
        //    base.PaintEvent(sender, e);

        //    var numberPoint = new Point(0, 0);

        //    if (_countDown > 0)
        //    {
        //        e.Graphics.DrawImageUnscaled(_bmpTab[_countDown - 1], numberPoint);
        //    }
        //    else
        //    {
        //        e.Graphics.DrawImageUnscaled(_hourglassBitmap, numberPoint);
        //    }
        //}

        public override void SetActivated()
        {
            base.SetActivated();

            var canonManager = CanonCameraManager.GetInstance();

            if (!canonManager.IsCameraConnected())
            {
                MessageBox.Show(this, "Camera is not connected.", "iBrand (tm)", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                Back();
                return;
            }

            if (!canonManager.IsLiveView())
                canonManager.StartLiveView();

            _countDown = 10;

            _fileList.Clear();
            _countDownTimer.Start();
        }

        public override void Recycle()
        {
            base.Recycle();

            _countDownTimer.Stop();
            _activated = false;

            if (_panelPreview.Image != null)
            {
                _panelPreview.Image.Dispose();
                _panelPreview.Image = null;
            }
            Refresh();
        }

        public void OnCountDownTimerEventt(object sender, EventArgs e)
        {
            if (_countDown > 0)
            {
                _countDown--;
                Refresh();
            }
            else
            {
                _countDownTimer.Stop();

                if (!CanonCameraManager.GetInstance().TakePicture())
                {
                    _countDownTimer.Start();
                    _countDown = 5;
                }
            }
        }

        private void OnFileCreatedOnCamera(object sender, IntPtr dirItem, EosCamera camera)
        {
            var dir = GetTerminalDirectory(
                DirectoryUtil.ProjectDataDirectoriesEnum.pictures, DirectoryUtil.ProjectDataDirectoriesModEnum.none);
            camera.SavePicture(dirItem, dir + GenerateUniqueId.GetId() + ".jpg");
        }

        private void HandlePictureUpdate(object sender, EosImageEventArgs e)
        {
            if (!IsDisposed && _activated)
                UpdatePicture(e.GetImage());
        }

        private void OnFileSavedOnHost(object sender, EosFileEventArgs e)
        {
            try
            {
                if (_activated)
                {
                    var questionnaire = ScreensControllerAccessor.ScreensController.Questionnaire;
                    var question = questionnaire.GetQuestion(4);
                    var choiceText = (ChoiceText)question.GetChoice(1);

//                    var answer = new Answer(QuestionTypeEnum.Photo, question.No, choiceText.No);
//                    answer.Response = e.FilePath;
//                    choiceText.SetAnswer(answer);

                    Next();
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(WorkingArea.Container, ex.Message, "iBrand(tm)", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdatePicture(Image image)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => UpdatePicture(image)));
            }
            else
            {
                image = image.Resize(1013, 675);

                image.RotateFlip(RotateFlipType.RotateNoneFlipX);

                ((Bitmap)image).SetResolution(96f, 96f);
                var toShow = new Bitmap(1013, 675);
                toShow.SetResolution(96f, 96f);
                var g = Graphics.FromImage(toShow);

                g.DrawImage(image, 0, 0);

                g.DrawImage(_guideBitmap, 690, -25);

                g.Dispose();

                _panelPreview.Image = toShow;
            }
        }
    }
}
