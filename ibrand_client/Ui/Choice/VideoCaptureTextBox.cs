using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using NomadLogic.Common;
using NomadLogic.Common.Ui.Dialog;
using NomadLogic.Common.Util;

namespace NomadLogic.iBrandDesktop.Ui.Choice
{

    public partial class VideoCaptureTextBox : UserControl
    {
        private readonly string _videosPath;

        private Choice _choice;

        public VideoCaptureTextBox(Choice choice)
        {
            InitializeComponent();

            _videosPath = DirectoryUtil.GetLocalMediaDir(DirectoryUtil.NomadLogicMediaDirectoriesEnum.videos);

            captureButton.Click += OnCaptureButtonClick;
            //this.facebookButton.Click += OnFacebookButtonClick;

            //this.facebookButton.Enabled = false;

            textBox.Enabled = false;

            _choice = choice;
        }

        public void OnCaptureButtonClick(object sender, EventArgs args)
        {
            var fileName = CreateVideoName();
            //WebCamDlg dialog = new WebCamDlg(file_name);

            /*CaptureDialog dialog = new CaptureDialog(file_name);

            if (dialog.IsCameraFound())
            {
                //dialog.Owner = this.ParentForm;
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    this.textBox.Text = file_name;
                    this.video_to_upload = file_name;
                }
            }
            else
                MsgBox.Show(this, "No Webcam found.", "iBrand (tm)", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);*/

        }

        private string CreateVideoName()
        {
            var videoName = GenerateUniqueId.GetId() + ".avi";
            var newVideoName = _videosPath + videoName;
            if (File.Exists(newVideoName))
            {
                newVideoName = CreateVideoName();
            }
            return newVideoName;
        }

        private void ShowMessageBox(string text, bool error)
        {
            if (InvokeRequired)
            {
                ShowMessageBoxDelegate d = ShowMessageBox;
                Invoke(d, text, error);
            }
            else
            {
                MsgBox.Show(this, text);
            }
        }

        /*private void OnTokenObtain(Boolean success, String token_and_file_name)
        {
            if (this.InvokeRequired)
            {
                // we were called on a worker thread
                // marshal the call to the user interface thread
                OnUploadFinishedDelegate d = new OnUploadFinishedDelegate(OnTokenObtain);
                this.Invoke(d, new object[] { success, token_and_file_name });
                return;
            }

            this.captureButton.Enabled = true;
            this.facebookButton.Enabled = true;

            if (success)
            {
                this.textBox.Text = token_and_file_name;
                //_choice.Question.ToSave = true;
            }
            
        }*/

        

        public override string Text => textBox.Text;

        public void SetSize(Size size)
        {
            Size = size;
            textBox.Width = size.Width - captureButton.Width - 10;

            var initialLocation = captureButton.Location;
            captureButton.Location = new Point(textBox.Width + 5, initialLocation.Y);
        }

        public void Reset()
        {
            textBox.Text = "";
        }

        public new bool Enabled
        {
            get
            {
                return base.Enabled;
            }
            set
            {
                textBox.Enabled = false;
                captureButton.Enabled = value;
                base.Enabled = value;

                textBox.BackColor = Color.LightGray;
            }
        }
    }
}
