using System;
using System.Drawing;
using System.Windows.Forms;
using NomadLogic.iBrandDesktop.Util;

namespace NomadLogic.iBrandDesktop.Ui
{
    public partial class LogoControl : UserControl
    {
        public event EventHandler OnLogoClick;

        Bitmap _logo;

        public LogoControl(Bitmap logo)
        {
            InitializeComponent();

            Size = WorkingArea.Size;

            _logo = logo;
            pictureBox.Click += OnClick;
            pictureBox.Image = logo;
            pictureBox.Width = logo.Width;
            pictureBox.Height = logo.Height;
            pictureBox.Size = WorkingArea.Size;
            pictureBox.SizeMode = PictureBoxSizeMode.CenterImage;
        }

        private void OnClick(object sender, EventArgs e)
        {
            if (OnLogoClick != null)
                OnLogoClick(this, EventArgs.Empty);
        }
    }
}
