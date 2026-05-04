using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using NLog;
using NomadLogic.Common.Serveur;
using NomadLogic.Common.Services;
using NomadLogic.Common.Services.Configuration;
using NomadLogic.Common.Ui.Dialog;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Networking.DownloadProgress;
using NomadLogic.Common.Util.Win32;
using NomadLogic.iBrandDesktop.Services;
using NomadLogic.iBrandDesktop.Synchro.DownloadRestRequest;

namespace NomadLogic.iBrandDesktop.Ui.Synchro.New_UI
{
    public partial class GetProjectTerminal : Form
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        public event EventHandler OnCompleted;

        private readonly Progress<DownloadReportInfo> _progress;

        private DownloadManager _downloadManager;

        private CancellationTokenSource _cts;
        
        public GetProjectTerminal()
        {
            InitializeComponent();
            Init();
            startButton.Click += Start;
            cancelButton.Click += Cancel;
            _progress = new Progress<DownloadReportInfo>(OnDownloadReport);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            StartPosition = FormStartPosition.CenterParent;
        }
        
        private void Init()
        {
            userNameTextBox.KeyPress += userNameTextBox_KeyPress;
            pwTextBox.KeyPress += pwTextBox_KeyPress;
        }

        void pwTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)13)
            { 
                Start(sender, e);
            }
        }

        void userNameTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)13)
            {
                Start(sender, e);
            }
        }

        protected void Start(object sender, EventArgs e)
        {
            if (!startButton.Enabled) return;

            if (!ValidateEntry()) return;

            startButton.Enabled = false;
            cancelButton.Enabled = true;

            var db = ServicesContainer.GetService<IbrandDatabase>();
            var conf = ServicesContainer.GetService<NomadConfiguration>();
            
            _cts = new CancellationTokenSource();
            _downloadManager = new DownloadManager(db, conf, _progress);

            using (_downloadManager)
            {
                try
                {
                    var authorizationInfo = new ServerTerminalDownloadRequest()
                    {
                        Username = userNameTextBox.Text.Trim(),
                        Password = pwTextBox.Text.Trim()
                    };

                    var baseProjectDir = DirectoryUtil.GetLocalApplicationDir() + "projects\\";

                    messageTextBox.Text += @"Preparing to download files..." + Environment.NewLine;

                    Task.Factory.StartNew(() => { _downloadManager.Execute(authorizationInfo, baseProjectDir, _cts.Token); }, _cts.Token, TaskCreationOptions.LongRunning,
                        TaskScheduler.Default).ContinueWith(t => { this.SafeCall(RevertButtons); });
                }
                catch (System.Exception err)
                {
                    log.Error(err, "Error has occured during terminal download.");
                    messageTextBox.Text += @"Error has occured during execution. Check logs for details." + Environment.NewLine;
                    messageTextBox.Text += err.Message + Environment.NewLine;
                    Refresh();
                }
            }
        }

        private void RevertButtons()
        {
            startButton.Enabled = true;
            cancelButton.Enabled = false;
            OnCompleted?.Invoke(this, EventArgs.Empty);
        }

        private void OnDownloadReport(DownloadReportInfo res)
        {
            // verify if any error has been reported
            if (res.Error != 0)
            {
                _cts.Cancel(); // immediately cancel any imminent or awaiting tasks.
            }

            progressBar.Minimum = 0;
            progressBar.Maximum = 100;
            progressBar.Value = 0;

            progressBar.Value = res.BytesOverallProgress;

            progressBar2.Minimum = 0;
            progressBar2.Maximum = res.TotalNumFilestoDownload;
            progressBar2.Value = 0;

            progressBar2.Value = res.FilesOverallProgress;

            messageTextBox.Text += res.Message + Environment.NewLine;
            messageTextBox.SelectionStart = messageTextBox.Text.Length;
            messageTextBox.Refresh();
            messageTextBox.ScrollToCaret();

            Refresh();            
        }

        private bool ValidateEntry()
        {
            if (userNameTextBox.Text.Length == 0)
            {
                MsgBox.Show(this, Resources.iBrand.EnterUserName, Resources.iBrand.AppName);
                return false;
            }

            if (pwTextBox.Text.Length == 0)
            {
                MsgBox.Show(this, Resources.iBrand.EnterPW, Resources.iBrand.AppName);
                return false;
            }
            return true;
        }
        
        private void Cancel(object sender, EventArgs e)
        {
            startButton.Enabled = true;
            cancelButton.Enabled = false;

            if (_cts == null || _cts.IsCancellationRequested) return;
            _cts.Cancel();
            Refresh();
            messageTextBox.Text += @"Operation cancelled by user." + Environment.NewLine;
        }
    }
}
