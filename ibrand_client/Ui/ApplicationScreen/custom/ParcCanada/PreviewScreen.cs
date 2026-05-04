using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using NomadLogic.Common;
using NomadLogic.Common.Respondent;
using NomadLogic.iBrandDesktop.Ui.Choice;
using NomadLogic.iBrandDesktop.Util;
using NomadLogic.Multimedia.XmlTransformation.image;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.custom.ParcCanada
{
    public partial class PreviewScreen : AbstractScreen
    {
        readonly Rectangle _nextArea;
        readonly Rectangle _backArea;
        readonly PictureBox _picture;
        readonly TransformationsController _transControl;

        public PreviewScreen(string backgroundImgEnSrc, string backgroundImageSrc, ScreenConfig sc)
            : base(backgroundImgEnSrc, backgroundImageSrc, sc)
        {
            //PictureBox.MouseDown += MouseDownHandler;

            IsSaveOnThisPage = false;

            _picture = new PictureBox
            {
                Location = new Point(486, 223),
                Size = new Size(950, 643),
                Visible = true
            };


            Controls.Add(_picture);
            _picture.BringToFront();

            _backArea = new Rectangle(new Point(988, 900), new Size(220, 82));
            _nextArea = new Rectangle(new Point(757, 900), new Size(171, 82));
            
            _transControl = new TransformationsController();
            var questionnaire = ScreensControllerAccessor.ScreensController.Questionnaire;
            _transControl.Initialize(ProjectAccessor.Project.ProjectDir + questionnaire.Id + Path.DirectorySeparatorChar + "picture_transformation_config.xml");
        }

        //protected override void MouseDownHandler(object sender, MouseEventArgs e)
        //{
        //    base.MouseDownHandler(sender, e);

        //    if (InArea(e.X, e.Y, _nextArea))
        //    {
        //        if (_picture.Image != null)
        //        {
        //            Next();
        //        }
        //    }
        //    else if (InArea(e.X, e.Y, _backArea))
        //    {
        //        Back();
        //    }
        //}

        public override void Recycle()
        {
            base.Recycle();
            _picture.Image = null;
        }

        public override void SetActivated()
        {
            base.SetActivated();

            // On fait la transformation maintenant.
            var resp = ScreensControllerAccessor.ScreensController.GetValidatedCurrentRespondent() as Respondent;

            var questionnaire = ScreensControllerAccessor.ScreensController.Questionnaire;
            var question = questionnaire.GetQuestion(4);
            var choiceText = (ChoiceText)question.GetChoice(1);
            var answer = new Answer{QuestionType = QuestionTypeEnum.Photo, QuestionNo = question.No, ChoiceNo = choiceText.No, Response = choiceText.GetAnswerText() };

            var tmp = answer.Response;

            var liste = new List<Answer> {answer};

            resp.Answers = liste;
            var trans = _transControl.GetTransformations(resp, false);
            trans[0].Respondent = resp;
            var originale = trans[0].TransformImage();
            _picture.Image = originale;
            _picture.SizeMode = PictureBoxSizeMode.StretchImage;

            answer.Response += ".jpg";
            choiceText.SetAnswer(answer);
            originale.Save(answer.Response, ImageFormat.Jpeg); // On écrase le fichier par le nouveau

            trans[0].Dispose();

            File.Delete(tmp);
        }
    }
}
