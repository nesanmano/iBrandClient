using System;
using System.Drawing;
using System.Windows.Forms;
using NomadLogic.Common;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Ui.NomadScreen;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.custom.Natrel
{
    public partial class WelcomeScreen : AbstractScreen, ILanguageScreenInterface
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


        public WelcomeScreen(string backgroundImgEnSrc)
            : base(backgroundImgEnSrc, backgroundImgEnSrc)
        {
            PictureBox.MouseDown += MouseDownHandler;

            _clickHereArea = new Rectangle(new Point(990, 720), new Size(300, 300));
            _clickIciArea = new Rectangle(new Point(637, 720), new Size(300, 300));
        }

        public new void MouseDownHandler(object sender, MouseEventArgs e)
        {
            if (InHereArea(e.X, e.Y))
            {
                if (OnLanguageSelection != null)
                    OnLanguageSelection(Constants.EN);
                Next();
            }
            else if (InIciArea(e.X, e.Y))
            {
                if (OnLanguageSelection != null)
                    OnLanguageSelection(Constants.FR);
                Next();
            }
        }

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
