using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using NomadLogic.Common.Ui.Keyboard;
using NomadLogic.Common.Util;

namespace NomadLogic.PrintStation.Ui
{
    public partial class SmsDialog : Form
    {
        private readonly List<TextBox> _inputList = new List<TextBox>();
        private readonly PictureBox _okButton;
        private readonly List<string> _smsList = new List<string>();
        private readonly int _userId;
        private readonly PictureBox _closePictureBox;
        private NumbersKeyboardControl _keyboard;
        private Label _message;

        public SmsDialog(int userId)
        {
            InitializeComponent();

            _userId = userId;

            panel.Location = new Point(0, 0);
            panel.Size = Size;

            //Bitmap close_bitmap = GetBitmap("x.png");
            var closeBitmap = GetBitmap("x_browser.png");

            if (closeBitmap != null)
            {
                _closePictureBox = new PictureBox
                {
                    Location = new Point(530, 12),
                    Image = closeBitmap,
                    Width = closeBitmap.Width,
                    Height = closeBitmap.Height
                };
                Controls.Add(_closePictureBox);
                _closePictureBox.BringToFront();
            }

            var sms = new PictureBox
            {
                Location = new Point(230, 10),
                Visible = true
            };
            var smsBitmap = GetBitmap("sms.png");
            sms.Image = smsBitmap;
            sms.Width = smsBitmap.Width;
            sms.Height = smsBitmap.Height;
            sms.MouseDown += MouseDownHandler;
            Controls.Add(sms);
            sms.BringToFront();

            var okBitmap = GetBitmap("ok.png");

            if (okBitmap != null)
            {
                _okButton = new PictureBox
                {
                    Image = okBitmap,
                    Width = okBitmap.Width,
                    Height = okBitmap.Height
                };

                Controls.Add(_okButton);

                _okButton.BringToFront();
                _okButton.MouseDown += MouseDownHandler;
            }

            AddInputs();

            _closePictureBox.MouseDown += MouseDownHandler;

            AddKeyboard();

            Location = new Point((Screen.PrimaryScreen.WorkingArea.Width - Width)/2, 5);
        }

        private void AddKeyboard()
        {
            _keyboard = new NumbersKeyboardControl(0,0,kb_panel.Width, kb_panel.Height);
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

        public List<string> GetSmsList()
        {
            return _smsList;
        }

        private bool ValidateEntries()
        {
            _smsList.Clear();
            var nbrEntry = _inputList.Count;
            for (var i = 0; i < nbrEntry; i++)
            {
                var textbox = _inputList[i];
                if (textbox.Text.Length > 0)
                {
                    if (!StringUtil.IsValidPhoneNumber(textbox.Text))
                    {
                        if (_message == null)
                        {
                            _message = new Label
                            {
                                Text = @"Not all phone numbers entered are valid.",
                                Width = 300,
                                Font = new Font(FontFamily.GenericSansSerif, 15),
                                ForeColor = Color.Red,
                                Location = new Point(37, 70)
                            };
                            Controls.Add(_message);
                            _message.BringToFront();
                        }

                        return false;
                    }
                    _smsList.Add(textbox.Text);
                }
            }

            return true;
        }

        private void AddInputs()
        {
            var emailTextBox = new TextBox
            {
                Location = new Point(20, 180),
                Size = new Size(400, 50),
                Font = new Font(FontFamily.GenericSansSerif, 24),
                TabIndex = 1
            };
            
            Controls.Add(emailTextBox);

            _inputList.Add(emailTextBox);

            emailTextBox.BringToFront();
            
            if (_okButton != null)
            {
                _okButton.Location = new Point(450, 165);
            }

            Refresh();
        }
    }
}