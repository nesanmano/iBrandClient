using System.Drawing;
using System.Windows.Forms;
using NomadLogic.iBrandDesktop.Ui.Choice;
using NomadLogic.iBrandDesktop.Util;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.custom.Bethenny
{
    public partial class PreviewScreen : AbstractScreen
    {
        readonly Rectangle _nextArea;
        readonly Rectangle _backArea;
        readonly PictureBox _picture;

        public PreviewScreen(string backgroundImgEnSrc)
            : base(backgroundImgEnSrc, backgroundImgEnSrc)
        {
            PictureBox.MouseDown += MouseDownHandler;

            _picture = new PictureBox();
            
            _picture.Location = new Point(311, 24);
            _picture.Size = new Size(1280, 720);
            _picture.Visible = true;

            Controls.Add(_picture);
            _picture.BringToFront();

            _backArea = new Rectangle(new Point(486, 753), new Size(453, 160));
            _nextArea = new Rectangle(new Point(945, 753), new Size(453, 160));
        }

        public new void MouseDownHandler(object sender, MouseEventArgs e)
        {
            if (InArea(e.X, e.Y, _nextArea))
            {
                //picture.Image.Dispose();
                Next();
            }
            else if (InArea(e.X, e.Y, _backArea))
            {
                Back();
            }
        }

        public override void Recycle()
        {
            base.Recycle();
            _picture.Image = null;
        }

        public override void SetActivated()
        {
            base.SetActivated();

            var questionnaire = ScreensControllerAccessor.ScreensController.Questionnaire;
            var question = questionnaire.GetQuestion(18);
            var choiceText = (ChoiceText)question.GetChoice(1);
            
            var photoName = //abstractConfiguration.PicturesDir +
                choiceText.GetAnswerText();
            var originale = new Bitmap(photoName);
            _picture.Image = originale;
            _picture.SizeMode = PictureBoxSizeMode.StretchImage;
        }
    }
}
