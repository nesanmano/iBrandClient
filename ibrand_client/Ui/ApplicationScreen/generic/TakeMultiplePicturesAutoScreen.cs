using System;
using System.Drawing;
using System.Windows.Forms;
using JetBrains.Annotations;
using NLog;
using NomadLogic.Common.Ui.Dialog;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers;
using NomadLogic.iBrandDesktop.Util;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic
{
    /// <summary>
    /// Cet écran prend plusieures images consécutivement, chacune après un temps de décompre fixe. Une fois le nombre d'image voulu saisi, on peut traiter
    /// immédiatement les images pour leur usage final, que ce soit pour l'impression ou la création d'un GIF ou d'un groupement d'image (bundle).
    /// Pendant le temps de traitement des images, on peut afficher une séquence d'image pour faire patienter l'usager. Cela permet d'indiquer qu'il y a traitement,
    /// pour ne pas créer l'illusion que l'application a plantée.
    /// </summary>
    public class TakeMultiplePicturesAutoScreen : TakePictureAutoScreenAbstract
    {
        // ReSharper disable once UnusedMember.Local
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        
        /// <summary>
        /// On peut utiliser un générateur d'image à la place d'un dictionnaire, dans le cas où on veut générer les images.
        /// </summary>
        [PublicAPI]
        public IImageGenerator FirstCountdownPresentationImageGenerator { get; set; }

        /// <summary>
        /// Si on utilise une image de présentation lors du premier countdown, on peut donner sa location absolue ici. Sinon, cette position sera calculée comme
        /// les autres. C'est à dire centrée et sous la zone de prévisualisation.
        /// </summary>
        [PublicAPI]
        public Point? FirstCountdownPresentationImageLocation { get; set; }

        private int _nbrPictureToTake = 4;

        /// Indique que le premier countdown est fait.
        [PublicAPI]
        protected bool _firstTimerDone;

        private Bitmap _firstCountDownImage;

        /// <summary>
        /// Constructeur complet.
        /// </summary>
        /// <param name="sc">Paramètre d'écrans globaux.</param>
        /// <param name="backgroundImgEnSrc">Image de fond anglais.</param>
        /// <param name="backgroundImgFrSrc">Image de fond français.</param>
        /// <param name="previewZone">Zone pour la prévisualisation.</param>
        /// <param name="countDownMax">Décompte en seconde entre chaque image à prendre. 5 par défaut.</param>
        /// <param name="firstCountDown">Valeur du premier décompte. Qui sera plus long (ou plus court) que les autres. Si null, on utilise la valeur des autres décomptes.</param>
        public TakeMultiplePicturesAutoScreen(ScreenConfig sc, string backgroundImgEnSrc, string backgroundImgFrSrc,
            Rectangle? previewZone, int countDownMax = 5, int? firstCountDown = null)
            : base(sc, backgroundImgEnSrc, backgroundImgFrSrc, previewZone, countDownMax, firstCountDown) { }

        /// <summary>
        /// Indique le nombre d'image à prendre. Par défaut, 4, minimum 2. Toute valeur plus petite sera changée à 2.
        /// </summary>
        [PublicAPI]
        public int NumberOfPictureToTake
        {
            get => _nbrPictureToTake;
            set => _nbrPictureToTake = value < 2 ? 2 : value;
        }

        /// <inheritdoc />
        protected override void InitBitmap()
        {
            base.InitBitmap();

            InitTopImage();

            if (FirstCountdownPresentationImageLocation != null) return;
            // ReSharper disable once MergeConditionalExpression
            var h = _panelPreview != null
                        ? _panelPreview.Location.Y + _panelPreview.Size.Height + 30
                        : 100;
            FirstCountdownPresentationImageLocation = new Point((PictureBox.Width - _firstCountDownImage?.Width ?? 0) / 2, h);
        }

        /// <inheritdoc />
        protected override void OnPaintEvent(Graphics g)
        {
            base.OnPaintEvent(g);

            if (_firstTimerDone) return;
            DrawImage(g, _firstCountDownImage, FirstCountdownPresentationImageLocation);
        }

        private void InitTopImage()
        {
            _firstCountDownImage?.Dispose();
            _firstCountDownImage = FirstCountdownPresentationImageGenerator?.GenerateImage(_activeLg);
        }

        /// <inheritdoc />
        public override bool ChangeLanguage(string language)
        {
            var change = base.ChangeLanguage(language);

            if (!change) return false;

            InitTopImage();
            return true;
        }

        /// <inheritdoc />
        protected override void SetActivatedCustom()
        {
            base.SetActivatedCustom();

            _firstTimerDone = false;
        }

        /// <inheritdoc />
        public override void Recycle()
        {
            base.Recycle();

            _firstTimerDone = false;
        }

        /// <inheritdoc />
        protected override void OnCountdownElapsed()
        {
            if (!_activated) return;

            _firstTimerDone = true;
            _takePictureTimer.Stop();
            CountImageSequence.StopSequence();
            SetPreviewPicture(_panelPreview, _safeBitmaps, _freezeBitmap);

            if (_canonManager.TakePicture()) return;

            // Affiche message de focus.
            MsgBox.Show(Resources.iBrand.UI_TakePicture_CouldNotTakePicture, Resources.iBrand.AppName, MessageBoxButtons.OK);

            // En cas d'erreur lors de la prise d'image.
            InitSecondaryCountdown();
        }

        private void InitSecondaryCountdown()
        {
            if (_panelPreview != null)
            {
                _canonManager.StartLiveView();
            }

            var skipTime = Math.Abs(_firstCountDown - _countDownMax);

            _takePictureTimer.Interval = (CountImageSequence.Count - skipTime) * 1000;
            _takePictureTimer.Start();
            CountImageSequence.StartSequence(skipTime);
            Refresh();
        }

        /// <inheritdoc />
        protected override void OnFileSavedOnHostDo(string filePath)
        {
            try
            {
                if (!_activated) return;

                _picturesTaken.Add(filePath);
                
                if (_picturesTaken.Count < NumberOfPictureToTake) { InitSecondaryCountdown(); }
                else
                {
                    LaunchTransformationSequence(_picturesTaken);
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(WorkingArea.Container, ex.Message, @"iBrand(tm)", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <inheritdoc/>
        protected override void OnUpdatePictureDo(Image image, bool force)
        {
            HandlePreviewImage(image);
        }
    }
}
