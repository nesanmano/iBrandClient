using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using NLog;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Util;
using NomadLogic.iBrandDesktop.Ui.Question;

namespace NomadLogic.iBrandDesktop.Util
{
    public class FullScreenImageViewer : Form
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private PictureBox _pictureBox;

        readonly string _path;

        public FullScreenImageViewer(int projetId, int questionnaireId, int terminalId, string backgroundDir)
        {

            if (Screen.AllScreens.Length > 1)
            {
                Bounds = Screen.AllScreens[1].Bounds;
                StartPosition = FormStartPosition.Manual;
            }
            

            FormBorderStyle = FormBorderStyle.None;

            InitializeComponent();

            _path = DirectoryUtil.GetLocalIbrandTerminalCustomDir(projetId, questionnaireId, terminalId, backgroundDir, DirectoryUtil.ProjectDataDirectoriesModEnum.none);
        }

        public void OnAnswerChanged(Question question)
        {
            var list = new List<Answer>();
            question.GetAnswers(list);
            var index = list.FirstOrDefault()?.ChoiceNo ?? -1;
            
            if (index != -1)
                ShowImage(index);
        }

        private void ShowImage(Bitmap image)
        {
            if(_pictureBox.Image != null)
            {
                _pictureBox.Image.Dispose();
                _pictureBox.Image = null;
            }

            try
            {
                _pictureBox.Image = image;
                _pictureBox.Invalidate();
            }
            catch (System.Exception ex)
            {
                var test = ex.Message;
            }
        }

        public void ShowImage(int index)
        {
            var image = GetImage(index);
            if(image != null)
            {
                ShowImage(image);
            }
        }

        private Bitmap GetImage(int index)
        {
            var imagepath = _path + index + ".jpg";
            try
            {
                if (File.Exists(imagepath))
                {
                    var image = new Bitmap(imagepath);
                    return image;
                }
                imagepath = _path + index + ".png";
                if (File.Exists(imagepath))
                {
                    var image = new Bitmap(imagepath);
                    return image;
                }
            }
            catch (System.Exception e)
            {
                log.Error(e, "Problem loading background image.");
            }
            return null;
        }

        private void InitializeComponent()
        {
            _pictureBox = new PictureBox();
            ((ISupportInitialize)(_pictureBox)).BeginInit();
            SuspendLayout();
            // 
            // pictureBox
            // 
            _pictureBox.Dock = DockStyle.Fill;
            _pictureBox.Location = new Point(0, 0);
            _pictureBox.Name = "_pictureBox";
            _pictureBox.Size = new Size(576, 503);
            _pictureBox.SizeMode = PictureBoxSizeMode.StretchImage;
            _pictureBox.TabIndex = 0;
            _pictureBox.TabStop = false;
            // 
            // FullScreenImageViewer
            // 
            BackgroundImageLayout = ImageLayout.Zoom;
            ClientSize = new Size(576, 503);
            Controls.Add(_pictureBox);
            FormBorderStyle = FormBorderStyle.None;
            Name = "FullScreenImageViewer";
            WindowState = FormWindowState.Maximized;
            ((ISupportInitialize)(_pictureBox)).EndInit();
            ResumeLayout(false);

        }

    }
}
