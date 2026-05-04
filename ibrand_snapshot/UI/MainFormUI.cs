using System;
using System.Collections.Concurrent;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using NLog;
using NomadLogic.iBrand.Common.Util;

namespace iBrand_SnapShot.UI
{
    struct LogEventData
    {
        public LogEventData(string message, Exception ex)
        {
            Message = message;
            Ex = ex;
        }

        public string Message;
        public Exception Ex;
    }

    struct ControlStatusData
    {
        public ControlStatusData(Control ctrl, Color col, string text, bool enabled)
        {
            Ctrl = ctrl;
            Col = col;
            Text = text;
            Enabled = enabled;
        }

        public Control Ctrl;
        public Color Col;
        public string Text;
        public bool Enabled;
    }

    public partial class MainFormUi : Form, IEventLogger
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        internal delegate void LogEventDelegate(LogEventData data);
        internal delegate void SetStatusDelegate(ControlStatusData data);
        internal delegate void ResetSettingsValuesDelegate(SnapShotSettings settings);

        /* Liste des Contrôles accessibles de l'extérieur. */
        public Control FtpStatusCtrl => FtpStatus;

        public Control CustomWorkerStatusCtrl => CustomWorkerStatus;
        public Control DisplayAliveCtrl => DisplayAlive;
        public Control CamStatusCtrl => CamStatus;
        public Control CamActivateBtnCtrl => CamActivateBtn;
        public Control CamTestBtnCtrl => CamTestBtn;
        public Control TrigStatusCtrl => TrigStatus;
        public Control TriggerTestBtnCtrl => TriggerTestBtn;
        public Control ProcessFilesBtnCtrl => ProcessFilesBtn;
        public Control FtpTestBtnCtrl => FtpTestBtn;
        public Control SettingsTrigClassCtrl => SettingsTrigClass;
        public Control SettingsWorkerClassCtrl => SettingsWorkerClass;
        public Control SettingsTriggerTimeCtrl => SettingsTriggerTime;

        private SnapShotController _controller;
        private readonly ConcurrentQueue<LogEventData> _logQueue = new ConcurrentQueue<LogEventData>();
        private readonly ConcurrentQueue<ControlStatusData> _statusQueue = new ConcurrentQueue<ControlStatusData>();
        private readonly Timer _timer = new Timer();

        public MainFormUi()
        {
            InitializeComponent();

            _timer.Interval = 200;
            _timer.Tick += UpdateUi;
            _timer.Start();
        }

        private void UpdateUi(object sender, EventArgs e)
        {
            LogEventData dataLog;
            while (_logQueue.TryDequeue(out dataLog))
            {
                LogEventDo(dataLog);
            }

            ControlStatusData dataStat;
            while (_statusQueue.TryDequeue(out dataStat))
            {
                SetStatusDo(dataStat);
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            try
            {
                _controller = new SnapShotController(this);
                _controller.Start();
                _controller.InitCameraFrameWork();
            }
            catch (Exception ex)
            {
                log.Debug("Error during initialization: " + ex.Message + "\r\n" + ex.StackTrace);
            }
        }

        private void TriggerTestBtn_Click(object sender, EventArgs e)
        {
            if (_controller.TriggerTestMode)
            {
                _controller.TriggerTestMode = false;
                TriggerTestBtn.Text = @"Test Trigger";
                TrigStatus.Text = @"Waiting for trigger";
            }
            else
            {
                _controller.TriggerTestMode = true;
                TriggerTestBtn.Text = @"Test Trigger (On)";
                TrigStatus.Text = @"Testing trigger";
            }
        }

        private void FtpTestBtn_Click(object sender, EventArgs e)
        {
            _controller.RequestTestPoolFtp();
        }

        private void CamActivateBtn_Click(object sender, EventArgs e)
        {
            LogEvent("Command sent to trigger camera manually.");
            _controller.SendTriggerEvent();
        }

        private void CamTestBtn_Click(object sender, EventArgs e)
        {
            LogEvent("Camera Test initiated.");
            _controller.InitCameraFrameWork();
        }

        /// <summary>
        /// Modifie la valeur d'un contrôle pour indiquer que sa valeur à changée.
        /// </summary>
        /// <param name="sender">Pas utilisé.</param>
        /// <param name="e">Pas utilisé.</param>
        private void SettingModified(object sender, EventArgs e)
        {
            var @base = sender as TextBoxBase;
            if (@base != null)
            {
                @base.ForeColor = SnapShotController.MODIFIED;
            }
            else if (sender is RadioButton)
            {
                ((RadioButton)sender).ForeColor = SnapShotController.MODIFIED;
            }
        }

        private void SettingsCameraRadioVideo_CheckedChanged(object sender, EventArgs e)
        {
            SettingModified(sender, e);

            if (SettingsCameraRadioVideo.Checked)
            {
                SettingsCameraVideoDuration.Enabled = true;
                SettingsCameraVideoThumbnail.Enabled = true;
                SettingsCameraVideoFileSize.Enabled = true;
            }
            else
            {
                SettingsCameraVideoDuration.Enabled = false;
                SettingsCameraVideoThumbnail.Enabled = false;
                SettingsCameraVideoFileSize.Enabled = false;
            }
        }

        /// <summary>
        /// Écrit les configurations dans le fichier.
        /// </summary>
        /// <param name="sender">Non utilisé</param>
        /// <param name="e">Non utilisé</param>
        private void SettingsWriteToFileBtn_Click(object sender, EventArgs e)
        {
            /* Settings qui ne requiert aucune validation. */
            _controller.Settings.UseFtp = SettingsEnableFtp.Checked;
            _controller.Settings.PoolVerboseLog = SettingsVerbose.Checked;
            _controller.Settings.IsCameraPicture = SettingsCameraRadioPicture.Checked;
            _controller.Settings.FtpServer = SettingsFtpServer.Text;
            _controller.Settings.FtpDirectory = SettingsFtpDir.Text;
            _controller.Settings.FtpArchive = SettingsFtpArchive.Text;

            /* Settings qui ont besoin de validation. */
            _controller.Settings.ModifyVideoDuration(SettingsCameraVideoDuration.Text, SettingsCameraVideoDuration, this);
            _controller.Settings.ModifyVideoThumbnail(SettingsCameraVideoThumbnail.Text, SettingsCameraVideoThumbnail, this);
            _controller.Settings.ModifyVideoFileSize(SettingsCameraVideoFileSize.Text, SettingsCameraVideoFileSize, this);
            _controller.Settings.ModifyTriggerParameters(SettingsTrigParams.Text, SettingsTrigParams, this);
            _controller.Settings.ModifyConcurrentWorkersFtp(SettingsWorkerNumberFtp.Text, SettingsWorkerNumberFtp, this);
            _controller.Settings.ModifyConcurrentWorkersCustom(SettingsWorkerNumberCustom.Text, SettingsWorkerNumberCustom, this);
            _controller.ValidateTriggerClassName(SettingsTrigClass.Text);
            _controller.ValidateWorkerClassName(SettingsWorkerClass.Text);
            _controller.ValidateTriggerTime(SettingsTriggerTime.Text);
            _controller.Settings.ModifyLocationFfmpeg(SettingsFfmpegLocation.Text, SettingsFfmpegLocation, this);

            // On vérifie si on doit partir le worker pool.
            _controller.StartWorkerPool();

            /* En dernier, on dit au controlleur de mettre à jour ses configurations. */
            _controller.UpdatePoolConfigs();

            var t = new Task(() => _controller.SaveSettingsToFile());
            t.Start();
        }

        public void LogEvent(string message)
        {
            _logQueue.Enqueue(new LogEventData(message, null));

        }

        public void LogEvent(string message, Exception ex)
        {
            _logQueue.Enqueue(new LogEventData(message, ex));

        }

        private void LogEventDo(LogEventData data)
        {
            if (InvokeRequired)
            {
                Invoke(new LogEventDelegate(LogEventDo), data);
                return;
            }

            try
            {
                if (data.Ex != null)
                {
                    log.Error(data.Ex);

                    if (EventsLog.Text.Length == 0)
                    {
                        EventsLog.Text = data.Message + @" [" + data.Ex.Message + @"]";
                    }
                    else
                    {
                        EventsLog.AppendText("\r\n" + data.Message + " [" + data.Ex.Message + "]");
                    }
                }
                else
                {
                    if (EventsLog.Text.Length == 0)
                    {
                        EventsLog.Text = data.Message;
                    }
                    else
                    {
                        EventsLog.AppendText("\r\n" + data.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex, "Error during Event log update.");
            }
        }

        /// <summary>
        /// Change la couleur et le texte d'un élément visuel.
        /// </summary>
        /// <param name="ctrl">L'élément à modifier.</param>
        /// <param name="col">La couleur du texte. Ne peut être nul (car c'est une structure).</param>
        /// <param name="text">Le texte à afficher.</param>
        /// <param name="enabled"></param>
        public void SetStatus(Control ctrl, Color col, string text, bool enabled)
        {
            _statusQueue.Enqueue(new ControlStatusData(ctrl, col, text, enabled));
        }

        private void SetStatusDo(ControlStatusData data)
        {
            if (InvokeRequired)
            {
                Invoke(new SetStatusDelegate(SetStatusDo), data);
                return;
            }

            if (data.Ctrl != null)
            {
                try
                {
                    data.Ctrl.ForeColor = data.Col;
                    if (data.Text != null) data.Ctrl.Text = data.Text;
                    data.Ctrl.Enabled = data.Enabled;
                }
                catch (Exception ex)
                {
                    log.Debug(ex, "Error during label modification.");
                }
            }
        }

        private void MainFormUI_FormClosing(object sender, FormClosingEventArgs e)
        {
            var t = new Task(() => _controller.Stop());
            t.Start();

            _timer.Stop();
        }

        private void UploadFilesBtn_Click(object sender, EventArgs e)
        {
            var t = new Task(() => _controller.ProcessLeftoverFiles());
            t.Start();
        }

        /// <summary>
        /// N'accepte que les caractères numérique et de contrôle (comme "delete" ou "backspace").
        /// </summary>
        /// <param name="sender">L'envoyeur.</param>
        /// <param name="e">La touche appuyée.</param>
        private void NumberOnly_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        /// <summary>
        /// Appel d'un bouton pour réinitialiser.
        /// </summary>
        private void ResetSettingsValues(object sender, EventArgs e)
        {
            ResetSettingsValues(_controller.Settings);
        }

        /// <summary>
        /// Met les valeurs dans les champs à partir de l'objet de configuration, et donc annule les modifications fait par l'usager aux configurations.
        /// </summary>
        internal void ResetSettingsValues(SnapShotSettings settings)
        {
            if (InvokeRequired)
            {
                Invoke(new ResetSettingsValuesDelegate(ResetSettingsValues), settings);
                return;
            }

            SettingsCameraVideoDuration.Text = settings.VideoDuration.ToString();
            SettingsCameraVideoDuration.ForeColor = SnapShotController.ORIGINAL;

            SettingsCameraVideoThumbnail.Text = settings.VideoThumbnail.ToString();
            SettingsCameraVideoThumbnail.ForeColor = SnapShotController.ORIGINAL;

            SettingsCameraVideoFileSize.Text = settings.VideoFileSize.ToString();
            SettingsCameraVideoFileSize.ForeColor = SnapShotController.ORIGINAL;

            if (settings.IsCameraPicture)
            {
                SettingsCameraRadioPicture.Checked = true;
            }
            else
            {
                SettingsCameraRadioVideo.Checked = true;
            }
            SettingsCameraRadioPicture.ForeColor = SnapShotController.ORIGINAL;
            SettingsCameraRadioVideo.ForeColor = SnapShotController.ORIGINAL;

            SettingsFtpArchive.Text = settings.FtpArchive;
            SettingsFtpArchive.ForeColor = SnapShotController.ORIGINAL;

            SettingsFtpDir.Text = settings.FtpDirectory;
            SettingsFtpDir.ForeColor = SnapShotController.ORIGINAL;

            SettingsFtpServer.Text = settings.FtpServer;
            SettingsFtpServer.ForeColor = SnapShotController.ORIGINAL;

            SettingsTrigClass.Text = settings.TriggerClassName;
            SettingsTrigClass.ForeColor = _controller.IsTriggerInitialized ? SnapShotController.ORIGINAL : SnapShotController.NOT_READY;

            SettingsTrigParams.Text = StringUtil.MergeString(settings.TriggerParameters, ",");
            SettingsTrigParams.ForeColor = SnapShotController.ORIGINAL;

            SettingsFtpServerBkp.Text = settings.BackupFtpServer;
            SettingsFtpServerBkp.ForeColor = SnapShotController.ORIGINAL;

            SettingsFfmpegLocation.Text = settings.LocationFfmpeg;
            SettingsFfmpegLocation.ForeColor = SnapShotController.ORIGINAL;

            SettingsVerbose.Checked = settings.PoolVerboseLog;

            SettingsTriggerTime.Text = settings.TimeTriggerWindow;
            SettingsTriggerTime.ForeColor = SnapShotController.ORIGINAL;

            SettingsWorkerClass.Text = settings.CustomWorker;
            SettingsWorkerClass.ForeColor = SnapShotController.ORIGINAL;

            SettingsEnableFtp.Checked = settings.UseFtp;

            SettingsWorkerNumberFtp.Text = settings.ConcurrentWorkersFtp.ToString();
            SettingsWorkerNumberFtp.ForeColor = SnapShotController.ORIGINAL;

            SettingsWorkerNumberCustom.Text = settings.ConcurrentWorkersCustom.ToString();
            SettingsWorkerNumberCustom.ForeColor = SnapShotController.ORIGINAL;
        }
    }
}
