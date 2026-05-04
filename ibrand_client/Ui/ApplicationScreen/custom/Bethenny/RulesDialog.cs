using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using NomadLogic.Common;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.custom.Bethenny
{
    public partial class RulesDialog : Form
    {
        readonly Rectangle _closeArea;

        private StringBuilder _rulesEn;

        RichTextBox _textBox;

        public RulesDialog()
        {
            InitializeComponent();

            AddTextBox();

            var backgroundName = Constants.APP_DIR + "images" + Path.DirectorySeparatorChar + "rules_dialog.png";

            if (File.Exists(backgroundName))
            {
                var stream = new FileStream(backgroundName, FileMode.Open);
                var bitmap = new Bitmap(stream);
                stream.Close();

                pictureBox.Location = new Point(0,0);
                pictureBox.Image = bitmap;
                pictureBox.Width = bitmap.Width;
                pictureBox.Height = bitmap.Height;

                Size = pictureBox.Size;
            }

            pictureBox.MouseDown += MouseDownHandler;

            _closeArea = new Rectangle(new Point(730, 16), new Size(64, 50));
        }

        private void AddTextBox()
        {
            _textBox = new RichTextBox
            {
                Font = new Font("Helveltica", 10, FontStyle.Regular),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Multiline = true,
                ReadOnly = true,
                Size = new Size(780, 680),
                Location = new Point(10, 69),
                ScrollBars = RichTextBoxScrollBars.ForcedVertical
            };

            SetRulesText();

            _textBox.Text = _rulesEn.ToString();

            Controls.Add(_textBox);

            _textBox.BringToFront();
        }

        private void SetRulesText()
        {
            try
            {
                _rulesEn = new StringBuilder();

                using (var sr = new StreamReader(Constants.APP_DIR + "images" + Path.DirectorySeparatorChar + "rules_en.txt", Encoding.UTF8))
                {
                    string line;
                    // Read and display lines from the file until the end of 
                    // the file is reached.
                    while ((line = sr.ReadLine()) != null)
                    {
                        _rulesEn.AppendLine(line);
                    }
                }

                
            }
            catch (System.Exception)
            {
                // rien
            }
        }

        public void MouseDownHandler(object sender, MouseEventArgs e)
        {
            if (InArea(e.X, e.Y, _closeArea))
            {
                Close();
            }
           
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
