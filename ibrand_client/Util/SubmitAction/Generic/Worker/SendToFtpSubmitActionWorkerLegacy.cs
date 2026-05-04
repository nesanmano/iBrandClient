using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Threading;
using NLog;
using NomadLogic.Common.Services;
using NomadLogic.Common.Services.Configuration;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Networking.Ftp;
using NomadLogic.iBrandDesktop.Services;
using NomadLogic.iBrandDesktop.Util.SubmitAction.BaseClasses;
using NomadLogic.Multimedia.Cli.Ffmpeg;
using WinSCP;

namespace NomadLogic.iBrandDesktop.Util.SubmitAction.Generic.Worker
{
    [SuppressMessage("ReSharper", "NotAccessedField.Local")]
    class SendToFtpSubmitActionWorkerLegacy : ISubmitActionWorker
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private readonly Respondent _respondent;
        private readonly SendToFtpSubmitAction _action;
        private readonly int _respId;

        public SendToFtpSubmitActionWorkerLegacy(SendToFtpSubmitAction action, Respondent resp, int respId)
        {
            _respondent = resp;
            _action = action;
            _respId = respId;
        }

        public void DoWork(CancellationToken? ct)
        {
            if (ct?.IsCancellationRequested ?? false) return;

            string tmpFile = null;
            
            var db = ServicesContainer.GetService<IbrandDatabase>();

            try
            {
                var file = _respondent.GetAnswerText(_action.FtpQuestion);
                
                if (file == null && _action.Optional)
                {
                    // La question n'est pas répondue, mais c'est optionnel.
                    db.SetRespondentReadyToSync(_respId);
                } else {
                    
                    if (file == null || !File.Exists(file))
                    {
                        log.Error("Could not find file to upload: {0}", file);
                        if (_action.SynchRespondentWithFtpError) {
                            db.SetRespondentReadyToSync(_respId);
                        }
                    }
                    else
                    {
                        // On regarde si on fait une réduction de la taille du vidéo (si le fichier est un vidéo).
                        if (_action.CompressVideoFile)
                        {
                            // Transformation du video
                            tmpFile = DirectoryUtil.CreateLocalTemporaryFile(StringUtil.GetFileName(file));

                            var conf = ServicesContainer.GetService<NomadConfiguration>();

                            FfmpegUtil.CompressVideo(conf.FfmpegLocation, file, tmpFile, 24);

                            if (!(File.Exists(tmpFile)))
                            {
                                log.Warn("Could not compress video file: " + file);
                            }
                            else
                            {
                                log.Debug("Video compressed and ready for upload: " + tmpFile);
                            }
                        }

                        var fileToSend = tmpFile ?? file;

                        var fi = new FileInfo(fileToSend);
                        var ftp = new FtpPool(_action.FtpAdress, _action.FtpPort, _action.FtpUser, _action.FtpPassword);
                        var rep = DateTime.Now.ToString("yyyy_MM_dd", CultureInfo.GetCultureInfo("fr-CA"));
                        if (_action.FtpDir != null)
                        {
                            rep = _action.FtpDir + "/" + rep;
                        }
                        var ftpPath = ftp.GetUploadPath(rep, file);
                        if (!ftp.CheckFtpDirectoryPath(ftpPath))
                        {
                            throw new System.Exception("Invalid file path.");
                        }

                        using (var session = new Session())
                        {
                            session.Open(ftp.ConnectionOptions);
                            FtpWebUtil.UploadFile(session, ftpPath, fi);
                        }


                        db.SetRespondentReadyToSync(_respId);
                    }

                    if (_action.DestroyFile) {
                        if (file != null) File.Delete(file);
                    }
                }
            }
            catch (System.Exception ex)
            {
                log.Fatal(ex, "Error while sending file to FTP.");

                _action.UiSynchro?.AddUiMessage("Error while sending files.", 4);

                // En cas d'erreur, mais que c'est optionnel, on met quand même le répondant prêt à être envoyé.
                if (_action.Optional)
                {
                    db.SetRespondentReadyToSync(_respId);
                }
            }
            finally
            {
                if (tmpFile != null)
                {
                    File.Delete(tmpFile);
                }
            }
        }

        public ISubmitActionWorker NextWorkerStep { get; set; }

        public ISubmitAction GetSubmitAction()
        {
            return _action;
        }

        public int GetRespId()
        {
            return _respId;
        }
    }
}