using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using NLog;
using NomadLogic.Common;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Services;
using NomadLogic.Common.Services.Configuration;
using NomadLogic.Common.Ui.Dialog;
using NomadLogic.Common.Util;
using NomadLogic.iBrandDesktop.Services;
using NomadLogic.iBrandDesktop.Ui.Question;
using NomadLogic.iBrandDesktop.Util;

namespace NomadLogic.iBrandDesktop.Ui
{    
    public class ScreenController : AbstractController
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        public event EventHandler QuestionnaireLoaded;
        
        private readonly List<ScreenControl> _questionsControls = new List<ScreenControl>();
        private ScreenControl _activeControl;
        private LogoControl _logoControl;
        private readonly ScreensContainer _screensContainer;

        public ScreenController(Project project, IbrandEventInfo eventt, string userName, Questionnaire qt)
            : base(project, eventt, userName, qt)
        {
            _screensContainer = new ScreensContainer(qt);

            AddEventsHandler();
            
            InitLogoControl(Questionnaire.LogoFile);

            AddTotalRespondents();
            
            if (Questionnaire.SubmitAction != null)
            {
                var syncServ = ServicesContainer.GetService<SynchronizationService>();
                syncServ.AssignSubmitAction(project.Id, Questionnaire.SubmitAction);

                Questionnaire.SubmitAction.Init(Questionnaire.Terminal, null);
                Questionnaire.SubmitAction.OnRespondentSaved += RespondentSaved;
                Questionnaire.SubmitAction.StartWorkerPools();
            }

            // Ajout d'un écoutant sur une demande de prochaine page venant d'un élément du questionnaire.
            Questionnaire.OnNextPageRequest += NextPageDo;

            if (Questionnaire.HasLeaderBoard)
                Questionnaire.InitLeaderBoard();

        }
        
        private void AddEventsHandler()
        {
            _screensContainer.Load += OnQuestionsContainerLoad;
            _screensContainer.OnClose += OnCloseQuestionnaire;
            _screensContainer.OnLoadLastRespondent += OnLoadLastRespondentAnswers;
            _screensContainer.OnChangeLanguage += OnChangeLanguage;
        }

        private void RemoveEventsHandler()
        {
            _screensContainer.Load -= OnQuestionsContainerLoad;
            _screensContainer.OnClose -= OnCloseQuestionnaire;
            _screensContainer.OnLoadLastRespondent -= OnLoadLastRespondentAnswers;
            _screensContainer.OnChangeLanguage -= OnChangeLanguage;
        }

        private void OnQuestionsContainerLoad(object sender, EventArgs e)
        {
            QuestionnaireLoaded?.Invoke(this, EventArgs.Empty);
        }

        private void RespondentSaved()
        {
            AddTotalRespondents();
        }

        private void OnChangeLanguage(string language)
        {
            Questionnaire.LanguageCurrent = language;

            foreach (var screenControl in _questionsControls)
            {
                screenControl.RemoveElements();
                screenControl.AddScreenElements(WorkingArea.Size.Height - _screensContainer.ToolStrip.Size.Height - WorkingArea.StatusStrip.Size.Height - -SystemInformation.CaptionHeight, Questionnaire.GetLastVisibleQuestionNo());
            }
        }

        private void OnNextPage(object sender, EventArgs e)
        {
            NextPageDo();
        }

        /// <summary>
        /// Contient la logique pour passer à la prochaine page.
        /// </summary>
        private void NextPageDo()
        {
            if (_activeControl == null)
                return;

            if (!_activeControl.Validate(_screensContainer)) return;

            var index = _questionsControls.IndexOf(_activeControl);

            //Verifier que la prochaine page a au moins 1 question active.
            index = GetNextIndexWithActiveQuestion(index);

            if (index == _questionsControls.Count - 1)//Last page
            {
                Cursor.Current = Cursors.WaitCursor;

                if (!Submit())
                {
                    Cursor.Current = Cursors.Default;
                    return;
                }

                Questionnaire.Reset();

                Cursor.Current = Cursors.Default;

                if (!ShowLogo())
                    SetFirstScreen();

                return;
            }

            index++;

            SetNextPage(index);
        }

        private int GetNextIndexWithActiveQuestion(int index)
        {
            for (var i = index; i < _questionsControls.Count - 1; i++)
            {
                var control = _questionsControls[i + 1];

                if (control.HasFocusableQuestion())
                    return i;
            }

            return _questionsControls.Count - 1;
        }

        private bool Submit()
        {
            if (Questionnaire.SubmitAction == null) return SaveQuestionnaire();

            var respondent = new Respondent
            {
                EventId = IbrandEventInfo.Id,
                Representative = UserName,
                Language = Questionnaire.GetRespondentLanguage(),
                ProjectId = Project.Id,
                QuestionnaireId = Questionnaire.Id,
                TerminalId = Questionnaire.Terminal.TerminalId,
                DateLocal = DateTime.Now,
                Answers = Questionnaire.GetAnswersToSaveInDb(),
                DynamicHeaders = Questionnaire.GetDynamicHeadersToSaveInDb()
            };

            var resultat = Questionnaire.SubmitAction.SubmitRespondent(respondent);

            return resultat;
        }

        public override IRespondent GetValidatedCurrentRespondent()
        {
            if (_activeControl == null) return null;

            if (!_activeControl.Validate(_screensContainer)) return null;
            var respondent = new Respondent
            {
                EventId = IbrandEventInfo.Id,
                Representative = UserName,
                Language = Questionnaire.GetRespondentLanguage(),
                ProjectId = Project.Id,
                QuestionnaireId = Questionnaire.Id,
                TerminalId = Questionnaire.Terminal.TerminalId,
                DateLocal = DateTime.Now,
                Answers = Questionnaire.GetAnswersToSaveInDb(),
                DynamicHeaders = Questionnaire.GetDynamicHeadersToSaveInDb()
            };
            return respondent;
        }

        private void AddTotalRespondents()
        {
            var db = ServicesContainer.GetService<IbrandDatabase>();
            var totalRespondent = db.GetTotalRespondentsToSync(Questionnaire.Id);
            WorkingArea.StatusStrip.RightMessage = Resources.iBrand.TotalRespondentMenu + totalRespondent;
        }

        private bool SaveQuestionnaire()
        {
            try
            {
                var answers = Questionnaire.GetAnswersToSaveInDb();
                var respondent = new Respondent
                {
                    EventId = IbrandEventInfo.Id,
                    Representative = UserName,
                    Language = Questionnaire.GetRespondentLanguage(),
                    ProjectId = Project.Id,
                    QuestionnaireId = Questionnaire.Id,
                    TerminalId = Questionnaire.Terminal.TerminalId,
                    DateLocal = DateTime.Now,
                    Answers = answers,
                    DynamicHeaders = Questionnaire.GetDynamicHeadersToSaveInDb()
                };
                var db = ServicesContainer.GetService<IbrandDatabase>();
                db.InsertRespondent(respondent, true);
                RespondentSaved();
            }
            catch (System.Exception)
            {
                return false;
            }

            return true;
        }

        // ReSharper disable once UnusedMember.Local
        private void SaveBackup()
        {
            //Save database file
            var conf = ServicesContainer.GetService<NomadConfiguration>();

            var backupDbFileName = DirectoryUtil.GetBackupDir(conf) +StringUtil.GetFileName(conf.Ibrand.DbIbrandFileName);

            if (File.Exists(backupDbFileName))
                File.Delete(backupDbFileName);

            File.Copy(conf.Ibrand.DbIbrandFileName, backupDbFileName);
            //Save video file
            if (!Directory.Exists(DirectoryUtil.GetBackupDir(conf) + "videos"))
                Directory.CreateDirectory(DirectoryUtil.GetBackupDir(conf) + "videos");

            var videoAnswers = Questionnaire.GetVideoQuestionsAnswers();
            foreach (var answer in videoAnswers)
            {
                var backupVideoFileName = DirectoryUtil.GetBackupDir(conf) + "videos" + Path.DirectorySeparatorChar + StringUtil.GetFileName(answer.Response);

                if (File.Exists(backupVideoFileName))
                    File.Delete(backupVideoFileName);

                File.Copy(answer.Response, backupVideoFileName);
            }
        }

        private void SetNextPage(int index)
        {
            if (index >= _questionsControls.Count) return;

            _activeControl.Visible = false;

            _activeControl = _questionsControls[index];

            _activeControl.Visible = true;
            _activeControl.Invalidate();
        }

        private void OnBackPage(object sender, EventArgs e)
        {
            if (_activeControl == null)
                return;

            var index = _questionsControls.IndexOf(_activeControl);

            index--;

            if (index != -1)
            {
                _activeControl.Visible = false;

                _activeControl = _questionsControls[index];

                _activeControl.Visible = true;
                _activeControl.Invalidate();
            }
        }

        private void OnCloseQuestionnaire(object sender, EventArgs e)
        {
            if (WaitingForExit) return; // Si on est déjà en train de fermer le questionnaire.
            var result = MsgBox.Show(_screensContainer, Resources.iBrand.CloseQuestionnaire, Resources.iBrand.AppName, MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
            if (result != DialogResult.Yes) return;

            StartExitSequence();
            //CloseController();
        }
        
        protected override void CloseController()
        {
            if (_screensContainer.InvokeRequired)
            {
                _screensContainer.Invoke(new Action(CloseControllerDo));
            }
            else
            {
                CloseControllerDo();
            }
        }

        private void CloseControllerDo()
        {
            WorkingArea.StatusStrip.RightMessage = "";

            RemoveEventsHandler();

            _screensContainer.Dispose();

            Questionnaire.Dispose();

            OnQuestionnaireClosed();
        }

        [SuppressMessage("ReSharper", "UnusedParameter.Local")]
        private void OnLoadLastRespondentAnswers(object sender, EventArgs e)
        {
            var db = ServicesContainer.GetService<IbrandDatabase>();
            var lastRespondentId = db.GetLastInsertedRespondentId();
            var lastRespondentAnswers = db.GetAnswers(lastRespondentId);
            foreach (var answer in lastRespondentAnswers)
            {
                var question = Questionnaire.GetQuestion(answer.QuestionNo);
                if (!(question is QuestionPhoto))
                    question.LoadAnswerInUiElement(answer);
            }

            // TODO Groupes dynamiques?
        }

        public void LoadScreens()
        {
            var uiLoader = new UiLoader(this, Questionnaire, Project);
            uiLoader.Load();
        }

        public void AddScreenControl(ScreenControl screenControl)
        {
            if (screenControl.Handle == IntPtr.Zero)
            {
                // ReSharper disable once UnusedVariable
                var tmp = screenControl.Handle;
            }

            screenControl.Parent = _screensContainer.ScreenPanel;
            var lastQno = Questionnaire.GetLastVisibleQuestionNo();
            screenControl.AddScreenElements(_screensContainer.ScreenPanel.Size.Height, lastQno);

            screenControl.OnBack += OnBackPage;
            screenControl.OnNext += OnNextPage;

            _questionsControls.Add(screenControl);
        }
        
        private void SetFirstScreen()
        {
            try
            {
                if (_activeControl != null)
                    _activeControl.Visible = false;

                Questionnaire.Reset();
                _activeControl = _questionsControls[0];

                _activeControl.Visible = true;
                _activeControl.Invalidate();
            }
            catch (System.Exception ex)
            {
                log.Fatal(ex);
            }
        }

        private void InitLogoControl(string logoFileName)
        {
            if (logoFileName != null)
            {
                Bitmap bitmap = null;
                if (File.Exists(Project.ProjectDir + logoFileName))
                {
                    var stream = new FileStream(Project.ProjectDir + logoFileName, FileMode.Open);
                    bitmap = new Bitmap(stream);
                    stream.Close();
                }

                if (bitmap != null)
                {
                    _logoControl = new LogoControl(bitmap);

                    _logoControl.OnLogoClick += OnLogoClick;
                    _logoControl.Parent = _screensContainer;
                }
            }
            else
            {
                InitDefaultLogoControl();
            }
        }

        private void InitDefaultLogoControl()
        {
            if (!File.Exists(Constants.DEFAULT_LOGO_FILE)) return;

            var streamV = new FileStream(Constants.DEFAULT_LOGO_FILE, FileMode.Open);
            var bitmap = new Bitmap(streamV);
            streamV.Close();

            _logoControl = new LogoControl(bitmap);

            _logoControl.OnLogoClick += OnLogoClick;
            _logoControl.Parent = _screensContainer;
        }

        private void OnLogoClick(object sender, EventArgs e)
        {
            _logoControl.Visible = false;
            SetFirstScreen();
        }

        private bool ShowLogo()
        {
            if (_activeControl != null)
                _activeControl.Visible = false;

            if (_logoControl == null) return false;
            _screensContainer.AutoScroll = false;

            _logoControl.BringToFront();
            _logoControl.Visible = true;

            return true;
        }

        public void Show(Form mainForm)
        {
            mainForm.Controls.Add(_screensContainer);

            ((KioskForm)mainForm).SetToolStrip(_screensContainer.ToolStrip);
            _screensContainer.BringToFront();

            if (!ShowLogo())
                SetFirstScreen();
        }

        public override void ShowMessageBox(string message, string caption)
        {
            MsgBox.Show(_screensContainer, message, caption);
        }

        public override void ShowMessageBox(string message)
        {
            MsgBox.Show(_screensContainer, message);
        }

        public virtual void FlagForClosing()
        {
            // Ne fait rien car les éléments ne sont pas responsive ici.
        }
    }
}
