using System.Drawing;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.custom.BronchiShield
{
    public class QuestionScreen : AbstractScreen
    {
        readonly Rectangle _yes;
        readonly Rectangle _no;

        public QuestionScreen(string backgroundImgSrc)
            : base(backgroundImgSrc)
        {
            //PictureBox.MouseDown += MouseDownHandler;

            _yes = new Rectangle(new Point(61, 1624), new Size(420, 212));
            _no = new Rectangle(new Point(573, 1624), new Size(420, 212));

        }

        //protected override void MouseDownHandler(object sender, MouseEventArgs e)
        //{
        //    base.MouseDownHandler(sender, e);

        //    if (InArea(e.X, e.Y, _yes))
        //    {
        //        Next();
        //    }
        //    else if (InArea(e.X, e.Y, _no))
        //    {
        //        Next(2);
        //    }
            
        //}
    }
}
