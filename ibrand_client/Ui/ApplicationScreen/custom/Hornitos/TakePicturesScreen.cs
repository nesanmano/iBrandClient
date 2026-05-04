using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using Canon.Eos.Framework;
using Canon.Eos.Framework.Eventing;
using NomadLogic.Common;
using NomadLogic.Common.Extentions;
using NomadLogic.Common.Util;
using NomadLogic.iBrandDesktop.Ui.Choice;
using NomadLogic.iBrandDesktop.Util;
using NomadLogic.Multimedia.Gif;
using Timer = System.Windows.Forms.Timer;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.custom.Hornitos
{
    public class TakePicturesScreen : AbstractScreen
    {
        //private EOSFrameworkManager _manager;

       // private List<EosCamera> _cameras = new List<EosCamera>();

        readonly Rectangle _configurationArea;

        private readonly Timer _countDownTimer;
        private readonly Timer _creatingTimer;

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

        readonly Bitmap _overlayBitmap;
        
        private int _countDown = 5;
        private int _creatingCount;

        private int _numberOfPictureTaken;

        private readonly List<string> _fileList = new List<string>();

        public TakePicturesScreen(string backgroundImgEnSrc)
            : base(backgroundImgEnSrc, backgroundImgEnSrc)
        {
            _countDownTimer = new Timer();
            _countDownTimer.Interval = 1000; //1 sec
            _countDownTimer.Tick += OnCountDownTimerEventt;

            _creatingTimer = new Timer();
            _creatingTimer.Interval = 1000; //1 sec
            _creatingTimer.Tick += OnCreatingTimerEventt;

            //PictureBox.Paint += PaintEvent;

            _1Bitmap = GetNoneLanguageBitmap("1.png");
            _2Bitmap = GetNoneLanguageBitmap("2.png");
            _3Bitmap = GetNoneLanguageBitmap("3.png");
            _4Bitmap = GetNoneLanguageBitmap("4.png");
            _5Bitmap = GetNoneLanguageBitmap("5.png");

            _creating1 = GetBitmap("creating_1.png");
            _creating2 = GetBitmap("creating_2.png");
            _creating3 = GetBitmap("creating_3.png");

            _freezeBitmap = GetBitmap("freeze.png");
            _flashBitmap = GetNoneLanguageBitmap("flash.png");    

            _panelPreview = new PictureBox();
            _panelPreview.Location = new Point(400, 45);
            _panelPreview.Size = new Size(480, 480);
            _panelPreview.BackColor = Color.White;

            Controls.Add(_panelPreview);

            var canonManager = CanonCameraManager.GetInstance();
            canonManager.LiveViewUpdate += HandlePictureUpdate;
            canonManager.FileCreatedOnCamera += OnFileCreatedOnCamera;
            canonManager.FileSavedOnHost += OnFileSavedOnHost;
            //canon_manager.CameraShutdown += this.OnCameraShutDown;

            _panelPreview.BringToFront();

            PictureBox.MouseDown += MouseDownHandler;

            _configurationArea = new Rectangle(new Point(0, 650), new Size(70, 70));

            _overlayBitmap = FileUtil.LoadBitmap(Constants.APP_DIR + "ui" + Path.DirectorySeparatorChar + "hornitos" + Path.DirectorySeparatorChar + "overlay.png");
        }

        /*public void OnCameraShutDown(object sender, EventArgs args)
        {
           // _count_down_timer.Stop(); 
            //this.Back();
        }*/

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

        public new void MouseDownHandler(object sender, MouseEventArgs e)
        {
            if (InArea(e.X, e.Y, _configurationArea))
            {
                var pwDlg = new PasswordDialog(_activeLg);
                pwDlg.ShowDialog();
                if (pwDlg.DialogResult == DialogResult.OK)
                {
                    new TouchModeConfigurationDlg().ShowDialog();
                }
            }
        }

        //public new void PaintEvent(object sender, PaintEventArgs e)
        //{
        //    base.PaintEvent(sender, e); 

        //    if (_countDown == 5)
        //    {
        //        if (_5Bitmap != null)
        //        {
        //            e.Graphics.DrawImageUnscaled(_5Bitmap, new Point(605, 550));
        //        }
        //    }
        //    else if (_countDown == 4)
        //    {
        //        if (_4Bitmap != null)
        //        {
        //            e.Graphics.DrawImageUnscaled(_4Bitmap, new Point(605, 550));
        //        }
        //    }
        //    else if (_countDown == 3)
        //    {
        //        if (_3Bitmap != null)
        //        {
        //            e.Graphics.DrawImageUnscaled(_3Bitmap, new Point(605, 550));
        //        }
        //    }
        //    else if (_countDown == 2)
        //    {
        //        if (_2Bitmap != null)
        //        {
        //            e.Graphics.DrawImageUnscaled(_2Bitmap, new Point(605, 550));
        //        }
        //    }
        //    else if (_countDown == 1)
        //    {
        //        if (_1Bitmap != null)
        //        {
        //            e.Graphics.DrawImageUnscaled(_1Bitmap, new Point(605, 550));
        //        }
        //    }
        //    else if (_countDown == 0)
        //    {
        //        if (_freezeBitmap != null)
        //        {
        //            CanonCameraManager.GetInstance().StopLiveView();
        //            _panelPreview.Image = _freezeBitmap;
        //            _panelPreview.Refresh();
        //        }

        //        if (_flashBitmap != null)
        //        {
        //            e.Graphics.DrawImageUnscaled(_flashBitmap, new Point(605, 550));
        //        }
        //    }
        //    else if (_countDown == -1 && _creatingCount == 0)
        //    {
        //        if (_flashBitmap != null)
        //        {
        //            e.Graphics.DrawImageUnscaled(_flashBitmap, new Point(605, 550));
        //        }
        //    }
        //    else if (_creatingCount == 1)
        //    {
        //        if (_creating1 != null)
        //        {
        //            e.Graphics.DrawImageUnscaled(_creating1, new Point(535, 570));
        //        }
        //    }
        //    else if (_creatingCount == 2)
        //    {
        //        if (_creating2 != null)
        //        {
        //            e.Graphics.DrawImageUnscaled(_creating2, new Point(535, 570));
        //        }
        //    }
        //    else if (_creatingCount == 3)
        //    {
        //        if (_creating3 != null)
        //        {
        //            e.Graphics.DrawImageUnscaled(_creating3, new Point(535, 570));
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
            _creatingCount = 0;

            _fileList.Clear();
            _numberOfPictureTaken = 0;
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

                if (_numberOfPictureTaken < 4)
                {
                    if (!CanonCameraManager.GetInstance().TakePicture())
                        _countDownTimer.Start();
                    if (_numberOfPictureTaken < 3)
                    {
                        Refresh();
                        _countDown = 5;
                    }
                    else
                    {
                        _countDown = -1;
                    }
                }
                else
                {
                    _countDown = -1;
                    _creatingTimer.Start();

                    new Thread(MakeGifAndPictureToPrint).Start();
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

        private void MakeGifAndPictureToPrint()
        {
            if (_fileList.Count > 0)
            {
                var questionnaire = ScreensControllerAccessor.ScreensController.Questionnaire;
                var question = questionnaire.GetQuestion(ConstantsPhotoBooth._question_first_picture_no);
                var choiceText = (ChoiceText)question.GetChoice(1);

                var pictureName = GenerateUniqueId.GetId() + ".jpg";

                var outputFilePath = //abstractConfiguration.PicturesDir + 
                    pictureName;

                var image = Image.FromFile(_fileList[0]);

                if (image.Width != 720)
                    image = ResizeImage(image, 720, 480);

                var b = CropImage(image, _panelPreview.Width, _panelPreview.Height);

                
                var result = BitmapExtentions.CreateFramedPicture(b, _overlayBitmap, 0, 0);

                result.Save(outputFilePath, ImageFormat.Jpeg);
                result.Dispose();

                b.Dispose();

                /*
                var answer = new Answer(QuestionTypeEnum.Photo, question.No, choiceText.No);
                answer.Response =outputFilePath;
                choiceText.SetAnswer(answer);
                */
            }

            CreateAnimatedGif();

            //Image to print
            CreatePictureToPrint();

            _creatingTimer.Stop();

            CanonCameraManager.GetInstance().StartLiveView();

            Next();
        }

        private void CreateAnimatedGif()
        {
            var pictureName = GenerateUniqueId.GetId() + ".gif";

            var outputFilePath = //abstractConfiguration.PicturesDir + 
                pictureName;

            var a = new AnimatedGifEncoder();
            a.Start(outputFilePath);
            a.SetDelay(350);
            //-1:no repeat,0:always repeat
            a.SetRepeat(0);

            //Bitmap logo = GetBitmap("logo.png");

            foreach (var fileName in _fileList)
            {
                var image = Image.FromFile(fileName);
                
                if (image.Width != 720)
                    image = ResizeImage(image, 720, 480);
                
                var b = CropImage(image, _panelPreview.Width, _panelPreview.Height/*, logo*/);

                var result = BitmapExtentions.CreateFramedPicture(b, _overlayBitmap, 0, 0);

                a.AddFrame(result);
                b.Dispose();
                result.Dispose();
            }

            a.Finish();

            //logo.Dispose();

            var questionnaire = ScreensControllerAccessor.ScreensController.Questionnaire;
            var question = questionnaire.GetQuestion(ConstantsPhotoBooth._question_gif_no);
            var choiceText = (ChoiceText)question.GetChoice(1);

//            var answer = new Answer(QuestionTypeEnum.Photo, question.No, choiceText.No);
//            answer.Response = pictureName;
//            choiceText.SetAnswer(answer);
        }

        private void CreatePictureToPrint()
        {
            if (ConstantsPhotoBooth._question_picture_to_print_no == -1) return;
            var pictureName = GenerateUniqueId.GetId() + ".jpg";

            var outputFilePath = //abstractConfiguration.PicturesDir + 
                pictureName;

            var finalImage = GetBitmap("frame.png");//new Bitmap(975, 975);
            finalImage.SetResolution(72f, 72f);

            var g = Graphics.FromImage(finalImage);

            var count = 0;
            var row = 0;
            var mod = 0;

            foreach (var fileName in _fileList)
            {
                var image = Image.FromFile(fileName);

                if (image.Width != 720)
                    image = ResizeImage(image, 720, 480);

                var b = CropImage(image, _panelPreview.Width, _panelPreview.Height);
                //b.RotateFlip(RotateFlipType.RotateNoneFlipX);

                g.DrawImage(b, new Point(45 + ((mod * b.Width) + (mod * 15)), 45 + ((row * b.Height) + (row * 15))));

                b.Dispose();

                count++;

                mod = (count % 2);

                if (mod == 0)
                    row++;
            }

            g.Dispose();

            finalImage.Save(outputFilePath, ImageFormat.Jpeg);
            finalImage.Dispose();

            var questionnaire = ScreensControllerAccessor.ScreensController.Questionnaire;
            var question = questionnaire.GetQuestion(ConstantsPhotoBooth._question_picture_to_print_no);
            var choiceText = (ChoiceText)question.GetChoice(1);

//            var answer = new Answer(QuestionTypeEnum.Photo, question.No, choiceText.No);
//            answer.Response = pictureName;
//            choiceText.SetAnswer(answer);
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
                    _fileList.Add(e.FilePath);
                    _numberOfPictureTaken++;

                    UpdatePicture(e.GetImage(), true);

                    if (_numberOfPictureTaken < 4)
                        CanonCameraManager.GetInstance().StartLiveView();

                    Refresh();
                    _countDownTimer.Start();
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
                graphics.DrawImage(logo, new Point(width-logo.Width, height-logo.Height));
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
