using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NLog;
using NomadLogic.Common.Util.Print;
using RestSharp;

namespace NomadLogic.Common.Util.Networking.QuestionnaireWebRequest.Emisys
{
    /// <summary>
    /// Requêtes web pour initialisé un panier d'achat et mettre des achats dedans.
    /// Ce que le "panier" représente et ce qu'un "achat" est peut varier grandement.
    /// </summary>
    public class WebRequestEmisysBasketTransaction : WebRequestEmisysAbstract
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

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

        /// <summary>
        /// Créer un nouveau panier. Pour ce faire, il faut avoir le code de terminal authorisé (TerminalId), l'identifiant de la feuille de caisse (SheetcashId),
        /// l'identifiant du produit et le nombre à créer, en plus des informations d'identification des headers.
        /// </summary>
        /// <param name="creationPanier">Information de création du panier.</param>
        /// <param name="webMethod">La méthode web. Convertit automatiquement en Post.</param>
        /// <param name="cToken">Si on utilise un jeton de cancellation.</param>
        /// <returns>Le résultat de la requête.</returns>
        public async Task<BasketCommandStatusInfo> CreateBasketTransaction(BasketCreationInfo creationPanier, Method webMethod = Method.Post, CancellationToken? cToken = null)
        {
            if (creationPanier == null) return null;

            var url = $"{LoginInfo?.RootApiUrl}/v{LoginInfo?.Version}/cashless/debit";

            log.Trace("Request Url: {0}\nRequest body: {1}", url, creationPanier);
            
            var pars = new List<RestRequestAdditionalParameters>
            {
                new RestRequestAdditionalParameters(new HttpJsonBody(creationPanier)),
                new RestRequestAdditionalParameters("no-cache", "cache-control", HttpParameterType.Header),
                new RestRequestAdditionalParameters(LoginInfo?.Apikey, "apikey", HttpParameterType.Header),
                new RestRequestAdditionalParameters(LoginInfo?.UserEmail, "email", HttpParameterType.Header)
            };
            
            var req =  await HttpRestUtil.ExecuteCompleteRestRequestAsync<BasketCommandStatusInfo>(cToken ?? CancellationToken.None, url, pars, null, HttpMethod.Post);

            var result = req?.InstancedBody;

            log.Debug("Basket transaction sent: {0}", result);

            return result;
        }

        /// <summary>
        /// Tentative d'achat sur la panier visé.
        /// </summary>
        /// <param name="basketId">L'identifiant du panier visé.</param>
        /// <param name="paiementPanier">Information sur le paiement.</param>
        /// <param name="webMethod">Post obligé.</param>
        /// <param name="cToken">Le jeton de cancellation.</param>
        /// <returns>Le résultat de la requête web Emisys.</returns>
        public async Task<BasketCommandStatusInfo> PayBasketTransaction(int basketId, BasketTransactionInfo paiementPanier, Method webMethod = Method.Post, CancellationToken? cToken = null)
        {
            if (paiementPanier == null) return null;

            var url = $"{LoginInfo?.RootApiUrl}/v{LoginInfo?.Version}/cashless/debit/{basketId}/pay";

            log.Trace("Request Url: {0}\nRequest body: {1}", url, paiementPanier);

            var pars = new List<RestRequestAdditionalParameters>
            {
                new RestRequestAdditionalParameters(new HttpJsonBody(paiementPanier)),
                new RestRequestAdditionalParameters("no-cache", "cache-control", HttpParameterType.Header),
                new RestRequestAdditionalParameters(LoginInfo?.Apikey, "apikey", HttpParameterType.Header),
                new RestRequestAdditionalParameters(LoginInfo?.UserEmail, "email", HttpParameterType.Header)
            };

            var result = await HttpRestUtil.ExecuteCompleteRestRequestAsync<BasketCommandStatusInfo>(cToken ?? CancellationToken.None, url, pars, null, HttpMethod.Post);
            
            log.Debug("Basket payment sent: {0}", result);

            return result.InstancedBody;
        }

        /// <summary>
        /// Tentative d'annulation sur la panier visé.
        /// </summary>
        /// <param name="basketId">L'identifiant du panier visé.</param>
        /// <param name="paiementPanier">Information sur le paiement.</param>
        /// <param name="webMethod">Post obligé.</param>
        /// <param name="cToken">Le jeton de cancellation.</param>
        /// <returns>Le résultat de la requête web Emisys.</returns>
        public async Task<BasketCommandStatusInfo> CancelBasketTransaction(int basketId, BasketTransactionInfo paiementPanier, Method webMethod = Method.Post, CancellationToken? cToken = null)
        {
            if (paiementPanier == null) return null;

            var url = $"{LoginInfo?.RootApiUrl}/v{LoginInfo?.Version}/cashless/debit/{basketId}/cancel";

            log.Trace("Request Url: {0}\nRequest body: {1}", url, paiementPanier);

            var pars = new List<RestRequestAdditionalParameters>
            {
                new RestRequestAdditionalParameters(new HttpJsonBody(paiementPanier)),
                new RestRequestAdditionalParameters("no-cache", "cache-control", HttpParameterType.Header),
                new RestRequestAdditionalParameters(LoginInfo?.Apikey, "apikey", HttpParameterType.Header),
                new RestRequestAdditionalParameters(LoginInfo?.UserEmail, "email", HttpParameterType.Header)
            };

            var result = await HttpRestUtil.ExecuteCompleteRestRequestAsync<BasketCommandStatusInfo>(cToken ?? CancellationToken.None, url, pars, null, HttpMethod.Post);

            log.Debug("Basket cancellation sent: {0}", result);

            return result.InstancedBody;
        }

        /// <summary>
        /// Cette méthode est une composite de la commande de transaction.
        /// On créé un panier d'un type spécifique, puis on tente de d'imprimer un fichier d'image. Selon le résultat de cette opération,
        /// on va tenter de confirmer ou d'annuler le panier créé.
        /// Plusieures vérifications sont faites pour s'assurer que le fichier est imprimable et que les configurations d'imprimantes sont fonctionnelles.
        /// </summary>
        /// <param name="cardUid">La carte qui a fait la transaction.</param>
        /// <param name="creationPanier">Les détails de création du panier.</param>
        /// <param name="filePath">Le chemin complet du fichier à imprimer. Les configurations d'impression sont globales.</param>
        /// <param name="webMethod">Post par obligation. Le contenue de cette variable importe peu.</param>
        /// <param name="cToken">Si on utilise un jeton de cancellation.</param>
        /// <returns>Une énumération qui indique l'état de la requête ou son réultat. En gros, c'estuniquement si on retourne "payment_accept"
        /// que l'on peut dire que tout à bien été.</returns>
        public async Task<BasketStatusEnum> FullPrintTransaction(string cardUid, BasketCreationInfo creationPanier, string filePath,
            Method webMethod = Method.Post, CancellationToken? cToken = null)
        {
            if (!PrintUtil.IsPrintReady())
            {
                log.Warn("Les configurations d'imprimante sont invalides.");
                return BasketStatusEnum.InvalidPrinterState;
            }

            if (creationPanier == null)
            {
                log.Error("Pas de panier valide lors de l'appel.");
                return BasketStatusEnum.InvalidState;
            }

            if (!File.Exists(filePath))
            {
                log.Error("Fichier d'impression invalide.");
                return BasketStatusEnum.InvalidState;
            }

            // 1- Création du panier
            log.Trace("Création du panier.");
            var createResult = await CreateBasketTransaction(creationPanier, webMethod, CancellationToken.None);

            if (createResult == null || createResult.Id <= 0)
            {
                log.Debug("Basket creation failed: {0}", createResult);
                return BasketStatusEnum.FailedBasketCreation;
            }

            var transac = new BasketTransactionInfo()
            {
                PointOfServiceId = creationPanier.PointOfServiceId,
                TerminalId = creationPanier.TerminalId,
                BasketSignature = createResult.Signature,
                CardPayments = new List<CardPaymentInfo>
                {
                    new CardPaymentInfo
                    {
                        Amount = 2.0f,
                        CardUid = cardUid
                    }
                }
            };
            log.Trace("Sending payment command: {0}", transac);
            var commandStatus = await PayBasketTransaction(createResult.Id, transac);

            log.Debug("Payment command result: {0}", commandStatus);

            if (commandStatus == null)
            {
                log.Warn("Payment command error. Connexion had a problem.");
                return BasketStatusEnum.InvalidRequest;
            }

            if (commandStatus.Status != BasketStatusEnum.payment_accept)
            {
                log.Warn("Payment command rejected: {0}", commandStatus);
                return commandStatus.Status;
            }

            log.Trace("Tentative d'impression.");
            // 2- Impression
            var printResult = false;
            try
            {
                printResult = PrintUtil.Print(filePath);
                log.Trace("Résultat de la tentative d'impression: {0}", printResult);
            }
            catch (Exception e)
            {
                log.Error(e, "Printing tentative could not be done. Cancelling basket.");
            }

            if (!printResult)
            {
                // L'impression n'a pas réussie, on tente d'annuler la commande.
                var transacAnnul = new BasketTransactionInfo()
                {
                    TerminalId = creationPanier.TerminalId,
                    BasketSignature = createResult.Signature
                };
                log.Trace("Sending cancel command: {0}", transacAnnul);
                var commandStatusAnnul = await CancelBasketTransaction(createResult.Id, transac);

                log.Debug("Payment cancel result: {0}", commandStatusAnnul);

                if (commandStatusAnnul == null)
                {
                    log.Warn("Payment cancel error. Connexion had a problem.");
                    return BasketStatusEnum.InvalidRequest;
                }

                if (commandStatusAnnul.Status != BasketStatusEnum.cancel)
                {
                    log.Warn("Payment cancel rejected: {0}", commandStatus);
                }

                return commandStatus.Status;
            }

            return BasketStatusEnum.payment_accept;
        }

        /// <summary>
        /// Obtient les information complète d'un individu via son identifiant de membre. Légère variation de point d'accès (user vs member).
        /// La structure de résultat est sensiblement la même.
        /// </summary>
        /// <param name="userId">L'identifiant unique de l'individu</param>
        /// <param name="webMethod">La méthode web à utiliser.</param>
        /// <param name="cToken">Jeton de cancellation optionel.</param>
        /// <returns>Les information complète de l'individu.</returns>
        public async Task<MemberIndividuInfo> GetUserIndividuInfo(string userId, Method webMethod = Method.Get, CancellationToken? cToken = null)
        {
            if (string.IsNullOrWhiteSpace(userId)) return null;

            var url = $"{LoginInfo?.RootApiUrl}/v{LoginInfo?.Version}/users/{userId.Trim()}";

            log.Trace("Request Url: {0}\nRequest body: {1}", url, null);

            //var result = await HttpRestUtil.ExecuteRestRequest<MemberIndividuInfo>(url, CommonHeaders, null, webMethod, cToken, Timeout);
            var result = await HttpRestUtil.ExecuteCompleteRestRequestAsync<MemberIndividuInfo>(cToken ?? CancellationToken.None, url);

            log.Debug("Get Member Info sent: {0}", result);

            return result.InstancedBody;
        }

        /// <summary>
        /// Obtient les information complète d'un individu via son identifiant de membre.
        /// </summary>
        /// <param name="cardUid">L'identifiant unique de l'individu</param>
        /// <param name="webMethod">La méthode web à utiliser.</param>
        /// <param name="cToken">Jeton de cancellation optionel.</param>
        /// <returns>Les information complète de l'individu.</returns>
        public async Task<BasketCardInfo> GetMemberCardInfo(string cardUid, Method webMethod = Method.Get, CancellationToken? cToken = null)
        {
            if (string.IsNullOrWhiteSpace(cardUid)) return null;

            var url = $"{LoginInfo?.RootApiUrl}/v{LoginInfo?.Version}/cashless/cards/{cardUid.Trim()}";

            log.Trace("Request Url: {0}\nRequest body: {1}", url, null);

            //var result =  await HttpRestUtil.ExecuteRestRequest<BasketCardInfo>(url, CommonHeaders, null, Method.GET, cToken, Timeout);
            var result = await HttpRestUtil.ExecuteCompleteRestRequestAsync<BasketCardInfo>(cToken ?? CancellationToken.None, url);

            log.Debug("Get Card Info sent: {0}", result);

            return result.InstancedBody;
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
        public async Task<MemberIndividuInfo> GetUserIndividuInfoFromCard(string cardUid, Method webMethod = Method.Get, CancellationToken? cToken = null)
        {
            if (string.IsNullOrWhiteSpace(cardUid)) return null;

            // Étape 1 - Obtenir l'identifiant du membre via la carte.
            var bcri = await GetMemberCardInfo(cardUid, webMethod, cToken);

            var res = await GetUserIndividuInfo(bcri?.MemberId);

            return res;
        }
    }

    public class BasketCardRequestInfo
    {
        public override string ToString()
        {
            return ClassUtil.ToStringPublicProperties(this);
        }

        /* {
               "result": true,
               "value": {
                   "ID": 63417,
                   "MEMBER_ID": null,
                   "EMISYS_NUMBER": "RAEPHK",
                   "CARD_UID": "B65B5F92",
                   "CARD_STATUS": 1,
                   "WALLETS": [],
                   "LEVEL_ACCESS": [],
                   "MEMBER": []
               }
           }
         */

        [JsonProperty(PropertyName = "result")]
        public bool Result { get; set; }
        [JsonProperty(PropertyName = "value")]
        public BasketCardInfo Value { get; set; }
    }

    public class BasketCardInfo
    {
        /*
         * {
               "ID": 63417,
               "MEMBER_ID": 1,
               "EMISYS_NUMBER": "RAEPHK",
               "CARD_UID": "B65B5F92",
               "CARD_STATUS": 0,
               "WALLETS": [],
               "LEVEL_ACCESS": [],
               "MEMBER": {
                   "ID": 1,
                   "FIRST_NAME": "Sébastien",
                   "LAST_NAME": "emisys test",
                   "EMAIL": "sebastien@leveque.eu"
               }
           }
         */

        public override string ToString()
        {
            return ClassUtil.ToStringPublicProperties(this);
        }

        [JsonProperty(PropertyName = "ID")] public int Id { get; set; }

        [JsonProperty(PropertyName = "MEMBER_ID")] public string MemberId { get; set; }

        [JsonProperty(PropertyName = "EMISYS_NUMBER")] public string EmisysNumber { get; set; }

        [JsonProperty(PropertyName = "CARD_UID")] public string CardUid { get; set; }

        [JsonProperty(PropertyName = "CARD_STATUS")] public int CardStatus { get; set; }

        // WALLETS
        // LEVEL_ACCESS
        [JsonProperty(PropertyName = "MEMBER")] public BasketCardMemberInfo Member { get; set; }
    }

    public class BasketCardMemberInfo
    {
        public override string ToString()
        {
            return ClassUtil.ToStringPublicProperties(this);
        }

        /*
         * {
               "ID": 1,
               "FIRST_NAME": "Sébastien",
               "LAST_NAME": "emisys test",
               "EMAIL": "sebastien@leveque.eu"
           }
         */

        [JsonProperty(PropertyName = "ID")] public int Id { get; set; }

        [JsonProperty(PropertyName = "FIRST_NAME")] public string FirstName { get; set; }

        [JsonProperty(PropertyName = "LAST_NAME")] public string LastName { get; set; }

        [JsonProperty(PropertyName = "EMAIL")] public string Email { get; set; }
    }
}
