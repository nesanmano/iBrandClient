using System;
using System.Windows.Forms;
using NomadLogic.Common.Ui;
using NomadLogic.Common.Ui.Dialog;

namespace NomadLogic.iBrandDesktop.Ui.Synchro
{

    /// <summary>
    /// Utiliser la class GetProject2. Cette class et les autres sont sont obsolete.
    /// </summary>
    /// 
    [Obsolete]
    public partial class GetProjectDlg : SynchroDlg
    {
        public event GetProject OnStartClick;
        public event EventHandler OnCancelClick;

        public GetProjectDlg()
        {
            InitializeComponent();
            Init();

            Closing += (sender, args) => OnScreenKeyboardUtil.CloseKeyboard();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            OnScreenKeyboardUtil.OpenKeyboard();
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
                OnStart(sender, e);
            }
        }

        void userNameTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)13)
            {
                OnStart(sender, e);
            }
        }

        protected override TextBox MessageTextBox => messageTextBox;

        protected override ProgressBar ProgressBar => progressBar;

        protected override void OnStart(object sender, EventArgs e)
        {
            if (ValidateEntry())
            {
                OnStartClick?.Invoke(userNameTextBox.Text, pwTextBox.Text);
            }
        }

        protected override void OnCancelEvent(object sender, EventArgs e)
        {
            if (ValidateEntry())
            {
                OnCancelClick?.Invoke(sender, e);
            }
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
    }
}