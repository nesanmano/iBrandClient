using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace NomadLogic.iBrandDesktop.Util.Social.Vimeo
{
    public class OAuthBase
    {
        /// <summary>
        /// Provides an internal structure to sort the query parameter
        /// </summary>
        protected class QueryParameter
        {
            public QueryParameter(string name, string value)
            {
                _name = name;
                _value = value;
            }

            public string _name;// { Get; private Set; }
            public string _value;// { Get; private Set; }
        }

        /// <summary>
        /// Comparer class used to perform the sorting of the query parameters
        /// </summary>
        protected class QueryParameterComparer : IComparer<QueryParameter>
        {
            public int Compare(QueryParameter x, QueryParameter y)
            {
                return x._name == y._name 
                    ? string.Compare(x._value, y._value) 
                    : string.Compare(x._name, y._name);
            }
        }

        protected const string OAuthVersion = "1.0";
        protected const string OAuthParameterPrefix = "oauth_";

        // List of know and used oauth parameters' names
        protected const string OAuthConsumerKeyKey = "oauth_consumer_key";
        protected const string OAuthCallbackKey = "oauth_callback";
        protected const string OAuthVersionKey = "oauth_version";
        protected const string OAuthSignatureMethodKey = "oauth_signature_method";
        protected const string OAuthSignatureKey = "oauth_signature";
        protected const string OAuthTimestampKey = "oauth_timestamp";
        protected const string OAuthNonceKey = "oauth_nonce";
        protected const string OAuthTokenKey = "oauth_token";
        protected const string OAuthTokenSecretKey = "oauth_token_secret";

        protected const string Hmacsha1SignatureType = "HMAC-SHA1";
        protected const string PlainTextSignatureType = "PLAINTEXT";
        protected const string Rsasha1SignatureType = "RSA-SHA1";

        protected Random _random = new Random();

        protected static string _unreserved_chars = 
            "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-_.~";

        /// <summary>
        /// Helper function to compute a hash value
        /// </summary>
        /// <param name="hashAlgorithm">The hashing algoirhtm used. If that algorithm needs 
        /// some initialization, like HMAC and its derivatives, they should be initialized 
        /// prior to passing it to this function</param>
        /// <param name="data">The data to hash</param>
        /// <returns>a Base64 string of the hash value</returns>
        private static string ComputeHash(HashAlgorithm hashAlgorithm, string data)
        {
            if (hashAlgorithm == null)
                throw new ArgumentNullException("hashAlgorithm");

            if (string.IsNullOrEmpty(data))
                throw new ArgumentNullException("data");

            var dataBuffer = Encoding.ASCII.GetBytes(data);
            var hashBytes = hashAlgorithm.ComputeHash(dataBuffer);

            return Convert.ToBase64String(hashBytes);
        }

        /// <summary>
        /// Internal function to cut out all non oauth query string parameters 
        /// (all parameters not begining with "oauth_")
        /// </summary>
        /// <param name="parameters">The query string part of the Url</param>
        /// <returns>A list of QueryParameter each containing the parameter name and 
        /// value</returns>
        private static List<QueryParameter> GetQueryParameters(string parameters)
        {
            if (parameters.StartsWith("?"))
                parameters = parameters.Remove(0, 1);

            var result = new List<QueryParameter>();

            if (!string.IsNullOrEmpty(parameters))
            {
                var p = parameters.Split('&');
                foreach (var s in p)
                {
                    if (string.IsNullOrEmpty(s)/* || s.StartsWith(OAuthParameterPrefix)*/) 
                        continue;

                    if (s.IndexOf('=') > -1)
                    {
                        var temp = s.Split('=');
                        result.Add(new QueryParameter(temp[0], temp[1]));
                    }
                    else
                        result.Add(new QueryParameter(s, string.Empty));
                }
            }

            return result;
        }

        /// <summary>
        /// This is a different Url Encode implementation since the default 
        /// .NET one outputs the percent encoding in lower case.
        /// While this is not a problem with the percent encoding spec, 
        /// it is used in upper case throughout OAuth
        /// </summary>
        /// <param name="value">The value to Url encode</param>
        /// <returns>Returns a Url encoded string</returns>
        public static string UrlEncode(string value)
        {
            var result = new StringBuilder();

            foreach (var symbol in value)
            {
                if (_unreserved_chars.IndexOf(symbol) != -1)
                    result.Append(symbol);
                else
                    result.Append('%' + string.Format("{0:X2}", (int)symbol));
            }

            return result.ToString();
        }

        /// <summary>
        /// Normalizes the request parameters according to the spec
        /// </summary>
        /// <param name="parameters">The list of parameters already sorted</param>
        /// <returns>a string representing the normalized parameters</returns>
        protected string NormalizeRequestParameters(IList<QueryParameter> parameters)
        {
            var sb = new StringBuilder();
            QueryParameter p;
            for (var i = 0; i < parameters.Count; i++)
            {
                p = parameters[i];
                sb.AppendFormat("{0}={1}", p._name, p._value);

                if (i < parameters.Count - 1)
                    sb.Append("&");
            }

            return sb.ToString();
        }

        /// <summary>
        /// Generate the signature base that is used to produce the signature
        /// </summary>
        /// <param name="url">The full url that needs to be signed including 
        /// its non OAuth url parameters</param>
        /// <param name="consumerKey">The consumer key</param>        
        /// <param name="token">The token, if available. If not available 
        /// pass null or an empty string</param>
        /// <param name="tokenSecret">The token secret, if available. If not 
        /// available pass null or an empty string</param>
        /// <param name="httpMethod">The http method used. Must be a valid HTTP 
        /// method verb (POST,GET,PUT, etc)</param>
        /// <param name="signatureType">The signature type. To use the default 
        /// values use <see cref="OAuthSignatureType">
        /// OAuthBase.OAuthSignatureType</see>.
        /// </param>
        /// <returns>The signature base</returns>
        
        public string GenerateSignatureBase(Uri url, string consumerKey, string token, 
            string tokenSecret, string httpMethod, string timeStamp, string nonce, 
            string signatureType, out string normalizedUrl, 
            out string normalizedRequestParameters, bool addCallBack)
        {
            if (token == null)
                token = string.Empty;

            //if (tokenSecret == null)
            //    tokenSecret = string.Empty;

            if (string.IsNullOrEmpty(consumerKey))
                throw new ArgumentNullException("consumerKey");

            if (string.IsNullOrEmpty(httpMethod))
                throw new ArgumentNullException("httpMethod");

            if (string.IsNullOrEmpty(signatureType))
                throw new ArgumentNullException("signatureType");

            normalizedUrl = null;
            normalizedRequestParameters = null;

            var parameters = GetQueryParameters(url.Query);
            parameters.Add(new QueryParameter(OAuthVersionKey, OAuthVersion));
            parameters.Add(new QueryParameter(OAuthNonceKey, nonce));
            parameters.Add(new QueryParameter(OAuthTimestampKey, timeStamp));
            parameters.Add(new QueryParameter(OAuthSignatureMethodKey, signatureType));
            parameters.Add(new QueryParameter(OAuthConsumerKeyKey, consumerKey));
            
            //As suggested by Steven
            if (addCallBack)
                parameters.Add(new QueryParameter(OAuthCallbackKey, "oob")); 

            if (!string.IsNullOrEmpty(token))
                parameters.Add(new QueryParameter(OAuthTokenKey, token));

            for (var i = 0; i < parameters.Count; i++)
            {
                var item = parameters[i];
                for (var j = parameters.Count - 1; j > i; j--)
                {
                    if (parameters[j]._name == parameters[i]._name)
                        parameters.RemoveAt(j);
                }

            }

            parameters.Sort(new QueryParameterComparer());

            normalizedUrl = string.Format("{0}://{1}", url.Scheme, url.Host);

            if (!((url.Scheme == "http" && url.Port == 80) || 
                (url.Scheme == "https" && url.Port == 443)))
                normalizedUrl += ":" + url.Port;

            normalizedUrl += url.AbsolutePath;
            normalizedRequestParameters = NormalizeRequestParameters(parameters);

            var signatureBase = new StringBuilder();
            signatureBase.AppendFormat("{0}&", httpMethod.ToUpper());
            signatureBase.AppendFormat("{0}&", UrlEncode(normalizedUrl));
            signatureBase.AppendFormat("{0}", UrlEncode(normalizedRequestParameters));

            return signatureBase.ToString();
        }

        /// <summary>
        /// Generate the signature value based on the given signature base
        /// and hash algorithm
        /// </summary>
        /// <param name="signatureBase">The signature based as produced 
        /// by the GenerateSignatureBase method or by any other means</param>
        /// <param name="hash">The hash algorithm used to perform the hashing. 
        /// If the hashing algorithm requires initialization or a key it 
        /// should be Set prior to calling this method</param>
        /// <returns>A base64 string of the hash value</returns>
        public string GenerateSignatureUsingHash(string signatureBase, HashAlgorithm hash)
        {
            return ComputeHash(hash, signatureBase);
        }

        /// <summary>
        /// Generates a signature using the HMAC-SHA1 algorithm
        /// </summary>		
        /// <param name="url">The full url that needs to be signed including its 
        /// non OAuth url parameters</param>
        /// <param name="consumerKey">The consumer key</param>
        /// <param name="consumerSecret">The consumer seceret</param>
        /// <param name="token">The token, if available. If not available pass 
        /// null or an empty string</param>
        /// <param name="tokenSecret">The token secret, if available. If not 
        /// available pass null or an empty string</param>
        /// <param name="httpMethod">The http method used. Must be a valid HTTP 
        /// method verb (POST,GET,PUT, etc)</param>
        /// <returns>A base64 string of the hash value</returns>
        public string GenerateSignature(Uri url, string consumerKey, string consumerSecret, 
            string token, string tokenSecret, string httpMethod, string timeStamp, 
            string nonce, out string normalizedUrl, out string normalizedRequestParameters, bool addCallBack)
        {
            return GenerateSignature(url, consumerKey, consumerSecret, token, tokenSecret, 
                httpMethod, timeStamp, nonce, OAuthSignatureType.Hmacsha1, out normalizedUrl, 
                out normalizedRequestParameters, addCallBack);
        }

        /// <summary>
        /// Generates a signature using the specified signatureType 
        /// </summary>		
        /// <param name="url">The full url that needs to be signed including 
        /// its non OAuth url parameters</param>
        /// <param name="consumerKey">The consumer key</param>
        /// <param name="consumerSecret">The consumer seceret</param>
        /// <param name="token">The token, if available. If not available pass 
        /// null or an empty string</param>
        /// <param name="tokenSecret">The token secret, if available. If not 
        /// available pass null or an empty string</param>
        /// <param name="httpMethod">The http method used. Must be a valid HTTP 
        /// method verb (POST,GET,PUT, etc)</param>
        /// <param name="signatureType">The type of signature to use</param>
        /// <returns>A base64 string of the hash value</returns>
        public string GenerateSignature(Uri url, string consumerKey, string consumerSecret, 
            string token, string tokenSecret, string httpMethod, string timeStamp, 
            string nonce, OAuthSignatureType signatureType, out string normalizedUrl, 
            out string normalizedRequestParameters, bool addCallBack)
        {
            normalizedUrl = null;
            normalizedRequestParameters = null;

            switch (signatureType)
            {
                case OAuthSignatureType.Plaintext:
                    return HttpUtility.UrlEncode(
                        string.Format("{0}&{1}", consumerSecret, tokenSecret));
                case OAuthSignatureType.Hmacsha1:
                    var signatureBase = GenerateSignatureBase(url, consumerKey, 
                        token, tokenSecret, 
                        httpMethod, timeStamp, nonce, Hmacsha1SignatureType,
                        out normalizedUrl, out normalizedRequestParameters, addCallBack);

                    var hmacsha1 = new HMACSHA1(
                                           Encoding.ASCII.GetBytes(
                                                string.Format("{0}&{1}", 
                                                UrlEncode(consumerSecret),
                                                string.IsNullOrEmpty(tokenSecret) 
                                                    ? "" 
                                                    : UrlEncode(tokenSecret)))
                                       );

                    return GenerateSignatureUsingHash(signatureBase, hmacsha1);
                case OAuthSignatureType.Rsasha1:
                    throw new NotImplementedException();
                default:
                    throw new ArgumentException("Unknown signature type", "signatureType");
            }
        }

        /// <summary>
        /// Generate the timestamp for the signature        
        /// </summary>
        /// <returns></returns>
        public virtual string GenerateTimeStamp()
        {
            // Default implementation of UNIX time of the current UTC time
            var ts = DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, 0));
            return ((long)ts.TotalSeconds).ToString();
        }

        /// <summary>
        /// Generate a nonce (childish giggle)
        /// </summary>
        /// <returns></returns>
        static int _nounceCount;
        public virtual string GenerateNonce()
        {
            // The old version was just a simple implementation 
            // of "a random number between 123400 and 9999999"
            // which was crappy. To avoid duplicates, I'll insert
            // the current time somewhere in between. Ha!
            var n = _random.Next(_random.Next(123400), 123400+_random.Next(9999999)).ToString();
            return n.Insert(_random.Next(n.Length), DateTime.Now.Ticks.ToString()) + (_nounceCount++);
        }
    }
    }
