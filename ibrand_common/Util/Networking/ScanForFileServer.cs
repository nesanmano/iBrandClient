using System;
using System.Net.Sockets;
using NLog;

namespace NomadLogic.Common.Util.Networking
{
    /// <summary>
    /// Cette classe fait une recherche sur une plage d'adresse IP pour trouver un écoutant TCP de fichiers à recevoir.
    /// TODO Cette classe est dépréciée totalement depuis que la recherche par SSDP a été implémentée.
    /// </summary>
    [Obsolete("Scan réseau déprécié. Utiliser le système via SSDP de la classe SsdpUtil.", true)]
    public class ScanForFileServer
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        public ServerFound OnFileServerFound;
        public SearchFinished OnSearchFinished;
        public SetProgressMinMaxDelegate OnProgressMinMax;
        public AddProgressStepDelegate OnProgress;

        private volatile bool _stop;

        /// <summary>
        /// Indique le port sur lequel faire la recherche. Défaut = 3000;
        /// </summary>
        public int PortToScan { get; set; } = 3000;
        /// <summary>
        /// Indique le temps maximal d'attente en milisecondes. pour tester la connexion (time out). Défaut = 200
        /// </summary>
        public int MaxWaitDelay { get; set; } = 200;
        /// <summary>
        /// L'adresse IP (premier octet) minimale sur laquelle tester la connexion. Déafut = 1
        /// </summary>
        public int MinIpAdress { get; set; } = 1;
        /// <summary>
        /// L'adresse IP (premier octet) maximale sur laquelle tester la connexion. Déafut = 255
        /// </summary>
        public int MaxIpAdress { get; set; } = 255;

        /// <summary>
        /// Lance la recherche des adresse qui écoutent des ports de réception de fichiers.
        /// </summary>
        public void Search()
        {
            try
            {
                var ip = NetUtil.GetLocalIpAddress();
                var network = ip.Substring(0, ip.LastIndexOf('.') + 1);

                OnProgressMinMax?.Invoke(MinIpAdress - 1, MaxIpAdress - 1);

                for (var i = MinIpAdress; i < MaxIpAdress; i++)
                {
                    if (_stop)
                        break;
                    var address = network + i;

                    try
                    {
                        if (NetUtil.Ping(address, 1, MaxWaitDelay))
                        {
                            if (IsPortOpen(address, PortToScan, MaxWaitDelay))
                            {

                                var uf = new UploadFileToTcpListener(address, PortToScan) {ConnectTimeout = MaxWaitDelay };
                                if (uf.IsFileServerRunning())
                                {
                                    OnFileServerFound?.Invoke(address);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Error(ex);
                    }

                    OnProgress?.Invoke(1);
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }

            OnSearchFinished?.Invoke();
        }

        private static bool IsPortOpen(string address, int port, int maxDelay)
        {
            var client = new TcpClient();
            var result = client.BeginConnect(address, port, null, null);

            var success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromMilliseconds(maxDelay));

            if (!success)
            {
                return false;
            }

            client.EndConnect(result);
            return true;
        }

        public void StopSearch(object sender, EventArgs e)
        {
            _stop = true;
        }
    }
}
