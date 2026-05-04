using System;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using NLog;
using NomadLogic.Common.Resources;
using NomadLogic.Common.Services;
using NomadLogic.Common.Services.Configuration;
using NomadLogic.Common.Ui;
using NomadLogic.Common.Ui.Choice;
using NomadLogic.Common.Ui.Dialog;
using NomadLogic.Common.Util;

namespace NomadLogic.iBrandDesktop.Ui.Choice
{
    public partial class VideoGalleryTextBox : UserControl
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        protected delegate void SetTextDelegate(string text);
        protected delegate void EnableButtonDelegate(bool enable);

        private string _fileName;
        private string _thumbnailFile;
        private Thread _lookForPictureThread;
        private static volatile object _lock = new object();
        private volatile bool _stopRequested;
        private bool _autoPopulate = true;
        private PreviewOnTv _previewOnTv;

        public VideoGalleryTextBox()
        {
            InitializeComponent();

            ThumbnailGalleryDir = DirectoryUtil.GetLocalMediaDir(DirectoryUtil.NomadLogicMediaDirectoriesEnum.videos_thumbnail);
            VideoGalleryDir = DirectoryUtil.GetLocalMediaDir(DirectoryUtil.NomadLogicMediaDirectoriesEnum.videos);

            textBox.Enabled = false;

            previewButton.Enabled = false;
            CanPrint = false;

            previewButton.Click += OnPreviewButtonClick;
            chooseButton.Click += OnChooseButtonClick;

        }

        public void OnChooseButtonClick(object sender, EventArgs args)
        {
            try
            {
                var gallery = new VideoGallery(this) {GalleryPath = ThumbnailGalleryDir};
                gallery.ShowDialog(this);
            }
            catch (System.Exception ex)
            {
                MsgBox.Show(this, ex.Message, common.app_title, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void OnPreviewButtonClick(object sender, EventArgs args)
        {
            try
            {
                if (_fileName == null || !File.Exists(_fileName)) return;
                var configuration = ServicesContainer.GetService<NomadConfiguration>();
                // On dispose si on lit l'image du disque.  Sinon, elle est dans le pictureController

                var picture = new Bitmap(_thumbnailFile);
                    
                if (configuration.Ibrand.ShowPreviewOnTv)
                {
                    if (Screen.AllScreens.Length > 1)
                    {
                        _previewOnTv = new PreviewOnTv();

                        _previewOnTv.ShowImage(picture);
                        _previewOnTv.Show();
                    }
                    else
                    {
                        MsgBox.Show(this, "No secondary screen detected.", common.app_title, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                    
                //MessageBox.Show(picture.ToString());
                var previewDialog = new PreviewDialog(picture) {Owner = ParentForm};

                // This ensures that the preview is shown on the primary screen
                var screens = Screen.AllScreens;
                SetFormLocation(previewDialog, screens[0]);

                previewDialog.WindowState = FormWindowState.Maximized;

                previewDialog.ShowDialog();

                //picture.Save(@"c:\test.jpg", ImageFormat.Jpeg);

                picture.Dispose();

                if (_previewOnTv == null) return;
                _previewOnTv.Dispose();
                _previewOnTv = null;
            }
            catch (System.Exception ex)
            {
                log.Fatal(ex);
            }
        }
        
        public void SetTextBoxText(string text)
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

        public void EnablePreviewButton(bool enable)
        {
            if (previewButton.InvokeRequired)
            {
                // we were called on a worker thread
                // marshal the call to the user interface thread
                Invoke(new EnableButtonDelegate(EnablePreviewButton), enable);
                return;
            }

            previewButton.Enabled = enable;

        }

        public void SetSize(Size size)
        {
            Size = size;

            textBox.Width = size.Width - chooseButton.Width - previewButton.Width - 10;

            var initialLocation = chooseButton.Location;

            chooseButton.Location = new Point(textBox.Width + 5, initialLocation.Y);

            previewButton.Location = new Point(textBox.Width + chooseButton.Width + 5, initialLocation.Y);

        }

        public void Reset()
        {
            textBox.Text = "";
            _fileName = null;
            EnablePreviewButton(false);
            _autoPopulate = true;
        }

        private void SetFormLocation(Form form, Screen screen)
        {
            var bounds = screen.Bounds;
            form.SetBounds(bounds.X, bounds.Y, bounds.Width, bounds.Height);
        }

        public override string Text => _fileName;

        public new bool Enabled
        {
            get
            {
                return base.Enabled;
            }
            set
            {
                chooseButton.Enabled = value;
                base.Enabled = value;
            }
        }

        public bool CanPrint { get; set; }

        public string ThumbnailGalleryDir { get; set; }
        public string VideoGalleryDir { get; set; }

        public void SetFile(string fileName, bool autoPopulate)
        {
            var thumbnailFile = new FileInfo(fileName);
            if (thumbnailFile.Name.Equals("filming") || thumbnailFile.Name.Equals("processing") || thumbnailFile.Name.Equals("sending"))
            {
                EnablePreviewButton(false);
                SetTextBoxText(thumbnailFile.Name);
            }
            else
            {
                _thumbnailFile = fileName;
                var videoFile = new FileInfo(VideoGalleryDir + Path.DirectorySeparatorChar + thumbnailFile.Name.Substring(0, thumbnailFile.Name.Length - 4));
                if (videoFile.Exists)
                {
                    _fileName = videoFile.FullName;
                    SetTextBoxText(videoFile.Name);
                    EnablePreviewButton(true);
                    _autoPopulate = autoPopulate;
                }
                else
                {
                    EnablePreviewButton(false);
                }
            }

        }

        private void LookForPictureInDir()
        {
            //mutex.WaitOne();
            
            var videoDir = DirectoryUtil.GetLocalMediaDir(DirectoryUtil.NomadLogicMediaDirectoriesEnum.videos_thumbnail);

            var lastFileFound = "";

            while (!StopRequested)
            {
                if (_autoPopulate)
                {
                    var di = new DirectoryInfo(videoDir);
                    var fiArray = di.GetFiles();
                    Array.Sort(fiArray, (x, y) => StringComparer.OrdinalIgnoreCase.Compare(x.LastWriteTime, y.LastWriteTime));
                    if (fiArray.Length > 0)
                    {
                        var fi = fiArray[fiArray.Length - 1];
                        var fileName = fi.FullName;
                        var temp = StringUtil.GetFileName(fileName);
                        if (!temp.Equals(lastFileFound))
                        {
                            lastFileFound = temp;
                            SetFile(fileName, true);
                        }
                    }
                    else
                    {
                        if (!"".Equals(lastFileFound))
                        {
                            lastFileFound = "";
                            SetTextBoxText("");
                        }
                    }
                }
                
                Thread.Sleep(1000);
            }

            //mutex.ReleaseMutex();
        }

        private bool StopRequested
        {
            get
            {
                lock (_lock)
                {
                    return _stopRequested;
                }
            }

            set
            {
                lock (_lock)
                {
                    _stopRequested = value;
                }
            }
        }

        public void StartThread()
        {
            if (_lookForPictureThread != null)
                StopThread();

            StopRequested = false;
            _lookForPictureThread = new Thread(LookForPictureInDir);
            _lookForPictureThread.Start();
        }

        public void StopThread()
        {
            StopRequested = true;
            //Wait for thread to stop
            //mutex.WaitOne();
            _lookForPictureThread = null;
        }

    }
}
