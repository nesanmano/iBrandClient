using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using NLog;
using NomadLogic.Common.Serveur;
using NomadLogic.Common.Services;
using NomadLogic.Common.Services.ActivityLogger;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Excel;
using NomadLogic.Common.Util.Json;
using NomadLogic.iBrandDesktop.Services;

namespace NomadLogic.iBrandDesktop.Ui
{
    public partial class ExportActivityDataDialog : Form
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        private readonly TerminalInfo _initialFilter;
        private IbrandDatabase _iBrandDb;
        private ActivityLoggerService _activityService;
        private List<TerminalInfo> _terminalList;
        private FileInfo _lastFilePath;

        public ExportActivityDataDialog(TerminalInfo initialTerminalFilter = null)
        {
            InitializeComponent();

            _initialFilter = initialTerminalFilter;
        }

        private void OnLoad(object sender, EventArgs e)
        {
            _iBrandDb = ServicesContainer.GetService<IbrandDatabase>();
            _activityService = ServicesContainer.GetService<ActivityLoggerService>();

            _btnSelectDir.Click += _btnSelectDir_Click;
            _btnStartExtraction.Click += BtnStartExtractionOnClick;

            _terminalList = _iBrandDb.GetTerminals();

            // Charger le dropbox
            if (_terminalList.Count == 0)
            {
                // Aucun projet.
                _cmbSelectedTerminal.Enabled = false;
            }

            var selectedIndex = -1;
            var idx = 0;
            foreach (var termDesc in _terminalList.Select(terminal => new TerminalControlItem(terminal)))
            {
                _cmbSelectedTerminal.Items.Add(termDesc);
                if (_initialFilter.TerminalId == termDesc.Terminal.TerminalId)
                {
                    selectedIndex = idx;
                }
                idx++;
            }

            // Si on a un terminal donné par défaut, on fait cette sélection et on gèle le sélecteur.
            if (selectedIndex > 0)
            {
                _cmbSelectedTerminal.SelectedIndex = selectedIndex;
                _cmbSelectedTerminal.Enabled = false;
            }

            _lastFilePath = new FileInfo(DirectoryUtil.GetLocalNomadLogicDir() + "last_activity_extract_dir.json");
            if (_lastFilePath.Exists)
            {
                _txtDir.Text = JsonUtil.ReadJsonFile<string>(_lastFilePath);
            }
        }
        
        private async void _btnSelectDir_Click(object sender, EventArgs e)
        {
            try
            {
                var txt = await PopUpWindow();

                if (!string.IsNullOrWhiteSpace(txt))
                {
                    _txtDir.Text = txt;
                }
            }
            catch (System.Exception exception)
            {
                log.Fatal(exception, "Could not start FolderBrowserDialog dialog.");
                MessageBox.Show(@"Error during folder selection. Please advise a technician.", @"Activity Exporter", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task<string> PopUpWindow ()
        {
            var folder = new TaskCompletionSource<string>();
            
            Thread t = new Thread(() => DialogPopUp(folder));
            t.SetApartmentState(ApartmentState.STA);
            t.Start();

            var path = await folder.Task;

            return path;
        }

        private static void DialogPopUp(TaskCompletionSource<string> folder)
        {
            FolderBrowserDialog folderDialog = new FolderBrowserDialog();

            while (folderDialog.ShowDialog() != DialogResult.OK)
            {
                MessageBox.Show("Must choose the location folder where the excel will be created", "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }

            folder.SetResult(folderDialog.SelectedPath);
        }

        
        private void BtnStartExtractionOnClick(object sender, EventArgs e)
        {
            var extractPath = _txtDir.Text;
            if (string.IsNullOrWhiteSpace(extractPath))
            {
                MessageBox.Show(@"No export directory selected. Cancelling.", @"Activity Exporter", MessageBoxButtons.OK);
                return;
            }
            
            var extractDirectory = Directory.CreateDirectory(extractPath);
            if (!extractDirectory.Exists)
            {
                MessageBox.Show(@"Export directory cannot be created. Cancelling.", @"Activity Exporter", MessageBoxButtons.OK);
                return;
            }

            // Écriture sur disque de la dernière place sauvegardée
            JsonUtil.WriteJsonFile(_lastFilePath, extractDirectory.FullName);
            _lastFilePath.Refresh();

            var tci = _cmbSelectedTerminal.SelectedItem as TerminalControlItem;
            if (tci == null)
            {
                MessageBox.Show(@"No terminal was selected for extraction.", @"Activity Exporter", MessageBoxButtons.OK);
                return;
            }

            var rawdata = _activityService.GetActivityReport(tci.Terminal.TerminalId);

            var dayDict = new Dictionary<DateTime, IList<ActivityReportItem>>();
            foreach (var ari in rawdata)
            {
                var dtc = ari.DateTimeCreated;
                var day = dtc.Date;

                if (dayDict.TryGetValue(day, out var dayList))
                {
                    dayList.Add(ari);
                }
                else
                {
                    var newList = new List<ActivityReportItem> { ari };
                    dayDict.Add(day, newList);
                }
            }

            // Maintenant que l'on a une liste des éléments par journée, on va créer les fichiers de exportations.
            foreach (var kv in dayDict)
            {
                var day = kv.Key;
                var list = kv.Value;

                if (!list.Any())
                {
                    continue;
                }

                var filePath = Path.Combine(extractDirectory.FullName, $"{day:yyyyMMdd}_{tci.Terminal.TerminalId}-{tci.Terminal.TerminalName}_activity_report.xlsx");
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }

                ExcelUtil.CreateExcelFileFromList(kv.Value, new FileInfo(filePath), "Activity Report", list[0]);
            }
        }
    }
}
