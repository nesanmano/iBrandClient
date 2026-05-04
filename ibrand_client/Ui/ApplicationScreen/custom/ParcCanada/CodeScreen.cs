using System;
using System.Drawing;
using System.Windows.Forms;
using NomadLogic.Common;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.custom.ParcCanada
{
    public partial class CodeScreen : AbstractScreen
    {
        readonly Rectangle _nextArea;
        readonly Rectangle _backArea;

        private TextBox _emailTextBox;

        public CodeScreen(string backgroundImgEnSrc, string backgroundImageSrc, ScreenConfig sc)
            : base(backgroundImgEnSrc, backgroundImageSrc, sc)
        {
            //InitializeComponent();
            AddTextBoxs();
            AddKeyboard();

            //PictureBox.MouseDown += MouseDownHandler;

            _nextArea = new Rectangle(new Point(839, 392), new Size(255, 81));
            _backArea = new Rectangle(new Point(40, 980), new Size(86, 86));
        }

        public override void Recycle()
        {
            base.Recycle();
            _emailTextBox.Text = "";
        }

        private void AddKeyboard()
        {
            AddKeyboardToControls((1920 - 1500) / 2, 1080 - 600, 1500, 500);
        }

        private bool CheckCode()
        {
            if (!_emailTextBox.Text.ToLower().Equals("parka"))
            {
                if (_activeLg.Equals(Constants.FR))
                {
                    MessageBox.Show(this, "Le code est invalide.", "iBrand (tm)", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                }
                else
                {
                    MessageBox.Show(this, "Code is not valid.", "iBrand (tm)", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                }

                return false;
            }

            _emailTextBox.Text = "";
            _emailTextBox.Invalidate();
            return true;
        }

        public void OnTextChanged(object sender, EventArgs e)
        {
            if (_activated)
                ResetTimer();
        }

        private void AddTextBoxs()
        {
            _emailTextBox = new TextBox();
            _emailTextBox.Location = new Point((1920 - 600) / 2, 260);
            _emailTextBox.Size = new Size(600, 40);
            _emailTextBox.TabIndex = 0;
            _emailTextBox.Font = new Font(FontFamily.GenericSansSerif, 20);
            _emailTextBox.TextChanged += OnTextChanged;
            _emailTextBox.TextAlign = HorizontalAlignment.Center;
            _emailTextBox.PasswordChar = '*';
            _emailTextBox.UseSystemPasswordChar = true;

            Controls.Add(_emailTextBox);
            
        }

        /*protected override void MouseDownHandler(object sender, MouseEventArgs e)
        {
            base.MouseDownHandler(sender, e);

            if (InArea(e.X, e.Y, _nextArea))
            {
                if (_emailTextBox.Text != null && !_emailTextBox.Text.Equals(""))
                {
                    if (CheckCode())
                        Next();
                }

            }
            else if (InArea(e.X, e.Y, _backArea))
            {
                Back(); // Retour au debut.
            }
        }
*/

        public override void SetActivated()
        {
            base.SetActivated();
            _emailTextBox.Text = "";
            _emailTextBox.BringToFront();
            _emailTextBox.Focus();
        }
    }
}
