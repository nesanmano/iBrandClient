using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace NomadLogic.iBrandDesktop.Ui
{
    public partial class PhotoControl : UserControl
    {
        private string _imageFilePath;
        readonly VideoGallery _gallery;

        public PhotoControl(VideoGallery gallery)
        {
            _gallery = gallery;
            InitializeComponent();
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.DoubleBuffer, true);
            Resize += PhotoControl_Resize;
            ImageBox.Click += Image_Click;
            ImageName.Click += Image_Click;
        }

        private void Image_Click(object sender, EventArgs e)
        {
            _gallery?.photo_Click(this, e);
        }

        public string ImageFile
        {
            get
            {
                return _imageFilePath;
            }
            set
            {
                _imageFilePath = value;
                if (!File.Exists(value)) return;
                ImageBox.ImageLocation = value;
                var lastIndex = value.LastIndexOf(Path.DirectorySeparatorChar) + 1;
                ImageName.Text = value.Substring(lastIndex, value.Length - lastIndex - 4);
            }
        }

        private void PhotoControl_Resize(object sender, EventArgs e)
        {
            var control = (Control)sender;

            // resize de l'image
            var imageSize = new Size {Width = control.Size.Width - 8};
            imageSize.Height = (int)(imageSize.Width * 9f / 16f);
            ImageBox.Size = imageSize;
            
            // resize du texte
            var textSize = new Size {Width = control.Size.Width - 8};
            var textFont = ImageName.Font;
            var textSizeInPoints = imageSize.Height / 13f;
            textFont = new Font(textFont.FontFamily, textSizeInPoints);
            var g = control.CreateGraphics();
            textSize.Height = (int)textFont.GetHeight(g)+5;
            ImageName.Size = textSize;
            ImageName.Font = textFont;
            ImageName.Location = new Point(3, imageSize.Height + 7);

            // resize du controlleur si la hauteur est pas bonne
            if (control.Size.Height != imageSize.Height + textSize.Height + 11)
            {
                control.Size = new Size(control.Size.Width, imageSize.Height + textSize.Height + 11);
            }
            
        }

    }
}
