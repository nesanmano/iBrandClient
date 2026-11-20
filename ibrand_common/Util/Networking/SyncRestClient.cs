using Newtonsoft.Json;
using NomadLogic.Common.Util.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace NomadLogic.Common.Util.Networking
{
    public static class SyncRestClient
    {
        private static readonly HttpClient _http;

        static SyncRestClient()
        {
            var handler = new HttpClientHandler();
            _http = new HttpClient(handler)
            {
                Timeout = TimeSpan.FromSeconds(200)
            };
            _http.DefaultRequestHeaders.ExpectContinue = false;
        }

        private static void ApplyParameters(
            HttpRequestMessage req,
            string url,
            IList<RestRequestAdditionalParameters> parameters)
        {
            var qs = new StringBuilder("?");

            foreach (var p in parameters ?? Enumerable.Empty<RestRequestAdditionalParameters>())
            {
                switch (p.Type)
                {
                    case HttpParameterType.Header:
                        req.Headers.Add(p.Name, p.Value.ToString());
                        break;

                    case HttpParameterType.QueryString:
                        qs.Append($"{p.Name}={HttpUtility.UrlEncode(p.Value.ToString())}&");
                        break;

                    case HttpParameterType.Body:
                        if (p.Value is HttpJsonBody jb)
                        {
                            var json = JsonConvert.SerializeObject(jb.Object, jb.SerializerSettings);
                            req.Content = new StringContent(json, jb.Encoding, "application/json");
                        }
                        break;
                }
            }

            if (qs.Length > 1)
                qs.Remove(qs.Length - 1, 1);
            else
                qs.Clear();

            req.RequestUri = new Uri(url + qs);
        }

        private static async Task<T> ReadJsonAsync<T>(HttpResponseMessage response)
        {
            using (var stream = await response.Content.ReadAsStreamAsync())
            {
                using (var reader = new StreamReader(stream))
                {
                    using (var jsonReader = new JsonTextReader(reader))
                    {
                        var serializer = new JsonSerializer();
                        return serializer.Deserialize<T>(jsonReader);
                    }
                }
            }
        }


        public static async Task<RestRequestJsonResponse<T>> ExecuteAsync<T>(
        CancellationToken ct,
        string url,
        IList<RestRequestAdditionalParameters> parameters,
        ISet<HttpStatusCode> expectedCodes,
        HttpMethod method,
        FileInfo debugFile = null)
        {
            var req = new HttpRequestMessage
            {
                Method = method,
                RequestUri = new Uri(url)
            };

            ApplyParameters(req, url, parameters);

            HttpResponseMessage resp;
            try
            {
                resp = await _http.SendAsync(
                        req,
                        HttpCompletionOption.ResponseContentRead,
                        ct).ConfigureAwait(false);

            }
            catch (Exception ex)
            {
                return new RestRequestJsonResponse<T>
                {
                    StatusCode = HttpStatusCode.Conflict,
                    ErrorRawBody = ex.Message
                };
            }

            var status = resp.StatusCode;

            // Error handling
            if (status != HttpStatusCode.OK &&
                status != HttpStatusCode.Created &&
                (expectedCodes == null || !expectedCodes.Contains(status)))
            {
                var raw = await resp.Content.ReadAsStringAsync();
                return new RestRequestJsonResponse<T>
                {
                    StatusCode = status,
                    ErrorRawBody = raw,
                    Headers = resp.Headers
                };
            }

            // JSON parsing
            var result = new RestRequestJsonResponse<T>
            {
                StatusCode = status,
                Headers = resp.Headers
            };

            if (resp.Content.Headers.ContentType?.MediaType == "application/json")
            {
                if (debugFile != null)
                {
                    using (var s = await resp.Content.ReadAsStreamAsync())
                    {
                        using (var fs = File.Create(debugFile.FullName))
                        {
                            await s.CopyToAsync(fs);
                        }
                    }

                    debugFile.Refresh();
                    result.InstancedBody = JsonUtil.ReadJsonFile<T>(debugFile);
                }
                else
                {
                    result.InstancedBody = await ReadJsonAsync<T>(resp);
                }
            }

            return result;
        }
    }


    }
