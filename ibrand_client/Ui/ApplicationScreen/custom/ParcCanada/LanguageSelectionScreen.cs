using System;
using System.Drawing;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Ui.NomadScreen;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.custom.ParcCanada
{
    public partial class LanguageSelectionScreen : AbstractScreen, ILanguageScreenInterface
    {
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

        Rectangle _clickHereArea;
        Rectangle _clickIciArea;

        public LanguageSelectionScreen(string backgroundImgEnSrc, ScreenConfig sc)
            : base(backgroundImgEnSrc, backgroundImgEnSrc, sc)
        {
            //PictureBox.MouseDown += MouseDownHandler;

            _clickHereArea = new Rectangle(new Point(1920 / 2, 0), new Size(1920 / 2, 1080));
            _clickIciArea = new Rectangle(new Point(0, 0), new Size(1920 / 2, 1080));
        }

        /*protected override void MouseDownHandler(object sender, MouseEventArgs e)
        {
            base.MouseDownHandler(sender, e);

            var questionnaire = ScreensControllerAccessor.ScreensController.Questionnaire;

            if (InHereArea(e.X, e.Y))
            {
                if (OnLanguageSelection != null)
                    OnLanguageSelection(Constants.EN);

                var question = questionnaire.GetQuestion(7);
                var choiceCheck = (ChoiceRadio)question.GetChoice(2);
                var answer = new Answer(-1, question.NumberInt, 2);
                choiceCheck.SetAnswer(answer);

                Next();
            }
            else if (InIciArea(e.X, e.Y))
            {
                if (OnLanguageSelection != null)
                    OnLanguageSelection(Constants.FR);

                var question = questionnaire.GetQuestion(7);
                var choiceCheck = (ChoiceRadio)question.GetChoice(1);
                var answer = new Answer(-1, question.NumberInt, 1);
                choiceCheck.SetAnswer(answer);

                Next();
            }
        }
*/

        protected bool InHereArea(int x, int y)
        {
            if (x > _clickHereArea.Location.X && x < _clickHereArea.Location.X + _clickHereArea.Width)
                if (y < _clickHereArea.Location.Y + _clickHereArea.Height && y > _clickHereArea.Location.Y)
                    return true;

            return false;
        }

        protected bool InIciArea(int x, int y)
        {
            if (x > _clickIciArea.Location.X && x < _clickIciArea.Location.X + _clickIciArea.Width)
                if (y < _clickIciArea.Location.Y + _clickIciArea.Height && y > _clickIciArea.Location.Y)
                    return true;

            return false;
        }
    }
}
