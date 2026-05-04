using System.Drawing;
using NLog;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.custom.RadioCanada
{
    public class BackgroundSelection2 : AbstractScreen
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        readonly Rectangle _nextArea;
        readonly Rectangle _backArea;

        readonly Rectangle _artvArea;
        readonly Rectangle _30ViesArea;
        readonly Rectangle _unite9Area;
        readonly Rectangle _ecouteMoiCaArea;


        readonly Bitmap _checkmark;

        readonly Bitmap _grey_square;

        bool _backgroundAlreadySelected = false;

        int _selectedBackground = -1;

        public BackgroundSelection2(string backgroundImageSrc, ScreenConfig sc)
            : base(backgroundImageSrc, backgroundImageSrc, sc)
        {
//            PictureBox.MouseDown += MouseDownHandler;
//            PictureBox.Paint += PaintEvent;

            _checkmark = GetNoneLanguageBitmap("checkmark.png");

            _grey_square = GetNoneLanguageBitmap("carre_gris.png");

            _artvArea = new Rectangle(new Point(292, 148), new Size(320, 273));
            _30ViesArea = new Rectangle(new Point(674, 148), new Size(320, 273));
            _unite9Area = new Rectangle(new Point(295, 433), new Size(320, 273));
            _ecouteMoiCaArea = new Rectangle(new Point(674, 433), new Size(320, 273));

            _backArea = new Rectangle(new Point(13, 668), new Size(170, 130));
            _nextArea = new Rectangle(new Point(1100, 672), new Size(170, 130));
        }

        public override void Recycle()
        {
            base.Recycle();

            _selectedBackground = -1;
        }

        /*protected override void MouseDownHandler(object sender, MouseEventArgs e)
        {
            base.MouseDownHandler(sender, e);

            if (InArea(e.X, e.Y, _nextArea))
            {
                SetAnswerToQuestionnaire();
                if (!IsQuestionAnswered(1))
                {
                    var mBox = new MessageDialog();

                    mBox.ShowMessage("Vous devez sélectionner un arrière-plan.");

                    return;
                }
                Next();
            }
            else if (InArea(e.X, e.Y, _backArea))
            {
                _selectedBackground = -1;
                Back();
            }
            if (!_backgroundAlreadySelected)
            { 
                if (InArea(e.X, e.Y, _artvArea))
                {
                    _selectedBackground = 4;
                    Refresh();
                }
                else if (InArea(e.X, e.Y, _30ViesArea))
                {
                    _selectedBackground = 5;
                    Refresh();
                }
                else if (InArea(e.X, e.Y, _unite9Area))
                {
                    _selectedBackground = 6;
                    Refresh();
                }
                else if (InArea(e.X, e.Y, _ecouteMoiCaArea))
                {
                    _selectedBackground = 7;
                    Refresh();
                }
            }

        }
*/

        private bool SetAnswerToQuestionnaire()
        {

            //if (_selectedBackground != -1)
            //    SaveAnswerToQuestion(1, 1, _selectedBackground);

            return true;
        }

        /*public new void PaintEvent(object sender, PaintEventArgs e)
        {
            base.PaintEvent(sender, e);

            if (_backgroundAlreadySelected)
            {
                //On dessine la case grise...
                if (_grey_square != null)
                {
                    e.Graphics.DrawImageUnscaled(_grey_square, new Point(431, 382));

                    e.Graphics.DrawImageUnscaled(_grey_square, new Point(811, 382));

                    e.Graphics.DrawImageUnscaled(_grey_square, new Point(431, 658));

                    e.Graphics.DrawImageUnscaled(_grey_square, new Point(811, 658));
                }
            }
            else
            {
                if (_selectedBackground == 4)
                {
                    if (_checkmark != null)
                    {
                        e.Graphics.DrawImageUnscaled(_checkmark, new Point(434, 381));
                    }
                }
                else if (_selectedBackground == 5)
                {
                    if (_checkmark != null)
                    {
                        e.Graphics.DrawImageUnscaled(_checkmark, new Point(814, 381));
                    }
                }
                else if (_selectedBackground == 6)
                {
                    if (_checkmark != null)
                    {
                        e.Graphics.DrawImageUnscaled(_checkmark, new Point(434, 657));
                    }
                }
                else if (_selectedBackground == 7)
                {
                    if (_checkmark != null)
                    {
                        e.Graphics.DrawImageUnscaled(_checkmark, new Point(814, 657));
                    }
                }
            }
        }
*/

       

        public override void SetActivated()
        {
            base.SetActivated();

            if (_selectedBackground == -1)
                _backgroundAlreadySelected = IsQuestionAnswered(1);
        }

      }
}
