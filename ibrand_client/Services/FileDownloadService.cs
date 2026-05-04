using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using NLog;
using NomadLogic.Common.Services;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Math;
using NomadLogic.Common.Util.Networking;
using Logger = NLog.Logger;

namespace NomadLogic.iBrandDesktop.Services
{
    /// <summary>
    /// Cette classe de service s'occupe de télécharger des fichiers via une liste de priorité, et utilise plusieurs Task en parrallèle pour y parvenir.
    /// Une fois chaque demande entrée dans le service, le fichier sera téléchargé, et on peut faire un rappel via Action vers l'instance qui a fait la demande.
    /// Deux possibilités; soit le téléchargement est réussi, ou a échoué. Le système ne fait aussi aucune garantie sur la vitesse du téléchargement: selon la priorité
    /// de chaque éléments, les demandes ne sont pas faites selon une file FIFO, ainsi que les aléas de téléchargements via internet (connexion brisée ou lente, taille du
    /// fichier, etc...)
    /// </summary>
    [PublicAPI]
    public class FileDownloadService : ServiceLongRunningAbstract<DownloadReport>
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private TaskFactory _factory;
        private readonly ISet<DownloadRequest> _downloadWaiting = new SortedSet<DownloadRequest>();
        private readonly List<Task> _tasksBag = new List<Task>();
        private int _nbrFilesDownloaded;
        private long _totalBytesDl;

        /// <summary>
        /// Créé l'instance avec ses configurations de base.
        /// </summary>
        /// <param name="maxConcurrentDownload">Minimum 1, 5 par défaut.</param>
        public FileDownloadService(uint maxConcurrentDownload = 20)
        {
            if (maxConcurrentDownload < 1)
            {
                throw new ArgumentException(@"Cannot have less than one concurrent download.", nameof(maxConcurrentDownload));
            }

            MaxConcurrentDownloads = maxConcurrentDownload;
        }

        /// <summary>
        /// Nombre maximal d'essais sur un fichier pour le téléchargement. Un code 404 (NOT FOUND) ignore cette valeur.
        /// </summary>
        public uint MaxNumberOfRetries { get; set; } = 3;

        /// <summary>
        /// Le nombre maximal de demandes de téléchargement simultanées.
        /// </summary>
        public uint MaxConcurrentDownloads { get; }

        public override string ServiceName { get; protected set; } = "Main server image download service";

        private ManualResetEventSlim NewDownloadGiven { get; } = new ManualResetEventSlim(false);

        protected override bool DoInit()
        {
            return true;
        }

        protected override void OnDispose()
        {
            lock (_downloadWaiting)
            {
                foreach (var dw in _downloadWaiting)
                {
                    dw.CallbackError.Invoke(dw);
                }
                _downloadWaiting.Clear();
            }

            _factory = null;
        }

        protected override Task<bool> DoMainLoop(CancellationToken ct)
        {
            if (_factory == null)
            {
                _factory = new TaskFactory(ct, TaskCreationOptions.PreferFairness, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
            }

            while (!ct.IsCancellationRequested)
            {
                // Attends un nouveau événement de téléchargement.
                try
                {
                    NewDownloadGiven.Wait(ct);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                log.Trace("New download request event detected.");
                NewDownloadGiven.Reset();

                lock (_tasksBag)
                {
                    _tasksBag.RemoveAll(t => t.IsCompleted);
                }

                var newElems = new List<DownloadRequest>();
                lock (_downloadWaiting)
                {
                    if (!_downloadWaiting.Any())
                    {
                        // Pas de téléchargement en attente.
                        continue;
                    }

                    var currentBagCount = _tasksBag.Count;
                    var nbrNewTasks = MaxConcurrentDownloads - currentBagCount;
                    if (nbrNewTasks < 1)
                    {
                        log.Trace("Max concurency reached ({0}). Back to waiting", currentBagCount);
                        continue;
                    }

                    nbrNewTasks = Math.Min(nbrNewTasks, _downloadWaiting.Count);

                    log.Trace("Adding {0} new download tasks.", nbrNewTasks);
                    
                    for (var i = 0; i < nbrNewTasks; i++)
                    {
                        var elem = _downloadWaiting.Max();
                        _downloadWaiting.Remove(elem);
                        newElems.Add(elem);
                        var t = _factory.StartNew(() => DoDownload(ct, elem), ct);
                        _tasksBag.Add(t);
                    }
                }

                var sb = new StringBuilder();
                sb.AppendLine("Following urls are now queued for download:");
                foreach (var ne in newElems)
                {
                    sb.AppendLine(ne.GetUrl());
                }
                UpdateAndSendReport(sb.ToString());
            }

            log.Debug("File Download service main cycle stopped.");
            _factory = null;
            return Task.FromResult(true);
        }
        
        public void AddDownloadRequest([NotNull] DownloadRequest newRequest)
        {
            lock (_downloadWaiting)
            {
                _downloadWaiting.Add(newRequest);
                NewDownloadGiven.Set();
            }
        }

        public void AddDownloadRequest([NotNull] IEnumerable<DownloadRequest> newRequests)
        {
            lock (_downloadWaiting)
            {
                var before = _downloadWaiting.Count;
                foreach (var nr in newRequests)
                {
                    _downloadWaiting.Add(nr);
                }
                var after = _downloadWaiting.Count;
                if (before != after) NewDownloadGiven.Set();
            }
        }

        private async Task DoDownload(CancellationToken ct, DownloadRequest dr)
        {
            if (ct.IsCancellationRequested)
            {
                return;
            }

            if (dr == null)
            {
                log.Error("Null reference for download request detected.");
                return;
            }

            var dlFile = dr.GetFinalFileLocation();
            var url = dr.GetUrl();
            log.Trace("Downloading file ({0}) to: {1}", url, dlFile.FullName);

            var res = await HttpRestUtil.ExecuteDownLoadRequestAsync(ct, dlFile, url);
            
            // Met à jour les statistiques de cette demande.
            dr.NbrTries++;
            dr.StatusCode = res.StatusCode;

            if (res.StatusCode != HttpStatusCode.OK)
            {
                if (res.StatusCode == HttpStatusCode.NotFound)
                {
                    log.Warn("File could not be found on server.");
                    UpdateAndSendReport(null, "File could not be found on server. Will not retry: " + url);
                    dr.CallbackError?.Invoke(dr);
                    return;
                }

                // Nombre maximal d'essais atteint?
                if (dr.NbrTries > MaxNumberOfRetries)
                {
                    log.Warn("File could not be downloaded for {0} tries: url={1}", dr.NbrTries, url);
                    UpdateAndSendReport(null, "Maximum attemps reached. Will not retry: " + url);
                    dr.CallbackError?.Invoke(dr);
                    return;
                }

                UpdateAndSendReport(null, "Error during download (Code={0}). Will retry later for file: " + url);
                // On remet le téléchargement dans la liste, avec un léger délai.
                // ReSharper disable once AssignmentIsFullyDiscarded
                _ = Task.Delay(TimeSpan.FromSeconds(3), ct).ContinueWith(_ =>
                  {
                      log.Trace("Delayed retry download add request for: {0}", url);
                      AddDownloadRequest(dr);
                  }, ct);
                return;
            }

            dlFile.Refresh();
            var fileSize = dlFile.Length;
            _nbrFilesDownloaded++;
            _totalBytesDl += fileSize;
            var dlMessage = $"File {dr.GetShortUrlName()} downloaded. Size={MathUtil.ByteNumberReadable(fileSize)}";
            log.Debug(dlMessage);
            UpdateAndSendReport(dlMessage);
            dr.CallbackComplete?.Invoke(dr);
        }

        protected override bool CheckIfIdle()
        {
            return !_tasksBag.Any();
        }
        
        private void UpdateAndSendReport(string message = null, string errorMessage = null)
        {
            int dlWait;
            lock (_downloadWaiting)
            {
                dlWait = _downloadWaiting.Count;
            }

            var info = new DownloadReport
            {
                TotalFilesDownloaded = _nbrFilesDownloaded,
                WaitingDownloads = dlWait,
                TotalBytesDownloaded = _totalBytesDl,
                ActiveDownloads = _tasksBag.Count,
                MessageError = errorMessage,
                Message = message
            };

            TriggerReport(info, true);
        }
    }

    /// <summary>
    /// Représente une demande de téléchargement. Contient des valeurs transitives, mais uniquement les valeurs fixes une fois crées sont
    /// considérées pour la comparaison et l'égalité.
    /// </summary>
    public class DownloadRequest : IEquatable<DownloadRequest>, IComparable<DownloadRequest>
    {
        private readonly FileInfo _destination;
        private readonly string _url;

        public DownloadRequest([NotNull] FileInfo finalFile, string url, 
            Action<DownloadRequest> callbackSuccess = null, 
            Action<DownloadRequest> callbackError = null)
        {
            _destination = finalFile;
            if (string.IsNullOrWhiteSpace(url))
            {
                throw new ArgumentException("URL cannot be empty or null.");
            }

            _url = url;

            CallbackComplete = callbackSuccess;
            CallbackError = callbackError;
        }

        public FileInfo GetFinalFileLocation()
        {
            return _destination;
        }

        public string GetUrl()
        {
            return _url;
        }

        public Action<DownloadRequest> CallbackError { get; }

        public Action<DownloadRequest> CallbackComplete { get; }
        
        /// <summary>
        /// Dernier code HTTP reçu pour une tentative de téléchargement de ce fichier.
        /// </summary>
        public HttpStatusCode? StatusCode { get; set; }
        
        /// <summary>
        /// Nombre d'essais complétés (avec succès ou non) sur ce fichier.
        /// Cette valeur est transitive et ne fait pas partie de la vérification d'égalité.
        /// </summary>
        internal int NbrTries { get; set; }

        /// <summary>
        /// Au besoin, on peut donner une référence d'objet pour donner une valeur supplémentaire à la demande de téléchargement.
        /// Cette valeur habituellement sert lors de l'appel de callback, ce qui permet à l'appellant de savoir à quel élément est associé
        /// la demande de téléchargement.
        /// Cette référence n,est pas générique pour respecter l'aspect unique de ce service, c'est donc à l'action appellée de "caster" cette
        /// référence dans le bon type.
        /// </summary>
        public object CustomData { get; set; }

        public string GetShortUrlName()
        {
            return StringUtil.GetFileName(_url);
        }

        public bool Equals(DownloadRequest other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(_destination, other._destination) && _url == other._url;
        }

        public override bool Equals(object obj)
        {
            if (obj is DownloadRequest other) return Equals(other);
            return false;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((_destination != null ? _destination.GetHashCode() : 0) * 397) ^ (_url != null ? _url.GetHashCode() : 0);
            }
        }

        public static bool operator ==(DownloadRequest left, DownloadRequest right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(DownloadRequest left, DownloadRequest right)
        {
            return !Equals(left, right);
        }

        public int CompareTo(DownloadRequest other)
        {
            if (ReferenceEquals(this, other)) return 0;
            if (other is null) return 1;
            var urlComparison = string.Compare(_url, other._url, StringComparison.Ordinal);
            if (urlComparison != 0) return urlComparison;
            return string.Compare(_destination.FullName, other._destination.FullName, StringComparison.Ordinal);
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.Append('[');
            sb.Append("url='");
            sb.Append(_url);
            sb.Append("', dest='");
            sb.Append(_destination.FullName);
            sb.Append("', code/tries=");
            sb.Append(StatusCode?.ToString() ?? "none");
            sb.Append('/');
            sb.Append(NbrTries);
            sb.Append(']');
            return sb.ToString();
        }
    }

    public class DownloadReport : IProgressDeepCopy<DownloadReport>
    {
        public string Message { get; set; }
        public string MessageError { get; set; }
        public int ActiveDownloads { get; set; }
        public int WaitingDownloads { get; set; }
        public int TotalFilesDownloaded { get; set; }
        public long TotalBytesDownloaded { get; set; }

        public DownloadReport Copy()
        {
            return new DownloadReport
            {
                Message = Message,
                MessageError = MessageError,
                ActiveDownloads = ActiveDownloads,
                WaitingDownloads = WaitingDownloads,
                TotalBytesDownloaded = TotalBytesDownloaded,
                TotalFilesDownloaded = TotalFilesDownloaded
            };
        }
    }
}