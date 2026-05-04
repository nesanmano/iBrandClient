using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;
using Canon.Eos.Framework;
using Canon.Eos.Framework.Eventing;
using NLog;
using NomadLogic.Common.Services;
using NomadLogic.Common.Services.Configuration;
using NomadLogic.Common.Util;
using NomadLogic.iBrandDesktop.Ui.Choice;
using NomadLogic.iBrandDesktop.Ui.Question;
using NomadLogic.iBrandDesktop.Util;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.custom.HarryPotter
{
    public class RecordScreen2 : AbstractScreen
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        readonly PictureBox _panelPreview;

        private readonly PictureBox _panelThanks;

        private int _timeBeforeRecord = 3; //seconds
        private int _timeToRecord = 30; //seconds

        private readonly Timer _timer;

        private string _videoFileName;

        private readonly List<Bitmap> _recordCountDownList;

        readonly Rectangle _startStopArea;

        readonly Bitmap _recordingImage;
        readonly Bitmap _stopButton;
        readonly Bitmap _startButton;

        readonly Bitmap _question1;
        readonly Bitmap _question2;
        readonly Bitmap _question3;

        int _questionNo = -1;
        bool _isRecording;
        bool _stopEnabled;
        bool _startEnabled = true;
        bool _isStoped;

        string _backUpDeviceName;

        bool _canSave;

        public RecordScreen2(string backgroundImgEnSrc)
            : base(backgroundImgEnSrc, backgroundImgEnSrc)
        {
            //PictureBox.MouseDown += MouseDownHandler;

            _panelPreview = new PictureBox();
            _panelPreview.Location = new Point(450, 155);
            _panelPreview.Size = new Size(400, 400);
            _panelPreview.BackColor = Color.White;

            Controls.Add(_panelPreview);

            _panelPreview.BringToFront();   

            _panelThanks = new PictureBox();
            _panelThanks.Location = new Point(450, 155);
            _panelThanks.Name = "panel_thanks";
            _panelThanks.Size = new Size(400, 400);

            var bitmap = GetNoneLanguageBitmap("thanks.jpg");
            if (bitmap != null) {
                _panelThanks.Image = bitmap;
            }
           
            Controls.Add(_panelThanks);

            _panelThanks.Visible = false;


            _timer = new Timer();
            _timer.Interval = 1000; //1 sec
            _timer.Tick += OnTimerEventt;

            //PictureBox.Paint += PaintEvent;

            _recordCountDownList = new List<Bitmap>(30);
            for (var i = 1; i <= 30; i++)
            {
                _recordCountDownList.Add(GetNoneLanguageBitmap(i + ".png"));
            }

            _startStopArea = new Rectangle(new Point(542, 641), new Size(195, 78));

            _recordingImage = GetNoneLanguageBitmap("recording.png");

            _stopButton = GetNoneLanguageBitmap("stop_button.png");

            _startButton = GetNoneLanguageBitmap("start_button.png");

            _question1 = GetNoneLanguageBitmap("question_1.png");

            _question2 = GetNoneLanguageBitmap("question_2.png");

            _question3 = GetNoneLanguageBitmap("question_3.png");

            var canonManager = CanonCameraManager.GetInstance();
            canonManager.LiveViewUpdate += HandlePictureUpdate;
            canonManager.FileCreatedOnCamera += OnFileCreatedOnCamera;
            canonManager.FileSavedOnHost += OnFileSavedOnHost;
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
                if (!_canSave) return;
                _videoFileName = e.FilePath;
                    
                var questionnaire = ScreensControllerAccessor.ScreensController.Questionnaire;

                var question = questionnaire.GetQuestion(3) as QuestionCombo;
                question?.SetSelectedIndex(_questionNo);

                var question2 = questionnaire.GetQuestion(4);
                var choiceText = (ChoiceText)question2.GetChoice(1);

                //choiceText.SetAnswer(new Answer(4, 4, 1, _videoFileName));

                FinishToDownloadFile();
            }
            catch (System.Exception ex)
            {
                log.Debug(ex.StackTrace);
            }
        }

        private void OnFileCreatedOnCamera(object sender, IntPtr dirItem, EosCamera camera)
        {
            if (_canSave)
            {
                string dir = null;


                dir = _backUpDeviceName ??
                      DirectoryUtil.GetLocalMediaDir(DirectoryUtil.NomadLogicMediaDirectoriesEnum.videos);


                var fileName = dir + GenerateUniqueId.GetId() + ".MOV";
                
                if (File.Exists(fileName))
                    File.Delete(fileName);

                try
                {
                    camera.SavePicture(dirItem, fileName);
                }
                catch (System.Exception ex)
                {
                    log.Error(ex.StackTrace);
                }
            }
        }

        private string GetBackUpDeviceName()
        {
            string deviceName = null;
            var allDrives = DriveInfo.GetDrives();
            DriveInfo backUpDevice = null;

            var abstractConfiguration = ServicesContainer.GetService<NomadConfiguration>();

            foreach (var d in allDrives)
            {
                if (d.IsReady)
                {
                    if (d.VolumeLabel.Contains(DirectoryUtil.GetBackupDir(abstractConfiguration)));
                    {
                        backUpDevice = d;
                        break;
                    }
                }
            }


            if (backUpDevice != null)
            {
                if (backUpDevice.IsReady)
                {
                    if (backUpDevice.AvailableFreeSpace > 200000)
                    {
                        deviceName = backUpDevice.Name;
                    }
                }
            }

            return deviceName;
        }

        private void UpdatePicture(Image image, bool force)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => UpdatePicture(image, force)));
            }
            else
            {

               // if (image.Width != 720)
               //     image = ResizeImage(image, 720, 480);

                var finalBitmap = CropImage(image, _panelPreview.Width, _panelPreview.Height);
                finalBitmap.RotateFlip(RotateFlipType.RotateNoneFlipX);
                _panelPreview.Image = finalBitmap;
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

        private void FinishToDownloadFile()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => FinishToDownloadFile()));
            }
            else
            {

                // if (image.Width != 720)
                //     image = ResizeImage(image, 720, 480);

                Next();
            }
        }

        //protected override void MouseDownHandler(object sender, MouseEventArgs e)
        //{
        //    base.MouseDownHandler(sender, e);

        //    if (InArea(e.X, e.Y, _startStopArea))
        //    {
        //        if (!_stopEnabled && !_isStoped)
        //        {
        //            CanonCameraManager.GetInstance().StartRecord();

        //            _timer.Start();
        //            _startEnabled = false;
        //        }
        //        else
        //        {
        //            if (!_isStoped)
        //            {
                        
        //                _timer.Stop();

        //                _canSave = true;

        //                Next();

        //                StopRecording();

        //                /*_is_recording = false;
        //                _stop_enabled = false;

        //                time_to_record = 30;*/
                        
                        
        //            }
        //        }
        //        Refresh();
        //    }

        //}

        private void StopRecording()
        {
            try
            {
                Cursor.Current = Cursors.WaitCursor;

                CanonCameraManager.GetInstance().StopRecord();

                _isRecording = false;
                _isStoped = true;

                //panel_thanks.Visible = true;

                //panel_thanks.BringToFront();

            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message + "\n\n" + ex);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        //public new void PaintEvent(object sender, PaintEventArgs e)
        //{
        //    base.PaintEvent(sender, e);

        //    if (_timeBeforeRecord != 0)
        //    {
        //        var bitmap = _recordCountDownList[_timeBeforeRecord - 1];
        //        if (bitmap != null)
        //            e.Graphics.DrawImageUnscaled(bitmap, new Point(596, 570));
        //    }

        //    if (_timeToRecord != 0 && _isRecording)
        //    {
        //        if (_recordingImage != null)
        //            e.Graphics.DrawImageUnscaled(_recordingImage, new Point(453, 565));

        //        var bitmap = _recordCountDownList[_timeToRecord - 1];
        //        if (bitmap != null)
        //            e.Graphics.DrawImageUnscaled(bitmap, new Point(740, 567));
        //    }

        //    if (_stopEnabled)
        //    {
        //        if (_stopButton != null)
        //            e.Graphics.DrawImageUnscaled(_stopButton, new Point(546, 646));
        //    }

        //    if (_startEnabled)
        //    {
        //        if (_startButton != null)
        //            e.Graphics.DrawImageUnscaled(_startButton, new Point(546, 646));
        //    }

        //    if (_questionNo == 1)
        //    {
        //        if (_question1 != null)
        //            e.Graphics.DrawImageUnscaled(_question1, new Point(0, 0));
        //    }
        //    else if (_questionNo == 2)
        //    {
        //        if (_question2 != null)
        //            e.Graphics.DrawImageUnscaled(_question2, new Point(0, 0));
        //    }
        //    else if (_questionNo == 3)
        //    {
        //        if (_question3 != null)
        //            e.Graphics.DrawImageUnscaled(_question3, new Point(0, 0));
        //    }

        //}

        public override void Recycle()
        {
            base.Recycle();

            _timer.Stop();

            _videoFileName = null;

            _questionNo = -1;

            _timeBeforeRecord = 3;
            _timeToRecord = 30;

            _isRecording = false;
            _stopEnabled = false;
            _isStoped = false;
            _startEnabled = true;
            _canSave = false;
        }

        public override void SetActivated()
        {
            _activated = true;

            var rnd = new Random();
            _questionNo = rnd.Next(1, 4);

            _panelPreview.BringToFront();
            
            _panelThanks.Visible = false;

            if (_panelPreview.Image != null)
            {
                _panelPreview.Image = null;
            }

            _backUpDeviceName = GetBackUpDeviceName();

            if (_backUpDeviceName == null) 
            { 
                var errorDialog = new ErrorMessageDialog("USB stick not inserted or full.");
                errorDialog.TopMost = true;
                errorDialog.Show();
            }
        }

        public void OnTimerEventt(object sender, EventArgs e)
        {
            if (_timeBeforeRecord != 0)
            {
                _timeBeforeRecord--;

                if (_timeBeforeRecord == 0)
                    _stopEnabled = true;
                Refresh();
            }
            else
            {
                if (_timeToRecord != 0)
                {
                    if (_timeToRecord == 30)
                    {
                        _isRecording = true;

                        Refresh();
                    }

                    _timeToRecord--;
                }
                else
                {
                    _timer.Stop();

                    _canSave = true;

                    Next();

                    StopRecording();

                    _isRecording = false;
                    _stopEnabled = false;
                }

                Refresh();
            }

        }
    }
}
