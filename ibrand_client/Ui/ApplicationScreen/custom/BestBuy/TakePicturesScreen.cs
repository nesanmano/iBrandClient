using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using GoPro.Hero;
using GoPro.Hero.Hero3;
using NLog;
using NomadLogic.Common.Extentions;
using NomadLogic.Common.Services;
using NomadLogic.Common.Services.Configuration;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Networking;
using NomadLogic.iBrandDesktop.Ui.Choice;
using NomadLogic.iBrandDesktop.Util;
using NomadLogic.Multimedia.Gif;
using Image = System.Drawing.Image;
using Timer = System.Windows.Forms.Timer;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.custom.BestBuy
{
    public class TakePicturesScreen : AbstractScreen
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private readonly Timer _countDownTimer;
        private readonly Timer _creatingTimer;

        private readonly Bitmap _1Bitmap;
        private readonly Bitmap _2Bitmap;
        private readonly Bitmap _3Bitmap;
        private readonly Bitmap _4Bitmap;
        private readonly Bitmap _5Bitmap;

        private Bitmap _creating1;
        private Bitmap _creating2;
        private Bitmap _creating3;

        private readonly Bitmap _freezeBitmap;
        private readonly Bitmap _beReadyBitmap;

        private readonly PictureBox _panelPreview;

        private readonly Bitmap _flashBitmap;
        
        private int _countDown = 5;
        private int _creatingCount;

        private int _numberOfPictureTaken;

        private readonly List<string> _fileList = new List<string>();

        private Hero3Camera _camera;

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
            _beReadyBitmap = GetBitmap("be_ready.jpg");
            _flashBitmap = GetNoneLanguageBitmap("flash.png");    

            _panelPreview = new PictureBox();
            _panelPreview.Location = new Point(405, 85);
            _panelPreview.Size = new Size(480, 480);
            _panelPreview.BackColor = Color.White;
            _panelPreview.SizeMode = PictureBoxSizeMode.StretchImage;

            if (_beReadyBitmap != null)
                _panelPreview.Image = _beReadyBitmap;

            Controls.Add(_panelPreview);

            _camera = GetCamera();

            _panelPreview.BringToFront();

            ServicesContainer.GetService<NomadConfiguration>();

            //PictureBox.MouseDown += MouseDownHandler;
        }

        private Hero3Camera GetCamera()
        {
            Hero3Camera camera = null;
            
            try
            {
                if (NetUtil.Ping("10.5.5.9", 2, 1000))
                {
                    camera = Camera.Create<Hero3Camera>("10.5.5.9");
                    camera.Mode(Mode.Photo);
                }
            }
            catch (System.Exception ex)
            {
                log.Error("Problem getting camera :" + ex.StackTrace);
            }
            return camera;
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
        //            //CanonCameraManager.GetInstance().StopLiveView();
        //            _panelPreview.Image = _freezeBitmap;
        //            _panelPreview.Refresh();
        //        }

        //        if (_flashBitmap != null)
        //        {
        //            e.Graphics.DrawImageUnscaled(_flashBitmap, new Point(599, 580));
        //        }
        //    }
        //    else if (_countDown == -1 && _creatingCount == 0)
        //    {
        //        if (_flashBitmap != null)
        //        {
        //            e.Graphics.DrawImageUnscaled(_flashBitmap, new Point(599, 580));
        //        }
        //    }
        //    else if (_creatingCount == 1)
        //    {
        //        if (_creating1 != null)
        //        {
        //            e.Graphics.DrawImageUnscaled(_creating1, new Point(535, 600));
        //        }
        //    }
        //    else if (_creatingCount == 2)
        //    {
        //        if (_creating2 != null)
        //        {
        //            e.Graphics.DrawImageUnscaled(_creating2, new Point(535, 600));
        //        }
        //    }
        //    else if (_creatingCount == 3)
        //    {
        //        if (_creating3 != null)
        //        {
        //            e.Graphics.DrawImageUnscaled(_creating3, new Point(535, 600));
        //        }
        //    }
        //}

        public override void SetActivated()
        {
            base.SetActivated();

            if (!NetUtil.Ping("10.5.5.9", 2, 1000))
            {
                MessageBox.Show(this, "Camera is not connected. See User Guide.", "iBrand (tm)", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                Back();
                return;
            }
            if (_camera == null)
            {
                GetCamera();
                if (_camera == null)
                {
                    MessageBox.Show(this, "Could not connect to Camera. See User Guide.", "iBrand (tm)", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                    Back();
                    return;
                }
            }

            /*BacpacStatus status = _camera.BacpacStatus();

            if (!status.CameraReady)
            {
                MessageBox.Show(this, "Camera is not ready. See User Guide.", "iBrand (tm)", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                Back();
                return;
            }*/
            //Start live view TODO

            if (_beReadyBitmap != null)
                _panelPreview.Image = _beReadyBitmap;

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
                //panelPreview.Image.Dispose();
                _panelPreview.Image = null;
            }

            try
            {
                Refresh();
            }
            catch (System.Exception ex)
            {
                log.Error("YYoooYP :" + ex.Message);
            }
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

                TakePicture();

                if (_numberOfPictureTaken < 4)
                {
                    _panelPreview.Image = _beReadyBitmap;
                    _countDown = 3;
                    _countDownTimer.Start();
                    Refresh();
                }
                else
                {
                    _countDown = -1;

                    _creatingTimer.Start();

                    new Thread(MakeGifAndPictureToPrint).Start();
                }
            }
        }

        private void TakePicture()
        {
            try
            {
                if (_camera == null)
                    _camera = GetCamera();

                if (_camera == null)
                    throw new System.Exception();

                _camera.OpenShutter();

                var browser = _camera.Contents();

                Thread.Sleep(2000);

                var image = browser.ImagesAsync().Result.LastOrDefault();

                if (image == null)
                    return;

                var response = image.Download().GetResponseStream();

                var pictureName = //_config.PicturesDir + 
                    GenerateUniqueId.GetId() + ".jpg";

                if (!ReadToFile(response, pictureName))
                    return;

                _fileList.Add(pictureName);

                _numberOfPictureTaken++;

                //this.UpdatePicture(System.Drawing.Image.FromFile(picture_name), true);

                /*if (_number_of_picture_taken < 4)
                    CanonCameraManager.GetInstance().StartLiveView();*/
            }
            catch (System.Exception ex)
            {
                log.Error("Problem with camera :" + ex.StackTrace);
                MessageBox.Show(this, "Could not take picture. See User Guide.", "iBrand (tm)", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
            }
        }

        private bool ReadToFile(Stream response, string fileName)
        {
            FileStream f = null;

            try
            {
                f = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None);

                var buffer = new byte[4096];
                while (true)
                {
                    var readCount = response.Read(buffer, 0, buffer.Length);
                    f.Write(buffer, 0, readCount);
                    if (readCount == 0) break;
                }
            }
            catch (System.Exception ex)
            {
                log.Error(ex.StackTrace);
                return false;
            }
            finally
            {
                if (f != null)
                    f.Close();
            }

            return true;
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

                var outputFilePath = //_config.PicturesDir + 
                    pictureName;

                var image = Image.FromFile(_fileList[0]);


                image = image.Resize(640, 480);

                var b = image.Crop(_panelPreview.Width, _panelPreview.Height);

                b.Save(outputFilePath, ImageFormat.Jpeg);
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

            /*if (_camera != null)
            {
                try
                {
                    _camera.DeleteAllFilesOnSdCard();

                }catch(System.Exception){

                }
            }*/

            Next();
        }

        private void CreateAnimatedGif()
        {
            var pictureName = GenerateUniqueId.GetId() + ".gif";

            var outputFilePath = //_config.PicturesDir + 
                pictureName;

            var a = new AnimatedGifEncoder();
            a.Start(outputFilePath);
            a.SetDelay(350);
            //-1:no repeat,0:always repeat
            a.SetRepeat(0);

            foreach (var fileName in _fileList)
            {
                var image = Image.FromFile(fileName);
                
                image = image.Resize(960, 720);

                var b = image.Crop(image.Height, image.Height);

                a.AddFrame(b);
                b.Dispose();
            }

            a.Finish();

            var questionnaire = ScreensControllerAccessor.ScreensController.Questionnaire;
            var question = questionnaire.GetQuestion(ConstantsPhotoBooth._question_gif_no);
            var choiceText = (ChoiceText)question.GetChoice(1);

            /*
            var answer = new Answer(QuestionTypeEnum.Photo, question.No, choiceText.No);
            answer.Response = pictureName;
            choiceText.SetAnswer(answer);
            */
        }

        private void CreatePictureToPrint()
        {
            if (ConstantsPhotoBooth._question_picture_to_print_no != -1)
            {
                var pictureName = GenerateUniqueId.GetId() + ".jpg";

                var outputFilePath = //_config.PicturesDir + 
                    pictureName;

                var finalImage = GetBitmap("frame.jpg");//new Bitmap(975, 975);
                finalImage.SetResolution(72f, 72f);

                var g = Graphics.FromImage(finalImage);

                var count = 0;
                var row = 0;
                var mod = 0;

                foreach (var fileName in _fileList)
                {
                    var image = Image.FromFile(fileName);

                    image = image.Resize(640, 480);

                    var b = image.Crop(_panelPreview.Width, _panelPreview.Height);

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

                /*
                var answer = new Answer(QuestionTypeEnum.Photo, question.No, choiceText.No);
                answer.Response = pictureName;
                choiceText.SetAnswer(answer);
                */
            }
        }

        /*private void UpdatePicture(System.Drawing.Image image, bool force)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => this.UpdatePicture(image, force)));
            }
            else
            {
                if (count_down != 0 || force)
                {
                    if (image.Width != 2560)
                        image = ImageUtil.ResizeImage(image, 2560, 1920);

                    Bitmap final_bitmap = ImageUtil.CropImageToSize(image, 1920, 1920);
                    final_bitmap.RotateFlip(RotateFlipType.RotateNoneFlipX);
                    panelPreview.Image = final_bitmap;
                    panelPreview.Refresh();
                }
            }
        }*/
      
     
    }
}
