using System.Drawing;
using NLog;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.custom.RadioCanada
{
    public class BackgroundSelection1 : AbstractScreen
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        readonly Rectangle _nextArea;
        readonly Rectangle _backArea;

        readonly Rectangle _telejournalArea;
        readonly Rectangle _iciPremiereArea;
        readonly Rectangle _salmigondisArea;


        readonly Bitmap _checkmark;

        int _selectedBackground = -1;

        public BackgroundSelection1(string backgroundImageSrc, ScreenConfig sc)
            : base(backgroundImageSrc, backgroundImageSrc, sc)
        {
            //PictureBox.MouseDown += MouseDownHandler;
            //PictureBox.Paint += PaintEvent;

            _checkmark = GetNoneLanguageBitmap("checkmark.png");

            _telejournalArea = new Rectangle(new Point(292, 148), new Size(320, 273));
            _iciPremiereArea = new Rectangle(new Point(674, 148), new Size(320, 273));
            _salmigondisArea = new Rectangle(new Point(485, 431), new Size(320, 273));

            _backArea = new Rectangle(new Point(13, 668), new Size(170, 130));
            _nextArea = new Rectangle(new Point(1100, 672), new Size(170, 130));
        }

        public override void Recycle()
        {
            base.Recycle();

            _selectedBackground = -1;
        }

        //protected override void MouseDownHandler(object sender, MouseEventArgs e)
        //{
        //    base.MouseDownHandler(sender, e);

        //    if (InArea(e.X, e.Y, _nextArea))
        //    {
        //        SetAnswerToQuestionnaire();
        //        Next();
        //    }
        //    else if (InArea(e.X, e.Y, _backArea))
        //    {
        //        Back();
        //    }
        //    else if (InArea(e.X, e.Y, _telejournalArea))
        //    {
        //        if (_selectedBackground == 1)
        //            _selectedBackground = -1;
        //        else
        //            _selectedBackground = 1;
        //        Refresh();
        //    }
        //    else if (InArea(e.X, e.Y, _iciPremiereArea))
        //    {
        //        if (_selectedBackground == 2)
        //            _selectedBackground = -1;
        //        else
        //            _selectedBackground = 2;
        //        Refresh();
        //    }
        //    else if (InArea(e.X, e.Y, _salmigondisArea))
        //    {
        //        if (_selectedBackground == 3)
        //            _selectedBackground = -1;
        //        else
        //            _selectedBackground = 3;
        //        Refresh();
        //    }

        //}

        private bool SetAnswerToQuestionnaire()
        {

            if (_selectedBackground != -1)
            {
                ResetQuestionAnswers(1);
                //SaveAnswerToQuestion(1, 1, _selectedBackground);
            }
            else
                ResetQuestionAnswers(1);

            return true;
        }

        //public new void PaintEvent(object sender, PaintEventArgs e)
        //{
        //    base.PaintEvent(sender, e);

        //    if (_selectedBackground == 1)
        //    {
        //        if (_checkmark != null)
        //        {
        //            e.Graphics.DrawImageUnscaled(_checkmark, new Point(434, 381));
        //        }
        //    }
        //    else if (_selectedBackground == 2)
        //    {
        //        if (_checkmark != null)
        //        {
        //            e.Graphics.DrawImageUnscaled(_checkmark, new Point(814, 381));
        //        }
        //    }
        //    else if (_selectedBackground == 3)
        //    {
        //        if (_checkmark != null)
        //        {
        //            e.Graphics.DrawImageUnscaled(_checkmark, new Point(623, 657));
        //        }
        //    }
        //}
    }
}
