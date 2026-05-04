using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using NomadLogic.Common.Extentions;
using NomadLogic.Common.Util;

namespace NomadLogic.Common.Ui
{
    /// <summary>
    /// Dialogue de présentation d'une série d'image, à fin de visualisation.
    /// </summary>
    public partial class MultiplePicturesPreviewDlg : Form
    {
        private List<Picture> _pictures;
        private readonly Timer _timer;
        private int _onScreenPictureIndex;
        private readonly HashSet<int> _discarded = new HashSet<int>();
        private readonly Action<IList<string>> _callback;

        /// <summary>
        /// Constructeur principal.
        /// </summary>
        /// <param name="dir">Répertoire source pour afficher les images.</param>
        /// <param name="removeCallback">Méthode de retour dans le cas où certains fichiers doivent être ignorés.</param>
        public MultiplePicturesPreviewDlg(string dir, Action<IList<string>> removeCallback = null)
        {
            InitializeComponent();

            pictureBox.SizeMode = PictureBoxSizeMode.Zoom;

            InitPictureList(dir);

            _callback = removeCallback;

            _timer = new Timer();
            _timer.Tick += OnTimerEvent;

            PoulateIntervalComboBox();

            startButton.Click += OnStartButtonClick;
            stopButton.Click += OnStopButtonClick;

            nextButton.Click += OnNextButtonClick;
            backButton.Click += OnBackButtonClick;

            intervalComboBox.SelectedIndexChanged += OnIntervalChanged;
            stopButton.Enabled = false;
            intervalComboBox.SelectedIndex = -1;

            if (_callback != null) return;
            btnDiscard.Visible = false;
            btnDiscard.Enabled = false;

            SizeChanged += (sender, args) => {
                pictureBox.Width = ClientSize.Width - 200;
                pictureBox.Height = ClientSize.Height - 20;
                Refresh();
            };
        }

        private void InitPictureList(string dir)
        {
            var di = new DirectoryInfo(dir);

            var orderedList = di.EnumerateFiles().ToList();
            FileUtil.SortFileCollection(orderedList);

            _pictures = new List<Picture>();

            foreach (var file in orderedList)
            {
                var fileFullName = file.FullName;

                var image = FileUtil.LoadBitmap(fileFullName);

                var bitmap = image.Resize(pictureBox.Width, pictureBox.Height);
                var p = new Picture(bitmap, fileFullName);

                _pictures.Add(p);

                image.Dispose();
            }

            if (_pictures.Count <= 0) return;
            pictureBox.Image = _pictures[0].Bitmap;
            pictureNbrLabel.Text = _pictures[0].Name;
        }

        public static int SortFunction(FileInfo file1, FileInfo file2)
        {
            var fileFullName1 = file1.FullName;

            var fileName1 = StringUtil.GetFileName(fileFullName1);
            var cameraNo = StringUtil.RemoveExtension(fileName1);

            var no1 = -1;

            if (StringUtil.IsInteger(cameraNo))
                no1 = int.Parse(cameraNo);

            var fileFullName2 = file2.FullName;

            var fileName2 = StringUtil.GetFileName(fileFullName2);
            cameraNo = StringUtil.RemoveExtension(fileName2);

            var no2 = -1;

            if (StringUtil.IsInteger(cameraNo))
                no2 = int.Parse(cameraNo);

            if (no1 != -1 && no2 != -1)
                return no1.CompareTo(no2);

            return string.Compare(fileName1, fileName2, StringComparison.Ordinal);
        }

        private void PoulateIntervalComboBox()
        {
            intervalComboBox.Items.Add("0.05");
            intervalComboBox.Items.Add("0.1");
            intervalComboBox.Items.Add("0.2");
            intervalComboBox.Items.Add("0.3");
            intervalComboBox.Items.Add("0.4");
            intervalComboBox.Items.Add("0.5");
            intervalComboBox.Items.Add("1");
        }

        private void OnIntervalChanged(object sender, EventArgs e)
        {
            var selectedItem = (string)intervalComboBox.Items[intervalComboBox.SelectedIndex];

            _timer.Interval = (int)(Convert.ToDouble(selectedItem, CultureInfo.InvariantCulture) * 1000);
        }

        private void OnStartButtonClick(object sender, EventArgs e)
        {
            _timer.Start();

            startButton.Enabled = false;
            stopButton.Enabled = true;
            nextButton.Enabled = false;
            backButton.Enabled = false;
            intervalComboBox.Enabled = false;
        }

        private void OnStopButtonClick(object sender, EventArgs e)
        {
            _timer.Stop();

            startButton.Enabled = true;
            stopButton.Enabled = false;
            nextButton.Enabled = true;
            backButton.Enabled = true;
            intervalComboBox.Enabled = true;
        }

        private void OnNextButtonClick(object sender, EventArgs e)
        {
            NextPicture();
        }

        private void OnBackButtonClick(object sender, EventArgs e)
        {
            PreviousPicture();
        }

        private void OnTimerEvent(object sender, EventArgs e)
        {
            NextPicture(true);
        }

        private void NextPicture(bool onTimer = false)
        {
            if (_pictures.Count <= 0) return;
            _onScreenPictureIndex++;

            while (onTimer && _discarded.Contains(_onScreenPictureIndex))
            {
                _onScreenPictureIndex++;
            }

            if (_pictures.Count <= _onScreenPictureIndex)
                _onScreenPictureIndex = 0;

            while (onTimer && _discarded.Contains(_onScreenPictureIndex))
            {
                _onScreenPictureIndex++;
            }

            var bmpToShow = _pictures[_onScreenPictureIndex].Bitmap;
            if (_discarded.Contains(_onScreenPictureIndex))
            {
                var tmp = bmpToShow.DebugDisplayCrossOnImage();
                bmpToShow.Dispose();
                bmpToShow = tmp;
            }

            pictureBox.Image = bmpToShow;
            pictureNbrLabel.Text = _pictures[_onScreenPictureIndex].Name;
            Refresh();
        }

        private void PreviousPicture()
        {
            if (_pictures.Count <= 0) return;
            _onScreenPictureIndex--;

            if (_onScreenPictureIndex < 0)
                _onScreenPictureIndex = _pictures.Count - 1;

            var bmpToShow = _pictures[_onScreenPictureIndex].Bitmap;
            if (_discarded.Contains(_onScreenPictureIndex))
            {
                var tmp = bmpToShow.DebugDisplayCrossOnImage();
                bmpToShow.Dispose();
                bmpToShow = tmp;
            }

            pictureBox.Image = bmpToShow;
            pictureNbrLabel.Text = _pictures[_onScreenPictureIndex].Name;
            Refresh();
        }

        /// <inheritdoc />
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _timer.Stop();

            pictureBox.Image = null;
            var list = new List<string>();
            for (var i = 0; i < _pictures.Count; i++)
            {
                _pictures[i].Bitmap.Dispose();
                if (_callback != null && _discarded.Contains(i))
                {
                    list.Add(_pictures[i].FilePath);
                }
            }

            if (_callback != null) Task.Run(() => _callback(list));

            base.OnFormClosing(e);
        }

        private class Picture
        {
            public Picture(Bitmap bitmap, string fullName)
            {
                Bitmap = bitmap;
                Name = StringUtil.GetFileName(fullName);
                FilePath = fullName;
            }

            public string FilePath { get; }

            public Bitmap Bitmap { get; }

            public string Name { get; }
        }

        private void btnDiscard_Click(object sender, EventArgs e)
        {
            if (!_discarded.Contains(_onScreenPictureIndex))
            {
                if (_discarded.Count == _pictures.Count - 1)
                {
                    // On refuse d'ajouter la dernière image.
                    return;
                }
                _discarded.Add(_onScreenPictureIndex);
                pictureBox.Image = _pictures[_onScreenPictureIndex].Bitmap.DebugDisplayCrossOnImage();
                pictureBox.Refresh();
            }
            else
            {
                _discarded.Remove(_onScreenPictureIndex);
                pictureBox.Image = _pictures[_onScreenPictureIndex].Bitmap;
                pictureBox.Refresh();
            }
        }
    }
}
