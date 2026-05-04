using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Windows.Forms;
using JetBrains.Annotations;
using NLog;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Services;
using NomadLogic.Common.Services.ActivityLogger;
using NomadLogic.Common.Services.Configuration;
using NomadLogic.Common.Util;
using NomadLogic.Common.Ui.NomadScreen;
using NomadLogic.Common.Util.Win32;
using NomadLogic.iBrandDesktop.Services;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen;
using NomadLogic.iBrandDesktop.Util.BarcodeScanner;

namespace NomadLogic.iBrandDesktop.Ui
{
    public class TouchScreenController : AbstractController
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        protected readonly List<IAbstractScreen> _screenList = new List<IAbstractScreen>();
        private readonly Dictionary<string, AbstractPseudoModalScreen> _pseudoModalScreens = new Dictionary<string, AbstractPseudoModalScreen>();
        protected IAbstractScreen _activeScreen;
        protected readonly Panel _screenPanel;

        private readonly object _lock = new object();
        
        public TouchScreenController(Project project, IbrandEventInfo eventt, string userName, Questionnaire questionnaire, Panel screenPanel)
            : base(project, eventt, userName, questionnaire)
        {
            _screenPanel = screenPanel;

            if (Questionnaire.SubmitAction == null) return;
            
            _syncService?.AssignSubmitAction(project.Id, Questionnaire.SubmitAction);

            Questionnaire.SubmitAction.Init(Questionnaire.Terminal, null);
            Questionnaire.SubmitAction.OnRespondentFinish += OnSubmitActionFinished;
            Questionnaire.SubmitAction.StartWorkerPools();
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        private void OnSubmitActionFinished(int respondentId)
        {
            SetNewRespondentEvent();
        }

        public string QuestionnaireDir => Project.ProjectDir + Questionnaire.Id + Path.DirectorySeparatorChar;
        
        public override IRespondent GetValidatedCurrentRespondent()
        {
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

        protected virtual void OnNextScreen(int nbrPage)
        {
            lock (_lock)
            {
                if (_activeScreen == null)
                    return;

                if (log.IsTraceEnabled)
                {
                    log.Trace("On change. Answers={0}", StringUtil.FormatDictionary(Questionnaire.GetAnswersDictionary()));
                }

                var index = _screenList.IndexOf(_activeScreen);

                if (_activeScreen.IsSaveOnThisPage)
                {
                    log.Trace("Saving questionnaire as instructed by the current screen ({0})", index);
                    SaveQuestionnaire(_activeScreen.SkipSubmitActions);
                }

                var nextIndex = index + nbrPage;
                var maxIndex = _screenList.Count - 1;

                // Si c'est la dernière page ou si on saute trop de page (passé son compte de page), on remet à neuf les écrans.
                if (nextIndex > maxIndex)
                {
                    log.Trace("End of screen sequence reached ({0}/{1}).", nextIndex, maxIndex);

                    // On sauvegarde si la page n'est pas indiquée comme sauvegardable, mais que l'on termine le questionnaire quand même
                    // Habituellement à cause d'un appel à End()
                    if (!_activeScreen.IsRespondentAlreadySaved) // Pour ne pas sauver deux fois, en somme
                    {
                        SaveQuestionnaire(_activeScreen.SkipSubmitActions);
                    }
                    else
                    {
                        SetNewRespondentEvent();
                    }

                    Cursor.Current = Cursors.WaitCursor;
                    
                    RecycleScreens();
                    
                    Cursor.Current = Cursors.Default;

                    ShowFirstScreen(false);
                    
                    return;
                }
                
                log.Trace("Showing screen index={0}", nextIndex);
                SetNextScreen(nextIndex);
            }
        }

        public void SaveQuestionnaire(bool skipSubmitActions)
        {
            if (log.IsTraceEnabled)
            {
                log.Trace("Save questionnaire initiated. Answers={0}", StringUtil.FormatDictionary(Questionnaire.GetAnswersDictionary()));
            }

            if (!Questionnaire.ValidateAnswers())
            {
                ShowMessageBox(Resources.iBrand.QuestionnaireValidationFailed, Resources.iBrand.AppName);
                return;
            }

            log.Trace("Validation succeded");

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
            
            if (Questionnaire.SubmitAction != null && !skipSubmitActions)
            {
                // Une soumission de répondant doit inscrire ce dernier dans la base de données.
                Questionnaire.SubmitAction.SubmitRespondent(respondent);
                log.Trace("Respondent sent to submit action: " + respondent.Id);
            }
            else
            {
                var db = ServicesContainer.GetService<IbrandDatabase>();
                db.InsertRespondent(respondent, true);
                db.SetRespondentReadyToSync(respondent.Id);
                log.Trace("Respondent saved: " + respondent.Id);
                SetNewRespondentEvent();
            }
        }

        protected void RecycleScreens()
        {
            foreach (var screen in _screenList)
            {
                screen.Recycle();
            }

            log.Trace("Screens recycled.");
        }

        /// <summary>
        /// Provoque la séquence de fermeture.
        /// </summary>
        public void CloseQuestionnaire()
        {
            if (WaitingForExit || Disposed) return;

            StartExitSequence();
        }

        public virtual void FlagForClosing()
        {
            foreach (var s in _screenList)
            {
                s.FlagForClosing();
            }
        }

        protected override void CloseController()
        {
            if (_screenPanel != null && _screenPanel.InvokeRequired)
            {
                _screenPanel.Invoke(new Action(CloseControllerDo));
            }
            else
            {
                CloseControllerDo();
            }
        }

        private void CloseControllerDo()
        {
            log.Trace("Exit sequence final execution reached.");

            foreach (var screen in _screenList)
            {
                try
                {
                    screen.Dispose();
                }
                catch (System.Exception e)
                {
                    log.Error(e, "Error in dispose method of  object: {0}", screen.GetType().FullName);
                }
            }

            Questionnaire.Dispose();
            log.Trace("Screen and questionnaire disposed.");

            Disposed = true;

            OnQuestionnaireClosed();
        }

        public bool Disposed { get; private set; }

        public override void ShowMessageBox(string message, string caption)
        {
            MessageBox.Show(message, caption, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
        }

        public override void ShowMessageBox(string message)
        {
            MessageBox.Show(message);
        }
        
        public void ShowFirstScreen(bool isCancel = true)
        {
            if (_activeScreen != null && _activeScreen.InvokeRequiredForm())
            {
                // we were called on a worker thread
                // marshal the call to the user interface thread
                _activeScreen.InvokeForm(delegate { ShowFirstScreen(isCancel); });
                return;
            }

            log.Trace("Show first screen activated.");

            ClearMouseClickQueue();

            Questionnaire.Reset();

            if (_activeScreen != _screenList[0])
            {
                _activeScreen = _screenList[0];
                _activeScreen.BringToFront();
                _activeScreen.Invalidate();
                _screenPanel.SafeCall(() => _activeScreen.SetActivated());
            }

            _screenPanel.Invalidate();
            if (isCancel) LogActivity(Questionnaire.Terminal.TerminalId, ActivityLoggerActionType.Canceled);
        }

        /// <summary>
        /// Ajoute l'écran à la liste, et fait les liaison d'usage (changement d'écran et événement).
        /// Dans quelque cas précis, si les bonnes interfaces sont utilisées, on peut lier des événements et liens externes supplémentaires,
        /// au cas par cas.
        /// </summary>
        /// <param name="screen">L'écran à ajouter, à la fin de la liste existante.</param>
        public virtual void AddScreen(IAbstractScreen screen)
        {
            _screenList.Add(screen);

            if (screen.ParentForm != _screenPanel)
            {
                screen.ParentForm = _screenPanel;
            }

            screen.Visible = true;
            screen.OnFirstScreen += OnFirstScreen;
            screen.OnNext += OnNextScreen;
            screen.OnBack += OnBackScreen;
            screen.OnChangeToPseudoModal += ChangeToPseudoModalScreen;

            if (screen is ILanguageScreenInterface lsi)
            {
                lsi.OnLanguageSelection += OnLanguageSelection;
            }

            if (screen is IBarcodeScannerListener barScreen)
            {
                Questionnaire.AddScannerListener(barScreen);
            }

            screen.Init();
        }

        public void AddPseudoModalScreen(AbstractPseudoModalScreen screen, string tag)
        {
            _pseudoModalScreens.Add(tag, screen);
            screen.OnReturn += OnBackFromPseudoModal;
        }

        public void ChangeToPseudoModalScreen(string tag)
        {
            if (_pseudoModalScreens.TryGetValue(tag, out var modalScreen))
            {
                // Peut être invoqué de thread non-ui.
                if (modalScreen.InvokeRequired)
                {
                    modalScreen.Invoke(new Action(() => { ChangeToPseudomodalDo(modalScreen); }));
                }
                else
                {
                    ChangeToPseudomodalDo(modalScreen);
                }
            }
            else
            {
                log.Warn("There is no pseudomodal screen tagged " + tag);
            }
        }

        private void ChangeToPseudomodalDo(AbstractPseudoModalScreen modalScreen)
        {
            var index = _screenList.IndexOf(_activeScreen);
            modalScreen.ShowPseudoModal(index);
            modalScreen.BringToFront();
            modalScreen.Invalidate();
            _screenPanel.Invalidate();
        }

        private void OnBackFromPseudoModal(int nbrPage)
        {
            SetNextScreen(nbrPage);
        }

        /// <summary>
        /// Effectue le changement de langue sur tous les écrans du gestionnaire.
        /// </summary>
        /// <param name="language">Le code de langue à utiliser.</param>
        public void OnLanguageSelection(string language)
        {
            Questionnaire.LanguageCurrent = language;
            
            foreach (var screen in _screenList)
            {
                screen.ChangeLanguage(language);
            }
        }

        protected void SetNextScreen(int index)
        {
            if (_activeScreen != null && _activeScreen.InvokeRequiredForm())
            {
                // we were called on a worker thread
                // marshal the call to the user interface thread
                _activeScreen.InvokeForm(SetNextScreen, index);
                return;
            }

            if (index >= _screenList.Count || index == -1) return;

            _activeScreen = _screenList[index];
            _activeScreen.SetActivated();
            _activeScreen.BringToFront();
            _activeScreen.Invalidate();
            _screenPanel.Invalidate();
        }

        [StructLayout(LayoutKind.Sequential)]
        [PublicAPI]
        public struct NativeMessage
        {
            public IntPtr handle;
            public uint msg;
            public IntPtr wParam;
            public IntPtr lParam;
            public uint time;
            public Point p;
        }

        [SuppressUnmanagedCodeSecurity]
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("User32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool PeekMessage(out NativeMessage message, IntPtr handle, uint filterMin, uint filterMax, uint flags);

        private static void ClearMouseClickQueue()
        {
            while (PeekMessage(out _, IntPtr.Zero, 0x0200, 0x02df, 1))
            {
            }
        }

        private void OnFirstScreen(object sender, EventArgs e)
        {
            log.Trace("Reset screens activated.");
            
            RecycleScreens();

            ShowFirstScreen();
        }

        protected void OnBackScreen(int nbrPage)
        {
            if (_activeScreen != null && _activeScreen.InvokeRequiredForm())
            {
                // we were called on a worker thread
                // marshal the call to the user interface thread
                _activeScreen.InvokeForm(OnBackScreen, nbrPage);
                return;
            }

            if (_activeScreen == null)
                return;

            var index = _screenList.IndexOf(_activeScreen);

            // Edge case: if the first screen is invoking "back", it already flagged itself as inactive.
            // We must reactivate it because it will cease to respond if not.
            // This does not happen with Next, has a index overflow triggers the questionnaire save sequence.
            if (index == 0)
            {
                _activeScreen.SetActivatedFlag();
                return;
            }

            index -= nbrPage;

            _activeScreen = _screenList[index];

            _activeScreen.BringToFront();
            _activeScreen.Invalidate();
            _activeScreen.SetActivated();
            _screenPanel.Invalidate();
        }
        
        public void OnConfigurationLanguageChanged(object sender, EventArgs e)
        {
            var configuration = ServicesContainer.GetService<NomadConfiguration>();

            Questionnaire.LanguageCurrent = configuration.PreferedLanguage;

            foreach (var screen in _screenList)
            {
                screen.ChangeLanguage(configuration.PreferedLanguage);
            }
        }
    }
}
