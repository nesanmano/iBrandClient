using System;
using System.IO;
using System.Windows.Forms;
using JetBrains.Annotations;
using NomadLogic.Common.Util;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers.ScreenDefinitions.MultimediaEventListing
{
    /// <summary>
    /// Affiche sur une forme tous les éléments simultanément, et permet une sélection individuelle de chaque élément.
    /// </summary>
    public partial class MultimediaEventItemSelectorDialog : Form
    {
        private readonly MultimediaEventGalleryRowItem[] _images;

        private readonly MultimediaEventItemPresentationDetailSelector _details;
        //private static readonly Logger log = LogManager.GetCurrentClassLogger();

        public MultimediaEventItemSelectorDialog([NotNull] MultimediaEventGalleryRowItem[] images,
            [NotNull] MultimediaEventItemPresentationDetailSelector presentationDataDetail)
        {
            InitializeComponent();
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.DoubleBuffer, true);
            FormBorderStyle =  FormBorderStyle.None;
            WindowState = FormWindowState.Maximized;
            MaximizeBox = false;

            _images = images;
            _details = presentationDataDetail;

            DialogResult = DialogResult.None;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            var img = FileUtil.LoadImage(Path.Combine(_details.AssetsDirectory, _details.QuitButtonImageFileName));

            var exitButton = new PictureBox
            {
                Location = _details.QuitButtonPosition.Location, 
                SizeMode = PictureBoxSizeMode.AutoSize, 
                Image = img
            };
            exitButton.MouseClick += (sender, args) =>
            {
                Close();
            };
            Controls.Add(exitButton);
            
            for(var i = 0; i < _images.Length; i++)
            {
                var megi = _images[i];
                var zone = _details.ZoneList[i];
                
                var bmp = megi.GenerateThumbnailImage(zone.Size, _details);
                
                var imagePbox = new PictureBox
                {
                    Location = zone.Location,
                    Size = zone.Size,
                    SizeMode = PictureBoxSizeMode.Zoom,
                    Image = bmp
                };

                imagePbox.MouseClick += (sender, args) =>
                {
                    megi.Selected = !megi.Selected;
                    var newBmp = megi.GenerateThumbnailImage(zone.Size, _details);
                    imagePbox.Image = newBmp;
                    DialogResult = DialogResult.OK; // Indique qu'il faut regacharger la ligne d'image
                };

                Controls.Add(imagePbox);
            }
        }
    }
}
