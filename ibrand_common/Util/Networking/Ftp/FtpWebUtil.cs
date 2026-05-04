using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using NLog;
using WinSCP;

namespace NomadLogic.Common.Util.Networking.Ftp
{
    /// <summary>
    /// Classe utilitaire pour exécuter des requête web de FTP.
    /// </summary>
    [PublicAPI]
    public static class FtpWebUtil
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        #region Utility methods
        
        private static void AsyncTransferCancelCheck(FileTransferProgressEventArgs e, CancellationToken ct)
        {
            if (e == null) return;
            if (!ct.IsCancellationRequested) return;
            log.Debug("Cancelled file transfer: {0}", e.FileName);
            e.Cancel = true;
        }

        private static void AsyncTransferReport(FileTransferProgressEventArgs args, CancellationToken ct, IProgress<FtpBatchProgress> progress, Dictionary<string, FtpTransferProgress> data,
            DateTime started, ref DateTime lastUpdate)
        {
            if (args == null) return;
            if (ct.IsCancellationRequested)
            {
                log.Debug("Cancelled file transfer: {0}", args.FileName);
                args.Cancel = true;
                return;
            }

            var k = args.FileName;

            FtpTransferProgress currentProgress;
            if (!data.ContainsKey(k))
            {
                currentProgress = new FtpTransferProgress
                {
                    IsDownload = args.Side == ProgressSide.Remote,
                    Path = args.Directory,
                    FileName = k
                };
                data.Add(k, currentProgress);
            }
            else
            {
                data.TryGetValue(k, out currentProgress);
            }

            if (currentProgress == null)
            {
                log.Debug("Could not continue event.");
                return;
            }

            currentProgress.PercentComplete = args.FileProgress;
            currentProgress.Speed = args.CPS;

            // On envoi le rapport, mais pas trop souvent.
            var now = DateTime.Now;
            var elapsed = now - lastUpdate;
            lastUpdate = now;
            if (elapsed >= TimeSpan.FromMilliseconds(300)) return;
            var batchProg = new FtpBatchProgress(data)
            {
                ElapsedTime = now - started,
                Completion = args.OverallProgress
            };

            progress.Report(batchProg);
        }

        public static SessionOptions CreateSessionOptions([NotNull] FtpConfigInfo conf)
        {
            return CreateSessionOptions(conf.Url, conf.User, conf.Password, conf.SshPrivateKeyFileInfo, conf.SshHostFingerprint, conf.Port);
        }
        
        public static SessionOptions CreateSessionOptions([NotNull] string host, [NotNull] string user, [NotNull] string password, 
            FileInfo sshFileKey = null, string keyFingerprint = null, int? port = null)
        {
            var sessionOptions = new SessionOptions
            {
                HostName = host,
                UserName = user,
                Password = password
            };

            if (sshFileKey != null && sshFileKey.Exists)
            {
                sessionOptions.SshPrivateKeyPath = sshFileKey.FullName;
                sessionOptions.SshHostKeyFingerprint = keyFingerprint;
                sessionOptions.Protocol = Protocol.Sftp;
                sessionOptions.PortNumber = port ?? 22;
            }
            else
            {
                sessionOptions.PortNumber = port ?? 21;
                sessionOptions.Protocol = Protocol.Ftp;
                log.Warn("SSH Private Key File does not exists. Configuring FTP session as plain FTP protocol.");
            }

            return sessionOptions;
        }
        
        #endregion

        /// <summary>
        /// Vérifie l'existence d'un répertoire ou d'un fichier précis sur le serveur Ftp.
        /// </summary>
        /// <param name="remotePath">L'élément à vérifier</param>
        /// <param name="session">La session FTP.</param>
        /// <returns>TRUE si l'élément à été validé d'exister.</returns>
        public static bool CheckIfExists([NotNull] Session session, [NotNull] string remotePath)
        {
            if (!session.Opened)
            {
                return false;
            }

            return session.FileExists(remotePath);
        }

        /// <summary>
        /// Créer un répertoire sur le serveur.
        /// </summary>
        /// <param name="remotePath">Le répertoire à créer</param>
        /// <param name="session">La session FTP.</param>
        /// <returns>TRUE si et seulement si le répertoire à été créé. FALSE si non créé mais pas d'autre erreurs.</returns>
        public static bool CreateDirectory([NotNull] Session session, [NotNull] string remotePath)
        {
            if (!session.Opened)
            {
                return false;
            }

            session.CreateDirectory(remotePath);
            return true;
        }

        public static bool UploadFile([NotNull] Session session, [NotNull] string remotePath, [NotNull] FileInfo fileInfo)
        {
            return UploadFile(session, "", remotePath, fileInfo);
        }

        public static bool UploadFile([NotNull] Session session, [NotNull] string localPath, [NotNull] string remotePath, [NotNull] FileInfo fileInfo)
        {
            if (!session.Opened)
            {
                return false;
            }

            var remoteFilePath =
                RemotePath.TranslateLocalPathToRemote(
                    fileInfo.FullName, localPath, remotePath);
 
            if (fileInfo.Attributes.HasFlag(FileAttributes.Directory))
            {
                // Create remote subdirectory, if it does not exist yet
                if (session.FileExists(remoteFilePath)) return true;
                session.CreateDirectory(remoteFilePath);
                return true;
            }

            // Upload file and remove original
            var result = session.PutFiles(fileInfo.FullName, remoteFilePath, true);
            result.Check();
            return result.IsSuccess;
        }
        
        /// <summary>
        /// Détruit le fichier sur le serveur au lieu de l'envoyer.
        /// </summary>
        public static bool DeleteFileOnServer([NotNull] Session session, [NotNull] string uri)
        {
            if (!session.Opened)
            {
                return false;
            }

            var result= session.RemoveFiles(uri);
            result.Check();
            return result.IsSuccess;
        }

        /// <summary>
        /// Cette méthode synchronize un répertoire complet d'un répertoire FTP, en une opération.
        /// La synchronisation initiale peut être longue selon le montant de fichier à télécharger.
        /// Cette méthode est une manière d'envelopper une opération synchrone avec des événement vers un format ASYNC cancellable avec le
        /// paradigme de rapport d'objet Progress.
        /// </summary>
        /// <param name="options">Options pour créer une session de transfert.</param>
        /// <param name="localPath">Répertoire de base source</param>
        /// <param name="remotePath">Répertoire de base de réception</param>
        /// <param name="ct">Le jeton de cancellation.</param>
        /// <param name="extensions">Liste d'extensions acceptées.</param>
        /// <param name="progress">Objet d'observation de l'opération.</param>
        /// <param name="newerThan">Date de modification maximale.</param>
        /// <returns>Le nombre de fichier téléchargés.</returns>
        public static Task<int> SynchronizeDirectory([NotNull] SessionOptions options, [NotNull] string localPath, [NotNull] string remotePath, 
            CancellationToken ct, DateTime? newerThan = null, string[] extensions = null, IProgress<FtpBatchProgress> progress = null)
        {
            var nbrFilesDownloaded = 0;

            try
            {
                var session = new Session();

                FileTransferProgressEventHandler localDelegate;
                if (progress != null)
                {
                    var data = new Dictionary<string, FtpTransferProgress>();
                    var started = DateTime.Now;
                    var lastUpdate = started;

                    localDelegate = (o, args) => { AsyncTransferReport(args, ct, progress, data, started, ref lastUpdate); };
                }
                else
                {
                    // ReSharper disable once ImplicitlyCapturedClosure
                    localDelegate = (o, args) => AsyncTransferCancelCheck(args, ct);
                }

                // Assignation du délégué d'événement.
                session.FileTransferProgress += localDelegate;

                session.Open(options);

                // Create Filter
                var sb = new StringBuilder();
                var first = true;
                if (extensions != null)
                {

                    foreach (var e in extensions)
                    {
                        if (first)
                        {
                            first = false;
                        }
                        else
                        {
                            sb.Append("; ");
                        }
                        sb.Append("*.");
                        sb.Append(e);
                    }
                }

                var mask = sb.Length == 0 ? null : sb.ToString();

                // Enumerate files and directories to download
                var fileInfos =
                    session.EnumerateRemoteFiles(
                        remotePath, mask,
                        EnumerationOptions.EnumerateDirectories |
                        EnumerationOptions.AllDirectories);
                
                foreach (var fileInfo in fileInfos)
                {
                    if (ct.IsCancellationRequested) break;

                    var localFilePath =
                        RemotePath.TranslateRemotePathToLocal(
                            fileInfo.FullName, remotePath, localPath);
 
                    if (fileInfo.IsDirectory)
                    {
                        // Create local subdirectory, if it does not exist yet
                        if (!Directory.Exists(localFilePath))
                        {
                            Directory.CreateDirectory(localFilePath);
                        }
                    }
                    else
                    {
                        var remoteFilePath = RemotePath.EscapeFileMask(fileInfo.FullName);

                        if (newerThan.HasValue)
                        {
                            var date = fileInfo.LastWriteTime;
                            if (date < newerThan.Value) continue;
                        }

                        // Download file
                        var transferResult = session.GetFiles(remoteFilePath, localFilePath);
 
                        // Did the download succeeded?
                        if (!transferResult.IsSuccess)
                        {
                            // Print error (but continue with other files)
                            log.Error("Error downloading file {0}: {1}", fileInfo.FullName, transferResult.Failures[0].Message);
                        }
                        else
                        {
                            nbrFilesDownloaded++;
                        }
                    }
                }

                return Task.FromResult(nbrFilesDownloaded);
            }
            catch (Exception e)
            {
                log.Error(e);
                return Task.FromResult(nbrFilesDownloaded);
            }
        }

        public static bool TestSession(Session ftpSession)
        {
            return ftpSession?.ListDirectory(".") != null;
        }

        public static FileInfo DownloadFile(Session session, string remPath, DirectoryInfo localWorkDirectory, bool useRemoteName)
        {
            // Création d'un nom de fichier local.
            string fileName;
            if (useRemoteName)
            {
                fileName = StringUtil.GetFileName(remPath);
            }
            else
            {
                var ext = StringUtil.GetFileExtension(remPath);
                fileName = GenerateUniqueId.GetId() + "." + ext;
            }
            
            var filePath = new FileInfo(Path.Combine(localWorkDirectory.FullName, fileName));

            using (var fileStream = new FileStream(filePath.FullName, FileMode.Create))
            {
                using (var stream = session.GetFile(remPath))
                {
                    stream.CopyTo(fileStream);
                    return filePath;
                }
            }
        }
    }
}