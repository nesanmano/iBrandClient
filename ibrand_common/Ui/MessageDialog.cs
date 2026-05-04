using System.Drawing;
using System.IO;
using System.Windows.Forms;
using NomadLogic.Common.Util;

namespace NomadLogic.Common.Ui
{
    public partial class MessageDialog : Form
    {
        readonly Rectangle _closeArea;

        public MessageDialog()
        {
            InitializeComponent();

            var backgroundName = DirectoryUtil.GetLocalNomadLogicAssetsDir() + "message_box.png";

            if (!File.Exists(backgroundName)) {
                throw new System.Exception("File does not exists: " + backgroundName);
            }

            var bitmap = FileUtil.LoadBitmap(backgroundName);

            pictureBox.Location = new Point(0,0);
            pictureBox.Image = bitmap;
            pictureBox.Width = bitmap.Width;
            pictureBox.Height = bitmap.Height;

            Size = pictureBox.Size;

            panel1.Location = new Point(0, 0);
            panel1.Size = Size;
            panel1.BorderStyle = BorderStyle.None;

            pictureBox.BringToFront();
            pictureBox.MouseDown += MouseDownHandler;


            _closeArea = new Rectangle(new Point(586, 0), new Size(76, 58));
        }

        public void ShowMessage(string message)
        {
            var label = new Label
            {
                BackColor = Color.White,
                Location = new Point(61, 33),
                Size = new Size(507, 90),
                Font = new Font("Arial", 15, FontStyle.Bold),
                ForeColor = Color.FromArgb(128, 130, 133),
                TextAlign = ContentAlignment.MiddleCenter,
                Text = message
            };

            Controls.Add(label);

            label.BringToFront();

            ShowDialog();
        }

        public void MouseDownHandler(object sender, MouseEventArgs e)
        {
            if (InArea(e.X, e.Y, _closeArea))
            {
                Close();
            }
        }

        public void MouseDownHandler2(object sender, MouseEventArgs e)
        {
            Close();
        }

        private static bool InArea(int x, int y, Rectangle area)
        {
            if (x <= area.Location.X || x >= area.Location.X + area.Width) return false;
            return y < area.Location.Y + area.Height && y > area.Location.Y;
        }
    }
}