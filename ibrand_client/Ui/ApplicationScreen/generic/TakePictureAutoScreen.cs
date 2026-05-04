using System.Drawing;
using System.IO;
using System.Windows.Forms;
using NLog;
using NomadLogic.Common.Ui.Dialog;
using NomadLogic.Common.Util.Win32;

// ReSharper disable VirtualMemberCallInContructor

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic
{
    /// <summary>
    /// Prend une seule image de façon automatique.
    /// </summary>
    public class TakePictureAutoScreen : TakePictureAutoScreenAbstract
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        
        /// <summary>
        /// Constructeur complet.
        /// </summary>
        /// <param name="sc">Paramètre d'écrans globaux.</param>
        /// <param name="backgroundImgEnSrc">Image de fond anglais.</param>
        /// <param name="backgroundImgFrSrc">Image de fond français.</param>
        /// <param name="previewZone">Zone pour la prévisualisation.</param>
        /// <param name="countDownMax">Décompte en seconde entre chaque image à prendre. 5 par défaut.</param>
        /// <param name="firstCountDown">Valeur du premier décompte. Qui sera plus long (ou plus court) que les autres. Si null, on utilise la valeur des autres décomptes.</param>
        public TakePictureAutoScreen(ScreenConfig sc, string backgroundImgEnSrc, string backgroundImgFrSrc,
            Rectangle? previewZone, int countDownMax = 5, int? firstCountDown = null)
            : base(sc, backgroundImgEnSrc, backgroundImgFrSrc, previewZone, countDownMax, firstCountDown) { }
        
        /// <inheritdoc/>
        protected override void OnCountdownElapsed()
        {
            if (_panelPreview != null)
            {
                if (_canonManager.IsLiveView()) _canonManager.StopLiveView();

                SetPreviewPicture(_panelPreview, _safeBitmaps, _freezeBitmap);
                this.SafeCall(() => { _panelPreview.Refresh(); });
            }

            if (_canonManager.TakePicture()) return;

            // Affiche message de focus.
            MsgBox.Show(Resources.iBrand.UI_TakePicture_CouldNotTakePicture, Resources.iBrand.AppName, MessageBoxButtons.OK);

            // Si ça n'a pas marché.
            StartCountdown();
        }

        /// <inheritdoc/>
        protected override void OnFileSavedOnHostDo(string filePath) {
            if (!File.Exists(filePath)) {
                log.Warn("No valid file was returned.");
                return;
            }

            _picturesTaken.Add(filePath);

            LaunchTransformationSequence(_picturesTaken);
        }

        /// <inheritdoc/>
        protected override void OnUpdatePictureDo(Image image, bool force)
        {
            HandlePreviewImage(image);
        }
        
    }
}
