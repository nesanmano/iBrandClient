using System.Drawing;
using System.Windows.Forms;
using NomadLogic.Common.Extentions;
using NomadLogic.Common.Services;
using NomadLogic.Common.Util;
using NomadLogic.iBrandDesktop.Services;
using NomadLogic.iBrandDesktop.Util;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.custom.MonsterEnergy
{
    public class ShareScreen : AbstractScreen
    {
        // ReSharper disable once UnusedMember.Local
        private static readonly NLog.Logger log = NLog.LogManager.GetCurrentClassLogger();
        readonly PictureBox _picture;

        private readonly Rectangle _doneArea;
        private readonly Rectangle _printArea;

        private Respondent _respondent;

        public ShareScreen(string backgroundImgEnSrc, ScreenConfig sc)
            : base(backgroundImgEnSrc, backgroundImgEnSrc, sc)
        {
            //PictureBox.MouseDown += MouseDownHandler;
            //PictureBox.Paint += PaintEvent;
            _picture = new PictureBox
            {
                Location = new Point(396, 109),
                Size = new Size(476, 476),
                Visible = true,
                SizeMode = PictureBoxSizeMode.StretchImage
            };

            Controls.Add(_picture);

            _picture.BringToFront();

            _doneArea = new Rectangle(new Point(517, 601), new Size(226, 88));
            _printArea = new Rectangle(new Point(905, 286), new Size(122, 122));
        }

        //protected override void MouseDownHandler(object sender, MouseEventArgs e)
        //{
        //    base.MouseDownHandler(sender, e);

        //    if (InArea(e.X, e.Y, _doneArea))
        //    {
        //        Next();
        //    }
        //    else if (InArea(e.X, e.Y, _printArea))
        //    {
        //        var fileName = StringUtil.GetFileName(GetTextAnswerFromQuestion(1, 1));
        //        var printFile =
        //            DirectoryUtil.GetLocalIbrandQuestionnaireDataDir(_screenConfig.ProjectId, _screenConfig.QuestionnaireId,
        //                DirectoryUtil.ProjectDataDirectoriesEnum.pictures,
        //                DirectoryUtil.ProjectDataDirectoriesModEnum.transformed) + fileName;
                
        //        if (!PrintUtil.Print(printFile, null, this)) return;

        //        IncrementEventInQuestionnaire(2, 1);

        //        // Force la mise à jour de la réponse, car le répondant est déjà sauvegardé.
        //        var db = ServicesContainer.GetService<IbrandDatabase>();
        //        var txt = GetTextAnswerFromQuestion(2, 1);
        //        var id = int.Parse(_respondent.Id);
        //        var ans = new Answer(4, 2, 1, txt);
        //        if (!txt.Equals("1"))
        //        {
        //            db.DeleteAnswer(id, ans);
        //        }
        //        db.InsertAnswer(id, ans);
        //    }
        //}

        public override void Recycle()
        {
            base.Recycle();

            _picture.Image?.Dispose();
            _picture.Image = null;

            _respondent = null;
        }

        public override void SetActivated()
        {
            base.SetActivated();
            try
            {
                var questionnaire = GetCurrentQuestionnaire();

                var tmp = FileUtil.LoadBitmap(GetTextAnswerFromQuestion(1));
                _picture.Image = tmp.ResizeForView(_picture.Width, _picture.Height);

                if (_respondent != null) return;

                var db = ServicesContainer.GetService<IbrandDatabase>();
                
                // Sauvegarde du répondant dans notre questionnaire.
                _respondent = ScreensControllerAccessor.ScreensController.GetValidatedCurrentRespondent() as Respondent;
                _respondent.Language = _activeLg;
                _respondent.Answers = questionnaire.GetAnswersToSaveInDb();

                var repId = db.InsertRespondent(_respondent, true);
                IsRespondentAlreadySaved = true;
                _respondent.Id = repId;
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(WorkingArea.Container, ex.Message, Resources.iBrand.AppName, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
            }
        }
    }
}
