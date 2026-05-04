using System;
using System.Drawing;
using System.Windows.Forms;
using NLog;
using NomadLogic.Common;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Ui;
using NomadLogic.Common.Ui.NomadScreen;
using NomadLogic.PrintStation.Ui.generic;

namespace NomadLogic.PrintStation.Ui.custom.OshKosh
{
    /// <summary>
    /// Écran sélection de la langue.
    /// </summary>
    public partial class LanguageSelectionScreen : AbstractPrintStationScreen, ILanguageScreenInterface
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Événement de modification de langue de l'usager.
        /// </summary>
        public event LanguageSelectionEventHandler OnLanguageSelection;

        public void LanguageSelection(string lg)
        {
            OnLanguageSelection?.Invoke(lg);
        }

        public Func<IRespondent> ObtainRespondentMethod { get; set; }
        public int? QuestionLanguageNo { get; set; }
        public int QuestionLanguageChoiceNoFr { get; set; }
        public int QuestionLanguageChoiceNoEn { get; set; }
        public bool IsQuestionLanguageRadio { get; set; }

        private Rectangle _clickHereArea;
        private Rectangle _clickIciArea;
        private readonly WebBrowserScreenPrintStation _webScreen;

        /// <summary>
        /// Constructeur par défaut.
        /// </summary>
        /// <param name="backgroundImgSrc">Fichier d'image de fond.</param>
        /// <param name="sc">AbstractConfiguration de l'écran</param>
        /// <param name="webScreen">L'écran web qui vient après cet écran.</param>
        public LanguageSelectionScreen(String backgroundImgSrc, PrintStationScreenConfig sc, WebBrowserScreenPrintStation webScreen)
            : base(backgroundImgSrc, backgroundImgSrc, sc)
        {
            //PictureBox.MouseDown += MouseDownHandler;

            _webScreen = webScreen;

            _clickIciArea = new Rectangle(new Point(1280 / 2, 0), new Size(1280 / 2, 800));
            _clickHereArea = new Rectangle(new Point(0, 0), new Size(1280 / 2, 800));
        }

        ///// <summary>
        ///// Pour les événements souris
        ///// </summary>
        ///// <param name="sender">Envoyeur</param>
        ///// <param name="e">L'événement</param>
        //protected override void MouseDownHandler(object sender, MouseEventArgs e)
        //{
        //    base.MouseDownHandler(sender, e);

        //    if (InHereArea(e.X, e.Y))
        //    {
        //        if (SaveAnswerToQuestion(2, 1, 2))
        //        {
        //            OnLanguageSelection?.Invoke(Constants.EN);
        //            _webScreen.NaviguateToUrl("https://pages.em.cartersoshkosh.ca/ice-cream/english/");
        //            Next();
        //        }
        //        else
        //        {
        //            log.Error("Could not save answer to questionnaire.");
        //        }
        //    }
        //    else if (InIciArea(e.X, e.Y))
        //    {
        //        if (SaveAnswerToQuestion(2, 1, 1))
        //        {
        //            if (OnLanguageSelection != null)
        //                OnLanguageSelection(Constants.FR);
        //            _webScreen.NaviguateToUrl("https://pages.em.cartersoshkosh.ca/ice-cream/french/");
        //            Next();
        //        }
        //        else
        //        {
        //            log.Error("Could not save answer to questionnaire.");
        //        }
        //    }
        //}

        /// <summary>
        /// Évalue si un click se trouve dans la zone française.
        /// </summary>
        /// <param name="x">Position X du clic</param>
        /// <param name="y">Position Y du clic</param>
        /// <returns>TRUE si la position absolue est dans la zone.</returns>
        protected bool InHereArea(int x, int y)
        {
            if (x > _clickHereArea.Location.X && x < _clickHereArea.Location.X + _clickHereArea.Width)
                if (y < _clickHereArea.Location.Y + _clickHereArea.Height && y > _clickHereArea.Location.Y)
                    return true;

            return false;
        }

        /// <summary>
        /// Évalue si un click se trouve dans la zone anglaise.
        /// </summary>
        /// <param name="x">Position X du clic</param>
        /// <param name="y">Position Y du clic</param>
        /// <returns>TRUE si la position absolue est dans la zone.</returns>
        protected bool InIciArea(int x, int y)
        {
            if (x > _clickIciArea.Location.X && x < _clickIciArea.Location.X + _clickIciArea.Width)
                if (y < _clickIciArea.Location.Y + _clickIciArea.Height && y > _clickIciArea.Location.Y)
                    return true;

            return false;
        }
    }
}
