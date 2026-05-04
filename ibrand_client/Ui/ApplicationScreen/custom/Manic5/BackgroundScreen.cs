using System.Drawing;
using NomadLogic.Common.Ui;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.custom.Manic5

{
    public class BackgroundScreen : AbstractScreen
    {
        readonly Rectangle _nextArea;

        readonly Rectangle _vueHelico;
        readonly Rectangle _village;
        readonly Rectangle _ciment;
        readonly Rectangle _camion;

        readonly Bitmap _checkmark;

        int _selectedBackground = -1;

        public BackgroundScreen(string backgroundImgEnSrc, ScreenConfig sc)
            : base(backgroundImgEnSrc, backgroundImgEnSrc, sc)
        {
            //PictureBox.MouseDown += MouseDownHandler;
            //PictureBox.Paint += PaintEvent;

            _checkmark = GetNoneLanguageBitmap("checkmark.png");

            _nextArea = new Rectangle(new Point(785, 878), new Size(350, 95));

            _vueHelico = new Rectangle(new Point(32, 260), new Size(440, 535));
            _village = new Rectangle(new Point(506, 260), new Size(440, 535));
            _ciment = new Rectangle(new Point(978, 260), new Size(440, 535));
            _camion = new Rectangle(new Point(1452, 260), new Size(440, 535));
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
        //        if (SetAnswerToQuestionnaire())
        //            Next();
        //    }           
        //    else if (InArea(e.X, e.Y, _vueHelico))
        //    {
        //        _selectedBackground = 1;
        //        Refresh();
        //    }
        //    else if (InArea(e.X, e.Y, _village))
        //    {
        //        _selectedBackground = 2;
        //        Refresh();
        //    }
        //    else if (InArea(e.X, e.Y, _ciment))
        //    {
        //        _selectedBackground = 3;
        //        Refresh();
        //    }
        //    else if (InArea(e.X, e.Y, _camion))
        //    {
        //        _selectedBackground = 4;
        //        Refresh();
        //    }
        //}

        private bool SetAnswerToQuestionnaire()
        {

            if (_selectedBackground == -1)
            {
                var mBox = new MessageDialog();

                mBox.ShowMessage("Vous devez choisir une image de fond.");

                return false;
            }
            
            //var questionnaire = ScreensControllerAccessor.ScreensController.Questionnaire;

            //var question = questionnaire.GetQuestion(1);
            //var choiceCombo = (ChoiceCombo)question.GetChoice(1);
            //choiceCombo.SetAnswer(_selectedBackground);
          
            return true;
        }

        //public new void PaintEvent(object sender, PaintEventArgs e)
        //{
        //    base.PaintEvent(sender, e);
        //    if (_checkmark == null) return;
        //    switch (_selectedBackground)
        //    {
        //        case 1:
        //            e.Graphics.DrawImageUnscaled(_checkmark, new Point(218, 750));
        //            break;
        //        case 2:
        //            e.Graphics.DrawImageUnscaled(_checkmark, new Point(700, 750));
        //            break;
        //        case 3:
        //            e.Graphics.DrawImageUnscaled(_checkmark, new Point(1172, 750));
        //            break;
        //        case 4:
        //            e.Graphics.DrawImageUnscaled(_checkmark, new Point(1647, 750));
        //            break;
        //    }
        //}
    }
}
