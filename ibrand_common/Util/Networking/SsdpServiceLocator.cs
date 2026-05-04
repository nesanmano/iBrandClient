using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Rssdp;

namespace NomadLogic.Common.Util.Networking
{
    /// <summary>
    /// Méthode pour découvrir des services sur le réseau par SSDP
    /// https://github.com/Yortw/RSSDP
    /// </summary>
    public class SsdpServiceLocator
    {
        public struct SsdpServiceLocatorResult
        {
            internal SsdpServiceLocatorResult(string m1, string m2)
            {
                Message01 = m1;
                Message02 = m2;
            }

            public string Message01 { get;}
            public string Message02 { get;}
        }

        /// <summary>
        /// Effectue la recherche. Méthode asynchrone.
        /// </summary>
        /// <returns>La tâche d'exécution.</returns>
        public async Task<List<SsdpServiceLocatorResult>> SearchForServicesAsync(CancellationToken? ct)
        {
            var result = new List<SsdpServiceLocatorResult>();
            using (var deviceLocator = new SsdpDeviceLocator())
            {
                var foundDevices = await deviceLocator.SearchAsync(TimeOut);

                var discoveredSsdpDevices = foundDevices as DiscoveredSsdpDevice[] ?? foundDevices.ToArray();
                var startSequence = $"uuid:{IdToCheck}";
                var devs = discoveredSsdpDevices.Where(r => r.Usn.StartsWith(startSequence) && r.Usn.Contains("::urn:")).ToList();
                result.AddRange(devs.Select(device => device.Usn.Split(':')).Select(split => new SsdpServiceLocatorResult(split[4], split[6])));
            }

            return result;
        }

        /// <summary>
        /// Effectue la recherche de façon synchrone.
        /// </summary>
        /// <returns>La liste trouvée.</returns>
        public List<SsdpServiceLocatorResult> SearchForServicesSync(CancellationToken? ct)
        {
            Func<Task<List<SsdpServiceLocatorResult>>> result = async () => await SearchForServicesAsync(ct);
            var task = result();
            task.Wait();
            return task.Result;
        }

        /// <summary>
        /// l'identifiant du message à chercher.
        /// </summary>
        public string IdToCheck { get; set; }

        /// <summary>
        /// C'est le temps total que va durer la recherche. Trop peu et on n'aura pas le temps de tout trouver, trop et on va attendre pour rien.
        /// </summary>
        public TimeSpan TimeOut { get; set; } = TimeSpan.FromMilliseconds(2500);
    }
}
