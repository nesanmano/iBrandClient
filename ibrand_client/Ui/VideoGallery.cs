using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using NomadLogic.iBrandDesktop.Ui.Choice;

namespace NomadLogic.iBrandDesktop.Ui
{
    public partial class VideoGallery : Form
    {

        private readonly VideoGalleryTextBox _choice;
        private string _galleryPath;

        public VideoGallery(VideoGalleryTextBox choice)
        {
            _choice = choice;
            InitializeComponent();
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.DoubleBuffer, true);
            Resize += VideoGallery_Resize;
            CancelVideoGalleryButton.Click += CancelVideoGalleryButton_Click; 
            PopulateGallery();
            // on met fullscreen
            FormBorderStyle = FormBorderStyle.None;
            TopMost = true;
            WindowState = FormWindowState.Maximized;
            Bounds = Screen.AllScreens[0].Bounds;
            StartPosition = FormStartPosition.Manual;
        }

        void CancelVideoGalleryButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void PopulateGallery()
        {
            if (GalleryPath == null) return;
            var thumnailDir = new DirectoryInfo(GalleryPath);

            GalleryContainer.Controls.Clear();
            var thumbnails = GetFilesByExtensionsOrderByDate(thumnailDir, ".jpg", ".JPG");

            var i = 0;

            foreach (var thumbnail in thumbnails)
            {
                var photo = new PhotoControl(this) {ImageFile = thumbnail.FullName};
                photo.Click += photo_Click;
                GalleryContainer.Controls.Add(photo);
                i++;
                if (i >= 15)
                {
                    break;
                }
            }
        }

        public void photo_Click(object sender, EventArgs e)
        {
            var control = sender as PhotoControl;
            if (control == null) return;
            var photoClicked = control;
            _choice.SetFile(photoClicked.ImageFile, false);
            Close();
        }

        void VideoGallery_Resize(object sender, EventArgs e)
        {
            var control = (Control)sender;

            var containerSize = new Size(control.Size.Width, control.Size.Height - 41);

            GalleryContainer.Size = containerSize;
            CancelVideoGalleryButton.Location = new Point(12, containerSize.Height + 6 );

            var imageWidth = (containerSize.Width - (SystemInformation.VerticalScrollBarWidth*2)) / 3;
            imageWidth = Math.Min(634, imageWidth);
            Console.WriteLine(imageWidth);
            var imageHeight = imageWidth * 7 / 10;

            foreach (Control image in GalleryContainer.Controls)
            {
                image.Size = new Size(imageWidth, imageHeight);
            }
        }

        public string GalleryPath
        {
            get {return _galleryPath;}
            set 
            { 
                _galleryPath = value;
                PopulateGallery();
            }
        }

        public static IEnumerable<FileInfo> GetFilesByExtensionsOrderByDate(DirectoryInfo dir, params string[] extensions)
        {
            if (extensions == null)
                throw new ArgumentNullException(nameof(extensions));
            var files = dir.EnumerateFiles();
            return files.Where(f => extensions.Contains(f.Extension)).OrderByDescending(f => f.CreationTime);
        }


    }
}
