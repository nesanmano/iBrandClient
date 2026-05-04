using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using NLog;
using NomadLogic.iBrand.Common;
using NomadLogic.iBrand.Ui.Dialog;
using NomadLogic.iBrand.Util;

namespace NomadLogic.iBrand.Ui.Choice
{
    public partial class CaptureTextBoxNikon : UserControl
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        protected delegate void SetTextDelegate(String text);

        private readonly Boolean _showFacebookButton;

        private bool _pictureTaken;

        //private Question.Question question = null;

        private readonly PictureController _pictureController;

        public CaptureTextBoxNikon(Boolean showFacebookButton/*, Question.Question question*/)
        {
            InitializeComponent();
            //this.question = question;
            _showFacebookButton = showFacebookButton;
            _pictureController = PictureController.GetInstance();
            _pictureController.CaptureWidget = this;

            captureButton.Click += OnCaptureButtonClick;
            previewButton.Click += OnPreviewButtonClick;
            connectButton.Click += OnConnectButtonClick;

        }

        public void StartThread()
        {
            _pictureController.StartLookingForPictures();
        }


        public new void Dispose()
        {
            PictureController.GetInstance().StopLookingForPictures();
        }

        public void OnCaptureButtonClick(object sender, EventArgs args)
        {
            _pictureController.CapturePhoto();
        }

        public void OnFacebookButtonClick(object sender, EventArgs args)
        {
            String fileName = Constants.APP_DIR + textBox.Text;
            if (File.Exists(fileName))
            {
                /*UploadPictureToFacebook upload_picture = new UploadPictureToFacebook(file_name);
                upload_picture.OnUploadFinished += OnUploadFinished;

                Thread t = new Thread(upload_picture.DoWork);
                t.SetApartmentState(ApartmentState.STA);
                t.Start();*/

                //new UploadPictureToTwitter(file_name).DoWork();
            }
        }

        private void OnUploadFinished(Boolean success)
        {
            //Sync respondent
            if (success)
            {
                
            }
        }

        public void OnPreviewButtonClick(object sender, EventArgs args)
        {
            try
            {
                Bitmap picture = _pictureController.GetTransformedPicture(); ;
                new PreviewDialog(picture).ShowDialog();
                picture.Dispose();
            }
            catch (System.Exception ex)
            {
                MsgBox.Show(WorkingArea.Container, "Problem while transforming picture.", "iBrand (tm)", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                log.Fatal(ex.Message);
            }
        }

        public void OnConnectButtonClick(object sender, EventArgs args)
        {
            _pictureController.StartLookingForPictures();
        }

        public void SetTextBoxText(String text)
        {
            if (textBox.InvokeRequired)
            {
                // we were called on a worker thread
                // marshal the call to the user interface thread
                Invoke(new SetTextDelegate(SetTextBoxText), text);
                return;
            }

            textBox.Text = text;
        }

        public void SetSize(Size size)
        {
            Size = size;

            if (_showFacebookButton)
                textBox.Width = size.Width - captureButton.Width - previewButton.Width - connectButton.Width - 15;
            else
                textBox.Width = size.Width - captureButton.Width - previewButton.Width - connectButton.Width - 10;

            Point initialLocation = captureButton.Location;

            captureButton.Location = new Point(textBox.Width + 5, initialLocation.Y);

            previewButton.Location = new Point(textBox.Width + captureButton.Width + 5, initialLocation.Y);

            connectButton.Location = new Point(textBox.Width + captureButton.Width + previewButton.Width + 5, initialLocation.Y);

        }

        public void SetCameraConnected(bool isConnected)
        {
            if (isConnected)
            {
                SetTextBoxText("Camera connected.");
                captureButton.Enabled = true;
            }
            else
            {
                SetTextBoxText("No camera connected.");
                captureButton.Enabled = false;
            }
        }

        public void Reset()
        {
            textBox.Text = "";
        }

        public override String Text
        {
            get
            {
                return textBox.Text;
            }
        }

        public bool PictureTaken
        {
            get
            {
                return _pictureTaken;
            }
            set
            {
                _pictureTaken = value;
            }
        }

        public new bool Enabled
        {
            get
            {
                return base.Enabled;
            }
            set
            {
                //this.textBox.Enabled = false;
                captureButton.Enabled = value;
                base.Enabled = value;

                //this.textBox.BackColor = Color.LightGray;
            }
        }
    }
}
