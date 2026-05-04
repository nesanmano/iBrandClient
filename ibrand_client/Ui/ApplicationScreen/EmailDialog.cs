using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using NomadLogic.Common.Ui.Keyboard;
using NomadLogic.Common.Util;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen
{
    public partial class EmailDialog : Form
    {
        const int MAX_INPUT = 3;

        private readonly List<TextBox> _inputList = new List<TextBox>();
        private readonly List<string> _emailList = new List<string>();
        private readonly PictureBox _plusButton = null;
        private readonly PictureBox _okButton;
        private readonly PictureBox _closePictureBox;
        private Label _message;
        private KeyboardControl _keyboard;
        private readonly string[] _defaultValues;
        private int _nbrInputs;
        private const int BASE_Y = 100;
        private const int DIFFERENCE_Y = 80;

        public EmailDialog() : this(3, null)
        {
            // Rien
        }

        public EmailDialog(int numberOfBoxes, string[] defaultValues)
        {
            _defaultValues = defaultValues;
            _nbrInputs = numberOfBoxes;

            if (_nbrInputs < 1)
            {
                _nbrInputs = 1;
            }
            else if (_nbrInputs > MAX_INPUT)
            {
                _nbrInputs = MAX_INPUT;
            }

            InitializeComponent();

            panel.Location = new Point(0, 0);

            var closeBitmap = GetBitmap("close.png");

            if (closeBitmap != null)
            {
                _closePictureBox = new PictureBox
                {
                    Location = new Point(Size.Width - closeBitmap.Width - 5, 7),
                    Image = closeBitmap,
                    Width = closeBitmap.Width,
                    Height = closeBitmap.Height
                };
                Controls.Add(_closePictureBox);
                _closePictureBox.BringToFront();
            }

            var email = new PictureBox
            {
                Location = new Point(600, 10),
                Visible = true
            };
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
            
            Location = new Point(0, 0);
        }

        public void SetEmailAddress(string email)
        {
            if (_inputList.Count <= 0) return;
            var textbox = _inputList[0];
            textbox.Text = email;
        }

        private void AddKeyboard()
        {
            _keyboard = new KeyboardControl(0,0,kb_panel.Width, kb_panel.Height);
            kb_panel.Controls.Add(_keyboard);

            _keyboard.BringToFront();
            _keyboard.Show();

        }

        protected Bitmap GetBitmap(string fileName)
        {
            Bitmap bitmap;

            var path = DirectoryUtil.GetLocalPhotoBoothDefaultAssetsDir(null) + fileName;

            if (File.Exists(path))
            {
                var stream = new FileStream(path, FileMode.Open);
                bitmap = new Bitmap(stream);
                bitmap.SetResolution(96f, 96f);
                stream.Close();
            }
            else
            {
                throw new FileNotFoundException("Could not find file: " + fileName);
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
                if (!ValidateEntries()) return;
                DialogResult = DialogResult.OK;
                Close();
            }
            else if (sender.Equals(_plusButton))
            {
                if (_inputList.Count < MAX_INPUT)
                    AddInput();
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
                if (textbox.Text.Length <= 0) continue;
                if (!StringUtil.IsValidEmail(textbox.Text))
                {
                    if (_message != null) return false;
                    _message = new Label
                    {
                        Text = Resources.iBrand.NotAllEmailsAreValid,
                        Width = 400,
                        Font = new Font(FontFamily.GenericSansSerif, 15),
                        ForeColor = Color.Red,
                        Location = new Point(37, 70)
                    };
                    Controls.Add(_message);
                    _message.BringToFront();

                    return false;
                }
                _emailList.Add(textbox.Text);
            }

            return true;
        }

        private void AddInput()
        {
            var nbrTextBox = _inputList.Count;

            var emailTextBox = new TextBox
            {
                Location = new Point(40, (((nbrTextBox + 1) * 80) + 20)),
                Size = new Size(600, 50),
                TabIndex = nbrTextBox + 1
            };

            Controls.Add(emailTextBox);

            _inputList.Add(emailTextBox);
            _nbrInputs++;

            emailTextBox.BringToFront();

            if (_okButton != null)
            {
                _okButton.Location = new Point(300, ((nbrTextBox + 2) * 80) + 30);
            }

            Refresh();
        }

        private void AddInputs()
        {
            for (var i = 0; i < _nbrInputs; i++)
            {
                var emailTextBox = new TextBox
                {
                    Location = new Point(170, BASE_Y + (i*DIFFERENCE_Y)),
                    Size = new Size(600, 50),
                    Font = new Font(FontFamily.GenericSansSerif, 24),
                    TabIndex = 1
                };

                if (_defaultValues != null && _defaultValues.Length >= i)
                {
                    emailTextBox.Text = _defaultValues[i];
                }

                Controls.Add(emailTextBox);
                _inputList.Add(emailTextBox);
                emailTextBox.BringToFront();
            }

            if (_okButton != null)
            {
                _okButton.Location = new Point(850, 165);
            }

            Refresh();
        }
    }
}
