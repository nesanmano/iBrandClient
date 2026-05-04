using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using NLog;
using NomadLogic.Common.Util.Json;

namespace NomadLogic.Common.Util.Networking
{
    /// <summary>
    /// Classe utilitaire pour gérer les configurations SSDP et de communication entre client/serveur TCP.
    /// </summary>
    [PublicAPI]
    public abstract class SsdpUtil
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private static readonly HashSet<TcpServiceLocation> previousFoundPrintstations = new HashSet<TcpServiceLocation>();
        private static readonly HashSet<TcpServiceLocation> filePrintstations = new HashSet<TcpServiceLocation>();
        private const string PRINTSTATION_SSDP_HEADER = "nl-psl-";
        private const string CAMERA_RING_SSDP_HEADER = "nl-crl-";
        private const string CUSTOM_CAMERA_RING_SERVER_INFO_FILE = "customCameraRingServerInfo.json";
        private const string CUSTOM_PRINTSTATION_SERVER_INFO_FILE = "customPrintstationServerInfo.json";

        static SsdpUtil()
        {
            var file = new FileInfo(DirectoryUtil.GetLocalApplicationDir() + CUSTOM_PRINTSTATION_SERVER_INFO_FILE);

            if (!File.Exists(file.FullName))
            {
                return;
            }

            var infos = JsonUtil.ReadJsonFile<List<TcpServiceLocation>>(file);
            foreach (var ps in infos)
            {
                filePrintstations.Add(ps);
            }
        }

        /// <summary>
        /// Remplace la liste d'adresses de serveurs Printstations actuellement utilisée.
        /// </summary>
        /// <param name="addresses"></param>
        public static void UpdateCustomPrintstations(List<string> addresses)
        {
            lock (filePrintstations)
            {
                filePrintstations.Clear();
                if (addresses != null && addresses.Any())
                {
                    var tmpAdd = (from ad in addresses
                                  where StringUtil.IsValidIPAddressWithPort(ad, true)
                                  select ad.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries)
                        into split
                                  select new TcpServiceLocation
                                  {
                                      Port = split[1],
                                      IpAddress = split[0],
                                      DateDetected = DateTime.Now,
                                      ServiceType = TcpServiceType.Printstation
                                  }).ToList();

                    foreach (var add in tmpAdd)
                    {
                        filePrintstations.Add(add);
                    }
                }

                var file = new FileInfo(DirectoryUtil.GetLocalApplicationDir() + CUSTOM_PRINTSTATION_SERVER_INFO_FILE);
                JsonUtil.WriteJsonFile(file, filePrintstations);
            }
        }

        /// <summary>
        /// Retourne la liste actuelle des addresses qui sont configurées.
        /// </summary>
        /// <returns>Une liste d'addresses IP avec port des printstation personnalisées.</returns>
        public static List<string> GetCurrentCustomPrintstations()
        {
            lock (filePrintstations)
            {
                return filePrintstations.Select(p => p.GetIpAddress()).ToList();
            }
        }

        /// <summary>
        /// Groupement complet de toutes les informations pertinentes pour un service de réception de fichiers Printstation.
        /// Quand cette structure est complète et accessible, les services à l'intérieur sont déjà actifs.
        /// Il faut s'assurer quand les services ne sont plus requis d'appeller la méthode StopAllServices() pour tout stopper
        /// et libérer la mémoire.
        /// </summary>
        public struct CompleteFileReceptionProvider
        {
            public ListenForFilesTask TcpFileListenerService { get; internal set; }
            internal CancellationTokenSource CancelTokenSource { get; set; }
            internal Task TcpFileListenerServiceTask { get; set; }
            public SsdpServiceProvider BroadcastSsdpService { get; internal set; }
            public TcpServiceType ServiceType { get; internal set; }

            /// <summary>
            /// Arrête tous les services et met leur références à nul.
            /// </summary>
            public void StopAllService()
            {
                if (TcpFileListenerServiceTask != null)
                {
                    CancelTokenSource.Cancel();
                    try
                    {
                        Task.WaitAll(new[] { TcpFileListenerServiceTask }, 2000);
                    }
                    catch (AggregateException e)
                    {
                        log.Error(e, "Error during TcpListener task stop.");
                    }
                }

                CancelTokenSource = null;

                if (TcpFileListenerService != null)
                {
                    TcpFileListenerService.OnFileReceptionCallback = null;
                    TcpFileListenerService = null;
                    TcpFileListenerServiceTask = null;
                }

                if (BroadcastSsdpService != null && BroadcastSsdpService.IsRunning)
                {
                    BroadcastSsdpService.StopPublishingSsdpService();
                }
                BroadcastSsdpService = null;
            }
        }

        /// <summary>
        /// Structuire pour obtenir les informations de location du réseau local d'un service d'envoi de fichier Tcp.
        /// </summary>
        public struct TcpServiceLocation
        {
            /// <summary>
            /// Adresse IP (sans port)
            /// </summary>
            public string IpAddress { get; set; }
            /// <summary>
            /// Le port de communication TCP.
            /// </summary>
            public string Port { get; set; }
            /// <summary>
            /// Le type de sercie.
            /// </summary>
            [JsonConverter(typeof(StringEnumConverter))]
            public TcpServiceType ServiceType { get; set; }
            /// <summary>
            /// La date détectée.
            /// </summary>
            public DateTime DateDetected { get; set; }

            /// <inheritdoc/>
            [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
            public override int GetHashCode()
            {
                return (IpAddress + ":" + Port).GetHashCode();
            }

            /// <summary>
            /// Un objet de ce type est égal si tous ses champs hormis la date sont égaux.
            /// </summary>
            /// <param name="obj">L'instance avec qui comparée.</param>
            /// <returns>TRUE si les deux instances sont vraies.</returns>
            public override bool Equals(object obj)
            {
                if (!(obj is TcpServiceLocation)) return false;

                var arg = (TcpServiceLocation)obj;
                return IpAddress.Equals(arg.IpAddress) &&
                       Port == arg.Port &&
                       ServiceType == arg.ServiceType;
            }

            /// <summary>
            /// Retourne l'adresse avec port sous forme de string valide pour une adresse IP.
            /// </summary>
            /// <returns></returns>
            public string GetIpAddress()
            {
                return IpAddress + ":" + Port;
            }

            /// <inheritdoc/>
            public override string ToString()
            {
                return $"[Ip={IpAddress}:{Port},type={ServiceType},date={DateDetected.ToShortDateString()}]";
            }
        }

        /// <summary>
        /// Indique le type de fournisseur de service.
        /// </summary>
        public enum TcpServiceType
        {
            /// <summary>
            /// Service Printstation
            /// </summary>
            Printstation,
            /// <summary>
            /// Service camera ring
            /// </summary>
            CameraRing
        }

        /// <summary>
        /// Créer tous les services utiles pour créer un serveur de réception de fichiers destinés à un PrintStation.
        /// </summary>
        /// <param name="receiveDirectory">Le répertoire où stocker les fichiers reçus.</param>
        /// <param name="fileCallbackAction">L'action à appeller pour chaque fichier reçu avec succès.</param>
        /// <returns>Uns structure contenant tous les services déjà initialisés et lancés.</returns>
        public static CompleteFileReceptionProvider PrepareAndLaunchPrintstationImageListener(DirectoryInfo receiveDirectory, Action<string> fileCallbackAction)
        {
            return PrepareServices(receiveDirectory, fileCallbackAction, PRINTSTATION_SSDP_HEADER, TcpServiceType.Printstation);
        }

        /// <summary>
        /// Créer tous les services utiles pour créer un serveur de réception de fichiers destinés à un Camera Ring.
        /// </summary>
        /// <param name="receiveDirectory">Le répertoire où stocker les fichiers reçus.</param>
        /// <param name="fileCallbackAction">L'action à appeller pour chaque fichier reçu avec succès.</param>
        /// <returns>Uns structure contenant tous les services déjà initialisés et lancés.</returns>
        public static CompleteFileReceptionProvider PrepareAndLaunchCameraRingImageListener(DirectoryInfo receiveDirectory, Action<string> fileCallbackAction)
        {
            return PrepareServices(receiveDirectory, fileCallbackAction, CAMERA_RING_SSDP_HEADER, TcpServiceType.CameraRing);
        }

        private static CompleteFileReceptionProvider PrepareServices(DirectoryInfo receiveDirectory, Action<string> fileCallbackAction, string header, TcpServiceType serviceType)
        {
            var ip = NetUtil.GetLocalIpAddress();
            var freePort = NetUtil.GetAvailablePort(3000);

            // Création et démmarage du service d'écoute de fichier.
            var tcpService = new ListenForFilesTask(freePort)
            {
                ReceptionDirectory = receiveDirectory,
                OnFileReceptionCallback = fileCallbackAction
            };

            var cts = new CancellationTokenSource();
            var task = Task.Run(() =>
            {
                tcpService.DoWork(cts.Token);
            }, cts.Token);

            // Création du service d'annonce SSDP pour trouver ce nouveau service sur le réseau local
            var provider = new SsdpServiceProvider
            {
                CacheTime = TimeSpan.FromMinutes(5),
                Message01 = ip.Replace('.', '-'),
                Message02 = freePort.ToString(),
                ServiceId = header + GenerateUniqueId.GetId()
            };

            provider.InitAndStart();

            // Création de la structure de retour.
            var services = new CompleteFileReceptionProvider
            {
                TcpFileListenerService = tcpService,
                CancelTokenSource = cts,
                TcpFileListenerServiceTask = task,
                BroadcastSsdpService = provider,
                ServiceType = serviceType
            };

            return services;
        }

        /// <summary>
        /// Cette méthode cherche via SSDP toutes les machine sur le réseau local qui sont des écoutants du type de service donné
        /// en paramètre.
        /// </summary>
        /// <param name="serviceToFind">Le type de service que l'on cherche.</param>
        /// <param name="nbrSecondsToSearch">Le nombre de secondes pour faire des recherches.</param>
        /// <param name="ct">Jeton de cancellation.</param>
        /// <returns>Une liste non-nulle de tous les services trouvés sur le réseau local. Peut être vide.</returns>
        public static List<TcpServiceLocation> FindAllListeners(TcpServiceType serviceToFind, int nbrSecondsToSearch = 5, CancellationToken? ct = null)
        {
            string idToCheck;
            switch (serviceToFind)
            {
                case TcpServiceType.Printstation:
                    idToCheck = PRINTSTATION_SSDP_HEADER;
                    break;
                case TcpServiceType.CameraRing:
                    idToCheck = CAMERA_RING_SSDP_HEADER;
                    break;
                default:
                    throw new Exception("Unknown Tcp Service type. Please add it to the logic: " + serviceToFind);
            }

            var locator = new SsdpServiceLocator
            {
                IdToCheck = idToCheck,
                TimeOut = TimeSpan.FromSeconds(nbrSecondsToSearch)
            };

            var foundServices = locator.SearchForServicesSync(ct);

            return foundServices.Select(serv => new TcpServiceLocation
            {
                ServiceType = serviceToFind,
                IpAddress = serv.Message01.Replace('-', '.'),
                Port = serv.Message02,
                DateDetected = DateTime.Now
            }).ToList();
        }

        /// <summary>
        /// Classe de résultat pour un envoi de fichiers vers les Printstations.
        /// </summary>
        public class SsdpPrintstationSendResult
        {
            /// <summary>
            /// Constrcuteur principal.
            /// </summary>
            public SsdpPrintstationSendResult()
            {
                SentPrintstations = new List<TcpServiceLocation>();
            }

            /// <summary>
            /// Nombre d'envoi à des Printstation.
            /// </summary>
            public int NbrRequest { get; set; }

            /// <summary>
            /// Liste de toutes les adresses qui ont réussies avec succès l'envoi.
            /// </summary>
            public List<TcpServiceLocation> SentPrintstations { get; }
        }

        /// <summary>
        /// Cette méthode tente d'envoyer le ou les fichiers d'image sur le réseau local.
        /// Cette méthode tente de garder en mémoire les printstations détectés. Si c'est le premier envoi, on va faire une détection SSDP pour trouver tous
        /// les printstations disponibles.
        /// Si la méthode a conservé une liste, elle va tenter de la réutiliser. Si une erreur se produit sur une adresse en mémoire, on va tenter un autre
        /// scan pour mettre à jour les printstations détectées, puis envoyer les fichiers aux addresses qui n'ont pas encore reçues les fichiers.
        /// </summary>
        /// <param name="ct">Jeton de cancellation.</param>
        /// <param name="filesToSend">Les fichiers à envoyer sur chaque printstation.</param>
        /// <returns>Une tâche contenant le résultat éventuel de l'ensemble de l'opération.</returns>
        public static async Task<SsdpPrintstationSendResult> TrySendFilesToPrintstations(CancellationToken? ct, params string[] filesToSend)
        {
            return await TrySendFilesToPrintstations(0, null, ct, filesToSend);
        }

        /// <summary>
        /// Cette méthode tente d'envoyer le ou les fichiers d'image sur le réseau local.
        /// Cette méthode tente de garder en mémoire les printstations détectés. Si c'est le premier envoi, on va faire une détection SSDP pour trouver tous
        /// les printstations disponibles.
        /// Si la méthode a conservé une liste, elle va tenter de la réutiliser. Si une erreur se produit sur une adresse en mémoire, on va tenter un autre
        /// scan pour mettre à jour les printstations détectées, puis envoyer les fichiers aux addresses qui n'ont pas encore reçues les fichiers.
        /// Le paramètre "minMachineToSend" va indiquer que l'on doit tenter un rescan si les printstations détectées ne sont pas en nombre suffisant. Même si
        /// le nouveau nombre est inférieur à celui voulu, on va tout de même envoyer les fichiers aux printstation détectés et retourner le résultat.
        /// La liste d'adresse à ignorer est là pour indiquer que si l'adresse est présente dans la liste, on n'envoi pas le fichier. On ne considère que les
        /// adresses ip: s'il y a un port dans l'adresse, il sera ignoré.
        /// </summary>
        /// <param name="minMachineToSend">Le nombre minimal d'appareil printstation visé.</param>
        /// <param name="ipAddressesToSkip">Les adresses IP à ignorer pour l'envoi (utile en cas de ré-envoi pour éviter les doubles).</param>
        /// <param name="ct">Jeton de cancellation.</param>
        /// <param name="filesToSend">La liste des fichiers à envoyer à chaque adresse.</param>
        /// <returns>Une tâche contenant le résultat éventuel de l'ensemble de l'opération.</returns>
        [SuppressMessage("ReSharper", "InconsistentlySynchronizedField")]
        public static async Task<SsdpPrintstationSendResult> TrySendFilesToPrintstations(int minMachineToSend, List<string> ipAddressesToSkip, 
            CancellationToken? ct, params string[] filesToSend)
        {
            return await Task.Run(() =>
            {
                var result = new SsdpPrintstationSendResult();
                
                if (previousFoundPrintstations.Count + filePrintstations.Count < minMachineToSend - (ipAddressesToSkip?.Count ?? 0))
                {
                    lock (previousFoundPrintstations)
                    {
                        previousFoundPrintstations.Clear();
                        foreach (var listeners in FindAllListeners(TcpServiceType.Printstation))
                        {
                            previousFoundPrintstations.Add(listeners);
                        }
                    }
                }

                lock (previousFoundPrintstations)
                {
                    if (!previousFoundPrintstations.Any() && !filePrintstations.Any()) return result;
                }

                var skipTable = new List<string>();

                // On enlève le port des adresses valides.
                if (ipAddressesToSkip != null && ipAddressesToSkip.Any())
                {
                    foreach (var ip in ipAddressesToSkip)
                    {
                        if (ip == null) continue;
                        try
                        {
                            if (!StringUtil.IsValidIPAddressWithPort(ip)) continue;
                            var split = ip.Split(new[] { ':' }, 1);
                            if (split.Any()) skipTable.Add(split.First());
                        }
                        catch (Exception e)
                        {
                            log.Warn(e, "Error in addresses to skip. Bad formatting when stripping the port.");
                        }
                    }
                }

                DoSendToPrintstations(filesToSend, result, ct, skipTable, out var errorDetected);
                if (!errorDetected && result.SentPrintstations.Count >= minMachineToSend) return result;

                if (ct?.IsCancellationRequested ?? false) return result;

                if (previousFoundPrintstations.Count < minMachineToSend)
                {
                    lock (previousFoundPrintstations)
                    {
                        previousFoundPrintstations.Clear();
                        foreach (var listeners in FindAllListeners(TcpServiceType.Printstation, 5, ct))
                        {
                            previousFoundPrintstations.Add(listeners);
                        }
                    }
                }
                DoSendToPrintstations(filesToSend, result, ct, skipTable, out errorDetected);

                return result;
            });
        }

        private static void DoSendToPrintstations(string[] filesToSend, SsdpPrintstationSendResult result, CancellationToken? ct, List<string> skipTable, out bool errorDetected)
        {
            // Remise à zéro du compteur, en prenant comptes des envois réussis.
            result.NbrRequest = result.SentPrintstations.Count;

            var tempList = new HashSet<TcpServiceLocation>();
            lock (previousFoundPrintstations)
            {
                lock (filePrintstations)
                {
                    foreach (var ps in previousFoundPrintstations)
                    {
                        tempList.Add(ps);
                    }
                    foreach (var ps in filePrintstations)
                    {
                        tempList.Add(ps);
                    }
                }
            }

            tempList.RemoveWhere(ps => skipTable.Contains(ps.IpAddress));

            var taskList = tempList.Select(t1 => Task.Run(() =>
            {
                var sendResult = new SendToPrintstationResult
                {
                    ErrorDetected = false, TcpService = t1, NbrFiles = filesToSend.Length, Cancelled = false
                };
                var uploadFile = new UploadFileToTcpListener(t1.IpAddress, int.Parse(t1.Port));
                foreach (var file in filesToSend)
                {
                    if (ct?.IsCancellationRequested ?? false) break;

                    try
                    {
                        sendResult.ErrorDetected |= uploadFile.Go(file, ct);
                    }
                    catch (Exception e)
                    {
                        log.Warn(e, "Could not send to printsation: {0}:{1}", t1.IpAddress, t1.Port);
                        return sendResult;
                    }
                }

                sendResult.Cancelled = ct?.IsCancellationRequested ?? false;

                return sendResult;
            })).ToList();

            try
            {
                // ReSharper disable once CoVariantArrayConversion
                if (ct == null)
                {
                    Task.WaitAll(taskList.ToArray());
                }
                else
                {
                    // ReSharper disable once CoVariantArrayConversion
                    Task.WaitAll(taskList.ToArray(), ct.Value);
                }
            }
            catch (Exception e)
            {
                log.Error(e);
            }

            result.NbrRequest = taskList.Count;

            errorDetected = taskList.Any(t => t.Result.ErrorDetected || t.Result.Cancelled);
            lock (result.SentPrintstations)
            {
                foreach (var t in taskList)
                {
                    var res = t.Result;
                    if (res.Cancelled || res.ErrorDetected) continue;

                    result.SentPrintstations.Add(res.TcpService);
                    skipTable.Add(res.TcpService.IpAddress);
                }
            }
        }

        [PublicAPI]
        private struct SendToPrintstationResult
        {
            public TcpServiceLocation TcpService { get; set; }
            public bool ErrorDetected { get; set; }
            public int NbrFiles { get; set; }
            public bool Cancelled { get; set; }
        }

        /// <summary>
        /// Assigne une adresse précise pour trouver un serveur de Camera Ring sur le réseau local.
        /// On sauvegarde cette adresse dans un fichier et on retourne une structure d'information sur le serveur à chercher.
        /// On ne fait pas de tests sur la disponibilité de cette addresse, donc on peut utiliser cette méthode sans avoir
        /// d'accès réseau.
        /// Donner une valeur nulle va enlever cette adresse de l'information locale.
        /// </summary>
        /// <param name="ipAddress">L'adresse IP, avec port, sous forme "192.168..123.210:40012"</param>
        /// <returns>null si on a pas d'adresse spécifique, sinon une référence vers une structure d'info.</returns>
        public static TcpServiceLocation? SetLocalCameraRingServerAddress(string ipAddress)
        {
            if (string.IsNullOrEmpty(ipAddress))
            {
                var pathToDestroy = DirectoryUtil.GetLocalApplicationDir() + CUSTOM_CAMERA_RING_SERVER_INFO_FILE;
                if (File.Exists(pathToDestroy))
                {
                    File.Delete(pathToDestroy);
                }
                return null;
            }

            if (!StringUtil.IsValidIPAddressWithPort(ipAddress, true)) return null;

            var split = ipAddress.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);

            TcpServiceLocation? res = new TcpServiceLocation
            {
                DateDetected = DateTime.Now,
                IpAddress = split[0],
                Port = split[1],
                ServiceType = TcpServiceType.CameraRing
            };

            var path = new FileInfo(DirectoryUtil.GetLocalApplicationDir() + CUSTOM_CAMERA_RING_SERVER_INFO_FILE);
            var writeRes = JsonUtil.WriteJsonFile(path, res.Value);

            return writeRes ? res : null;
        }

        /// <summary>
        /// Obtient l'adresse personnalisé écrite sur disque. Si retourne null, alors il n'y a pas
        /// </summary>
        /// <returns></returns>
        public static TcpServiceLocation? GetLocalCameraRingServerAddress()
        {
            var path = new FileInfo(DirectoryUtil.GetLocalApplicationDir() + CUSTOM_CAMERA_RING_SERVER_INFO_FILE);

            if (!File.Exists(path.FullName)) return null;

            try
            {
                var info = JsonUtil.ReadJsonFile<TcpServiceLocation>(path);

                if (info.ServiceType != TcpServiceType.CameraRing) return null; // Vérifie que la structure n'est pas vide.
                return info;
            }
            catch (Exception e)
            {
                log.Error(e, "Could not read custom json file for camera ring server data: {0}", path);
            }
            return null;
        }
    }
}