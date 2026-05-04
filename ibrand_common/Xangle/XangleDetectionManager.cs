using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Newtonsoft.Json;
using NLog;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Services;
using NomadLogic.Common.Util;

namespace NomadLogic.Common.Xangle
{
    /// <summary>
    /// Cette classe est un service qui, une fois lancé, va périodiquement tenter de trouver des nouvelles créations du système Xangle de Éric Paré.
    /// Elle fonctionne selon un système d'événements, à partir d'une date précise. Quand un ou plusieurs répertoires contenant ce que l'on veut est détecté,
    /// on lance un événement, et ce service peut fournir les informations sur le nouvel élément.
    /// Les fichiers originaux du système Xangle ne sont pas modifiés d'aucune façon.
    /// Les paramètres de recherche sont les propriétés de cette instances. Ils sont copiés en mémoire lorsque l'on commence une recherche (Start ou Reset).
    /// Il faut stopper la recherche pour mettre à jour ces paramètres, et l'instance va refuser de réinitialiser ses éléments trouvés si l'on est en marche.
    /// </summary>
    [PublicAPI]
    public class XangleDetectionManager : IService
    {
        /// <summary>
        /// Regex pour détecter les répertoires potentiels. "yyMMdd-HHMMSS.X"
        /// Explication:
        /// 1) yyMMdd -> année (deux chiffres) + mois + jour
        /// 2) Trait d'union obligatoire
        /// 3) HHMMSS => Heure (0 - 23) + minutes + secondes
        /// 4) Point obligatoire
        /// 5) Valeur entière positive (milisecondes en fait)
        /// 6) Est la dernière section de la chaîne (I.E. peut être le dernier élément d'un répertoire relatif ou absolu ).
        /// </summary>
        public const string PATTERN_DETECT_XANGLE_DIRS = @"((([0-9][0-9])(0[1-9]|1[012])(0[1-9]|[12]\d|3[01]))-(([01]\d|2[0-3])([0-5]\d)([0-5]\d)).(\d+))$";
        private static readonly Regex REGEX_POTENTIAL_DIRS = new Regex(PATTERN_DETECT_XANGLE_DIRS, RegexOptions.Compiled);
        public const string DATE_FORMAT = "yyyyMMdd-HHmmss";
        public const string DATE_TOSTRING_FORMAT = "yyyy-MM-dd HH:mm:ss";
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        private SortedSet<XangleItem> _foundItems = new SortedSet<XangleItem>();
        private SynchronizedCollection<IProgress<XangleReportEvent>> _observers = new SynchronizedCollection<IProgress<XangleReportEvent>>();
        
        private System.Timers.Timer _timer;
        private CancellationTokenSource _cts;
        private DirectoryInfo _baseDir;
        private DateTime _timeToCheck;
        private HashSet<string> _oldDirs = new HashSet<string>();
        private HashSet<string> _rejectedDirs = new HashSet<string>();
        private HashSet<string> _detectedDirs = new HashSet<string>();
        private Task _task;

        #region Events and Properties

        /// <summary>
        /// Cet événement est lancé dès qu'un ou plus nouvel éléments Xangle à été trouvé.
        /// Écouter ce signal garantit que chaque répertoire valide est signalé une et une seule fois.
        /// </summary>
        public event EventHandler<XangleItemDetected> NewXangleItem;
        /// <summary>
        /// Lancé quand on démarre le service.
        /// </summary>
        public event EventHandler Started;
        /// <summary>
        /// Lancé quand on stoppe le service.
        /// </summary>
        public event EventHandler<DateTime> Stopped;
        /// <summary>
        /// Indique qu'une séquence de recherche à eu lieu. Contient un compteur des séquences.
        /// </summary>
        public event EventHandler<long> UpdateCompleted;

        private long _updateSequenceId;

        /// <summary>
        /// Le répertoire de base que Xangle utilise pour créer ses éléments.
        /// </summary>
        public DirectoryInfo BaseDirectory { get; set; } // TODO mettre le répertoire de base de Xangle.

        /// <summary>
        /// Le temps minimal, inclusif, pour effectuer les recherches d'éléments.
        /// Par défaut, aujourd'hui, date locale.
        /// </summary>
        public DateTime TimeToCheck { get; set; } = DateTime.Now;

        /// <summary>
        /// Interval de vérification du répertoire, en millisecondes
        /// </summary>
        public int CheckInterval { get; set; } = 300;

        /// <summary>
        /// Indique la dernière date de vérification que le manager a lancé.
        /// Une fois stoppé, on peut utiliser cette valeur pour recommencer à une exécution ultérieur sans perdre
        /// un répertoire.
        /// Cette date est la même que celle retournée par l'événement Stopped.
        /// </summary>
        [JsonIgnore]
        public DateTime LastCheckDate { get; private set; }

        /// <summary>
        /// On assume que les dates sont dans le 21ième siècle par défaut.
        /// </summary>
        public string DatePrefix { get; set; } = "20";

        /// <summary>
        /// Information de pour transposer les éléments Xangle détectés à une classe capable de sauvegarde un questionnaire.
        /// Si vide, aucune sauvegarde vers un questionnaire n'est possible.
        /// </summary>
        public IList<XangleIbrandMapping> Mapping { get; set; }

        /// <summary>
        /// Indique s'il faut tenter de passer au prochain écran quand on obtient des nouveaux éléments Xangle.
        /// La méthode pour y arriver est laissée aux écoutants de ce manager.
        /// </summary>
        public bool AutomaticSubmit { get; set; }

        #endregion

        /// <summary>
        /// Retourne tous les éléments Xangle trouvé sur disque depuis une date précise, avec un identifiant optionnel.
        /// La date demandé peut être différente de celle utilisé par TimeToCheck.
        /// </summary>
        /// <param name="cutOff">Date minimal, inclusive, des éléments voulus.</param>
        /// <param name="xangleId">L'identifiant unique de l'item Xangle. Est égal au nom du répertoire.</param>
        /// <returns>Une collection en lecture seule, non-null, mais peut-être vide.</returns>
        public IReadOnlyCollection<XangleItem> FoundItems(DateTime cutOff, string xangleId = null)
        {
            lock (_foundItems)
            {
                var col = !string.IsNullOrWhiteSpace(xangleId)
                    ? _foundItems.Where(x => x.Id.Equals(xangleId)).ToList()
                    : _foundItems.Where(x => x.Date >= cutOff).ToList();

                return new ReadOnlyCollection<XangleItem>(col);
            }
        }

        /// <summary>
        /// Retourne tous les éléments Xangle trouvé sur disque depuis le départ, avec un identifiant optionnel.
        /// </summary>
        /// <param name="xangleId">L'identifiant unique de l'item Xangle. Est égal au nom du répertoire.</param>
        /// <returns>Une collection en lecture seule, non-null, mais peut-être vide.</returns>
        public IReadOnlyCollection<XangleItem> FoundItems(string xangleId = null)
        {
            return FoundItems(DateTime.MinValue, xangleId);
        }

        public void Dispose()
        {
            _timer?.Stop();
            log.Debug("Manager main task stop.");
            _task?.ContinueWith(t =>
            {
                _timer = null;
                _cts = null;
                _task = null;
                Reset();
                ClearAllProgressObservers();
                log.Trace("Manager instance disposed");
            });
            _cts?.Cancel();
            _task?.Wait();
        }

        public bool Init()
        {
            if (_timer != null) return true;

            if (CheckInterval <= 0) return false;
            if (!BaseDirectory?.Exists ?? true)
            {
                // Essaie un répertoire par défaut
                var tmp = new DirectoryInfo(Environment.ExpandEnvironmentVariables(@"%userprofile%\xangle\pictures"));

                if (!tmp.Exists)
                {
                    log.Error("The directory for Xangle does not exists: " + BaseDirectory);
                    return false;
                }

                BaseDirectory = tmp;
            }
            return true;
        }

        /// <summary>
        /// Démarre la recherche automatique de nouveaux éléments.
        /// </summary>
        public Task Start()
        {
            log.Debug("Manager Start called");
            if (!Init()) return Task.CompletedTask;

            if (_cts != null || _timer != null || _task != null)
            {
                log.Debug("Manager already started. Skipped start command.");
                return Task.CompletedTask;
            }

            _cts = new CancellationTokenSource();
            _timer = new System.Timers.Timer
            {
                AutoReset = true,
                Interval = CheckInterval
            };
            _timer.Elapsed += _timer_Elapsed;
            _timer.Start();

            // Copie des informations de paramètres, pour ne pas avoir des changements pendant l'exécution.
            _baseDir = BaseDirectory;
            _timeToCheck = TimeToCheck;
            _oldDirs.Clear();
            Started?.Invoke(this, null);
            return Task.CompletedTask;
        }

        private void _timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (_cts != null && _task == null || _task.IsCompleted) _task = Task.Run(() => CheckDirectory(_cts.Token), _cts.Token);
        }

        private void CheckDirectory(CancellationToken ct)
        {
            try
            {
                log.Trace("Check start.");
                if (ct.IsCancellationRequested) return;

                var dirs = _baseDir.GetDirectories();

                // Enlève les répertoires déjà traités.
                var subset = dirs.Where(d => !_oldDirs.Contains(d.FullName))
                    .Where(d => !_rejectedDirs.Contains(d.FullName)).Where(d => !_detectedDirs.Contains(d.FullName));

                Tuple<DirectoryInfo, XangleItem> lastIncomplete = null;
                var detectedItems = new List<Tuple<DirectoryInfo, XangleItem>>();

                // On cherche tout les répertoires qui représentent un potentiel répertoire Xangle, via REGEX
                foreach (var di in subset)
                {
                    if (ct.IsCancellationRequested) return;

                    // Filtre les répertoires dont le nom est invalide.
                    if (!REGEX_POTENTIAL_DIRS.IsMatch(di.Name))
                    {
                        log.Debug("Rejected directory: {0}", di);
                        _rejectedDirs.Add(di.FullName);
                        continue;
                    }

                    if (!ExtractDateAndSalt(di, out var time, out _))
                    {
                        log.Error("Could not extract date and salt from valid directory name: {0}", di.FullName);
                        _rejectedDirs.Add(di.FullName);
                        continue;
                    }

                    // On vérifie la date de "cut-off"
                    if (time < _timeToCheck)
                    {
                        // Si plus vieux, on ajoute dans la liste de "vieux" et on passe au prochain.
                        _oldDirs.Add(di.FullName);
                        log.Debug("Added directory as old. ({1}) below cutoff ({2}) Dir={0}", di, 
                            time.ToString(DATE_TOSTRING_FORMAT), _timeToCheck.ToString(DATE_TOSTRING_FORMAT));
                        continue;
                    }

                    var prospect = new XangleItem
                    {
                        Date = time,
                        Id = StringUtil.GetLastDirName(di.FullName)
                    };

                    // On vérifie si il contient des éléments
                    var alignedFiles = GatherFiles(di, "aligned", FileUtil.FILE_EXTENSION_JPG_FILTER);
                    var gifFiles = GatherFiles(di, "gif", FileUtil.FILE_EXTENSION_GIF_FILTER);
                    var videoFiles = GatherFiles(di, "mp4", FileUtil.FILE_EXTENSION_VIDEO_FILTER);
                    var thumbsFiles = GatherFiles(di, "thumbs", FileUtil.FILE_EXTENSION_JPG_FILTER);

                    //On assure toujours un cas complet car on a des probleme de reponse video vide. Cela entraine que on doit encore prendre le sujet en photo.
                    while (videoFiles != null && videoFiles.Length == 0)
                    {
                        videoFiles = GatherFiles(di, "mp4", FileUtil.FILE_EXTENSION_VIDEO_FILTER);
                    }
                    
                    // TODO Vérifier pour le dernier fichier disponible, pour vérifier le contenu.
                    // TODO Ceci est temporaire
                    if (alignedFiles == null || (gifFiles?.Length == 0 && videoFiles?.Length == 0 && thumbsFiles?.Length == 0))
                    {
                        if (lastIncomplete == null)
                        {
                            // Répertoire incomplet, on skip, on attend plus tard, mais on élimine si un répertoire incomplet n'est pas le plus récent
                            log.Debug("Incomplete directory detected: {0}. Aligned={1}, gif={2}, video={3}, thumbs={4}", 
                                di, alignedFiles?.Length, gifFiles?.Length, videoFiles?.Length, thumbsFiles?.Length);
                            lastIncomplete = new Tuple<DirectoryInfo, XangleItem>(di, prospect);
                        }
                        continue;
                    }

                    prospect.FrameNumber = alignedFiles.Length;

                    prospect.FullPath = di;
                    prospect.SetFilesArray(alignedFiles, gifFiles, videoFiles, thumbsFiles);
                    detectedItems.Add(new Tuple<DirectoryInfo, XangleItem>(di, prospect));

                    // Élimination du dernier répertoire incomplet, si le répertoire actuel est plus récent.
                    if (lastIncomplete == null || prospect.Date < lastIncomplete.Item2.Date) continue;
                    log.Debug("Old, incomplete directory detected, Invalidating it: " + di);
                    _rejectedDirs.Add(lastIncomplete.Item1.FullName);
                    lastIncomplete = null;
                }

                if (ct.IsCancellationRequested) return;

                // Une fois la boucle terminée, on lance les événements et met à jour les informations.
                var report = new XangleReportEvent();
                if (detectedItems.Any())
                {
                    lock (_foundItems)
                    {
                        foreach (var tp in detectedItems)
                        {
                            _foundItems.Add(tp.Item2);
                            _detectedDirs.Add(tp.Item1.FullName);
                            log.Debug("Added directory as found: {0}", tp.Item1);
                        }

                        LastCheckDate = _foundItems.Last().Date;
                        log.Trace("Updated last date checked: {0}", LastCheckDate);

                        report.NewItems = detectedItems.Select(t => t.Item2).ToList().AsReadOnly();
                        report.Message = $"Found {report.NewItems.Count} new directories.";

                        NewXangleItem?.Invoke(this, new XangleItemDetected
                        {
                            NewItems = detectedItems.Select(x => x.Item2).ToList()
                        });
                    }
                }
                else
                {
                    log.Trace("No new directory to parse.");
                    report.Message = "No new Xangle items: " + DateTime.Now.ToShortTimeString();
                }

                if (_updateSequenceId == long.MaxValue) _updateSequenceId = 0;
                else _updateSequenceId++;
                UpdateCompleted?.Invoke(this, _updateSequenceId);

                foreach (var obs in _observers)
                {
                    obs.Report(report);
                }
            }
            catch (Exception e)
            {
                log.Error(e);
            }
            finally
            {
                log.Trace("Check ended: {0}", _updateSequenceId);
            }
        }

        private FileInfo[] GatherFiles(DirectoryInfo baseDir, string subdir, string filter)
        {
            try
            {
                var newDir = baseDir.FullName + Path.DirectorySeparatorChar + subdir;
                if (!Directory.Exists(newDir))
                {
                    return null;
                }
                return Directory.GetFiles(newDir, filter, SearchOption.TopDirectoryOnly).Select(f => new FileInfo(f)).ToArray();
            }
            catch (Exception)
            {
                log.Error("Could not get files for dir:{0}, sub:{1}, filter:{2}", baseDir.FullName, subdir, filter);
                throw;
            }
        }

        private bool ExtractDateAndSalt(DirectoryInfo di, out DateTime dateTime, out string salt)
        {
            try
            {
                var dir = StringUtil.GetLastDirName(di.FullName);
                var data = dir.Split('.');
                var tmp = DatePrefix + data[0];
                log.Trace("Parsing as date: {0}", tmp);
                dateTime = DateTime.ParseExact(tmp, DATE_FORMAT, CultureInfo.InvariantCulture);
                salt = data[1];
                return true;
            }
            catch (Exception)
            {
                dateTime = DateTime.MinValue;
                salt = null;
                return false;
            }
        }

        /// <summary>
        /// Stoppe la recherche d'éléments. Ne vide pas la liste.
        /// </summary>
        public Task Stop()
        {
            _timer?.Stop();
            log.Debug("Manager Stop called");
            var finishTask = _task?.ContinueWith(t =>
            {
                _timer = null;
                _cts = null;
                _task = null;
                Stopped?.Invoke(this, LastCheckDate);
                log.Trace("Manager task stopped.");
            });
            _cts?.Cancel();

            return finishTask ?? Task.CompletedTask;
        }

        /// <summary>
        /// Vide la liste d'éléments trouvés. À noter que cette liste sera remise à neuf dans le même état si
        /// on ne modifie pas les paramètres de recherche.
        /// </summary>
        public void Reset()
        {
            if (_task != null && !_task.IsCompleted) return;
            log.Debug("Manager Reset called");
            lock (_foundItems)
            {
                _oldDirs.Clear();
                _foundItems.Clear();
            }
        }

        /// <summary>
        /// Ajoute une instance d'observeur à l'instance de manager.
        /// </summary>
        public void AddProgressObserver(Progress<XangleReportEvent> obs)
        {
            if (obs == null) return;
            log.Trace("Manager Observer added.");
            _observers.Add(obs);
        }

        /// <summary>
        /// Enlève un observeur, si assigné.
        /// </summary>
        /// <param name="obs"></param>
        public void RemoveProgressObserver(IProgress<XangleReportEvent> obs)
        {
            if (_observers.Remove(obs))
            {
                log.Trace("Manager Observer removed.");
            }
        }

        /// <summary>
        /// Enlève tous les observeurs.
        /// </summary>
        public void ClearAllProgressObservers()
        {
            _observers.Clear();
            _observers = new SynchronizedCollection<IProgress<XangleReportEvent>>(); // On remplace le sac. Simplement. L'ancien va passer par le GC.
            log.Trace("Manager Observer cleared.");
        }

        /// <summary>
        /// Retourne l'ensemble des répertoires qui ont été testés mais rejetés par le manager. Ne retourne pas les fichiers qui sont
        /// hors limite de la date minimale: cette liste est interne seulement.
        /// Cette liste englobe tous les répertoires qui ont été jugés totalement invalides, c'est à dire ne respectent pas le REGEX principal de date.
        /// Cette liste est construite à chaque invocation, mais n'est jamais vidée.
        /// </summary>
        public IReadOnlyCollection<string> RejectedDirectories()
        {
            return _rejectedDirs.ToList().AsReadOnly();
        }

        /// <summary>
        /// Exécute une sauvegarde de réponses basé sur les élément Xangle trouvés ainsi qu'un "mapping" d'élément vers des questions spécifiques.
        /// </summary>
        /// <param name="questSource">L'instance responsable de modifier le questionnaire.</param>
        /// <param name="respSource">Le répondant sur lequel basé l'opération.</param>
        /// <param name="mapping">Les information de "mapping"</param>
        /// <param name="xangleItem">L'élément Xangle qui peut servir à remplir le questionnaire.</param>
        /// <returns>TRUE si la liste est vide (No-Op), ou si toutes les sauvegardes se sont correctement effectuées.</returns>
        public static bool SaveToQuestionnaire(ISaveLocalRespondentData questSource, ICurrentRespondentSource respSource, IList<XangleIbrandMapping> mapping, XangleItem xangleItem)
        {
            if (questSource == null || respSource == null || xangleItem == null || mapping == null) return false;

            var resp = respSource.GetValidatedCurrentRespondent();

            foreach (var mapper in mapping)
            {
                if (!mapper.FindMapping(resp, xangleItem, out var ans, out var isCheck)) continue;
                var ok = isCheck
                    ? questSource.SaveAnswerToQuestionCheck(ans.QuestionNo, ans.ChoiceNo, true)
                    : questSource.SaveAnswerToQuestion(ans);

                if (!ok)
                {
                    log.Error("Could not save answer to questionnaire: {0} for respondent {1}", ans, resp);
                    return false;
                }
            }

            return true;
        }
    }
}