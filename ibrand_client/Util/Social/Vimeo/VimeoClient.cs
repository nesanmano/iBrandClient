using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Xml.Linq;
using NLog;
using NomadLogic.Common.Util.Networking;
// ReSharper disable PossibleNullReferenceException
// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global

namespace NomadLogic.iBrandDesktop.Util.Social.Vimeo
{
    public class VimeoClient
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        protected AdvancedApi _vimeo_api;
        protected const string OAuthTokenKey = "oauth_token";
        protected const string OAuthTokenSecretKey = "oauth_token_secret";

        protected const string Hmacsha1SignatureType = "HMAC-SHA1";
        protected const string Delimiter = "%2C";


        public const bool IsVfw = false;
        public string _token;
        public string _token_secret;
        public string _user_id;
        public string _user_name;
        public long _free_quota;
        public long _max_quota;
        public long _used_quota;
        public bool _allows_hd;
        public int _quota_resets;

        public WebProxy _proxy;

        public Person _me;

        public VimeoClient(string consumerKey, string consumerSecret)
        {
            _vimeo_api = new AdvancedApi(consumerKey, consumerSecret, "write");
        }

        public void GetUnauthorizedRequestToken()
        {
            var q = _vimeo_api.GetUnauthorizedRequestToken(_proxy);
            _token = AdvancedApi.GetParameterValue(q, OAuthTokenKey);
            _token_secret = AdvancedApi.GetParameterValue(q, OAuthTokenSecretKey);
        }

        public string GenerateAuthorizationUrl()
        {
            return _vimeo_api.GetAuthorizationUrl(OAuthTokenKey + "=" + _token);
        }

        public string PostVideo(Ticket ticket, string path)
        {
            return PostVideo(ticket, 0, path, 0, -1);
        }

        public string PostVideo(Ticket ticket, int chunkId, string path, long startbyte, int size)
        {
            var fi = new FileInfo(path);
            var sr = fi.OpenRead();
            sr.Seek(startbyte, SeekOrigin.Begin);
            byte[] data;
            if (size > 0)
            {
                if (size + startbyte > fi.Length)
                {
                    size = (int)(fi.Length - startbyte);
                }
                data = new byte[size];
                sr.Read(data, 0, size);
            }
            else
            {
                data = new byte[fi.Length - startbyte];
                sr.Read(data, 0, (int)(fi.Length - startbyte));
            }

            var response = PostVideo(ticket, chunkId, Path.GetFileName(path), data);
            sr.Close();
            return response;
        }

        private HttpWebRequest CreateWebRequest(string url)
        {
            var uri = new Uri(url);
            var servicePoint = ServicePointManager.FindServicePoint(uri);
            servicePoint.BindIPEndPointDelegate = Bind;

            //Will cause bind to be called periodically
            servicePoint.ConnectionLeaseTimeout = 0;

            var req = (HttpWebRequest)WebRequest.Create(uri);
            //will cause bind to be called for each request (as long as the consumer of the request doesn't set it back to true!
            req.KeepAlive = false;

            return req;
        }


        private IPEndPoint Bind(ServicePoint servicePoint, IPEndPoint remoteEndPoint, int retryCount)
        {
            var localIp = NetUtil.GetLocalIpAddress();
            var address = IPAddress.Parse(localIp);
            return new IPEndPoint(address, 0);
        }

        public string PostVideo(Ticket ticket, int chunkId, string fileName, byte[] fileData)
        {
            var parameters = new Dictionary<string, string> {{"ticket_id", ticket._id}, {"chunk_id", chunkId.ToString()}};

            GetRequestUrl(ticket._endpoint, null, parameters, out var oauthParameters, "POST");
            ticket._endpoint += "&chunk_id=" + chunkId;
            foreach (var item in parameters)
            {
                oauthParameters[item.Key] = item.Value;
            }
            oauthParameters.Add("file_data", "");
            //HttpWebRequest req = WebRequest.Create(ticket._endpoint) as HttpWebRequest;
            var req = CreateWebRequest(ticket._endpoint);
            req.Timeout = 10 * 60 * 1000;
            if (_proxy != null) req.Proxy = _proxy;

            // Get a boundary string - used to separate parts of the form data
            var boundary = $"----------{Guid.NewGuid()}";

            req.ContentType = $"multipart/form-data; boundary={boundary}";
            req.Method = "POST";
            req.KeepAlive = true;
            req.Credentials = CredentialCache.DefaultCredentials;

            var header = new StringBuilder();
            var header2 = new StringBuilder();

            foreach (var item in oauthParameters)
            {
                header.AppendFormat("--{0}\r\n", boundary);
                if (item.Key == "file_data")
                {
                    header.AppendFormat("Content-Disposition: form-data; name=\"file_data\"; filename=\"{0}\"\r\n",
                        fileName);
                    header.Append("Content-Type: application/octet-stream\r\n\r\n");
                    header2 = header;
                    header = new StringBuilder();
                    header.Append("\r\n");
                }
                else
                {
                    header.AppendFormat("Content-Disposition: form-data; name=\"" + item.Key + "\"\r\n\r\n{0}\r\n",
                        item.Value);
                }
            }
            
            // Get the header as bytes
            var headerBytes = Encoding.ASCII.GetBytes(header.ToString());
            var header2Bytes = Encoding.ASCII.GetBytes(header2.ToString());
            
            // Get the footer bytes
            var footerBytes = Encoding.ASCII.GetBytes($"\r\n--{boundary}--");

           
            
            // Get the complete Set of bytes
            /*byte[] data = header2Bytes
            .Concat(file_data)
            .Concat(headerBytes)
            .Concat(footerBytes)
            .ToArray();*/

            var data = Combine(header2Bytes, fileData, headerBytes, footerBytes);

            // Set the content length
            req.ContentLength = data.Length;

            // write the bytes to the request stream
            using (var s = req.GetRequestStream())
            {
                s.Write(data, 0, data.Length);
            }

            // Get the response
            var response = req.GetResponse() as HttpWebResponse;

            var responseStream = response.GetResponseStream();
            // ReSharper disable once AssignNullToNotNullAttribute
            var responseReader = new StreamReader(responseStream);

            return responseReader.ReadToEnd();
        }

        public static byte[] Combine(byte[] first, byte[] second, byte[] third, byte[] fourth)
        {
            var ret = new byte[first.Length + second.Length + third.Length + fourth.Length];
            Buffer.BlockCopy(first, 0, ret, 0, first.Length);
            Buffer.BlockCopy(second, 0, ret, first.Length, second.Length);
            Buffer.BlockCopy(third, 0, ret, first.Length + second.Length,
                             third.Length);
            Buffer.BlockCopy(fourth, 0, ret, first.Length + second.Length + third.Length,
                             fourth.Length);
            return ret;
        }

#pragma warning disable IDE1006 // Naming Styles
        public string vimeo_videos_upload_complete(string filename, string ticketId)
#pragma warning restore IDE1006 // Naming Styles
        {
            var parameters = new Dictionary<string, string> {{"filename", filename}, {"ticket_id", ticketId}};

            var x = ExecuteGetRequest("vimeo.videos.upload.complete", parameters);
            if (!IsResponseOK(x)) return null;
            return x.Element("rsp").Element("ticket").Attribute("video_id").Value;
        }

        public void GetAccessToken(string verifier)
        {
            var q = _vimeo_api.GetAccessToken(_proxy, _token, _token_secret, verifier);
            _token = AdvancedApi.GetParameterValue(q, OAuthTokenKey);
            _token_secret = AdvancedApi.GetParameterValue(q, OAuthTokenSecretKey);
        }

        public bool Login()
        {
            if (string.IsNullOrEmpty(_token) || string.IsNullOrEmpty(_token_secret))
                return false;
            var loginResponse = vimeo_test_login();
            
            if (loginResponse._stat != "ok") return false;
            
            if (_me == null)
                if ((_me = vimeo_people_getInfo(loginResponse._userid)) == null) return false;
            
            _user_id = loginResponse._userid;
            _user_name = loginResponse._username;
            return true;
        }

        public class VimeoTestLoginResponse
        {
            public string _stat;
            public string _userid;
            public string _username;
        }

#pragma warning disable IDE1006 // Naming Styles
        public VimeoTestLoginResponse vimeo_test_login()
#pragma warning restore IDE1006 // Naming Styles
        {
            var x = ExecuteGetRequest("vimeo.test.login", null);
            var r = new VimeoTestLoginResponse {_stat = "n/a"};
            try
            {
                r._stat = (string)x.Element("rsp").Attribute("stat");
                r._userid = (string)x.Element("rsp").Element("user").Attribute("id");
                r._username = x.Element("rsp").Element("user").Element("username").Value;
            }
            catch
            {
                r._stat = "error";
            }
            return r;
        }

#pragma warning disable IDE1006 // Naming Styles
        public Person vimeo_people_getInfo(string userId)
#pragma warning restore IDE1006 // Naming Styles
        {
            var parameters = new Dictionary<string, string> {{"user_id", userId}};


            var x = ExecuteGetRequest("vimeo.people.getInfo", parameters);
            if (!IsResponseOK(x)) return null;
            return Person.FromElement(x.Element("rsp").Element("person"));
        }

        public bool RefreshQuota()
        {
            if (string.IsNullOrEmpty(_token) || string.IsNullOrEmpty(_token_secret))
                return false;
            var quotaResponse = vimeo_videos_upload_getQuota();
            if (quotaResponse == null) return false;
            _free_quota = quotaResponse._free;
            _max_quota = quotaResponse._max;
            _used_quota = quotaResponse._used;
            _quota_resets = quotaResponse._resets;
            _allows_hd = quotaResponse._hd_quota;

            return true;
        }

        //.getQuota
        public class VimeoVideosUploadGetQuotaResponse
        {
            public string _userid;
            public bool _is_plus;
            public long _free;
            public long _max;
            public int _resets;
            public long _used;
            public bool _hd_quota;
            public bool _sd_quota;

            public static VimeoVideosUploadGetQuotaResponse FromElement(XElement e)
            {
                var quota = new VimeoVideosUploadGetQuotaResponse
                {
                    _userid = e.Element("user").Attribute("id").Value,
                    _is_plus = e.Element("user").Attribute("is_plus").Value == "1",
                    _free = long.Parse(e.Element("user").Element("upload_space").Attribute("free").Value),
                    _max = long.Parse(e.Element("user").Element("upload_space").Attribute("max").Value),
                    _resets = int.Parse(e.Element("user").Element("upload_space").Attribute("resets").Value),
                    _used = long.Parse(e.Element("user").Element("upload_space").Attribute("used").Value),
                    _hd_quota = e.Element("user").Element("hd_quota").Value == "1",
                    _sd_quota = e.Element("user").Element("sd_quota").Value == "1"
                };
                return quota;
            }
        }

#pragma warning disable IDE1006 // Naming Styles
        public VimeoVideosUploadGetQuotaResponse vimeo_videos_upload_getQuota()
#pragma warning restore IDE1006 // Naming Styles
        {
            var x = ExecuteGetRequest("vimeo.videos.upload.getQuota", null);
            if (!IsResponseOK(x)) return null;
            return VimeoVideosUploadGetQuotaResponse.FromElement(x.Element("rsp"));
        }

#pragma warning disable IDE1006 // Naming Styles
        public Ticket vimeo_videos_upload_getTicket(string videoId, string mode)
#pragma warning restore IDE1006 // Naming Styles
        {
            var parameters = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(videoId)) parameters.Add("video_id", videoId);
            if (!string.IsNullOrEmpty(mode)) parameters.Add("mode", mode);
            
            var x = ExecuteGetRequest("vimeo.videos.upload.getTicket", parameters);
            
            if (!IsResponseOK(x)) return null;
            
            return Ticket.FromElement(x.Element("rsp").Element("ticket"));
        }

        public string GetRequestUrl(string baseUrl, string method, Dictionary<string, string> parameters, out Dictionary<string, string> oauthParameters, string httpMethod)
        {
            if (parameters == null) parameters = new Dictionary<string, string>();
            var url = baseUrl;
            if (!string.IsNullOrEmpty(method))
                 url += "?method=" + method;
            if (!string.IsNullOrEmpty(_token) && !parameters.ContainsKey(OAuthTokenKey))
                parameters.Add(OAuthTokenKey, _token);

            //var lst = from e in parameters orderby e.Key ascending select e;

            foreach (var item in parameters)
            {
                url += "&" + item.Key + "=";
                if (!string.IsNullOrEmpty(item.Value))
                {
                    if (item.Value[0] == '\'')
                        url += item.Value.Substring(1, item.Value.Length - 1);
                    else
                        url += /*HttpUtility*/ OAuthBase.UrlEncode(item.Value);
                }
            }
            return _vimeo_api.BuildOAuthApiRequestUrl(url, _token, _token_secret, out oauthParameters, httpMethod);
        }

        public XDocument ExecuteGetRequest(string method, Dictionary<string, string> parameters)
        {
            return ExecuteGetRequest(GetRequestUrl(
                AdvancedApi.StandardAdvancedApiUrl, method, parameters));
        }

        public XDocument ExecuteGetRequest(string url)
        {
            if (url == null)
            {
                return null;
            }
            //mysterious error comes out of this hole
            //let's seal it
            try
            {
                return XDocument.Parse(
                    _vimeo_api.ExecuteGetCommand(url, null, null, _proxy),
                    LoadOptions.None);
            }
            catch
            {
                log.Debug("Error when executing Get request\nwith url: " + url, "ExecuteGetRequest");
                return null;
            }
        }

        public string GetRequestUrl(string baseUrl, string method, Dictionary<string, string> parameters)
        {
            return GetRequestUrl(baseUrl, method, parameters, out _, "GET");
        }

        public static bool IsResponseOK(XDocument xml)
        {
            return GetResponseState(xml) == "ok";
        }

        public bool IsResponseOK(string url)
        {
            return IsResponseOK(ExecuteGetRequest(url));
        }

        public static string GetResponseState(XDocument xml)
        {
            try
            {
                return (string)xml.Element("rsp").Attribute("stat");
            }
            catch
            {
                return "error";
            }
        }

        
    }
}
