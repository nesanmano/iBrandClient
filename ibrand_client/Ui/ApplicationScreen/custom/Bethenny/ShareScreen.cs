using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Print;
using NomadLogic.iBrandDesktop.Ui.Choice;
using NomadLogic.iBrandDesktop.Util;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.custom.Bethenny
{
    public partial class ShareScreen : AbstractScreen
    {
        [DllImport("user32.dll")]
        private static extern bool EnableWindow(IntPtr hWnd, bool enable);

        private delegate void UiDelegate();

        readonly Rectangle _nextArea;
        readonly PictureBox _facebookButton;
        readonly PictureBox _twitterButton;
        readonly PictureBox _picture;

        string _photoName;

        readonly Bitmap _facebookImage;
        readonly Bitmap _twitterImage;
        readonly Bitmap _printImage;
        readonly Bitmap _doneImage;

        readonly Bitmap _facebookImageGrey;
        readonly Bitmap _twitterImageGrey;
        readonly Bitmap _printImageGrey;
        readonly Bitmap _doneImageGrey;

        readonly PictureBox _doneButton;
        readonly PictureBox _printButton;
        //PictureBox _uploading_button = null;
        
        
        public ShareScreen(string backgroundImgEnSrc)
            : base(backgroundImgEnSrc, backgroundImgEnSrc)
        {
            PictureBox.MouseDown += MouseDownHandler;

            IsSaveOnThisPage = true;

            _picture = new PictureBox();
            _picture.Location = new Point(435, 39);
            _picture.Size = new Size(714, 714);
            _picture.Visible = true;
            Controls.Add(_picture);
            _picture.BringToFront();

            _facebookImage = GetNoneLanguageBitmap("fb.color.png");
            _twitterImage = GetNoneLanguageBitmap("tw.color.png");
            _printImage = GetNoneLanguageBitmap("print.color.png");
            

            _facebookImageGrey = GetNoneLanguageBitmap("fb.grey.png");
            _twitterImageGrey = GetNoneLanguageBitmap("tw.grey.png");
            _doneImageGrey = GetNoneLanguageBitmap("done.grey.png");
            _printImageGrey = GetNoneLanguageBitmap("print.grey.png");

            var facebookLocation = new Point(1377, 267);
            var facebookSize = new Size(383, 167);

            var twitterLocation = new Point(1377, 478);
            var twitterSize = new Size(383, 167);

            _facebookButton = new PictureBox();
            _facebookButton.Location = facebookLocation;
            _facebookButton.Size = facebookSize;
            _facebookButton.Visible = true;
            _facebookButton.Image = _facebookImage;
            _facebookButton.MouseDown += MouseDownHandler;
            Controls.Add(_facebookButton);
            _facebookButton.BringToFront();

            _twitterButton = new PictureBox();
            _twitterButton.Location = twitterLocation;
            _twitterButton.Size = twitterSize;
            _twitterButton.Visible = true;
            _twitterButton.Image = _twitterImage;
            _twitterButton.MouseDown += MouseDownHandler;
            Controls.Add(_twitterButton);
            _twitterButton.BringToFront();

            _doneImage = GetNoneLanguageBitmap("done.png");
            _doneButton = new PictureBox();
            _doneButton.Location = new Point(798, 784);
            _doneButton.Size = new Size(364, 131);
            _doneButton.Image = _doneImage;
            _doneButton.Visible = true;
            _doneButton.MouseDown += MouseDownHandler;
            Controls.Add(_doneButton);
            _doneButton.BringToFront();

            /*Bitmap uploading = GetNoneLanguageBitmap("uploading.png");
            _uploading_button = new PictureBox();
            _uploading_button.Location = new Point(798, 784);
            _uploading_button.Size = new Size(364, 131);
            _uploading_button.Image = uploading;
            _uploading_button.Visible = false;
            this.Controls.Add(_uploading_button);
            _uploading_button.BringToFront();*/

            _printButton = new PictureBox();
            _printButton.Location = new Point(423, 784);
            _printButton.Size = new Size(364, 131);
            _printButton.Image = _printImage;
            _printButton.Visible = true;
            _printButton.MouseDown += MouseDownHandler;
            Controls.Add(_printButton);
            _printButton.BringToFront();

            _nextArea = new Rectangle(new Point(1052, 897), new Size(181, 90));
        }

        public override void Recycle()
        {
            base.Recycle();

            if (_picture.Image != null)
                _picture.Image.Dispose();

            _photoName = null;

            _facebookButton.Image = _facebookImage;
            _facebookButton.Enabled = true;

            _twitterButton.Image = _twitterImage;
            _twitterButton.Enabled = true;

            _printButton.Image = _printImage;
            _printButton.Enabled = true;

            _doneButton.Visible = true;

            //_uploading_button.Visible = false;

        }

        public new void MouseDownHandler(object sender, MouseEventArgs e)
        {

            if (InArea(e.X, e.Y, _nextArea))
            {
                Next();
            }
            else if (sender.Equals(_doneButton))
            {
                Next();
            }
            else if (sender.Equals(_facebookButton))
            {
                // facebook
                SuspendTimer();
                if (_photoName != null && File.Exists(_photoName))
                {
                    /*MainFormUtil.GetInstance().MainForm.BeginInvoke(new Action(() => EnableWindow(MainFormUtil.GetInstance().MainForm.Handle, false)));

                    UploadPictureToFacebookBethenny upload_picture = new UploadPictureToFacebookBethenny(photo_name);
                    upload_picture.OnUploadFinished += OnUploadFinishedFacebook;

                    _facebook_button.Enabled = false;

                    Thread t = new Thread(upload_picture.DoWork);
                    t.SetApartmentState(ApartmentState.STA);
                    t.Start();

                    _done_button.Image = doneImageGrey;*/
                }
            }
            else if (sender.Equals(_twitterButton))
            {
                // twitter
                SuspendTimer();
                if (_photoName != null && File.Exists(_photoName))
                {
                    MainFormUtil.GetInstance().MainForm.BeginInvoke(new Action(() => EnableWindow(MainFormUtil.GetInstance().MainForm.Handle, false)));

                    /*
                    UploadPictureToTwitterLeNomad uploadPictureTwitter = new UploadPictureToTwitterLeNomad(_screen_config);
                    uploadPictureTwitter.FileName = _photoName;
                    uploadPictureTwitter.OnUploadFinished += OnUploadFinishedTwitter;
                    
                    _twitterButton.Enabled = false;

                    //DeleteCookies();

                    Thread t = new Thread(uploadPictureTwitter.DoWork);
                    t.SetApartmentState(ApartmentState.STA);
                    t.Start();
                    */

                    /*_uploading_button.Visible = true;
                    _uploading_button.BringToFront();*/
                    _doneButton.Image = _doneImageGrey;
                }
            }
            else if (sender.Equals(_printButton))
            {
                SuspendTimer();
                
                var frame = GetNoneLanguageBitmap("frame.png");

                var finalImage = new Bitmap(frame.Width, frame.Height);
                var g = Graphics.FromImage(finalImage);
                g.DrawImage(frame, new Point(0, 0));
                g.DrawImage(Image.FromFile(_photoName), new Point(35, 32));

                g.Dispose();

                frame.Dispose();

                var fileName = //abstractConfiguration.TransformedPicturesDir + 
                    StringUtil.GetFileName(_photoName);

                finalImage.Save(fileName, ImageFormat.Jpeg);
                finalImage.Dispose();

                PrintUtil.Print(fileName, null, this);

                _printButton.Image = _printImageGrey;
                _printButton.Enabled = false;

                ResumeTimer();
            }
        }

        /*private void DeleteCookies()
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.CreateNoWindow = false;
            startInfo.UseShellExecute = false;
            startInfo.FileName = "rundll32.exe";
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.Arguments = "InetCpl.cpl,ClearMyTracksByProcess 10";

            try
            {
                // Start the process with the info we specified.
                // Call WaitForExit and then the using statement will close.
                using (Process exeProcess = Process.Start(startInfo))
                {
                    exeProcess.WaitForExit();
                }
            }
            catch
            {
                // Log error.
            }
        }*/

        private void OnUploadFinishedFacebook(bool success)
        {
            ResumeTimer();
            if (!success)
            {
                EnableFacebookButton();
            }
            else
            {
                DisableFacebookButton();
            }

            MainFormUtil.GetInstance().MainForm.BeginInvoke(new Action(() => EnableWindow(MainFormUtil.GetInstance().MainForm.Handle, true)));
            SingleApplication.SwitchToCurrentInstance();
        }

        protected override void OnTimerEvent(object sender, EventArgs e)
        {
            Next();
        }

        private void EnableFacebookButton()
        {
            if (_facebookButton.InvokeRequired)
            {
                // we were called on a worker thread
                // marshal the call to the user interface thread
                Invoke(new UiDelegate(EnableFacebookButton));
                return;
            }

            _facebookButton.Image = _facebookImage;
            _facebookButton.Enabled = true;
            _doneButton.Image = _doneImage;
            //_uploading_button.Visible = false;
            Refresh();
        }

        private void DisableFacebookButton()
        {
            if (_facebookButton.InvokeRequired)
            {
                // we were called on a worker thread
                // marshal the call to the user interface thread
                Invoke(new UiDelegate(DisableFacebookButton));
                return;
            }

            _facebookButton.Image = _facebookImageGrey;
            _facebookButton.Enabled = false;
            //_uploading_button.Visible = false;
            _doneButton.Image = _doneImage;
            Refresh();
        }

        private void ShowDoneButton()
        {
            if (InvokeRequired)
            {
                // we were called on a worker thread
                // marshal the call to the user interface thread
                Invoke(new UiDelegate(ShowDoneButton));
                return;
            }

            //_uploading_button.Visible = false;
            _doneButton.Image = _doneImage;
            _doneButton.Visible = true;
            _doneButton.BringToFront();
            _doneButton.Invalidate();
            Refresh();
        }

        private void OnUploadFinishedTwitter(bool success)
        {
            ResumeTimer();
            //Sync respondent
            if (!success)
            {
                EnableTwitterButton();
            }
            else
            {
                DisableTwitterButton();
            }

            MainFormUtil.GetInstance().MainForm.BeginInvoke(new Action(() => EnableWindow(MainFormUtil.GetInstance().MainForm.Handle, true)));
            SingleApplication.SwitchToCurrentInstance();
        }

        private void EnableTwitterButton()
        {
            if (_twitterButton.InvokeRequired)
            {
                // we were called on a worker thread
                // marshal the call to the user interface thread
                Invoke(new UiDelegate(EnableTwitterButton));
                return;
            }

            _twitterButton.Image = _twitterImage;
            _twitterButton.Enabled = true;
            //_uploading_button.Visible = false;
            _doneButton.Image = _doneImage;
            Refresh();
        }

        private void DisableTwitterButton()
        {
            if (_twitterButton.InvokeRequired)
            {
                // we were called on a worker thread
                // marshal the call to the user interface thread
                Invoke(new UiDelegate(DisableTwitterButton));
                return;
            }

            _twitterButton.Image = _twitterImageGrey;
            _twitterButton.Enabled = false;

            //_uploading_button.Visible = false;
            _doneButton.Image = _doneImage;
            Refresh();
        }

        public override void SetActivated()
        {
            base.SetActivated();

            var questionnaire = ScreensControllerAccessor.ScreensController.Questionnaire;
            var question = questionnaire.GetQuestion(18);
            var choiceText = (ChoiceText)question.GetChoice(1);
            
            _photoName = //abstractConfiguration.PicturesDir + 
                choiceText.GetAnswerText();

            _picture.Image = Image.FromFile(_photoName);
            _picture.SizeMode = PictureBoxSizeMode.StretchImage;
        }
    }
}
