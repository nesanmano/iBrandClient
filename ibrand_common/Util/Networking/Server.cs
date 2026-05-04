using System;

namespace NomadLogic.Common.Util.Networking
{
    [Obsolete]
    public struct Server
    {
        public Server(string adr, string webApp, string streamPort, string webPort)
        {
            Address = adr;
            WebApp = webApp;
            StreamPort = streamPort;
            WebPort = webPort;
        }

        public string Address { get; }

        public string WebApp { get; }
        
        public string StreamPort { get; }

        public string WebPort { get; }
    }
}
