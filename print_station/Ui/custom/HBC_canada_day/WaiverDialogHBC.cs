using System.Drawing;
using System.Windows.Forms;

namespace NomadLogic.PrintStation.Ui.custom.HBC_Canada_Day
{
    public partial class WaiverDialogHbc : Form
    {
        private readonly Rectangle _closeArea;

        public WaiverDialogHbc(Bitmap img)
        {
            InitializeComponent();

            if (img != null)
            {
                pictureBox.Image = img;
                pictureBox.Width = img.Width;
                pictureBox.Height = img.Height;
                Size = img.Size;
            }
            pictureBox.Location = new Point(0, 0);
            Location = new Point(0, 0);

            pictureBox.MouseClick += MouseDownHandler;

            _closeArea = new Rectangle(new Point(1180, 5), new Size(113, 94));
        }

        public void MouseDownHandler(object sender, MouseEventArgs e)
        {
            if (InArea(e.X, e.Y, _closeArea))
            {
                Close();
            }
        }

        protected bool InArea(int x, int y, Rectangle area)
        {
            if (x > area.Location.X && x < area.Location.X + area.Width)
                if (y < area.Location.Y + area.Height && y > area.Location.Y)
                    return true;

            return false;
        }
    }
}