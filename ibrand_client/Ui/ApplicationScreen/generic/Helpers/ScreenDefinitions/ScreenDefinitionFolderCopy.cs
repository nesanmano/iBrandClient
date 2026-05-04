using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using NLog;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Services;
using NomadLogic.Common.Ui.NomadScreen;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Json;
using NomadLogic.Common.Util.Win32;
using NomadLogic.iBrandDesktop.Services.RespondentEventDataDownload;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers.ScreenDefinitions
{
    public class ScreenDefinitionFolderCopy : IJsonConfig, IExternalFileConfig, IRecyclable, IOnLoad, IActivateScreen
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        private Button _transfertBtn;
        private AbstractScreen _screen;
        private RespondentEventDataDownloadService _serviceDl;
        private CancellationTokenSource _cts;
        private Task _task;

        public string DefaultConfigFileName => "folder_copy_config.json";
        public string ConfigFileName { get; set; }
        
        public FolderCopyFileSource FileSource { get; set; }

        [JsonRequired]
        public Rectangle ButtonZone { get; set; }

        public Color? ButtonBackgroundColor { get; set; }

        /// <summary>
        /// Le nom du fichier d'image pour le bouton d'impression.
        /// </summary>
        public string ButtonImageFileName { get; set; }

        public void Recycle(Control parent)
        {
            _cts?.Cancel();
            _cts = null;
            _task = null;
        }

        public bool OnLoad(AbstractScreen screen)
        {
            _serviceDl = ServicesContainer.GetService<RespondentEventDataDownloadService>();

            if (_serviceDl == null)
            {
                log.Warn("No respondent download service detected. One must be set-up for this screen definition to work.");
                return false;
            }

            // Création du bouton
            var imgPath = screen.GetTerminalDirectory() + ButtonImageFileName;
            var imgBmp = FileUtil.LoadImage(imgPath);

            if (imgBmp == null)
            {
                log.Error("Could not load image file: " + imgPath);
                return false;
            }

            _transfertBtn = new Button
            {
                Location = ButtonZone.Location,
                Size = ButtonZone.Size,
                Image = imgBmp,
                BackColor = ButtonBackgroundColor ?? Color.Transparent, // Transparence ne fonctionne pas de toute façon Voir: https://stackoverflow.com/questions/1086621/drawing-a-transparent-button
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = {BorderSize = 0}
            };
            screen.Controls.Add(_transfertBtn);
            _transfertBtn.BringToFront();

            _transfertBtn.Click += (sender, args) => OnClick();

            _screen = screen;
            return true;
        }

        private void OnClick()
        {
            if (_serviceDl == null)
            {
                log.Error("No active service detected. Dangerous situation.");
                return;
            }
            if (!_screen?.IsActivated() ?? true) return;

            if (_task != null) return;
            
            _transfertBtn.Enabled = false;

            _cts = new CancellationTokenSource();
            
            _task = Task.Run(() =>
            {
                StartTransfert(_cts.Token);
            }, _cts.Token).ContinueWith(t =>
            {
                _cts = null;
                _task = null;
            }, TaskContinuationOptions.None);
        }

        private void StartTransfert(CancellationToken ct)
        {
            var data = _serviceDl.GetLastRespondentData();
            if (data == null)
            {
                log.Warn("No data was found. Transfert cannot proceed.");
                MessageBox.Show(_screen, @"No previous user data was found. Did the gallery was properly set-up?", @"No prior data downloaded.", MessageBoxButtons.OK);
                return;
            }

            if (data.DataUnusable || !data.DataAvailiable || data.IsEmpty)
            {
                log.Warn("Data is no longer usable, or does not simply exists: {0}", data.ToStringPublicProperties());
                MessageBox.Show(_screen, @"Data is either empty or now unusable.", @"No usable data found.", MessageBoxButtons.OK);
                return;
            }

            if (!data.IsAllFilesReady())
            {
                log.Debug("Some files may not have been completely downloaded yet: {0}", data.ToStringPublicProperties());
                var diagRes = MessageBox.Show(_screen, @"Not all files are currently on disk. Some are still in transit. If you continue, only availiable files will be transfered.", 
                    @"Some files are still in transit.", MessageBoxButtons.YesNo);

                if (diagRes == DialogResult.No)
                {
                    return;
                }
            }
            
            var t = new Thread(() =>
            {
                var fbd = new FolderBrowserDialog
                {
                    ShowNewFolderButton = false,
                    Description = @"Choose where to copy files from the user.",
                    SelectedPath = @"C:\\"
                };

                // Force le dialogue à être le plus "visible" de tous.
                // https://stackoverflow.com/questions/54037292/folderbrowserdialog-bring-to-front
                var res = fbd.ShowDialog(new Form {TopMost = true}) != DialogResult.OK ? null : fbd.SelectedPath;

                DelegateActOnTransfert doTransfert = TransfertToSelectedFolder;

                _screen.BeginInvoke(doTransfert, ct, res, data);

            });
            t.SetApartmentState(ApartmentState.STA);
            t.Start();
        }

        internal delegate void DelegateActOnTransfert(CancellationToken ct, string folder, RespondentBundleFinalData data);

        internal void TransfertToSelectedFolder(CancellationToken ct, string folder, RespondentBundleFinalData data)
        {
            List<FileInfo> fileList = null;
            try
            {
                if (data == null) return;
                if (string.IsNullOrWhiteSpace(folder)) return;
                fileList = data.ObtainFiles();
                
                var nbr = 0;
                var total = fileList.Count;

                foreach (var originalFile in fileList)
                {
                    if (ct.IsCancellationRequested)
                    {
                        log.Debug("Cancellation detected during transfert Completed {0} of {1}.", nbr, total);
                        return;
                    }

                    var newFilePath = Path.Combine(folder, originalFile.Name);
                    File.Copy(originalFile.FullName, newFilePath, true);
                    nbr++;
                }

                MessageBox.Show(_screen, $@"Copy complete. No error detected. {nbr} files copied.", @"File copy.", MessageBoxButtons.OK);
            }
            catch (System.Exception e)
            {
                log.Error(e, "Error during files ({1}) transfert to: {0}", folder, fileList?.Count);
                MessageBox.Show(_screen, @"Error during transfert. Files may be incomplete.", @"Error detected.", MessageBoxButtons.OK);
            }
            finally
            {
                _screen.SafeCall(() =>
                {
                    _transfertBtn.Enabled = true;
                });
            }
        }

        public void Activate(Control parent, IRespondent respondent)
        {
            parent.SafeCall(() =>
            {
                _transfertBtn.Enabled = true;
            });
        }
    }

    public abstract class FolderCopyFileSource
    {

    }

    public class FileSourceDownloadService : FolderCopyFileSource
    {

    }
}