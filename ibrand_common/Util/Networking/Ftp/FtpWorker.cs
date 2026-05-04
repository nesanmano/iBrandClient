using System;
using System.IO;
using System.Threading;
using NomadLogic.Common.Util.ThreadPool;
using WinSCP;

namespace NomadLogic.Common.Util.Networking.Ftp
{
    /// <summary>
    /// Classe d'exécution pour l'envoi FTP.
    /// </summary>
    public class FtpWorker : CustomWorker<FtpUploadParameter, string[], FtpPool>
    {
        private static readonly NLog.Logger log = NLog.LogManager.GetCurrentClassLogger();
        private static readonly string[] SENDING_MESSAGE = { "sending" };
        private static readonly string[] SENDING_MESSAGE_SUB_DIRS = { @"thumbnails" };

        /// <summary>
        /// Méthode d'action principale.
        /// </summary>
        public override void DoWork(CancellationToken ct)
        {
            try
            {
                Pool.SendFalseFile(SENDING_MESSAGE_SUB_DIRS, SENDING_MESSAGE);

                var nbrElems = Parameter.FilesToUpload.Length;

                for (var i = 0; i < nbrElems && Error == null; i++)
                {
                    if (ct.IsCancellationRequested) return;

                    if (File.Exists(Parameter.FilesToUpload[i]))
                    {
                        var fileInfo = new FileInfo(Parameter.FilesToUpload[i]);

                        // Create FtpWebRequest object from the Uri provided
                        var uploadPath = Pool.GetUploadPath(Parameter.OutputDirectories[i], Parameter.FilesToUpload[i]);

                        using (var session = new Session())
                        {

                            session.Open(Pool.ConnectionOptions);

                            if (FtpWebUtil.CheckIfExists(session, uploadPath))
                            {
                                if (!FtpWebUtil.DeleteFileOnServer(session, uploadPath))
                                {
                                    Error = "Could not delete already existing file on server: " + Parameter.FilesToUpload[i];
                                    continue;
                                }
                            }

                            if (!FtpWebUtil.UploadFile(session, uploadPath, fileInfo))
                            {
                                Error = "Could not upload file on server: " + Parameter.FilesToUpload[i];
                            } else if (Pool.ActiveLog)
                            {
                                log.Debug("File uploaded (" + Id + ", " + uploadPath + ")");
                            }
                        }
                    }
                    else
                    {
                        Error = "File did not exist: " + Parameter.FilesToUpload[i];
                    }
                }

                for (var i = 0; i < nbrElems && Error == null; i++)
                {
                    if (ct.IsCancellationRequested) return;
                    try
                    {
                        // On tente maintenant de transférer le fichier vers le répertoire d'archive, s'il existe.
                        if (Parameter.ArchiveDirectory == null) continue;
                        if (Pool.ActiveLog)
                        {
                            log.Debug("Preparing to move file: " + Parameter.FilesToUpload[i]);
                        }

                        var dir = new DirectoryInfo(Parameter.ArchiveDirectory);
                        var fileMoved = dir.FullName + @"\" + StringUtil.GetFileName(Parameter.FilesToUpload[i]);
                        if (File.Exists(fileMoved))
                        {
                            File.Delete(fileMoved);
                            if (Pool.ActiveLog)
                            {
                                log.Debug("Duplicate file destroyed: " + fileMoved);
                            }
                        }

                        File.Move(Parameter.FilesToUpload[i], fileMoved);

                        if (File.Exists(fileMoved) && !File.Exists(Parameter.FilesToUpload[i]))
                        {
                            if (Pool.ActiveLog)
                            {
                                log.Debug("File moved: " + fileMoved);
                            }
                        }
                        else
                        {
                            log.Error("Could not move file to archive: " + Parameter.FilesToUpload[i]);
                        }
                    }
                    catch (Exception)
                    {
                        Error = "File could not be transfered to archive directory: " + Parameter.FilesToUpload[i];
                        throw;
                    }
                }

                if (Error != null) return;
                Success = true;
                WorkerResult = Parameter.FilesToUpload;
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }
    }
}