using System;
using System.Threading;
using System.Windows.Forms;
using NomadLogic.Common.Ui.Dialog;

// ReSharper disable ConvertToAutoProperty

namespace NomadLogic.iBrandDesktop.Ui.Synchro
{
    [Obsolete]
    public partial class UploadDataDlg : SynchroDlg
    {
        private readonly CancellationTokenSource _cts;
        public event BeginDataUpload OnStartClick;
        
        public UploadDataDlg(CancellationTokenSource cts)
        {
            InitializeComponent();
            Init();
            _cts = cts;
        }

        private void Init()
        {
            userNameTextBox.KeyPress += userNameTextBox_KeyPress;
        }

        void userNameTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)13)
            {
                OnStart(sender, e);
            }
        }

        protected override void OnCancelEvent(object sender, EventArgs e)
        {
            _cts.Cancel();
        }

        protected override void OnStart(object sender, EventArgs e)
        {
            if (ValidateEntry())
            {
                OnStartClick?.Invoke(userNameTextBox.Text);
            }
        }

        protected override TextBox MessageTextBox => messageTextBox;

        protected override ProgressBar ProgressBar => totalProgressBar;

        protected ProgressBar RespondentProgressBar { get; private set; }

        public void SetRespondentProgressBarMinMax(int min, int max)
        {
            if (InvokeRequired)
            {
                // we were called on a worker thread
                // marshal the call to the user interface thread
                Invoke(new SetProgressMinMaxDelegate(SetRespondentProgressBarMinMax), min, max);
                return;
            }

            RespondentProgressBar.Minimum = min;
            RespondentProgressBar.Maximum = max;
            RespondentProgressBar.Value = min;
        }

        public void AddRespondentProgressStep(int value)
        {
            if (InvokeRequired)
            {
                // we were called on a worker thread
                // marshal the call to the user interface thread
                Invoke(new AddProgressStepDelegate(AddRespondentProgressStep), value);
                return;
            }

            RespondentProgressBar.Value = RespondentProgressBar.Value + value;
        }

        private bool ValidateEntry()
        {
            if (userNameTextBox.Text.Length != 0) return true;
            MsgBox.Show(this, Resources.iBrand.EnterUserName, Resources.iBrand.AppName);
            return false;
        }
    }
}