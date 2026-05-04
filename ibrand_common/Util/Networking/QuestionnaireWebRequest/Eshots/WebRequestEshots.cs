using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using NomadLogic.Common.Services;
using static System.Net.Http.HttpMethod;

namespace NomadLogic.Common.Util.Networking.QuestionnaireWebRequest.Eshots
{
    /// <summary>
    /// Classe pour les requêtes web vers le serveur de Eshots pour leur API.
    /// Il s'agit d'un système avec jetons d'identification et de rafraîchissement. Quand une requête est faite, il peut
    /// y avoir refus et dans ce cas il faut redemander un jeton de connexion et de rafraîshissement, ainsi que de
    /// faire des rafraîchissements de temps en temps.
    /// De plus, il y a un système de sélection de jours, et ces jours sont, littéralement, des jours de calendriers valides.
    /// Le résultat des requête peut être analysé, mais plus facilement, ils utilisent le code HTTP de retour pour le résultat global
    /// des requêtes: 200 (Ok), 201 (Created), 400 (bad request), 401 (Not authorized), 403 (Access denied), 404 (Not found).
    /// Basé sur la révision 2.1 de leur document.
    /// </summary>
    public class WebRequestEshots : IWebRequestExecutor
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Information d'authentification pour le serveur API de Eshots.
        /// </summary>
        public EshotsServerInfo ServerInfo { get; set; } = new EshotsServerInfo();

        /// <summary>
        /// Lien vers la couche de persistence des informations, pour faciliter la sauvegarde automatisée des informations.
        /// Si cette propriété est nulle, aucune sauvegarde ne sera faite et aucune commande ne fera de sauvegarde pour faciliter
        /// les commandes subséquentes: les informations passée devront être conservées en mémoire dans les classes appellantes.
        /// </summary>
        public IDatabaseEshots EshotsDatabase { get; set; }

        public void Dispose()
        {
            EshotsDatabase?.Dispose();
        }

        public Task<bool> InitializeExecutor()
        {
            log.Trace("Initializing abstract Emisys executor");
            if (ServerInfo?.Version == null) return Task.FromResult(false);
            if (ServerInfo?.RootApiUrl == null) return Task.FromResult(false);

            return Task.FromResult(true);
        }

        public int Timeout { get; set; } = 5000;
        
        #region Méthodes de requêtes simples

        /// <summary>
        /// Cette commande fait la demande initiale de demande d'authorisation avec un nom d'usager et mot de passe.
        /// Si ça réussit, les jetons sont envoyés et peuvent être utilisés immédiatement.
        /// </summary>
        /// <param name="logInfo">Information de Login.</param>
        /// <param name="cToken">Jeton de cancellation.</param>
        /// <returns>Le résultat de la requête REST.</returns>
        public async Task<RestRequestJsonResponse<EshotsAuthentificationResultInfo>> GetFullAuthentication(EshotsAuthentificationRequestInfo logInfo, CancellationToken cToken)
        {
            if (logInfo == null || ServerInfo == null) return null;

            log.Trace("Sending full Eshots authentification request.");
            var url = $"{ServerInfo?.RootApiUrl}api/{ServerInfo?.Version}/auth";
            
            var res = await HttpRestUtil.ExecuteCompleteRestRequestAsync<EshotsAuthentificationResultInfo>(cToken, url, new HttpJsonBody(logInfo), null, Post);
            return res;
        }

        /// <summary>
        /// Refraîchit les jetons avec le jeton de rafraîchissement donné au préalable.
        /// </summary>
        /// <param name="refreshToken">Le jeton de rafraîchissement.</param>
        /// <param name="cToken">Le jeton de cancellation.</param>
        /// <returns>Le résultat du rafraîchissement (similaire à un log complet)</returns>
        public async Task<RestRequestJsonResponse<EshotsAuthentificationResultInfo>> RenewToken(string refreshToken, CancellationToken cToken)
        {
            if (refreshToken == null || ServerInfo == null) return null;

            log.Trace("Sending refresh token Eshots request.");
            var url = $"{ServerInfo?.RootApiUrl}api/{ServerInfo?.Version}/auth/refresh";

            var pars = new List<RestRequestAdditionalParameters>
            {
                new RestRequestAdditionalParameters("Bearer " + refreshToken, "Authorization", HttpParameterType.Header)
                //,new RestRequestAdditionalParameters("application/json", "Content-Type", HttpParameterType.Header)
            };

            var res = await HttpRestUtil.ExecuteCompleteRestRequestAsync<EshotsAuthentificationResultInfo>(cToken, url, pars, Post);

            return res;
        }

        /// <summary>
        /// Obtient la "master list" des événements lié à l'usager du jeton donné en paramètre.
        /// Va uniquement chercher les événement en cours ou à venir.
        /// </summary>
        /// <param name="accessToken">Le jeton d'accès.</param>
        /// <param name="cToken">Le jeton de cancellation.</param>
        /// <returns>La liste des résultats.</returns>
        public async Task<RestRequestJsonResponse<EshotsEventsResultInfo>> GetEventList(string accessToken, CancellationToken cToken)
        {
            return await GetEventList(accessToken, false, cToken);
        }

        /// <summary>
        /// Obtient la "master list" des événements lié à l'usager du jeton donné en paramètre.
        /// Si on veut la liste des événements passés, on peut donner l'option de le faire.
        /// </summary>
        /// <param name="accessToken">Le jeton d'accès.</param>
        /// <param name="pastEvents">Indique si on veut les événements passés (selon le serveur).</param>
        /// <param name="cToken">Le jeton de cancellation.</param>
        /// <returns>La liste des résultats.</returns>
        public async Task<RestRequestJsonResponse<EshotsEventsResultInfo>> GetEventList(string accessToken, bool pastEvents, CancellationToken cToken)
        {
            if (accessToken == null || ServerInfo == null) return null;

            log.Trace("Sending Eshots event listing request.");
            var url = $"{ServerInfo?.RootApiUrl}api/{ServerInfo?.Version}/events";
            if (pastEvents)
            {
                url += "?past=1";
            }

            var pars = new List<RestRequestAdditionalParameters>
            {
                new RestRequestAdditionalParameters("Bearer " + accessToken, "Authorization", HttpParameterType.Header)
                //,new RestRequestAdditionalParameters("application/json", "Content-Type", HttpParameterType.Header)
            };

            var res = await HttpRestUtil.ExecuteCompleteRestRequestAsync<EshotsEventsResultInfo>(cToken, url, pars);
            return res;
        }

        /// <summary>
        /// Similaire à "GetEventList", mais retourne une version plus courte qui contient la liste d'événements avec leurs jours avec un minimum d'information
        /// pour les événements. Plus utile si on veut obtenir les jours d'activations plus rapidement.
        /// </summary>
        /// <param name="accessToken">Le jeton d'accès.</param>
        /// <param name="pastEvents">Indique si on veut les événements passé (selon la date du serveur).</param>
        /// <param name="cToken">Le jeton de cancellation.</param>
        /// <returns>La liste (minimale) des événements et de leurs journées d'activation.</returns>
        public async Task<RestRequestJsonResponse<EshotsEventDaysResultInfo>> GetEventDaysList(string accessToken, bool pastEvents, CancellationToken cToken)
        {
            if (accessToken == null || ServerInfo == null) return null;

            log.Trace("Sending Eshots event days listing request.");
            var url = $"{ServerInfo?.RootApiUrl}api/{ServerInfo?.Version}/events/eventdays";
            if (pastEvents)
            {
                url += "?past=1";
            }

            var pars = new List<RestRequestAdditionalParameters>
            {
                new RestRequestAdditionalParameters("Bearer " + accessToken, "Authorization", HttpParameterType.Header)
                //,new RestRequestAdditionalParameters("application/json", "Content-Type", HttpParameterType.Header)
            };

            var res = await HttpRestUtil.ExecuteCompleteRestRequestAsync<EshotsEventDaysResultInfo>(cToken, url, pars);
            return res;
        }

        /// <summary>
        /// Obtient les informations d'un consommateur de Eshots via un moyen d'identification spécifique.
        /// Ce moyen peut être, au choix, un identifiant interne à Eshots, un identifiant externe à Eshots,
        /// son courriel ou son numéro de téléphone (format nord-américain, je crois).
        /// </summary>
        /// <param name="id">La valeur numérique de l'identifiant.</param>
        /// <param name="idType">La type de l'identifiant (pour préfacer dans le URL de requête)</param>
        /// <param name="accessToken">Le jeton d'accès.</param>
        /// <param name="cToken">Le jeton de cancellation.</param>
        /// <returns>La réponse du serveur.</returns>
        public async Task<RestRequestJsonResponse<EshotsConsumerResultInfo>> GetConsumerFromId(string id, EshotsConsumerRequestIdType idType, string accessToken, CancellationToken cToken)
        {
            if (accessToken == null || ServerInfo == null || id == null) return null;

            log.Trace("Sending Eshots consumer identification request.");
            var sb = new StringBuilder();
            sb.Append($"{ServerInfo?.RootApiUrl}api/{ServerInfo?.Version}/consumers?");
            switch (idType)
            {
                case EshotsConsumerRequestIdType.EventTokenId:
                    sb.Append("eventTokenId=");
                    break;
                case EshotsConsumerRequestIdType.ExternalId:
                    sb.Append("uId=");
                    break;
                case EshotsConsumerRequestIdType.Email:
                    sb.Append("email=");
                    break;
                case EshotsConsumerRequestIdType.Phone:
                    sb.Append("phone=");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(idType), idType, null);
            }

            sb.Append(id);

            var pars = new List<RestRequestAdditionalParameters>
            {
                new RestRequestAdditionalParameters("Bearer " + accessToken, "Authorization", HttpParameterType.Header),
                new RestRequestAdditionalParameters("application/json", "Content-Type", HttpParameterType.Header)
            };

            var url = sb.ToString();
            log.Debug("URL request sent (GET): {0}", url);

            var res = await HttpRestUtil.ExecuteCompleteRestRequestAsync<EshotsConsumerResultInfo>(cToken, url, pars);
            return res;
        }

        /// <summary>
        /// Envoi un fichier vers le serveur de Eshots. Ne pas oublier que le fichier doit être convertit en Base64, puis stocké directement dans le "JSON"
        /// de l'instance de configuration. Cette instance peut contenir plusieurs fichiers à la fois, mais il est recommandé de ne pas trop mettre de données
        /// en mémoire.
        /// Comme il s'agit d'un envoi qui peut prendre pas mal de temps, le "timeout" est multiplié par 10.
        /// </summary>
        /// <param name="uploadData">L'instance du "request body".</param>
        /// <param name="accessToken">Le jeton de communication.</param>
        /// <param name="cToken">Le jeton de cancellation.</param>
        /// <returns>La réponse du serveur.</returns>
        public async Task<RestRequestJsonResponse<EshotsFileUploadsResultInfo>> UploadFileLinkedToConsumer(EshotsFileUploadsRequest uploadData, string accessToken, CancellationToken cToken)
        {
            if (accessToken == null || ServerInfo == null || uploadData == null) return null;

            log.Trace("Sending Eshots consumer identification request.");
            var url = $"{ServerInfo?.RootApiUrl}api/{ServerInfo?.Version}/media";
            
            var pars = new List<RestRequestAdditionalParameters>
            {
                new RestRequestAdditionalParameters(new HttpJsonBody(uploadData)),
                new RestRequestAdditionalParameters("Bearer " + accessToken, "Authorization", HttpParameterType.Header)
            };

            var extraCodes = new HashSet<HttpStatusCode>
            {
                HttpStatusCode.BadRequest
            };

            var res = await HttpRestUtil.ExecuteCompleteRestRequestAsync<EshotsFileUploadsResultInfo>(cToken, url, pars, extraCodes, Post);
            return res;
        }

        #endregion

        #region Méthodes de requêtes combinées

        public async Task<EshotsConsumerIdentificationResult> GetConsumerRequestFromFacade(string id, string typeStr, CancellationToken cToken)
        {
            if (ServerInfo == null)
            {
                log.Error("No server information configured.");
                return new EshotsConsumerIdentificationResult { ExecutionResult = EshotsComplexRequestResultEnum.Error };
            }
            
            if (!Enum.TryParse<EshotsConsumerRequestIdType>(typeStr, true, out var type))
            {
                log.Error("Could not parse enum (EshotsConsumerRequestIdType) for value: " + typeStr);
                return new EshotsConsumerIdentificationResult { ExecutionResult = EshotsComplexRequestResultEnum.Error };
            }

            // Lecture des configurations à partir du service API
            var serviceApi = ServicesContainer.GetService<EshotsApiService>();
            serviceApi = EshotsApiService.InitialInitCheck(serviceApi, EshotsDatabase);

            if (serviceApi == null)
            {
                log.Error("Eshots API service could not be initialized.");
                return new EshotsConsumerIdentificationResult { ExecutionResult = EshotsComplexRequestResultEnum.Error };
            }

            if (EshotsDatabase == null)
            {
                EshotsDatabase = serviceApi.LocalDatabase;
                if (EshotsDatabase == null)
                {
                    log.Error("No valid database was given before execution and Eshots API service does not contain a valid database reference.");
                    return new EshotsConsumerIdentificationResult { ExecutionResult = EshotsComplexRequestResultEnum.Error };
                }
            }

            var status = serviceApi.ApiState;
            // On vérifie les cas que l'on ne peut pas régler dans cette méthode, et qui requiert l'utilisation du dialogue de configuration.
            switch (status)
            {
                case EshotsApiConfigState.TodayInvalid: return new EshotsConsumerIdentificationResult { ExecutionResult = EshotsComplexRequestResultEnum.InvalidDay };
                case EshotsApiConfigState.FullLoginRequired: return new EshotsConsumerIdentificationResult { ExecutionResult = EshotsComplexRequestResultEnum.RequireLogin };
                case EshotsApiConfigState.ManualSetupRequired: return new EshotsConsumerIdentificationResult { ExecutionResult = EshotsComplexRequestResultEnum.RequireLogin };
            }

            if (cToken.IsCancellationRequested) return new EshotsConsumerIdentificationResult { ExecutionResult = EshotsComplexRequestResultEnum.Cancelled };

            var config = EshotsDatabase.GetLastEshotsConfiguration();
            if (config == null || string.IsNullOrWhiteSpace(config.AccessToken) || string.IsNullOrWhiteSpace(config.RefreshToken))
            {
                log.Error("Eshots API service and configuration are disjoint (no valid tokens).");
                return new EshotsConsumerIdentificationResult { ExecutionResult = EshotsComplexRequestResultEnum.RequireLogin };
            }

            // Vérification et renouvellement du jeton de rafraîchissement la cas échéant.
            var renewAttempt = await TryRenewToken(serviceApi, config, status, cToken);
            if (renewAttempt != EshotsComplexRequestResultEnum.Continue)
            {
                return new EshotsConsumerIdentificationResult { ExecutionResult = renewAttempt };
            }

            if (cToken.IsCancellationRequested) return new EshotsConsumerIdentificationResult { ExecutionResult = EshotsComplexRequestResultEnum.Cancelled };

            // Token valide
            var res = await GetConsumerFromId(id, type, config.AccessToken, cToken);
            if (res == null)
            {
                log.Error("Error during consumer information fetch (null).");
                return new EshotsConsumerIdentificationResult { ExecutionResult = EshotsComplexRequestResultEnum.InternetProblem };
            }

            if (res.StatusCode != HttpStatusCode.OK)
            {
                log.Info("Invalid status code from request.");
                return new EshotsConsumerIdentificationResult { ExecutionResult = EshotsComplexRequestResultEnum.ServerError };
            }

            var resultValue = res.InstancedBody.ConsumerIdentificationResult;
            resultValue.ExecutionResult = EshotsComplexRequestResultEnum.Success;

            return resultValue;
        }

        /// <summary>
        /// Tente un renouvellement du jeton avec des configuration existantes. Selon le résultat (si un rafraîchissement est nécessaire), on met à jour les variables
        /// de configuration (effet de bord) et on continue. Si le résultat n'est pas "Continue", alors il faut arrêter et retourner le résultat par la méthode appellante,
        /// car d'autres actions sont nécessaire.
        /// </summary>
        /// <param name="serviceApi">Le service API en cours.</param>
        /// <param name="config">La configuration persistante.</param>
        /// <param name="initialStatus">Le status initial (tel que donnée par l'instance du service API en cours).</param>
        /// <param name="cToken">Un jeton de cancellation.</param>
        /// <returns>Si n'est pas "Continue", l'appellant doit retourner ce résultat plus haut.</returns>
        public async Task<EshotsComplexRequestResultEnum> TryRenewToken(EshotsApiService serviceApi, EshotsDataConfiguration config, EshotsApiConfigState initialStatus, CancellationToken cToken)
        {
            if (initialStatus != EshotsApiConfigState.RenewRequired) return EshotsComplexRequestResultEnum.Continue;
            log.Debug("Tentative de renouvellement du jeton de configuration.");

            var refreshReq = await RenewToken(config.RefreshToken, cToken);
            if (refreshReq == null)
            {
                log.Error("Refresh token command has failed.");
                return EshotsComplexRequestResultEnum.InternetProblem;
            }

            if (refreshReq.StatusCode == HttpStatusCode.BadRequest || refreshReq.StatusCode == HttpStatusCode.Unauthorized)
            {
                log.Warn("Bad token format or expired token. Login required.");
                return EshotsComplexRequestResultEnum.RequireLogin;
            }

            // Autres erreurs
            if (refreshReq.StatusCode != HttpStatusCode.OK)
            {
                log.Error("Unknown status request. Please investigate: " + refreshReq.ToStringPublicProperties(1));
                return EshotsComplexRequestResultEnum.Error;
            }

            if (cToken.IsCancellationRequested) return EshotsComplexRequestResultEnum.Cancelled;

            // Mise à jour du service et de la couche de persistance.
            config.AccessToken = refreshReq.InstancedBody.AccessToken;
            config.RefreshToken = refreshReq.InstancedBody.RefreshToken;

            if (string.IsNullOrWhiteSpace(config.AccessToken) || string.IsNullOrWhiteSpace(config.RefreshToken))
            {
                log.Error("Eshots authentification service seems to be anormal. No access token sent while returning an OK status. Please investigate.\n" + refreshReq.InstancedBody.ToStringPublicProperties());
                return EshotsComplexRequestResultEnum.Error;
            }

            config.ConfigurationDate = DateTime.Now;
            if (!EshotsDatabase.RegisterCurrentEshotsConfiguration(config))
            {
                log.Error("Could not save modified configuration to disk: " + config.ToStringPublicProperties(1));
                return EshotsComplexRequestResultEnum.Error;
            }

            serviceApi.LastLogin = config.ConfigurationDate;
            switch (serviceApi.ApiState)
            {
                case EshotsApiConfigState.TodayInvalid: return EshotsComplexRequestResultEnum.InvalidDay;
                case EshotsApiConfigState.FullLoginRequired: return EshotsComplexRequestResultEnum.RequireLogin;
                case EshotsApiConfigState.ManualSetupRequired: return EshotsComplexRequestResultEnum.RequireLogin;
                case EshotsApiConfigState.RenewRequired:
                {
                    log.Warn("Despite renewing token being successful, renew is asked again.");
                    return EshotsComplexRequestResultEnum.RequireLogin;
                }
            }

            // Tout est ok.
            return EshotsComplexRequestResultEnum.Continue;
        }

        #endregion

    }
}