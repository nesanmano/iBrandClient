using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using JetBrains.Annotations;
using NLog;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic
{
    /// <summary>
    /// Écran qui ne fait que terminer le questionnaire en s'affichant. Si on touche l'écran ou si le timer termine. On ferme le questionnaire
    /// en sauvant le répondant. En somme, on a pas le choix de terminer si on tombe sur ce genre de page.
    /// </summary>
    [PublicAPI]
    [Obsolete]
    public class ThanksEndScreen : AbstractScreen
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private readonly Rectangle? _nextArea;
        private bool _earlyPrintSent;

        /// <summary>
        /// Constrcuteur principal
        /// </summary>
        /// <param name="backgroundImgEnSrc">Image de fond anglais.</param>
        /// <param name="sc">Paramètres globaux d'écrans.</param>
        /// <param name="backgroundImgFrSrc">Image de fond français, optionel.</param>
        public ThanksEndScreen(string backgroundImgEnSrc, ScreenConfig sc, string backgroundImgFrSrc = null)
            : base(backgroundImgEnSrc, backgroundImgFrSrc ?? backgroundImgEnSrc, sc)
        {
            _nextArea = new Rectangle(0, 0, PictureBox.Width, PictureBox.Height);
        }

        /// <summary>
        /// Indique si on doit tenter une impression de fichier dès que l'on arrive sur cette page.
        /// Si le numéro de question pour l'incrémentation du nombre d'impression existe, il sera aussi incrémenté.
        /// </summary>
        public bool ForcePrintOnActivation { get; set; }

        protected override bool OnMouseDownHandler(int x, int y)
        {
            if (!base.OnMouseDownHandler(x, y)) return false;

            if (InArea(x, y, _nextArea))
            {
                End();
                return false;
            }

            return true;
        }

        /// <inheritdoc/>
        protected override void OnTimerEvent(object sender, EventArgs e)
        {
            End(); // On ne renvoi pas au premier écran, on termine.
        }

        /// <inheritdoc/>
        public override void SetActivated()
        {
            base.SetActivated();

            if (!ForcePrintOnActivation || _earlyPrintSent) return;
            _earlyPrintSent = true;
            log.Trace("Begin Print sequence.");
            var t = new Thread(() =>
            {
                var watch = new Stopwatch();
                watch.Start();

                try
                {
                    if (_screenConfig.QuestionPictureToPrintNo == null)
                    {
                        log.Trace("No print question given. Aborting print sequence.");
                    }
                    else
                    {
                        if (!PrintAnswerFile(_screenConfig.QuestionPictureToPrintNo.Value, 1))
                        {
                            log.Trace("Could not send to printer spool. Aborting print sequence.");
                        }
                        else if (_screenConfig.QuestionNumberOfPrintNo != null)
                        {
                            log.Trace("Incrementing Print number question.");
                            IncrementEventInQuestionnaire(_screenConfig.QuestionNumberOfPrintNo.Value, 1);
                        }
                    }
                }
                catch (System.Exception e)
                {
                    log.Error(e, "Error during print sequence.");
                }

                watch.Stop();
                if (watch.ElapsedMilliseconds < MINIMUM_PRINT_WAIT_TIME)
                {
                    Thread.Sleep(MINIMUM_PRINT_WAIT_TIME - (int)watch.ElapsedMilliseconds);
                }
            });
            t.SetApartmentState(ApartmentState.STA);
            t.Start();
        }

        /// <inheritdoc/>
        public override void Recycle()
        {
            base.Recycle();
            
            _earlyPrintSent = false;
        }
    }
}
