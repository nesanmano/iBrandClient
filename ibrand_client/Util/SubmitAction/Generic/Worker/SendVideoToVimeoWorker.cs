using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using NLog;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.UploadHelper;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Json;
using NomadLogic.Common.Util.Networking.VimeoApi;
using NomadLogic.iBrandDesktop.Services;
using NomadLogic.iBrandDesktop.Util.SubmitAction.BaseClasses;

namespace NomadLogic.iBrandDesktop.Util.SubmitAction.Generic.Worker
{
    /// <summary>
    /// Classe pour envoyer un fichier video sur notre compte VIMEO.
    /// La prochaine étape de cette classe est l'envoi vers le backup.
    /// </summary>
    public class SendVideoToVimeoWorker : SubmitActionWorkerAbstract<AbstractCreateVideoAndSendSubmitAction>
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private readonly FtpConfigurationInfo _ftpConfiguration;

        /// <summary>
        /// Constructeur principal
        /// </summary>
        /// <param name="action">L'initiateur</param>
        /// <param name="resp">Le répondant à créer</param>
        /// <param name="respId">L'identifiant.</param>
        /// <param name="stepInfo">L'information de chaîne et d'exécution par bassin.</param>
        public SendVideoToVimeoWorker(AbstractCreateVideoAndSendSubmitAction action, Respondent resp, int respId,
            SubmitActionWorkerNextStepInfo stepInfo = null) : base(action, resp, respId, stepInfo)
        {
            // chargement de la config
            var file = DirectoryUtil.GetLocalIbrandTerminalDir(_action.TerminalInfo) + "ftp.config";
           _ftpConfiguration = JsonUtil.CreateInstanceFromJsonConfigFile<FtpConfigurationInfo>(file);


        }

        /// <inheritdoc />
        protected override ISubmitActionWorker NextWorkerInstance(SubmitActionWorkerNextStepInfo next)
        {
            return new CopyToBackUpSubmitActionWorker(_action, _respondent, _respId, next);
        }

        /// <summary>
        /// Cette methode est appele pour amorcer la tache d'evoie vers FTP.
        /// </summary>
        /// <param name="qNo"></param>
        /// <param name="answerMap"></param>
        private void TransferFileOverFtp(QuestionIdentifier qNo, Dictionary<QuestionIdentifier, Answer> answerMap)
        {
            //On tente d'envoyer les fichiers vers FTP si et seulement si questionFichierNo contient au-moins un element;
            //On ne tentera meme pas d'envoyer les fichiers sur FTP si questionFichierNo est vide;
            //Implicitement on gere le cas ou le questionnaire n'aurai pas de question Vimeo(On assume que les tokens sont valables pas de NA,..)

            var type = typeof(FtpConfigurationInfo);
            foreach (var property in type.GetProperties())
            {
                if (property.GetValue(_ftpConfiguration) == null)
                    property.SetValue(_ftpConfiguration, new List<string> { "{Q" + qNo + ":filename}" });
                //throw new System.Exception("One or more properties in the instance of FtpConfigurationInfo are NULL. Verify that your ftp.config file is a valid JSON.");
            }

            try
            {
                FtpFileUpload.FileUploader(_ftpConfiguration, _respondent.ProjectId, _respondent.QuestionnaireId, _respondent.TerminalId, _respondent, answerMap,qNo);
            }
            catch (System.Exception e)
            {

                log.Error(e.Message);
                log.Error(e.StackTrace);
                // si lance une exception on assert erreur
            }
            finally
            {
                Console.WriteLine(@"FTP operation finished");
            }
        }

        /// <inheritdoc />
        protected override void DoWorkExecute(IbrandDatabase db, CancellationToken? ct)
        {
            var questionsVimeoNo = _action.QuestionsVimeoIds;
            var questionsFichierNo = _action.QuestionsVimeoFilesToSend;

            if (questionsVimeoNo == null || questionsFichierNo == null || !questionsFichierNo.Any() || !questionsVimeoNo.Any())
            {
                return;
            }
            
            var accountVimeo = _action._configData[AbstractCreateVideoAndSendSubmitAction.VIMEO_ACCOUNT_ENUM];
            var answerMap = db.GetAnswersMap(_respId);

            var vimeoAccountEnum = VimeoAccountLogin.MainAccount;
            if (!string.IsNullOrWhiteSpace(accountVimeo))
            {
                Enum.TryParse(accountVimeo, true, out vimeoAccountEnum);
            }

            var vimeo = VimeoApi.GetInstance(vimeoAccountEnum);

            for (var i = 0; i < questionsFichierNo.Length; i++)
            {
                var qNoFichier = questionsFichierNo[i];
                var qVi = questionsVimeoNo.Length > i ? new QuestionIdentifier(questionsVimeoNo[i]) : null;

                if (qVi != null && !answerMap.ContainsKey(qVi))
                {
                    var tmp = answerMap[qVi]; //answerMap[qNo][1].Response;

                    var fileName = tmp?.Response;

                    // Si le fichier video n'existe pas, marquer le repondant comme pret a etre synchronise 
                    if(!File.Exists(fileName))
                    {
                        db.SetRespondentReadyToSync(_respId);
                    }

                    if (!_action.NoNomadFtp) TransferFileOverFtp(new QuestionIdentifier(qNoFichier), answerMap);

                    var videoInfo = vimeo.UploadVideo(fileName);

                    var validId = false;
                    if (videoInfo != null)
                    {
                        // Est-ce un long? Si oui et plus que zéro, valide.
                        validId = long.TryParse(videoInfo.Id, out var val); 
                        validId &= val > 0;
                    }

                    if (validId)
                    {
                        var albumName = _action._configData[AbstractCreateVideoAndSendSubmitAction.ALBUM];
                        if (!string.IsNullOrWhiteSpace(albumName))
                        {
                            var resultListAlbums = vimeo.ListAlbums();
                            var albumFound = resultListAlbums.FirstOrDefault(a => a.Name.Equals(albumName)) ??
                                             vimeo.CreateAlbum(albumName, null, GenerateUniqueId.GetId()); // Génère un mot de passe automatiquement

                            var successAssignToAlbum = vimeo.AddVideoToAlbum(videoInfo, albumFound);
                            if (!successAssignToAlbum)
                            {
                                // Étape non-critique, mais on flag quand même le problème.
                                log.Warn($"Could not assign video '{videoInfo.Id}' to album '{albumFound.Id}'. Please verify Vimeo and album information.");
                            }
                        }

						// Enregistrement de la question Viméo
						var vimeoA = new Answer {QuestionIdentifier = qVi, Response = videoInfo.Id};
						db.DeleteAnswer(_respId, vimeoA);
						db.InsertAnswer(_respId, vimeoA);

						_respondent.Answers.Add(vimeoA);
                        
                        _action.UiSynchro?.AddUiMessage("VIMEO sent: " + videoInfo.Id, 0);
                    }
                    else
                    {
                        _action.UiSynchro?.AddUiMessage("Error while VIMEO sending. Was not successful.", 1);
                        throw new System.Exception("Error while VIMEO sending. Was not successful."); // Lance une action pour tout stopper pour ce répondant.
                    }
                }
                else
                {
                    // S'il y a déjà une réponse, no-op.
                    log.Debug("Video already sent.");
                }
            }
        }
    }
}
