using System;
using System.Drawing;
using System.Windows.Forms;

namespace NomadLogic.Common.Ui
{
    /// <summary>
    /// Affiche une image dans une forme, au maximum de résolution possible.
    /// </summary>
    public sealed partial class PreviewDialog : Form
    {
        /// <summary>
        /// Affiche une image. Plusieurs modes différents selon les arguments
        /// </summary>
        /// <param name="picture">L'image à afficher.</param>
        /// <param name="bkgColor">Coulkeur de remplissage. Par défaut, blanc.</param>
        /// <param name="fullScreen">Force le dialogue à être plein écran, sans possibilité de redimensionnement.</param>
        /// <param name="background">Si donnée, on met cette image en fond et l'image principale sera une portion de cette dernière. Si utilisé, 'pictureZone' ne doit pas être nul.</param>
        /// <param name="pictureZone">La zone où afficher l'image principale, au besoin.</param>
        public PreviewDialog(Image picture, Color? bkgColor = null, bool fullScreen = false, Image background = null, Rectangle? pictureZone = null)
        {
            var baseColor = bkgColor ?? Color.White;
            InitializeComponent();
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.DoubleBuffer, true);
            BackgroundImageLayout = ImageLayout.Zoom;
            BackColor = baseColor;

            if (background != null && pictureZone != null)
            {
                BackgroundImage = background;
                var pic = pictureZone.Value;
                var pb = new PictureBox
                {
                    BackColor = baseColor,
                    Location = pic.Location,
                    Size = pic.Size,
                    Image = picture,
                    SizeMode = PictureBoxSizeMode.Normal,
                    Parent = this
                };
                if (!picture.Size.Equals(pic.Size))
                {
                    pb.SizeMode = PictureBoxSizeMode.Zoom;
                }
                pb.MouseClick += (sender, args) => Close();
                Controls.Add(pb);
            }
            else
            {
                BackgroundImage = picture;
            }

            if (!fullScreen)
            {
                return;
            }
            FormBorderStyle =  FormBorderStyle.None;
            WindowState = FormWindowState.Maximized;
            MaximizeBox = false;
            MouseClick += (sender, args) => Close();
        }

        /// <inheritdoc />
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            StartPosition = FormStartPosition.CenterScreen;
        }
    }
}