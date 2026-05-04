using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using NomadLogic.Common.Util.Net;

namespace NomadLogic.iBrand.CameraRingClient
{
    [PublicAPI]
    public class CameraRingNetworkManager : IDisposable
    {
        private CancellationTokenSource _cts;
        private Task _sendTask;
        private readonly object _lock = new object();
        private volatile bool _started;
        private SsdpUtil.TcpServiceLocation? _cameraRingServer;

        /// <summary>
        /// Constructeur principal.
        /// </summary>
        public CameraRingNetworkManager()
        {
            GetCustomAddress = SsdpUtil.GetLocalCameraRingServerAddress();
        }

        /// <summary>
        /// Démarre le cycle de vie de ce manager.
        /// </summary>
        public void Start()
        {
            if (_started) return;
            
            lock (_lock)
            {
                _sendTask = null;
                _cts = null;
                IsServerDetected = false;
                UpdateStatus?.Invoke(NetworkStatusEnum.Searching, "Initializing");
                _started = true;

                _cts = new CancellationTokenSource();
                _sendTask = new Task(ConnectToServer, _cts.Token, TaskCreationOptions.AttachedToParent);
                _sendTask.Start();
            }
        }

        private void ConnectToServer()
        {
            if (_cameraRingServer == null)
            {
                if (GetCustomAddress == null)
                {
                    var list = SsdpUtil.FindAllListeners(SsdpUtil.TcpServiceType.CameraRing);
                    if (!list.Any())
                    {
                        ReactOnBrokenConnection();
                        IsServerDetected = false;
                        UpdateStatus?.Invoke(NetworkStatusEnum.Searching, "No server detected. Searching.");
                        return;
                    }
                    _cameraRingServer = list.First();
                }
                else
                {
                    _cameraRingServer = GetCustomAddress;
                }
            }

            // On fait un check pour voir si le serveur est vivant.
            var uf = new UploadFileToTcpListener(ServerAddress, ServerPort) { ConnectTimeout = 500 };
            if (!uf.IsFileServerRunning())
            {
                IsServerDetected = false;
                UpdateStatus?.Invoke(NetworkStatusEnum.Error, "Server did not react to connection attempt. Reconnecting in progress.");
                _cts = null;
                _sendTask = null;
                ReactOnBrokenConnection();
            }
            else
            {
                _cts = null;
                _sendTask = null;
                IsServerDetected = true;
                UpdateStatus?.Invoke(NetworkStatusEnum.Connected, "Waiting for files to send.");
            }
        }

        /// <summary>
        /// Stope tout et se libère tous ses objets.
        /// </summary>
        public void Stop()
        {
            if (!_started) return;

            lock (_lock)
            {
                _cameraRingServer = null;

                if (_sendTask != null)
                {
                    _cts?.Cancel();

                    try
                    {
                        Task.WaitAny(_sendTask);
                    }
                    catch (AggregateException)
                    {
                        // die nice
                    }
                    finally
                    {
                        _cts = null;
                        _sendTask = null;
                    }
                }

                _started = false;
            }
        }

        ~CameraRingNetworkManager()
        {
            Stop();
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Stop();
        }

        /// <summary>
        /// Envoi un fichier vers le serveur.
        /// </summary>
        /// <param name="fileName">Le fichier à envoyer.</param>
        /// <param name="onSuccessCallback">Si succès, invoque cette action.</param>
        /// <param name="onErrorCallback">Si erreur, invoque cette action.</param>
        /// <param name="progressCallback">Pour chaque étape de progrès, on met à jour via cette action.</param>
        /// <param name="initCallback">Action invoquée lors de l'initialisation.</param>
        public void SendFileToServer(string fileName, Action onSuccessCallback, Action<Exception> onErrorCallback, Action<int> progressCallback, Action<int, int> initCallback)
        {
            lock (_lock)
            {
                if (!IsServerDetected)
                {
                    onErrorCallback?.Invoke(new Exception("No reception server detected. Cannot send files."));
                }
                
                _cts = new CancellationTokenSource();
                _sendTask = new Task(() =>
                {
                    try
                    {
                        var uftl = new UploadFileToTcpListener(ServerAddress, ServerPort)
                        {
                            ConnectTimeout = 500,
                            ReadTimeout = 3000,
                            SendTimeout = 3000,
                            OnSendProgress = totalBytes =>
                            {
                                progressCallback((int) (totalBytes / 1024.0f));
                            },
                            OnInitBeforeSend = maxBytes =>
                            {
                                initCallback(0, (int) (maxBytes / 1024.0f));
                            }
                        };

                        if (!uftl.IsFileServerRunning())
                        {
                            ReactOnBrokenConnection();
                            onErrorCallback?.Invoke(
                                new Exception(
                                    $"File server is not running on following address: {ServerAddress}:{ServerPort}"));
                        }
                        else
                        {
                            if (uftl.Go(fileName, _cts.Token))
                            {
                                onErrorCallback?.Invoke(new Exception("Could not send file to server."));
                            }
                            else
                            {
                                onSuccessCallback?.Invoke();
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        onErrorCallback?.Invoke(e);
                    }
                    finally
                    {
                        lock (_lock)
                        {
                            _sendTask = null;
                            _cts = null;
                        }
                    }
                }, _cts.Token, TaskCreationOptions.AttachedToParent);
                _sendTask.Start();
            }
        }

        /// <summary>
        /// Le port du serveur présentement joint. Zéro si aucune connexion.
        /// </summary>
        public int ServerPort => _cameraRingServer.HasValue ? int.Parse(_cameraRingServer.Value.Port) : 0;

        /// <summary>
        /// L'adresse IP du serveur présentement joint. NULL si aucune connexion.
        /// </summary>
        public string ServerAddress => _cameraRingServer?.IpAddress;

        private void ReactOnBrokenConnection()
        {
            if (_cameraRingServer != null)
            {
                UpdateStatus?.Invoke(NetworkStatusEnum.Error,
                    "Could not connect to server. Will attempt new connexion soon.");
                _cameraRingServer = null;
            }

            // On réessaye dans quelques instants
            _cts = new CancellationTokenSource();
            _sendTask = new Task(async () =>
            {
                await Task.Delay(15000); // Mieux qu'un "Thread.Sleep"

                // On tente de se reconnecter au serveur.
                ConnectToServer();

            }, _cts.Token, TaskCreationOptions.AttachedToParent);
            _sendTask.Start();
        }

        /// <summary>
        /// Indique si on a trouvé un serveur de réception et qu'il est prêt à recevoir nos envois.
        /// </summary>
        public bool IsServerDetected { get; private set; }

        /// <summary>
        /// Cette action est appellée à chaque fois que l'état de connexion du manager à un serveur change.
        /// </summary>
        public Action<NetworkStatusEnum, string> UpdateStatus { get; set; }
        
        /// <summary>
        /// Applique une addresse particulière pour la recherche d'un server distant. Quand cette valeur est non-nulle, elle sera la seule
        /// adresse qui sera tentée d'être rejointe par le gestionnaire.
        /// </summary>
        /// <param name="serverAddress">NULL si on ne veut pas d'adresse spécifique, sinon une référence vers les informations de ce serveur.</param>
        public void SetCustomAddress(SsdpUtil.TcpServiceLocation? serverAddress)
        {
            GetCustomAddress = serverAddress;

            if (GetCustomAddress == null) return;
            Stop();
            Start();
        }

        /// <summary>
        /// Retourne l'adresse personnalisée de ce serveur. Si on-nulle, ce sera la seule adresse qui sera tentée d'être rejointe par le gestionnaire.
        /// </summary>
        public SsdpUtil.TcpServiceLocation? GetCustomAddress { get; private set; }

        /// <summary>
        /// Énumération des différents état de recherche pour le server distant.
        /// </summary>
        public enum NetworkStatusEnum
        {
            /// <summary>
            /// Connexion avec le serveur établie, prêt à envoyer fichiers.
            /// </summary>
            Connected,
            /// <summary>
            /// Erreur de connexion détectée. Ne peut pas envoyer vers le serveur.
            /// </summary>
            Error,
            /// <summary>
            /// Recherche initiale du serveur en cours.
            /// </summary>
            Searching
        }
    }
}
