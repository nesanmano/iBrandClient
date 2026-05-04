using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using NomadLogic.Common.Services;
using NomadLogic.Common.Services.Configuration;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Networking.QuestionnaireWebRequest.Eshots;
using NomadLogic.iBrandDesktop.Services;
using NomadLogic.iBrandDesktop.Util.SubmitAction.BaseClasses;

namespace NomadLogic.iBrandDesktop.Util.SubmitAction.Generic.Worker
{
    /// <summary>
    /// Classe de base pour l'envoi d'un fichier multimédia vers le serveur de eShots.
    /// Il existe principalement deux sous-classes: l'une pour les vidéos (car ils demandent un transformation)
    /// et l'autre pour les images (qui sont déjà transformées).
    /// </summary>
    public abstract class SendMultimediaToEshotsWorkerBase : ISubmitActionWorker
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private readonly Respondent _respondent;
        private readonly SendMultimediaToEshotsSubmitActionBase _action;
        private readonly int _respId;

        protected SendMultimediaToEshotsWorkerBase(SendMultimediaToEshotsSubmitActionBase action, Respondent resp, int respId)
        {
            _respondent = resp;
            _action = action;
            _respId = respId;
        }

        public ISubmitActionWorker NextWorkerStep { get; set; }

        public void DoWork(CancellationToken? ct)
        {
            var cancelToken = ct ?? CancellationToken.None;

            if (cancelToken.IsCancellationRequested) return;
            
            var db = ServicesContainer.GetService<IbrandDatabase>();
            var configNomad = ServicesContainer.GetService<NomadConfiguration>();
            var serviceApi = ServicesContainer.GetService<EshotsApiService>();

            TransformFile(_respondent, db, configNomad);

            var filePath = _respondent.GetAnswerText(_action.OriginalFileQuestion);
            var eventDayIdStr = _respondent.GetAnswerText(_action.EventDaysIdQuestion);
            var emailIdStr = _respondent.GetAnswerText(_action.EmailIdQuestion);
            var consumerIdStr = _respondent.GetAnswerText(_action.EshotsIdQuestion);
            var evenTokenId = _respondent.GetAnswerText(_action.EshotsEventTokenQuestion);

            // Vérifications importantes d'un répondant intraitable
            if (!File.Exists(filePath))
            {
                log.Fatal("File to send does not exists. Cannnot use action on respondent. Missing file: " + filePath);
                _action.UiSynchro?.AddUiMessage("Respondent impossible to process (missing file)", 4);
                db.SetRespondentReadyToSync(_respId);
                _action.NewRespondentEventHandle?.Set();
                return;
            }

            // Vérifie si on est en mode déboguage (Ne fait pas l'envoi vers eShots, ni la vérification du eventDay.
            if (_action.DebugDoNotSendToEshots)
            {
                log.Info("Debug mode activated: will only do video transformation. EventDay and eShots API send will not be attempted.");
                db.SetRespondentReadyToSync(_respId);
                _action.NewRespondentEventHandle?.Set();
                return;
            }

            // On doit forcer null si la chaîne est vide. C'est important pour la sérialisation de la requête.
            if (string.IsNullOrWhiteSpace(emailIdStr)) emailIdStr = null;
            if (string.IsNullOrWhiteSpace(consumerIdStr)) consumerIdStr = null;

            if (!int.TryParse(eventDayIdStr, out var eventDayId))
            {
                log.Fatal($"Could not convert eshots event day id (value={eventDayIdStr}) from question {_action.EventDaysIdQuestion}");
                _action.UiSynchro?.AddUiMessage("Respondent impossible to process (invalid event day id)", 4);
                db.SetRespondentReadyToSync(_respId);
                _action.NewRespondentEventHandle?.Set();
                return;
            }
            
            // Vérifications importantes d'un répondant intraitable
            if (consumerIdStr == null && emailIdStr == null && evenTokenId == null)
            {
                log.Fatal("No valid ID is used. Cannnot use action on respondent.");
                _action.UiSynchro?.AddUiMessage("Respondent impossible to process (customer Id invalid)", 4);
                db.SetRespondentReadyToSync(_respId);
                _action.NewRespondentEventHandle?.Set();
                return;
            }

            // On traite le fichier puis on l'envoie.
            try
            {
                // Surtout mis en place pour profiter des "awaits"
                var task = Task.Run(async () => { 
                    // Initialisation de la facade et validation du service
                    serviceApi = EshotsApiService.InitialInitCheck(serviceApi, db);
                    if (serviceApi == null)
                    {
                        throw new System.Exception("Service is not initialized. Cannot continue.");
                    }

                    var workDir = DirectoryUtil.GetLocalApplicationWorkDir();
                    var base64File = workDir + "base64." + StringUtil.GetFileName(filePath);
                    // On créer une seule fois le fichier base64
                    if (!File.Exists(base64File))
                    {
                        var result = await FileUtil.ConvertFiletoBase64(new FileInfo(filePath), new FileInfo(base64File), cancelToken, workDir);
                        if (!result)
                        {
                            throw new System.Exception($"Base64 transformation for file ({filePath}) did not complete succesfully.");
                        }
                        log.Trace("Encoded file into base64: " + base64File);
                    }

                    // On envoi le fichier via la requête web.
                    var executor = new WebRequestEshots
                    {
                        EshotsDatabase = serviceApi.LocalDatabase
                    };
                    if (!await executor.InitializeExecutor())
                    {
                        throw new System.Exception("Could not initialize web executor correctly.");
                    }

                    var state = serviceApi.ApiState;
                    var config = serviceApi.LocalDatabase.GetLastEshotsConfiguration();
                    if (state == EshotsApiConfigState.RenewRequired)
                    {
                        var resultRenew = await executor.TryRenewToken(serviceApi, config, state, cancelToken);

                        if (resultRenew != EshotsComplexRequestResultEnum.Continue)
                        {
                            // On stoppe. Erreur lors du renouvellement.
                            throw new System.Exception("Renew operation failed. New state was: " + resultRenew);
                        }
                    }
                    else if (state != EshotsApiConfigState.CanSend)
                    {
                        // On stoppe. État incompatible.
                        throw new System.Exception("Cannot continue file sending. Eshots API state does not allow it: " + state);
                    }

                    var uploadConfig = new EshotsFileUploadsRequest
                    {
                        EventDaysUploads = new List<EshotsEventDayFileUploads>
                        {
                            new EshotsEventDayFileUploads
                            {
                                EventDayId = eventDayId,
                                EventDaysUploads = new List<EshotsFileUpload>
                                {
                                    new EshotsFileUpload
                                    {
                                        FileName = StringUtil.GetFileName(StringUtil.GetFileName(filePath)),
                                        CreationDateUtc = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
                                        ConsumersToAssociate = new List<EshotsConsumerUploadData>
                                        {
                                            new EshotsConsumerUploadData
                                            {
                                                ConsumerUniqueId = consumerIdStr,
                                                Email = emailIdStr,
                                                EventTokenId = evenTokenId
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    };

                    if (log.IsDebugEnabled)
                    {
                        log.Debug("Sending file upload request (w/o file payload): " + ClassUtil.ToStringPublicProperties(uploadConfig));
                    }

                    uploadConfig.EventDaysUploads[0].EventDaysUploads[0].FileContent = File.ReadAllText(base64File, Encoding.UTF8);
                    log.Trace("Finished associating file content to upload config.");

                    var resultSend = await executor.UploadFileLinkedToConsumer(uploadConfig, config.AccessToken, cancelToken);

                    if (!(resultSend.StatusCode == HttpStatusCode.OK || resultSend.StatusCode == HttpStatusCode.Created))
                    {
                        if (log.IsDebugEnabled)
                        {
                            log.Debug("Could not send file to Eshots servers: \n" + ClassUtil.ToStringPublicProperties(resultSend));
                        }

                        var errorMessage = $"Send result incorrect ({resultSend.StatusCode}). Aborting.";
                        if (!log.IsTraceEnabled) throw new System.Exception(errorMessage);

                        uploadConfig.EventDaysUploads[0].EventDaysUploads[0].FileContent = "<REMOVED>";
                        log.Trace("Failed upload content (W/O file payload): \n" + ClassUtil.ToStringPublicProperties(resultSend, 1));
                        // ReSharper disable once AssignNullToNotNullAttribute
                        log.Trace("Failed upload content (W/O file payload): \n" + ClassUtil.ToStringPublicProperties(uploadConfig.EventDaysUploads?.FirstOrDefault(), 1));

                        throw new System.Exception(errorMessage);
                    }

                    // Tout est beau, l'envoi est un succès.
                    log.Trace("File send with success.");
                    _action.UiSynchro?.AddUiMessage($"File sent to Eshots servers (RespId={_respId})", 3);
                    db.SetRespondentReadyToSync(_respId);
                    _action.NewRespondentEventHandle?.Set();

                }, cancelToken);
                task.Wait(cancelToken);
            }
            catch (System.Exception ex)
            {
                log.Warn(ex, "Error while processing file to Eshots with customer.");

                _action.UiSynchro?.AddUiMessage("Error while processing files.", 4);

                // En cas d'erreur, mais que c'est optionnel, on met quand même le répondant prêt à être envoyé.
                if (_action.Optional)
                {
                    db.SetRespondentReadyToSync(_respId);
                    _action.NewRespondentEventHandle?.Set();
                }
            }
        }

        protected abstract void TransformFile(Respondent resp, IbrandDatabase db, NomadConfiguration config);

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