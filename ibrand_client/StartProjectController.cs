using System;
using System.Windows.Forms;
using NLog;
using NomadLogic.Common.Serveur;
using NomadLogic.Common.Services;
using NomadLogic.iBrandDesktop.Services;
using NomadLogic.iBrandDesktop.Ui;
using NomadLogic.iBrandDesktop.Util;

namespace NomadLogic.iBrandDesktop
{
    public class StartProjectController
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private delegate void StartDelegate();
        
        [Obsolete]
        private readonly Project _project;
        private readonly KioskForm _mainForm;

        private StartProjectDialog _startProjectDlg;
        private TerminalInfo _terminal;

        public StartProjectController(Project project, KioskForm mainForm, TerminalInfo ti)
        {
            _terminal = ti;
            _project = project;
            _mainForm = mainForm;
        }

        public void Start()
        {
            if (_mainForm.ActiveQuestionnaire) return;
            if (_mainForm.InvokeRequired)
            {
                try
                {
                    _mainForm.Invoke(new StartDelegate(Start));
                } catch (System.Exception e) {
                    log.Error(e, "Cannot start Project.");
                    throw new System.Exception("Exception during launch.", e);
                }
                return;
            }

            _startProjectDlg = new StartProjectDialog(_project) {Owner = _mainForm};

            _startProjectDlg.OnOkClick += OnOkClick;
            _startProjectDlg.ShowDialog();
        }

        private void OnOkClick(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;

            _mainForm.ActiveQuestionnaire = true;

            var qInfo = _startProjectDlg.SelectedQuestionnaireInfo;

            var questionnaireLoader = new JujuQuestionnaireLoader(_terminal);
            var questionnaire = questionnaireLoader.Load(true);
            var syncServ = ServicesContainer.GetService<SynchronizationService>();
            syncServ?.AssignSubmitAction(qInfo.ProjectId, questionnaire.SubmitAction);

            questionnaire.SetQuestionsLanguage();

            var screenController = new ScreenController(_project, _startProjectDlg.SelectedIbrandEventInfo, _startProjectDlg.UserName, questionnaire);
            screenController.QuestionnaireLoaded += QuestionnaireLoaded;
            screenController.QuestionnaireClosed += QuestionnaireClosed;


            QuestionnaireControllerAccessor.QuestionnaireController = screenController;
            ProjectAccessor.Project = _project;

            _startProjectDlg.Close();

            //questionnaire.StartLookForPictureOrVideoThread();

            screenController.LoadScreens();
            screenController.Show(_mainForm);
        }

        private void QuestionnaireLoaded(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.Default;        
        }

        private void QuestionnaireClosed(object sender, EventArgs e)
        {
            _mainForm.RestoreMainMenu();
            _mainForm.ActiveQuestionnaire = false;
            QuestionnaireControllerAccessor.QuestionnaireController = null;
            ProjectAccessor.Project = null;
        }
        
    }
}
