using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using NLog;
using NomadLogic.Common.Extentions;
using NomadLogic.Common.Ui;
using NomadLogic.Common.Ui.Keyboard;
using NomadLogic.Common.Util;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers;
using NomadLogic.Multimedia;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.custom.HydroSense
{
    class ScoreScreen : AbstractScreen
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private readonly Rectangle _nextArea;
        private readonly Rectangle _backArea;
        private TextBox _scoreTextbox;
        private Font _font;
        private readonly Size _size = new Size(1280, 720);
        private PrivateFontCollection _fontCollection;

        public ScoreScreen(string bkgEn, string bkgFr, ScreenConfig sc)
            : base(bkgEn, bkgFr, sc)
        {
            //PictureBox.MouseClick += MouseDownHandler;

            AddTextBoxs();
            AddKeyboard();

            _nextArea = new Rectangle(660, 721, 114, 54);
            _backArea = new Rectangle(448,721,172,54);
        }

        public override void Recycle()
        {
            base.Recycle();

            _scoreTextbox.Text = "";

        }

        private void AddKeyboard()
        {
            AddKeyboardToControls<NumbersKeyboardControl>(426,288,425,417);
        }

        private void AddTextBoxs()
        {
            _scoreTextbox = new TextBox();
            this.InitTextBox(_scoreTextbox, new Rectangle(429, 215, 421, 61), null);
        }

        //protected override void MouseDownHandler(object sender, MouseEventArgs e)
        //{
        //    base.MouseDownHandler(sender, e);
        //    if (InArea(e.X, e.Y, _nextArea))
        //    {
        //        if (SetAnswersToQuestionnaire())
        //        {
        //            CreateScoreImage();
        //            Next();
        //        }
        //    }

        //    if (InArea(e.X, e.Y, _backArea))
        //    {
        //        _scoreTextbox.Text = "";
        //        Back();
        //    }
        //}
        ///creeer une image pour le display on TV
        private void CreateScoreImage()
        {
            
            if (_font == null)
            {
                _fontCollection = new PrivateFontCollection();
                var fontFile = GetTerminalDirectory() + "Digital-Dismay.ttf";
                if (!File.Exists(fontFile)) throw new System.Exception("Font file does not exists: " + fontFile);
                _fontCollection.AddFontFile(fontFile);
                var fontFam = _fontCollection.Families.First(fontCheck => fontCheck.Name.Contains("Digital"));
                _font = new Font(fontFam, 700, FontStyle.Regular, GraphicsUnit.Pixel);
            }

            try
            {
                var image = ImageUtil.CreateSimpleTextImage(_scoreTextbox.Text, Color.Transparent, Color.Red, _font, _size);
                var fileName = DirectoryUtil.GetLocalShowPicturesOnTvPicturesDir() + GenerateUniqueId.GetId() + ".png";
                image.Save(ImageFormat.Png, 9, fileName);
            }
            catch (System.Exception e)
            {
                log.Error(e, "Could not create image of score.");
            }
        }

        private bool SetAnswersToQuestionnaire()
        {
            // Validation nom
            if (!string.IsNullOrWhiteSpace(_scoreTextbox.Text) && int.TryParse(_scoreTextbox.Text,out _))
            {
                //SaveAnswerToQuestion(15, 1, _scoreTextbox.Text.Trim());
                return true;
            }

            var mBox = new MessageDialog();
            mBox.ShowMessage("Enter a Number. Eg: 0,12,50...");
            return false;

        }

        public override void SetActivated()
        {
            base.SetActivated();

            _scoreTextbox.Focus();
        }

    }
}
