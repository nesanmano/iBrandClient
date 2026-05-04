using System;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using Canon.Eos.Framework;
using Canon.Eos.Framework.Eventing;
using NLog;
using NomadLogic.Common.Extentions;
using NomadLogic.Common.Util;
using NomadLogic.iBrandDesktop.Util;
using NomadLogic.Multimedia.XmlTransformation.Filters;
using Timer = System.Windows.Forms.Timer;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.custom.InfoPresse
{
    public class TakePictureScreen : AbstractScreen
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        private readonly Timer _countDownTimer;

        readonly Bitmap _1Bitmap;
        readonly Bitmap _2Bitmap;
        readonly Bitmap _3Bitmap;
        readonly Bitmap _4Bitmap;
        readonly Bitmap _5Bitmap;

        readonly Bitmap _freezeBitmap;

        readonly PictureBox _panelPreview;

        readonly Bitmap _flashBitmap;

        private int _countDown = 5;

        private string _pictureTaken;
        private readonly string _overlay;

        public TakePictureScreen(ScreenConfig sc, string backgroundImgSrc, string overlay, string freeze)
            : base(backgroundImgSrc, backgroundImgSrc, sc)
        {
            _overlay = overlay;

            _countDownTimer = new Timer { Interval = 1000 }; //1 sec
            _countDownTimer.Tick += OnCountDownTimerEventt;

            //PictureBox.Paint += PaintEvent;

            _1Bitmap = GetNoneLanguageBitmap("1.png");
            _2Bitmap = GetNoneLanguageBitmap("2.png");
            _3Bitmap = GetNoneLanguageBitmap("3.png");
            _4Bitmap = GetNoneLanguageBitmap("4.png");
            _5Bitmap = GetNoneLanguageBitmap("5.png");

            _freezeBitmap = FileUtil.LoadBitmap(freeze);
            _flashBitmap = GetNoneLanguageBitmap("flash.png");

            _panelPreview = new PictureBox
            {
                Location = new Point((PictureBox.Width - 675) / 2, 110),
                Size = new Size(675, 450),
                BackColor = Color.White
            };

            Controls.Add(_panelPreview);

            var canonManager = CanonCameraManager.GetInstance();
            canonManager.LiveViewUpdate += HandlePictureUpdate;
            canonManager.FileCreatedOnCamera += OnFileCreatedOnCamera;
            canonManager.FileSavedOnHost += OnFileSavedOnHost;

            _panelPreview.BringToFront();

            //PictureBox.MouseDown += MouseDownHandler;
        }


        //public new void PaintEvent(object sender, PaintEventArgs e)
        //{
        //    base.PaintEvent(sender, e);

        //    switch (_countDown)
        //    {
        //        case 5:
        //            e.Graphics.DrawImageUnscaled(_5Bitmap, new Point(605, 620));
        //            break;
        //        case 4:
        //            e.Graphics.DrawImageUnscaled(_4Bitmap, new Point(605, 620));
        //            break;
        //        case 3:
        //            e.Graphics.DrawImageUnscaled(_3Bitmap, new Point(605, 620));
        //            break;
        //        case 2:
        //            e.Graphics.DrawImageUnscaled(_2Bitmap, new Point(605, 620));
        //            break;
        //        case 1:
        //            e.Graphics.DrawImageUnscaled(_1Bitmap, new Point(605, 620));
        //            break;
        //        case 0:
        //            try
        //            {
        //                SetPreviewPicture(_panelPreview, _freezeBitmap, _freezeBitmap);
        //                _panelPreview.Refresh();
        //            }
        //            catch (System.Exception ex)
        //            {
        //                log.Error("Could not display Freeze image. :" + ex.Message);
        //            }

        //            e.Graphics.DrawImageUnscaled(_flashBitmap, new Point(605, 620));
        //            break;
        //    }
        //}

        public override void SetActivated()
        {
            base.SetActivated();

            var canonManager = CanonCameraManager.GetInstance();

            if (!canonManager.IsCameraConnected())
            {
                MessageBox.Show(this, @"Camera is not connected.", @"iBrand (tm)", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                Back();
                return;
            }

            canonManager.StartLiveView();

            _countDown = 5;

            _pictureTaken = null;

            _countDownTimer.Start();
        }

        public override void Recycle()
        {
            base.Recycle();

            SetPreviewPicture(_panelPreview, _freezeBitmap, null);
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

                CanonCameraManager.GetInstance().StopLiveView();

                if (CanonCameraManager.GetInstance().TakePicture()) return;

                // Si ça n'a pas marché.
                CanonCameraManager.GetInstance().StartLiveView();
                _countDown = 5;
                _countDownTimer.Start();
            }
        }

        private void MakePictureToPrint()
        {
            if (_pictureTaken == null) return;

            var fileName = GetTerminalDirectory(
                               DirectoryUtil.ProjectDataDirectoriesEnum.pictures, DirectoryUtil.ProjectDataDirectoriesModEnum.none)
                           + GenerateUniqueId.GetId() + ".jpg";
            var outputFileName = GetTerminalDirectory(
                                     DirectoryUtil.ProjectDataDirectoriesEnum.pictures, DirectoryUtil.ProjectDataDirectoriesModEnum.transformed)
                                 + GenerateUniqueId.GetId() + ".jpg";

            File.Move(_pictureTaken, fileName);

//            if (_screenConfig.QuestionFirstPictureNo != null)
//                SaveAnswerToQuestion(_screenConfig.QuestionFirstPictureNo.Value, 1, fileName);

            using (var finalImage = FileUtil.LoadBitmap(fileName))
            {
                // Étape un: Désaturation
                var filter = new GreyScaleFilter();
                using (var grey = filter.Filter(finalImage))
                {
                    // Étape deux: overlay
                    using (var overlay = FileUtil.LoadBitmap(_overlay))
                    {
                        grey.AddOverlay(overlay);
                    }

                    // Sauvegarde
                    grey.Save(90, outputFileName);
                    //SaveAnswerToQuestion(3, 1, outputFileName);
                }
            }

            SetPreviewPicture(_panelPreview, _freezeBitmap, null);

            Next();
        }

        private static void OnFileCreatedOnCamera(object sender, IntPtr dirItem, EosCamera camera)
        {
            camera.SavePicture(dirItem, DirectoryUtil.GetLocalMediaDir(DirectoryUtil.NomadLogicMediaDirectoriesEnum.pictures) + GenerateUniqueId.GetId() + ".jpg");
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
            if (!_activated) return;

            try
            {
                _pictureTaken = e.FilePath;

                new Thread(MakePictureToPrint).Start();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(WorkingArea.Container, ex.Message, @"iBrand(tm)", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                if (_countDown == 0 && !force) return;

                SetPreviewPicture(_panelPreview, _freezeBitmap, image.Frame(_panelPreview.Width, _panelPreview.Height));
            }
        }
    }
}
