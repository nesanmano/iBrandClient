using System;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using NomadLogic.Common.Util;
using NomadLogic.iBrandDesktop.Util;
using NomadLogic.Multimedia.Cli.Ffmpeg;

namespace NomadLogic.iBrandDesktop.Ui.Choice
{
    public partial class VideoCaptureCanonTextBox : UserControl
    {
        private volatile static object _lock;
        
        protected delegate void SetTextDelegate(string text);

        protected delegate void SelectFileDelegate(string text);

        private Thread _lookForPictureThread;

        private volatile bool _stopRequested;

        private volatile Mutex _mutex = new Mutex();

        static VideoCaptureCanonTextBox()
        {
            _lock = new object(); 
        }

        public VideoCaptureCanonTextBox()
        {
            InitializeComponent();

            Enabled = false;

            previewButton.Click += OnPreviewButtonClick;
        }

        public void StartThread()
        {
            if (_lookForPictureThread != null)
                StopThread();

            StopRequested = false;
            _lookForPictureThread = new Thread(LookForPictureInDir);
            _lookForPictureThread.Start();
        }

        private void LookForPictureInDir()
        {
            //mutex.WaitOne();

            var videoDir = DirectoryUtil.GetLocalMediaDir(DirectoryUtil.NomadLogicMediaDirectoriesEnum.videos_thumbnail);

            var lastFileFound = "";

            while (!StopRequested)
            {
                var di = new DirectoryInfo(videoDir);
                var fiArray = di.GetFiles();
                Array.Sort(fiArray, (x, y) => StringComparer.OrdinalIgnoreCase.Compare(x.CreationTime, y.CreationTime));
                if (fiArray.Length > 0)
                {
                    var fi = fiArray[fiArray.Length - 1];
                    var fileName = fi.FullName;
                    var temp = StringUtil.GetFileName(fileName);
                    if (!temp.Equals(lastFileFound))
                    {
                        lastFileFound = temp;
                        SetTextBoxText(fileName);
                    }
                }
                else
                {
                    if (!"".Equals(lastFileFound))
                    {
                        lastFileFound = lastFileFound = "";
                        SetTextBoxText("");
                    }
                }

                Thread.Sleep(1000);
            }

            //mutex.ReleaseMutex();
        }

        private void ShowFile(string fileName)
        {

            if (WorkingArea.Container.InvokeRequired)
            {
                WorkingArea.Container.Invoke(new SelectFileDelegate(ShowFile), fileName);
                return;
            }
            
            var thumbnailFileName = fileName.Substring(0, fileName.LastIndexOf(".")) + ".jpg";

            thumbnailFileName = DirectoryUtil.GetLocalMediaDir(DirectoryUtil.NomadLogicMediaDirectoriesEnum.videos_thumbnail) + StringUtil.GetFileName(thumbnailFileName);

            if (File.Exists(thumbnailFileName))
                File.Delete(thumbnailFileName);

            new FfmpegThumbnailsGenerator().GenerateThumbnail(fileName, thumbnailFileName, 1, 640, 360);

            if (File.Exists(thumbnailFileName))
            {
                var dlg = new ShowPictureDlg(thumbnailFileName);
                dlg.Owner = WorkingArea.Container;
                
                if (DialogResult.Cancel == dlg.ShowDialog())
                {
                    File.Delete(fileName);
                }
            }
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

        public void StopThread()
        {
            StopRequested = true;
            //Wait for thread to stop
            //mutex.WaitOne();
            _lookForPictureThread = null;
        }

        public void OnPreviewButtonClick(object sender, EventArgs args)
        {
                var file = textBox.Text;
                if (file != null && file.Length > 0)
                    ShowFile(file);
        }

        private void SetTextBoxText(string text)
        {
            if (textBox.InvokeRequired)
            {
                // we were called on a worker thread
                // marshal the call to the user interface thread
                try
                {
                    if (!textBox.IsDisposed)
                        textBox.Invoke(new SetTextDelegate(SetTextBoxText), text);
                }catch(System.Exception)
                {
                }
                return;
            }
            textBox.Text = text;
        }

        public override string Text => textBox.Text;

        public void SetSize(Size size)
        {
            Size = size;

            textBox.Width = size.Width - previewButton.Width - 5;

            var initialLocation = previewButton.Location;

            previewButton.Location = new Point(textBox.Width + 5, initialLocation.Y);
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
                //return this.textBox.Enabled;
            }
            set
            {
                textBox.Enabled = false;
                textBox.BackColor = Color.LightGray;
            }
        }
    }
}
