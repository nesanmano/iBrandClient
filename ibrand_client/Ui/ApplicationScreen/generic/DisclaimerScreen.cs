using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using NomadLogic.Common.Ui;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic
{
    /// <summary>
    /// Cet écran affiche habituellement un gros morceau de texte agrémenté de un ou plus option de "opt-ins", ou
    /// des options "on-off" pour l'usager. Certaines de ces options peuvent être obligatoires, auquel cas on ne peut
    /// passer au prochain écran sans les activer.
    /// <br/>Si on bloque ici, il faut avoir une option de cancellation pour tout annuler et revenir au début du questionnaire.
    /// </summary>
    [Obsolete]
    public class DisclaimerScreen : AbstractScreen
    {
        private readonly List<ScreenCheckOption> _zones = new List<ScreenCheckOption>();
        private readonly Rectangle _nextArea;
        private readonly Rectangle? _cancelArea;
        private readonly Bitmap _checkmark;

        /// <summary>
        /// Constructeur principal.
        /// </summary>
        /// <param name="backgroundImgEnSrc">Image de fond anglais.</param>
        /// <param name="backgroundImgFrSrc">Image de fond Français.</param>
        /// <param name="sc">Configuration globale des écrans.</param>
        /// <param name="nextArea">Zone pour passer à l'écran suivant</param>
        /// <param name="cancelArea">Zone pour canceller le tout.</param>
        /// <param name="zones">Optionnel. Définition des zones de checkbox.</param>
        public DisclaimerScreen(string backgroundImgEnSrc, string backgroundImgFrSrc, ScreenConfig sc, Rectangle nextArea, Rectangle? cancelArea, IList<ScreenCheckOption> zones = null)
            : base(backgroundImgEnSrc, backgroundImgFrSrc, sc)
        {
            if (zones != null && zones.Any()) {
                _checkmark = GetNoneLanguageBitmap("checkmark.png");
                _zones.AddRange(zones);
            }
            _nextArea = nextArea;
            _cancelArea = cancelArea;
        }

        protected override void OnPaintEvent(Graphics g)
        {
            base.OnPaintEvent(g);
            PaintCheckBoxList(g, _zones, _checkmark);
        }
        
        /// <inheritdoc/>
        public override void Recycle()
        {
            base.Recycle();
            RecycleCheckBoxs(_zones);
        }

        /// <summary>
        /// Renvoie en arrière au lieu d'en avant.
        /// </summary>
        public bool GoBackInsteadOfNext { get; set; }

        /// <inheritdoc/>
        protected override bool OnMouseDownHandler(int x, int y)
        {
            if (!base.OnMouseDownHandler(x, y)) return false;

            if (InArea(x, y, _nextArea))
            {
                if (!AllMandatoryChecks(_zones))
                {
                    var mBox = new MessageDialog();
                    mBox.ShowMessage(Resources.iBrand.NotAllMandatoryCheckEnabled);
                    return false;
                }

                if (GoBackInsteadOfNext)
                {
                    Back();
                }
                else
                {
                    Next();
                }
                return false;
            }

            if (InArea(x, y, _cancelArea))
            {
                FirstScreen();
                return false;
            }

            // ReSharper disable once ForCanBeConvertedToForeach
            for(var i = 0; i < _zones.Count; i++)
            {
                var zone = _zones[i];
                if (!InArea(x, y, zone.Position)) continue;
                zone.Toggle();
                zone.SaveToAllModels(GetCurrentQuestionnaire(), zone.AnswerState);
                Refresh();
                break; // Normalement, les zones ne "overlap" pas...
            }

            return true;
        }
    }
}
