using System.Drawing;
using System.Drawing.Text;
using System.Threading;
using NLog;
using NomadLogic.Common;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Services;
using NomadLogic.Common.Util;
using NomadLogic.iBrandDesktop.Services;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers.ImageTransforms;
using NomadLogic.iBrandDesktop.Util.SubmitAction.BaseClasses;

namespace NomadLogic.iBrandDesktop.Util.SubmitAction
{
    class Esh2018Car03SubmitAction : AbstractSubmitAction
    {
        // ReSharper disable once UnusedMember.Local
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        protected override void InstanciateSubmitActionPools()
        {
            AddSubmitActionWorkerPool("transform", 1);
        }

        protected override ISubmitActionWorker CreateWorker(Respondent resp, bool usePool = true)
        {
            return new Esh2018Car03SubmitActionWorker(this, resp, resp.Id);
        }

        public PrivateFontCollection CustomFontCollection { get; } = new PrivateFontCollection();


        private class Esh2018Car03SubmitActionWorker : ISubmitActionWorker
        {
            private static readonly Logger logWorker = LogManager.GetCurrentClassLogger();
            private readonly Respondent _respondent;
            private readonly Esh2018Car03SubmitAction _action;
            private readonly int _respId;

            public Esh2018Car03SubmitActionWorker(Esh2018Car03SubmitAction action, Respondent resp, int respId)
            {
                _respondent = resp;
                _action = action;
                _respId = respId;
            }

            public void DoWork(CancellationToken? ct)
            {
                if (ct?.IsCancellationRequested ?? false) return;

                var db = ServicesContainer.GetService<IbrandDatabase>();

                try
                {
                    var pId = _action.ProjectId;
                    var qId = _action.QuestionnaireId;

                    var baseDir = _action.GetTerminalDirectory;

                    var langResp = int.Parse("1"); // 1 = anglais, 2 = français
                    var langMod = langResp == 2 ? "" : "_en";

                    var uniqueName = GenerateUniqueId.GetId() + ".jpg";

                    var imgFile = DirectoryUtil.GetLocalIbrandTerminalDataDir(_action.TerminalInfo,
                                      DirectoryUtil.ProjectDataDirectoriesEnum.pictures,
                                      DirectoryUtil.ProjectDataDirectoriesModEnum.none) + uniqueName;

                  


                    var options = new ImageListTransformOptionsFirstPicture
                    {
                        JpegSaveQuality = 90,
                        CanvasSize = new Size(1920, 1280),
                        CanvasBaseColor = Color.Black,
                        NewPos = new Point(0, 0),
                        NewSize = new Size(1920, 1280),
                        NewRotateFlip = RotateFlipType.Rotate180FlipNone
                    };

                    //ImageListTransform.CreateFirstPicture(_respondent, new List<string> { Directory.GetFiles(_respondent.GetAnswerText(1))[0]}, imgFile, options);


                    var ansPic = new Answer{QuestionType = QuestionTypeEnum.PhotoText, QuestionNo = 1, Response = uniqueName};
                    //db.DeleteAnswer(_respId, ansPic);
                    db.InsertAnswer(_respId, ansPic);

                    // 5- On finalise le répondant
                    db.SetRespondentReadyToSync(_respId);
                    _action.UiSynchro?.AddUiMessage("Respondent Synchro finished.", 4);

                }
                catch (System.Exception ex)
                {
                    logWorker.Fatal(ex, "Error while executing treatment of respondent.");
                    _action.UiSynchro?.AddUiMessage("Error while treating respondent.", 4);
                }
            }



            public ISubmitActionWorker NextWorkerStep { get; set; }

            public ISubmitAction GetSubmitAction()
            {
                return _action;
            }

            public int GetRespId()
            {
                return _respId;
            }
        }
    }
}
