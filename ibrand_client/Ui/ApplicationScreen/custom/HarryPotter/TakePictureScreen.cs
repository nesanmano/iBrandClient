using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using Canon.Eos.Framework;
using Canon.Eos.Framework.Eventing;
using NLog;
using NomadLogic.Common.Services;
using NomadLogic.Common.Services.Configuration;
using NomadLogic.Common.Util;
using NomadLogic.iBrandDesktop.Ui.Choice;
using NomadLogic.iBrandDesktop.Util;
using NomadLogic.Multimedia.XmlTransformation.image;
using Timer = System.Windows.Forms.Timer;
// ReSharper disable LocalizableElement
// ReSharper disable UnusedVariable

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.custom.HarryPotter
{
    public class TakePictureScreen : AbstractScreen
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private readonly Timer _countDownTimer;

        readonly Bitmap _1Bitmap;
        readonly Bitmap _2Bitmap;
        readonly Bitmap _3Bitmap;
        //Bitmap _4_bitmap = null;
        //Bitmap _5_bitmap = null;

        readonly Bitmap _freezeBitmap;

        readonly PictureBox _panelPreview;

        readonly Bitmap _flashBitmap;

        private int _countDown = 3;

        private string _pictureTaken;

        readonly TransformationsController _transformationController;

        public TakePictureScreen(string backgroundImgEnSrc)
            : base(backgroundImgEnSrc, backgroundImgEnSrc)
        {
            //PictureBox.MouseDown += MouseDownHandler;
            
            _countDownTimer = new Timer();
            _countDownTimer.Interval = 1000; //1 sec
            _countDownTimer.Tick += OnCountDownTimerEventt;

            //PictureBox.Paint += PaintEvent;

            _1Bitmap = GetNoneLanguageBitmap("_1.png");
            _2Bitmap = GetNoneLanguageBitmap("_2.png");
            _3Bitmap = GetNoneLanguageBitmap("_3.png");
            //_4_bitmap = GetNoneLanguageBitmap("_4.png");
            //_5_bitmap = GetNoneLanguageBitmap("_5.png");

            _freezeBitmap = GetNoneLanguageBitmap("freeze.png");
            _flashBitmap = GetNoneLanguageBitmap("flash.png");

            _panelPreview = new PictureBox();
            _panelPreview.Location = new Point(420, 95);
            _panelPreview.Size = new Size(480, 480);
            _panelPreview.BackColor = Color.White;

            Controls.Add(_panelPreview);

            var canonManager = CanonCameraManager.GetInstance();
            canonManager.LiveViewUpdate += HandlePictureUpdate;
            canonManager.FileCreatedOnCamera += OnFileCreatedOnCamera;
            canonManager.FileSavedOnHost += OnFileSavedOnHost;
            
            canonManager.StartLiveView();

            _panelPreview.BringToFront();

            _transformationController = new TransformationsController();
            _transformationController.Initialize(ProjectAccessor.Project.ProjectDir + ScreensControllerAccessor.ScreensController.Questionnaire.Id + Path.DirectorySeparatorChar + "picture_transformation_config.xml");
        }

            
        //public new void PaintEvent(object sender, PaintEventArgs e)
        //{
        //    base.PaintEvent(sender, e);

        //    var point = new Point(630, 615);

        //   /* if (count_down == 5)
        //    {
        //        if (_5_bitmap != null)
        //        {
        //            e.Graphics.DrawImageUnscaled(_5_bitmap, point);
        //        }
        //    }
        //    else if (count_down == 4)
        //    {
        //        if (_4_bitmap != null)
        //        {
        //            e.Graphics.DrawImageUnscaled(_4_bitmap, point);
        //        }
        //    }
        //    else*/ if (_countDown == 3)
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
        //            //CanonCameraManager.GetInstance().StopLiveView();

        //            try { 
        //                _panelPreview.Image = _freezeBitmap;
        //                _panelPreview.Refresh();
        //            }
        //            catch (System.Exception ex)
        //            {
        //                log.Error("YYYYP :" + ex.Message);
        //            }
        //        }

        //        if (_flashBitmap != null)
        //        {
        //            e.Graphics.DrawImageUnscaled(_flashBitmap, new Point(620, 615));
        //        }
        //    }
        //    else if (_countDown == -1)
        //    {
        //        if (_flashBitmap != null)
        //        {
        //            e.Graphics.DrawImageUnscaled(_flashBitmap, new Point(620, 615));
        //        }
        //    }
           
        //}

        public override void SetActivated()
        {
            base.SetActivated();

            var canonManager = CanonCameraManager.GetInstance();

            if (!canonManager.IsCameraConnected())
            {

                MessageBox.Show(this, "Camera is not connected.", "iBrand (tm)", MessageBoxButtons.OK,
                    MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                Back();
                return;
            }
            canonManager.StartLiveView();

            _countDown = 3;

            _pictureTaken = null;

            _countDownTimer.Start();

            if (_panelPreview.Image != null)
            {
                _panelPreview.Image = null;
            }
        }

        public override void Recycle()
        {
            base.Recycle();
            _countDownTimer.Stop();
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
                    _countDown = 3;
                    _countDownTimer.Start();
                }
            }
        }

        private void TransformPicture()
        {
            var configuration = ServicesContainer.GetService<NomadConfiguration>();

            if (_pictureTaken != null)
            {

                var pictureName = GenerateUniqueId.GetId() + ".jpg";

                var outputFilePath = //configuration.PicturesDir + 
                    pictureName;
                
                var image = Image.FromFile(_pictureTaken);


                var b = CropImage(image, image.Height, image.Height);

                b.Save(outputFilePath, ImageFormat.Jpeg);

                b.Dispose();

                var questionnaire = ScreensControllerAccessor.ScreensController.Questionnaire;
                
                var question = questionnaire.GetQuestion(2);
                
                var choiceText = (ChoiceText)question.GetChoice(1);
                
                //var answer = new Answer(QuestionTypeEnum.Photo, question.No, choiceText.No);
                //answer.Response = outputFilePath;
                
                //choiceText.SetAnswer(answer);

                //On fait la transformation maintenant.
                var resp = ScreensControllerAccessor.ScreensController.GetValidatedCurrentRespondent() as Respondent;

                resp.Answers = ScreensControllerAccessor.ScreensController.Questionnaire.GetAnswersToSaveInDb();
                var trans = _transformationController.GetTransformations(resp, false);
                /*
                trans.Respondent = resp;
                
                var originale = trans.TransformImage();

                outputFilePath = //configuration.TransformedPicturesDir + 
                    pictureName;

                answer.Response = outputFilePath;

                choiceText.SetAnswer(answer);
                originale.Save(answer.Response, ImageFormat.Jpeg);

                trans.Dispose();
                */
            }

            var canonManager = CanonCameraManager.GetInstance();
            canonManager.StartLiveView();
            
            Next();
        }

        private void OnFileCreatedOnCamera(object sender, IntPtr dirItem, EosCamera camera)
        {
            if (_activated)
            {
                camera.SavePicture(dirItem, "pictures\\" + GenerateUniqueId.GetId() + ".jpg");
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
                    if (image.Width != 720)
                        image = ResizeImage(image, 720, 480);

                    var finalBitmap = CropImage(image, _panelPreview.Width, _panelPreview.Height);
                    finalBitmap.RotateFlip(RotateFlipType.RotateNoneFlipX);
                    _panelPreview.Image = finalBitmap;
                    //final_bitmap.Dispose();
                }
            }
        }

        public Bitmap CropImage(Image image, int width, int height)
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
                var x = (image.Width - width) / 2;
                var y = (image.Height - height) / 2;
                graphics.DrawImage(image, 0, 0, new Rectangle(x, y, result.Width, result.Height),
                    GraphicsUnit.Pixel);
                graphics.Dispose();
            }

            return result;
        }

        public Bitmap CropImage(Image image, int width, int height, Bitmap logo)
        {
            var result = new Bitmap(width, height);
            // Set the resolutions the same to avoid cropping due to resolution differences
            result.SetResolution(image.HorizontalResolution, image.VerticalResolution);
            logo.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(result))
            {
                //Set the resize quality modes to high quality
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                //draw the image into the target bitmap
                var x = (image.Width - width) / 2;
                var y = (image.Height - height) / 2;
                graphics.DrawImage(image, 0, 0, new Rectangle(x, y, result.Width, result.Height),
                    GraphicsUnit.Pixel);
                graphics.DrawImage(logo, new Point(width - logo.Width, height - logo.Height));
                graphics.Dispose();
            }

            return result;
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
