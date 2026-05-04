using System;
using System.Collections.Generic;
using System.Drawing;
using JetBrains.Annotations;
using NLog;
using NomadLogic.Common.Util;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers;
using NomadLogic.Multimedia;
using Timer = System.Timers.Timer;

// ReSharper disable VirtualMemberCallInContructor

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic
{
    /// <summary>
    /// Classe de base pour les prises de photo avec un décompte automatique. Quand les instances sont activées, un décompte automatique est
    /// lancé et certains éléments visuels sont affichés selon le décompte, ainsi que certains événements (par exemple la prise de photo elle-même).
    /// En gros, cet écran ne requiert pas d'événements déclenché par l'usager et va automatiquement faire son traitement, incluant le passage vers
    /// la prochaine page une fois la ou les photos prises.
    /// </summary>
    public abstract class TakePictureAutoScreenAbstract : TakePictureScreenAbstract
    {
        // ReSharper disable once UnusedMember.Local
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        /// Minuteur pour la prise d'image
        protected readonly Timer _takePictureTimer;
        /// Temps, en secondes, pour le décompte entre chaque image.
        protected readonly int _countDownMax;
        /// Valeur du premier décompte, si nécessaire.
        protected readonly int _firstCountDown;
        /// Séquence d'image du décompte.
        [PublicAPI]
        protected AnimatedImageSequence _countSequence;

        /// <summary>
        /// Zone pour manuellement démarrer le compteur, au lieu de le faire automatiquement lors de l'activation de la page.
        /// </summary>
        public Rectangle? ManualStartZone { get; set; }

        /// <summary>
        /// Constrcuteur principal pour un écran de prise d'image automatique. Les images se prennent automatiquement après un décompte, affiché à l'écran.
        /// </summary>
        /// <param name="sc">Paramètre d'écrans globaux.</param>
        /// <param name="backgroundImgEnSrc">Image de fond anglais.</param>
        /// <param name="backgroundImgFrSrc">Image de fond français.</param>
        /// <param name="previewRectangle">Zone pour la prévisualisation.</param>
        /// <param name="countDownMax">Décompte en seconde entre chaque image à prendre. 5 par défaut.</param>
        /// <param name="firstCountDown">Valeur du premier décompte. Qui sera plus long (ou plus court) que les autres. Si null, on utilise la valeur des autres décomptes.</param>
        protected TakePictureAutoScreenAbstract(ScreenConfig sc, string backgroundImgEnSrc, string backgroundImgFrSrc,
            Rectangle? previewRectangle = null, int countDownMax = 5, int? firstCountDown = null)
            : base(backgroundImgEnSrc, backgroundImgFrSrc, sc, previewRectangle)
        {
            if (countDownMax < 1 || (firstCountDown.HasValue && firstCountDown.Value < 1))
            {
                //throw new System.Exception("Countdown cannot be less than 1.");
                return;
            }

            _firstCountDown = firstCountDown ?? countDownMax;
            _countDownMax = countDownMax;

            CountImageSequence = new AnimatedImageSequence
            {
                Loop = false,
                DrawLocation = previewRectangle == null ?
                    new Point((PictureBox.Width - 130) / 2, (PictureBox.Height - 70) / 2) :
                    new Point((PictureBox.Width - 130) / 2, previewRectangle.Value.Y + previewRectangle.Value.Height + 30),
                ImageGenerators = new List<IAnimatedImageSequenceGenerator>
                {
                    new AnimatedImageSequenceGeneratorNumbers
                    {
                        ProceduralImageTextInfo = new ProceduralImageTextInfo
                        {
                            BitmapSize = new Size(96, 102),
                            FontFamilly = FontFamily.GenericSerif.ToString(),
                            FontSize = 60,
                            FontStyle = FontStyle.Bold
                        },
                        MaxValue = Math.Max(_countDownMax, _firstCountDown),
                        ReverseOrder = true
                    },
                    new AnimatedImageSequenceGeneratorBasicFiles
                    {
                        FilePaths = new List<string> { DirectoryUtil.GetLocalPhotoBoothDefaultAssetsDir() + "flash.png" } // 96x102
                    }
                }
            };

            _takePictureTimer = new Timer
            {
                AutoReset = false
            }; //1 sec
            _takePictureTimer.Elapsed += (o, e) =>
            {
                CountImageSequence.StopSequence();
                OnCountdownElapsed();
            };
        }

        protected override bool OnMouseDownHandler(int x, int y)
        {
            if (!base.OnMouseDownHandler(x, y)) return false;

            if (InArea(x, y, ManualStartZone))
            {
                StartCountdown();
                return false;
            }

            return true;
        }

        /// <summary>
        /// Générateur d'images de décompte. Par défaut, génère autant d'images que le plus grand décompte
        /// </summary>
        [PublicAPI]
        public AnimatedImageSequence CountImageSequence{
            get => _countSequence;
            set {
                if (_countSequence != null)
                {
                    _countSequence.StopSequence();
                    _countSequence.UpdateDraw -= InvokeRefresh;
                    _countSequence.Dispose();
                    _countSequence = null;
                }

                _countSequence = value;
                _countSequence.UpdateDraw += InvokeRefresh;
            }
        }

        #region Overrides of TakePictureScreenAbstract

        /// <inheritdoc />
        protected override void InitBitmap()
        {
            base.InitBitmap();
            CountImageSequence?.CreateImages(_activeLg);
        }
                
        /// <inheritdoc />
        public override bool ChangeLanguage(string language)
        {
            var change = base.ChangeLanguage(language);
            if (!change) return false;

            CountImageSequence.ClearCurrentImages();
            CountImageSequence.CreateImages(_activeLg);

            return true;
        }

        protected override void OnPaintEvent(Graphics g)
        {
            base.OnPaintEvent(g);
            CountImageSequence?.Paint(g);
        }

        #endregion
        
        /// <inheritdoc/>
        protected override void SetActivatedCustom() {
            if (ManualStartZone == null)
            {
                StartCountdown();
            }

            if (_panelPreview != null)
            {
                _canonManager.StartLiveView();
            }
        }

        /// Lance l'équivalent du premier countdown.
        protected void StartCountdown() {

            if (_takePictureTimer == null) return;
            // Ceci est l'équivalent de la durée du premier CountDown, avec les informations de base.
            _takePictureTimer.Interval = CountImageSequence.Count * 1000; // Tient compte si on a un flash ou non.
            _takePictureTimer.Start();
            CountImageSequence.StartSequence();
        }
        
        /// <summary>
        /// Méthode à exécuter quand le décompte tombe à zéro. Les séquences et minuteurs sont stoppées lors de l'appel.
        /// </summary>
        protected abstract void OnCountdownElapsed();
        
    }
}
