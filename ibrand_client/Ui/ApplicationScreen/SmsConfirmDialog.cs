using System.Drawing;
using System.Windows.Forms;
using NomadLogic.Common.Ui;
using NomadLogic.Common.Ui.Keyboard;
using NomadLogic.Common.Util;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen
{
    public partial class SmsConfirmDialog : Form
    {
        private NumbersKeyboardControl _keyboard;
        private readonly Rectangle _checkRect;
        private readonly Bitmap _checkBmp;
        private bool _checked;
        private readonly TextBox _smsTextBox1;
        private readonly TextBox _smsTextBox2;

        readonly Rectangle _okArea;
        readonly Rectangle _cancelArea;

        public SmsConfirmDialog(Bitmap background, Point tboxPos1, Size tboxSize1, Point tboxPos2, Size tboxSize2, Rectangle check, Bitmap checkBitmap, Rectangle okArea, Rectangle cancelArea,
            Font textFont)
        {
            InitializeComponent();

            _okArea = okArea;
            _cancelArea = cancelArea;

            panel.Location = new Point(0, 0);
            panel.Width = background.Width;
            panel.Height = background.Height;

            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true); // Force le OnPaint à fonctionner.

            _checkRect = check;
            _checkBmp = checkBitmap;
            _checked = false;

            var bkg = new PictureBox();
            bkg.Location = new Point(0, 0);
            bkg.Visible = true;
            bkg.Image = background;
            bkg.Width = background.Width;
            bkg.Height = background.Height;
            bkg.MouseDown += MouseDownHandler;
            Controls.Add(bkg);
            bkg.BringToFront();

            bkg.Paint += PaintEvent;

            // Inputs
            _smsTextBox1 = new TextBox();

            _smsTextBox1.Location = tboxPos1;
            _smsTextBox1.Size = tboxSize1;
            if (textFont == null)
            {
                _smsTextBox1.Font = new Font(FontFamily.GenericSansSerif, 20);
            }
            else
            {
                _smsTextBox1.Font = textFont;
            }
            _smsTextBox1.TabIndex = 1;
            Controls.Add(_smsTextBox1);
            _smsTextBox1.BringToFront();

            _smsTextBox2 = new TextBox();

            _smsTextBox2.Location = tboxPos2;
            _smsTextBox2.Size = tboxSize2;
            if (textFont == null)
            {
                _smsTextBox2.Font = new Font(FontFamily.GenericSansSerif, 20);
            }
            else
            {
                _smsTextBox2.Font = textFont;
            }
            _smsTextBox2.TabIndex = 2;
            Controls.Add(_smsTextBox2);
            _smsTextBox2.BringToFront();

            kb_panel.Width = background.Width;

            AddKeyboard();

            Location = new Point((Screen.PrimaryScreen.WorkingArea.Width - Width) / 2, 5);

            Size = new Size(background.Width, panel.Height + kb_panel.Height);
            kb_panel.Location = new Point(0, panel.Height);
        }

        private void AddKeyboard()
        {
            _keyboard = new NumbersKeyboardControl(0, 0, kb_panel.Width, kb_panel.Height);

            kb_panel.Controls.Add(_keyboard);

            _keyboard.BringToFront();
            _keyboard.Show();

        }
 
        public void MouseDownHandler(object sender, MouseEventArgs e)
        {
            if (InArea(e.X, e.Y, _cancelArea))
            {
                DialogResult = DialogResult.Cancel;
                Close();
            }
            else if (InArea(e.X, e.Y, _okArea))
            {
                if (ValidateEntries())
                {
                    if (!_checked)
                    {
                        var mBox = new MessageDialog();
                        mBox.ShowMessage("You must checked the \"Text my photo to me.\" box to proceed.");
                        return;
                    }
                    DialogResult = DialogResult.OK;
                    Close();
                }
            }
            else if (InArea(e.X, e.Y, _checkRect))
            {
                _checked = !_checked;
                Refresh();
            }
        }

        public void PaintEvent(object sender, PaintEventArgs e)
        {
            if (_checked)
            {
                if (_checkBmp != null)
                {
                    e.Graphics.DrawImageUnscaled(_checkBmp, new Point(13, 32));
                }
            }
        }
        
        public string GetSms()
        {
            return _smsTextBox1.Text;
        }

        private bool ValidateEntries()
        {

            if (_smsTextBox1.Text.Length > 0)
            {
                if (!StringUtil.IsValidPhoneNumber(_smsTextBox1.Text))
                {
                    var mBox = new MessageDialog();
                    mBox.ShowMessage("Phone numbers must be valid and identical.");
                    return false;
                }
                _smsTextBox1.ForeColor = Color.Black;
            }
            else
            {
                var mBox = new MessageDialog();
                mBox.ShowMessage("Phone numbers must be valid and identical.");
                return false;
            }

            if (_smsTextBox2.Text.Length > 0)
            {
                if (!StringUtil.IsValidPhoneNumber(_smsTextBox2.Text))
                {
                    var mBox = new MessageDialog();
                    mBox.ShowMessage("Phone numbers must be valid and identical.");
                    return false;
                }
                _smsTextBox2.ForeColor = Color.Black;
            }
            else
            {
                var mBox = new MessageDialog();
                mBox.ShowMessage("Phone numbers must be valid and identical.");
                return false;
            }

            if (!_smsTextBox2.Text.Equals(_smsTextBox1.Text))
            {
                var mBox = new MessageDialog();
                mBox.ShowMessage("Phone numbers must be valid and identical.");
                return false;
            }

            return true;
        }
        
        private bool InArea(int x, int y, Rectangle area)
        {
            if (x > area.Location.X && x < area.Location.X + area.Width)
                if (y < area.Location.Y + area.Height && y > area.Location.Y)
                    return true;

            return false;
        }
    }
}