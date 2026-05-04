/*
VimeoDotNet 3.0 By Saeed Afshari (saeed@saeedoo.com)
To support this project, please visit http://saeedoo.com
for more information.

Copyright (c) 2014 Saeed Afshari

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using NLog;

namespace VimeoAPI.API
{
    public struct HttpRequestResultInfo
    {
        public HttpRequestResultInfo(HttpStatusCode responseCode, string responseBody, WebHeaderCollection headers)
        {
            Headers = headers;
            ResponseCode = responseCode;
            ResponseBody = responseBody;
        }

        public HttpStatusCode ResponseCode { get; }
        public WebHeaderCollection Headers { get; }
        public string ResponseBody { get; }
    }

    public static class Helpers
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        public static IWebProxy Proxy = null;

        public static byte[] ToByteArray(string s)
        {
            return Encoding.UTF8.GetBytes(s);
        }

        public static string ToBase64(string s)
        {
            return Convert.ToBase64String(ToByteArray(s));
        }

        public static string ComputeHash(HashAlgorithm hashAlgorithm, string data)
        {
            if (hashAlgorithm == null)
                throw new ArgumentNullException(nameof(hashAlgorithm));

            if (string.IsNullOrEmpty(data))
                throw new ArgumentNullException(nameof(data));

            var dataBuffer = Encoding.ASCII.GetBytes(data);
            var hashBytes = hashAlgorithm.ComputeHash(dataBuffer);
            return Convert.ToBase64String(hashBytes);
        }

        public static string PercentEncode(string value)
        {
            const string unreservedChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-_.~";
            var result = new StringBuilder();

            foreach (var symbol in value)
            {
                if (unreservedChars.IndexOf(symbol) != -1)
                    result.Append(symbol);
                else
                    result.Append('%' + $"{(int)symbol:X2}");
            }

            return result.ToString();
        }

        public static string KeyValueToString(Dictionary<string, string> payload)
        {
            var body = "";
            foreach (var item in payload)
                body += $"{PercentEncode(item.Key)}={PercentEncode(item.Value)}&";
            if (body[body.Length - 1] == '&') body = body.Substring(0, body.Length - 1);
            return body;
        }

        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public static async Task<HttpRequestResultInfo> HTTPFetchAsync(string url, string method,
            WebHeaderCollection headers, Dictionary<string, string> payload,
            string contentType = "application/x-www-form-urlencoded")
        {
            return await HTTPFetchAsync(url, method, headers, KeyValueToString(payload), contentType);
        }

        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public static async Task<HttpRequestResultInfo> HTTPFetchAsync(string url, string method, WebHeaderCollection headers, string payload,
            string contentType = "application/x-www-form-urlencoded")
        {
            var request = WebRequest.CreateHttp(url);
            if (Proxy != null) request.Proxy = Proxy;

            request.Headers = headers;
            request.Method = method;
            request.Accept = "application/vnd.vimeo.*+json; version=3.2";
            request.ContentType = contentType;
            request.KeepAlive = false;

            var streamBytes = ToByteArray(payload);
            request.ContentLength = streamBytes.Length;
            var dataStream = await request.GetRequestStreamAsync();
            await dataStream.WriteAsync(streamBytes, 0, streamBytes.Length);
            dataStream.Close();

            return CraftResponse(request);
        }

        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public static HttpRequestResultInfo HTTPFetch(string url, string method,
            WebHeaderCollection headers, Dictionary<string, string> payload,
            string contentType = "application/x-www-form-urlencoded")
        {
            return HTTPFetch(url, method, headers, KeyValueToString(payload), contentType);
        }

        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public static HttpRequestResultInfo HTTPFetch(string url, string method,
            WebHeaderCollection headers, string payload,
            string contentType = "application/x-www-form-urlencoded")
        {
            var request = WebRequest.CreateHttp(url);
            if (Proxy != null) request.Proxy = Proxy;

            request.Headers = headers;
            request.Method = method;
            request.Accept = "application/vnd.vimeo.*+json; version=3.2";
            request.ContentType = contentType;
            request.KeepAlive = false;

            if (!string.IsNullOrWhiteSpace(payload))
            {
                var streamBytes = ToByteArray(payload);
                request.ContentLength = streamBytes.Length;
                var reqStream = request.GetRequestStream();
                reqStream.Write(streamBytes, 0, streamBytes.Length);
                reqStream.Close();
            }

            return CraftResponse(request);
        }

        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public static HttpRequestResultInfo HTTPFetch(string url, FileInfo fileToUpload,
            string contentType = "video/mp4")
        {
            if (fileToUpload == null || !fileToUpload.Exists) throw new Exception("Invalid file to send.");

            var request = WebRequest.CreateHttp(url);
            if (Proxy != null) request.Proxy = Proxy;
            
            request.Method = "PUT";
            request.ContentType = contentType;
            request.KeepAlive = true;
            
            using (var fileStream = File.OpenRead(fileToUpload.FullName))
            {
                request.ContentLength = fileStream.Length;
                using (var reqStream = request.GetRequestStream())
                {
                    fileStream.CopyTo(reqStream);
                }
            }

            return CraftResponse(request);
        }

        private static HttpRequestResultInfo CraftResponse(HttpWebRequest request)
        {
            try
            {
                var response = (HttpWebResponse)request.GetResponse();
                Debug.WriteLine(response.StatusDescription);

                var dataStream = response.GetResponseStream();
                if (dataStream == null) return new HttpRequestResultInfo(HttpStatusCode.NotImplemented, null, null);
                var reader = new StreamReader(dataStream);
                var responseFromServer = reader.ReadToEnd();
                reader.Close();
                dataStream.Close();
                response.Close();

                Debug.WriteLine(responseFromServer, "HTTPFetch");
                return new HttpRequestResultInfo(response.StatusCode, responseFromServer, response.Headers);
            }
            catch (Exception e)
            {
                log.Error(e);
                return new HttpRequestResultInfo(HttpStatusCode.BadRequest, null, null);
            }
        }
    }
}
