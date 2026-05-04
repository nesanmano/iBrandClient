using System.Drawing;
using System.Windows.Forms;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Win32;

namespace NomadLogic.Common.Ui.Forms
{
    /// <summary>
    /// Affiche une image dans un dialogue, habituellement pour afficher un renseignement et le fermer facilement.
    /// </summary>
    public partial class WaiverImageDialog : Form
    {
        readonly Rectangle _closeArea;

        /// <summary>
        /// Constructreur principal.
        /// </summary>
        /// <param name="img">L'image à afficher.</param>
        public WaiverImageDialog(Image img)
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
        
        /// <summary>
        /// Pour les événements souris.
        /// </summary>
        /// <param name="sender">L'envoyeur</param>
        /// <param name="e">Les paramètres d'événement.</param>
        public void MouseDownHandler(object sender, MouseEventArgs e)
        {
            if (FormUtil.InArea(e.X, e.Y, _closeArea))
            {
                Close();
                if (pictureBox.Image != null)
                    pictureBox.Image.Dispose();
            }
        }
    }
}
