using System.Drawing;
using System.Windows.Forms;
using NomadLogic.Common.Ui;
using NomadLogic.iBrandDesktop.Ui.Choice;
using NomadLogic.iBrandDesktop.Util;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.custom.Bethenny
{
    public partial class ChanceScreen : AbstractScreen
    {
        readonly Rectangle _nextArea;
        readonly Rectangle _checkArea;

        readonly PictureBox _checkPictureBox;

        int _checked;


        public ChanceScreen(string backgroundImgEnSrc)
            : base(backgroundImgEnSrc, backgroundImgEnSrc)
        {
            PictureBox.MouseDown += MouseDownHandler;

            var checkImage = GetNoneLanguageBitmap("checked.png");
            _checkPictureBox = new PictureBox();
            _checkPictureBox.Location = new Point(243, 500);
            _checkPictureBox.Size = new Size(41, 40);
            _checkPictureBox.Visible = false;
            _checkPictureBox.MouseDown += MouseDownHandler;

            _checkPictureBox.Image = checkImage;

            Controls.Add(_checkPictureBox);

            _checkPictureBox.BringToFront();

            _nextArea = new Rectangle(new Point(669, 656), new Size(615, 240));
            _checkArea = new Rectangle(new Point(216, 500), new Size(1465, 120));

        }

        public override void Recycle()
        {
            base.Recycle();

            _checked = 0;
            _checkPictureBox.Visible = false;
        }

        public new void MouseDownHandler(object sender, MouseEventArgs e)
        {

            ResetTimer();
            
            if (InArea(e.X, e.Y, _nextArea) && _checked == 0)
            {                
                var mBox = new MessageDialog();
                mBox.ShowMessage("Please check the agreement box to continue.");
                return;
            }


            if (InArea(e.X, e.Y, _nextArea) && _checked != 0)
            {
                var questionnaire = ScreensControllerAccessor.ScreensController.Questionnaire;
                var question = questionnaire.GetQuestion(1);
                var choiceCheck = (ChoiceCheck)question.GetChoice(1);

                //choiceCheck.SetAnswer(new Answer(1, 1, 1));

                Next();
            }
            else if (InArea(e.X, e.Y, _checkArea) || sender.Equals(_checkPictureBox))
            {

                if (_checked == 0)
                {
                    _checkPictureBox.Visible = true;
                    _checked = 1;
                }
                else
                {
                    _checkPictureBox.Visible = false;
                    _checked = 0;
                }                
            }
         
        }
    }
}
