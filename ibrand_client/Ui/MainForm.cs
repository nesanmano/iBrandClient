using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NLog;
using NomadLogic.Common.Serveur;
using NomadLogic.Common.Services;
using NomadLogic.Common.Services.Configuration;
using NomadLogic.Common.Services.LocalCapabilities;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Win32;
using NomadLogic.iBrandDesktop.Services;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen;
using NomadLogic.iBrandDesktop.Util;
using NomadLogic.Multimedia.Cli.Ffmpeg;
using NomadLogic.Multimedia.Gpu.OpenCV;
using NomadLogic.Multimedia.Gpu.OpenGL;

// ReSharper disable InconsistentNaming
// ReSharper disable RedundantArgumentDefaultValue
#pragma warning disable 618

// ReSharper disable UnusedMember.Local

namespace NomadLogic.iBrandDesktop.Ui
{
    /// <summary>
    /// Classe pour la mise en place des configuration de photobooth.
    /// </summary>
    public partial class MainForm : Form, ILocalCapabilityTester
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private readonly SplashForm _splashForm;
        private TouchScreenController _screensController;
        private int _globalKeyHandle1;
        private int _globalKeyHandle2;
        private bool _hidden;
        private bool _inFront = true;

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool IsWindow(IntPtr hWnd);

        /// <summary>
        /// Constructeur principal.
        /// </summary>
        public MainForm(TerminalInfo ti)
        {
            _splashForm = new SplashForm();
            _splashForm.Show();
            InitializeComponent();

            // TODO Déplacer dans le lifecycle d'écran correct.
            LoadFullScreenDefinition(ti);

            TouchModeConfigurationDlg.LanguageChanged += _screensController.OnConfigurationLanguageChanged;

            Load += RegisterHandles;
        }

        private void RegisterHandles(object sender, EventArgs e)
        {
            _globalKeyHandle1 = FormUtil.RegisterGlobalHotKey(this, Keys.F2, WinUserConstants.RH_PARAMS.ALT);

            _globalKeyHandle2 = FormUtil.RegisterGlobalHotKey(this, Keys.F3, WinUserConstants.RH_PARAMS.ALT);
        }

        public new void Dispose()
        {
            base.Dispose();

            FormUtil.UnregisterGlobalHotKey(this, _globalKeyHandle1);
            FormUtil.UnregisterGlobalHotKey(this, _globalKeyHandle2);
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WinUserConstants.WM_HOTKEY_MSG_ID)
            {
                FormUtil.GetKeysAndModifiersFromHotkeyMessage(m, out var keyPreview, out _);

                if (keyPreview == Keys.F3)
                {
                    FormUtil.SetWindowPos(this,
                        _inFront ? WinUserConstants.HWND_CONSTANTS.HWND_BOTTOM : WinUserConstants.HWND_CONSTANTS.HWND_TOPMOST, 0, 0, 0, 0,
                        WinUserConstants.SWP_PARAMS_UFLAGS.SWP_NOMOVE | WinUserConstants.SWP_PARAMS_UFLAGS.SWP_NOSIZE);
                    _inFront = !_inFront;
                }
                else
                {
                    if (_hidden)
                    {
                        FormUtil.SetWinHidden(Handle, false);
                    }
                    else
                    {
                        FormUtil.SetWinHidden(Handle);
                    }
                    _hidden = !_hidden;
                }
            }
            base.WndProc(ref m);
        }

        #region baseCode

        private void LoadFullScreenDefinition(TerminalInfo ti)
        {
            var db = ServicesContainer.GetService<IbrandDatabase>();

            var prj = db.GetProject(ti.ProjectId);

            var loader = new JujuQuestionnaireLoader(ti);
            var questionnaire = loader.Load(true);
            var iBrandEvent = new IbrandEventInfo(-1, "obsolete", ti.QuestionnaireId);

#pragma warning disable 612
            var sc = new ScreenConfig(ti)
#pragma warning restore 612
            {
                ProjectNo = ti.ProjectId.ToString(),
                EventId = iBrandEvent?.Id,
                ForceFileCheck = true
            };

            _screensController = new TouchScreenController(prj, iBrandEvent, "PC", questionnaire, screen_panel);

            ScreensControllerAccessor.ScreensController = _screensController;

            try
            {
                // Charge la recette réflexivement
                var className = questionnaire.LoaderClass;
                var t = ClassUtil.CheckIfClassExists(className, "NomadLogic.iBrandDesktop.Ui.ApplicationScreen.FullscreenLoaders", typeof(IFullscreenLoader));
                if (t?.FullName == null)
                {
                    throw new System.Exception("Inexisting class for full screen loading: " + className);
                }

                if (t.Assembly.CreateInstance(t.FullName) is IFullscreenLoader recetteInstance)
                {
                    recetteInstance.CreateScreens(_screensController, sc);
                }

                _screensController.ShowFirstScreen(false);
            }
            catch (System.Exception e)
            {
                db.SetTerminalAsFaulty(ti);
                db.ClearActive();
                log.Error(e, "Chargement de la classe recette à échoué. On enlève le projet actif.");
                throw;
            }
        }

        public IList<ILocalCapabilityVerifier> CapabilityVerifiers { get; set; }
        
        public void VerifyCapabilities()
        {
            var config = ServicesContainer.GetService<NomadConfiguration>();

            // Assigne les vérificateurs de forme avec la référence de cet object
            foreach (var cv in CapabilityVerifiers)
            {
                if (!(cv is ILocalCapabilityVerifierForm cvf)) continue;
                cvf.ParentForm = this;
            }
            
            LocalCapabilitiesManager.AnalyseCapabilities(CapabilityVerifiers, config, capResult =>
            {
                if (capResult.Groups.All(g => g.Status == StatusEnum.Valid)) return;
                var sb = new StringBuilder();
                sb.AppendLine(Resources.iBrand.UI_IncompleteReport);
                foreach (var gr in capResult.Groups)
                {
                    foreach (var message in gr.ErrorMessages)
                    {
                        sb.AppendLine(message);
                    }
                }

                Invoke(new Action(() =>
                {
                    var diagResult = MessageBox.Show(this, sb.ToString(),
                        Resources.iBrand.UI_MissingCapabilities, MessageBoxButtons.YesNo);
                    if (diagResult == DialogResult.Yes)
                    {
                        Application.Exit();
                    }
                }));
            });
        }
        
        /// <inheritdoc />
        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            _splashForm.Close();
        }

        /// <inheritdoc />
        protected override void OnClosing(CancelEventArgs e)
        {
            log.Trace("OnClosing Invoked");
            if (_screensController == null || _screensController.Disposed)
            {
                log.Trace("Screen controller already disposed.");
                base.OnClosing(e);
                return;
            }

            // Fermeture lancée, mais pas terminée.
            if (_screensController.WaitingForExit)
            {
                log.Trace("On Waiting Close.");
                e.Cancel = true;
                return;
            }

            _screensController.FlagForClosing();

            Task.Run(() =>
            {
                log.Trace("Closing task run activation.");
                try
                {
                    // TODO BUG Avec t5i, dans certaines circonstances, hang indéfiniment.
                    //var canonManager = CanonCameraManager.GetInstance();
                    //canonManager.ShutDown();
                }
                catch (System.Exception exception)
                {
                    log.Error(exception);
                }
                finally
                {
                    log.Trace("Closing questionnaire Invoked");
                    _screensController.QuestionnaireClosed += (sender, args) =>
                    {
                        if (IsWindow(Handle))
                        {
                            Invoke(new Action(Close));
                        }
                    };
                    _screensController.CloseQuestionnaire();
                }
            });

            // On ne sort pas immédiatement. On va ré-invoquer cette méthode (au niveau du parent)
            // Pour compléter la fermeture de la forme.
            e.Cancel = true;
        }

        /// <inheritdoc />
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            VerifyCapabilities();
            
            WorkingArea.Container = this;
            CanonCameraManager.MainFormWindow = this;

            var config = ServicesContainer.GetService<NomadConfiguration>();
            
            var tests = new List<ILocalCapabilityVerifier>()
            {
                new LocalCapabilityVerifierAssets(),
                new LocalCapabilityVerifierVlc(),
                new LocalCapabilityVerifierFfmpeg(),
                new LocalCapabilityVerifierOpenGl
                {
                    ParentForm = this
                },
                new LocalCapabilityVerifierOpenCv()
            };
            
            LocalCapabilitiesManager.AnalyseCapabilities(tests, config, capResult =>
            {
                if (capResult.Groups.All(g => g.Status == StatusEnum.Valid)) return;
                var sb = new StringBuilder();
                sb.AppendLine("Some capabilities were detected as invalid. Would you like to stop executing? You can check the capabilities report in the local directory for more information.");
                foreach (var gr in capResult.Groups)
                {
                    foreach (var message in gr.ErrorMessages)
                    {
                        sb.AppendLine(message);
                    }
                }

                Invoke(new Action(() =>
                {
                    var diagResult = MessageBox.Show(this, sb.ToString(),
                        $@"{config.ApplicationName}- Missing capabilities", MessageBoxButtons.YesNo);
                    if (diagResult == DialogResult.Yes)
                    {
                        Application.Exit();
                    }
                }));
            });
        }

        public void ResizeElementsForMaximize()
        {
            screen_panel.Size = Size;
        }

        #endregion baseCode
    }
}