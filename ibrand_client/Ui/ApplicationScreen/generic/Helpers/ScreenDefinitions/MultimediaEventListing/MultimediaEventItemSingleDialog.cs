using System;
using System.IO;
using System.Windows.Forms;
using JetBrains.Annotations;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Win32;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers.ScreenDefinitions.MultimediaEventListing
{
    /// <summary>
    /// Affiche un seul élément sur une forme, pour maximiser sa présentation. On peut sélectionner l'élément en cliquant dessus ou
    /// </summary>
    public partial class MultimediaEventItemSingleDialog : Form
    {
        private readonly MultimediaEventGalleryRowItem _item;

        private readonly MultimediaEventItemPresentationDetailSingle _details;
        private readonly bool _initialSelection;

        public MultimediaEventItemSingleDialog([NotNull] MultimediaEventGalleryRowItem item,
            [NotNull] MultimediaEventItemPresentationDetailSingle presentationDataDetail)
        {
            InitializeComponent();
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.DoubleBuffer, true);
            FormBorderStyle =  FormBorderStyle.None;
            WindowState = FormWindowState.Maximized;
            MaximizeBox = false;

            _item = item;
            _initialSelection = item.Selected;
            _details = presentationDataDetail;

            DialogResult = DialogResult.None;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            BackgroundImage = FileUtil.LoadImage(Path.Combine(_details.AssetsDirectory, _details.BackgroundImageFileName));

            var img = FileUtil.LoadImage(Path.Combine(_details.AssetsDirectory, _details.QuitButtonImageFileName));

            var exitButton = new PictureBox
            {
                Location = _details.QuitButtonPosition.Location,
                SizeMode = PictureBoxSizeMode.AutoSize,
                BackColor = _details.QuitButtonBackColor,
                Image = img
            };

            exitButton.MouseClick += (sender, args) =>
            {
                DialogResult = _initialSelection != _item.Selected ? DialogResult.OK : DialogResult.None;
                Close();
            };
            Controls.Add(exitButton);

            this.SafeCall(() => {
                var zone = _details.PresentationZone;
                var bmp = _item.GenerateThumbnailImage(zone.Size, _details);
                
                var imagePbox = new PictureBox
                {
                    Location = zone.Location,
                    Size = zone.Size,
                    SizeMode = PictureBoxSizeMode.Zoom,
                    Image = bmp
                };

                imagePbox.MouseClick += (sender, args) =>
                {
                    _item.Selected = !_item.Selected;
                    var newBmp = _item.GenerateThumbnailImage(zone.Size, _details);
                    imagePbox.Image = newBmp;
                };

                Controls.Add(imagePbox);
            });
        }
    }
}
