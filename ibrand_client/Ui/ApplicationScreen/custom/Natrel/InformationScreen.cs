using System.Drawing;
using System.Windows.Forms;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.custom.Natrel
{
    public partial class InformationScreen : AbstractScreen
    {
        readonly Rectangle _nextArea;

        readonly Rectangle _backArea;

        readonly Rectangle _didYouKnowArea;

        readonly Rectangle _symptomsArea;

        readonly Rectangle _productsArea;

        private string _test;

        public InformationScreen(string backgroundImgFrSrc, string backgroundImgEnSrc)
            : base(backgroundImgFrSrc, backgroundImgEnSrc)
        {
            //InitializeComponent();
            _test = backgroundImgEnSrc;
            PictureBox.MouseDown += MouseDownHandler;

            _backArea = new Rectangle(new Point(96, 370), new Size(190, 160));
            _nextArea = new Rectangle(new Point(1659, 370), new Size(190, 160));

            _didYouKnowArea = new Rectangle(new Point(480, 744), new Size(260, 260));
            _symptomsArea = new Rectangle(new Point(832, 744), new Size(260, 260));
            _productsArea = new Rectangle(new Point(1180, 744), new Size(260, 260));
        }

        public new void MouseDownHandler(object sender, MouseEventArgs e)
        {
            
            if (InArea(e.X, e.Y, _nextArea))
            {
                Next();
            }
            else if (InArea(e.X, e.Y, _backArea))
            {
                Back();
            }
            else if (InArea(e.X, e.Y, _didYouKnowArea))
            {
               GoTo(1);
            }
            else if (InArea(e.X, e.Y, _symptomsArea))
            {
                GoTo(8);
            }
            else if (InArea(e.X, e.Y, _productsArea))
            {
                GoTo(14);
            }
           
        }
    }
}
