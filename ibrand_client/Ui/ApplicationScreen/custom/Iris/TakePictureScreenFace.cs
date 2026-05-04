using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using Canon.Eos.Framework;
using Canon.Eos.Framework.Eventing;
using NLog;
using NomadLogic.Common.Extentions;
using NomadLogic.Common.Util;
using NomadLogic.iBrandDesktop.Util;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.custom.Iris
{
    public class TakePictureScreenFace : AbstractScreen
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private readonly Timer _countDownTimer;

        private readonly Bitmap[] _bitmaps;
        private readonly int _countNumber;

        private Bitmap _freezeBitmap;

        private readonly PictureBox _panelPreview;

        private readonly Bitmap _flashBitmap;
        private readonly Bitmap _explanation;

        private int _countDown = -1;

        public TakePictureScreenFace(string backgroundImgEnSrc, ScreenConfig sc, int count, string explanationPicture)
            : base(backgroundImgEnSrc, backgroundImgEnSrc, sc)
        {
            //PictureBox.MouseDown += MouseDownHandler;
            
            _countDownTimer = new Timer();
            _countDownTimer.Interval = 1000; //1 sec
            _countDownTimer.Tick += OnCountDownTimerEventt;

            //PictureBox.Paint += PaintEvent;

            _countNumber = count;
            _bitmaps = new Bitmap[_countNumber];
            for (var i = 1; i <= _countNumber; i++)
            {
                _bitmaps[i-1] = GetNoneLanguageBitmap(i + ".png");
            }

            _explanation = GetNoneLanguageBitmap(explanationPicture);
            _freezeBitmap = GetBitmap("freeze.png");
            _flashBitmap = GetNoneLanguageBitmap("flash.png");

            _panelPreview = new PictureBox();
            _panelPreview.Location = new Point((2160-705)/2, 100);
            _panelPreview.Size = new Size(705, 705);
            _panelPreview.BackColor = Color.White;

            Controls.Add(_panelPreview);

            var canonManager = CanonCameraManager.GetInstance();
            canonManager.LiveViewUpdate += HandlePictureUpdate;
            canonManager.FileCreatedOnCamera += OnFileCreatedOnCamera;
            canonManager.FileSavedOnHost += OnFileSavedOnHost;
            
            _panelPreview.BringToFront();

        }

        //public new void PaintEvent(object sender, PaintEventArgs e)
        //{
        //    base.PaintEvent(sender, e);

        //    var point = new Point(1800, 200);
            
        //    if (_countDown > 0)
        //    {
        //        if (_bitmaps[_countDown - 1] != null)
        //        {
        //            e.Graphics.DrawImageUnscaled(_bitmaps[_countDown - 1], point);
        //        }
        //    }
        //    else if (_countDown == 0)
        //    {
        //        if (_freezeBitmap != null)
        //        {
        //            CanonCameraManager.GetInstance().StopLiveView();

        //            try { 
        //                _panelPreview.Image = _freezeBitmap;
        //                _panelPreview.Refresh();
        //            }
        //            catch (System.Exception ex)
        //            {
        //                log.Error("Could not refresh preview. :" + ex.Message);
        //            }
        //        }

        //        if (_flashBitmap != null)
        //        {
        //            e.Graphics.DrawImageUnscaled(_flashBitmap, point);
        //        }
        //    }
        //    else if (_countDown == -1)
        //    {
        //        if (_flashBitmap != null)
        //        {
        //            e.Graphics.DrawImageUnscaled(_flashBitmap, point);
        //        }
        //    }
           
        //    if (_explanation != null)
        //    {
        //        e.Graphics.DrawImageUnscaled(_explanation, new Point(100, 100));
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

            canonManager.StartLiveView();

            _countDown = _countNumber;

            _countDownTimer.Start();

            _panelPreview.Image = null;
            
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
                    _countDown = _countNumber;
                    _countDownTimer.Start();
                }
            }
        }

        private void OnFileCreatedOnCamera(object sender, IntPtr dirItem, EosCamera camera)
        {
            if (_activated)
            {
                camera.SavePicture(dirItem, DirectoryUtil.GetLocalMediaDir(DirectoryUtil.NomadLogicMediaDirectoriesEnum.pictures) + GenerateUniqueId.GetId() + ".jpg");
                //log.Debug("File created on camera.");
            }
        }

        private void HandlePictureUpdate(object sender, EosImageEventArgs e)
        {
            if (!IsDisposed && _activated)
                UpdatePicture(e.GetImage(), false);
        }

        protected virtual void OnFileSavedOnHost(object sender, EosFileEventArgs e)
        {
            if (_activated)
            {
                SaveFileFromCameraToQuestion(e, 1);
                //log.Debug("File saved to disk from camera.");
            }
        }

        protected void SaveFileFromCameraToQuestion(EosFileEventArgs e, int questionNo)
        {
            try
            {
                if (_activated)
                {
                    var tmp = FileUtil.LoadBitmap(e.FilePath);

                    var cropped = tmp.ResizeToSquare();

                    var bmp = new Bitmap(700, 700);
                    bmp.SetResolution(cropped.HorizontalResolution, cropped.VerticalResolution);
                    var g = Graphics.FromImage(bmp);
                    var destRect = new RectangleF(0, 0, 700, 700);
                    g.DrawImage(cropped, destRect);

                    var modifiedFile = GetTerminalDirectoryDataFile(GenerateUniqueId.GetId() + ".png", DirectoryUtil.ProjectDataDirectoriesEnum.pictures);

                    bmp.Save(ImageFormat.Png, 9, modifiedFile);

                    cropped.Dispose();
                    bmp.Dispose();
                    tmp.Dispose();

                    //if (SaveAnswerToQuestion(questionNo, 1, modifiedFile))
                    //{
                        Next();
                    //}
                    //else
                    {
                        log.Error("Could not save data to question " + questionNo);
                        End();
                    }
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
                _panelPreview.Image = image.ResizeForView(_panelPreview.Size.Width, _panelPreview.Size.Height);
            }
        }

        public override bool ChangeLanguage(string language)
        {
            var change = base.ChangeLanguage(language);
            if (!change) return false;
            _freezeBitmap?.Dispose();
            _freezeBitmap = GetBitmap("freeze.png");
            return true;
        }
    }
}
