using System.Drawing;
using System.Windows.Forms;
using NLog;
using NomadLogic.iBrandDesktop.Ui.Choice;
using NomadLogic.iBrandDesktop.Util;

#pragma warning disable 618

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.custom.Permatex
{
    public class PreviewScreen : AbstractScreen
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        readonly Rectangle _nextArea;
        readonly Rectangle _backArea;
        readonly Rectangle _closeArea;
        readonly PictureBox _picture;

        private string _photoName;

        public PreviewScreen(string backgroundImgEnSrc, string backgroundImageSrc)
            : base(backgroundImgEnSrc, backgroundImageSrc)
        {
            //PictureBox.MouseDown += MouseDownHandler;
            //PictureBox.Paint += PaintEvent;

            _picture = new PictureBox
            {
                Location = new Point(286, 65),
                Size = new Size(720, 480),
                Visible = true
            };

            Controls.Add(_picture);
            _picture.BringToFront();

            _backArea = new Rectangle(new Point(394, 567), new Size(223, 90));
            _nextArea = new Rectangle(new Point(646, 567), new Size(223, 90));
            _closeArea = new Rectangle(new Point(1180, 16), new Size(74, 74));
        }

        //protected override void MouseDownHandler(object sender, MouseEventArgs e)
        //{
        //    base.MouseDownHandler(sender, e);
            
        //    if (InArea(e.X, e.Y, _nextArea))
        //    {
        //        Next();
                
        //        Task.Run(async () => { await SsdpUtil.TrySendFilesToPrintstations(null, _photoName); });
        //    }
        //    else if (InArea(e.X, e.Y, _backArea))
        //    {
        //        Back();
        //    }
        //    else if (InArea(e.X, e.Y, _closeArea))
        //    {
        //        FirstScreen();
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

            var question = questionnaire.GetQuestion(ConstantsPhotoBooth._question_gif_no != -1 ? ConstantsPhotoBooth._question_gif_no : ConstantsPhotoBooth._question_first_picture_no);

            var choiceText = (ChoiceText)question.GetChoice(1);
            
            _photoName = //configuration.PicturesDir + 
                choiceText.GetAnswerText();
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
