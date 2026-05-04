using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Threading;
using System.Windows.Forms;
using Canon.Eos.Framework;
using Canon.Eos.Framework.Eventing;
using NLog;
using NomadLogic.Common.Util;
using NomadLogic.iBrandDesktop.Ui.Choice;
using NomadLogic.iBrandDesktop.Util;
using Timer = System.Windows.Forms.Timer;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.custom.Permatex
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
        readonly Bitmap _souriezBitmap;

        readonly PictureBox _panelPreview;

        readonly Bitmap _flashBitmap;

        private int _countDown = 5;

        private string _pictureTaken;


        public TakePictureScreen(string backgroundImgEnSrc)
            : base(backgroundImgEnSrc, backgroundImgEnSrc)
        {
            _countDownTimer = new Timer();
            _countDownTimer.Interval = 1000; //1 sec
            _countDownTimer.Tick += OnCountDownTimerEventt;

            //PictureBox.Paint += PaintEvent;

            _1Bitmap = GetNoneLanguageBitmap("1.png");
            _2Bitmap = GetNoneLanguageBitmap("2.png");
            _3Bitmap = GetNoneLanguageBitmap("3.png");
            _4Bitmap = GetNoneLanguageBitmap("4.png");
            _5Bitmap = GetNoneLanguageBitmap("5.png");

            _freezeBitmap = GetBitmap("freeze.png", "en");
            _souriezBitmap = GetBitmap("freeze.png", "fr");

            _flashBitmap = GetNoneLanguageBitmap("flash.png");

            _panelPreview = new PictureBox();
            _panelPreview.Location = new Point(286, 65);
            _panelPreview.Size = new Size(720, 480);
            _panelPreview.BackColor = Color.White;

            Controls.Add(_panelPreview);

            var canonManager = CanonCameraManager.GetInstance();
            canonManager.LiveViewUpdate += HandlePictureUpdate;
            canonManager.FileCreatedOnCamera += OnFileCreatedOnCamera;
            canonManager.FileSavedOnHost += OnFileSavedOnHost;

            _panelPreview.BringToFront();

            //PictureBox.MouseDown += MouseDownHandler;

        }

        /*public void MouseDownHandler(object sender, MouseEventArgs e)
        {
            base.MouseDownHandler(sender, e);
        }*/

        //public new void PaintEvent(object sender, PaintEventArgs e)
        //{
        //    base.PaintEvent(sender, e);

        //    if (_countDown == 5)
        //    {
        //        if (_5Bitmap != null)
        //        {
        //            e.Graphics.DrawImageUnscaled(_5Bitmap, new Point(599, 580));
        //        }
        //    }
        //    else if (_countDown == 4)
        //    {
        //        if (_4Bitmap != null)
        //        {
        //            e.Graphics.DrawImageUnscaled(_4Bitmap, new Point(599, 580));
        //        }
        //    }
        //    else if (_countDown == 3)
        //    {
        //        if (_3Bitmap != null)
        //        {
        //            e.Graphics.DrawImageUnscaled(_3Bitmap, new Point(599, 580));
        //        }
        //    }
        //    else if (_countDown == 2)
        //    {
        //        if (_2Bitmap != null)
        //        {
        //            e.Graphics.DrawImageUnscaled(_2Bitmap, new Point(599, 580));
        //        }
        //    }
        //    else if (_countDown == 1)
        //    {
        //        if (_1Bitmap != null)
        //        {
        //            e.Graphics.DrawImageUnscaled(_1Bitmap, new Point(599, 580));
        //        }
        //    }
        //    else if (_countDown == 0)
        //    {
        //        if (_freezeBitmap != null)
        //        {
        //            CanonCameraManager.GetInstance().StopLiveView();

        //            try {
        //                if (_activeLg.Equals("en"))
        //                    _panelPreview.Image = _freezeBitmap;
        //                else
        //                    _panelPreview.Image = _souriezBitmap;

        //                _panelPreview.Refresh();
        //            }
        //            catch (System.Exception ex)
        //            {
        //                log.Error("YYYYP :" + ex.Message);
        //            }
        //        }

        //        if (_flashBitmap != null)
        //        {
        //            e.Graphics.DrawImageUnscaled(_flashBitmap, new Point(599, 580));
        //        }
        //    }
        //    else if (_countDown == -1)
        //    {
        //        if (_flashBitmap != null)
        //        {
        //            e.Graphics.DrawImageUnscaled(_flashBitmap, new Point(599, 580));
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

            if (!canonManager.IsLiveView())
                canonManager.StartLiveView();

            _countDown = 5;

            _pictureTaken = null;

            _countDownTimer.Start();
        }

        public override void Recycle()
        {
            base.Recycle();
            _countDownTimer.Stop();
            _activated = false;
            if (_panelPreview.Image != null)
            {
                //panelPreview.Image.Dispose();
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
                    CanonCameraManager.GetInstance().StartLiveView();
                    _countDown = 5;
                    _countDownTimer.Start();
                }

            }
        }

        
        private void MakePictureToPrint()
        {
            if (_pictureTaken != null)
            {
                var questionnaire = ScreensControllerAccessor.ScreensController.Questionnaire;
                var question = questionnaire.GetQuestion(ConstantsPhotoBooth._question_first_picture_no);
                var choiceText = (ChoiceText)question.GetChoice(1);

                var pictureName = GenerateUniqueId.GetId() + ".jpg";

                var outputFilePath = //abstractConfiguration.PicturesDir + 
                    pictureName;

                var image = Image.FromFile(_pictureTaken);

                if (image.Width != 1350)
                    image = ResizeImage(image, 1350, 900);

                image.Save(outputFilePath, ImageFormat.Jpeg);
                image.Dispose();

//                var answer = new Answer(QuestionTypeEnum.Photo, question.No, choiceText.No);
//                answer.Response = pictureName;
//                choiceText.SetAnswer(answer);
            }

            CanonCameraManager.GetInstance().StartLiveView();

            Next();
        }

        private void OnFileCreatedOnCamera(object sender, IntPtr dirItem, EosCamera camera)
        {
            camera.SavePicture(dirItem, "pictures\\" + GenerateUniqueId.GetId() + ".jpg");
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
                    _pictureTaken = e.FilePath;

                    new Thread(MakePictureToPrint).Start();
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
                    if (image.Width != 720)
                        image = ResizeImage(image, 720, 480);

                    image.RotateFlip(RotateFlipType.RotateNoneFlipX);
                    _panelPreview.Image = image;
                    //final_bitmap.Dispose();
                }
            }
        }

        

        public Bitmap ResizeImage(Image image, int width, int height)
        {
            var result = new Bitmap(width, height);
            // Set the resolutions the same to avoid cropping due to resolution differences
            result.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(result))
            {
                //Set the resize quality modes to high quality
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                //draw the image into the target bitmap
                graphics.DrawImage(image, 0, 0, result.Width, result.Height);
                graphics.Dispose();
            }

            return result;
        }

    }
}
