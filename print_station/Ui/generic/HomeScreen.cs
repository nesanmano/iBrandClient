using System;
using System.Drawing;
using NLog;
using NomadLogic.Common;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Ui;
using NomadLogic.Common.Ui.NomadScreen;
using NomadLogic.iBrandDesktop.Ui.Question;

namespace NomadLogic.PrintStation.Ui.generic
{
    public class HomeScreen : AbstractPrintStationScreen, ILanguageScreenInterface
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        public event LanguageSelectionEventHandler OnLanguageSelection;
        public void LanguageSelection(string lg)
        {
            OnLanguageSelection?.Invoke(lg);
        }
        
        private readonly Rectangle? _nextArea;
        private readonly Rectangle? _nextAreaFr;
        private readonly Rectangle? _nextAreaEn;
        private readonly Rectangle? _cancelZone;

        public HomeScreen(string backgroundImgEnSrc, PrintStationScreenConfig sc)
            : this(backgroundImgEnSrc, sc, null, null, null)
        {
            // Rien de plus
        }

        public HomeScreen(string backgroundImgEnSrc, PrintStationScreenConfig sc, Rectangle? nextFrench, Rectangle? nextEnglish, Rectangle? cancelZone)
            : base(backgroundImgEnSrc, backgroundImgEnSrc, sc)
        {
            _nextAreaFr = nextFrench;
            _nextAreaEn = nextEnglish;
            _cancelZone = cancelZone;

            if (_nextAreaFr == null && _nextAreaEn == null)
            {
                _nextArea = new Rectangle(new Point(0, 0), new Size(PictureBox.Image.Width, PictureBox.Image.Height));
            }
        }

        public int? QuestionLanguageNo { get; set; }
        public int QuestionLanguageChoiceNoFr { get; set; } = 2;
        public int QuestionLanguageChoiceNoEn { get; set; } = 1;

        protected override bool OnMouseDownHandler(int x, int y)
        {
            if (!base.OnMouseDownHandler(x, y)) return false;
            
            if (InArea(x, y, _nextArea))
            {
                Next();
                return false;
            }

            if (InArea(x, y, _nextAreaFr))
            {
                log.Trace("French Next area activated.");
                ExecuteLanguageChange(Constants.FR, QuestionLanguageChoiceNoFr);
                return false;
            }

            if (InArea(x,y, _nextAreaEn))
            {
                log.Trace("English Next area activated.");
                ExecuteLanguageChange(Constants.EN, QuestionLanguageChoiceNoEn);
                return false;
            }

            if (InArea(x, y, _cancelZone))
            {
                Close();
                return false;
            }

            return true;
        }

        private void ExecuteLanguageChange(string langCode, int subChoiceNo)
        {
            log.Trace("Changing touch screens language to {0}.", langCode);
            OnLanguageSelection?.Invoke(langCode);

            log.Trace("Changing respondent language to {0}", langCode);
            var resp = GetCurrentRespondentWithAnswers();
            resp.Language = langCode;

            log.Trace("Changing current questionnaire language to {0}.", langCode);
            GetCurrentQuestionnaire().LanguageCurrent = langCode;

            /*if (QuestionLanguageNo != null)
            {
                if (!IsQuestionLanguageRadio)
                {
                    SaveAnswerToQuestion(QuestionLanguageNo.Value, 1, subChoiceNo);
                }
                else
                {
                    var answer = new Answer(QuestionTypeEnum.Radio, QuestionLanguageNo.Value, subChoiceNo);
                    SaveAnswerToQuestion(QuestionLanguageNo.Value, subChoiceNo, answer);
                }
                log.Trace("Language question saved to {1}: qId={0}", QuestionLanguageNo?.ToString(), langCode);
            }*/

            Next();
        }

        public bool IsQuestionLanguageRadio { get; set; }
    }
}
