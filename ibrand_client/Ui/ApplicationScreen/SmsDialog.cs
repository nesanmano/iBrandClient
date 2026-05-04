using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using NomadLogic.Common.Ui.Keyboard;
using NomadLogic.Common.Util;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen
{
    public partial class SmsDialog : Form
    {
        const int MAX_INPUT = 3;

        readonly List<TextBox> _inputList = new List<TextBox>();

        readonly List<string> _smsList = new List<string>();

        readonly PictureBox _plusButton = null;
        readonly PictureBox _okButton;

        readonly PictureBox _closePictureBox;

        private Label _message;

        private NumbersKeyboardControl _keyboard;
        private readonly string[] _defaultValues;
        private int _nbrInput;
        private const int DIFFERENCE_Y = 80;
        private const int BASE_Y = 100;

        public SmsDialog(int numberOfBoxes, string[] defaultValues)
        {
            _defaultValues = defaultValues;

            _nbrInput = numberOfBoxes;
            if (_nbrInput < 1)
            {
                _nbrInput = 1;
            }
            else if (_nbrInput > 3)
            {
                _nbrInput = MAX_INPUT;
            }

            InitializeComponent();

            panel.Location = new Point(0, 0);
            panel.Size = Size;

            var closeBitmap = GetBitmap("close.png");

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

            Location = new Point((Screen.PrimaryScreen.WorkingArea.Width - Width) / 2, 5);
        }

        public SmsDialog() : this(3, null)
        {
            // rien d'autre
        }

        private void AddKeyboard()
        {
            _keyboard = new NumbersKeyboardControl(0,0,kb_panel.Width, kb_panel.Height);
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
                if (textbox.Text.Length <= 0) continue;
                if (!StringUtil.IsValidPhoneNumber(textbox.Text))
                {
                    if (_message != null) return false;
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

                    return false;
                }
                _smsList.Add(textbox.Text);
            }

            return true;
        }

        private void AddInput()
        {
            var nbrTextBox = _inputList.Count;

            var emailTextBox = new TextBox
            {
                Location = new Point(40, (((nbrTextBox + 1)*DIFFERENCE_Y) + 20)),
                Size = new Size(370, 70),
                TabIndex = nbrTextBox + 1,
                Font = new Font(FontFamily.GenericSansSerif, 24)
            };
            
            Controls.Add(emailTextBox);

            _inputList.Add(emailTextBox);
            _nbrInput++;

            emailTextBox.BringToFront();

            if (_plusButton != null)
            {
                _plusButton.Location = new Point(40, (((nbrTextBox + 1) * DIFFERENCE_Y) + BASE_Y));
            }

            if (_okButton != null)
            {
                _okButton.Location = new Point(190, ((nbrTextBox + 2) * DIFFERENCE_Y));
            }

            var temp = Size;

            Size = new Size(temp.Width, ((nbrTextBox + 2) * DIFFERENCE_Y) + BASE_Y);
            panel.Size = Size;
            Refresh();
        }

        private void AddInputs()
        {
            for (var i = 0; i < _nbrInput; i++)
            {
                var emailTextBox = new TextBox
                {
                    Location = new Point(20, BASE_Y + (i*DIFFERENCE_Y)),
                    Size = new Size(400, 50),
                    Font = new Font(FontFamily.GenericSansSerif, 24),
                    TabIndex = 1
                };

                if (_defaultValues != null && _defaultValues.Count() >= i)
                {
                    emailTextBox.Text = _defaultValues[i];
                }

                Controls.Add(emailTextBox);

                _inputList.Add(emailTextBox);

                emailTextBox.BringToFront();
            }

            if (_okButton != null)
            {
                _okButton.Location = new Point(450, 165);
            }

            Refresh();
        }
    }
}