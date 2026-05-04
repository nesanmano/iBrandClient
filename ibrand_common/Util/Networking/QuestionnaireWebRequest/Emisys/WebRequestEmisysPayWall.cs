using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using RestSharp;
using static NomadLogic.Common.Util.Networking.HttpRestUtil;

namespace NomadLogic.Common.Util.Networking.QuestionnaireWebRequest.Emisys
{
    /// <summary>
    /// Classe d'implémentation pour des requête Emisys pour le projet de mur payant pour le project MySpend wall (DIA-2018-02)
    /// </summary>
    public class WebRequestEmisysPayWall : WebRequestEmisysAbstract
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Cette méthode permet d'initialiser une carte RFID pour un usager. Si cet usager n'existe pas, il sera créé.
        /// </summary>
        /// <param name="cardUid">Identifiant de carte.</param>
        /// <param name="cToken">Jeton de cancellation. Optionnel.</param>
        /// <param name="bodyData">Objet info pour les données à envoyer.</param>
        /// <returns>Une objet de réponse REST propre à cette commande.</returns>
        public async Task<ResponseInitCardInfo> InitCard(string cardUid, PostInitCardInfo bodyData, CancellationToken? cToken = null)
        {
            if (string.IsNullOrWhiteSpace(cardUid)) return null;

            var url = $"{LoginInfo?.RootApiUrl}/v{LoginInfo?.Version}/users/card/{cardUid.Trim()}/init";
            var tok = cToken ?? CancellationToken.None;

            var pars = CommonHeaders;
            pars.Add(new RestRequestAdditionalParameters(new HttpJsonBody(bodyData)));

            //var reqResponse =  await ExecuteCompleteRestRequest<ResponseInitCardInfo>(url, CommonHeaders, tok, Method.POST, Timeout, body);
            var reqResponse =  await ExecuteCompleteRestRequestAsync<ResponseInitCardInfo>(tok, url, pars, null, HttpMethod.Post);

            if (reqResponse?.InstancedBody == null)
            {
                log.Warn("Null return. Request failed.");
                return null;
            }

            var returnData = reqResponse.InstancedBody;

            if (returnData.Result != 2)
            {
                if (!log.IsDebugEnabled) return returnData;
                log.Debug($"Request details (success): \nURL={url}\nHeaders={StringUtil.FormatEnumerable(pars)}\nBody={ClassUtil.ToStringPublicProperties(bodyData, 1)}\n");
                log.Debug(ClassUtil.ToStringPublicProperties(reqResponse, 1));
                return returnData;
            }

            log.Warn("No user could be properly identified. Request failed.");
            if (!log.IsDebugEnabled) return null;
            log.Debug($"Request details (failiure): \nURL={url}\nHeaders={StringUtil.FormatEnumerable(pars)}\nBody={ClassUtil.ToStringPublicProperties(bodyData, 1)}\n");
            log.Debug(ClassUtil.ToStringPublicProperties(reqResponse, 1));
            return null;
        }

        /// <summary>
        /// Finalize un usager sur une carte.
        /// </summary>
        /// <param name="cardUid">L'identifiant de la carte.</param>
        /// <param name="cToken">Jeton de cancellation.</param>
        /// <returns>Un objet contenant toutes les informations de l'usager à ce stade, contenue sur les serveurs d'Emisys.</returns>
        public async Task<FinalizeCardInfo> FinalizeCard(string cardUid, CancellationToken? cToken = null)
        {
            var url = $"{LoginInfo?.RootApiUrl}/v{LoginInfo?.Version}/users/card/{cardUid.Trim()}/finalize";
            var reqResponse = await FinalizeCardFullAnswer(cardUid, cToken);

            if (reqResponse?.InstancedBody == null)
            {
                log.Warn("Null return. Request failed.");
                return null;
            }

            var returnData = reqResponse.InstancedBody;
            if (returnData == null)
            {
                log.Warn("Null body instance. Request failed.");
                return null;
            }

            if (returnData.Result != 2)
            {
                if (!log.IsDebugEnabled) return returnData;
                log.Debug($"Request details (success): \nURL={url}\nHeaders={StringUtil.FormatEnumerable((Array)CommonHeaders)}\n");
                log.Debug(ClassUtil.ToStringPublicProperties(reqResponse, 1));
                return returnData;
            }

            log.Warn("No user could be properly identified. Request failed.");
            if (!log.IsDebugEnabled) return null;
            log.Debug($"Request details (failiure): \nURL={url}\nHeaders={StringUtil.FormatEnumerable((Array)CommonHeaders)}\n");
            log.Debug(ClassUtil.ToStringPublicProperties(reqResponse, 1));
            return null;
        }

        /// <summary>
        /// Version sans vérification de la requête de finalisation.
        /// </summary>
        /// <param name="cardUid"></param>
        /// <param name="cToken"></param>
        /// <returns></returns>
        public async Task<RestRequestJsonResponse<FinalizeCardInfo>> FinalizeCardFullAnswer(string cardUid, CancellationToken? cToken = null)
        {
            if (string.IsNullOrWhiteSpace(cardUid)) return null;

            var url = $"{LoginInfo?.RootApiUrl}/v{LoginInfo?.Version}/users/card/{cardUid.Trim()}/finalize";

            var tok = cToken ?? CancellationToken.None;
            return await ExecuteCompleteRestRequestAsync<FinalizeCardInfo>(tok, url, CommonHeaders);
        }

        /// Teste la disponibilité du serveur Emisys.
        public override async Task<bool> TestRestEndPoint()
        {
            if (!_init) return false;

            try
            {
                var client = new RestClient(LoginInfo.RootApiUrl);

                var req = new RestRequest("/products/items", Method.Get);
                req.AddHeader("cache-control", "no-cache");
                req.AddHeader("apikey", LoginInfo.Apikey);
                req.AddHeader("email", LoginInfo.UserEmail);
                req.Timeout = Timeout;

                var cts = new CancellationTokenSource(Timeout);

                var response = await client.ExecuteAsync(req, cts.Token);
                return response.StatusCode == HttpStatusCode.OK;
            }
            catch (Exception e)
            {
                log.Warn(e, "Could not test Emisys end-point by web request.");
                return false;
            }
        }
    }
}