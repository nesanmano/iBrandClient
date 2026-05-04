using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using NLog;
using NomadLogic.Common.Serveur;
using NomadLogic.Common.Services;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Json;
using NomadLogic.iBrandDesktop.Report;
using NomadLogic.iBrandDesktop.Services;
using NomadLogic.iBrandDesktop.Util;

namespace NomadLogic.DatabaseExporter
{
    public partial class ExtractorUi : Form
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        private DatabaseExtractorConfig _config;
        private bool _configModified;
        private List<TerminalInfo> _terminalList;
        private IbrandDatabase _db;
        private TerminalInfo _currentTerminal;
        private Tuple<DateTime, DateTime> _dates;
        private Tuple<int, int> _termData;

        public ExtractorUi()
        {
            InitializeComponent();

            Closing += OnClosing;
        }

        private void OnClosing(object sender, CancelEventArgs e)
        {
            // Principalement, on annule les opérations en cours et on sauvegarde les configurations, si modifiées.

            // TODO Stoppe tâche d'exportation.

            var possibleDir = _txtDir.Text;
            if (string.IsNullOrWhiteSpace(_config.LastSelectedExportDir) != string.IsNullOrWhiteSpace(possibleDir)
                || !string.Equals(_config.LastSelectedExportDir, possibleDir))
            {
                if (string.IsNullOrWhiteSpace(possibleDir) && FileUtil.CheckIfDirectoryIsWritable(possibleDir))
                {
                    _configModified = true;
                }
            }

            if (!_configModified) return;
            var diagResult = MessageBox.Show(@"Configuration has been modified. Save to disk?", @"iBrand",
                MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);

            if (diagResult == DialogResult.Cancel)
            {
                e.Cancel = true;
                return;
            }

            if (diagResult != DialogResult.Yes) return;

            _config.LastSelectedExportDir = _txtDir.Text;
            var fi = new FileInfo(DirectoryUtil.GetLocalApplicationDir() + _config.DefaultConfigFileName);
            if (!JsonUtil.WriteJsonFile(fi, _config))
            {
                log.Debug("Could not write configuration to disk: {}", ClassUtil.ToStringPublicProperties(_config, 1));
            }
        }

        private void ExtractorUi_Load(object sender, EventArgs e)
        {
            _config = new DatabaseExtractorConfig();
            if (!FileUtil.LoadConfigFromDefaultFilepath(_config))
            {
                throw new Exception("No configuration could be loaded.");
            }

            _lblLastDate.Text = "";
            _lblTotalRespondent.Text = "";

            _chkDestroyEverything.Checked = _config.DestroyAll;
            _chkDestroyData.Checked = _config.DestroyData;
            _chkDestroyFile.Checked = _config.DestroyFiles;
            _chkDestroyLogs.Checked = _config.DestroyLogs;
            _chkDestroyUnsynched.Checked = _config.DestroyUnsynched;
            _chkGenerateHeaders.Checked = _config.GenerateHeaders;
            _chkIncludeMetadata.Checked = _config.IncludeMetadata;

            var da = _config.DestroyAll;
            _chkDestroyData.Enabled = !da;
            _chkDestroyFile.Enabled = !da;
            _chkDestroyLogs.Enabled = !da;
            _chkDestroyUnsynched.Enabled = !da;

            _chkDestroyUnsynched.CheckedChanged += OnCheckedChanged;
            _chkDestroyData.CheckedChanged += OnCheckedChanged;
            _chkDestroyFile.CheckedChanged += OnCheckedChanged;
            _chkDestroyLogs.CheckedChanged += OnCheckedChanged;
            _chkDestroyEverything.CheckedChanged += OnCheckedChanged;
            _chkGenerateHeaders.CheckedChanged += OnCheckedChanged;
            _chkIncludeMetadata.CheckedChanged += OnCheckedChanged;

            // Loading Projects
            _db = ServicesContainer.GetService<IbrandDatabase>();

            if (_db == null) throw new Exception("No valid database.");

            _btnStartExtraction.Enabled = false;
            ToggleGroupSelection(false);

            _terminalList = _db.GetTerminals();

            if (_terminalList.Count == 0)
            {
                // Aucun projet.
                _cmbSelectTerminal.Enabled = false;
                _lblTotalRespondent.Text = @"0";
                _lblLastDate.Text = @"None";
                return;
            }
            
            foreach (var terminal in _terminalList)
            {
                var termDesc = new TerminalControlItem(terminal);
                _cmbSelectTerminal.Items.Add(termDesc);
            }

            _cmbSelectTerminal.SelectedIndexChanged += CmbSelectTerminalOnSelectedIndexChanged;

            _cmbReportType.SelectedIndex = _config.ReportType;
            _cmbReportType.SelectedIndexChanged += CmbReportTypeOnSelectedIndexChanged;

            _txtDir.Text = _config.LastSelectedExportDir ?? "";
        }

        private void CmbSelectTerminalOnSelectedIndexChanged(object sender, EventArgs e)
        {
            var index = _cmbSelectTerminal.SelectedIndex;
            ToggleGroupSelection(index >= 0);

            if (index < 0) return;

            if (!(_cmbSelectTerminal.SelectedItem is TerminalControlItem termDesc)) return;

            _currentTerminal = termDesc.Terminal;
            _termData = _db.GetTerminalTotalRespondents(_currentTerminal.TerminalId);
            
            _lblTotalRespondent.Text = $@"{_termData.Item1} ({_termData.Item2})";
            _dates = _db.RespondentCreationDateIntervalFromTerminal(_currentTerminal.TerminalId);

            _lblLastDate.Text = _dates == null ? "None" : $"'{_dates.Item1:yyyy-MM-dd HH:mm:ss}' to '{_dates.Item2:yyyy-MM-dd HH:mm:ss}'";
        }

        private void CmbReportTypeOnSelectedIndexChanged(object sender, EventArgs e)
        {
            if (_cmbReportType != sender) return;
            _configModified = true;
            _config.ReportType = _cmbReportType.SelectedIndex;
        }

        private void ToggleGroupSelection(bool enable)
        {
            _btnSelectDir.Enabled = true;
            _txtDir.Enabled = true;
            _cmbReportType.Enabled = true;
            _btnStartExtraction.Enabled = enable;
        }

        private void OnCheckedChanged(object sender, EventArgs e)
        {
            _configModified = true;

            if (sender == _chkIncludeMetadata)
            {
                _config.IncludeMetadata = _chkIncludeMetadata.Checked;
            } else if (sender == _chkGenerateHeaders)
            {
                _config.GenerateHeaders = _chkGenerateHeaders.Checked;
            } else if (sender == _chkDestroyData)
            {
                _config.DestroyData = _chkDestroyData.Checked;
            } else if (sender == _chkDestroyFile)
            {
                _config.DestroyFiles = _chkDestroyFile.Checked;
            } else if (sender == _chkDestroyLogs)
            {
                _config.DestroyLogs = _chkDestroyLogs.Checked;
            } else if (sender == _chkDestroyUnsynched)
            {
                _config.DestroyUnsynched = _chkDestroyUnsynched.Checked;
            } else if (sender == _chkDestroyEverything)
            {
                _config.DestroyAll = _chkDestroyEverything.Checked;
            }

            if (sender != _chkDestroyEverything) return;

            var all = _chkDestroyEverything.Checked;
            _chkDestroyData.Enabled = !all;
            _chkDestroyFile.Enabled = !all;
            _chkDestroyLogs.Enabled = !all;
            _chkDestroyUnsynched.Enabled = !all;
        }

        private void _btnSelectDir_Click(object sender, EventArgs e)
        {
            var diag = new FolderBrowserDialog
            {
                Description = @"Choose destination directory",
                ShowNewFolderButton = false
            };

            var diagResult = diag.ShowDialog(this);

            if (diagResult == DialogResult.OK)
            {
                _txtDir.Text = diag.SelectedPath;
            }
        }

        private void _btnStartExtraction_Click(object sender, EventArgs e)
        {
            if (_termData == null || _termData.Item1 == 0)
            {
                MessageBox.Show(@"No data to export. Empty project.", @"iBrand", MessageBoxButtons.OK);
                return;
            }

            if (!FileUtil.CheckIfDirectoryIsWritable(_txtDir.Text))
            {
                MessageBox.Show(@"Selected directory is invalid/unwritable.", @"iBrand", MessageBoxButtons.OK);
                return;
            }

            // TODO implémenter les autre types d'exportation.
            if (_cmbReportType.SelectedIndex == 1)
            {
                MessageBox.Show(@"Excel 97 export not implemented yet.", @"iBrand", MessageBoxButtons.OK);
                return;
            }

            if (_cmbReportType.SelectedIndex == 2)
            {
                MessageBox.Show(@"JSON export not implemented yet.", @"iBrand", MessageBoxButtons.OK);
                return;
            }

            // Vérification du nom de fichier
            var basePath = _txtDir.Text + Path.DirectorySeparatorChar;
            var extension = ".csv";
            var name = $"{Environment.MachineName}_{_dates.Item1:yyyyMMdd}-{_dates.Item2:yyyyMMdd}_{_currentTerminal.TerminalId}_ibrand_export";
            var filePath = FileUtil.FileNameValidation(basePath, extension, name);

            // On demande si on est totalement sûr de détruire les données.
            if (_config.DestroyAll || _config.DestroyData || _config.DestroyFiles)
            {
                var sb = new StringBuilder();
                sb.AppendLine("Once completed, the following data will be permanently destroyed and cannot be recovered:");
                if (_config.DestroyAll)
                {
                    sb.AppendLine("- Every data. (files, logs, respondent data, even those unsynchronized)");
                }
                else
                {
                    if (_config.DestroyData) sb.AppendLine("- Respondent data.");
                    if (_config.DestroyFiles) sb.AppendLine("- Associated files.");
                    if (_config.DestroyUnsynched) sb.AppendLine("- Including those of unsynchronized respondents.");
                }

                sb.AppendLine();
                sb.AppendLine("Are you sure?");

                var result = MessageBox.Show(sb.ToString(), @"iBrand", MessageBoxButtons.OKCancel);

                if (result == DialogResult.OK)
                {
                    var resultConfirm = MessageBox.Show(@"Absolutely sure?", @"iBrand", MessageBoxButtons.YesNo);
                    if (resultConfirm == DialogResult.No) return;
                }
                else
                {
                    return;
                }
            }

            // On commence

            // 1- Obtenir information.

            var writer = new ReportWriterCsv
            {
                FilePath = filePath
            };

            writer.Open();

            var loader = new JujuQuestionnaireLoader(_currentTerminal);
            var q = loader.Load(true);

            var tupleGen = new ReportTupleGeneratorIbrandTerminal(_db, _currentTerminal, q, _config.IncludeMetadata);

            if (_config.GenerateHeaders)
            {
                writer.Write(tupleGen.GenerateHeaderData());
            }

            while (tupleGen.CreateNextTuple())
            {
                writer.Write(tupleGen.CurrentTuple());
            }
            
            writer.Close();

            // 3- Détruire les données.

            // Logs
            if (_config.DestroyLogs || _config.DestroyAll)
            {
                var logsPath = new DirectoryInfo(DirectoryUtil.GetLocalApplicationLogsDir());
                if (!logsPath.Exists)
                {
                    log.Error("No logs directory exists.");
                }
                else
                {
                    var files = logsPath.GetFiles();
                    var logsToDestroy = files.Where(f => !f.Name.EndsWith(".xml", StringComparison.InvariantCultureIgnoreCase));
                    var totalFiles = 0;
                    foreach (var fi in logsToDestroy)
                    {
                        try
                        {
                            log.Trace("Destroying log file: {0}", fi.FullName);
                            fi.Delete();
                            totalFiles++;
                        }
                        catch (Exception exception)
                        {
                            log.Error(exception, "Error while destroying log file. Doest not stop process: {0}", fi.FullName);
                        }
                    }

                    log.Info("Destroyed {0} files on disk.", totalFiles);
                }
            }

            if (_config.DestroyFiles || _config.DestroyAll)
            {
                MessageBox.Show(@"File destruction is currently not implementd. Will skip. Continuing cleanup-up.", @"iBrand", MessageBoxButtons.OK);
            }

            if (_config.DestroyData || _config.DestroyAll)
            {
                var resps = _db.GetRespondentsByTerminal(_currentTerminal, _config.DestroyUnsynched);
                var total = 0;
                foreach (var resp in resps)
                {
                    log.Trace("Destroying respondent {0} in terminal {1}.", resp.Id, _currentTerminal.TerminalId);
                     total += _db.DeleteRespondant(resp.Id);
                }
                log.Info("Destroyed {0} lines in database (respondents + answers).", total);
            }

            CmbSelectTerminalOnSelectedIndexChanged(null, null);
            MessageBox.Show(@"Extraction complete.", @"iBrand", MessageBoxButtons.OK);
        }
    }

    
}

