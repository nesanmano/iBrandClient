using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using NomadLogic.Common.Services;
using NomadLogic.Common.Services.Configuration;
using NomadLogic.Common.Ui;
using NomadLogic.Common.Ui.Keyboard;
using NomadLogic.Common.Util;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen
{
    public partial class PasswordDialog : Form
    {
        private TextBox _pwTextBox;

        private readonly PictureBox _okButton;
        private KeyboardControl _keyboard;
        private readonly PictureBox _closePictureBox;

        private Label _message;
        private readonly string _currentLanguage;
        private readonly bool _showKeyBoard;

        public PasswordDialog(string currentLanguage, bool showKeyboard = false)
        {
            InitializeComponent();
            _showKeyBoard = showKeyboard;

            var config = ServicesContainer.GetService<NomadConfiguration>();

            var passwordDialogSize = config?.PasswordDialogSize ?? new Size(1000, 600);
            var fontSizeRatio = config?.PasswordDialogKeyboardFontRatio ?? 0.075f;

            if (showKeyboard)
            {
                ClientSize = passwordDialogSize;
                AddKeyboard(fontSizeRatio);
            }

            _currentLanguage = currentLanguage;

            var closeBitmap = GetBitmap("x.png");

            _closePictureBox = new PictureBox
            {
                Location = new Point(Size.Width - closeBitmap.Width - 7, 10),
                Image = closeBitmap,
                Width = closeBitmap.Width,
                Height = closeBitmap.Height
            };
            Controls.Add(_closePictureBox);
            _closePictureBox.BringToFront();

            AddInput();

            var okBitmap = GetBitmap("ok.png");

            _okButton = new PictureBox
            {
                Location = new Point((Size.Width - okBitmap.Width) / 2, 115),
                Image = okBitmap,
                Width = okBitmap.Width,
                Height = okBitmap.Height
            };

            Controls.Add(_okButton);

            _okButton.BringToFront();
            _okButton.MouseDown += MouseDownHandler;

            _closePictureBox.MouseDown += MouseDownHandler;

            Location = new Point((Screen.PrimaryScreen.WorkingArea.Width - Width) / 2, 5);
            ActiveControl = _pwTextBox;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (_showKeyBoard) return;
            OnScreenKeyboardUtil.OpenKeyboard();
        }

        public void MouseDownHandler(object sender, MouseEventArgs e)
        {
            if (sender.Equals(_closePictureBox))
            {
                DialogResult = DialogResult.Cancel;
                Close();
            }
            else if (sender.Equals(_okButton))
            {
                if (!ValidatePassword()) return;
                DialogResult = DialogResult.OK;
                Close();
            }

        }

        private bool ValidatePassword()
        {
            if (_pwTextBox.Text.Equals("nomad")) return true;
            if (_message != null) return false;

            var tmp = Resources.iBrand.ResourceManager.GetString("PasswordInvalid", Resources.iBrand.Culture);

            _message = new Label
            {
                Text = tmp,
                Width = 300,
                Font = new Font(FontFamily.GenericSansSerif, 15),
                ForeColor = Color.Red,
                Location = new Point(15, 30)
            };
            Controls.Add(_message);
            _message.BringToFront();

            return false;
        }

        private void AddInput()
        {
            var enterBitmap = GetBitmap("enter_password.png");

            var enterPictureBox = new PictureBox
            {
                Image = enterBitmap,
                Width = enterBitmap.Width,
                Height = enterBitmap.Height
            };
            Controls.Add(enterPictureBox);
            enterPictureBox.BringToFront();

            _pwTextBox = new TextBox
            {
                Size = new Size(200, 80),
                TabIndex = 1,
                Font = new Font(FontFamily.GenericSansSerif, 24)
            };

            Controls.Add(_pwTextBox);

            _pwTextBox.BringToFront();

            var offset = (_pwTextBox.Size.Width + enterPictureBox.Size.Width + 20) / 2;
            var mid = Size.Width / 2;

            _pwTextBox.Location = new Point(mid - offset + 20 + enterPictureBox.Size.Width, 60);
            _pwTextBox.Focus();
            enterPictureBox.Location = new Point(mid - offset, 70);
        }

        private void AddKeyboard(float fontSizeRatio)
        {
            var height = Size.Height - 240;
            _keyboard = new KeyboardControl(10, 200, Size.Width - 20, height)
            {
                KeyPadding = -1, 
                CustomLabelInfo =
                {
                    Font = StaticCustomFontRepository.CreateCustomFont(new FontDefinitionInfo
                    {
                        Size = Math.Max(20, (int)(height * fontSizeRatio))
                    })
                }
            };

            Controls.Add(_keyboard);
            _keyboard.BringToFront();
            _keyboard.Show();
        }

        protected Bitmap GetBitmap(string fileName)
        {
            Bitmap bitmap;

            var path = DirectoryUtil.GetLocalPhotoBoothDefaultAssetsDir(_currentLanguage) + fileName;

            if (!File.Exists(path)) // TODO Test pour le printstation. Faudrait une meilleure méthode.
            {
                path = DirectoryUtil.GetLocalPhotoBoothDefaultAssetsDir() + fileName;
            }

            if (File.Exists(path))
            {
                var stream = new FileStream(path, FileMode.Open);
                bitmap = new Bitmap(stream);
                stream.Close();
            }
            else
            {
                throw new FileNotFoundException("Could not find image file: " + path);
            }

            return bitmap;
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);

            if (_showKeyBoard) return;
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            HideKeyboard();
        }

        private void HideKeyboard()
        {
            if (_showKeyBoard) return;
            OnScreenKeyboardUtil.CloseKeyboard();
        }

    }
}
