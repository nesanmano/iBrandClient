using System;
using System.Drawing;
using System.Windows.Forms;
using NomadLogic.iBrandDesktop.Util;

namespace NomadLogic.iBrandDesktop.Ui
{
    public partial class InputDlg : Form
    {

        public event PasswordHandler OnOkClick;
        
        public InputDlg(bool password, string labelText)
        {
            InitializeComponent();
            Init();

            if(password)
                inputTextBox.PasswordChar = '*';

            label.Text = labelText;

            cancel_button.Click += OnCancel;
            ok_button.Click += OnOk;
        }

        private void Init()
        {
            //LOCALIZATION
            label.Text = Resources.iBrand.Password;
            ok_button.Text = Resources.iBrand.Ok;
            cancel_button.Text = Resources.iBrand.Cancel;
            inputTextBox.KeyPress += inputTextBox_KeyPress;
        }

        void inputTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)13)
            {
                OnOk(sender, e);
            }
        }

        /*protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            KeyboardForm keyboard = new KeyboardForm(WorkingArea.Size.Width);
            keyboard.Owner = this;
            keyboard.StartPosition = FormStartPosition.Manual;
            Point p = this.PointToScreen(WorkingArea.Container.Location);
            keyboard.Location = new Point(WorkingArea.Container.Location.X + 4, WorkingArea.Container.Location.Y + WorkingArea.Container.Height - keyboard.Height - 3);

            keyboard.Show();
            //keyboard.BringToFront();
            //this.BringToFront();
        }*/

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            StartPosition = FormStartPosition.Manual;
            var p = PointToScreen(WorkingArea.Container.Location);
            var x = WorkingArea.Container.Location.X + WorkingArea.Container.Width / 2 - Size.Width / 2;
            var y = WorkingArea.Container.Location.Y + (WorkingArea.Container.Height - Height) / 2;
            Location = new Point(x, y);
        }


        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);

            //Win32.HideStartIcon(this, false);
            //this.inputPanel.Enabled = true;
        }

        protected override void OnClosed(EventArgs e)
        {            
            base.OnClosed(e);
        }

        public string InputText
        {
            get
            {
                return inputTextBox.Text;
            }

            set
            {
                inputTextBox.Text = value;
            }
        }

        public int MaxLength
        {
            set
            {
                if (inputTextBox != null)
                    inputTextBox.MaxLength = value;
            }
        }

        /*public override void PositionWithinScreen()
        {
            int x = (Screen.PrimaryScreen.WorkingArea.Width - this.Width) / 2;
            int taskbarHeight = Screen.PrimaryScreen.Bounds.Height - Screen.PrimaryScreen.WorkingArea.Height;
            int y = taskbarHeight + 3;

            this.Location = new Point(x, y);
        }*/

        private void OnCancel(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void OnOk(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            if (OnOkClick != null)
                OnOkClick(inputTextBox.Text);
            Close();
        }
    }
}