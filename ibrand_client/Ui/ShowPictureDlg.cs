using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using NomadLogic.iBrandDesktop.Util;

namespace NomadLogic.iBrandDesktop.Ui
{
    public partial class ShowPictureDlg : Form
    {
        public ShowPictureDlg(string fileName)
        {
            InitializeComponent();

            var stream = new FileStream(fileName, FileMode.Open);
            var picture = new Bitmap(stream);
            stream.Close();

            pictureBox.Image = picture;
            //this.pictureBox.Width = picture.Width;
            //this.pictureBox.Height = picture.Height;
            pictureBox.SizeMode = PictureBoxSizeMode.CenterImage;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (WorkingArea.Container == null) return;
            StartPosition = FormStartPosition.Manual;
            var p = PointToScreen(WorkingArea.Container.Location);
            var x = WorkingArea.Container.Location.X + WorkingArea.Container.Width / 2 - Size.Width / 2;
            var y = WorkingArea.Container.Location.Y + (WorkingArea.Container.Height - Height) / 2;
            Location = new Point(x, y);
        }

        
        private void cancelButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Dispose();
        }

        private void selectButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Dispose();
        }


    }
}
