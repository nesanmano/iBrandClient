using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using NomadLogic.Common.Util.Networking;

//using System.Linq;

namespace NomadLogic.iBrandDesktop.Util.Social.Vimeo
{
    public class AdvancedApi
    {
        public const string RequestTokenUrl = "https://vimeo.com/oauth/request_token?";
        public const string RequestAuthorizeUrl = "https://vimeo.com/oauth/authorize?";
        public const string RequestAccessUrl = "https://vimeo.com/oauth/access_token";
        public const string StandardAdvancedApiUrl = "https://vimeo.com/api/rest/v2";
        public const string BaseVimeoUrl = "https://vimeo.com/api/v2/";

        readonly string _consumerKey;
        readonly string _consumerSecret;
        readonly string _requestPermission;

        public AdvancedApi(string consumerKey, string consumerSecret, string permission)
        {
            _consumerKey = consumerKey;
            _consumerSecret = consumerSecret;
            _requestPermission = permission;
        }

        public AdvancedApi(string consumerKey, string consumerSecret)
        {
            _consumerKey = consumerKey;
            _consumerSecret = consumerSecret;
            _requestPermission = "delete";
        }

        public string ExecuteGetCommand(string url, string userName,
            string password)
        {
            return ExecuteGetCommand(url, userName,
            password, null);
        }

        private HttpWebRequest CreateWebRequest(string url)
        {
            var uri = new Uri(url);
            /*ServicePoint servicePoint = ServicePointManager.FindServicePoint(uri);
            servicePoint.BindIPEndPointDelegate = new BindIPEndPoint(Bind);

            //Will cause bind to be called periodically
            servicePoint.ConnectionLeaseTimeout = 0;*/

            var req = (HttpWebRequest)WebRequest.Create(uri);
            //will cause bind to be called for each request (as long as the consumer of the request doesn't set it back to true!
            req.ServicePoint.BindIPEndPointDelegate = new BindIPEndPoint(Bind);

            return req;
        }


        private IPEndPoint Bind(ServicePoint servicePoint, IPEndPoint remoteEndPoint, int retryCount)
        {
            var localIp = NetUtil.GetLocalIpAddress();
            var address = IPAddress.Parse(localIp);
            return new IPEndPoint(address, 0);
        }

        public string ExecuteGetCommand(string url, string userName,
            string password, WebProxy proxy)
        {

            try
            {
                var request = CreateWebRequest(url);

                using (var response = (HttpWebResponse)request.GetResponse())
                {

                    using (var stream = response.GetResponseStream())
                    {
                        using (var reader = new StreamReader(stream))
                        {
                            var r = reader.ReadToEnd();
                            Debug.WriteLine("OK: " + url + "\n[" + r.Length + "B]: " + r.Substring(0, Math.Min(256, r.Length)), "ExecuteGetCommand");
                            return r;
                        }
                    }
                }
            }
            catch (WebException ex)
            {
                Debug.WriteLine("FAIL: " + url, "ExecuteGetCommand");
                Debug.WriteLine("MSG: " + ex.Message, "ExecuteGetCommand");

                // Handle HTTP 404 errors gracefully and return a null string 
                // to indicate there is no content.
                if (ex.Response is HttpWebResponse)
                    Debug.WriteLine("RSP: " + (ex.Response as HttpWebResponse).StatusCode, "ExecuteGetCommand");

                return null; //wtf was the other one for then? hmm
            }


        }

        /*public string ExecuteGetCommand(string url, string userName, 
            string password, WebProxy proxy)
		{

            using (WebClient wc = new WebClient())
			{
                if (proxy != null) wc.Proxy = proxy;
                Debug.WriteLine("GET: " + url, "ExecuteGetCommand");

				if (!string.IsNullOrEmpty(userName) && !string.IsNullOrEmpty(password))
					wc.Credentials = new NetworkCredential(userName, password);
                
				try
				{
					using (Stream stream = wc.OpenRead(url))
					{
                        using (StreamReader reader = new StreamReader(stream))
						{
                            string r = reader.ReadToEnd();
                            Debug.WriteLine("OK: " + url + "\n[" + r.Length + "B]: " + r.Substring(0,Math.Min(256, r.Length)), "ExecuteGetCommand");
							return r;
						}
					}
				}
				catch (WebException ex)
				{
                    Debug.WriteLine("FAIL: " + url, "ExecuteGetCommand");
                    Debug.WriteLine("MSG: " + ex.Message, "ExecuteGetCommand");

					// Handle HTTP 404 errors gracefully and return a null string 
                    // to indicate there is no content.
					if (ex.Response is HttpWebResponse)
                        Debug.WriteLine("RSP: " + (ex.Response as HttpWebResponse).StatusCode, "ExecuteGetCommand");
                       
                    return null; //wtf was the other one for then? hmm
				}
			}
		}*/

        public string BuildOAuthApiRequestUrl(string url)
        {
            return BuildOAuthApiRequestUrl(url, string.Empty, string.Empty);
        }

        public string BuildOAuthApiRequestUrl(string url, string token, string tokenSecret)
        {
            Dictionary<string, string> x;
            return BuildOAuthApiRequestUrl(url, token, tokenSecret, out x, "GET");
        }

        public string BuildOAuthApiRequestUrl(string url, string token, string tokenSecret, out Dictionary<string, string> parameters, string httpMethod)
        {
            return BuildOAuthApiRequestUrl(
                url,
                token,
                tokenSecret,
                out parameters,
                httpMethod,
                _consumerKey,
                _consumerSecret);
        }

        public static string BuildOAuthApiRequestUrl(string url, string token, string tokenSecret, out Dictionary<string, string> parameters, string httpMethod, string consumerKey, string consumerSecret)
        {
            return BuildOAuthApiRequestUrl(url, token, tokenSecret, out parameters, httpMethod, consumerKey, consumerSecret, true);
        }

        public static string BuildOAuthApiRequestUrl(string url, string token, string tokenSecret, out Dictionary<string, string> parameters, string httpMethod, string consumerKey, string consumerSecret, bool addCallBack)
        {
            var oAuth = new OAuthBase();
            var nonce = oAuth.GenerateNonce();
            var timeStamp = oAuth.GenerateTimeStamp();

            string normalizedUrl;
            string normalizedRequestParameters;

            var uri = new Uri(url);

            var sig = oAuth.GenerateSignature(uri, consumerKey,
                    consumerSecret,
                    token, tokenSecret, httpMethod, timeStamp, nonce,
                    OAuthSignatureType.Hmacsha1,
                    out normalizedUrl, out normalizedRequestParameters, addCallBack);

            sig = HttpUtility.UrlEncode(sig);

            parameters = new Dictionary<string, string>();
            var query = uri.Query;
            var newurl = url.Split('?')[0] + '?';

            if (query[0] == '?') query = query.Remove(0, 1);

            if (query.Length > 0)
                foreach (var item in query.Split('&'))
                {
                    var keyvalue = item.Split('=');
                    if (keyvalue[0] == "method")
                    {
                        newurl += item + "&";
                    }
                    else
                    {
                        if (!parameters.ContainsKey(keyvalue[0]))
                            parameters.Add(keyvalue[0], keyvalue[1]);
                    }
                }

            uri = new Uri(newurl);

            if (addCallBack)
                parameters.Add("oauth_callback", "oob"); //Steven

            parameters.Add("oauth_consumer_key", consumerKey);
            parameters.Add("oauth_nonce", nonce);
            parameters.Add("oauth_timestamp", timeStamp);
            parameters.Add("oauth_signature_method", "HMAC-SHA1");
            parameters.Add("oauth_version", "1.0");
            parameters.Add("oauth_signature", sig);

            //var lst = from e in parameters orderby e.Key ascending select e;

            //parameters = lst.ToDictionary(k => k.Key, k => k.Value);

            var sb = new StringBuilder(uri.ToString());
            foreach (var item in parameters)
                sb.AppendFormat(item.Key + "={0}&", item.Value);

            return sb.ToString();
        }

        public string GetUnauthorizedRequestToken(WebProxy proxy)
        {
            var url = BuildOAuthApiRequestUrl(RequestTokenUrl);
            return ExecuteGetCommand(url, null, null, proxy);
        }

        public string GetAuthorizationUrl(string unauthorizedToken)
        {
            return RequestAuthorizeUrl + unauthorizedToken +
                "&permission=" + _requestPermission;
        }

        public string GetAccessToken(WebProxy proxy, string oauthToken, string secret, string oauthVerifier)
        {
            var url = BuildOAuthApiRequestUrl(RequestAccessUrl +
                "?oauth_verifier=" + oauthVerifier + "&oauth_token=" + oauthToken, oauthToken, secret);
            return
                ExecuteGetCommand(
                url,
                null, null, proxy);
        }

        public static string GetParameterValue(string url, string parameterName)
        {
            if (string.IsNullOrEmpty(url)) return null;
            var vars = url.Split('?', '&');
            foreach (var item in vars)
            {
                var pair = item.Split('=');
                if (pair[0] == parameterName)
                    return pair[1];
            }
            return null;
        }

        public static string GetVimeoVideoIdFromUrl(string videoUrl)
        {
            if (videoUrl.Contains("/"))
            {
                var splitter = videoUrl.Split('/');

                return splitter[splitter.Length - 1];
            }

            int v;
            return int.TryParse(videoUrl, out v) ? videoUrl : string.Empty;
        }
    }
}

