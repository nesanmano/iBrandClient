using System;
using System.Drawing;
using JetBrains.Annotations;
using NLog;
using NomadLogic.Common;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Ui.NomadScreen;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic
{
    /// <summary>
    /// Cet écran affiche un message de bienvenue et passe automatiquement au prochain écran. On peut optionnellement donner des
    /// zones de clic pour changer la langue du questionnaire.
    /// </summary>
    [PublicAPI]
    [Obsolete]
    public class HomeScreen : AbstractScreen, ILanguageScreenInterface
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Événement quand on choisit une langue.
        /// </summary>
        public event LanguageSelectionEventHandler OnLanguageSelection;

        public void LanguageSelection(string lg)
        {
            OnLanguageSelection?.Invoke(lg);
        }

        private readonly Rectangle? _nextArea;
        private readonly Rectangle? _nextAreaFr;
        private readonly Rectangle? _nextAreaEn;
        
        /// <summary>
        /// Constructeur par défaut.
        /// </summary>
        /// <param name="backgroundImgEnSrc">Écran de fond. Il n'ya pas d'option pour français car ces cet écran qui indique les choix de langue.</param>
        /// <param name="sc">COnfiguration d'écran.</param>
        /// <param name="nextFrench">Zone pour passer au prochain écran, en choisissant le français. Optionnel.</param>
        /// <param name="nextEnglish">Zone pour passer au prochain écran, en choisissant l'anglais. Optionnel.</param>
        [Obsolete]
        public HomeScreen(string backgroundImgEnSrc, ScreenConfig sc, Rectangle? nextFrench = null, Rectangle? nextEnglish = null)
            : base(backgroundImgEnSrc, backgroundImgEnSrc, sc)
        {
            _nextAreaFr = nextFrench;
            _nextAreaEn = nextEnglish;

            if (_nextAreaFr != null || _nextAreaEn != null) return;
            log.Trace("No language section detected: creatating full-screen next zone.");
            _nextArea = new Rectangle(new Point(0, 0), new Size(PictureBox.Image.Width, PictureBox.Image.Height));
        }
        
        /// <summary>
        /// Indique quelle est la question de langue dans le questionnaire. Null par défaut, ce qui implique aucune
        /// sélection de langue dans le questionnaire. Le gestionnaire d'écran change quand même.
        /// </summary>
        public int? QuestionLanguageNo { get; set; }
        /// <summary>
        /// Le choix qui est associé au français avec la question de langue. Par défaut, 2
        /// </summary>
        /// TODO: Il n'y a plus de sous-choix dans juju. sur coup on parle plutot d'un choix 1 par defaut
        public int QuestionLanguageChoiceNoFr { get; set; } = 1;
        /// <summary>
        /// Le choix qui est associé à l'anglais avec la question de langue. Par défaut, 1
        /// </summary>
        ///TODO: Il n'y a plus de sous-choix dans juju. sur coup on parle plutot d'un choix 0 par defaut
        public int QuestionLanguageChoiceNoEn { get; set; } = 0;
        /// <summary>
        /// Indique que le type de question qui gère la langue est une question radio, au lieu d'une question combo. 
        /// Faux par défaut (on assume combo par défaut).
        /// </summary>
        public bool IsQuestionLanguageRadio { get; set; }

        /// <inheritdoc/>
        protected override bool OnMouseDownHandler(int x, int y)
        {
            if (!base.OnMouseDownHandler(x, y)) return false;

            if (InArea(x, y, _nextArea))
            {
                log.Trace("Full-screen Next area activated.");
                Next();
                return false;
            }

            if (InArea(x, y, _nextAreaFr))
            {
                log.Trace("French Next area activated.");
                // ExecuteLanguageChange(Constants.FR, QuestionLanguageChoiceNoFr);
                ExecuteLanguageChangeJuJu(Constants.FR, QuestionLanguageChoiceNoFr);
                return false;
            }

            if (InArea(x,y, _nextAreaEn))
            {
                log.Trace("English Next area activated.");
                // ExecuteLanguageChange(Constants.EN, QuestionLanguageChoiceNoEn);
                ExecuteLanguageChangeJuJu(Constants.EN, QuestionLanguageChoiceNoEn);
                return false;
            }

            return true;
        }


        private void ExecuteLanguageChangeJuJu(string langCode, int choice)
        {
            log.Trace("Changing respondent language to {0}", langCode);
            var resp = GetCurrentRespondentWithAnswers();
            resp.Language = langCode;

            log.Trace("Changing current questionnaire language to {0}.", langCode);
            GetCurrentQuestionnaire().LanguageCurrent = langCode;

            if (QuestionLanguageNo != null)
            {
                if (!IsQuestionLanguageRadio)
                {
                    SaveAnswerToQuestion(new Answer {QuestionNo = QuestionLanguageNo.Value, ChoiceNo = choice});
                }
                else
                {
                    var answer = new Answer{QuestionType = QuestionTypeEnum.Radio, QuestionNo = QuestionLanguageNo.Value, ChoiceNo = choice};
                    //SaveAnswerToQuestion(QuestionLanguageNo.Value, choice, answer);
                }
                log.Trace("Language question saved to {1}: qId={0}", QuestionLanguageNo?.ToString(), langCode);
            }

            log.Trace("Changing touch screens language to {0}.", langCode);
            OnLanguageSelection?.Invoke(langCode);

            Next();
        }

        private void ExecuteLanguageChange(string langCode, int choiceNo)
        {
            log.Trace("Changing respondent language to {0}", langCode);
            var resp = GetCurrentRespondentWithAnswers();
            resp.Language = langCode;

            log.Trace("Changing current questionnaire language to {0}.", langCode);
            GetCurrentQuestionnaire().LanguageCurrent = langCode;

            if (QuestionLanguageNo != null)
            {
                if (!IsQuestionLanguageRadio)
                {
                    SaveAnswerToQuestion(new Answer{QuestionNo = QuestionLanguageNo.Value, ChoiceNo = choiceNo});
                }
                else
                {
                    var answer = new Answer{QuestionType = QuestionTypeEnum.Radio, QuestionNo = QuestionLanguageNo.Value, ChoiceNo = choiceNo};
                    //SaveAnswerToQuestion(QuestionLanguageNo.Value, choiceNo, answer);
                }
                log.Trace("Language question saved to {1}: qId={0}", QuestionLanguageNo?.ToString(), langCode);
            }

            log.Trace("Changing touch screens language to {0}.", langCode);
            OnLanguageSelection?.Invoke(langCode);

            Next();
        }
    }
}
