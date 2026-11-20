using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using NLog;
using NomadLogic.Common.Serveur;
using NomadLogic.Common.Serveur.Requests;
using NomadLogic.Common.Services.Configuration;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Math;
using NomadLogic.Common.Util.Networking.DownloadProgress;

#pragma warning disable 612


namespace NomadLogic.iBrandDesktop.Synchro.DownloadRestRequest
{
    public class DownloadManager : IDisposable
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        private readonly UserAuthTokenManager _tokenManager;
        
        public DownloadManager([NotNull] IDatabaseProjectFromServer database, NomadConfiguration configuration, IProgress<DownloadReportInfo> report = null)
        {
            _db = database;
            _report = report;
            _conf = configuration;

            _tokenManager = new UserAuthTokenManager(configuration.CurrentServerConfiguration);
        }

        private readonly IDatabaseProjectFromServer _db;

        private int _overallProgress;
   
        private readonly NomadConfiguration _conf;

        private readonly IProgress<DownloadReportInfo> _report;

        public void Execute(ServerTerminalDownloadRequest desc, string baseProjectDir, CancellationToken token)
        {
          /*  if (!_tokenManager.GetAuthToken(out var authToken))
            {
                GenerateReport(new DownloadReportInfo
                {
                    Error = 1,
                    Message = "Could not get access token for main serveur. You may try again, or check internet connection. If problem persists, contact tech support."
                });
                return;
            }*/
            
            // On obtient les informations du terminal.

            if (token.IsCancellationRequested) return;
            var jwtToken = "";
            _tokenManager.GetAuthToken(out jwtToken);
            var tdiRes = ServerRestRequestsUtil.DownloadProjectInfo(jwtToken, token, _conf.CurrentServerConfiguration, desc.Username, desc.Password).GetAwaiter().GetResult();
            
            if (tdiRes.Result != ServerRestRequestResultType.Success)
            {
                GenerateReport(new DownloadReportInfo
                {
                    Error = 1,
                    Message = tdiRes.MessageUi
                });
                return;
            }
            
            var terminalInfo = tdiRes.Data;

            if (!terminalInfo.Validate(out var errorMessage))
            {
                GenerateReport(new DownloadReportInfo
                {
                    Error = 1,
                    Message = "Invalid terminal info. Reason => " + errorMessage
                });
                return;
            }

            IList<ResourceFileToDownload> deltaFiles = new List<ResourceFileToDownload>();

            // Analyse des fichiers à télécharger.
            if (terminalInfo.FilesToDownload != null)
            {
                IList<ResourceFileToDownload> serverList = terminalInfo.FilesToDownload.ToList();
                
                var baseTerminalDir = new DirectoryInfo(DirectoryUtil.GetLocalIbrandTerminalDir(terminalInfo.ProjectId, terminalInfo.QuestionnaireId, terminalInfo.TerminalId));
                // TODO Éventuellement, utiliser "flatten" à faux pour les sous-répertoires.
                var localList = ServerRestRequestsUtil.DirectoryContentAsDownloadRessources(token, baseTerminalDir);
                var dbList = _db.GetResourceFiles(terminalInfo.TerminalId);

                var dbDiff = ServerRestRequestsUtil.CompareDownloadList(dbList, localList, RessourceFileComparison.FileSize);
                deltaFiles = ServerRestRequestsUtil.CompareDownloadList(serverList, dbDiff, RessourceFileComparison.Both);
            }

            if (token.IsCancellationRequested) return;

            // Analyser le résultat
            // _deltaFileToDOwnload reste similaire pour les autres actions, au besoin.
            var updateOrAdd = deltaFiles.Where(d => d.Action == RessourceFileAction.Update || d.Action == RessourceFileAction.Add).ToList();
            if (!updateOrAdd.Any())
            {
                // TODO Internationaliser le message
                GenerateReport("There is no file to update or add: everything is on disk and up-to-date.", 0);
            }
            else
            {
                var sb = new StringBuilder();
                sb.AppendLine($"The following files ({updateOrAdd.Count}) must be downloaded:");
                long totalBytes = 0;
                foreach (var download in updateOrAdd)
                {
                    sb.AppendLine($"- {download.Src} ({MathUtil.ByteNumberReadable(download.FileSize)})");
                    totalBytes += download.FileSize;
                }
                sb.Append("Total download size: " + MathUtil.ByteNumberReadable(totalBytes));
                GenerateReport(sb.ToString(), 0);
            }
            
            if (token.IsCancellationRequested) return;
            
            var allGood = LaunchDownload( _conf.CurrentServerConfiguration, deltaFiles, terminalInfo, baseProjectDir, token).GetAwaiter().GetResult();

            if (!allGood)
            {
                GenerateReport(new DownloadReportInfo
                {
                    Error = 1,
                    Message = "Could not download all the files identified. Terminal creation halted. You may try again later."
                });
                return;
            }

            // Si tout s'est bien passé.
            if (!_db.IsTerminalAlreadyInserted(terminalInfo.TerminalId))
            {
                // Sauvegarde dans la BD
                var res = _db.InsertTerminal(terminalInfo);
                if (res == 0)
                {
                    log.Error("Could not add Terminal and related info to database.");
                    GenerateReport(new DownloadReportInfo
                    {
                        Error = 1,
                        Message = "Could not add Terminal and related info to database."
                    });
                    return;
                }
            }
            
            _db.ManageRessourceFiles(terminalInfo.TerminalId, deltaFiles);
            _db.ResetTerminalMetadata(terminalInfo);
        }
        
        protected async Task<bool> LaunchDownload( ServerConfiguration conf, IList<ResourceFileToDownload> files, TerminalDownloadInfo tdi, string outputDir, CancellationToken token)
        {
            if (token.IsCancellationRequested || files == null) return false;

            var terminalDirPath = Path.Combine(outputDir, tdi.ProjectId.ToString(), tdi.QuestionnaireId.ToString(), tdi.TerminalId.ToString());
            if (!Directory.Exists(terminalDirPath))
            {
                Directory.CreateDirectory(terminalDirPath);
            }

            var toDl = files.Where(d => d.Action == RessourceFileAction.Add || d.Action == RessourceFileAction.Update).ToList();
            var totalFile = toDl.Count;

            foreach (var x in toDl)
            {
                if (token.IsCancellationRequested) return false;

                var finalFile = Path.Combine(terminalDirPath, x.Src);
                if (!await DownloadFile(tdi, x, finalFile, token, totalFile)) return false;

                _overallProgress++;

                GenerateReport("", 0, _overallProgress, totalFile);
            }

            return true;
        }
        
        private async Task<bool> DownloadFile( TerminalInfo ti, [NotNull] ResourceFileToDownload fic, [NotNull] string outFile, CancellationToken token, int totalToDownload)
        {
            var dirPath = Path.GetDirectoryName(outFile);
            if (dirPath == null) return false;
            Directory.CreateDirectory(dirPath);
            if (token.IsCancellationRequested)
            {
                return false;
            }

            var dlFile = new FileInfo(outFile);

            try
            {
                var jwtToken = "";
                _tokenManager.GetAuthToken(out jwtToken);
                var resp = await ServerRestRequestsUtil.DownloadTerminalFile(jwtToken,token, _conf.CurrentServerConfiguration, ti, fic, dlFile);

                if (resp.Data != true)
                {
                    if (_report == null) return false;
                    var downloadReportInfo = new DownloadReportInfo
                    {
                        TotalNumFilestoDownload = totalToDownload,
                        Message = resp.MessageUi
                    };
                    log.Warn("Request for download failed with message: {0}", resp.MessageUi);
                    GenerateReport(downloadReportInfo);
                    return false;
                }

                dlFile.Refresh();
                if (dlFile.Length != fic.FileSize)
                {
                    log.Warn("File downloaded ({0}) has different file size ({1}) than file described by server request ({2}). Investigate.",
                        dlFile.FullName, MathUtil.ByteNumberReadable(dlFile.Length), MathUtil.ByteNumberReadable(fic.FileSize));
                }

                if (_report == null) return true;
                var downloadReportInfo2 = new DownloadReportInfo
                {
                    TotalNumFilestoDownload = totalToDownload,
                    Message = $"{fic.Src} downloaded! ({MathUtil.ByteNumberReadable(dlFile.Length)})"
                };

                GenerateReport(downloadReportInfo2);
                return true;
            }
            catch (System.Exception e)
            {
                try
                {
                    if (dlFile.Exists)
                    {
                        dlFile.Delete();
                    }
                }
                catch (System.Exception)
                {
                    // Die nice
                }

                if (!(e is OperationCanceledException)) return false;
                log.Trace(e, @"DownloadFile() -> Download Task has been cancelled!");
                GenerateReport("Download Task has been cancelled!",1);
                return false;
            }
        }

        protected void GenerateReport(DownloadReportInfo rep)
        {
            if (rep != null) _report?.Report(rep);
        }

        [Obsolete] // Toujours générer un rapport avec des variables d'instance si possible.
        protected void GenerateReport(string msg, int error, int overallProgress = 0, int totalCount=0)
        {
            if (_report == null) return;

            var downloadReportInfo = new DownloadReportInfo
            {
                Message = msg, 
                Error = error, 
                FilesOverallProgress = overallProgress, 
                TotalNumFilestoDownload = totalCount
            };
            _report.Report(downloadReportInfo);
        }

        public void Dispose()
        {

        }
        
    }
}
