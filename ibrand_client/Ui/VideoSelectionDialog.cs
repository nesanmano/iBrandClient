using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using NomadLogic.Common.Extentions;
using NomadLogic.Common.Services;
using NomadLogic.Common.Services.Configuration;
using NomadLogic.Common.Util;
using NomadLogic.Multimedia.Cli.Ffmpeg;

namespace NomadLogic.iBrandDesktop.Ui
{
    public partial class VideoSelectionDialog : Form
    {
        private List<string> _videoList;

        private int _index;

        public VideoSelectionDialog(string dir)
        {
            InitializeComponent();

            ServicesContainer.GetService<NomadConfiguration>();

            Init(dir);

            ShowFile();

            previous_button.Click += OnPreviousButtonClick;
            next_button.Click += OnNextButtonClick;
            select_button.Click += OnSelectButtonClick;
            select_button.TabIndex = 0;
        }

        private void ShowFile()
        {
            if (_videoList.Count <= 0) return;
            if (_index >= _videoList.Count) return;
            var fileName = _videoList[_index];

            var thumbnailFileName = fileName.Substring(0, fileName.LastIndexOf(".", StringComparison.InvariantCulture)) + ".jpg";

            thumbnailFileName = DirectoryUtil.GetLocalMediaDir(DirectoryUtil.NomadLogicMediaDirectoriesEnum.videos) + StringUtil.GetFileName(thumbnailFileName);

            if (!File.Exists(thumbnailFileName))
            {
                //TDB trouver le nombre de secondes ou generer le thumbnail automatiquement FSF
                new FfmpegThumbnailsGenerator().GenerateThumbnail(fileName, thumbnailFileName, 1);
            }

            if (!File.Exists(thumbnailFileName)) return;
            var stream = new FileStream(thumbnailFileName, FileMode.Open);
            var picture = new Bitmap(stream);
            stream.Close();

            if (picture.Width > 720)
            {
                var resizeImage = picture.Resize(720, picture.Height * 720 / picture.Width);
                picture.Dispose();

                picture = resizeImage;
            }

            picture_box.Size = picture.Size;
            picture_box.Image = picture;
            picture_box.Location = new Point(0, 0);

            buttons_panel.Location = new Point(0, picture_box.Height + 5);
            buttons_panel.Size = new Size(picture_box.Width, buttons_panel.Height);

            SetButtonsPositions();

            Size = new Size(picture_box.Width, picture_box.Height + buttons_panel.Height + SystemInformation.CaptionHeight + 5);
        }

        private void SetButtonsPositions()
        {
            var x = (buttons_panel.Width / 2) - (select_button.Width / 2);

            select_button.Location = new Point(x, 3);
            previous_button.Location = new Point(x - previous_button.Width - 3, 3);
            next_button.Location = new Point(x + select_button.Width + 3, 3);

        }

        public void Init(string dir)
        {
            _videoList = new List<string>();
            
            var info = new DirectoryInfo(dir);
            var files = info.GetFiles("*.MOV").OrderByDescending(p => p.CreationTime).ToArray();
            var count = 0;
            foreach (var t in files)
            {
                _videoList.Add(t.FullName);
                count++;

                if (count == 10)
                    break;
            }

            if (files.Length == 0)
                select_button.Enabled = false;

        }

        public void OnPreviousButtonClick(object sender, EventArgs args)
        {
            if (_index >= _videoList.Count) return;
            _index++;
            ShowFile();
        }

        public void OnNextButtonClick(object sender, EventArgs args)
        {
            if (_index <= 0) return;
            _index--;
            ShowFile();
        }

        public string SelectedFile => _videoList[_index];

        public void OnSelectButtonClick(object sender, EventArgs args)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

    }
}
