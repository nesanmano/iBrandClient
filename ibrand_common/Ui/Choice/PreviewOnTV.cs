using System.Drawing;
using System.Windows.Forms;

namespace NomadLogic.Common.Ui.Choice
{
    public partial class PreviewOnTv : Form
    {
        public PreviewOnTv()
        {
            if (Screen.AllScreens.Length > 1)
            {
                Bounds = Screen.AllScreens[1].Bounds;
                StartPosition = FormStartPosition.Manual;
            }
            BackgroundImageLayout = ImageLayout.Zoom;
            
            InitializeComponent();

            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.DoubleBuffer, true);
        }

        public void ShowImage(Bitmap image)
        {
            BackgroundImage = image;
        }
    }
}
