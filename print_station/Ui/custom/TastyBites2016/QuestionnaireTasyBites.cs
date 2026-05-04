using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using NomadLogic.Common.Ui;
using NomadLogic.Common.Ui.Keyboard;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers;
using NomadLogic.PrintStation.Ui.generic;

namespace NomadLogic.PrintStation.Ui.custom.TastyBites2016
{
    public class QuestionnaireTastyBites2016 : MailScreen
    {
        private readonly Rectangle? _cancelZone;

        public QuestionnaireTastyBites2016(string backgroundImageEnSrc, string backgroundImageFrSrc,
            PrintStationScreenConfig sc, int nbrPageToGoBack, Rectangle textBoxLocation,
            Rectangle previousLocation, KeyboardControl keyboard, IList<ScreenCheckOption> optInOptions,
            Rectangle? cancelZone) :
                base(
                backgroundImageEnSrc, backgroundImageFrSrc, sc, nbrPageToGoBack, textBoxLocation, previousLocation,
                keyboard, optInOptions)
        {
            _cancelZone = cancelZone;
        }

        public override void SetActivated()
        {
            base.SetActivated();

            //SaveAnswerToQuestion(1, 1, GetSelectedPicture().Src);
        }

        //protected override void MouseDownHandler(object sender, MouseEventArgs e)
        //{
        //    base.MouseDownHandler(sender, e);

        //    if (InArea(e.X, e.Y, _cancelZone))
        //    {
        //        Close();
        //    }
        //}

        protected override bool SetAnswersToQuestionnaire()
        {
            var result = base.SetAnswersToQuestionnaire();

            if (!result) return false;

            var ques2 = GetBooleanAnswerFromQuestionCheck(2, 1);
            var ques3 = GetTextAnswerFromQuestion(3, 1);

            if (ques2 == null || !ques2.Value || !string.IsNullOrWhiteSpace(ques3)) return true;
            var mBox = new MessageDialog();
            mBox.ShowMessage("You cannot opt in if you do not provide an email address.");
            return false;
        }
    }
}
