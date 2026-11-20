
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Serveur.Requests;
using NomadLogic.Common.Serveur.Structures;
using NomadLogic.Common.Services;
using NomadLogic.Common.Services.Configuration;
using NomadLogic.Common.Util.Json;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic
{
    
    public class GenericUpdateServerScreen : GenericScreen
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        public bool LoadAnswersFromServer { get; set; }

        public bool LoadDynamicHeadersFromServer { get; set; }

        public bool LoadKeepExistingAnswers { get; set; }

        public RespondentDataDynamicValueRequestFilter LoadDynamicFilters { get; set; }

        public List<string> LoadQuestionFilters { get; set; }
        
        public bool SendSkipDynamicHeaders { get; set; }

        public bool SendSkipAnswers { get; set; }

        public GenericUpdateServerScreen(string backgroundImgEnSrc, string backgroundImageFrSrc, ScreenConfig screenConf) :
            base(backgroundImgEnSrc, backgroundImageFrSrc, screenConf) { }

        private async Task<bool> ExecuteRestRequest(CancellationToken ct, string authToken, string externalIdentifier, IRespondent respondent, FileInfo debugFile = null)
        {
            var currentConfig = ServicesContainer.GetService<NomadConfiguration>().CurrentServerConfiguration;
            
            if (authToken == null)
            {
                var login = await ServerRestRequestsUtil.LoginSync(currentConfig).ConfigureAwait(false);

                if (login.Result != ServerRestRequestResultType.Success)
                {
                    log.Debug("Could not aobtain login token information from server.");
                    return false;
                }

                authToken = login.Data;
            }

            var getRespPayload = new RequestGetRespondentDataPayload
            {
                DynamicValuesFilter = LoadDynamicFilters,
                QuestionDescriptionsFilter = LoadQuestionFilters,
                RespondentIdentification = new RespondentIdentificationStrategy
                {
                    Strategy = new RespondentIdentificationStrategyDefinition
                    {
                        StrategyType = RequestRespondentDataIdentificationStrategyEnum.ExternalIdentifier,
                        ExternalUniqueIdentifier = externalIdentifier,
                        TerminalId = respondent.TerminalId,
                        QuestionnaireId = respondent.QuestionnaireId
                    }
                }
            };

            if (debugFile != null && debugFile.DirectoryName != null)
            {
                var addDebug = new FileInfo(Path.Combine(debugFile.DirectoryName, "get_payload" + debugFile.Name));

                if (addDebug.Exists)
                {
                    addDebug.Delete();
                    addDebug.Refresh();
                }

                var debugRez = JsonUtil.WriteJsonFile(addDebug, getRespPayload);
                if (!debugRez)
                {
                    log.Error("Could not write to debug payload file: " + addDebug.FullName);
                }
            }

            // ReSharper disable once AssignNullToNotNullAttribute
            var rez = await ServerRestRequestsUtil.GetRespondentData(ct, currentConfig, authToken, getRespPayload, false, debugFile);

            if (rez.Result != ServerRestRequestResultType.Success && rez.Result != ServerRestRequestResultType.NoContent)
            {
                return false;
            }

            if (LoadAnswersFromServer)
            {
                if (!LoadKeepExistingAnswers) respondent.Answers = new List<Answer>();
                if (rez.Data?.AnswersFound != null) // Répondant peut être inexistant, ou avec réponses vides
                {
                    foreach (var ans in rez.Data.AnswersFound)
                    {
                        respondent.Answers.Add(new Answer
                        {
                            QuestionIdentifier = new QuestionIdentifier
                            {
                                QuestionNo = ans.QuestionNo,
                                ChoiceNo = ans.ChoiceNo,
                                QuestionType = ans.QuestionType
                            }
                        });
                    }
                }

                SaveAnswers(respondent.Answers);
            }

            if (LoadDynamicHeadersFromServer)
            {
                if (rez.Data != null) // Répondant peut être inexistant.
                {
                    foreach (var dh in rez.Data.DynamicValuesFound)
                    {
                        respondent.DynamicHeaders.Add(new DynamicHeader
                        {
                            DataKey = dh.ValuesKey,
                            DateCreated = dh.CreationDate,
                            DataValues = dh.Values.Select(v => new DataValue(v)).ToList()
                        });

                    }
                }
                SaveDynamicHeaderCollection(respondent.DynamicHeaders);
            }

            return true;
        }
        

        public async Task<bool> LoadFromServer(CancellationToken ct, string externalIdentifier, IRespondent respondent, 
            string authToken = null, FileInfo debug = null)
        {
            // Validation poussée du répondant.
            if (respondent == null)
            {
                throw new ArgumentNullException(nameof(respondent), @"Respondent cannot be null.");
            }

            if (respondent.TerminalId < 1 || respondent.QuestionnaireId < 1 || respondent.ProjectId < 1)
            {
                throw new ArgumentNullException(nameof(respondent), @"Respondent does not have a valid terminal ID");
            }

            var act = new Func<Task<bool>>(async () => await ExecuteRestRequest(ct, authToken, externalIdentifier, respondent, debug));

            var t = Task.Run(act, ct).ConfigureAwait(false);
            var result = await t;

            return result;
        }


        public async Task<bool> SendToServer(CancellationToken ct, IRespondent respondent, string authToken = null, FileInfo debugFile = null)
        {
            // Validation poussée du répondant.
            if (respondent == null)
            {
                throw new ArgumentNullException(nameof(respondent), @"Respondent cannot be null.");
            }

            if (respondent.TerminalId < 1 || respondent.QuestionnaireId < 1 || respondent.ProjectId < 1)
            {
                throw new ArgumentNullException(nameof(respondent), @"Respondent does not have a valid terminal ID");
            }

            if (string.IsNullOrWhiteSpace(respondent.ExternalUniqueIdentifier))
            {
                
                return false;
            }

            var act = new Func<Task<bool>>(async () =>
            {
                var currentConfig = ServicesContainer.GetService<NomadConfiguration>().CurrentServerConfiguration;

                if (authToken == null)
                {
                    var login = await ServerRestRequestsUtil.Login(currentConfig, ct).ConfigureAwait(false);

                    if (login.Result != ServerRestRequestResultType.Success)
                    {
                        return false;
                    }

                    authToken = login.Data;
                }

                var payload = ServerRestRequestsUtil.PrepareRespondentSendRequestPayload(respondent, SendSkipAnswers, SendSkipDynamicHeaders);

                if (debugFile != null && debugFile.DirectoryName != null)
                {
                    var addDebug = new FileInfo(Path.Combine(debugFile.DirectoryName, "send_payload" + debugFile.Name));

                    if (addDebug.Exists)
                    {
                        addDebug.Delete();
                        addDebug.Refresh();
                    }

                    var debugRez = JsonUtil.WriteJsonFile(addDebug, payload);
                    if (!debugRez)
                    {
                        log.Error("Could not write to debug payload file: " + addDebug.FullName);
                    }
                }

                // ReSharper disable once AssignNullToNotNullAttribute
                var rez = await ServerRestRequestsUtil.SendRespondentData(ct, currentConfig, authToken, payload, false, debugFile).ConfigureAwait(false);
            
                return rez.Result == ServerRestRequestResultType.Success;
            });

            var t = Task.Run(act, ct).ConfigureAwait(false);
            var result = await t;

            return result;
        }

    }
}