using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using NLog;
using NomadLogic.Common.Services;
using NomadLogic.Common.Services.Configuration;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Math;
using NomadLogic.Common.Util.Win32;
using NomadLogic.iBrandDesktop.Services;
using NomadLogic.iBrandDesktop.Synchro.Report;
using static NomadLogic.Common.Services.LongRunningStatus;

namespace NomadLogic.iBrandDesktop.Synchro
{
    public partial class SynchronizationMonitorUi : Form, IProgress<SynchronizationReport>
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        private bool _liveSettingsChanged;
        private SynchronizationService _syncService;

        public SynchronizationMonitorUi()
        {
            InitializeComponent();
        }

        private void SynchronizationMonitorUi_Load(object sender, EventArgs e)
        {
            var config = ServicesContainer.GetService<NomadConfiguration>();
            _syncService = ServicesContainer.GetService<SynchronizationService>();
            _chkLiveSynchro.Checked = config.LiveSynchro;

            if (!_chkLiveSynchro.Checked)
            {
                _btnPause.Enabled = false;
            }

            Report(new SynchronizationReport());
            
            _syncService.AddObserver(this);
            _syncService.StatusChanged += OnSyncStatusChanged;
            _lblStatusInfo.Text = _syncService.Status.ToString();

            _lblSubmitActionClassName.Text = StringUtil.FormatEnumerable(_syncService.GetActiveSubmitActionList());
            _syncService.SendLastReport();
        }

        private void OnSyncStatusChanged(object sender, LongRunningStatus e)
        {
            this.SafeCall(() => { _lblStatusInfo.Text = e.ToString(); });
        }

        private void SynchronizationMonitorUi_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_liveSettingsChanged)
            {
                var config = ServicesContainer.GetService<NomadConfiguration>();
                config.LiveSynchro = _chkLiveSynchro.Checked;
                NomadConfiguration.SaveConfigurationInstanceToDisk(config);
                log.Debug("Saved Nomad nofiguration to disk: Live synchro is {0}", config.LiveSynchro ? "enabled." : "disabled.");
            }

            _syncService.StatusChanged -= OnSyncStatusChanged;
            _syncService.RemoveObserver(this);
        }

        private async void ChkLiveSynchro_CheckedChanged(object sender, EventArgs e)
        {
            _liveSettingsChanged = true;
            var status = _syncService.Status;
            var chk = _chkLiveSynchro.Checked;
            if (!chk && status != Paused)
            {
                await _syncService.Pause();
                _btnPause.Enabled = false;
            }
            else if (chk && (status == Paused || status == ServiceCreated))
            {
                await _syncService.StartLongRunning();
                _btnPause.Enabled = true;
            }
        }

        public void Report(SynchronizationReport value)
        {
            if (value == null) return;
            this.SafeCall(() =>
            {
                _lblErrorNumber.Text = value.ErrorNumber.ToString();

                var dateString = value.ReportTime.ToString("HH:MM:ss");
                var txtToAdd = new List<string>(2);
                if (!string.IsNullOrWhiteSpace(value.ErrorMessage))
                {
                    txtToAdd.Add($@"[{dateString}] Error: {value.ErrorMessage}");
                }
                if (!string.IsNullOrWhiteSpace(value.Message))
                {
                    txtToAdd.Add($@"[{dateString}] {value.Message}");
                }

                if (!string.IsNullOrWhiteSpace(value.ErrorMessage) || !string.IsNullOrWhiteSpace(value.Message))
                {
                    var sb = new StringBuilder();
                    sb.Append(_txtMessages.Text);
                    foreach (var txt in txtToAdd)
                    {
                        sb.AppendLine(txt);
                    }
                    _txtMessages.Text = sb.ToString();
                }

                _lblRemainToSync.Text = value.RemainToSync.ToString();
                _lblTotalSinceStart.Text = value.TotalSync.ToString();

                _lblTotalData.Text = MathUtil.FormatByteNumber(value.TotalData, true);

                if (value.CurrentUpload != null)
                {
                    _prbCurrentUpload.Minimum = value.CurrentUpload.Min;
                    _prbCurrentUpload.Maximum = value.CurrentUpload.Max;
                    _prbCurrentUpload.Value = value.CurrentUpload.Val;
                    _prbCurrentUpload.Step = value.CurrentUpload.Step;
                }
                else
                {
                    _prbCurrentUpload.Minimum = 0;
                    _prbCurrentUpload.Maximum = 10;
                    _prbCurrentUpload.Value = 0;
                    _prbCurrentUpload.Step = 1;
                }
            });
        }

        private async void BtnPause_Click(object sender, EventArgs e)
        {
            var status = _syncService.Status;
            if (status == Stopped)
            {
                _btnPause.Text = @"Stopped";
                _btnPause.Enabled = false;
                return;
            }
            if (status != Paused)
            {
                await _syncService.Pause();
                _btnPause.Text = @"Start";
            }
            else if (status == Paused || status == ServiceCreated)
            {
                await _syncService.StartLongRunning();
                _btnPause.Text = @"Pause";
            }
        }
    }
}
