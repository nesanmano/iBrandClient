using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using NLog;
using NomadLogic.Common.Serveur.Requests;
using NomadLogic.Common.Util.Networking.QuestionnaireWebRequest.ConnectAndGo.Schemas.v1;
using NomadLogic.Common.Util.Networking.QuestionnaireWebRequest.ConnectAndGo.Schemas.v1.payloads;
using static NomadLogic.Common.Serveur.Requests.ServerRestRequestResultType;
using GrantTypeEnum = NomadLogic.Common.Util.Networking.QuestionnaireWebRequest.ConnectAndGo.Schemas.v1.payloads.GrantTypeEnum;

namespace NomadLogic.Common.Util.Networking.QuestionnaireWebRequest.ConnectAndGo
{
    [PublicAPI]
    public class WebRequestConnectAndGo : IWebRequestExecutor
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        public void Dispose()
        {
            // Rien
        }

        public Task<bool> InitializeExecutor()
        {
            return Task.FromResult(true); // Stateless
        }

        public string BaseUrl { get; set; } = "https://connectngo-qa.com";

        public string VersionNumber { get; set; } = "v1";

        public int Timeout { get; set; } = 6000;

        public async Task<ServerRestRequestResult<LoginResponse>> CnGLogin(CancellationToken token, int clientId, string password, string username, 
            string clientSecret, GrantTypeEnum grant = GrantTypeEnum.password, string scope = "*", FileInfo debugFileContent = null)
        {
            if (token.IsCancellationRequested)
            {
                return new ServerRestRequestResult<LoginResponse>
                {
                    Result = Cancelled
                };
            }

            var url = $"{BaseUrl}/oauth/token";

            var body = new LoginPayload
            {
                ClientId = clientId,
                ClientSecret = clientSecret,
                Password = password,
                UserName = username,
                GrantType = grant,
                Scope = scope
            };

            var pars = new List<RestRequestAdditionalParameters>
            {
                new RestRequestAdditionalParameters(new HttpJsonBody(body))
            };

            var result = await HttpRestUtil.ExecuteCompleteRestRequestAsync<LoginResponse>(token, url, pars, null, HttpMethod.Post, debugFileContent);

            if (result.StatusCode != HttpStatusCode.OK)
            {
                return new ServerRestRequestResult<LoginResponse>
                {
                    Result = Failed,
                    MessageUi = $"Server ({BaseUrl}) authorization failed. Cause: " + result.StatusCode
                };
            }

            if (result.InstancedBody != null)
            {
                return new ServerRestRequestResult<LoginResponse>
                {
                    Result = Success,
                    Data = result.InstancedBody
                };
            }
            
            return new ServerRestRequestResult<LoginResponse>
            {
                Result = Failed,
                MessageUi = "Authorization was problematic. No bearer token found. Check credentials."
            };
        }

        public async Task<AccessControlLog> ProcessAccessScan(LoginResponse tokenInfo, ProcessAccessScanPayload parameters, CancellationToken? cToken = null, FileInfo debugFileContent = null)
        {
            if (parameters == null) return null;

            var url = $"{BaseUrl}/api/{VersionNumber}/access-control";
            var tok = cToken ?? CancellationToken.None;

            var pars = new List<RestRequestAdditionalParameters>
            {
                new RestRequestAdditionalParameters(tokenInfo.GetCompleteHeader(), "Authorization", HttpParameterType.Header)
                ,new RestRequestAdditionalParameters("application/json", "Accept", HttpParameterType.Header)
                ,new RestRequestAdditionalParameters(parameters.XtenantId, "x-tenant-id", HttpParameterType.Header)
                ,new RestRequestAdditionalParameters(parameters.GateName, "gate_name", HttpParameterType.QueryString)
                ,new RestRequestAdditionalParameters(parameters.UniqueId, "uid", HttpParameterType.QueryString)
                ,new RestRequestAdditionalParameters(parameters.AccessPointId, "access_point_id", HttpParameterType.QueryString)
                ,new RestRequestAdditionalParameters(parameters.Direction, "direction", HttpParameterType.QueryString)
            };

            ISet<HttpStatusCode> expected = new SortedSet<HttpStatusCode>
            {
                HttpStatusCode.Unauthorized,
                HttpStatusCode.Forbidden,
                // 422 requiert C# 5 (da fuq?)
            };
            var reqResponse =  await HttpRestUtil.ExecuteCompleteRestRequestAsync<AccessControlLog>(tok, url, pars, expected, HttpMethod.Post, debugFileContent);

            if (reqResponse?.InstancedBody == null)
            {
                log.Warn("Null return. Request failed.");
                return null;
            }

            var returnData = reqResponse.InstancedBody;

            if (returnData != null)
            {
                if (!log.IsDebugEnabled) return returnData;
                log.Debug($"Request details (success): \nURL={url}\nHeaders={StringUtil.FormatEnumerable(pars)}\n");
                log.Debug(reqResponse.ToStringPublicProperties(1));
                return returnData;
            }

            log.Warn("No Access Log found. Request failed.");
            if (!log.IsDebugEnabled) return null;
            log.Debug($"Request details (failiure): \nURL={url}\nHeaders={StringUtil.FormatEnumerable(pars)}\n");
            log.Debug(reqResponse.ToStringPublicProperties(1));
            return null;
        }

        public async Task<MediaResult> SendMediaInfo(LoginResponse tokenInfo, MediaPayload parameters, CancellationToken? cToken = null, FileInfo debugFileContent = null)
        {
            if (parameters == null) return null;

            var url = $"{BaseUrl}/api/{VersionNumber}/medias";
            var tok = cToken ?? CancellationToken.None;

            var pars = new List<RestRequestAdditionalParameters>
            {
                new RestRequestAdditionalParameters(tokenInfo.GetCompleteHeader(), "Authorization", HttpParameterType.Header)
                ,new RestRequestAdditionalParameters("application/json", "Accept", HttpParameterType.Header)
                ,new RestRequestAdditionalParameters(parameters.XtenantId, "x-tenant-id", HttpParameterType.Header)
                ,new RestRequestAdditionalParameters(new HttpJsonBody(parameters))
            };

            ISet<HttpStatusCode> expected = new SortedSet<HttpStatusCode>
            {
                HttpStatusCode.Unauthorized,
                HttpStatusCode.Forbidden,
                // 422 requiert C# 5 (da fuq?)
            };
            var reqResponse =  await HttpRestUtil.ExecuteCompleteRestRequestAsync<List<MediaResultItem>>(tok, url, pars, expected, HttpMethod.Post, debugFileContent);

            if (reqResponse?.InstancedBody == null)
            {
                log.Warn("Null return. Request failed.");
                return null;
            }

            var returnData = reqResponse.InstancedBody;

            if (returnData != null)
            {
                // Comme cette requête renvoi un "array" sans nom de propriété, on fait une liste d'item précis avant d'envelopper le tout
                // dans le bon format de retour.
                var trueReturn = new MediaResult
                {
                    ResultItems = returnData
                };
                if (!log.IsDebugEnabled) return trueReturn;
                log.Debug($"Request details (success): \nURL={url}\nHeaders={StringUtil.FormatEnumerable(pars)}\n");
                log.Debug(reqResponse.ToStringPublicProperties(1));
                return trueReturn;
            }

            log.Warn("No media return information found. Request failed.");
            if (!log.IsDebugEnabled) return null;
            log.Debug($"Request details (failiure): \nURL={url}\nHeaders={StringUtil.FormatEnumerable(pars)}\n");
            log.Debug(reqResponse.ToStringPublicProperties(1));
            return null;
        }


    }
}