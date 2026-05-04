using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using NomadLogic.Common.Ui.Keyboard;
using NomadLogic.Common.Util;

namespace NomadLogic.PrintStation.Ui.custom.Nestea
{
    public partial class EmailDialog : Form
    {
        private readonly List<string> _emailList = new List<string>();
        private readonly List<TextBox> _inputList = new List<TextBox>();
        private readonly PictureBox _okButton;
        private readonly int _userId;
        private readonly PictureBox _closePictureBox;
        private KeyboardControl _keyboard;
        private Label _message;

        public EmailDialog(int userId)
        {
            InitializeComponent();

            _userId = userId;

            panel.Location = new Point(0, 0);

            var closeBitmap = GetBitmap("x_browser.png");

            if (closeBitmap != null)
            {
                _closePictureBox = new PictureBox();
                _closePictureBox.Location = new Point(Size.Width - closeBitmap.Width - 5, 7);
                _closePictureBox.Image = closeBitmap;
                _closePictureBox.Width = closeBitmap.Width;
                _closePictureBox.Height = closeBitmap.Height;
                Controls.Add(_closePictureBox);
                _closePictureBox.BringToFront();
            }

            var email = new PictureBox();
            email.Location = new Point(390, 10);
            email.Visible = true;
            var emailBitmap = GetBitmap("email.png");
            email.Image = emailBitmap;
            email.Width = emailBitmap.Width;
            email.Height = emailBitmap.Height;
            email.MouseDown += MouseDownHandler;
            Controls.Add(email);
            email.BringToFront();

            var okBitmap = GetBitmap("ok.png");

            if (okBitmap != null)
            {
                _okButton = new PictureBox();
                _okButton.Image = okBitmap;
                _okButton.Width = okBitmap.Width;
                _okButton.Height = okBitmap.Height;

                Controls.Add(_okButton);

                _okButton.BringToFront();
                _okButton.MouseDown += MouseDownHandler;
            }

            AddInputs();

            _closePictureBox.MouseDown += MouseDownHandler;

            AddKeyboard();

            //this.Location = new Point((Screen.PrimaryScreen.WorkingArea.Width - this.Width) / 2, 5);
            Location = new Point(0, 0);
        }

        public void SetEmailAddress(string email)
        {
            if (_inputList.Count > 0)
            {
                var textbox = _inputList[0];
                textbox.Text = email;
            }
        }

        private void AddKeyboard()
        {
            _keyboard = new KeyboardControl(0,0,kb_panel.Width, kb_panel.Height);
            kb_panel.Controls.Add(_keyboard);
            _keyboard.BringToFront();
            _keyboard.Show();
        }

        private Bitmap GetBitmap(string fileName)
        {
            Bitmap bitmap = null;

            var path =
                DirectoryUtil.GetLocalPrintstationMediaDir(_userId,
                    DirectoryUtil.NomadLogicPrintstationDirectoriesEnum.ui) + fileName;

            if (File.Exists(path))
            {
                var stream = new FileStream(path, FileMode.Open);
                bitmap = new Bitmap(stream);
                bitmap.SetResolution(96f, 96f);
                stream.Close();
            }

            if (bitmap == null)
            {
                throw new FileNotFoundException("Could not find image file: " + path);
            }

            return bitmap;
        }

        public void MouseUpHandler(object sender, MouseEventArgs e)
        {
            //Do nothing
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
                if (ValidateEntries())
                {
                    DialogResult = DialogResult.OK;
                    Close();
                }
            }
        }

        public List<string> GetEmailList()
        {
            return _emailList;
        }

        private bool ValidateEntries()
        {
            _emailList.Clear();
            var nbrEntry = _inputList.Count;
            for (var i = 0; i < nbrEntry; i++)
            {
                var textbox = _inputList[i];
                if (textbox.Text.Length > 0)
                {
                    if (!StringUtil.IsValidEmail(textbox.Text))
                    {
                        if (_message == null)
                        {
                            _message = new Label();
                            _message.Text = "Email address entered is not valid.";
                            _message.Width = 400;
                            _message.Font = new Font(FontFamily.GenericSansSerif, 15);
                            _message.ForeColor = Color.Red;
                            _message.Location = new Point(37, 70);
                            Controls.Add(_message);
                            _message.BringToFront();
                        }

                        return false;
                    }
                    _emailList.Add(textbox.Text);
                }
            }

            return true;
        }

        private void AddInputs()
        {
            var emailTextBox = new TextBox();

            emailTextBox.Location = new Point(50, 150);
            emailTextBox.Size = new Size(700, 50);
            emailTextBox.Font = new Font(FontFamily.GenericSansSerif, 24);
            emailTextBox.TabIndex = 1;

            Controls.Add(emailTextBox);

            _inputList.Add(emailTextBox);

            emailTextBox.BringToFront();

            if (_okButton != null)
            {
                _okButton.Location = new Point(390, 235);
            }

            Refresh();
        }
    }
}