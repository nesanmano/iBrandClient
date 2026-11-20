using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using NLog;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Serveur.Structures;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.DateAndTime;
using NomadLogic.Common.Util.Json;
using NomadLogic.Common.Util.Math;
using NomadLogic.Common.Util.Networking;
using RestSharp;
using static NomadLogic.Common.Serveur.Requests.ServerRestRequestResultType;
using static NomadLogic.Common.Serveur.RessourceFileAction;
using static NomadLogic.Common.Util.Networking.HttpParameterType;

namespace NomadLogic.Common.Serveur.Requests
{
    public enum RessourceFileComparison
    {
        FileSize,
        Timestamp,
        Both
    }

    /// <summary>
    /// Cette classe rassemble les différente méthodes de connexion pour communiquer avec le serveur principal, dit Juju.
    /// </summary>
    [PublicAPI]
    public static class ServerRestRequestsUtil
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        private static ServerRestRequestResult<T> CreateResult<T>(string message, ServerRestRequestResultType res, T data = null, Exception exp = null) where T : class
        {
            var inst = new ServerRestRequestResult<T> { MessageUi = message, Result = res, Error = exp, Data = data };
            return inst;
        }

        private static ServerRestRequestResult<bool> CreateResult(string message, ServerRestRequestResultType res, bool result = false, Exception exp = null)
        {
            var inst = new ServerRestRequestResult<bool> { MessageUi = message, Result = res, Error = exp, Data = result };
            return inst;
        }


        /// <summary>
        /// Prend un répertoire de base et liste tout son contenu ainsi que celui de ses enfants sous forme de liste de "ResourceFileToDownload", pour fins
        /// de comparaisons. La liste ne contient que des fichiers, les répertoire n'étant que des contenants de fichier.
        /// Cette méthode n'est pas asynchro comme telle mais peut fonctionner avec un jeton d'arrêt. Si cancellée, retourne une liste vide.
        /// </summary>
        /// <param name="token">Le jeton d'arrêt d'une tâche.</param>
        /// <param name="baseDir">Le répertoire de base d'où lancer la recherche.</param>
        /// <param name="flattenPath">Indique s'il faut enlever toute référence au chemin et ne garder que le nom de fichier. Vrai par défaut. Si faux, ne garde que le
        /// chemin relatif du fichier en plus de son nom, par rapport au répertoire initial.</param>
        /// <returns>Une liste non-nulle, mais peut-être vide, des instances de "RessourceFileToDownload" associées aux fichier lus.</returns>
        public static List<ResourceFileToDownload> DirectoryContentAsDownloadRessources(CancellationToken token, [NotNull] DirectoryInfo baseDir, bool flattenPath = true)
        {
            log.Trace("Parcour du répertoire pour ressources de téléchargement: {0}", baseDir.FullName);

            var queue = new Queue<FileSystemInfo>(baseDir.EnumerateFileSystemInfos());

            var foundRessources = new List<ResourceFileToDownload>();
            var baseDirStr = baseDir.FullName;
            var nbrChar = baseDirStr.Length + 1; // À cause du caractère "\" supplémentaire

            while (queue.Count > 0 && !token.IsCancellationRequested)
            {
                var current = queue.Dequeue();
                switch (current)
                {
                    case DirectoryInfo _:
                        {
                            var newDir = new DirectoryInfo(current.FullName);
                            foreach (var fsi in newDir.EnumerateFileSystemInfos())
                            {
                                queue.Enqueue(fsi);
                            }
                            break;
                        }
                    case FileInfo f:
                        {
                            var rfd = new ResourceFileToDownload
                            {
                                FileSize = f.Length
                            };

                            if (flattenPath)
                            {
                                rfd.Src = f.Name;
                            }
                            else if (f.FullName.StartsWith(baseDirStr))
                            {
                                rfd.Src = f.FullName.Substring(nbrChar);
                            }

                            rfd.ServerModifiedTime = DateUtil.GetJavaTimeFromDateTime(f.LastWriteTimeUtc);
                            foundRessources.Add(rfd);
                            break;
                        }
                }
            }

            if (token.IsCancellationRequested)
            {
                log.Debug("Parcour de répertoire annulé.");
                return new List<ResourceFileToDownload>();
            }

            log.Debug("Nombres de fichiers de téléchargement trouvés: {0}", foundRessources.Count);

            return foundRessources;
        }

        /// <summary>
        /// Extrapole une liste d'éléments avec actions appropriée pour modifier des éléments pour les rendre similaires.
        /// En gros, si on exécute les actions de la liste générée, on "transforme" la liste courante pour qu'elle soit égale à la liste maître.
        /// Individuellement:
        /// 1- Si un élément existe dans la liste maître mais pas dans la liste courante, cette élément existera dans la liste en sortie avec l'action "ADD".
        /// 2- Si un élément n'existe pas dans la liste maître, mais existe dans la liste courante, cet élément sera ajouté dans la liste en sortie avec l'action "DELETE"
        /// 3- Si un élément existe dans les deux, et est égal (selon l'option de comparaison choisie), cet élément sera ajouté (basé sur le maître), avec l'action "IGNORE"
        /// 3b- Si existe dans les deux, mais non-égal, sera ajouté en sortie, copie basée sur le maître, avec l'action "ADD".
        /// </summary>
        /// <param name="masterList">Liste provenant du serveur.</param>
        /// <param name="currentList">Liste locale.</param>
        /// <param name="comparison">Le type de comparaison voule.</param>
        /// <returns>Une nouvelle liste, qui contient les éléments et les actions appropriées.</returns>
        public static IList<ResourceFileToDownload> CompareDownloadList([NotNull] IList<ResourceFileToDownload> masterList, [NotNull] IList<ResourceFileToDownload> currentList, RessourceFileComparison comparison)
        {
            var resultList = new List<ResourceFileToDownload>();

            if (!masterList.Any()) return resultList;

            masterList.Sort((x, y) => Comparer<string>.Default.Compare(x.Src, y.Src));
            currentList.Sort((x, y) => Comparer<string>.Default.Compare(x.Src, y.Src));

            if (log.IsTraceEnabled)
            {

                var sb = new StringBuilder();
                sb.Append("Initial (Sorted) lists of files to download:");
                sb.Append($"\nMaster: ({masterList.Count} total)");
                foreach (var s in masterList)
                {
                    sb.Append($"\n\t- '{s.Src}' t={s.ServerModifiedTime} s={MathUtil.ByteNumberReadable(s.FileSize)}, a={s.Action}");
                }
                sb.Append($"\nCurrent: ({currentList.Count} total)");
                foreach (var s in currentList)
                {
                    sb.Append($"\n\t- '{s.Src}' t={s.ServerModifiedTime} s={MathUtil.ByteNumberReadable(s.FileSize)}, a={s.Action}");
                }

                sb.AppendLine();
                log.Trace(sb.ToString);
            }

            // Pour chaque élément de la liste du serveur qui n'existe pas dans la liste locale
            var maxServeurIndex = masterList.Count - 1;
            var maxLocalIndex = currentList.Count - 1;
            var sIndex = 0;
            var lIndex = 0;
            while (sIndex <= maxServeurIndex)
            {
                if (lIndex > maxLocalIndex)
                {
                    // Plus de fichiers dans la liste locale. On ajoute tous les fichier serveurs dans la liste résultante
                    var curr = masterList[sIndex].Copy();
                    curr.Action = Add;
                    log.Trace($"{curr.Action} '{curr}': local file list exhausted.");
                    resultList.Add(curr);
                    sIndex++;
                    continue;
                }

                var currentMaster = masterList[sIndex];
                var currentLocal = currentList[lIndex];

                // Conversion des chemins pour éliminer les différences de plateformes UNIX/Windows
                // "test/server.txt" devient "test\server.txt"
                var sFi = Path.Combine(string.Empty, currentMaster.Src);
                var lFi = Path.Combine(string.Empty, currentLocal.Src);

                // Windows ignore la case, donc "server.txt" et "Server.TXT" sont égaux.
                var comp = string.Compare(sFi, lFi, StringComparison.OrdinalIgnoreCase);

                log.Trace("(Comparing '{0}' and '{1}' => {2})", sFi, lFi, comp);

                if (comp == 0)
                {
                    var sT = currentMaster.ServerModifiedTime;
                    var lT = currentLocal.ServerModifiedTime;

                    var sF = currentMaster.FileSize;
                    var lF = currentLocal.FileSize;

                    bool check;
                    string message;

                    switch (comparison)
                    {
                        case RessourceFileComparison.FileSize:
                            check = sF != lF;
                            message = "{5} '{0}': has different file size ({3}/{4}).";
                            break;
                        case RessourceFileComparison.Timestamp:
                            check = sT != lT;
                            message = "{5} '{0}': has different timestamp ({1}/{2}).";
                            break;
                        case RessourceFileComparison.Both:
                            check = sT != lT || sF != lF;
                            message = "{5} '{0}': has different timestamp ({1}/{2}) or file size ({3}/{4}).";
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(comparison), comparison, null);
                    }

                    if (check)
                    {
                        var curr = currentMaster.Copy();
                        curr.Action = ResourceFileToDownload.PrioritizeAction(Update, currentLocal.Action);
                        log.Trace(message, curr.Src, sT, lT, sF, lF, curr.Action);
                        resultList.Add(curr);
                    }
                    else
                    {
                        var curr = currentMaster.Copy();
                        curr.Action = ResourceFileToDownload.PrioritizeAction(Ignore, currentLocal.Action);
                        log.Trace($"{curr.Action} '{curr.Src}': file is present and equal.");
                        resultList.Add(curr);
                    }

                    // Correspondance exacte: on pase au prochain dans les deux cas.
                    sIndex++;
                    lIndex++;
                    continue;
                }

                if (comp < 0)
                {
                    // N'existe pas localement.
                    var curr = masterList[sIndex].Copy();
                    curr.Action = Add;
                    log.Trace($"{curr.Action} '{curr.Src}': Does not exists in current list.");
                    resultList.Add(curr);
                    sIndex++;
                    continue;
                }

                // Si plus grand que zéro, c'est un fichier local "superflu" qui n'existe pas dans la liste maître.
                var sup = currentList[sIndex].Copy();
                sup.Action = Remove; // Quelque soit l'ancienne action, il faut enlever ce fichier.
                log.Trace($"({sup.Action} '{sup.Src}': no matching file from master list.)");
                resultList.Add(sup);
                lIndex++;
            }

            // S'il reste de fichiers dans la liste courante.
            while (lIndex <= maxLocalIndex)
            {
                var sup = currentList[sIndex].Copy();
                sup.Action = Remove; // Quelque soit l'ancienne action, il faut enlever ce fichier.
                log.Trace($"({sup.Action} '{sup.Src}': no matching file from master list.)");
                resultList.Add(sup);
                lIndex++;
            }

            if (log.IsTraceEnabled)
            {

                var sb = new StringBuilder();
                sb.Append($"Final list for differences: ({resultList.Count} total)");
                foreach (var s in resultList)
                {
                    sb.Append($"\n\t- '{s.Src}' t={s.ServerModifiedTime} s={MathUtil.ByteNumberReadable(s.FileSize)}, a={s.Action}");
                }

                sb.AppendLine();
                log.Trace(sb.ToString);
            }

            return resultList;
        }

        /// <summary>
        /// Tente d'obtenir une authorisation du serveur, sous forme d'authentification.
        /// </summary>
        /// <param name="serConf">La configuration server.</param>
        /// <param name="token">Le jeton de cancellation.</param>
        /// <returns>Le résultat de la requête.</returns>
        public static async Task<ServerRestRequestResult<string>> Login(ServerConfiguration serConf, CancellationToken token)
        {
            const string AUTHORIZATION = "Authorization";

            if (token.IsCancellationRequested)
            {
                return new ServerRestRequestResult<string>
                {
                    Result = Cancelled
                };
            }

            var url = serConf.GetRestUrl("/login");

            var pars = new List<RestRequestAdditionalParameters>
            {
                new RestRequestAdditionalParameters(serConf.Identifier)
            };

            var result = await HttpRestUtil.ExecuteCompleteRestRequestAsync<RestResponse>(token, url, pars, null, HttpMethod.Post);

            if (result.StatusCode != HttpStatusCode.OK && result.Headers == null)
            {
                return new ServerRestRequestResult<string>
                {
                    Result = Failed,
                    MessageUi = $"Server ({url}) is unreachable. Check internet connection."
                };
            }

            foreach (var param in result.Headers)
            {
                if (!param.Key.Equals(AUTHORIZATION)) continue;

                var tok = param.Value.First();

                return new ServerRestRequestResult<string>
                {
                    Result = Success,
                    MessageUi = "Connected to main server.",
                    Data = tok
                };
            }

            return new ServerRestRequestResult<string>
            {
                Result = Failed,
                MessageUi = "Authorization was denied. Check credentials."
            };
        }

        /// <summary>
        /// Tente d'obtenir une authorisation du serveur, sous forme d'authentification.
        /// ATTENTION: C'est une version synchrone, à utiliser dans les circonstance où un "hang-up" dû au système async de C# cause des problèmes.
        /// </summary>
        /// <param name="serConf">La configuration server.</param>
        /// <returns>Le résultat de la requête.</returns>
        public static async Task<ServerRestRequestResult<string>> LoginSync(ServerConfiguration serConf)
        {
            const string AUTHORIZATION = "Authorization";

            var url = serConf.GetRestUrl("/login");

            var pars = new List<RestRequestAdditionalParameters>
            {
                new RestRequestAdditionalParameters(serConf.Identifier)
            };

            var result = await HttpRestUtil.ExecuteCompleteRestRequest<RestResponse>(url, pars, null, HttpMethod.Post).ConfigureAwait(false);

            if (result.StatusCode != HttpStatusCode.OK && result.Headers == null)
            {
                return new ServerRestRequestResult<string>
                {
                    Result = Failed,
                    MessageUi = $"Server ({url}) is unreachable. Check internet connection."
                };
            }

            foreach (var param in result.Headers)
            {
                if (!param.Key.Equals(AUTHORIZATION)) continue;

                var tok = param.Value.First();

                return new ServerRestRequestResult<string>
                {
                    Result = Success,
                    MessageUi = "Connected to main server.",
                    Data = tok
                };
            }

            return new ServerRestRequestResult<string>
            {
                Result = Failed,
                MessageUi = "Authorization was denied. Check credentials."
            };
        }

        public static async Task<ServerRestRequestResult<RequestSyncResponse>> Sync(CancellationToken cToken,
            [NotNull] ServerConfiguration serConf, [NotNull] string authToken,
            [NotNull] RequestSyncRespondent resp, [NotNull] TerminalInfo tdi, bool isMock = false)
        {
            var url = serConf.GetRestUrl("sync");

            var payload = new RequestSyncPayload
            {
                Respondent = resp,
                ClientMachine = serConf.LocalPPC,
                ProjectId = tdi.ProjectId,
                QuestionnaireId = tdi.QuestionnaireId,
                TerminalId = tdi.TerminalId
            };

            log.Trace("Sending request: " + url);

            var addReq = new List<RestRequestAdditionalParameters>
            {
                new RestRequestAdditionalParameters
                {
                    Type = Header,
                    Name = "mock",
                    Value = isMock
                },
                new RestRequestAdditionalParameters
                {
                    Type = Header,
                    Name = "Authorization",
                    Value = authToken
                },
                new RestRequestAdditionalParameters(new HttpJsonBody(payload))
            };

            var extraCodes = new HashSet<HttpStatusCode>
            {
                HttpStatusCode.PartialContent,
                HttpStatusCode.Unauthorized,
                HttpStatusCode.BadRequest
            };

            try
            {

                 var result = await HttpRestUtil.ExecuteCompleteRestRequestAsync<RequestSyncResponse>(cToken, url, addReq, extraCodes, HttpMethod.Post).ConfigureAwait(false);

                //var result = await SyncRestClient.ExecuteAsync<RequestSyncResponse>(cToken,url,addReq,extraCodes,HttpMethod.Post).ConfigureAwait(false);


                switch (result.StatusCode)
                {
                    case HttpStatusCode.OK:

                        // Vérification du contenu pour empêcher des bugs:
                        if (string.IsNullOrWhiteSpace(result.InstancedBody.SyncSessionId))
                        {
                            return CreateResult<RequestSyncResponse>("200 code with no session id. Cannot continue.", Error);
                        }

                        if (!result.InstancedBody.FilesToUpload?.Any() ?? true)
                        {
                            return CreateResult<RequestSyncResponse>("200 code with null or empty file upload list. Cannot continue.", Error);
                        }

                        return CreateResult("Sync session created. remaining steps detected.", SuccessPartial, result.InstancedBody);
                    case HttpStatusCode.Created:

                        // Vérification du contenu pour empêcher des bugs:
                        if (string.IsNullOrWhiteSpace(result.InstancedBody.RespondentCreationDateStr))
                        {
                            return CreateResult<RequestSyncResponse>("201 code with no respondent creation date. Cannot continue.", Error);
                        }

                        if (result.InstancedBody.RespondentServerId == 0)
                        {
                            return CreateResult<RequestSyncResponse>("201 code with default (0) server respondent ID. Cannot continue.", Error);
                        }

                        return CreateResult("Sync session ended successfully.", Success, result.InstancedBody);
                    case HttpStatusCode.PartialContent:
                        if (string.IsNullOrWhiteSpace(result.InstancedBody.SyncSessionId))
                        {
                            return CreateResult<RequestSyncResponse>("Kugan 200 code with no session id. Cannot continue.", Error);
                        }

                        if (!result.InstancedBody.FilesToUpload?.Any() ?? true)
                        {
                            return CreateResult<RequestSyncResponse>("Kugan 200 code with null or empty file upload list. Cannot continue.", Error);
                        }

                        return CreateResult("Sync session created. remaining steps detected.", SuccessPartial, result.InstancedBody);
                    case HttpStatusCode.Unauthorized:
                        return CreateResult<RequestSyncResponse>("New authorization token needed.", Unauthorized);
                    case HttpStatusCode.BadRequest:
                        return CreateResult<RequestSyncResponse>("Invalid payload.", Failed);
                    default:
                        return CreateResult<RequestSyncResponse>("Unknown request result detected.", Error);
                }
            }
            catch (Exception e)
            {
                return CreateResult<RequestSyncResponse>("Exception thrown during SYNC request.", Error, null, e);
            }
        }

        /// <summary>
        /// Cet appel est une version pour fin de débogage seulement.
        /// On retourne le contenu du payload tel que reçu par le serveur, mais le reste de son comportement est le même
        /// que SYNC, c'est à dire qu'il a besoin d'un jeton de session et de paramètre valides.
        /// N'a pas de version "mock" (of course).
        /// </summary>
        public static ServerRestRequestResult<RequestSyncPayload> DebugSync(
            [NotNull] ServerConfiguration serConf, [NotNull] string authToken,
            [NotNull] RequestSyncRespondent resp, [NotNull] TerminalInfo tdi)
        {
            var url = serConf.GetRestUrl("debug-sync");

            var payload = new RequestSyncPayload
            {
                Respondent = resp,
                ClientMachine = serConf.LocalPPC,
                ProjectId = tdi.ProjectId,
                QuestionnaireId = tdi.QuestionnaireId,
                TerminalId = tdi.TerminalId
            };

            log.Trace("Sending request: " + url);

            var addReq = new List<RestRequestAdditionalParameters>
            {
                new RestRequestAdditionalParameters
                {
                    Type = Header,
                    Name = "Authorization",
                    Value = authToken
                },
                new RestRequestAdditionalParameters(new HttpJsonBody(payload))
            };

            var extraCodes = new HashSet<HttpStatusCode>
            {
                HttpStatusCode.PartialContent,
                HttpStatusCode.Unauthorized,
                HttpStatusCode.BadRequest
            };

            try
            {
                var result = HttpRestUtil.ExecuteSimpleRequest<RequestSyncPayload>(url, addReq, extraCodes, HttpMethod.Post);
                return CreateResult("Debug-Sync called.", Success, result);
            }
            catch (Exception e)
            {
                return CreateResult<RequestSyncPayload>("Exception thrown during DEBUG-SYNC request.", Error, null, e);
            }
        }


        public static async Task<ServerRestRequestResult<object>> SyncUpload(ServerConfiguration conf, string authToken, string sessionId, string filePath, string answerIdentifier)
        {
            var url = conf.GetRestUrl("sync-upload");

            log.Trace("Sending request: " + url);

            var pars = new List<RestRequestAdditionalParameters>
            {
                new RestRequestAdditionalParameters(authToken, "Authorization", Header),
                new RestRequestAdditionalParameters(sessionId, "Session-id", Header),
                new RestRequestAdditionalParameters(answerIdentifier, "Question-id", Header),
            };

            var extraCodes = new HashSet<HttpStatusCode>
            {
                HttpStatusCode.Unauthorized,
                HttpStatusCode.BadRequest
            };

            try
            {
                var result = await HttpRestUtil.ExecuteUploadRequest<object>(new FileInfo(filePath), url, pars, extraCodes).ConfigureAwait(false);

                switch (result.StatusCode)
                {
                    case HttpStatusCode.OK:
                        return CreateResult<object>("Sync upload successful.", Success);
                    default:
                        return CreateResult<object>("Unknow request result detected.", Error);
                }
            }
            catch (Exception e)
            {
                return CreateResult<object>("Exception thrown during SYNC_UPLOAD request.", Error, null, e);
            }
        }

        public static async Task<ServerRestRequestResult<RequestSyncResponse>> SyncFinalize(ServerConfiguration conf, string authToken, string sessionId)
        {
            var url = conf.GetRestUrl("sync-finalize");

            log.Trace("Sending request: " + url);
            var pars = new List<RestRequestAdditionalParameters>
            {
                new RestRequestAdditionalParameters(authToken, "Authorization", Header),
                new RestRequestAdditionalParameters(sessionId, "Session-id", Header)
            };

            var extraCodes = new HashSet<HttpStatusCode>
            {
                HttpStatusCode.PartialContent,
                HttpStatusCode.Unauthorized,
                HttpStatusCode.BadRequest
            };

            try
            {
                var result = await HttpRestUtil.ExecuteCompleteRestRequest<RequestSyncResponse>(url, pars, extraCodes).ConfigureAwait(false); ;

                switch (result.StatusCode)
                {
                    case HttpStatusCode.NotAcceptable:
                        return CreateResult("Sync session not found: " + result.ErrorRawBody, NoContent, result.InstancedBody);
                    case HttpStatusCode.PartialContent:
                        return CreateResult("Sync session remain open. Remaining steps detected.", SuccessPartial, result.InstancedBody);
                    case HttpStatusCode.Created:
                        return CreateResult("Sync session ended successfully.", Success, result.InstancedBody);
                    case HttpStatusCode.Unauthorized:
                        return CreateResult<RequestSyncResponse>("New authorization token needed.", Unauthorized);
                    case HttpStatusCode.BadRequest:
                        return CreateResult<RequestSyncResponse>("Invalid payload.", Failed);
                    default:
                        return CreateResult<RequestSyncResponse>("Unknow request result detected.", Error);
                }
            }
            catch (Exception e)
            {
                return CreateResult<RequestSyncResponse>("Exception thrown during SYNC-FINALIZE request.", Error, null, e);
            }
        }

        public static async Task<ServerRestRequestResult<object>> SyncCancel(ServerConfiguration conf, string authToken, string sessionId)
        {
            var url = conf.GetRestUrl("sync-cancel");

            log.Trace("Sending request: " + url);
            var pars = new List<RestRequestAdditionalParameters>
            {
                new RestRequestAdditionalParameters(authToken, "Authorization", Header),
                new RestRequestAdditionalParameters(sessionId, "Session-id", Header)
            };

            var extraCodes = new HashSet<HttpStatusCode>
            {
                HttpStatusCode.Unauthorized,
                HttpStatusCode.BadRequest
            };

            try
            {
                var result = await HttpRestUtil.ExecuteCompleteRestRequest<RequestSyncResponse>(url, pars, extraCodes, HttpMethod.Delete).ConfigureAwait(false);

                switch (result.StatusCode)
                {
                    case HttpStatusCode.OK:
                        return CreateResult<object>("Sync cancellation accepted.", Success);
                    case HttpStatusCode.Unauthorized:
                        return CreateResult<object>("Authorisation refused or session does not exists.", Unauthorized);
                    case HttpStatusCode.BadRequest:
                        return CreateResult<object>("Invalid payload.", Failed);
                    default:
                        return CreateResult<object>("Unknow request result detected.", Error);
                }
            }
            catch (Exception e)
            {
                return CreateResult<object>("Exception thrown during SYNC_CANCEL request.", Error, null, e);
            }
        }

        public static async Task<ServerRestRequestResult<TerminalDownloadInfo>> DownloadProjectInfo(string jwtToken,
            CancellationToken ct, ServerConfiguration conf, string terminalUsername, string terminalPassword)
        {
            if (ct.IsCancellationRequested)
            {
                return new ServerRestRequestResult<TerminalDownloadInfo>
                {
                    Result = Cancelled
                };
            }

            var url = conf.GetRestUrl("downloadProjectInfo");

            log.Trace("Sending request: " + url);

            var extraCodes = new HashSet<HttpStatusCode>
            {
                HttpStatusCode.Unauthorized,
                HttpStatusCode.BadRequest,
                HttpStatusCode.NoContent
            };
            
            var pars = new List<RestRequestAdditionalParameters>
            {
               // new RestRequestAdditionalParameters(authToken, "Authorization", Header),
                new RestRequestAdditionalParameters
                {
                    Type = Header,
                    Name = "Authorization",
                    Value = jwtToken
                },
                new RestRequestAdditionalParameters
                {
                    Type = QueryString,
                    Name = "username",
                    Value = terminalUsername
                },
                new RestRequestAdditionalParameters
                {
                    Type = QueryString,
                    Name = "password",
                    Value = terminalPassword
                },
                new RestRequestAdditionalParameters
                {
                    Type = QueryString,
                    Name = "terminalType",
                    Value = "Windows"
                }
            };

            try
            {
                var result = await HttpRestUtil.ExecuteCompleteRestRequestAsync<TerminalDownloadInfo>(ct, url, pars, extraCodes);

                switch (result.StatusCode)
                {
                    case HttpStatusCode.OK:
                        if (result.InstancedBody != null)
                            return CreateResult("Terminal info downloaded.", Success, result.InstancedBody);
                        log.Error("'Successful' project info download has null body. Server-side error detected.");
                        return CreateResult<TerminalDownloadInfo>("Request success but with null body..", Failed);
                    case HttpStatusCode.Unauthorized:
                        return CreateResult<TerminalDownloadInfo>("Terminal user/pass does not exists", Unauthorized);
                    case HttpStatusCode.BadRequest:
                        return CreateResult<TerminalDownloadInfo>("Invalid payload.", Failed);
                    case HttpStatusCode.NoContent:
                        return CreateResult<TerminalDownloadInfo>("No terminal data associated with user/password.", NoContent);
                    default:
                        return CreateResult<TerminalDownloadInfo>("Unknow request result detected.", Error);
                }
            }
            catch (Exception e)
            {
                return CreateResult<TerminalDownloadInfo>("Exception thrown during DOWN_PROJECT_INFO request.", Error, null, e);
            }
        }

        public static async Task<ServerRestRequestResult<bool>> DownloadTerminalFile(String jwtToken,
            CancellationToken token, [NotNull] ServerConfiguration conf, [NotNull] TerminalInfo ti,
            [NotNull] ResourceFileToDownload fic, FileInfo outFile)
        {
            if (token.IsCancellationRequested)
            {
                return new ServerRestRequestResult<bool>
                {
                    Result = Cancelled,
                    Data = false
                };
            }

            var url = conf.GetRestUrl("downloadProject");

            log.Trace("Sending request: " + url);

            var extraCodes = new HashSet<HttpStatusCode>
            {
                HttpStatusCode.Unauthorized,
                HttpStatusCode.BadRequest
            };
           


            var addParams = new List<RestRequestAdditionalParameters>
            {
               // new RestRequestAdditionalParameters(authToken, "Authorization", Header),
                new RestRequestAdditionalParameters
                {
                    Type = Header,
                    Name = "Authorization",
                    Value = jwtToken
                },

                new RestRequestAdditionalParameters
                {
                    Type = QueryString,
                    Name = "projectID",
                    Value = ti.ProjectId
                },
                new RestRequestAdditionalParameters
                {
                    Type = QueryString,
                    Name = "questionnaireID",
                    Value = ti.QuestionnaireId
                },
                new RestRequestAdditionalParameters
                {
                    Type = QueryString,
                    Name = "terminalID",
                    Value = ti.TerminalId
                },new RestRequestAdditionalParameters
                {
                    Type = QueryString,
                    Name = "clientTypeID",
                    Value = 1
                },new RestRequestAdditionalParameters
                {
                    Type = QueryString,
                    Name = "fileName",
                    Value = fic.Src
                }
            };

            try
            {
                var resp = await HttpRestUtil.ExecuteDownLoadRequestAsync(token, outFile, url, addParams, extraCodes);

                switch (resp.StatusCode)
                {
                    case HttpStatusCode.OK:
                        return CreateResult("Terminal info downloaded.", Success, true);
                    case HttpStatusCode.Unauthorized:
                        return CreateResult("Terminal user/pass does not exists", Unauthorized);
                    case HttpStatusCode.BadRequest:
                        return CreateResult("Invalid payload.", Failed);
                    default:
                        return CreateResult($"Unknow request result detected for file `{fic.Src}`: code={resp.StatusCode}", Error);
                }
            }
            catch (Exception e)
            {
                return CreateResult("Exception thrown during DOWNLOAD_TERMINAL_FILE request.", Error, false, e);
            }
        }

        public static async Task<ServerRestRequestResult<RequestQuestionnaireDataResponse>> GetQuestionnaireData(
            CancellationToken ct, [NotNull] ServerConfiguration conf, [NotNull] string authToken, [NotNull] RequestQuestionnaireDataPayload parameters, bool mockRequest = false)
        {
            if (ct.IsCancellationRequested)
            {
                return new ServerRestRequestResult<RequestQuestionnaireDataResponse>
                {
                    Result = Cancelled
                };
            }

            var url = conf.GetRestUrl("questionnaire-data");

            log.Trace("Sending request: " + url);

            var addReq = new List<RestRequestAdditionalParameters>
            {
                new RestRequestAdditionalParameters(new HttpJsonBody(parameters)),
                new RestRequestAdditionalParameters(authToken, "Authorization", Header),
                new RestRequestAdditionalParameters
                {
                    Type = Header,
                    Name = "mock",
                    Value = mockRequest
                }
            };

            var extraCodes = new HashSet<HttpStatusCode>
            {
                HttpStatusCode.Unauthorized,
                HttpStatusCode.BadRequest
            };

            try
            {
                var result = await HttpRestUtil.ExecuteCompleteRestRequestAsync<RequestQuestionnaireDataResponse>(ct, url, addReq, extraCodes, HttpMethod.Post);

                switch (result.StatusCode)
                {
                    case HttpStatusCode.OK:
                        return CreateResult("Data request accepted.", Success, result.InstancedBody);
                    case HttpStatusCode.Unauthorized:
                        return CreateResult<RequestQuestionnaireDataResponse>("New authorization token needed.", Unauthorized);
                    case HttpStatusCode.BadRequest:
                        return CreateResult<RequestQuestionnaireDataResponse>("Invalid payload.", Failed);
                    default:
                        return CreateResult<RequestQuestionnaireDataResponse>("Unknow request result detected.", Error);
                }
            }
            catch (Exception e)
            {
                return CreateResult<RequestQuestionnaireDataResponse>("Exception thrown during GET_QUESTIONNAIRE_DATA request.", Error, null, e);
            }
        }

        /// <summary>
        /// Cette requête web demande au end-point "get-respondent-data" les informations sur un répondant unique.
        /// Voir: https://nomadlogic.atlassian.net/wiki/spaces/DEV/pages/1649901596/get-respondent-data
        /// </summary>
        /// <param name="ct">Jeton de cancellation async</param>
        /// <param name="conf">Configfuration d'accès serveur.</param>
        /// <param name="authToken">Jeton d'authentification du serveur.</param>
        /// <param name="parameters">Les paramètres de la requête.</param>
        /// <param name="mockRequest">Indique s'il faut "mocker" la requête.</param>
        /// <param name="debugFile">Au besoin, un fichier de débogage pour stocker le contenu retourné par la requête.</param>
        /// <returns>Le résultat de la requête REST, ainsi que les données si applicables.</returns>
        public static async Task<ServerRestRequestResult<RequestGetRespondentDataResult>> GetRespondentData(
            CancellationToken ct, [NotNull] ServerConfiguration conf, [NotNull] string authToken, [NotNull] RequestGetRespondentDataPayload parameters,
            bool mockRequest = false, FileInfo debugFile = null)
        {
            if (ct.IsCancellationRequested)
            {
                return new ServerRestRequestResult<RequestGetRespondentDataResult>
                {
                    Result = Cancelled
                };
            }

            var url = conf.GetRestUrl("get-respondent-data");

            log.Trace("Sending request: " + url);

            var addReq = new List<RestRequestAdditionalParameters>
            {
                new RestRequestAdditionalParameters(new HttpJsonBody(parameters)),
                new RestRequestAdditionalParameters(authToken, "Authorization", Header),
                new RestRequestAdditionalParameters
                {
                    Type = Header,
                    Name = "mock",
                    Value = mockRequest
                }
            };

            var extraCodes = new HashSet<HttpStatusCode>
            {
                HttpStatusCode.NoContent,
                HttpStatusCode.Unauthorized,
                HttpStatusCode.BadRequest
            };

            try
            {
                var result = await HttpRestUtil.ExecuteCompleteRestRequestAsync<RequestGetRespondentDataResult>(ct, url, addReq, extraCodes, HttpMethod.Post, debugFile);

                switch (result.StatusCode)
                {
                    case HttpStatusCode.OK:
                        return CreateResult("Data request accepted.", Success, result.InstancedBody);
                    case HttpStatusCode.NoContent:
                        return CreateResult("Data request accepted. But no content found", Success, result.InstancedBody);
                    case HttpStatusCode.Unauthorized:
                        return CreateResult<RequestGetRespondentDataResult>("New authorization token needed.", Unauthorized);
                    case HttpStatusCode.BadRequest:
                        return CreateResult<RequestGetRespondentDataResult>("Invalid payload.", Failed);
                    default:
                        return CreateResult<RequestGetRespondentDataResult>("Unknow request result detected.", Error);
                }
            }
            catch (Exception e)
            {
                return CreateResult<RequestGetRespondentDataResult>("Exception thrown during GET_RESPONDENT_DATA request.", Error, null, e);
            }
        }

        /// <summary>
        /// Méthode utilitaire standard pour préparer un objet de charge pour un répondant vers le serveur.
        /// </summary>
        public static RequestSendRespondentDataPayload PrepareRespondentSendRequestPayload(IRespondent respondent, bool skipAnswers = false, bool skipDynamicHeaders = false)
        {
            var indexRefFichiers = 0;
            var answerList = new List<RequestSendRespondentDataAnswer>();
            var dicoFiles = new Dictionary<string, FileInfo>();
            var dynamicHeaders = new List<RequestSendRespondentDataDynamicValue>();
            
            // Marshalling des réponses de questionnaire.
            if (!skipAnswers)
            {
                foreach (var ans in respondent.Answers)
                {
                    var stringData = ans.Response;

                    // Si type de fichier, on ajoute au dictionnaire de fichiers.
                    if (ans.QuestionType.IsFileType())
                    {
                        var filePath = ans.Response;
                        var fileTag = $"file{++indexRefFichiers}";
                        dicoFiles.Add(fileTag, new FileInfo(filePath));
                        // On remplace la référence de la réponse vers le fichier local pour le remplacer avec une référence vers le dictionnaire de fichier de la requête.
                        stringData = RequestSendRespondentDataConfig.FILE_REFERENCE_PREFIX + fileTag;
                    }

                    var rrda = new RequestSendRespondentDataAnswer
                    {
                        DontOverrideData = true,
                        QuestionNumber = ans.QuestionNo,
                        ChoiceNumber = ans.ChoiceNo,
                        TextValue = stringData
                    };

                    answerList.Add(rrda);
                }
            }

            // Marshalling des valeurs dynamiques.
            if (!skipDynamicHeaders)
            {
                foreach (var dh in respondent.DynamicHeaders)
                {
                    var data = new RequestSendRespondentDataDynamicValue
                    {
                        DataKey = dh.DataKey,
                        DateCreated = dh.DateCreated
                    };
                    var dataValues = new List<string>();
                    foreach (var dhv in dh.DataValues)
                    {
                        if (dhv.TypeMetadata == DataValueType.FileLocal)
                        {
                            // Si c'est un fichier local, il faut ajouter le dictionnaire et utiliser un préfixe spécial pour cette requête web.
                            var filePath = dhv.ValueRaw;
                            var fileTag = $"file{++indexRefFichiers}";
                            dicoFiles.Add(fileTag, new FileInfo(filePath));

                            dataValues.Add("ref:" + fileTag);
                        }
                        else
                        {
                            dataValues.Add(dhv.ToString()); // Ajoute automatiquement les bons préfixes.
                        }
                    }

                    data.DataValues = dataValues;

                    dynamicHeaders.Add(data);
                }
            }

            // On construit le payload final.
            var payload = new RequestSendRespondentDataPayload
            {
                RequestConfig = new RequestSendRespondentDataConfig
                {
                    SendRespondentIdentification = new RequestSendRespondentDataIdentification
                    {
                        SearchStrategy = new RequestSendRespondentDataIdentificationStrategy
                        {
                            SearchStrategy = RequestRespondentDataIdentificationStrategyEnum.ExternalIdentifier,
                            TextValue = respondent.ExternalUniqueIdentifier,
                            ServerIdQuestionnaire = respondent.QuestionnaireId,
                            ServerIdTerminal = respondent.TerminalId
                        }
                    }
                    ,AnswersToSend = answerList
                    ,DynamicValuesToSend = dynamicHeaders
                },
                RequestFiles = dicoFiles
            };

            return payload;
        }

        /// <summary>
        /// Envoi une requête d'ajouter ou remplacer les données d'un répondant spécifique. On tente avant d'identifier un répondant de façon unique, puis on ajoute
        /// les données spécifiées dans la configuration.
        /// On peut également envoyer des fichiers simultanément avec cette requête.
        /// </summary>
        /// <param name="ct">Le jeton de cancellation.</param>
        /// <param name="conf">La configuration du serveur vers lequel envoyer les données.</param>
        /// <param name="authToken">Le jeton d'authentification vers le serveur.</param>
        /// <param name="parameters">Les paramètres de configuration, ainsi que les données à envoyer. Et les fichiers au besoin. </param>
        /// <param name="mockRequest">Indique si la requête est un "mock".</param>
        /// <returns>Le résultat de la requête web.</returns>
        public static async Task<ServerRestRequestResult<RequestSendRespondentDataResult>> SendRespondentData(CancellationToken ct, [NotNull] ServerConfiguration conf, 
            [NotNull] string authToken, [NotNull] RequestSendRespondentDataPayload parameters, bool mockRequest = false, FileInfo debugFile = null)
        {
            if (ct.IsCancellationRequested)
            {
                return new ServerRestRequestResult<RequestSendRespondentDataResult>
                {
                    Result = Cancelled
                };
            }

            if (parameters.RequestConfig == null)
            {
                log.Warn("Request rejected: NULL configuration object.");
                return new ServerRestRequestResult<RequestSendRespondentDataResult>
                {
                    Result = Error
                };
            }

            // Vérifie que l'on a une dictionnaire avec au moins le même nombre de fichiers définis que les références de fichiers trouvées.
            // On teste juste le nombre ici.
            var fileRefs = parameters.RequestConfig.GetFileReferences();
            var fileDico = parameters.RequestFiles;
            if (fileRefs.Any() && (fileDico == null || fileDico.Count != fileRefs.Count))
            {
                log.Warn("Request rejected: No files or not enough of them described by the request configuration.");
                return new ServerRestRequestResult<RequestSendRespondentDataResult>
                {
                    Result = Error
                };
            }

            if (fileDico != null)
            {
                // Vérification d'intégrité des données. On refuse une requête qui possède des fichiers qui ne sont pas référencés correctement.
                if (!fileRefs.All(refs => fileDico.ContainsKey(refs)))
                {
                    log.Warn("Request rejected: you cannot have a reference to a file which is not in the dictionary.");
                    return new ServerRestRequestResult<RequestSendRespondentDataResult>
                    {
                        Result = Error
                    };
                }
            }

            var url = conf.GetRestUrl("send-respondent-data");

            log.Trace("Sending request: " + url);

            var addReq = new List<RestRequestAdditionalParameters>
            {
                new RestRequestAdditionalParameters(authToken, "Authorization", Header),
                new RestRequestAdditionalParameters
                {
                    Type = Header,
                    Name = "mock",
                    Value = mockRequest
                }
            };

            var extraCodes = new HashSet<HttpStatusCode>
            {
                HttpStatusCode.NoContent,
                HttpStatusCode.Unauthorized,
                HttpStatusCode.BadRequest
            };

            var fieldList = new List<RestRequestMultipartFormBase>();
            
            // Ajoute l'objet de configuration de la requête comme élément multipart.
            var requestConfig = new RestRequestMultipartFormText
            {
                StringData = JsonUtil.CreateJsonString(parameters.RequestConfig),
                MimeType = "application/json", // Au lieu d'un simple format texte.
                FieldName = "request_config", // Requis par la requête web.
                EncodingType = Encoding.UTF8
            };
            fieldList.Add(requestConfig);

            if (fileDico != null)
            {
                // Ajoute tous les fichiers binaires comme champs multipart subséquents.
                fieldList.AddRange(fileDico.AsEnumerable()
                    .Select(kvp => new RestRequestMultipartFormBinaryFile
                    {
                        FileDesc = kvp.Value, FieldName = kvp.Key
                        // Type est binaire par défaut.
                    }));
            }

            if (ct.IsCancellationRequested)
            {
                return new ServerRestRequestResult<RequestSendRespondentDataResult>
                {
                    Result = Cancelled
                };
            }

            try
            {
                var result = await HttpRestUtil.ExecuteCompleteMultipartRequest<RequestSendRespondentDataResult>(fieldList, url, addReq, extraCodes, HttpMethod.Post, debugFile).ConfigureAwait(false);

                switch (result.StatusCode)
                {
                    case HttpStatusCode.Created:
                        return CreateResult("Data sent to respondent.", Success, result.InstancedBody);
                    case HttpStatusCode.NoContent:
                        return CreateResult("No respondent identified.", NoContent, result.InstancedBody);
                    case HttpStatusCode.Unauthorized:
                        return CreateResult<RequestSendRespondentDataResult>("New authorization token needed.", Unauthorized);
                    case HttpStatusCode.BadRequest:
                        return CreateResult<RequestSendRespondentDataResult>("Invalid payload.", Failed);
                    default:
                        return CreateResult<RequestSendRespondentDataResult>("Unknow request result detected.", Error);
                }
            }
            catch (Exception e)
            {
                return CreateResult<RequestSendRespondentDataResult>("Exception thrown during SEND_RESPONDENT_DATA request.", Error, null, e);
            }
        }

        /// <summary>
        /// Créer et exécute une requête vers le serveur principal pour télécharger plusieurs fichier simultanément.
        /// Cette méthode obtient un fichier ZIP.
        /// </summary>
        /// <param name="ct">Jeton de cancellation.</param>
        /// <param name="conf">Configuration serveur.</param>
        /// <param name="authToken">Jeton d'authorisation</param>
        /// <param name="downloadParameters">Paramètres de la requête.</param>
        /// <param name="fileReceived">Le fichier à créer avec le téléchargement.</param>
        /// <returns>TRUE si le fichier a été télécharger et se trouve sur disque.</returns>
        public static ServerRestRequestResult<bool> DownloadMultipleFiles(CancellationToken ct, ServerConfiguration conf, string authToken, 
            RequestDownloadZipFilePayload downloadParameters, [NotNull] FileInfo fileReceived)
        {
            if (ct.IsCancellationRequested)
            {
                return new ServerRestRequestResult<bool>
                {
                    Result = Cancelled
                };
            }

            var url = conf.GetRestUrl("download-zipped-file-bundle");

            log.Trace("Sending request: " + url);

            var addReq = new List<RestRequestAdditionalParameters>
            {
                new RestRequestAdditionalParameters(new HttpJsonBody(downloadParameters)),
                new RestRequestAdditionalParameters(authToken, "Authorization", Header),
            };

            var extraCodes = new HashSet<HttpStatusCode>
            {
                HttpStatusCode.NoContent,
                HttpStatusCode.Unauthorized,
                HttpStatusCode.NotAcceptable
            };

            try
            {
                var result = HttpRestUtil.ExecuteDownLoadRequest(fileReceived, url, addReq, extraCodes, ct);

                switch (result.StatusCode)
                {
                    case HttpStatusCode.NoContent:
                        return CreateResult("Data request returned nothing. No associated files.", Success, true);
                    case HttpStatusCode.OK:
                        return CreateResult("Data request accepted.", Success, true);
                    case HttpStatusCode.Unauthorized:
                        return CreateResult("New authorization token needed.", Unauthorized);
                    case HttpStatusCode.NotAcceptable:
                        return CreateResult("Invalid payload.", Failed);
                    default:
                        return CreateResult("Unknow request result detected.", Error);
                }
            }
            catch (Exception e)
            {
                return CreateResult("Exception thrown during '/download-zipped-file-bundle' request.", Error, false, e);
            }
        }
    }
}