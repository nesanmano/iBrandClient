using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using NLog;
using RestSharp;

namespace NomadLogic.Common.Util.Networking.QuestionnaireWebRequest.Emisys
{
    /// <summary>
    /// Cette classe sert à consommer le service REST pour les cartes RFID à partir de l'API de Emisys.
    /// </summary>
    [PublicAPI]
    public class WebRequestEmisysStandard : WebRequestEmisysAbstract
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        /// Teste la disponibilité du serveur Emisys.
        public override async Task<bool> TestRestEndPoint()
        {
            if (DoNotTestEndPointInit) return true;

            var client = new RestClient($"{LoginInfo.RootApiUrl}/{LoginInfo.Version}/core/access/zones/");

            var req = new RestRequest("", Method.Get);
            req.AddHeader("cache-control", "no-cache");
            req.AddHeader("accept", "application/json");
            req.AddHeader("apikey", LoginInfo.Apikey);
            req.Timeout = Timeout;

            var cts = new CancellationTokenSource(Timeout);

            try
            {
                var response = await client.ExecuteAsync(req, cts.Token);
                return response.StatusCode == HttpStatusCode.OK;
            }
            catch (Exception e)
            {
                log.Warn(e, "Could not test Emisys end-point by web request: " + req);
                return false;
            }
        }

        /// <summary>
        /// Exécute les requêtes nécessaire pour obtenir les information d'un membre à partir d'un identifiant de carte.
        /// On commence par obtenir le "MemberCardInfo", puis on utilise ces données (si réussi) pour obtenir
        /// les informations de "MemberInfo"
        /// </summary>
        /// <param name="cardUid">l'identifiant unique de la carte RFID</param>
        /// <param name="webMethod">Le type de demande web.</param>
        /// <param name="cToken">Jeton de cancellation optionel.</param>
        /// <returns>Les information de l'usager</returns>
        public async Task<MemberIndividuInfo> GetMemberIndividuInfoFromCard(string cardUid, Method webMethod, CancellationToken cToken)
        {
            if (string.IsNullOrWhiteSpace(cardUid)) return null;

            // Étape 1 - Obtenir l'identifiant du membre via la carte.
            var cardInfo = await GetMemberCardInfo(cardUid, webMethod, cToken);

            // Étape 2 - On appelle le résultat d'obtenir les informations de l'individu via son ID naturel.
            return await GetMemberIndividuInfo(cardInfo?.MemberId, webMethod, cToken);
        }

        /// <summary>
        /// Obtient les information complète d'un individu via son identifiant de membre.
        /// </summary>
        /// <param name="cardUid">L'identifiant unique de l'individu</param>
        /// <param name="webMethod">La méthode web à utiliser.</param>
        /// <param name="cToken">Jeton de cancellation optionel.</param>
        /// <returns>Les information complète de l'individu.</returns>
        public async Task<CardValueInfo> GetMemberCardInfo(string cardUid, Method webMethod, CancellationToken cToken)
        {
            if (string.IsNullOrWhiteSpace(cardUid)) return null;
            
            var url = $"{LoginInfo?.RootApiUrl}/{LoginInfo?.Version}/cashless/cards/{cardUid.Trim()}";

            //var res = await HttpRestUtil.ExecuteCompleteRestRequest<CardValueInfo>(url, CommonHeaders, cToken, webMethod);
            var res = await HttpRestUtil.ExecuteCompleteRestRequestAsync<CardValueInfo>(cToken, url, CommonHeaders);
            
            return res?.InstancedBody;
        }

        /// <summary>
        /// Obtient les information complète d'un individu via son identifiant de membre.
        /// </summary>
        /// <param name="memberId">L'identifiant unique de l'individu</param>
        /// <param name="webMethod">La méthode web à utiliser.</param>
        /// <param name="cToken">Jeton de cancellation optionel.</param>
        /// <returns>Les information complète de l'individu.</returns>
        public async Task<MemberIndividuInfo> GetMemberIndividuInfo(string memberId, Method webMethod, CancellationToken cToken)
        {
            if (string.IsNullOrWhiteSpace(memberId)) return null;

            var url = $"{LoginInfo?.RootApiUrl}/{LoginInfo?.Version}/members/{memberId.Trim()}";
            var res = await HttpRestUtil.ExecuteCompleteRestRequestAsync<MemberIndividuInfo>(cToken, url, CommonHeaders);
            return res?.InstancedBody;
        }

        /// <summary>
        /// Obtient les information complète d'un individu via son identifiant de membre. Légère variation de point d'accès (user vs member).
        /// La structure de résultat est sensiblement la même.
        /// </summary>
        /// <param name="userId">L'identifiant unique de l'individu</param>
        /// <param name="webMethod">La méthode web à utiliser.</param>
        /// <param name="cToken">Jeton de cancellation optionel.</param>
        /// <returns>Les information complète de l'individu.</returns>
        public async Task<MemberIndividuInfo> GetUserIndividuInfo(string userId, Method webMethod, CancellationToken cToken)
        {
            if (string.IsNullOrWhiteSpace(userId)) return null;

            var url = $"{LoginInfo?.RootApiUrl}/{LoginInfo?.Version}/users/{userId.Trim()}";
            var res = await HttpRestUtil.ExecuteCompleteRestRequestAsync<MemberIndividuInfo>(cToken, url, CommonHeaders);
            return res?.InstancedBody;
        }

        /// <summary>
        /// Exécute les requêtes nécessaire pour obtenir les information d'un USER à partir d'un identifiant de carte.
        /// On commence par obtenir le "MemberCardInfo", puis on utilise ces données (si réussi) pour obtenir
        /// les informations de "MemberInfo".
        /// Techniquement, on obtient un "user" et non un membre, mais les structure de données sont similaires.
        /// </summary>
        /// <param name="cardUid">l'identifiant unique de la carte RFID</param>
        /// <param name="webMethod">Le type de demande web.</param>
        /// <param name="cToken">Jeton de cancellation optionel.</param>
        /// <returns>Les information de l'usager</returns>
        public async Task<MemberIndividuInfo> GetUserIndividuInfoFromCard(string cardUid, Method webMethod, CancellationToken cToken)
        {
            if (string.IsNullOrWhiteSpace(cardUid)) return null;

            // Étape 1 - Obtenir l'identifiant du membre via la carte.
            var mci = await GetMemberCardInfo(cardUid, webMethod, cToken);

            // Étape 2 - On appelle le résultat d'obtenir les informations de l'individu via son ID naturel.
            return await GetUserIndividuInfo(mci?.MemberId, webMethod, cToken);
        }
    }
}
