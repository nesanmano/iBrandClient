using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using NLog;
using NomadLogic.Common.Util;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers;
using NomadLogic.Multimedia;

#pragma warning disable 618

// ReSharper disable VirtualMemberCallInContructor

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic
{
    /// <summary>
    /// Classe de base pour les prises de photo. Contient les éléments de base pour l'affichage ainsi que les méthodes de callback pour
    /// les Évnements de caméra.
    /// </summary>
    public class TakeMultiplePicturesManualScreen : TakePictureManualScreen
    {
        // ReSharper disable once UnusedMember.Local
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private readonly IList<Bitmap> _countBitmaps = new List<Bitmap>();

        /// <summary>
        /// Construteur par défaut.
        /// </summary>
        /// <param name="sc">Paramètres d'écrans globaux.</param>
        /// <param name="backgroundImgEnSrc">Fond d'image anglais.</param>
        /// <param name="backgroundImgFrSrc">Fond d'image français.</param>
        /// <param name="previewRectangle">Rectangle pour la zone de prévisualisation. Si null, on n'utilise pas le preview.</param>
        /// <param name="takePictureRectangle">Zone pour provoquer la prise de photo.</param>
        /// <param name="backRectangle">Zone pour revenir en arrière.</param>
        public TakeMultiplePicturesManualScreen(ScreenConfig sc, string backgroundImgEnSrc, string backgroundImgFrSrc,
                                                Rectangle? previewRectangle = null, Rectangle? takePictureRectangle = null, Rectangle? backRectangle = null)
            : base(sc, backgroundImgEnSrc, backgroundImgFrSrc, previewRectangle, takePictureRectangle, backRectangle)
        {
            CountImageGenerator = new AnimatedImageSequenceGeneratorNumbers
            {
                ProceduralImageTextInfo = new ProceduralImageTextInfo
                {
                    BitmapSize = new Size(130, 70),
                    Color = Color.White,
                    FontFamilly = FontFamily.GenericSerif.ToString(),
                    FontSize = 60,
                    FontStyle = FontStyle.Bold
                },
                MaxValue = 4
            };
        }

        #region Overrides of TakePictureScreenAbstract

        /// <inheritdoc />
        protected override void InitBitmap()
        {
            base.InitBitmap();

            if (CountImagePosition == null) return;
            CountImageGenerator?.GenerateImages(_countBitmaps, _activeLg);
        }

        protected override void OnPaintEvent(Graphics g)
        {
            base.OnPaintEvent(g);

            if (_picturesTaken.Any() && _countBitmaps.Count >= _picturesTaken.Count)
            {
                DrawImage(g, _countBitmaps[_picturesTaken.Count - 1], CountImagePosition);
            }
        }

        /// <inheritdoc />
        public override bool ChangeLanguage(string language)
        {
            var change = base.ChangeLanguage(language);
            if (!change) return false;

            foreach (var bmp in _countBitmaps)
            {
                bmp.Dispose();
            }
            _countBitmaps.Clear();

            if (CountImagePosition == null) return true;
            CountImageGenerator?.GenerateImages(_countBitmaps, _activeLg);

            return true;
        }

        #endregion

        /// <summary>
        /// Location où afficher les images qui représentent le compte d'image prises. Le compteur n'apparaît pas si cette valeur est nulle lors de l'initialisation.
        /// </summary>
        public Point? CountImagePosition { get; set; }

        /// <summary>
        /// Générateur de la séquence d'images pour le nombre d'images prises. Si null, on n'affiche pas le compteur.
        /// </summary>
        public IAnimatedImageSequenceGenerator CountImageGenerator { get; set; }

        /// <summary>
        /// Nombre d'images à saisir. Par défaut, 4.
        /// </summary>
        public int NbrPicturesToTake { get; set; } = 4;

        /// <inheritdoc/>
        protected override void OnFileSavedOnHostDo(string filePath)
        {
            _blockInputs = false;
            _picturesTaken.Add(filePath);

            if (_picturesTaken.Count >= NbrPicturesToTake)
            {
                Refresh();
                _blockInputs = true;
                LaunchTransformationSequence(_picturesTaken);
            }
            else
            {
                if (_panelPreview != null && !CanonCameraManager.GetInstance().IsLiveView())
                    CanonCameraManager.GetInstance().StartLiveView();
            }

            Refresh();
        }
    }
}
