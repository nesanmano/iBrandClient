using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using JetBrains.Annotations;
using NLog;
using NomadLogic.Common.Serveur;
using NomadLogic.Common.Services;
using NomadLogic.Common.Services.Configuration;
using NomadLogic.Common.Services.LocalCapabilities;
using NomadLogic.Common.Ui.Localization;
using NomadLogic.Common.Util;
using NomadLogic.iBrandDesktop.Services;
using NomadLogic.iBrandDesktop.Synchro;
using NomadLogic.iBrandDesktop.Ui.Synchro.New_UI;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen;
using NomadLogic.iBrandDesktop.Util;
using NomadLogic.Multimedia.Gpu.OpenCV;
using NomadLogic.Multimedia.Gpu.Ui.BackgroundPictureDialog;
using NomadLogic.Multimedia.Gpu.Ui.ChromaKeyDialogs;

namespace NomadLogic.iBrandDesktop.Ui
{
    public partial class KioskForm : LocalizedForm, ILocalCapabilityTester
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private delegate void AddProjectsLaunchItemsDelegate();

        private Label _noTerminalLabel;
        private ToolStrip _activeToolStrip;
        private string _baseDirAssets;
        private string _basicLogo;
        private string _activeLogo;
        private string _faultyLogo;

        private GetProjectTerminal _projectTerminal;

        /// <inheritdoc />
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            VerifyCapabilities();
                
            InitMenus();

            AddNoTerminalLabel();

            terminalsLauncherItemsList.LargeImageList = launcherImages;
            terminalsLauncherItemsList.LargeImageList.ImageSize = new Size(60, 60);
            terminalsLauncherItemsList.View = View.LargeIcon;
            
            WorkingArea.Container = this;
            WorkingArea.StatusStrip = statusStrip;
#pragma warning disable 618
            CanonCameraManager.MainFormWindow = this;
#pragma warning restore 618

            _baseDirAssets = DirectoryUtil.GetLocalNomadLogicAssetsDir();
            _basicLogo = _baseDirAssets + "nomad_logo.png";
            _activeLogo = _baseDirAssets + "nomad_logo_selected.png";
            _faultyLogo = _baseDirAssets + "x_browser.png";

            if (!File.Exists(_activeLogo) || !File.Exists(_faultyLogo))
            {
                log.Warn("Some assets file does not exists. Please update the Nomad asset directory.");
                _activeLogo = _baseDirAssets + "red_signal.png";
            }

            PopulateTerminalsLaunchItems();
        }

        private void InitMenus()
        {
            var fileMenuItem = new ToolStripMenuItem { Text = Resources.iBrand.FileMenu };
            menuStrip.Items.Add(fileMenuItem);

            var uploadDataMenuItem = new ToolStripMenuItem { Text = @"Synchronization Monitor" };
            uploadDataMenuItem.Click += (sender, args) => new SynchronizationMonitorUi().Show();

            fileMenuItem.DropDownItems.Add(uploadDataMenuItem);

            var aboutMenuItem = new ToolStripMenuItem { Text = Resources.iBrand.AboutMenu };
            aboutMenuItem.Click += OnAbout;

            menuStrip.Items.Add(aboutMenuItem);

            var deleteProjectMenuItem = new ToolStripMenuItem { Text = Resources.iBrand.DeleteProjectsMenu };
            var deleteProjectsController = new DeleteProjectsController(this);
            deleteProjectMenuItem.Click += deleteProjectsController.OnDeleteProjects;
            deleteProjectsController.OnCompleted += OnDeleteTerminalsCompleted;

            fileMenuItem.DropDownItems.Add(deleteProjectMenuItem);

            var configMenuItem = new ToolStripMenuItem { Text = Resources.iBrand.ConfigurationMenu };
            configMenuItem.Click += OnConfiguration;

            fileMenuItem.DropDownItems.Add(configMenuItem);

            var configChromaKey = new ToolStripMenuItem { Text = Resources.iBrand.ChromaConfig };
            configChromaKey.Click += OnChromaConfig;

            fileMenuItem.DropDownItems.Add(configChromaKey);

            var configBasePicture = new ToolStripMenuItem { Text = @"Base picture settings..." };
            configBasePicture.Click += OnBasePictureSettingClick;

            fileMenuItem.DropDownItems.Add(configBasePicture);

            var bundleCalibration = new ToolStripMenuItem { Text = Resources.iBrand.MENU_BundleCalibration };
            bundleCalibration.Click += OnBundleCalibration;
            
            fileMenuItem.DropDownItems.Add(bundleCalibration);

            var updateProjectsMenuItem = new ToolStripMenuItem { Text = Resources.iBrand.GetProjectMenu };
            /*var getProjectController = new GetProjectController(this);
            updateProjectsMenuItem.Click += getProjectController.OnGetProject;    //TODO: separer les concerns: controlleur doit etre proprement associe a sa vue
            getProjectController.OnCompleted += OnGetProjectCompleted;*/

            updateProjectsMenuItem.Click += OnGetProjectUpdateClick;

            fileMenuItem.DropDownItems.Add(updateProjectsMenuItem);

            var exitMenuItem = new ToolStripMenuItem { Text = Resources.iBrand.ExitMenu };
            exitMenuItem.Click += (s, e) => Application.Exit();
            
            fileMenuItem.DropDownItems.Add(exitMenuItem);
            
            //LOCALIZATION
            deleteProjectMenuItem.Tag = "DeleteProjectsMenu";
            configMenuItem.Tag = "ConfigurationMenu";
            updateProjectsMenuItem.Tag = "GetProjectMenu";
            fileMenuItem.Tag = "FileMenu";
            exitMenuItem.Tag = "ExitMenu";
            uploadDataMenuItem.Tag = "UploadDataMenu";
            aboutMenuItem.Tag = "AboutMenu";
            configChromaKey.Tag = "ChromaConfig";
            bundleCalibration.Tag = "MENU_BundleCalibration";
            configBasePicture.Tag = "UI_BasePictureSettingsButton";
        }


        void OnGetProjectUpdateClick(object sender, EventArgs e)
        {
            _projectTerminal = new GetProjectTerminal {Owner = this};
            _projectTerminal.OnCompleted += OnGetTerminalCompleted;
            _projectTerminal.Show();

        }

        private void OnBasePictureSettingClick(object sender, EventArgs e)
        {
            var config = ServicesContainer.GetService<NomadConfiguration>();

            if (config.ForceUsageOfFfmpeg || !config.IsCapabilityReportGroupValid(CapabilityStatusGroupEnum.OpenCv))
            {
                MessageBox.Show(@"No OpenCv settings detected. This feature requires a working OpenCv Environement to work.", @"iBrand",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                var bpmd = new BackgroundPictureManagerDialog
                {
                    Owner = this
                };
                bpmd.ShowDialog();
            }
        }

        private static void OnBundleCalibration(object sender, EventArgs e) {
            new ImageBundleCalibrationPickerDialog(null).ShowDialog();
        }

        private void OnChromaConfig(object sender, EventArgs e)
        {
            var config = ServicesContainer.GetService<NomadConfiguration>();
            
            if (config.ForceUsageOfFfmpeg || !config.IsCapabilityReportGroupValid(CapabilityStatusGroupEnum.OpenCv))
            {
                var chromaDialog = new PreviewDialogChromaKeyFfmpeg
                {
                    Owner = this
                };
                chromaDialog.ShowDialog();
            }
            else
            {
                var chromaDialog = new PreviewDialogChromaKeyOpenCv
                {
                    Owner = this
                };
                chromaDialog.ShowDialog();
            }
        }

        /// <inheritdoc />
        protected override void InitializeControlTags()
        {
            // rien, les tags sont mis ailleurs.
        }

        /// <inheritdoc />
        protected override void UpdateCustomControls()
        {
            foreach (ToolStripMenuItem item in menuStrip.Items.Cast<object>().Where(item => item is ToolStripMenuItem))
            {
                var tag = item.Tag?.ToString() ?? "UNKNOWN_TAG";

                item.Text = GetLocalizedTextFromManager(tag);
                foreach (ToolStripItem subItem in item.DropDownItems)
                {
                    if (subItem.Tag == null) continue;
                    tag = subItem.Tag.ToString();
                    subItem.Text = GetLocalizedTextFromManager(tag);
                }
            }
        }

        /// <inheritdoc />
        protected override string GetLocalizedTextFromManager(string tag)
        {
            return Resources.iBrand.ResourceManager.GetString(tag, Resources.iBrand.Culture);
        }

        private void PopulateTerminalsLaunchItems()
        {
            if (InvokeRequired)
            {
                Invoke(new AddProjectsLaunchItemsDelegate(PopulateTerminalsLaunchItems));
                return;
            }

            var db = ServicesContainer.GetService<IbrandDatabase>();

            if (db == null) return;

            var terminaux = db.GetTerminals();
            var currentTerminal = db.GetActive()?.TerminalId ?? 0;

            terminalsLauncherItemsList.Items.Clear();

            if (terminaux.Count == 0)
            {
                SetTerminalsLauncherVisible(false);
                _noTerminalLabel.Visible = true;
                AddNoTerminalLabel();
            }
            else
            {
                SetTerminalsLauncherVisible(true);
                _noTerminalLabel.Visible = false;

                foreach (var t in terminaux)
                {
                    var isFaulty = db.IsTerminalFaulty(t);
                    AddTerminalLaunchItem(t, currentTerminal, isFaulty);
                }
            }
        }

        private void SetTerminalsLauncherVisible(bool visible)
        {
            terminalsLauncherItemsList.Visible = visible;
        }

        public void SetToolStrip(ToolStrip toolStrip)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => { SetToolStripDo(toolStrip); }));
            }
            else
            {
                SetToolStripDo(toolStrip);
            }
        }

        private void SetToolStripDo(ToolStrip toolStrip)
        {
            _activeToolStrip = toolStrip;
            Controls.Remove(menuStrip);
            Controls.Add(toolStrip);

        }

        public void RestoreMainMenu()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(RestoreMainMenuDo));
            }
            else
            {
                RestoreMainMenuDo();
            }
        }

        private void RestoreMainMenuDo()
        {
            //this.Controls.Remove(MainMenuStrip);
            Controls.Add(menuStrip);
            if (_activeToolStrip != null)
            {
                Controls.Remove(_activeToolStrip);
                _activeToolStrip = null;
            }
            MainMenuStrip = menuStrip;
        }

        /// <summary>
        /// Indique si on a un questionnaire d'actif.
        /// </summary>
        public bool ActiveQuestionnaire { get; set; }

        private void AddNoTerminalLabel()
        {
            _noTerminalLabel = new Label
            {
                Size = new Size(200, 80),
                ForeColor = Color.Red,
                Text = Resources.iBrand.NoProjectAvailable,
                Location = new Point(5, 35),
                Visible = false,
                Name = "NoProjectAvailable"
            };

            Controls.Add(_noTerminalLabel);
        }

        private void AddTerminalLaunchItem([NotNull] TerminalInfo ter, int currentTerminal, bool faulty)
        {
            var li = new ListViewItem(ter.TerminalName) { Tag = ter };

            string name;
            
            // Projet par défaut
            if (faulty)
            {
                name = _faultyLogo;
            }
            else if (ter.TerminalId == currentTerminal)
            {
                name = _activeLogo;
            }
            else
            {
                name = _basicLogo;
            }

            var bitmap = FileUtil.LoadBitmap(name);

            if (bitmap != null)
            {
                launcherImages.Images.Add(bitmap);
                li.ImageIndex = launcherImages.Images.Count - 1;
            }

            terminalsLauncherItemsList.Items.Add(li);
        }

        private void OnGetTerminalCompleted(object sender, EventArgs e)
        {
            PopulateTerminalsLaunchItems();
        }

        private void OnConfiguration(object sender, EventArgs e)
        {
            using (var config = new ConfigurationDlg {Owner = this})
            {
                config.LanguageChanged += OnChangeLanguageKiosk;
                config.ShowDialog();
            }
        }

        private void OnChangeLanguageKiosk(object sender, EventArgs e)
        {
            OnChangeLanguage(sender, e);
        }

        private void OnAbout(object sender, EventArgs e)
        {
            using (var aboutDlg = new AboutDlg {Owner = this})
            {
                aboutDlg.ShowDialog();
            }
        }

        private void OnDeleteTerminalsCompleted(object sender, EventArgs e)
        {
            PopulateTerminalsLaunchItems();
        }

        private void TerminalItemActivate(object sender, EventArgs e)
        {
            var db = ServicesContainer.GetService<IbrandDatabase>();
            var problem = false;
            var majorError = false;
            var lvi = terminalsLauncherItemsList.Items[terminalsLauncherItemsList.SelectedIndices[0]];
            
            if (!(lvi.Tag is TerminalInfo ter))
            {
                // Vraiment pas supposé se produire.
                log.Error("No valid terminal selected. Cancelling operation.");
                MessageBox.Show(@"FATAL - Cannot select this terminal for unknown reasons.", @"Terminal cannot be used.", MessageBoxButtons.OK);
                return;
            }

            var isFullScreen = false;

            // On change le terminal actif.
            
            // 1- On charge le projet
            var project = db.GetProject(ter.ProjectId);
            var questInfo = project.KeepOnlySpecificQuestionnaire(ter.QuestionnaireId);
            questInfo.TerminalID = ter.TerminalId;
            
            try
            {
                // On valide chaque Questionnaire, et on met à jour ses métadonnées, si non existantes.
                // 1- Le questionnaire est-il plein écran? ET si oui, existe-il une classe de recette valide?
                // 2- Le questionnaire existe-il sur disque?
                // 2b- Le dernier chargement de ce questionnaire à t-il été correct? (aucune erreur).
                
                // 2 est implicitement vérifié ici, en chargeant le questionnaire. S'il y a un problème une exception est lancée.
                var qLoader = new JujuQuestionnaireLoader(ter);
                var q = qLoader.Load(false);

                if (q.IsFullScreen)
                {
                    isFullScreen = true;
                    db.UpdateQuestionnaireFullSCreen(questInfo, true);
                    db.SetStartFullScreen();

                    var className = q.LoaderClass;
                    var t = ClassUtil.CheckIfClassExists(className, "NomadLogic.iBrandDesktop.Ui.ApplicationScreen.FullscreenLoaders", typeof(IFullscreenLoader));
                    if (t == null)
                    {
                        log.Error("There is no valid loader class named '{0}' in assembly.", className);
                        problem = true;
                    }
                }
            }
            catch (System.Exception err)
            {
                log.Error(err, $"Could not load questionnaire for terminal '{ter}'. Questionnaire with problems: {ter.QuestionnaireId}");
                majorError = true;
            }

            if (!problem && !majorError)
            {
                // Si oui, on indique ce terminal comme départ plein écran.
                db.SetActive(ter.TerminalId);
            }
            else
            {
                db.SetTerminalAsFaulty(ter);
                db.SetStartFullScreen(false);
                MessageBox.Show(@"This terminal cannot load and as such cannot be used. Try re-downloading it.", @"Terminal cannot be used.", MessageBoxButtons.OK);
                return;
            }
            
            if (!isFullScreen)
            {
                StartAsKioskTerminal(project, ter);
            }
            else
            {
                PopulateTerminalsLaunchItems();
                var msgBoxRes = MessageBox.Show(@"Start as fullscreen? (This instance will close)", @"Fullscreen terminal detected", MessageBoxButtons.YesNoCancel);
                switch (msgBoxRes)
                {
                    case DialogResult.No:
                        StartAsKioskTerminal(project, ter);
                        return;
                    case DialogResult.Yes:
                        // Termine l'application
                        Application.Exit();
                        break;
                }
            }
        }

        private void StartAsKioskTerminal(Project project, TerminalInfo ti)
        {
            // On part le projet uniquement si le terminal est le terminal actif.
            var spc = new StartProjectController(project, this, ti);
            var newThread = new Thread(spc.Start);
            newThread.Start();
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
    }
}