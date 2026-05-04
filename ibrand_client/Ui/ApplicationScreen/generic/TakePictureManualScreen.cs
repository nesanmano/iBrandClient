using System.Drawing;
using System.Windows.Forms;
using NLog;
using NomadLogic.Common.Ui.Dialog;

// ReSharper disable VirtualMemberCallInContructor

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic
{
    /// <summary>
    /// Classe de base pour les prises de photo. Contient les éléments de base pour l'affichage ainsi que les méthodes de callback pour
    /// les Événements de caméra.
    /// Il n'y a aucun délai pour la prise de photo. Si on veut un délai d'activation, il faut plutôt utiliser la classe TakePictureAutoScreenAbstract et ses enfants.
    /// </summary>
    public class TakePictureManualScreen : TakePictureScreenAbstract
    {
        // ReSharper disable once UnusedMember.Local
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        /// Zone pour déclencher la prise de photo.
        private readonly Rectangle? _takePictureRectangle;
        /// Zone pour revenir en arrière.
        private readonly Rectangle? _backRectangle;
        /// Empêche l'activation de la caméra quand on prend une image ou que l'on fait une transformation.
        protected bool _blockInputs;

        /// <summary>
        /// Construteur par défaut.
        /// </summary>
        /// <param name="sc">Paramètres d'écrans globaux.</param>
        /// <param name="backgroundImgEnSrc">Fond d'image anglais.</param>
        /// <param name="backgroundImgFrSrc">Fond d'image français.</param>
        /// <param name="previewRectangle">Rectangle pour la zone de prévisualisation. Si null, on n'utilise pas le preview.</param>
        /// <param name="takePictureRectangle">Zone pour provoquer la prise de photo.</param>
        /// <param name="backRectangleRectangle">ZOne pour revenir en arrière.</param>
        public TakePictureManualScreen(ScreenConfig sc, string backgroundImgEnSrc, string backgroundImgFrSrc,
            Rectangle? previewRectangle = null, Rectangle? takePictureRectangle = null, Rectangle? backRectangleRectangle = null)
            : base(backgroundImgEnSrc, backgroundImgFrSrc, sc, previewRectangle)
        {
            _takePictureRectangle = takePictureRectangle;
            _backRectangle = backRectangleRectangle;
        }

        /// <inheritdoc/>
        protected override bool OnMouseDownHandler(int x, int y)
        {
            if (!base.OnMouseDownHandler(x, y)) return false;
            if (_blockInputs) return false;

            if (InArea(x, y, _backRectangle))
            {
                Back();
                return false;
            }

            if (InArea(x, y, _takePictureRectangle))
            {
                _blockInputs = true;

                if (_canonManager.IsLiveView() && !DisableLiveView)
                {
                    _canonManager.StopLiveView();
                }

                SetPreviewPicture(_panelPreview, _safeBitmaps, _freezeBitmap);

                if (_canonManager.TakePicture()) return false;

                // Affiche message de focus.
                MsgBox.Show(Resources.iBrand.UI_TakePicture_CouldNotTakePicture, Resources.iBrand.AppName, MessageBoxButtons.OK);

                _blockInputs = false;
                if (_panelPreview != null) { _canonManager.StartLiveView(); }
            }

            return true;
        }

        /// <inheritdoc/>
        protected override void SetActivatedCustom()
        {
            _blockInputs = false;
        }

        /// <inheritdoc/>
        protected override void OnFileSavedOnHostDo(string filePath)
        {
            _picturesTaken.Add(filePath);
            SetPreviewPicture(_panelPreview, _safeBitmaps, null);
            _blockInputs = true;
            
            LaunchTransformationSequence(_picturesTaken);
        }


        /// <inheritdoc/>
        protected override void OnUpdatePictureDo(Image image, bool force)
        {
            HandlePreviewImage(image);
        }
    }
}
