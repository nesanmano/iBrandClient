using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using JetBrains.Annotations;
using NLog;
using NomadLogic.Common.Services;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.DateAndTime;
using NomadLogic.Common.Util.Networking;
using NomadLogic.Common.Util.Networking.QuestionnaireWebRequest.Eshots;
using NomadLogic.Common.Util.Win32;

namespace NomadLogic.Common.Ui.Configuration.Eshots
{
    public partial class EshotApiSetup : Form
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        private readonly IDatabaseEshots _database;
        private IList<EshotsCampaign> _backingData;
        private EshotsCampaign _selectedCampaign;
        private EshotsEvent _selectedEvent;
        private EshotsEventDay _selectedEventDay;
        private DateTime? _lastLogin;
        private string _lastUser;
        private CancellationTokenSource _cts;
        private bool _modified;
        private EshotsDataConfiguration _lastDataConfig;
        private EshotsEventDay _loadedEventDay;
        private IList<EshotsEventDay> _cachedDays;
        private string _accessToken;
        private string _refreshToken;

        public EshotApiSetup(IDatabaseEshots eshotsDb)
        {
            InitializeComponent();

            _database = eshotsDb ?? throw new Exception("Cannot create EshotApiSetup form without a valid persistent storage object.");

            Closing += OnClosing;
            _btnLogin.Click += _btnLogin_Click;
        }

        private async void CampaignChange(object sender, EventArgs e)
        {
            _selectedCampaign = _cbCampaign.SelectedItem as EshotsCampaign;
            if (_selectedCampaign == null) return;

            await AddConsoleText("Campaign selected:");
            await AddConsoleText($"\t{_selectedCampaign.CampaignName}");

            await DeselectComboBox(_cbEvent, EventChange);
            await DeselectComboBox(_cbDay, EventDayChange);
            _selectedEvent = null;
            _selectedEventDay = null;
            
            await ModifyComboBox(_cbEvent, EventChange, _selectedCampaign.Events);

            if (_selectedCampaign.Events.Count == 1)
            {
                _cbEvent.SelectedIndex = 0;
            }
        }

        private async void EventChange(object sender, EventArgs e)
        {
            _selectedEvent = _cbEvent.SelectedItem as EshotsEvent;
            if (_selectedEvent == null) return;

            await AddConsoleText("Event selected:");
            await AddConsoleText($"\t{_selectedEvent.FullDescription}");

            await DeselectComboBox(_cbDay, EventDayChange);
            _selectedEventDay = null;

            var days = EshotsEvent.ExtractEventDays(_selectedEvent.EventDays);
            await ModifyComboBox(_cbDay, EventDayChange, days);
            
            // Auto-sélection (en cascade)
            if (_selectedEvent != null)
            {
                // Tente de trouver un élément qui est aujourd'hui
                var ite = 0;
                foreach (var day in days)
                {
                    if (day.Date == DateTime.Today)
                    {
                        _cbDay.SelectedIndex = ite;
                        break;
                    }
                    ite++;
                }
            }
        }

        private async void EventDayChange(object sender, EventArgs e)
        {
            _selectedEventDay = _cbDay.SelectedItem as EshotsEventDay;
            if (_selectedEventDay == null) return;

            var notToday = _selectedEventDay.Date != DateTime.Today;

            await AddConsoleText("Event Day selected:");
            await AddConsoleText($"\t{_selectedEventDay}");
            if (notToday && !_chkAllowAnyDay.Checked)
            {
                await AddConsoleText("Warning: You cannot select a day that is not today with current configuration. Allow any selection of make another choice.");
            }

            if (_selectedEventDay == null)
            {
                _cbDay.ForeColor = Color.Black;
                return;
            }

            // Valide le jour (est-ce aujourd'hui?)
            _cbDay.ForeColor = notToday ? Color.Red : Color.Black;
        }

        private void OnClosing(object sender, CancelEventArgs e)
        {
            if (_modified || _selectedEventDay != _loadedEventDay)
            {
                if (_lastDataConfig == null)
                {
                    _lastDataConfig = new EshotsDataConfiguration();
                }

                // Enregistre les jours actuels
                _lastDataConfig.LastUserName = _lastUser;
                _lastDataConfig.ConfigurationDate = _lastLogin;
                _lastDataConfig.DontForceRefresh = _chkForceSelection.Checked;
                _lastDataConfig.AllowAllDays = _chkAllowAnyDay.Checked;
                _lastDataConfig.AllowPastData = _chkPastDay.Checked;
                _lastDataConfig.AccessToken = _accessToken;
                _lastDataConfig.RefreshToken = _refreshToken;

                var cachedDays = _cachedDays ?? _selectedEvent?.ExtractEventDays();
                _lastDataConfig.EventDaysCached = cachedDays;

                if (MessageBox.Show(@"Eshots configuration data modified. Save to disk?",
                        @"iBrand",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Information) == DialogResult.Yes)
                {

                    try
                    {
                        _database.RegisterCurrentEshotsConfiguration(_lastDataConfig);
                        var config = ServicesContainer.GetService<EshotsApiService>();
                        config.CurrentCampaign = _selectedCampaign;
                        config.CurrentEvent = _selectedEvent;
                        config.CurrentEventDay = _selectedEventDay;
                        config.LastLogin = _lastDataConfig.ConfigurationDate;
                        config.LastUserName = _lastDataConfig.LastUserName;
                        config.ManualDaySelect = _lastDataConfig.DontForceRefresh;
                        config.CanUseLastConfig = _lastDataConfig.AllowAllDays;
                        config.SetFullData(_backingData, cachedDays);
                    }
                    catch (Exception exception)
                    {
                        log.Fatal(exception, "Error during save of eShots configuration.");
                        MessageBox.Show(@"Fatal error occured in database. Application will now close to prevent data loss.", @"iBrand", MessageBoxButtons.OK);
                        Application.Exit();
                    }
                }
            }

            _cts?.Cancel();
        }

        private async void _btnLogin_Click(object sender, EventArgs e)
        {
            var context = TaskScheduler.FromCurrentSynchronizationContext();
            _cts = new CancellationTokenSource();

            var diagResult = await Task.Factory.StartNew(() =>
            {
                var userPassDiag = new UserPasswordModalDialog();
                userPassDiag.ShowDialog();
                return userPassDiag;
            }, _cts.Token, TaskCreationOptions.None, context).ConfigureAwait(true);
            
            if (diagResult == null || diagResult.DialogResult != DialogResult.OK)
            {
                await AddConsoleText("Login not attempted.");
                return;
            }

            var user = diagResult.GetUsername();
            var pass = diagResult.GetPassword();

            if (string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(pass))
            {
                await AddConsoleText("Invalid authentification information. Both must be not empty.");
                return;
            }
            var success = await GetDataFromEshorsServer(user, pass, _cts.Token);
            if (!success)
            {
                await AddConsoleText("Could not get data from serveur.");
                return;
            }

            await UpdateButtons(@"Logout", _lastDataConfig.ConfigurationDate, _lastDataConfig.LastUserName, true);
            await AutoUpdateUiFromData(_cts.Token);
        }

        /// <summary>
        /// Met à jour les combobox selon les résultats obtenus. Pas les boutons!
        /// </summary>
        /// <returns></returns>
        private async Task AutoUpdateUiFromData(CancellationToken ct)
        {
            if (ct.IsCancellationRequested)
            {
                await ModifyComboBox(_cbCampaign, CampaignChange, null);
                await ModifyComboBox(_cbEvent, EventChange, null);
                await ModifyComboBox(_cbDay, EventDayChange, null);

                return;
            }

            // Met à jour le UI et auto-sélectionne les éléments
            await ModifyComboBox(_cbCampaign, CampaignChange, _backingData, _selectedCampaign);
            await ModifyComboBox(_cbEvent, EventChange, _selectedCampaign?.Events, _selectedEvent);
            await ModifyComboBox(_cbDay, EventDayChange, _selectedEvent?.ExtractEventDays(), _selectedEventDay);

            // Auto-sélection (en cascade)
            if (_backingData != null && _backingData.Count == 1)
            {
                _cbCampaign.SelectedIndex = 0; // Provoque une chaîne d'événements.
            }
        }

        private async Task<bool> GetDataFromEshorsServer(string getUsername, string getPassword, CancellationToken ct)
        {
            // On assume que l'on va remplacer l'infomation sur disque quand on lance cette requête.
            _modified = true;

            var facade = new WebRequestEshots();
            if (!await facade.InitializeExecutor())
            {
                await AddConsoleText("Failed to initialize request executor. Please contact support.");
                return false;
            }

            // Auth
            var authInfo = new EshotsAuthentificationRequestInfo
            {
                UserName = getUsername,
                Password = getPassword
            };
            var authResult = await facade.GetFullAuthentication(authInfo, ct);

            if (! await CheckRequestState(authResult, ct, 
                "Unknown error. Please contact support. (NULL authentication response)", 
                "Authorization failed (Code={0}). Please check internet connection or verify authentication credentials."))
            {
                return false;
            }

            var resEvents = await facade.GetEventList(authResult.InstancedBody.AccessToken, _chkPastDay.Checked, ct);
            if (! await CheckRequestState(resEvents, ct,
                "Unknown error. Please contact support. (NULL Events response)",
                "Event list request failed (Code={0})."))
            {
                return false;
            }

            var resEventDaysPresent = await facade.GetEventDaysList(authResult.InstancedBody.AccessToken, _chkPastDay.Checked, ct);
            if (! await CheckRequestState(resEventDaysPresent, ct,
                "Unknown error. Please contact support. (NULL Event Days response)",
                "Event days list request failed (Code={0})."))
            {
                return false;
            }

            // On regarde maintenant les listes et on ajuste.

            var events = resEvents.InstancedBody;

            if (events == null)
            {
                await AddConsoleText("No events was found, even if request succeded.");
                return false;
            }

            _cachedDays = null;
            _backingData = events.Campaigns;
            
            if (_backingData == null || !_backingData.Any())
            {
                await AddConsoleText("No valid campaign data was found after request. Please contact support.");
                return false;
            }

            _lastLogin = DateTime.Now;
            _lastUser = getUsername;
            _accessToken = authResult.InstancedBody.AccessToken;
            _refreshToken = authResult.InstancedBody.RefreshToken;

            _lastDataConfig = new EshotsDataConfiguration
            {
                AccessToken = authResult.InstancedBody.AccessToken,
                RefreshToken = authResult.InstancedBody.RefreshToken,
                LastUserName = _lastUser,
                ConfigurationDate = _lastLogin,
                DontForceRefresh = _chkForceSelection.Checked,
                AllowAllDays = _chkAllowAnyDay.Checked,
                AllowPastData = _chkPastDay.Checked
            };

            await AddConsoleText($"Added {_backingData.Count} campaigns, {_backingData.Sum(c => c.Events.Count)} events and {_backingData.Sum(c => c.Events.Sum(e => e.EventDays.Count))} event days.");

            return true;
        }

        private async Task<bool> CheckRequestState<T>(RestRequestJsonResponse<T> requestJsonResult, CancellationToken ct, 
            string nullResultMessage, string notCorrectStatusMessage)
        {
            if (requestJsonResult == null) // Erreur grave
            {
                await AddConsoleText(nullResultMessage);
                return false;
            }
            if (requestJsonResult.StatusCode != HttpStatusCode.OK)
            {
                await AddConsoleText(string.Format(notCorrectStatusMessage, requestJsonResult.StatusCode));
                return false;
            }

            if (!ct.IsCancellationRequested) return true;
            await AddConsoleText("Operation cancellation requested.");
            return false;
        }

        private async void OnLoad(object sender, EventArgs e)
        {
            var eshotServiceData = ServicesContainer.GetService<EshotsApiService>();
            eshotServiceData = EshotsApiService.InitialInitCheck(eshotServiceData, _database); // Lecture sur disque et initialisation du service (lazy init).
            
            if (eshotServiceData == null)
            {
                await NoLoginUi();
                return;
            }

            _accessToken = eshotServiceData.AccessToken;
            _refreshToken = eshotServiceData.RefreshToken;
            
            _backingData = eshotServiceData.GetFullData();
            _cachedDays = eshotServiceData.GetCachedDays();

            _selectedCampaign = eshotServiceData.CurrentCampaign;
            _selectedEvent = eshotServiceData.CurrentEvent;
            _selectedEventDay = eshotServiceData.CurrentEventDay;

            _loadedEventDay = _selectedEventDay;

            // Un seul élément, un seul
            await ModifyComboBox(_cbCampaign, CampaignChange, _backingData, _selectedCampaign);
            await ModifyComboBox(_cbEvent, EventChange, _selectedCampaign?.Events, _selectedEvent);

            // Si seulement le jour n'est pas null, c'est un chargement de disque et c'est valide.
            var dayList = _selectedEventDay != null ?
                new List<EshotsEventDay> { _selectedEventDay } :
                null;
            var daysData = _cachedDays ?? dayList;

            await ModifyComboBox(_cbDay, EventDayChange, daysData, _selectedEventDay);

            _chkForceSelection.Checked = eshotServiceData.ManualDaySelect;
            _chkAllowAnyDay.Checked = eshotServiceData.CanUseLastConfig;
            _chkPastDay.Checked = eshotServiceData.RequestPastData;

            if (_selectedEventDay == null && (daysData == null || daysData.Count == 0))
            {
                eshotServiceData.LastLogin = null;
                eshotServiceData.LastUserName = null;
                log.Debug("No event day selected. Login needed.");
                await NoLoginUi();
                return;
            }

            _lastLogin = eshotServiceData.LastLogin;
            _lastUser = eshotServiceData.LastUserName;

            await UpdateButtons(@"Logout", eshotServiceData.LastLogin, eshotServiceData.LastUserName, true);
            await AddConsoleText(@"Current configuration loaded.");

            if (eshotServiceData.LastLogin != null)
            {
                var timeRemain = eshotServiceData.LastLogin.Value.AddDays(1) - DateTime.Now;
                await AddConsoleText($@"Current iem remaining for token: {timeRemain.ToStringHourMinSec()}");
            }

            if (eshotServiceData.CurrentEvent == null) await AddConsoleText(@"Only Event day loaded. To change event or campaign, log out then log in.");
            if (eshotServiceData.ManualDaySelect) await AddConsoleText(@"Manual day selection enforced. You must choose a day manually for current session.");

            if (_selectedEventDay != null && !eshotServiceData.CanUseLastConfig && _selectedEventDay.Date != DateTime.Today)
            {
                await AddConsoleText(@"Warning: Current day selection is invalid (not today, and option to use any day not selected).");
                await AddConsoleText(@"Data will not be sent during use unless configuration is changed.");
            }
            
            _modified = false;
        }

        private async Task NoLoginUi()
        {
            await UpdateButtons(@"Login", null, null, false);
            _cbCampaign.Enabled = false;
            _cbEvent.Enabled = false;
            _cbDay.Enabled = false;
            await AddConsoleText(@"No existing configuration. Login to retreive data from the Eshots server.");
        }

        private Task UpdateButtons(string btnLogintext, DateTime? lastLogin, string lastUserName, bool enableClear)
        {
            btnClear.Enabled = enableClear;
            _btnLogin.Text = btnLogintext;
            _lblLastLoginTime.Text = lastLogin?.ToShortDateString() ?? "";
            _lblLastUsername.Text = lastUserName ?? "";
            return Task.CompletedTask;
        }

        private Task AddConsoleText(string newText, bool noNewLine = false)
        {
            this.SafeCall(() => { 
                var sb = new StringBuilder(_textConsole.Text);

                if (noNewLine) sb.Append(newText); else sb.AppendLine(newText);

                _textConsole.Text = sb.ToString();
            });
            return Task.CompletedTask;
        }

        private Task ModifyComboBox(ComboBox cb, EventHandler e, [CanBeNull] IEnumerable<object> items, object toSelect = null)
        {
            if (cb == null || e == null) return Task.CompletedTask;

            cb.SelectedIndexChanged -= e;
            cb.BeginUpdate();

            cb.Items.Clear();

            var ite = 0;
            var index = -1;
            if (items != null)
            {
                foreach (var i in items)
                {
                    if (i == null) continue;
                    if (i.Equals(toSelect))
                    {
                        index = ite;
                    }
                    cb.Items.Add(i);
                    ite++;
                }
            }

            if (index != -1) cb.SelectedIndex = index;

            cb.Enabled = cb.Items.Count != 0;

            cb.EndUpdate();
            cb.SelectedIndexChanged += e;

            return Task.CompletedTask;
        }

        private Task DeselectComboBox(ComboBox cb, EventHandler e, bool clearItems = true)
        {
            cb.SelectedIndexChanged -= e;
            cb.SelectedIndex = -1;
            if (clearItems) cb.Items.Clear();
            cb.Enabled = cb.Items.Count != 0;
            cb.SelectedIndexChanged += e;
            return Task.CompletedTask;
        }

        private async void btnClear_Click(object sender, EventArgs e)
        {
            var context = TaskScheduler.FromCurrentSynchronizationContext();
            _cts = new CancellationTokenSource();

            var diagResult = await Task.Factory.StartNew(() => 
                    MessageBox.Show(@"Clear Eshots authentification and campaign data?", @"iBrand",
                        MessageBoxButtons.OKCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2),
                _cts.Token, TaskCreationOptions.None, context).ConfigureAwait(true);

            if (diagResult != DialogResult.OK)
            {
                await AddConsoleText("Login not attempted.");
                return;
            }

            // Vider les infos.
            _lastDataConfig = null;
            _cachedDays = null;
            _selectedCampaign = null;
            _selectedEvent = null;
            _selectedEventDay = null;
            _backingData = null;
            _lastLogin = null;
            _lastUser = null;

            await AutoUpdateUiFromData(CancellationToken.None);

            var service = ServicesContainer.GetService<EshotsApiService>();
            service.Clear();

            await UpdateButtons(@"Login", null, null, false);
            await AddConsoleText("Data cleared for system.");
        }

        private void _chkForceSelection_CheckedChanged(object sender, EventArgs e)
        {
            _modified = true;
        }

        private void _chkAllowAnyDay_CheckedChanged(object sender, EventArgs e)
        {
            _modified = true;
        }

        private void _chkPastDay_CheckedChanged(object sender, EventArgs e)
        {
            _modified = true;
        }
    }
}
