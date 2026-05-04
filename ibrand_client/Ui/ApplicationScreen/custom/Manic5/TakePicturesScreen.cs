using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;
using System.Windows.Forms;
using Canon.Eos.Framework;
using Canon.Eos.Framework.Eventing;
using NLog;
using NomadLogic.Common.Extentions;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Util;
using NomadLogic.iBrandDesktop.Ui.Choice;
using NomadLogic.iBrandDesktop.Util;
using NomadLogic.Multimedia.Gif;
using NomadLogic.Multimedia.XmlTransformation.Filters;
using Timer = System.Windows.Forms.Timer;
// ReSharper disable PossibleLossOfFraction

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.custom.Manic5
{
    public class TakePicturesScreen : AbstractScreen
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
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

        private int _countDown = 5;
        private int _creatingCount;

        private int _numberOfPictureTaken;

        private readonly List<string> _fileList = new List<string>();

        public TakePicturesScreen(string backgroundImgEnSrc, ScreenConfig sc)
            : base(backgroundImgEnSrc, backgroundImgEnSrc, sc)
        {
            _countDownTimer = new Timer {Interval = 1000};
            //1 sec
            _countDownTimer.Tick += OnCountDownTimerEventt;

            _creatingTimer = new Timer {Interval = 1000};
            //1 sec
            _creatingTimer.Tick += OnCreatingTimerEventt;

            //PictureBox.Paint += PaintEvent;

            _1Bitmap = GetNoneLanguageBitmap("1b.png");
            _2Bitmap = GetNoneLanguageBitmap("2b.png");
            _3Bitmap = GetNoneLanguageBitmap("3b.png");
            _4Bitmap = GetNoneLanguageBitmap("4b.png");
            _5Bitmap = GetNoneLanguageBitmap("5b.png");

            _creating1 = GetBitmap("creating_1.png");
            _creating2 = GetBitmap("creating_2.png");
            _creating3 = GetBitmap("creating_3.png");

            _freezeBitmap = GetNoneLanguageBitmap("freeze.png");
            _flashBitmap = GetNoneLanguageBitmap("flash.png");

            _panelPreview = new PictureBox
            {
                Location = new Point(608, 87),
                Size = new Size(705, 705),
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

        //    var p = new Point((1920 - 120)/2, 900);

        //    if (_countDown == 5)
        //    {
        //        if (_5Bitmap != null)
        //        {
        //            e.Graphics.DrawImageUnscaled(_5Bitmap, p);
        //        }
        //    }
        //    else if (_countDown == 4)
        //    {
        //        if (_4Bitmap != null)
        //        {
        //            e.Graphics.DrawImageUnscaled(_4Bitmap, p);
        //        }
        //    }
        //    else if (_countDown == 3)
        //    {
        //        if (_3Bitmap != null)
        //        {
        //            e.Graphics.DrawImageUnscaled(_3Bitmap, p);
        //        }
        //    }
        //    else if (_countDown == 2)
        //    {
        //        if (_2Bitmap != null)
        //        {
        //            e.Graphics.DrawImageUnscaled(_2Bitmap, p);
        //        }
        //    }
        //    else if (_countDown == 1)
        //    {
        //        if (_1Bitmap != null)
        //        {
        //            e.Graphics.DrawImageUnscaled(_1Bitmap, p);
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
        //            e.Graphics.DrawImageUnscaled(_flashBitmap, new Point((1920 - 75)/2, 900));
        //        }
        //    }
        //    else if (_countDown == -1 && _creatingCount == 0)
        //    {
        //        if (_flashBitmap != null)
        //        {
        //            e.Graphics.DrawImageUnscaled(_flashBitmap, new Point((1920 - 75) / 2, 900));
        //        }
        //    }
        //    else if (_creatingCount == 1)
        //    {
        //        if (_creating1 != null)
        //        {
        //            e.Graphics.DrawImageUnscaled(_creating1, new Point((1920 - 203) / 2, 900));
        //        }
        //    }
        //    else if (_creatingCount == 2)
        //    {
        //        if (_creating2 != null)
        //        {
        //            e.Graphics.DrawImageUnscaled(_creating2, new Point((1920 - 203) / 2, 900));
        //        }
        //    }
        //    else if (_creatingCount == 3)
        //    {
        //        if (_creating3 != null)
        //        {
        //            e.Graphics.DrawImageUnscaled(_creating3, new Point((1920 - 203) / 2, 900));
        //        }
        //    }
        //}

        public override void SetActivated()
        {
            base.SetActivated();

            var canonManager = CanonCameraManager.GetInstance();

            if (!canonManager.IsCameraConnected())
            {
                    MessageBox.Show(this, @"La caméra n'est pas connectée.", @"iBrand (tm)", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
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
                if (_screenConfig.QuestionFirstPictureNo != null)
                {
                    var question = questionnaire.GetQuestion(_screenConfig.QuestionFirstPictureNo.Value);
                    var choiceText = (ChoiceText)question.GetChoice(1);

                    var pictureName = GenerateUniqueId.GetId() + ".jpg";

                    var outputFilePath = GetTerminalDirectory(
                        DirectoryUtil.ProjectDataDirectoriesEnum.pictures, DirectoryUtil.ProjectDataDirectoriesModEnum.none) + pictureName;

                    var image = Image.FromFile(_fileList[0]);

                    var b = image.ResizeForView(1024, 1024);

                    b.Save(outputFilePath, ImageFormat.Jpeg);
                    b.Dispose();

//                    var answer = new Answer(QuestionTypeEnum.Photo, question.No, choiceText.No)
//                    {
//                        Response = outputFilePath
//                    };
//                    choiceText.SetAnswer(answer);
                }
            }

            CreateAnimatedGif();

            CreatePictureToPrint();

            _creatingTimer.Stop();

            CanonCameraManager.GetInstance().StartLiveView();

            Next();
        }

        private void CreateAnimatedGif()
        {
            var pictureName = GenerateUniqueId.GetId() + ".gif";

            var outputFilePath = GetTerminalDirectory(
                    DirectoryUtil.ProjectDataDirectoriesEnum.pictures, DirectoryUtil.ProjectDataDirectoriesModEnum.gif) + pictureName;

            var a = new AnimatedGifEncoder();
            a.Start(outputFilePath);
            a.SetDelay(350);
            a.SetRepeat(0);
            
            // Pour transformation
            var questionnaire = ScreensControllerAccessor.ScreensController.Questionnaire;
            var question = questionnaire.GetQuestion(1);
            var liste = new List<Answer>();
            question.GetAnswers(liste);
            var noBkg = -1;//liste[0].SubChoiceNoInt;
            string backFile = null;
            switch (noBkg) {
                case 1: backFile = "vue_helico.png"; break;
                case 2: backFile = "village.png"; break;
                case 3: backFile = "ciment.png"; break;
                case 4: backFile = "camion.png"; break;
            }
            var bkg = GetTerminalDirectory() + backFile;
            var bkgImage = new Bitmap(bkg);
            var bkgImageRes = bkgImage.Resize(512, 512);
            bkgImage.Dispose();

            Bitmap firstPic = null;

            foreach (var fileName in _fileList)
            {
                var image = Image.FromFile(fileName);

                var b = image.Resize(bkgImageRes.Width / 2, bkgImageRes.Height / 2);

                // Transformation par frame. On ne peut pas utiliser le système de transformation conventionnel.
                var composite = new Bitmap(bkgImageRes);
                var greenFilter = new ChromaKeyHslFilter();
                greenFilter.SetParameters(new Dictionary<string, float>());
                var bGreen = greenFilter.Filter(b);
                var desatFilter = new GreyScaleFilter();
                var bDesaturated = desatFilter.Filter(bGreen);
                composite.AddOverlay(bDesaturated, 0, bkgImageRes.Height / 2);

                a.AddFrame(composite);

                if (firstPic == null)
                {
                    firstPic = new Bitmap(composite);
                }

                b.Dispose();
                composite.Dispose();
                bGreen.Dispose();
                bDesaturated.Dispose();
            }

            bkgImage.Dispose();
            a.Finish();

            if (_screenConfig.QuestionGifNo != null)
                question = questionnaire.GetQuestion(_screenConfig.QuestionGifNo.Value);
            var choiceText = (ChoiceText)question.GetChoice(1);

//            var answer = new Answer(QuestionTypeEnum.Photo, question.No, choiceText.No) {Response = outputFilePath};
//            choiceText.SetAnswer(answer);

            // On sauvegarde maintenant la première image modifiée dans la question de première image.
            if (_screenConfig.QuestionFirstPictureNo.HasValue)
            {
                question = questionnaire.GetQuestion(_screenConfig.QuestionFirstPictureNo.Value);
                choiceText = (ChoiceText)question.GetChoice(1);

                var firstPicFileName = GetTerminalDirectory(
                    DirectoryUtil.ProjectDataDirectoriesEnum.pictures, DirectoryUtil.ProjectDataDirectoriesModEnum.transformed) + GenerateUniqueId.GetId() + ".jpg";
                firstPic?.Save(firstPicFileName);

                //choiceText.SetAnswer(new Answer(QuestionTypeEnum.Photo, question.No, choiceText.No, firstPicFileName));
            }

            firstPic?.Dispose();
        }

        private void CreatePictureToPrint()
        {
            if (!_screenConfig.QuestionPictureToPrintNo.HasValue) return;

            var pictureName = GenerateUniqueId.GetId() + ".jpg";

            var outputFilePath = GetTerminalDirectory(
                DirectoryUtil.ProjectDataDirectoriesEnum.pictures, DirectoryUtil.ProjectDataDirectoriesModEnum.print) + pictureName;

            var finalImage = new Bitmap(GetTerminalDirectory() + "printout.jpg");
            finalImage.SetResolution(72f, 72f);

            var g = Graphics.FromImage(finalImage);

            var count = 0;
            var row = 0;
            var mod = 0;

            const int space = 15;
            const int offset = 50;

            var imageSize = (finalImage.Width - (2 * offset) - space) / 2;

            // Pour transformation
            var questionnaire = ScreensControllerAccessor.ScreensController.Questionnaire;
            var question = questionnaire.GetQuestion(1);
            var liste = new List<Answer>();
            question.GetAnswers(liste);
            var noBkg = -1;//liste[0].SubChoiceNoInt;
            string backFile = null;
            switch (noBkg)
            {
                case 1: backFile = "vue_helico.png"; break;
                case 2: backFile = "village.png"; break;
                case 3: backFile = "ciment.png"; break;
                case 4: backFile = "camion.png"; break;
            }
            var bkg = GetTerminalDirectory() + backFile;
            var bkgImage = new Bitmap(bkg);
            var bkgImageRes = bkgImage.Resize(imageSize, imageSize);
            bkgImage.Dispose();

            foreach (var fileName in _fileList)
            {
                var image = FileUtil.LoadBitmap(fileName);

                var b = image.Resize(imageSize / 2, imageSize / 2);

                // Transformation par frame. On ne peut pas utiliser le système de transformation conventionnel.
                var composite = new Bitmap(bkgImageRes);
                var greenFilter = new ChromaKeyHslFilter();
                greenFilter.SetParameters(new Dictionary<string, float>());
                var bGreen = greenFilter.Filter(b);
                var desatFilter = new GreyScaleFilter();
                var bDesaturated = desatFilter.Filter(bGreen);
                composite.AddOverlay(bDesaturated, 0, imageSize / 2);
                composite.SetResolution(finalImage.HorizontalResolution, finalImage.VerticalResolution);

                g.DrawImage(composite, new Point(offset + ((mod * composite.Width) + (mod * space)), ((int)(offset * 1.5f)) + ((row * composite.Height) + (row * space))));

                b.Dispose();

                count++;

                mod = (count % 2);

                if (mod == 0)
                    row++;
            }

            g.Dispose();

            finalImage.Save(outputFilePath, ImageFormat.Jpeg);
            finalImage.Dispose();

            questionnaire = ScreensControllerAccessor.ScreensController.Questionnaire;
            question = questionnaire.GetQuestion(_screenConfig.QuestionPictureToPrintNo.Value);
            var choiceText = (ChoiceText)question.GetChoice(1);

//            var answer = new Answer(QuestionTypeEnum.Photo, question.No, choiceText.No) {Response = outputFilePath};
//            choiceText.SetAnswer(answer);
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
                if (IsDisposed || !_activated) return;

                _fileList.Add(e.FilePath);
                _numberOfPictureTaken++;

                UpdatePicture(e.GetImage(), true);

                if (_numberOfPictureTaken < 4)
                    CanonCameraManager.GetInstance().StartLiveView();

                Refresh();
                _countDownTimer.Start();
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
                if (_countDown != 0 || force)
                {
                    var finalBitmap = image.ResizeForView(_panelPreview.Width, _panelPreview.Height);
                    finalBitmap.RotateFlip(RotateFlipType.RotateNoneFlipX);
                    _panelPreview.Image = finalBitmap;
                }
            }
        }
    }
}
