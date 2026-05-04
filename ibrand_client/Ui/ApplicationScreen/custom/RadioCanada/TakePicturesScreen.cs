using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using Canon.Eos.Framework;
using Canon.Eos.Framework.Eventing;
using NLog;
using NomadLogic.Common.Extentions;
using NomadLogic.Common.Util;
using NomadLogic.iBrandDesktop.Util;
using NomadLogic.Multimedia.XmlTransformation.image;
using Timer = System.Windows.Forms.Timer;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.custom.RadioCanada
{
    public class TakePicturesScreen : AbstractScreen
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private readonly Timer _countDownTimer;

        readonly Bitmap _1Bitmap;
        readonly Bitmap _2Bitmap;
        readonly Bitmap _3Bitmap;
        readonly Bitmap _4Bitmap;
        readonly Bitmap _5Bitmap;

        Bitmap _creating1;
        Bitmap _creating2;
        Bitmap _creating3;

        readonly Bitmap _freezeBitmap;

        readonly PictureBox _panelPreview;

        readonly Bitmap _flashBitmap;

        private int _countDown = 5;
        private int _creatingCount;

        private string _pictureTaken;

        readonly TransformationsController _transformationController;

        public TakePicturesScreen(string backgroundImgEnSrc, ScreenConfig sc)
            : base(backgroundImgEnSrc, backgroundImgEnSrc, sc)
        {
            _countDownTimer = new Timer {Interval = 1000};
            //1 sec
            _countDownTimer.Tick += OnCountDownTimerEventt;

            var creatingTimer = new Timer();
            creatingTimer.Interval = 1000; //1 sec
            creatingTimer.Tick += OnCreatingTimerEventt;

            //PictureBox.Paint += PaintEvent;

            _1Bitmap = GetNoneLanguageBitmap("1.png");
            _2Bitmap = GetNoneLanguageBitmap("2.png");
            _3Bitmap = GetNoneLanguageBitmap("3.png");
            _4Bitmap = GetNoneLanguageBitmap("4.png");
            _5Bitmap = GetNoneLanguageBitmap("5.png");

            _creating1 = GetBitmap("creating_1.png");
            _creating2 = GetBitmap("creating_2.png");
            _creating3 = GetBitmap("creating_3.png");

            _freezeBitmap = GetNoneLanguageBitmap("freeze.png");
            _flashBitmap = GetNoneLanguageBitmap("flash.png");

            _panelPreview = new PictureBox();
            _panelPreview.Location = new Point((PictureBox.Size.Width - 720) / 2, 95);
            _panelPreview.Size = new Size(720, 480);
            _panelPreview.BackColor = Color.White;

            Controls.Add(_panelPreview);

            var canonManager = CanonCameraManager.GetInstance();
            canonManager.LiveViewUpdate += HandlePictureUpdate;
            canonManager.FileCreatedOnCamera += OnFileCreatedOnCamera;
            canonManager.FileSavedOnHost += OnFileSavedOnHost;

            _panelPreview.BringToFront();

            //PictureBox.MouseDown += MouseDownHandler;

            _transformationController = new TransformationsController();
            _transformationController.Initialize(GetTerminalDirectory() +  Path.DirectorySeparatorChar + "picture_transformation_config.xml");
        }

        public override bool ChangeLanguage(string language)
        {
            var c = base.ChangeLanguage(language);
            if (!c) return false;

            _creating1?.Dispose();
            _creating1 = GetBitmap("creating_1.png");

            _creating2?.Dispose();
            _creating2 = GetBitmap("creating_2.png");

            _creating3?.Dispose();
            _creating3 = GetBitmap("creating_3.png");
            return true;
        }

        //public new void PaintEvent(object sender, PaintEventArgs e)
        //{
        //    base.PaintEvent(sender, e);

        //    var point = new Point(603, 615);

        //     if (_countDown == 5)
        //     {
        //         if (_5Bitmap != null)
        //         {
        //             e.Graphics.DrawImageUnscaled(_5Bitmap, point);
        //         }
        //     }
        //     else if (_countDown == 4)
        //     {
        //         if (_4Bitmap != null)
        //         {
        //             e.Graphics.DrawImageUnscaled(_4Bitmap, point);
        //         }
        //     }
        //    else if (_countDown == 3)
        //    {
        //        if (_3Bitmap != null)
        //        {
        //            e.Graphics.DrawImageUnscaled(_3Bitmap, point);
        //        }
        //    }
        //    else if (_countDown == 2)
        //    {
        //        if (_2Bitmap != null)
        //        {
        //            e.Graphics.DrawImageUnscaled(_2Bitmap, point);
        //        }
        //    }
        //    else if (_countDown == 1)
        //    {
        //        if (_1Bitmap != null)
        //        {
        //            e.Graphics.DrawImageUnscaled(_1Bitmap, point);
        //        }
        //    }
        //    else if (_countDown == 0)
        //    {
        //        if (_freezeBitmap != null)
        //        {
        //            try
        //            {
        //                SetPreviewPicture(_panelPreview, _freezeBitmap, _freezeBitmap);
                        
        //            }
        //            catch (System.Exception ex)
        //            {
        //                log.Error(ex, "Could not update preview image.");
        //            }
        //        }

        //        if (_flashBitmap != null)
        //        {
        //            e.Graphics.DrawImageUnscaled(_flashBitmap, new Point(603, 615));
        //        }
        //    }
        //    else if (_countDown == -1)
        //    {
        //        if (_flashBitmap != null)
        //        {
        //            e.Graphics.DrawImageUnscaled(_flashBitmap, new Point(603, 615));
        //        }
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

            _countDown = 5;

            _pictureTaken = null;

            _countDownTimer.Start();

            SetPreviewPicture(_panelPreview, _freezeBitmap, null);
        }


        public override void Recycle()
        {
            base.Recycle();
            _countDownTimer.Stop();
            _activated = false;
            SetPreviewPicture(_panelPreview, _freezeBitmap, null);

            try
            {
                Refresh();
            }
            catch (System.Exception ex)
            {
                log.Error(ex, "Refresh failed.");
            }
        }

        
        public void OnCountDownTimerEventt(object sender, EventArgs e)
        {
            if (_countDown > 0)
            {
                _countDown--;

                try
                {
                    Refresh();
                }
                catch (System.Exception ex)
                {
                    log.Error(ex, "Refresh failed.");
                }
            }
            else
            {
                _countDownTimer.Stop();

                if (!CanonCameraManager.GetInstance().TakePicture())
                {
                    CanonCameraManager.GetInstance().StartLiveView();
                    _countDown = 5;
                    _countDownTimer.Start();
                }
            }
        }

        public void OnCreatingTimerEventt(object sender, EventArgs e)
        {
            if (_creatingCount == 3)
                _creatingCount = 0;

            _creatingCount++;
            Refresh();
        }

        private void TransformPicture()
        {
            if (_pictureTaken != null)
            {

                var pictureName = GenerateUniqueId.GetId() + ".jpg";

                //SaveAnswerToQuestion(7, 1, _pictureTaken);

                //On fait la transformation maintenant.
                var resp = ScreensControllerAccessor.ScreensController.GetValidatedCurrentRespondent() as Respondent;

                resp.Answers = ScreensControllerAccessor.ScreensController.Questionnaire.GetAnswersToSaveInDb();
                var trans = _transformationController.GetTransformations(resp, false);
                trans[0].Respondent = resp;

                var originale = trans[0].TransformImage();

                var outputFilePath = DirectoryUtil.GetLocalMediaDir(DirectoryUtil.NomadLogicMediaDirectoriesEnum.pictures) +
                    GenerateUniqueId.GetId() + ".jpg";

                originale.Save(outputFilePath, ImageFormat.Jpeg);

                //SaveAnswerToQuestion(7, 1, outputFilePath);

                trans[0].Dispose();
            }

            var canonManager = CanonCameraManager.GetInstance();
            canonManager.StartLiveView();

            Next();
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
            {
                UpdatePicture(e.GetImage(), false);
            }
        }

        private void OnFileSavedOnHost(object sender, EosFileEventArgs e)
        {
            try
            {
                if (_activated)
                {
                    _pictureTaken = e.FilePath;

                    new Thread(TransformPicture).Start();
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(WorkingArea.Container, ex.Message, "iBrand(tm)", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private void UpdatePicture(Image image, bool force)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => UpdatePicture(image, force)));
            }
            else
            {
                if (_countDown != 0 || force)
                {
                    //Bitmap finalBitmap = MakeImageForView(image, _panelPreview.Width, _panelPreview.Height);
                    var finalBitmap = image.Resize(720, 480);
                    finalBitmap.RotateFlip(RotateFlipType.RotateNoneFlipX);
                    //_panelPreview.Image = finalBitmap;

                    SetPreviewPicture(_panelPreview, _freezeBitmap, finalBitmap);
                }
            }
        }
    }
}
