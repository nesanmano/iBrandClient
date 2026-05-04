using System.Drawing;
using System.Windows.Forms;
using NLog;
using NomadLogic.iBrandDesktop.Ui.Choice;
using NomadLogic.iBrandDesktop.Util;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.custom.HarryPotter
{
    public class PreviewScreen : AbstractScreen
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        readonly Rectangle _nextArea;
        readonly Rectangle _backArea;

        readonly PictureBox _picture;

        string _photoName;

        public PreviewScreen(string backgroundImgEnSrc)
            : base(backgroundImgEnSrc, backgroundImgEnSrc)
        {
            //PictureBox.MouseDown += MouseDownHandler;
            //PictureBox.Paint += PaintEvent;

            _picture = new PictureBox
            {
                Location = new Point(451, 68),
                Size = new Size(405, 570),
                Visible = true
            };

            Controls.Add(_picture);
            _picture.BringToFront();

            _backArea = new Rectangle(new Point(448, 645), new Size(199, 75));
            _nextArea = new Rectangle(new Point(657, 646), new Size(201, 75));
        }

        //protected override void MouseDownHandler(object sender, MouseEventArgs e)
        //{
        //    base.MouseDownHandler(sender, e);

        //    if (InArea(e.X, e.Y, _nextArea))
        //    {
        //        Next();
                
        //        // Envoi l'image sur les printstations détectés. On ne se préoccupe des erreurs ou nombre de printstation qui ont reçu l'image.
        //        Task.Run(async () => { await SsdpUtil.TrySendFilesToPrintstations(null, _photoName); });
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

            var question = questionnaire.GetQuestion(2);

            var choiceText = (ChoiceText)question.GetChoice(1);

            _photoName = choiceText.GetAnswerText();
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
