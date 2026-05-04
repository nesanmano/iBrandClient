using System;
using System.Drawing;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Ui.NomadScreen;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.custom.Permatex
{
    public class HomeScreen : AbstractScreen, ILanguageScreenInterface
    {
        public event LanguageSelectionEventHandler OnLanguageSelection;
        public void LanguageSelection(string lg)
        {
            throw new System.NotImplementedException();
        }

        public Func<IRespondent> ObtainRespondentMethod { get; set; }
        public int? QuestionLanguageNo { get; set; }
        public int QuestionLanguageChoiceNoFr { get; set; }
        public int QuestionLanguageChoiceNoEn { get; set; }
        public bool IsQuestionLanguageRadio { get; set; }

        public new void ChangeLanguage(string lg)
        {
            throw new System.NotImplementedException();
        }

        readonly Rectangle _clickHereArea;
        readonly Rectangle _clickIciArea;

        public HomeScreen(string backgroundImgEnSrc, string backgroundImageSrc)
            : base(backgroundImgEnSrc, backgroundImageSrc)
        {
            //PictureBox.MouseDown += MouseDownHandler;

            _clickHereArea = new Rectangle(new Point(822, 31), new Size(444, 741));
            _clickIciArea = new Rectangle(new Point(17, 30), new Size(479, 741));
        }

        //protected override void MouseDownHandler(object sender, MouseEventArgs e)
        //{
        //    base.MouseDownHandler(sender, e);

        //    if (InArea(e.X, e.Y, _clickHereArea))
        //    {
        //        if (OnLanguageSelection != null)
        //            OnLanguageSelection(Constants.EN);

        //        Next();
        //    }
        //    else if (InArea(e.X, e.Y, _clickIciArea))
        //    {
        //        if (OnLanguageSelection != null)
        //            OnLanguageSelection(Constants.FR);

        //        Next();
        //    }

        //}
    }
}
