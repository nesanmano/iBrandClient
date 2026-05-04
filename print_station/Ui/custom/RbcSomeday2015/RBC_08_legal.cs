using System.Drawing;
using System.Windows.Forms;
using NomadLogic.PrintStation.Ui.generic;

namespace NomadLogic.PrintStation.Ui.custom.RbcSomeday2015
{
    public class Rbc08Legal : AbstractPrintStationScreen
    {
        readonly Rectangle _previousArea;

        public Rbc08Legal(string backgroundImgEnSrc, PrintStationScreenConfig sc)
            : base(backgroundImgEnSrc, backgroundImgEnSrc, sc)
        {
            //PictureBox.MouseClick += MouseDownHandler;
            
            _previousArea = new Rectangle(new Point(552, 498), new Size(175, 57));
        }

        //protected override void MouseDownHandler(object sender, MouseEventArgs e)
        //{
        //    base.MouseDownHandler(sender, e);

        //    if (!InArea(e.X, e.Y, _previousArea)) return;

        //    Back();
        //}
    }
}
