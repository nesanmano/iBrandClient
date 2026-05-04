using System.Drawing;
using System.Windows.Forms;
using NLog;
using NomadLogic.iBrandDesktop.Ui.Choice;
using NomadLogic.iBrandDesktop.Util;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.custom.Manic5
{
    public class PreviewScreen : AbstractScreen
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        readonly Rectangle _nextArea;
        readonly Rectangle _backArea;


        readonly PictureBox _picture;

        string _photoName;

        public PreviewScreen(string backgroundImgEnSrc, ScreenConfig sc)
            : base(backgroundImgEnSrc, backgroundImgEnSrc, sc)
        {
            //PictureBox.MouseDown += MouseDownHandler;
            //PictureBox.Paint += PaintEvent;

            _picture = new PictureBox
            {
                Location = new Point(606, 82),
                Size = new Size(705, 705),
                Visible = true
            };

            Controls.Add(_picture);
            _picture.BringToFront();

            _backArea = new Rectangle(new Point(607, 853), new Size(343, 128));
            _nextArea = new Rectangle(new Point(867, 853), new Size(343, 128));
        }

        //protected override void MouseDownHandler(object sender, MouseEventArgs e)
        //{
        //    base.MouseDownHandler(sender, e);

        //    if (InArea(e.X, e.Y, _nextArea))
        //    {
        //        Task.Run(async () => { await SsdpUtil.TrySendFilesToPrintstations(null, _photoName); });
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

            var questionnaire = ScreensControllerAccessor.ScreensController.Questionnaire;

            if (_screenConfig.QuestionGifNo != null)
            {
                var question = questionnaire.GetQuestion(_screenConfig.QuestionGifNo.Value);

                var choiceText = (ChoiceText)question.GetChoice(1);

                _photoName = choiceText.GetAnswerText();
            }
            try
            {
                var originale = new Bitmap(_photoName);

                _picture.Image = originale;
                _picture.SizeMode = PictureBoxSizeMode.StretchImage;
            }
            catch (System.Exception ex)
            {
                log.Error(ex.Message);
            }
        }

    }
}
