using System.Collections.Generic;
using System.Linq;
using System.Net.Configuration;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace NomadLogic.Common.Util.Networking
{
    /// <summary>
    /// Méthode génériques pour obtenir des informations de base sur l'ordinateur local, ou faire des commandes réseau simples.
    /// </summary>
    [PublicAPI]
    public class NetUtil
    {
        [DllImport("wininet.dll")]  
        private static extern bool InternetGetConnectedState( out int description, int reservedValue ) ;  

        /// <summary>
        /// Indique si l'interface réseau est disponible. AKA. Si un câble réseau est branché.
        /// </summary>
        /// <returns></returns>
        public static bool IsConnectedToInternet( )  
        {
            return InternetGetConnectedState( out _, 0 ) ;  
        } 

        /// <summary>
        /// Obtient l'addresse IP locale de cet ordinateur.
        /// </summary>
        /// <returns>L'adresse IP sous forme "XXX.XXX.XXX.XXX". Si rien trouvé, l'adresse sera vide mais non-nulle.</returns>
        public static string GetLocalIpAddress()
        {
            var localIp = "";
            //var host = Dns.GetHostEntry(Dns.GetHostName());
            var adapters = NetworkInterface.GetAllNetworkInterfaces();
            foreach (var adapter in adapters)
            {
                if (adapter.GetIPProperties().GatewayAddresses.Count > 0)// Si vrai, il est connecte au reseau via cet adapteur
                {
                    foreach (var ip in adapter.GetIPProperties().UnicastAddresses)
                    {
                        if (ip.Address.AddressFamily == AddressFamily.InterNetwork && !ip.Address.ToString().Contains("192.168.122"))  // peut importe l<ordre des adapteur on veut eviter de recuperer l'ip de l<adapteur connecte a la camera
                        {
                            localIp = ip.Address.ToString();
                            return localIp;

                        }
                            
                    }
                    

                } 

            }

            /*foreach (var ip in host.AddressList.Where(ip => ip.AddressFamily == AddressFamily.InterNetwork))
            {
                localIp = ip.ToString();
                break;
            }*/
            return localIp;
        }

        /// <summary>
        /// Envoi un Ping pour savoir si un ordinateur se trouve derrière une addresse web particulière.
        /// </summary>
        /// <param name="host">L'adresse de la machine.</param>
        /// <param name="attempts">Le nombre de tentatives.</param>
        /// <param name="timeout">L'attente maximale en chaque tentative.</param>
        /// <returns>TRUE si au moins une réponse positive est obtenue.</returns>
        public static bool Ping(string host, int attempts, int timeout)
        {
            var ping = new Ping();

            for (var i = 0; i < attempts; i++)
            {
                try
                {
                    var pingReply = ping.Send(host, timeout);

                    // If there is a successful ping then return true.
                    if (pingReply != null &&
                        pingReply.Status == IPStatus.Success)
                        return true;
                }
                catch
                {
                    // Do nothing and let it try again until the attempts are exausted.
                    // Exceptions are thrown for normal ping failurs like address lookup
                    // failed.  For this reason we are supressing errors.
                }
            }

            // Return false if we can't successfully ping the server after several attempts.
            return false;
        }

        /// <summary>
        /// Obtient le premier port libre à partir d'un numéro particulier.
        /// </summary>
        /// <param name="startingPort">Le numéro de port le plus bas voulu. Par défaut 0.</param>
        /// <returns>Le premier port libre. Si le retour est zéro, aucun port n'a été détecté libre à partir du numéro donné en paramètre.</returns>
        public static ushort GetAvailablePort(ushort startingPort = 0)
        {
            var portArray = new SortedSet<ushort>();

            var properties = IPGlobalProperties.GetIPGlobalProperties();

            //getting active connections
            var connections = properties.GetActiveTcpConnections();
            foreach (var port in connections.Select(n => n.LocalEndPoint.Port).Where(n => n >= startingPort))
            {
                portArray.Add((ushort)port);
            }

            //getting active tcp listners - WCF service listening in tcp
            var endPoints = properties.GetActiveTcpListeners();
            foreach (var port in endPoints.Select(n => n.Port).Where(n => n >= startingPort))
            {
                portArray.Add((ushort)port);
            }

            //getting active udp listeners
            endPoints = properties.GetActiveUdpListeners();
            foreach (var port in endPoints.Select(n => n.Port).Where(n => n >= startingPort))
            {
                portArray.Add((ushort)port);
            }

            for (var i = startingPort; i < ushort.MaxValue; i++)
                if (!portArray.Contains(i))
                    return i;

            return 0;
        }

        /// Permet de mettre à jour l'usage du parsing de Header Http
        /// https://stackoverflow.com/questions/8424144/how-to-set-useunsafeheaderparsing-in-code
        public static bool ToggleAllowUnsafeHeaderParsing(bool enable)
        {
            // Enable/disable useUnsafeHeaderParsing.
            // See http://o2platform.wordpress.com/2010/10/20/dealing-with-the-server-committed-a-protocol-violation-sectionresponsestatusline/
            //Get the assembly that contains the internal class
            var assembly = Assembly.GetAssembly(typeof(SettingsSection));
            if (assembly == null) return false;
            //Use the assembly in order to get the internal type for the internal class
            var settingsSectionType = assembly.GetType("System.Net.Configuration.SettingsSectionInternal");
            if (settingsSectionType == null) return false;
            //Use the internal static property to get an instance of the internal settings class.
            //If the static instance isn't created already invoking the property will create it for us.
            var anInstance = settingsSectionType.InvokeMember("Section", BindingFlags.Static | BindingFlags.GetProperty | BindingFlags.NonPublic, null, null, new object[] { });
            if (anInstance == null) return false;
            //Locate the private bool field that tells the framework if unsafe header parsing is allowed
            var aUseUnsafeHeaderParsing = settingsSectionType.GetField("useUnsafeHeaderParsing", BindingFlags.NonPublic | BindingFlags.Instance);
            if (aUseUnsafeHeaderParsing == null) return false;
            aUseUnsafeHeaderParsing.SetValue(anInstance, enable);
            return true;
        }
    }
}
