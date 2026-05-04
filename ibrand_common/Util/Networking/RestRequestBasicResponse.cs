using System.Net;
using System.Net.Http.Headers;

namespace NomadLogic.Common.Util.Networking
{
    public class RestRequestBasicResponse
    {
        public HttpHeaders Headers { get; set; }
        public HttpStatusCode StatusCode { get; set; }
        public int NumberOfAttempts { get; set; } = 0;

        /// <summary>
        /// En cas d'erreur, on stocke le body de façon la plus simple possible.
        /// </summary>
        public string ErrorRawBody { get; set; }
    }
}