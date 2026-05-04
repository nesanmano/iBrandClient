using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using RestSharp;

namespace NomadLogic.Common.Util.Networking.QuestionnaireWebRequest.iBrand
{
    public class WebExecutorIbrand : IWebRequestExecutor
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        public void Dispose()
        {
            // Nothing
        }

        public Task<bool> InitializeExecutor()
        {
            return Task.FromResult(true);
        }

        public int Timeout { get; set; } = 5000;

        /// Emplacement web du serveur.
        public string Url { get; set; } = "http://www.nomadlogicapp.com";

        /// Nom de l'application à utiliser (change avec les serveurs de test).
        public string AppName { get; set; } = "ibrand";

        /// Appelle un serveur iBrand pour obtenir une liste de réponse condensée via un identifiant de questionnaire et le dernier répondant qui a été obtenu avec succès.
        /// Si aucun répondant n'a jamais été obtenu, utilisez la valeur zéro.
        public async Task<List<IBrandCondensedQuestionnaireAnswer>> GetCondensedQuestionnaire(string questionnaireId, string lastRespondentNumber, 
            Method webMethod = Method.Get, CancellationToken? cToken = null)
        {
            if (string.IsNullOrWhiteSpace(questionnaireId) || string.IsNullOrWhiteSpace(lastRespondentNumber)) return null;
            
            var url = $"{Url}/{AppName}/GetCondensedQuestionnaireAnswers?questionnaire={questionnaireId}&resp_number={lastRespondentNumber}";
            log.Debug(url);
            var req = await HttpRestUtil.ExecuteCompleteRestRequestAsync<List<IBrandCondensedQuestionnaireAnswer>>(cToken ?? CancellationToken.None, url);

            var result = req?.InstancedBody;
            
            if (log.IsTraceEnabled)
            {
                if (result != null && result.Any())
                {
                    log.Trace($"Fetched respondents {result.First().Numero} to {result.Last().Numero}. Size={result.Count}");
                }
                else
                {
                    log.Trace(result == null ? "Failed query." : "No data.");
                }
            }

            return result;
        }
    }
}