using System;
using System.Drawing;
using System.Windows.Forms;
using Canon.Eos.Framework;
using Canon.Eos.Framework.Eventing;
using NLog;
using NomadLogic.Common.Extentions;
using NomadLogic.Common.Util;
using NomadLogic.iBrandDesktop.Util;

// ReSharper disable LocalizableElement
// ReSharper disable PossibleInvalidOperationException
#pragma warning disable 618

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.custom.VetGamesBooth
{
    public class TakePictureScreen : AbstractScreen
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private readonly Timer _countDownTimer;

        private readonly Bitmap _1Bitmap;
        private readonly Bitmap _2Bitmap;
        private readonly Bitmap _3Bitmap;
        private readonly Bitmap _4Bitmap;
        private readonly Bitmap _5Bitmap;

        private readonly Bitmap _freezeBitmap;

        private readonly PictureBox _panelPreview;

        private readonly Bitmap _flashBitmap;

        private int _countDown = 5;

        public TakePictureScreen(string backgroundImgEnSrc, ScreenConfig sc)
            : base(backgroundImgEnSrc, backgroundImgEnSrc, sc)
        {
            _countDownTimer = new Timer {Interval = 1000};
            //1 sec
            _countDownTimer.Tick += OnCountDownTimerEventt;
            
            //PictureBox.Paint += PaintEvent;

            _1Bitmap = GetNoneLanguageBitmap("1.png");
            _2Bitmap = GetNoneLanguageBitmap("2.png");
            _3Bitmap = GetNoneLanguageBitmap("3.png");
            _4Bitmap = GetNoneLanguageBitmap("4.png");
            _5Bitmap = GetNoneLanguageBitmap("5.png");

            _freezeBitmap = GetBitmap("freeze.png");
            _flashBitmap = GetNoneLanguageBitmap("flash.png");

            _panelPreview = new PictureBox
            {
                Location = new Point(150, 620),
                Size = new Size(780, 780),
                BackColor = Color.FromArgb(182, 0, 106)
            };

            Controls.Add(_panelPreview);

            var canonManager = CanonCameraManager.GetInstance();
            canonManager.LiveViewUpdate += HandlePictureUpdate;
            canonManager.FileCreatedOnCamera += OnFileCreatedOnCamera;
            canonManager.FileSavedOnHost += OnFileSavedOnHost;

            //PictureBox.MouseDown += MouseDownHandler;

            _panelPreview.BringToFront();
        }

        //private new void PaintEvent(object sender, PaintEventArgs e)
        //{
        //    base.PaintEvent(sender, e);


        //    try
        //    {
        //        var p = new Point((1080 - _5Bitmap.Width) / 2, 1450);

        //        if (_countDown == 5)
        //        {
        //            if (_5Bitmap != null)
        //            {
        //                e.Graphics.DrawImageUnscaled(_5Bitmap, p);
        //            }
        //        }
        //        else if (_countDown == 4)
        //        {
        //            if (_4Bitmap != null)
        //            {
        //                e.Graphics.DrawImageUnscaled(_4Bitmap, p);
        //            }
        //        }
        //        else if (_countDown == 3)
        //        {
        //            if (_3Bitmap != null)
        //            {
        //                e.Graphics.DrawImageUnscaled(_3Bitmap, p);
        //            }
        //        }
        //        else if (_countDown == 2)
        //        {
        //            if (_2Bitmap != null)
        //            {
        //                e.Graphics.DrawImageUnscaled(_2Bitmap, p);
        //            }
        //        }
        //        else if (_countDown == 1)
        //        {
        //            if (_1Bitmap != null)
        //            {
        //                e.Graphics.DrawImageUnscaled(_1Bitmap, p);
        //            }
        //        }
        //        else if (_countDown == 0)
        //        {
        //            if (_freezeBitmap != null)
        //            {
        //                _panelPreview.Image = _freezeBitmap;
        //                _panelPreview.Refresh();
        //            }

        //            if (_flashBitmap != null)
        //            {
        //                e.Graphics.DrawImageUnscaled(_flashBitmap, p);
        //            }
        //        }
        //    }
        //    catch (System.Exception err)
        //    {
        //        log.Error(err);
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

            _countDown = 5;

            _countDownTimer.Start();
        }

        public override void Recycle()
        {
            base.Recycle();
            _countDownTimer.Stop();
            _activated = false;
            if (_panelPreview.Image != null)
            {
                _panelPreview.Image = null;
            }
            Refresh();
        }

        private void OnCountDownTimerEventt(object sender, EventArgs e)
        {
            if (_countDown > 0)
            {
                _countDown--;

                Refresh();
            }
            else
            {
                _countDownTimer.Stop();
                var canonManager = CanonCameraManager.GetInstance();
                canonManager.TakePicture();
            }
        }

        private void CreatePicture(string pictureFile)
        {
            var pictureName = GenerateUniqueId.GetId() + ".png";
            var outputFilePath = GetTerminalDirectory(
                    DirectoryUtil.ProjectDataDirectoriesEnum.pictures, DirectoryUtil.ProjectDataDirectoriesModEnum.none) + pictureName;

            /*
            var resp = QuestionnaireControllerAccessor.TouchQuestionnaireController.GetValidatedCurrentRespondent();
            var tc =
                ((AbstractSubmitActionPictureTransformation)
                    (QuestionnaireControllerAccessor.TouchQuestionnaireController.Questionnaire.SubmitAction))
                    .PictureTransformationController;

            var liste = new List<Answer>();

            var a = new Answer(-1, _screenConfig.QuestionFirstPictureNo.Value, 1, pictureFile);
            liste.Add(a);

            resp.Answers = liste;

            var mainTransformation = tc.GetTransformation(resp, true);
            var result = mainTransformation.TransformImage();

            result.Save(ImageFormat.Png, 9, outputFilePath);

            result.Dispose();
            // On sauvegarde la question dans le questionnaire.
            SaveAnswerToQuestion(_screenConfig.QuestionFirstPictureNo.Value, 1, outputFilePath);
            */
        }

        private void OnFileCreatedOnCamera(object sender, IntPtr dirItem, EosCamera camera)
        {
            if (!IsDisposed && _activated)
            {
                camera.SavePicture(dirItem, DirectoryUtil.GetLocalMediaDir(DirectoryUtil.NomadLogicMediaDirectoriesEnum.pictures) + GenerateUniqueId.GetId() + ".jpg");
            }
        }

        private void HandlePictureUpdate(object sender, EosImageEventArgs e)
        {
            if (!IsDisposed && _activated)
                UpdatePicture(e.GetImage(), false);
        }

        private void OnFileSavedOnHost(object sender, EosFileEventArgs e)
        {
            try
            {
                if (_activated)
                {
                    CreatePicture(e.FilePath);
                }
            }
            catch (System.Exception ex)
            {
                log.Error(e);
                MessageBox.Show(WorkingArea.Container, ex.Message, @"iBrand(tm)", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            GC.Collect();
            Next();
        }

        private void UpdatePicture(Image image, bool force)
        {
            if (_panelPreview.InvokeRequired)
            {
                _panelPreview.Invoke(new Action(() => UpdatePicture(image, force)));
            }
            else
            {
                if (_countDown != 0 || force)
                {
                    _panelPreview.Image = image.ResizeForView(_panelPreview.Width, _panelPreview.Height);
                }
            }
        }
    }
}
