using System.Net;
using JetBrains.Annotations;

namespace NomadLogic.Common.Util.Networking.Ftp
{
    [PublicAPI]
    public class WebRequestSetup
    {
        private NetworkCredential _nc;

        public WebRequestSetup(string username, string password, string groupname = null, int? connectionLimit = 2, int timeout = 5000)
        {
            Username = username;
            Password = password;
            GroupName = groupname;

            if (connectionLimit.HasValue)
            {
                var val = connectionLimit.Value;
                if (val == 2 || val == 4 || val == 8)
                {
                    ConnectionLimit = val;
                }
                else
                {
                    ConnectionLimit = 2;
                }
            }
            else
            {
                ConnectionLimit = 2;
            }

            TimeOut = timeout;
        }

        public int TimeOut { get; }

        public int ConnectionLimit { get; }

        public string GroupName { get; }

        public string Password { get;  }

        public string Username { get; }

        public ICredentials Credentials => _nc ?? (_nc = new NetworkCredential(Username, Password));
    }
}