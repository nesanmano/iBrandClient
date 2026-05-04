using System;
using System.Net.NetworkInformation;

namespace NomadLogic.Common.Util.Networking
{
    public class NetworkMonitor
    {

        public static bool IsInternetAvailable
        {
            get { return NetworkInterface.GetIsNetworkAvailable() && _CanPingGoogle(); }
        }

        private static bool _CanPingGoogle()
        {
            const int timeout = 1000;
            const string host = "google.com";

            var ping = new Ping();
            var buffer = new byte[32];
            var pingOptions = new PingOptions();

            try
            {
                var reply = ping.Send(host, timeout, buffer, pingOptions);
                return (reply != null && reply.Status == IPStatus.Success);
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
