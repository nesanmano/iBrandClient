using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using NetFx;
using NomadLogic.Common.Util;
using NomadLogic.iBrandDesktop.Ui.Choice;
using NomadLogic.iBrandDesktop.Util;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.custom.Bethenny
{
    public partial class FilterScreen : AbstractScreen
    {
        readonly Rectangle _nextArea;
        readonly Rectangle _backArea;
        readonly Rectangle _noFilter;
        readonly Rectangle _brannan;
        readonly Rectangle _sunset;
        readonly PictureBox _picture;
        readonly PictureBox _noFilterSelected;
        readonly PictureBox _brannanFilterSelected;
        readonly PictureBox _sunsetFilterSelected;

        Bitmap _originale;
        Bitmap _displayed;
        Bitmap _brannanImage;
        Bitmap _sunsetImage;
        readonly Bitmap _applyingFilterImage;
        readonly Bitmap _brannanFilterSelectedImage;
        readonly Bitmap _sunsetFilterSelectedImage;

        public FilterScreen(string backgroundImgEnSrc)
            : base(backgroundImgEnSrc, backgroundImgEnSrc)
        {

            //InitializeComponent();
            PictureBox.MouseDown += MouseDownHandler;

            _picture = new PictureBox();
            _picture.Location = new Point(436, 42);
            _picture.Size = new Size(713, 713);
            _picture.Visible = true;
            Controls.Add(_picture);
            _picture.BringToFront();

            _applyingFilterImage = GetNoneLanguageBitmap("applying_filter.png");
            _brannanFilterSelectedImage = GetNoneLanguageBitmap("brannan_selected.png");
            _sunsetFilterSelectedImage = GetNoneLanguageBitmap("sunset_selected.png");
            
            _noFilterSelected = new PictureBox();
            _noFilterSelected.Location = new Point(1185, 40);
            _noFilterSelected.Size = new Size(283, 283);
            _noFilterSelected.Visible = true;
            _noFilterSelected.Image = GetNoneLanguageBitmap("no_filter_selected.png");
            _noFilterSelected.SizeMode = PictureBoxSizeMode.Zoom;
            Controls.Add(_noFilterSelected);
            _noFilterSelected.BringToFront();
            
            _brannanFilterSelected = new PictureBox();
            _brannanFilterSelected.Location = new Point(1185, 331);
            _brannanFilterSelected.Size = new Size(283, 283);
            _brannanFilterSelected.Visible = false;
            _brannanFilterSelected.Image = _brannanFilterSelectedImage;
            _brannanFilterSelected.SizeMode = PictureBoxSizeMode.Zoom;
            Controls.Add(_brannanFilterSelected);
            _brannanFilterSelected.BringToFront();
            
            _sunsetFilterSelected = new PictureBox();
            _sunsetFilterSelected.Location = new Point(1185, 625);
            _sunsetFilterSelected.Size = new Size(283, 283);
            _sunsetFilterSelected.Visible = false;
            _sunsetFilterSelected.Image = _sunsetFilterSelectedImage;
            _sunsetFilterSelected.SizeMode = PictureBoxSizeMode.Zoom;
            Controls.Add(_sunsetFilterSelected);
            _sunsetFilterSelected.BringToFront();

            _backArea = new Rectangle(new Point(430, 780), new Size(360, 140));
            _nextArea = new Rectangle(new Point(795, 780), new Size(360, 140));
            _noFilter = new Rectangle(new Point(1185, 40), new Size(290, 290));
            _brannan = new Rectangle(new Point(1185, 330), new Size(290, 290));
            _sunset = new Rectangle(new Point(1185, 625), new Size(290, 290));
        }

        public new void MouseDownHandler(object sender, MouseEventArgs e)
        {
            if (InArea(e.X, e.Y, _nextArea))
            {
                var questionnaire = ScreensControllerAccessor.ScreensController.Questionnaire;
                var question = questionnaire.GetQuestion(18);
                var choiceText = (ChoiceText)question.GetChoice(1);
                
                var photoName = DirectoryUtil.GetLocalMediaDir(DirectoryUtil.NomadLogicMediaDirectoriesEnum.pictures) + choiceText.GetAnswerText();
                _displayed.Save(photoName, ImageFormat.Jpeg);
                _originale.Dispose();
                if (_brannanImage != null)
                {
                    _brannanImage.Dispose();
                    _brannanImage = null;
                }
                if (_sunsetImage != null)
                {
                    _sunsetImage.Dispose();
                    _sunsetImage = null;
                }
                Next();
            }
            else if (InArea(e.X, e.Y, _backArea))
            {
                Back();
            }
            else if (InArea(e.X, e.Y, _noFilter))
            {
                _noFilterSelected.Visible = true;
                _brannanFilterSelected.Visible = false;
                _sunsetFilterSelected.Visible = false;
                ResetTimer();

                _displayed = new Bitmap(_originale); ;
                _picture.Image = _displayed;
            }
            else if (InArea(e.X, e.Y, _brannan))
            {
                _noFilterSelected.Visible = false;
                _brannanFilterSelected.Image = _applyingFilterImage;
                _brannanFilterSelected.Visible = true;
                Refresh();
                ResetTimer();
                if (_brannanImage == null)
                {
                    var effects = new Effects();
                    _brannanImage = effects.Brannan((new BitmapW(_originale)).Clone()).GetBitmap();
                }

                _displayed = new Bitmap(_brannanImage);
                _picture.Image = _displayed;

                _brannanFilterSelected.Image = _brannanFilterSelectedImage;
                _sunsetFilterSelected.Visible = false;
                Refresh();
            }
            else if (InArea(e.X, e.Y, _sunset))
            {
                _noFilterSelected.Visible = false;
                _brannanFilterSelected.Visible = false;
                _sunsetFilterSelected.Image = _applyingFilterImage;
                _sunsetFilterSelected.Visible = true;
                Refresh();
                ResetTimer();
                if (_sunsetImage == null)
                {
                    var effects = new Effects();

                    _sunsetImage = effects.Sunset((new BitmapW(_originale)).Clone()).GetBitmap();
                }

                _sunsetFilterSelected.Image = _sunsetFilterSelectedImage;

                _displayed = new Bitmap(_sunsetImage);
                _picture.Image = _displayed;
                Refresh();
            }
        }

        public override void Recycle()
        {
            base.Recycle();

            Reset();
        }

        public override void SetActivated()
        {
            base.SetActivated();

            Reset();

            var questionnaire = ScreensControllerAccessor.ScreensController.Questionnaire;
            var question = questionnaire.GetQuestion(18);
            var choiceText = (ChoiceText)question.GetChoice(1);
            
            var photoName = //abstractConfiguration.PicturesDir + 
                choiceText.GetAnswerText();
            var tmp = new Bitmap(photoName);
            _originale = new Bitmap(tmp);
            tmp.Dispose();
            _displayed = new Bitmap(_originale);
            _picture.Image = _displayed;
            _picture.SizeMode = PictureBoxSizeMode.Zoom;
        }

        private void Reset()
        {
            _picture.Image = null;

            _brannanFilterSelected.Visible = false;
            _sunsetFilterSelected.Visible = false;
            _noFilterSelected.Visible = true;

            _brannanImage = null;
            _sunsetImage = null;
        }
    }
}
