using System.Drawing;
using System.Windows.Forms;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.custom.VetGamesBooth
{
    public class PreviewScreen : AbstractScreen
    {
        readonly Rectangle _nextArea;
        readonly Rectangle _backArea;
        readonly PictureBox _picture;

#pragma warning disable 414
        private string _photoName;
#pragma warning restore 414


        public PreviewScreen(string backgroundImgEnSrc, ScreenConfig sc)
            : base(backgroundImgEnSrc, backgroundImgEnSrc, sc)
        {
            //PictureBox.MouseDown += MouseDownHandler;

            _picture = new PictureBox();
            // Position et taille custom
            _picture.Visible = true;

            Controls.Add(_picture);
            _picture.BringToFront();

            _backArea = new Rectangle(new Point(135, 1465), new Size(385, 150));
            _nextArea = new Rectangle(new Point(610, 1465), new Size(385, 150));
        }

        //protected override void MouseDownHandler(object sender, MouseEventArgs e)
        //{
        //    base.MouseDownHandler(sender, e);
            
        //    if (InArea(e.X, e.Y, _nextArea))
        //    {
        //        Next();                    
        //    }
        //    else if (InArea(e.X, e.Y, _backArea))
        //    {
        //        Back();
        //    }
        //}

        public override void Recycle()
        {
            base.Recycle();

            if (_picture.Image != null)
            {
                _picture.Image.Dispose();
                _picture.Image = null;
            }

            _photoName = null;
        }

        public override void SetActivated()
        {
            base.SetActivated();
            // ReSharper disable once PossibleInvalidOperationException
            DisplayQuestionnaireAnswerInPictureBox(_picture, _screenConfig.QuestionFirstPictureNo.Value, 1, false, true);
            _picture.Location = new Point((Width - _picture.Size.Width)/2, 610);
            Refresh();
        }
    }
}
