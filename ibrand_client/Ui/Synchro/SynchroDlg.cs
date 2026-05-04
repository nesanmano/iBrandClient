using System;
using System.Drawing;
using System.Windows.Forms;
using NomadLogic.iBrandDesktop.Synchro;
using NomadLogic.iBrandDesktop.Util;

namespace NomadLogic.iBrandDesktop.Ui.Synchro
{

    /// Utiliser la class GetProject2. Cette class et les autres sont obsolete.
    /// </summary>
    [Obsolete]
    public partial class SynchroDlg : Form, IUiSynchro
    {
        protected delegate void AddUiMessageDelegate(string message, int priority);
        protected delegate void AddProgressStepDelegate(int value);
        protected delegate void SetProgressMinMaxDelegate(int min, int max);

        protected delegate void ChangeMenusDelegate();
        protected delegate void ClearUiMessagesDelegate();
        
        public SynchroDlg()
        {
            InitializeComponent();
            //InitMenus();
            startButton.Click += OnStart;
            cancelButton.Click += OnCancel;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            StartPosition = FormStartPosition.Manual;
            PointToScreen(WorkingArea.Container.Location);
            var x = WorkingArea.Container.Location.X + WorkingArea.Container.Width / 2 - Size.Width / 2;
            var y = WorkingArea.Container.Location.Y + (WorkingArea.Container.Height - Height) / 2;
            Location = new Point(x, y);
        }

        public void SetCancelButtonToOk()
        {
            if (InvokeRequired)
            {
                Invoke(new ChangeMenusDelegate(SetCancelButtonToOk));
                return;
            }

            cancelButton.Text = Resources.iBrand.Ok;
            cancelButton.Enabled = true;

            Refresh();
        }

        public void DisableButtons()
        {
            if (InvokeRequired)
            {
                Invoke(new ChangeMenusDelegate(DisableButtons));
                return;
            }

            startButton.Enabled = false;

            Refresh();
        }

        public void EnableButtons()
        {
            if (InvokeRequired)
            {
                Invoke(new ChangeMenusDelegate(EnableButtons));
                return;
            }

            startButton.Enabled = true;
            cancelButton.Text = Resources.iBrand.Cancel;
            cancelButton.Enabled = true;

            Refresh();
        }

        public void AddUiMessage(string message, int priority)
        {
			// We don't need the priority here
            if (InvokeRequired)
            {
                // we were called on a worker thread
                // marshal the call to the user interface thread
                Invoke(new AddUiMessageDelegate(AddUiMessage), message, priority);
                return;
            }

            MessageTextBox.Text += message;
            // ReSharper disable once LocalizableElement
            MessageTextBox.Text += "\r\n";
            MessageTextBox.SelectionStart = MessageTextBox.Text.Length;
            MessageTextBox.Refresh();
            MessageTextBox.ScrollToCaret();
        }

        public void SetProgressBarMinMax(int min, int max)
        {
            if (InvokeRequired)
            {
                // we were called on a worker thread
                // marshal the call to the user interface thread
                Invoke(new SetProgressMinMaxDelegate(SetProgressBarMinMax), min, max);
                return;
            }

            ProgressBar.Minimum = min;
            ProgressBar.Maximum = max;
            ProgressBar.Value = min;
        }

        public void AddProgressStep(int value)
        {
            if (InvokeRequired)
            {
                // we were called on a worker thread
                // marshal the call to the user interface thread
                Invoke(new AddProgressStepDelegate(AddProgressStep), value);
                return;
            }

            ProgressBar.Value = ProgressBar.Value + value;
        }

        public void ClearUiMessages()
        {
            if (InvokeRequired)
            {
                // we were called on a worker thread
                // marshal the call to the user interface thread
                Invoke(new ClearUiMessagesDelegate(ClearUiMessages));
                return;
            }

            MessageTextBox.Text = "";
            MessageTextBox.Refresh();
            MessageTextBox.ScrollToCaret();
        }

        private void OnCancel(object sender, EventArgs e)
        {
            if (cancelButton.Text.Equals(Resources.iBrand.Cancel))
            {
                OnCancelEvent(sender, e);
                SetCancelButtonToOk();
                AddUiMessage("Data upload cancelled.", 1);
                return;
            }

            Close();
        }

        protected virtual void OnCancelEvent(object sender, EventArgs e)
        {
            // Rien à la base.
        }

        protected virtual void OnStart(object sender, EventArgs e) { }
        
        protected virtual TextBox MessageTextBox => null;

        protected virtual ProgressBar ProgressBar => null;

    }
}