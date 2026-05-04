using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using JetBrains.Annotations;
using WinSCP;

namespace NomadLogic.Common.Util.Networking.Ftp
{
    public partial class FtpSynchroUi : Form
    {
        private CancellationTokenSource _cts;
        private Task<bool> _task;
        private readonly string _localPath;
        private readonly string _remotePath;
        private readonly SessionOptions _options;

        public FtpSynchroUi([NotNull] string localPath, [NotNull] string remotePath, [NotNull] SessionOptions options)
        {
            _localPath = localPath;
            _remotePath = remotePath;
            _options = options;
        }

        public FtpSynchroUi([NotNull] string localPath, [NotNull] string remotePath, [NotNull] string addr, uint port, [NotNull] string user, [NotNull] string pass)
        {
            _localPath = localPath;
            _remotePath = remotePath;
            _options = new SessionOptions
            {
                HostName = addr,
                PortNumber = (int)port,
                UserName = user,
                Password = pass
            };
            InitializeComponent();
        }

        private void FtpSynchroUi_Load(object sender, EventArgs e)
        {
            _cts = new CancellationTokenSource();

            var prog = new Progress<FtpBatchProgress>(Handler);

            _txtBoxFilters.Text = @"Not implemented";

            _task = FtpWebUtil.SynchronizeDirectory(_options, _localPath, _remotePath, _cts.Token, null, null, prog).ContinueWith(task =>
            {
                Close();
                return true;
            });

        _btnCancel.Enabled = true;
        }

        private void Handler(FtpBatchProgress obj)
        {
            if (obj == null) return;
            var totalFiles = obj.Transfers.Count;
            var completed = obj.Transfers.Sum(e => e.PercentComplete == 1.0 ? 1 : 0);

            _lblFileTotal.Text = $@"{completed}/{totalFiles}";
            _progBarTotal.Value = (int)obj.Completion;
        }

        private void FtpSynchroUi_FormClosing(object sender, FormClosingEventArgs e)
        {
            Task.WaitAll(_task);

            if (_cts.IsCancellationRequested)
            {
                DialogResult = DialogResult.Cancel;
            }
            else
            {
                DialogResult = _task.Result ? DialogResult.OK : DialogResult.No;    
            }
        }

        private void _btnCancel_Click(object sender, EventArgs e)
        {
            _cts?.Cancel();
            Close();
        }
    }
}
