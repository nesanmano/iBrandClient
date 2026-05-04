using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using JetBrains.Annotations;
using Newtonsoft.Json;
using NLog;
using NomadLogic.Common.Util.Json;
using static NomadLogic.Common.Util.Networking.HttpParameterType;

namespace NomadLogic.Common.Util.Networking
{
    /// <summary>
    /// Classe pour englober les requêtes web via le concept de requête HTTP REST.
    /// </summary>
    [PublicAPI]
    public static class HttpRestUtil
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private static readonly HttpClient HTTP_CLIENT_INSTANCE;

        static HttpRestUtil()
        {
            var handler = new WinHttpHandler();
            HTTP_CLIENT_INSTANCE = new HttpClient(handler);

            HTTP_CLIENT_INSTANCE.DefaultRequestHeaders.ExpectContinue = false;
            HTTP_CLIENT_INSTANCE.Timeout = TimeSpan.FromSeconds(200);
        }

        #region Helper methods
        
        private static void LogFullResponseData(string url, HttpMethod webMethod, string content, HttpHeaders headers)
        {
            if (!log.IsTraceEnabled) return;

            var sb = new StringBuilder();

            sb.Append("REST request content: Method= ");
            sb.Append(webMethod);
            sb.Append(" URL => ");
            sb.AppendLine(url);
            if (headers != null)
            {
                sb.AppendLine("\t\t***************** Request Headers:");
                using (var iter = headers.GetEnumerator())
                {
                    while (iter.MoveNext())
                    {
                        var cur = iter.Current;
                        sb.Append("\t\t");
                        sb.Append(cur.Key);
                        sb.Append(": ");
                        sb.AppendLine(StringUtil.FormatEnumerable(cur.Value, false));
                    }
                }
            }

            if (content != null)
            {
                // Assume que le contenu n'est pas un format binaire mais texte.
                sb.AppendLine("\t\t***************** Request Content:");
                sb.Append("\t\t");
                sb.AppendLine(content);
            }

            log.Trace(sb.ToString);
        }

        private static void PrepareHttpRequestMessage(IEnumerable<RestRequestAdditionalParameters> additionalParameters, HttpRequestMessage hrm, string url)
        {
            var queryStringBuilder = new StringBuilder("?");

            if (additionalParameters != null)
            {
                foreach (var par in additionalParameters)
                {
                    try
                    {
                        switch (par.Type)
                        {
                            case QueryString:
                                queryStringBuilder.Append(par.Name);
                                queryStringBuilder.Append('=');
                                queryStringBuilder.Append(HttpUtility.UrlEncode(par.Value.ToString()).Trim());
                                queryStringBuilder.Append('&');
                                break;
                            case Header:
                                var toAdd = par.Value.ToString();
                                hrm.Headers.Add(par.Name, toAdd);
                                log.Trace("Header added to request:\n{0}={1}", par.Name, toAdd);
                                break;
                            case Body:
                                if (par.Value is HttpJsonBody jsonBody)
                                {
                                    var json = JsonConvert.SerializeObject(jsonBody.Object, Formatting.None, jsonBody.SerializerSettings);
                                    log.Trace("JSON body added to request:\n{0}", json);
                                    hrm.Content = new StringContent(json, jsonBody.Encoding, "application/json");
                                }
                                else if (par.Value is string str)
                                {
                                    log.Trace("STRING body added to request:\n{0}", str);
                                    hrm.Content = new StringContent(str);
                                }
                                else if (par.Value is Stream stream)
                                {
                                    log.Trace("STREAM body added to request. (No content view)");
                                    hrm.Content = new StreamContent(stream);
                                }
                                else
                                {
                                    // Tout autre type d'objets. On assume un object "json-ifiable" avec des configurations de base.
                                    // On assume également que l'on a passé un paramètre de header représentatif.
                                    var json = JsonConvert.SerializeObject(par.Value, Formatting.None, JsonUtil.BASE_SERIALIZER_SETTINGS);
                                    log.Trace("JSON (Default assumption) body added to request:\n{0}", json);
                                    hrm.Content = new StringContent(json);
                                }
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }
                    catch (InvalidOperationException e)
                    {
                        throw new Exception($"Tried to add header to the wrong message part. Name={par.Name}, Value={par.Value}, type={par.Type}, Url={url}", e);
                    }
                }

                if (queryStringBuilder.Length > 1)
                {
                    queryStringBuilder.Remove(queryStringBuilder.Length - 1, 1); // Enlève le dernier '&'
                }
                else
                {
                    // Si pas de query string, on vide ce buffer.
                    queryStringBuilder.Clear();
                }
            }

            var queryResult = queryStringBuilder.ToString();
            if (log.IsTraceEnabled && !string.IsNullOrWhiteSpace(queryResult))
            {
                log.Trace("JSON body added to request:\n{0}", queryResult);
            }

            hrm.RequestUri = new Uri(url + queryResult);
        }
        
        /// <summary>
        /// Fait une analyse poussée de la requête pour déterminer si elle s'est bien passée ou si on tombe dans un comportement prévu.
        /// Retourne NULL si aucune erreur n'est détectée, sinon, retourne un type de retour de base RestRequestBasicResponse avec le code
        /// d'erreur.
        /// </summary>
        /// <typeparam name="T">Le type de retour prévu.</typeparam>
        /// <param name="url">Le URL de base.</param>
        /// <param name="webMethod">La méthode web utilisée.</param>
        /// <param name="expected">Une série de code de retour prévus. OK et CREATED sont toujours assumé valides.</param>
        /// <param name="code">Le code Http retourné.</param>
        /// <param name="content">Le contenu de la réponse.</param>
        /// <param name="headers">Les en-têtes de la réponse.</param>
        /// <param name="alreadyLogged">Indique si un journal à déjà été créé.</param>
        /// <returns>NULL si tout est beau, sinon un type de base RestRequestBasicResponse</returns>
        private static T AnalyseRequestCode<T>(string url, HttpMethod webMethod, ISet<HttpStatusCode> expected, HttpStatusCode code, HttpContent content,
            HttpHeaders headers, bool alreadyLogged = false) where  T : RestRequestBasicResponse
        {
            if (code == HttpStatusCode.OK || 
                code == HttpStatusCode.Created || 
                (expected != null && expected.Contains(code))) return null;

            var contentData = content.ReadAsStringAsync().GetAwaiter().GetResult();

            log.Warn($"Http request was not successful: code={code}, content='{contentData}'");
            if (!alreadyLogged)
            {
                LogFullResponseData(url, webMethod, contentData, headers);
            }
            
            var rrbr = (T)Activator.CreateInstance(typeof(T));
            rrbr.StatusCode = code;
            rrbr.ErrorRawBody = contentData;
            return rrbr;
        }

        private static bool AnalyseSimpleRequestCode(string url, HttpMethod webMethod, ISet<HttpStatusCode> expected, HttpStatusCode code, HttpContent content,
            HttpHeaders headers, bool alreadyLogged = false)
        {
            if (code == HttpStatusCode.OK || 
                code == HttpStatusCode.Created || 
                (expected != null && expected.Contains(code))) return true;

            var contentData = content.ReadAsStringAsync().GetAwaiter().GetResult();

            log.Warn($"Http request was not successful: code={code}, content='{content}'");
            if (!alreadyLogged)
            {
                LogFullResponseData(url, webMethod, contentData, headers);
            }

            return false;
        }

        private static RestRequestJsonResponse<T> CreateJsonRequestResponse<T>(HttpResponseMessage response, FileInfo debugWriteToFilePath = null)
        {
            var restRequestResponse = new RestRequestJsonResponse<T>
            {
                // ReSharper disable once PossibleNullReferenceException
                StatusCode = response.StatusCode,
                Headers = response.Headers
            };

            if (response.Content.Headers.ContentLength == 0) return restRequestResponse;

            if (response.Content.Headers.ContentType.MediaType != "application/json") return restRequestResponse;

            try
            {
                // Fonctionne pour les Strings et les contenus binaires.
                var contentStream = response.Content.ReadAsStreamAsync().Result;

                using (var streamReader = new StreamReader(contentStream))
                {
                    bool success;
                    T info;
                    if (debugWriteToFilePath != null)
                    {
                        log.Debug("Writing request result to file: {0}", debugWriteToFilePath.FullName);
                        // On créer le fichier sur disque avant (partie débug) qui va persister après l'opération.
                        var baseStream = streamReader.BaseStream;
                        using (Stream s = File.Create(debugWriteToFilePath.FullName))
                        {
                            baseStream.CopyTo(s);
                        }
                        debugWriteToFilePath.Refresh();

                        // Puis on lit le fichier comme source pour le démarshalling.
                        info = JsonUtil.ReadJsonFile<T>(debugWriteToFilePath);
                        success = info != null;
                    }
                    else
                    {
                        using (var jsonReader = new JsonTextReader(streamReader))
                        {
                            success = JsonUtil.CreateInstanceFromJson(jsonReader, out info);
                        }
                    }
                    if (success)
                    {
                        restRequestResponse.InstancedBody = info;
                        log.Trace("Rest request converted to JSON with success");
                    }
                    else
                    {
                        log.Warn("Could not marshall request response as JSON.");
                    }
                }
            }
            catch (Exception e)
            {
                log.Error(e, $"Could not instanciate objet type ({typeof(T).Name}) with JSON data.");
                throw;
            }

            return restRequestResponse;
        }

        private static T CreateJsonResponse<T>(HttpResponseMessage response, FileInfo debugWriteToFilePath = null)
        {
            if (response.Content.Headers.ContentLength == 0) return default;

            if (!(response.Content is StreamContent) && response.Content.Headers.ContentType.MediaType != "application/json") return default;

            try
            {
                var contentStream = response.Content.ReadAsStreamAsync().Result;

                using (var streamReader = new StreamReader(contentStream))
                {
                    bool success;
                    T info;
                    if (debugWriteToFilePath != null)
                    {
                        log.Debug("Writing request result to file: {0}", debugWriteToFilePath.FullName);
                        // On créer le fichier sur disque avant (partie débug) qui va persister après l'opération.
                        var baseStream = streamReader.BaseStream;
                        using (Stream s = File.Create(debugWriteToFilePath.FullName))
                        {
                            baseStream.CopyTo(s);
                        }
                        debugWriteToFilePath.Refresh();

                        // Puis on lit le fichier comme source pour le démarshalling.
                        info = JsonUtil.ReadJsonFile<T>(debugWriteToFilePath);
                        success = info != null;
                    }
                    else
                    {
                        using (var jsonReader = new JsonTextReader(streamReader))
                        {
                            success = JsonUtil.CreateInstanceFromJson(jsonReader, out info);
                        }
                    }

                    if (success)
                    {
                        log.Trace("Rest request converted to JSON with success");
                        return info;
                    }
                }
            }
            catch (Exception e)
            {
                log.Error(e, $"Could not instanciate objet type ({typeof(T).Name}) with JSON data.");
                throw;
            }

            log.Trace("Rest request failed to convert to JSON.");
            return default;
        }

        private static async Task<RestRequestJsonResponse<T>> CreateJsonRequestResponseAsync<T>(HttpResponseMessage response, string initialUrl, 
            FileInfo debugWriteToFilePath = null)
        {
            var restRequestResponse = new RestRequestJsonResponse<T>
            {
                // ReSharper disable once PossibleNullReferenceException
                StatusCode = response.StatusCode,
                Headers = response.Headers
            };

            if (response.Content.Headers.ContentLength == 0) return restRequestResponse;

            if (!(response.Content is StreamContent) && response.Content.Headers.ContentType.MediaType != "application/json") return restRequestResponse;

            var contentStream = await response.Content.ReadAsStreamAsync();

            using (var streamReader = new StreamReader(contentStream))
            {
                bool success;
                T info;
                if (debugWriteToFilePath != null)
                {
                    log.Debug("Writing request result to file: {0}", debugWriteToFilePath.FullName);
                    // On créer le fichier sur disque avant (partie débug) qui va persister après l'opération.
                    var baseStream = streamReader.BaseStream;
                    using (Stream s = File.Create(debugWriteToFilePath.FullName))
                    {
                        await baseStream.CopyToAsync(s);
                    }
                    debugWriteToFilePath.Refresh();

                    // Puis on lit le fichier comme source pour le démarshalling.
                    info = JsonUtil.ReadJsonFile<T>(debugWriteToFilePath);
                    success = info != null;
                }
                else
                {
                    using (var jsonReader = new JsonTextReader(streamReader))
                    {
                        success = JsonUtil.CreateInstanceFromJson(jsonReader, out info);
                    }
                }

                // Branche commune
                if (success)
                {
                    restRequestResponse.InstancedBody = info;
                    log.Trace("Rest request converted to JSON with success for URL: {0}", initialUrl);
                }
                else
                {
                    log.Warn("Could not marshall request response as JSON for URL: {0}", initialUrl);
                }
            }

            return restRequestResponse;
        }

        private static async Task<RestRequestJsonResponse<T>> ExecuteAsyncRequest<T>(CancellationToken cToken, string url, 
            IList<RestRequestAdditionalParameters> additionalParameters, ISet<HttpStatusCode> expected, HttpRequestMessage hrm, FileInfo debugFileContent = null)
        {
            try
            {
                PrepareHttpRequestMessage(additionalParameters, hrm, url);

                if (log.IsTraceEnabled)
                {
                    log.Trace($"HttpClient request created:\n\tBase Url={url}\n\t" + hrm.ToStringPublicProperties(1));
                }

                var response = await HTTP_CLIENT_INSTANCE.SendAsync(hrm, HttpCompletionOption.ResponseContentRead, cToken).ConfigureAwait(false); // On attend que toute le requête soit complète.
                
                var errorInst = AnalyseRequestCode<RestRequestJsonResponse<T>>(url, hrm.Method, expected,
                    response?.StatusCode ?? HttpStatusCode.NotAcceptable,
                    response?.Content,
                    response?.Headers,
                    log.IsTraceEnabled);
                return errorInst ?? await CreateJsonRequestResponseAsync<T>(response, url, debugFileContent);
            }
            catch (Exception e)
            {
                if (e is TaskCanceledException || e is OperationCanceledException)
                {
                    log.Trace("Request has been canceled.");
                }
                else
                {
                    log.Error(e, "Request ended with exception.");
                }

                var err = new RestRequestJsonResponse<T>
                {
                    StatusCode = HttpStatusCode.Conflict
                };
                return err;
            }
        }

        private static RestRequestJsonResponse<T> ExecuteRequest<T>(string url, IList<RestRequestAdditionalParameters> additionalParameters, 
            ISet<HttpStatusCode> expected, HttpRequestMessage hrm, FileInfo debugFileContent = null)
        {
            try
            {
                PrepareHttpRequestMessage(additionalParameters, hrm, url);

                if (log.IsTraceEnabled)
                {
                    log.Trace("HttpClient request created:\n\t" + hrm.ToStringPublicProperties(1));
                }

                var response = HTTP_CLIENT_INSTANCE.SendAsync(hrm, HttpCompletionOption.ResponseContentRead).Result; // On attend que toute le requête soit complète.
                
                var errorInst = AnalyseRequestCode<RestRequestJsonResponse<T>>(url, hrm.Method, expected,
                    response?.StatusCode ?? HttpStatusCode.NotAcceptable,
                    response?.Content,
                    response?.Headers,
                    log.IsTraceEnabled);
                return errorInst ?? CreateJsonRequestResponse<T>(response, debugFileContent);
            }
            catch (Exception e)
            {
                log.Error(e, "Request ended with exception.");

                var err = new RestRequestJsonResponse<T>
                {
                    StatusCode = HttpStatusCode.Conflict
                };
                return err;
            }
        }

        public static T ExecuteSimpleRequest<T>(string url, IList<RestRequestAdditionalParameters> additionalParameters, 
            ISet<HttpStatusCode> expected, HttpMethod method = null)
        {
            try
            {
                var hrm = new HttpRequestMessage
                {
                    Method = method ?? HttpMethod.Get
                };

                PrepareHttpRequestMessage(additionalParameters, hrm, url);

                if (log.IsTraceEnabled)
                {
                    log.Trace("HttpClient request created:\n\t" + hrm.ToStringPublicProperties(1));
                }

                var response = HTTP_CLIENT_INSTANCE.SendAsync(hrm, HttpCompletionOption.ResponseContentRead).Result; // On attend que toute le requête soit complète.
                
                var good = AnalyseSimpleRequestCode(url, hrm.Method, expected,
                    response?.StatusCode ?? HttpStatusCode.NotAcceptable,
                    response?.Content,
                    response?.Headers,
                    log.IsTraceEnabled);
                return good ? CreateJsonResponse<T>(response) : default;
            }
            catch (Exception e)
            {
                log.Error(e, "Request ended with exception.");
                return default;
            }
        }
        #endregion

        public static async Task<RestRequestJsonResponse<T>> ExecuteCompleteRestRequestAsync<T>(CancellationToken cToken, string url,
            HttpJsonBody mainBody, ISet<HttpStatusCode> expected = null, HttpMethod webMethod = null)
        {
            var pars = new List<RestRequestAdditionalParameters>
            {
                new RestRequestAdditionalParameters(mainBody)
            };

            return await ExecuteCompleteRestRequestAsync<T>(cToken, url, pars, expected, webMethod).ConfigureAwait(false);
        }

        public static RestRequestJsonResponse<T> ExecuteCompleteRestRequest<T>(string url,
            HttpJsonBody mainBody, ISet<HttpStatusCode> expected = null, HttpMethod webMethod = null)
        {
            var pars = new List<RestRequestAdditionalParameters>
            {
                new RestRequestAdditionalParameters(mainBody)
            };

            return ExecuteCompleteRestRequest<T>(url, pars, expected, webMethod);
        }


        public static async Task<RestRequestJsonResponse<T>> ExecuteCompleteRestRequestAsync<T>(CancellationToken cToken, string url,
            IList<RestRequestAdditionalParameters> additionalParameters, HttpMethod webMethod)
        {
            return await ExecuteCompleteRestRequestAsync<T>(cToken, url, additionalParameters, null, webMethod).ConfigureAwait(false);
        }

        public static RestRequestJsonResponse<T> ExecuteCompleteRestRequest<T>(string url,
            IList<RestRequestAdditionalParameters> additionalParameters, HttpMethod webMethod)
        {
            return ExecuteCompleteRestRequest<T>(url, additionalParameters, null, webMethod);
        }

        /// <summary>
        /// Exécute une requête REST vers un serveur. Utilise JSON comme format de demande.
        /// Utilise un type spécifique comme format de retour auquel il faut "mapper" la réponse de la requête automatiquement.
        /// Le type convertit en instance dans un objet de "wrapping" qui contient les métadonnées de la requête elle-même, comme le code de retour et autre
        /// données comme les "headers" de la réponse web. Cette conversion ne survient que si le type de retour dans la requête HTTP est clairement identifiée comme
        /// le format JSON, ce que toute bonne requête devrait faire via le header: "Content-Type" => "application/json".
        /// Sinon, il n'y a pas de marshalling en JSON mais on retourne tout de même la requête, ce qui permet à l'appellant de formatter lui-même le
        /// résultat au besoin.
        /// </summary>
        /// <typeparam name="T">La réponse Http sera convertit dans ce type.</typeparam>
        /// <param name="url">l'url complet de la requête.</param>
        /// <param name="webMethod">La méthode web. Si nul, on assume GET.</param>
        /// <param name="cToken">Jeton de cancellation.</param>
        /// <param name="expected">Indique un code Http spécifique de retour où l'on ne doit pas considérer comme une erreur (en plus de OK et CREATED)</param>
        /// <param name="additionalParameters"> Paramètres de requête additionnels.</param>
        /// <param name="debugFileContent">SI on veut déboguer le contenu JSON de la réponse.</param>
        /// <returns>Réponse complete avec mapping JSON déjà réalisé.</returns>
        public static async Task<RestRequestJsonResponse<T>> ExecuteCompleteRestRequestAsync<T>(CancellationToken cToken, string url,
            IList<RestRequestAdditionalParameters> additionalParameters = null, ISet<HttpStatusCode> expected = null, HttpMethod webMethod = null,
            FileInfo debugFileContent = null)
        {
            if (cToken.IsCancellationRequested) return null;

            var hrm = new HttpRequestMessage
            {
                Method = webMethod ?? HttpMethod.Get
            };

            return await ExecuteAsyncRequest<T>(cToken, url, additionalParameters, expected, hrm, debugFileContent).ConfigureAwait(false);
        }

        public static RestRequestJsonResponse<T> ExecuteCompleteRestRequest<T>(string url,
            IList<RestRequestAdditionalParameters> additionalParameters = null, ISet<HttpStatusCode> expected = null, HttpMethod webMethod = null)
        {
            var hrm = new HttpRequestMessage
            {
                Method = webMethod ?? HttpMethod.Get
            };

            return ExecuteRequest<T>(url, additionalParameters, expected, hrm);
        }
        
        /// <summary>
        /// Exécute une requête GET vers un serveur pour obtenir un fichier. Par covention, une requête de ce genre est toujours GET, sans corps (sinon, ce serait un POST).
        /// Cette méthode est optimisée pour télécharger des fichiers sans demander une large empreinte mémoire.
        /// Utilise des fichiers temporaires pour le téléchargement et l'écriture (ou écrasement) du fichier final
        /// se fait à la toute fin du processus.
        /// </summary>
        /// <param name="url">l'url complet de la requête.</param>
        /// <param name="cToken">Jeton de cancellation.</param>
        /// <param name="fileToReceive">Si la requête implique un fichier à envoyer, on passe son chemin complet ici.</param>
        /// <param name="expected">Le code HTTP de résultat voulu, si non standard.</param>
        /// <param name="additionalParameters">Paramètres de requête additionnels.</param>
        /// <returns>Réponse complete avec mapping JSON déjà réalisé.</returns>
        public static async Task<RestRequestBasicResponse> ExecuteDownLoadRequestAsync(CancellationToken cToken, [NotNull] FileInfo fileToReceive, string url, 
            IList<RestRequestAdditionalParameters> additionalParameters = null, ISet<HttpStatusCode> expected = null)
        {
            if (cToken.IsCancellationRequested)
            {
                return new RestRequestBasicResponse()
                {
                    StatusCode = HttpStatusCode.SeeOther
                };
            }
            var tempFile = Path.GetTempFileName();
            
            var hrm = new HttpRequestMessage()
            {
                Method = HttpMethod.Get
            };

            try
            {
                PrepareHttpRequestMessage(additionalParameters, hrm, url);
                
                if (log.IsTraceEnabled)
                {
                    log.Trace("HttpClient Download request created:\n\t" + hrm.ToStringPublicProperties(1));
                }

                var response = await HTTP_CLIENT_INSTANCE.SendAsync(hrm, HttpCompletionOption.ResponseHeadersRead, cToken).ConfigureAwait(false); // Important! ResponseHeadersRead.

                using (var stream = await response.Content.ReadAsStreamAsync())
                {
                    using (var fs = new FileStream(tempFile,
                        FileMode.Append, FileAccess.Write, FileShare.None,
                        4096, true))
                    {
                        ulong totalBytesRead = 0;
                        while (!cToken.IsCancellationRequested)
                        {
                            // Read from the web.
                            var buffer = new byte[1024];
                            var read = await stream.ReadAsync(buffer, 0, 1024, cToken);

                            if (read == 0)
                            {
                                // There is nothing else to read.
                                break;
                            }

                            // Report progress.
                            totalBytesRead += (ulong)read;
                            System.Diagnostics.Debug.WriteLine("Bytes read: {0}", totalBytesRead);

                            // Write to file.
                            await fs.WriteAsync(buffer, 0, read, cToken);
                        }
                    }
                }

                var restRequestResponse = new RestRequestBasicResponse
                {
                    StatusCode = response.StatusCode,
                    Headers = response.Headers
                };

                var errorInst = AnalyseRequestCode<RestRequestBasicResponse>(url, HttpMethod.Get, expected, response.StatusCode,response.Content, response.Headers);
                if (errorInst != null)
                {
                    return errorInst;
                }

                var fiTmp = new FileInfo(tempFile);

                var success = FileUtil.CompleteMove(fiTmp.FullName, fileToReceive.FullName);

                if (success)
                {
                    log.Trace("File download finished: {0}", fileToReceive.FullName);
                    return restRequestResponse;
                }

                log.Trace("File download failed: temporary file move failed: {0}", fileToReceive.FullName);
                restRequestResponse.StatusCode = HttpStatusCode.ExpectationFailed;
                return restRequestResponse;
            }
            catch (Exception e)
            {
                log.Error($"Could not transfert temprary file ({tempFile}) to final file location ({fileToReceive.FullName})");
                throw new Exception("Could not move temporary file to final file location.", e);
            }
            finally
            {
                try
                {
                    if (File.Exists(tempFile))
                    {
                        File.Delete(tempFile);
                    }
                }
                catch (Exception)
                {
                    // Die nice
                }
            }
        }

        /// <summary>
        /// Exécute une requête qui se résout via l'écriture sur disque d'un fichier téléchargé.
        /// Si la requête retourne "No Content", la requête est considérée comme valide et succès, mais le fichier n'existera pas.
        /// L'appellant doit vérifier ce cas de figure.
        /// </summary>
        /// <param name="fileToReceive">L'emplacement disque du fichier à créer. Si la réponse du serveur est "no content", le fichier ne sera pas créé.</param>
        /// <param name="url">Le URL du fichier à chercher.</param>
        /// <param name="additionalParameters">Paramètre additionnels.</param>
        /// <param name="expected">Les code HTTP accepté comme réponse.</param>
        /// <param name="cancelStreamCopy">Jeton de cancellation du processus.</param>
        /// <returns>La réponse à la requête.</returns>
        public static RestRequestBasicResponse ExecuteDownLoadRequest([NotNull] FileInfo fileToReceive, string url, 
            IList<RestRequestAdditionalParameters> additionalParameters = null, ISet<HttpStatusCode> expected = null, CancellationToken? cancelStreamCopy = null)
        {
            var ct = cancelStreamCopy ?? CancellationToken.None;

            var tempFile = Path.GetTempFileName();
            
            var hrm = new HttpRequestMessage()
            {
                Method = HttpMethod.Get
            };

            try
            {
                PrepareHttpRequestMessage(additionalParameters, hrm, url);

                if (log.IsTraceEnabled)
                {
                    log.Trace("HttpClient Download request created:\n\t" + hrm.ToStringPublicProperties(1));
                }

                var response = HTTP_CLIENT_INSTANCE.SendAsync(hrm, HttpCompletionOption.ResponseHeadersRead, ct).GetAwaiter().GetResult(); // Important! ResponseHeadersRead.
                
                var restRequestResponse = new RestRequestBasicResponse
                {
                    StatusCode = response.StatusCode,
                    Headers = response.Headers
                };

                // Dans le cas de No Content, c'est une réponse valide, mais qui requiert moins de traitement, car on ne créer pas de fichiers.
                if (response.StatusCode == HttpStatusCode.NoContent)
                {
                    log.Trace("File request finished. No file was download, because content is empty. Return File does NOT exists: {0}", fileToReceive.FullName);
                    return restRequestResponse;
                }

                using (var stream = response.Content.ReadAsStreamAsync().GetAwaiter().GetResult())
                {
                    using (var fs = new FileStream(tempFile,
                        FileMode.Append, FileAccess.Write, FileShare.None,
                        4096, true))
                    {
                        ulong totalBytesRead = 0;

                        while (!ct.IsCancellationRequested)
                        {
                            // Read from the web.
                            var buffer = new byte[1024];
                            var read = stream.Read(buffer, 0, 1024);

                            if (read == 0)
                            {
                                // There is nothing else to read.
                                break;
                            }

                            // Report progress.
                            totalBytesRead += (ulong) read;
                            System.Diagnostics.Debug.WriteLine("Bytes read: {0}", totalBytesRead);

                            // Write to file.
                            fs.Write(buffer, 0, read);
                        }
                    }
                }
                
                var errorInst = AnalyseRequestCode<RestRequestBasicResponse>(url, HttpMethod.Get, expected, response.StatusCode, response.Content, response.Headers);
                if (errorInst != null)
                {
                    return errorInst;
                }

                var fiTmp = new FileInfo(tempFile);

                var success = FileUtil.CompleteMove(fiTmp.FullName, fileToReceive.FullName);

                if (success)
                {
                    log.Trace("File download finished: {0}", fileToReceive.FullName);
                    return restRequestResponse;
                }

                log.Trace("File download failed: temporary file move failed: {0}", fileToReceive.FullName);
                restRequestResponse.StatusCode = HttpStatusCode.ExpectationFailed;
                return restRequestResponse;
            }
            catch (ProtocolViolationException e)
            {
                log.Error(e,"Request protocol is illegal.");
                throw;
            }
            catch (Exception e)
            {
                log.Error(e, $"Could not transfert temprary file ({tempFile}) to final file location ({fileToReceive.FullName})");
                throw new Exception("Could not move temporary file to final file location.", e);
            }
            finally
            {
                try
                {
                    if (File.Exists(tempFile))
                    {
                        File.Delete(tempFile);
                    }
                }
                catch (Exception)
                {
                    // Die nice
                }
            }
        }

        /// <summary>
        /// Exécute une requête REST vers un serveur, dans le but d'envoyer un fichier. Utilise JSON comme format de demande.
        /// Utilise un type spécifique comme format de retour auquel il faut "mapper" la réponse de la requête automatiquement.
        /// Le type convertit en instance dans un objet de "wrapping" qui contient les métadonnées de la requête elle-même, comme le code de retour et autre
        /// données comme les "headers" de la réponse web. Cette conversion ne survient que si le type de retour dans la requête HTTP est clairement identifiée comme
        /// le format JSON, ce que toute bonne requête devrait faire via le header: "Content-Type" => "application/json".
        /// Sinon, il n'y a pas de marshalling en JSON mais on retourne tout de même la requête, ce qui permet à l'appellant de formatter lui-même le
        /// résultat au besoin.
        /// </summary>
        /// <typeparam name="T">La réponse Http sera convertit dans ce type.</typeparam>
        /// <param name="fileToSend">Le fichier a envoyer.</param>
        /// <param name="url">l'url complet de la requête.</param>
        /// <param name="cToken">Jeton de cancellation.</param>
        /// <param name="expected">Indique un code Http spécifique de retour où l'on ne doit pas considérer comme une erreur (en plus de OK et CREATED)</param>
        /// <param name="additionalParameters"> Paramètres de requête additionnels.</param>
        /// <param name="method">La méthode web à utiliser. Si nul, on assume POST</param>
        /// <param name="debugFileContent">SI on veut déboguer la réponse en l'écrivant sur disque au préalable.</param>
        /// <returns>Réponse complete avec mapping JSON déjà réalisé.</returns>
        public static async Task<RestRequestJsonResponse<T>> ExecuteUploadRequestAsync<T>(CancellationToken cToken, [NotNull] FileInfo fileToSend, string url,
            IList<RestRequestAdditionalParameters> additionalParameters = null, ISet<HttpStatusCode> expected = null, HttpMethod method = null, FileInfo debugFileContent = null)
        {
            if (cToken.IsCancellationRequested) return null;

            if (!fileToSend.Exists)
            {
                log.Warn("File does not exists. Cancelling file upload request: " + fileToSend.FullName);
                return null;
            }

            var hrm = new HttpRequestMessage
            {
                Method = method ?? HttpMethod.Post
            };

            using (var form = new MultipartFormDataContent())
            using (var stream = fileToSend.OpenRead())
            using (var streamContent = new StreamContent(stream))
            {
                form.Add(streamContent, "file");

                return await ExecuteAsyncRequest<T>(cToken, url, additionalParameters, expected, hrm, debugFileContent).ConfigureAwait(false);
            }
        }

        public static RestRequestJsonResponse<T> ExecuteUploadRequest<T>([NotNull] FileInfo fileToSend, string url,
            IList<RestRequestAdditionalParameters> additionalParameters = null, ISet<HttpStatusCode> expected = null, HttpMethod method = null)
        {
            if (!fileToSend.Exists)
            {
                log.Warn("File does not exists. Cancelling file upload request: " + fileToSend.FullName);
                return null;
            }

            var hrm = new HttpRequestMessage
            {
                Method = method ?? HttpMethod.Post
            };
            hrm.Headers.ExpectContinue = false;
            
            using (var form = new MultipartFormDataContent())
            using (var stream = fileToSend.OpenRead())
            using (var streamContent = new StreamContent(stream))
            {
                streamContent.Headers.Add("Content-Type", "application/octet-stream");

                // Cette ligne est importante car c'est elle qui lie le contenu à un nom de variable fichier à décoder pour le parser HTML
                streamContent.Headers.Add( "Content-Disposition", $"form-data; name=\"file\"; filename=\"{fileToSend.Name}\"" );
                form.Add(streamContent, "file", fileToSend.Name);
                hrm.Content = form;

                return ExecuteRequest<T>(url, additionalParameters, expected, hrm);
            }
        }

        /// <summary>
        /// Cette requête créer une requête "multipart/form-data" complète, qui peut contenir un nombre arbitraire de champs distincts les un
        /// des autres.
        /// On fournit une liste des "champs" à envoyer, qui peuvent prendre plusieures forms distinctes: texte ou format binaire, principalement. Mais chaque "champ"
        /// est libre de donner son format MIME individuellement.
        /// </summary>
        /// <typeparam name="T">Le type de réponse JSON attendue.</typeparam>
        /// <param name="formData">La liste des champs et de leur valeurs.</param>
        /// <param name="url">Le url complet du end-point.</param>
        /// <param name="additionalParameters">Paramètres additionnels de la requête.</param>
        /// <param name="expected">Codes HTTP additionnels qui sont normalement attendu par l'appellant comme valide.</param>
        /// <param name="method">Type de requête HTTP.</param>
        /// <returns>Le résultat de la requête.</returns>
        public static RestRequestJsonResponse<T> ExecuteCompleteMultipartRequest<T>([NotNull] IList<RestRequestMultipartFormBase> formData, string url,
            IList<RestRequestAdditionalParameters> additionalParameters = null, ISet<HttpStatusCode> expected = null, HttpMethod method = null,
            FileInfo debugFileContent = null)
        {
            if (!formData.Any())
            {
                log.Warn("Empty or null field list. Cancelling multipart request.");
                return null;
            }

            var hrm = new HttpRequestMessage
            {
                Method = method ?? HttpMethod.Post
            };
            hrm.Headers.ExpectContinue = false;

            RestRequestJsonResponse<T> requestResult;
            using (var form = new MultipartFormDataContent())
            {
                try
                {
                    // Ajoute les données à la forme
                    foreach (var field in formData)
                    {
                        log.Trace("Form field added to request: Field name={0} ({1}), value={2}", field.FieldName, field.MimeType, field.ToStringDesc());
                        field.AddToForm(form);
                    }

                    hrm.Content = form;
                    requestResult = ExecuteRequest<T>(url, additionalParameters, expected, hrm, debugFileContent);
                }
                catch (Exception e)
                {
                    log.Error(e, "Could not finish multipart form construction.");
                    requestResult = new RestRequestJsonResponse<T>
                    {
                        StatusCode = HttpStatusCode.Conflict
                    };
                }
                finally
                {
                    // Libère les ressources des champs (dans le cas de fichiers binaires surtout).
                    foreach (var field in formData)
                    {
                        field.Dispose();
                    }
                }
            }

            return requestResult;
        }
    }

    [PublicAPI]
    public struct RestRequestAdditionalParameters
    {
        public RestRequestAdditionalParameters(object value, string name = null, HttpParameterType type = Body)
        {
            Name = name?.Trim();
            Value = value;
            Type = type;
        }

        public string Name { get; set; }

        public object Value { get; set; }

        public HttpParameterType Type { get; set; }

        public override string ToString()
        {
            return this.ToStringPublicProperties();
        }

        public string ToStringDebug()
        {
            var sb = new StringBuilder();

            sb.Append("Parameter type=");
            sb.Append(Type.ToString());
            if (!string.IsNullOrWhiteSpace(Name))
            {
                sb.Append(", Name=");
                sb.Append(Name);
            }
            sb.Append(", Value=");
            if (Value is string s)
            {
                sb.Append(s);
            }
            else if (Value is HttpJsonBody hjb)
            {
                sb.Append("[Enc=");
                sb.Append(hjb.Encoding);
                sb.Append("] Json=");
                sb.Append(JsonConvert.SerializeObject(hjb.Object, Formatting.None, hjb.SerializerSettings));
            } 
            else if (Value.GetType().IsValueType)
            {
                sb.Append(Value);
            }
            else
            {
                // Pour tout type d'objet autre, pour ne pas mettre des valeurs trop grosse dans le debug.
                sb.Append("[Reference type=");
                sb.Append(Value.GetType().FullName);
                sb.Append("]");
            }

            return sb.ToString();
        }
    }

    public enum HttpParameterType
    {
        QueryString,
        Header,
        Body
    }

    [PublicAPI]
    public class HttpJsonBody
    {
        public JsonSerializerSettings SerializerSettings { get; set; } = JsonUtil.BASE_SERIALIZER_SETTINGS;
        public object Object { get; set; }
        public Encoding Encoding { get; set; } = Encoding.UTF8;

        public HttpJsonBody() {}

        public HttpJsonBody(object instance, JsonSerializerSettings settings = null, Encoding encoding = null)
        {
            Object = instance;
            if (settings != null) SerializerSettings = settings;
            if (encoding != null) Encoding = encoding;
        }
    }

    public abstract class RestRequestMultipartFormBase
    {
        public string FieldName { get; set; }

        public virtual string MimeType { get; set; }

        public abstract void AddToForm(MultipartFormDataContent form);

        public abstract void Dispose();

        public abstract string ToStringDesc();
    }

    public class RestRequestMultipartFormText : RestRequestMultipartFormBase
    {
        public string StringData { get; set; }
        
        public Encoding EncodingType { get; set; } = Encoding.UTF8;

        public override string MimeType { get; set; } = "application/text";

        public override void AddToForm(MultipartFormDataContent form)
        {
            /*
            var textContent = new StringContent(StringData, EncodingType);
            textContent.Headers.Add("Content-Disposition", MimeType);
            form.Add(textContent, FieldName);
            */

            form.Add(new StringContent(StringData, EncodingType), FieldName);

        }

        public override void Dispose()
        {
            // Nothing
        }

        public override string ToStringDesc()
        {
            return StringData;
        }
    }

    public class RestRequestMultipartFormBinaryFile : RestRequestMultipartFormBase
    {
        public FileInfo FileDesc { get; set; }

        private FileStream _fileStream;

        public override string MimeType { get; set; } = "application/octet-stream";

        public override void AddToForm(MultipartFormDataContent form)
        {
            if (FileDesc == null || !FileDesc.Exists) throw new FileNotFoundException("File does not exists :" + (FileDesc?.FullName ?? "null"));

            _fileStream = new FileStream(FileDesc.FullName, FileMode.Open);

            /*
            var streamContent = new StreamContent(_fileStream);
            streamContent.Headers.Add("Content-Type", MimeType);
            streamContent.Headers.Add( "Content-Disposition", $"form-data; name=\"{FieldName}\"; filename=\"{FileDesc.Name}\"" );
            form.Add(streamContent);
            */

            form.Add(new StreamContent(_fileStream), FieldName, FileDesc.FullName);
        }

        public override void Dispose()
        {
            try
            {
                _fileStream?.Close();
                _fileStream = null;
            }
            catch (Exception)
            {
                // die nice
            }
        }

        public override string ToStringDesc()
        {
            return $"File Stream (\"{FileDesc?.FullName}\")";
        }
    }
}