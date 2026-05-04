using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Timers;
using System.Windows.Forms;
using JetBrains.Annotations;
using NLog;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Ui.NomadScreen;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers
{
    /// <summary>
    /// Classe pour initialiser et gérer une séquence d'image à afficher à l'écran.
    /// Une séquence est composée d'images similaires en dimensions, à un endroit précis dans l'écran.
    /// La séquence est lancée et stoppée via des méthodes simples, et gère à l'interne ses propres minuteurs.
    /// Une méthode de dessein peut être appellée pour dessiner le contenu au moment de l'appel, et on possède aussi
    /// une méthode de "callback" optionnelle quand le contenu à changé, permettant de mentionner à l'écran qu'il
    /// peut se mettre à jour.
    /// </summary>
    [PublicAPI]
    public class AnimatedImageSequence : IDisposable, IPaint
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private readonly System.Windows.Forms.Timer _countDownTimer;
        private readonly IList<Bitmap> _bitmaps = new List<Bitmap>();

        /// Délégué avec aucun paramètre.
        public delegate void NoParam();

        private int _currentIndex;

        /// Événement lancé pour indiquer que l'aspect visuel à changé.
        public NoParam UpdateDraw;

        private bool _active;

        /// <summary>
        /// Constructeur de base.
        /// </summary>
        public AnimatedImageSequence()
        {
            _countDownTimer = new System.Windows.Forms.Timer
            {
                Interval = 1000,
                Enabled = true
            }; //1 sec
            _countDownTimer.Tick += OnCountDownTimerEvent;
        }
        
        private void OnCountDownTimerEvent(object sender, EventArgs e)
        {

            if (_currentIndex + 1 >= _bitmaps.Count)
            {
                _currentIndex = 0;
                if (!Loop) { _countDownTimer.Stop(); }
            }
            else
            {
                _currentIndex++;
            }
            log.Trace("Timer event: new index => {0}", _currentIndex);

            UpdateDraw?.Invoke();
        }

        /// <summary>
        /// Indique si on doit boucler le séquence ou si elle stopppe d'elle-même.
        /// </summary>
        public bool Loop { get; set; }

        /// <summary>
        /// Obtient l'endroit d'où les images sont dessinées sur le canevas.
        /// </summary>
        public Point DrawLocation { get; set; } = new Point(0, 0);

        /// <summary>
        /// Vide la liste d'images actuelle.
        /// </summary>
        public void ClearCurrentImages()
        {
            foreach (var bmp in _bitmaps)
            {
                bmp?.Dispose();
            }
            _bitmaps.Clear();
        }

        /// <summary>
        /// Liste de générateurs d'images.
        /// </summary>
        public IList<IAnimatedImageSequenceGenerator> ImageGenerators { get; set; }

        /// Retourne le montant d'images présentement chargée dans la séquence.
        public int Count => _bitmaps.Count;

        /// <summary>
        /// Lance l'exécution de chaque générateur d'images pour nourrir la liste d'images.
        /// </summary>
        /// <param name="languageCode">Code de langue actuel.</param>
        /// <param name="questionnaire">Référence optionnelle vers le questionnaire.</param>
        /// <param name="respondent">Référence vers le répondant actuel.</param>
        public void CreateImages(string languageCode, Questionnaire questionnaire = null, IRespondent respondent = null)
        {
            if (ImageGenerators == null || !ImageGenerators.Any()) {
                throw new System.Exception("No image generators specified for instance od animated sequence.");
            }

            foreach (var gen in ImageGenerators) { gen.GenerateImages(_bitmaps, languageCode, questionnaire, respondent); }
            log.Trace("Sequence images created. Count={0}", _bitmaps.Count);
        }

        /// <summary>
        /// Démarre la séquence d'affichage. Ré-initialise son état à un état de départ.
        /// Si la séquence est déjà démarrée, ne fait rien.
        /// On peut couper une partie de la duration, ce qui est pratique si on veut afficher la même séquence mais avec une
        /// durée plus courte.
        /// S'il n'y a pas assez d'images initialisées pour le compte donné, on retourne immédiatement et le décompte stoppe
        /// immédiatement.
        /// </summary>
        /// <param name="durationCut">Durée, en secondes, coupé de la séquence.</param>
        public void StartSequence(int? durationCut = null)
        {
            if (_active) return;

            _currentIndex = durationCut ?? 0;

            if (_currentIndex >= _bitmaps.Count) return;
            log.Trace("Sequence index initialized to {0}", _currentIndex);

            _countDownTimer.Start();
            _active = true;
            UpdateDraw?.Invoke();
        }

        /// <summary>
        /// Stoppe la séquence.
        /// </summary>
        public void StopSequence()
        {
            _countDownTimer.Stop();
            _active = false;
            UpdateDraw?.Invoke();
            log.Trace("Sequence stopped.");
        }
        
        #region Implementation of IDisposable

        /// <inheritdoc />
        public void Dispose()
        {
            UpdateDraw = null;
            StopSequence();
            ClearCurrentImages();
        }

        #endregion

        public void Paint(Graphics g, bool debug = false)
        {
            if (_bitmaps == null || !_bitmaps.Any()) return;
            if (!(_bitmaps[_currentIndex] is Image bmp)) return;
            if (_active)
            {
                log.Trace("Drawing bmp {0}/{1}", _currentIndex, _bitmaps.Count);
                ScreenUtil.DrawImage(g, bmp, DrawLocation);
                if (debug) ScreenUtil.DrawDebugRectangle(g, ScreenUtil.DebugBluePen, DrawLocation, bmp.Size);
            }
            else if (debug) 
            {
                ScreenUtil.DrawDebugInactiveRectangle(g, ScreenUtil.DebugRedPen, DrawLocation, bmp.Size);
            }
        }
    }
}