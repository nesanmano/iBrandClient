using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using JetBrains.Annotations;
using NLog;
using NomadLogic.Common.Extentions;
using NomadLogic.Common.Services;
using NomadLogic.Common.Services.Configuration;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.DateAndTime;
using NomadLogic.Common.Util.Json;
using NomadLogic.Common.Util.Networking;
using NomadLogic.Common.Util.Networking.Ftp;
using NomadLogic.Common.Util.Print;
using NomadLogic.Common.Util.Win32;
using NomadLogic.PrintStation.Json;
using NomadLogic.PrintStation.Services;
using NomadLogic.PrintStation.Ui;
using NomadLogic.PrintStation.Util.ListenForFilesFromIBrandAction;
using WinSCP;
using static NomadLogic.PrintStation.FeatureConfigEnum;

// ReSharper disable AssignNullToNotNullAttribute

namespace NomadLogic.PrintStation
{
    /// <summary>
    ///     Cette classe recherche les posts de multiples façon différente.
    /// </summary>
    public class GetPostsTask
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private readonly NomadConfiguration _psConfiguration;
        private readonly PrintStationDatabase _database;
        private readonly PrintStationUser _user;
        private bool _lookForInternetAccess = true;
        private Task _localImageDirTask;
        private CancellationTokenSource _cts;

        /// <summary>
        ///     Contructeur principal.
        /// </summary>
        /// <param name="database">La base de donnée à utiliser pour gérer les Posts.</param>
        /// <param name="config">La abstractConfiguration du système.</param>
        public GetPostsTask(PrintStationDatabase database, NomadConfiguration config)
        {
            if (database == null || config == null)
            {
                throw new Exception("Cannot instanciate a Post Task without a database and abstractConfiguration object.");
            }

            _database = database;
            _user = _database.GetActiveUser();
            _psConfiguration = config;
        }

        /// <summary>
        ///     Indique que de nouveaux Posts ont été détectés.
        /// </summary>
        public event EventHandler NewPosts;

        /// <summary>
        ///     Lance la tâche, et ne peut être arrêtée que si le paramètre de Token d'arrêt est notifié d'un arrêt.
        ///     <br />Cette méthode ne PEUT PAS arrêter d'elle-même et donc doit absolument être encapsulée dans un Task C#.
        /// </summary>
        /// <param name="cancel">Le Token d'arrêt.</param>
        /// <param name="control">Si existant, le control de Forms pour les messages en dialogue.</param>
        public void DoWork(CancellationToken cancel, Control control)
        {
            log.Trace("GetPost Task has started.");
            while (!cancel.IsCancellationRequested)
            {
                try
                {
                    List<Post> posts = null;
                    var hasIbrandPost = false;

                    if (!_psConfiguration.Printstation.DontLookForPost)
                    {
                        // Recherche 1: Regarde pour l'unique Post le plus récent sur le serveur.
                        if (RunningFeatureConfig.GetInstance().GetConfiguration<bool>(IsLookForMostRecentPostOnServer))
                        {
                            var post = GetLastPost();

                            if (post != null)
                            {
                                InsertPost(post);

                                DownloadPictures(post);

                                NewPosts?.Invoke(this, EventArgs.Empty);
                            }

                            _database.ResetPicturesToDownload(_user.EventId);

                            RunningFeatureConfig.GetInstance().SetConfiguration(IsLookForMostRecentPostOnServer, false);
                        }

                        // Recherche 2: Vérification du dernier Post enregistré sur le serveur.
                        if ((!cancel.IsCancellationRequested))
                        {
                            var lastUpdateId = _database.GetLastUpdateId(_user.EventId);

                            posts = GetPost(lastUpdateId);

                            if (posts != null)
                            {
                                foreach (var post in posts)
                                {
                                    InsertPost(post);
                                }
                            }
                        }
                    }



                    // Recherche 3: Vérifie s'il faut créer un post à partir d'un fichier ZIP directement envoyé par socket.
                    if ((!cancel.IsCancellationRequested))
                    {
                        if (_psConfiguration.Printstation.ListenForPicturesFromIBrand)
                        {
                            hasIbrandPost |= CreatePostFromIBrandPictures(cancel, control);
                        }
                    }

                    // Recherche 4: Vérifie s'il faut regarder un répertoire unique comme source de fichiers
                    if (!cancel.IsCancellationRequested && _localImageDirTask == null)
                    {
                        var dir = RunningFeatureConfig.GetInstance().GetConfiguration<string>(CheckForLocalImagesDir);
                        if (dir != null)
                        {
                            _cts = new CancellationTokenSource();
                            // ReSharper disable once MethodSupportsCancellation
                            _localImageDirTask = CreatePostFromLocalDirectory(control, dir, CancellationTokenSource.CreateLinkedTokenSource(cancel, _cts.Token).Token, NewPosts).ContinueWith(t =>
                            {
                                _cts = null;
                                _localImageDirTask = null;
                            });
                        }
                    }

                    // Maintenant que l'on a mis à jour tous les Posts disponibles, on fait un téléchargement de ces images.
                    if (!cancel.IsCancellationRequested)
                    {
                        DownloadImages(cancel);
                    }

                    //Post recherches: On indique si on a de nouveaux posts via un événement.
                    if ((posts != null && posts.Count > 0) || hasIbrandPost)
                    {
                        NewPosts?.Invoke(this, EventArgs.Empty);
                    }

                    // On attend un peu, en regardant souvent si on doit canceller
                    for (var i = 0; i < 50; i++)
                    {
                        if (cancel.IsCancellationRequested)
                        {
                            break;
                        }

                        Thread.Sleep(100);
                    }
                }
                catch (Exception ex)
                {
                    log.Error(ex);
                    
                    if (!_lookForInternetAccess) continue;

                    if (NetworkMonitor.IsInternetAvailable) continue;

                    var noInternetDlg = new NoInternetAccessDialog();
                    noInternetDlg.ShowDialog();

                    _lookForInternetAccess = noInternetDlg.HasToShowAgain();
                }
                finally
                {
                    _cts?.Cancel();
                    _localImageDirTask = null;
                }
            }
            log.Trace("GetPost Task has stopped.");
        }

        private Task CreatePostFromLocalDirectory(Control parentForm, 
            [NotNull] string dir,
            CancellationToken cancel,
            [NotNull] EventHandler onNewPosts)
        {
            var timer = new Stopwatch();
            timer.Start();
            
            // 1- Vérification du répertoire
            DirectoryInfo dirInfo;
            if (Path.IsPathRooted(dir) && !dir.StartsWith(Path.DirectorySeparatorChar.ToString()))
            {
                // Tentative de référencer un répertoire absolu
                dirInfo = new DirectoryInfo(dir);
            }
            else
            {
                // Path relatif. À partir de Printstation base, pas de l'application.
                var dir1 = DirectoryUtil.GetLocalMediaDir(DirectoryUtil.NomadLogicMediaDirectoriesEnum.pictures) + _user.Id;
                dirInfo = new DirectoryInfo(Path.Combine(dir1, dir));
            }

            if (!dirInfo.Exists)
            {
                dirInfo.Create();
            }

            log.Trace("Checking directory '{0}'", dirInfo.FullName);

            if (cancel.IsCancellationRequested) return Task.CompletedTask;

            // 2- Vérifie si on a un rapatriement de fichier d'un FTP à faire au préalable.
            var jsonFile = UiFilesUtil.GetFileNameWhitoutDefault(UiFilesUtil.FtpSourceJson, _user.Id);
            if (File.Exists(jsonFile))
            {
                // On fait la synchro du site FTP.
                var options = JsonUtil.ReadJsonFile<SessionOptions>(new FileInfo(jsonFile));
                if (options != null)
                {
                    Progress<FtpBatchProgress> prog = null;
                    if (log.IsDebugEnabled)
                    {
                        prog = new Progress<FtpBatchProgress>(e =>
                        {
                            log.Debug("Completed: {0: F2}, total files: {1}", e.Completion, e.Transfers.Count);
                        });
                    }

                    var t = FtpWebUtil.SynchronizeDirectory(options, dirInfo.FullName, "/", cancel, DateTime.MinValue, new []{"jpg"}, prog);
                    t.Wait(cancel);
                }
            }

            // 3- Énumération des fichiers
            var subDir = RunningFeatureConfig.GetInstance().GetConfiguration<bool>(CheckForLocalImagesUseSubDir);
            var so = subDir ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            log.Trace("Is subdirectory search enabled: {0}", subDir);

            Func<FileInfo, bool> dateFilter = null;
            var dateStr = RunningFeatureConfig.GetInstance().GetConfiguration<string>(CheckForLocalImagesDateSpan);
            if (!string.IsNullOrWhiteSpace(dateStr))
            {
                var dates = dateStr.Split('|');
                var dateMax = DateTime.MaxValue;
                if (DateTime.TryParseExact(dates[0], "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateMin))
                {   
                    if (dates.Length > 1 && !DateTime.TryParseExact(dates[1], "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out dateMax))
                    {
                        log.Warn("Unparsable max date value, will remain MAX: {0}", dates[1]);
                    }
                }
                else
                {
                    log.Warn("Unparsable min date value, will remain MIN: {0}", dates[0]);
                }

                if (dateMin != DateTime.MinValue)
                {
                    log.Trace("Min/Max date filter detected. Original='{0}', Min='{1}', Max='{2}'", dateStr, dateMin.ToLongDateString(), dates.Length > 1 ? dateMax.ToLongDateString() : "None");
                    dateFilter = FileUtil.FileDateFilterFunction(dateMin, dateMax);
                }
            }

            if (cancel.IsCancellationRequested) return Task.CompletedTask;

            if (dateFilter == null)
            {
                var spanStr = RunningFeatureConfig.GetInstance().GetConfiguration<string>(CheckForLocalImagesMaxAge);

                // https://docs.microsoft.com/en-us/dotnet/api/system.timespan.parseexact?view=netframework-4.6.1
                if (!string.IsNullOrWhiteSpace(spanStr) && TimeSpan.TryParseExact(spanStr, "G", CultureInfo.InvariantCulture, TimeSpanStyles.None, out var maxAge))
                {
                    log.Trace("Max age filter detected: {0}", maxAge.ToString());
                    dateFilter = FileUtil.FileDateFilterFunction(maxAge);
                }
            }

            var files = dirInfo.FilterFiles(so, dateFilter).ToList();

            log.Trace("Number of files identified by filters (pass 1): {0}", files.Count);

            if (cancel.IsCancellationRequested) return Task.CompletedTask;

            // On enlève maintenant les fichiers antérieur à la dernière date de mise à jour retenue par le PS.
            _database.GetLastLocalFileDate(_user, dir, out var lastUpdate);

            if (lastUpdate != DateTime.MinValue)
            {
                var culled = files.RemoveAll(f => f.LastWriteTime <= lastUpdate);
                log.Trace("Number of files culled by last update (pass 2): {0}", culled);
            }

            if (cancel.IsCancellationRequested) return Task.CompletedTask;

            if (files.Any())
            {
                // Pour tous les fichiers restant à traiter.
                foreach (var f in files)
                {
                    var post = new Post(-1, GenerateUniqueId.GetId(), f.FullName, null, "local",
                        f.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss"), true, false, -1, 1, -1, -1)
                    {
                        EventId = _user.EventId,
                        SocialAppServiceName = "Local"
                    };
                    _database.InsertIBrandPost(post);

                    if (lastUpdate < f.LastWriteTime)
                    {
                        lastUpdate = f.LastWriteTime;
                        _database.UpdateLastLocalFileDate(_user, dir, lastUpdate);
                        log.Debug("New last time updated: ({0}) - {1}", lastUpdate.Ticks, lastUpdate.ToString("yyyy-MM-dd HH:mm:ss.fff"));
                    }

                    log.Trace("File added as local post file: timeCreated='{1}', path='{0}'", f.FullName, f.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss.fff"));

                    if (cancel.IsCancellationRequested) return Task.CompletedTask;
                }

                onNewPosts.Invoke(this, EventArgs.Empty);

                if (log.IsDebugEnabled)
                {
                    _database.GetLastLocalFileDate(_user, dir, out lastUpdate);
                    log.Debug("Last update date after all files processed: ({0}) - {1}", lastUpdate.Ticks, lastUpdate.ToString("yyyy-MM-dd HH:mm:ss.fff"));
                }
            }
            else
            {
                log.Trace("No new local file detected.");
            }

            timer.Stop();
            log.Debug("Local directory check time: {0}", timer.Elapsed.ToStringHourMinSec(true, true));
            return Task.CompletedTask;
        }

        private bool CreatePostFromIBrandPictures(CancellationToken ct, Control control)
        {
            var files =
                Directory.GetFiles(
                    DirectoryUtil.GetLocalMediaDir(DirectoryUtil.NomadLogicMediaDirectoriesEnum.listen_from_ibrand));

            // Pour chaque fichier qui n'a pas été traité (chacun sera considéré comme un post)
            foreach (var file in files)
            {
                if (ct.IsCancellationRequested)
                {
                    break;
                }

                // Si on a une classe de submitaction pour le projet actif, on utilise cete classe pour traiter les fichiers reçus.
                var className = RunningFeatureConfig.GetInstance().GetConfiguration<string>(CreatePostFromIbrandCustomAction);
                if (className != null)
                {
                    var type = Type.GetType("NomadLogic.PrintStation.Util.ListenForFilesFromIBrandAction." + className);

                    if (type == null) continue;
                    var submitAction = (AbstractAction)Activator.CreateInstance(type);

                    submitAction.Init(file, _user);
                    submitAction.DoWork();
                }
                else
                {
                    // On tente de déterminer si le fichier est un ZIP (fichiers multiples)
                    if (StringUtil.GetFileExtension(file).Equals("zip", StringComparison.OrdinalIgnoreCase))
                    {
                        MakePostMultipleFiles(file);
                    }
                    else
                    {
                        // On continue avec un seul fichier, qui doit être une image.
                        MakePostSingleImage(file, control);
                    }
                }
            }

            return files.Length > 0;
        }

        private void MakePostMultipleFiles(string file)
        {
            try
            {
                // Étape 1: création du répertoire de travail, et extraction du fichier.
                string firstImage;
                string commonId;

                var dirName = DirectoryUtil.ExtractZipToLocalTemporaryDirectory(file);

                try
                {
                    // On tente de trouver le premier fichier d'image.
                    firstImage = dirName.GetFiles().FirstOrDefault(f =>
                                                                       f.Name.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase) ||
                                                                       f.Name.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                                                                       f.Name.EndsWith(".png", StringComparison.OrdinalIgnoreCase)
                                                                  )?.FullName;

                    // Vérification du premier fichier d'image.
                    if (firstImage == null || !File.Exists(firstImage))
                    {
                        log.Error("Could not find the first image file in ZIP from socket.");
                        return;
                    }

                    commonId = StringUtil.RemoveExtension(StringUtil.GetFileName(firstImage));

                    // Étape trois, on bouge les fichiers dans un répertoire précis
                    var finalDir = DirectoryUtil.GetLocalPrintstationMediaDir(_user.Id, DirectoryUtil.NomadLogicPrintstationDirectoriesEnum.file_bundles) + commonId;
                    if (Directory.Exists(finalDir)) { Directory.Delete(finalDir, true); }
                    Directory.CreateDirectory(finalDir);

                    foreach (var fileFromTmpDir in dirName.EnumerateFiles())
                    {
                        var newFileName = fileFromTmpDir.Name;
                        var tmpFile = Path.Combine(finalDir, newFileName);
                        File.Move(fileFromTmpDir.FullName, tmpFile);
                    }

                    firstImage = Directory.GetFiles(finalDir).FirstOrDefault(f =>
                                                                       f.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase) ||
                                                                       f.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                                                                       f.EndsWith(".png", StringComparison.OrdinalIgnoreCase)
                                                                  );
                    if (firstImage == null || !File.Exists(firstImage))
                    {
                        log.Error("Could not find the first image in the final directory.");
                        return;
                    }
                }
                finally
                {
                    // Le répertoire de travail du ZIP est maintenant vide. On peut le détruire
                    FileUtil.DeleteFolderContent(dirName.FullName);
                }
                
                var database = ServicesContainer.GetService<PrintStationDatabase>();
                PrintStationUtil.CreatePostFromFile(commonId, firstImage, database, _user);
            }
            finally
            {
                // On détruit le fichier original, car on a terminé avec.
                File.Delete(file);
            }
        }

        /// <summary>
        /// Créer un post à partir d'une seule image reçue.
        /// </summary>
        /// <param name="file">Le fichier à traiter.</param>
        /// <param name="control">Le contrôle WinForm appellant.</param>
        /// <param name="appService">Le service qui a émis le Post.</param>
        /// <param name="appServiceUser">L'usager du service qui a émis le Post.</param>
        private void MakePostSingleImage(string file, Control control, string appService = "iBrand", string appServiceUser = "ibrand")
        {
            if (!FileUtil.IsImageFile(file)) return;

            var fileCreatedDate = File.GetCreationTime(file);

            var fileName =
                DirectoryUtil.GetLocalPrintstationMediaDir(_user.Id,
                    DirectoryUtil.NomadLogicPrintstationDirectoriesEnum.pictures) +
                StringUtil.GetFileName(file);
            
            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }
            File.Move(file, fileName);

            var post = new Post(-1, GenerateUniqueId.GetId(), fileName, null, appServiceUser,
                fileCreatedDate.ToString("yyyy-MM-dd HH:mm:ss"), true, false, -1, 1, -1, -1)
            {
                EventId = _user.EventId,
                SocialAppServiceName = appService
            };
            _database.InsertIBrandPost(post);

            // On vérifie si maintenant on doit gérer pour une impression automatique.
            if (!RunningFeatureConfig.GetInstance().GetConfiguration<bool>(IsAutomaticPrint)) return;

            log.Debug("Automatic HasPrint preparing.");

            if (RunningFeatureConfig.GetInstance().GetConfiguration<bool>(HasFrame))
            {
                var pictureName = GenerateUniqueId.GetId() + ".jpg";

                var fileToPrint =
                    DirectoryUtil.GetLocalPrintstationMediaDir(_user.Id,
                        DirectoryUtil.NomadLogicPrintstationDirectoriesEnum.pictures) + pictureName;



                var pictInfo = new PicturePostInfo(fileName, @"iBrand", null, _user.Id, -1, post.Id, post.RepondantNoFromServer);

                var result = PrintStationUtil.CreateFramedPhoto(fileName, pictInfo, fileToPrint);

                PrintUtil.Print(fileToPrint, null, control);

                result.Dispose();
            }
            else
            {
                if (RunningFeatureConfig.GetInstance().GetConfiguration<bool>(IsPictureSizeTwoBySix))
                {
                    var pictureName = GenerateUniqueId.GetId() + ".jpg";
                    var fileToPrint =
                        DirectoryUtil.GetLocalPrintstationMediaDir(_user.Id,
                            DirectoryUtil.NomadLogicPrintstationDirectoriesEnum.pictures) + pictureName;

                    var result = PrintStationUtil.Create2By6Photo(fileName, fileToPrint);

                    PrintUtil.Print(fileToPrint, null, control);

                    result.Dispose();
                }
                else
                {
                    PrintUtil.Print(fileName, null, control);
                }
            }

            _database.AddPrintCountSocialCam(_user.EventId);
        }

        /// <summary>
        /// Insère un Post venant du serveur dans la base de données locale.
        /// </summary>
        /// <param name="post"></param>
        private void InsertPost(Post post)
        {
            if (_database.IsPostInserted(post.ServerId))
            {
                _database.UpdatePost(post);
            }
            else
                _database.InsertPost(post);
        }

        /// <summary>
        /// Va chercher les Posts sur le serveur à partir d'un identifiant BD.
        /// </summary>
        /// <param name="lastUpdateId">Dernier identifiant téléchargé du serveur principal.</param>
        /// <returns>La liste des Posts récent transmis par le serveur.</returns>
        private List<Post> GetPost(int lastUpdateId)
        {
            Stopwatch timer = null;
            if (log.IsTraceEnabled)
            {
                timer = new Stopwatch();
                timer.Start();
            }

            List<Post> posts = null;
            try
            {
                var configuration = ServicesContainer.GetService<NomadConfiguration>();

                var server = new Server("192.95.36.173", "le-server", "8186", "8186");

                var request = HttpUtil.GenerateRequest(server, "print-station/GetPosts", "&event_id=" + _user.EventId + "&last_element_id=" + lastUpdateId, 1000);
                var response = (HttpWebResponse)request.GetResponse();


                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var jsonSerializer = new DataContractJsonSerializer(typeof(List<Post>));
                    if (response.GetResponseStream() != null)
                    {
                        var objResponse = jsonSerializer.ReadObject(response.GetResponseStream());
                        posts = objResponse as List<Post>;
                    }
                }
            }
            catch (WebException ex)
            {
                // On ne log pas les time out: trop fréquents.
                if (!ex.Message.Contains("timed out"))
                {
                    log.Error(ex);
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }

            if (log.IsTraceEnabled)
            {
                timer?.Stop();
                log.Trace("GetPost call. Elapsed: " + timer?.ElapsedMilliseconds / 1000.0f + " seconds");
            }

            return posts;
        }

        private Post GetLastPost()
        {
            Stopwatch timer = null;
            if (log.IsTraceEnabled)
            {
                timer = new Stopwatch();
                timer.Start();
            }

            Post post = null;

            try
            {
                var configuration = ServicesContainer.GetService<NomadConfiguration>();

                var server = new Server("192.95.36.173", "le-server", "8186", "8186");

                var request = HttpUtil.GenerateRequest(server, "print-station/GetLastPost", "&event_id=" + _user.EventId,
                    1000);
                var response = (HttpWebResponse)request.GetResponse();

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var jsonSerializer = new DataContractJsonSerializer(typeof(Post));
                    // ReSharper disable once AssignNullToNotNullAttribute
                    var objResponse = jsonSerializer.ReadObject(response.GetResponseStream());
                    post = objResponse as Post;
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }

            if (log.IsTraceEnabled)
            {
                timer?.Stop();
                log.Info("GetLastPost call. Elapsed: " + timer?.ElapsedMilliseconds / 1000.0f + " seconds");
            }

            return post;
        }

        private void DownloadImages(CancellationToken ct)
        {
            Stopwatch timer = null;
            if (log.IsTraceEnabled)
            {
                timer = new Stopwatch();
                timer.Start();
            }

            var posts = _database.GetPostsWithImageToDownload(_user.EventId);

            if (!posts.Any()) return;

            var listeFetch = new List<Task>();

            // On lance chaque tâche dans un Task séparé.
            foreach (var post in posts)
            {
                if (ct.IsCancellationRequested)
                {
                    log.Warn("Cancelled DownloadImages during fetch. One or more DownloadPicture task not launched.");
                    break;
                }

                listeFetch.Add(Task.Run(() => { DownloadPictures(post); }, ct));
            }

            // On attend l'ensemble des tâches.
            try
            {
                Task.WaitAll(listeFetch.ToArray());
            }
            catch (AggregateException)
            {
                // Peut arriver lors d'une fermeture.
                log.Info("One or more GetPost task was cancelled.");
            }

            if (!log.IsTraceEnabled || timer == null) return;
            timer.Stop();
            log.Trace("DownloadImages call. Elapsed: {0}", timer.Elapsed.ToStringHourMinSec(true, true));
        }

        private void DownloadPictures(Post post)
        {
            Stopwatch timer = null;
            if (log.IsTraceEnabled)
            {
                timer = new Stopwatch();
                timer.Start();
            }

            if (!post.PictureDownloaded && post.PictureUrl != null)
            {
                try
                {
                    if (!string.IsNullOrWhiteSpace(post.PictureUrl))
                    {
                        var tentativeFileName = StringUtil.GetFileName(post.PictureUrl);
                        var fileName = DirectoryUtil.GetLocalPrintstationMediaDir(_user.Id, DirectoryUtil.NomadLogicPrintstationDirectoriesEnum.pictures) + tentativeFileName;
                        HttpUtil.DownloadFileFromUrl(post.PictureUrl, fileName);
                        BitmapExtentions.MatchExifData(fileName);
                    }

                    post.PictureDownloaded = true;
                    _database.UpdatePost(post);
                }
                catch (Exception ex)
                {
                    log.Info(ex, "Could not download picture:\n\t- " + post.PictureUrl);
                }
            }

            if (!post.UserPictureDownloaded && post.UserPictureUrl != null)
            {
                try
                {
                    if (!string.IsNullOrWhiteSpace(post.UserPictureUrl))
                    {
                        var tentativeFileName = StringUtil.GetFileName(post.UserPictureUrl);
                        var fileName = DirectoryUtil.GetLocalPrintstationMediaDir(_user.Id, DirectoryUtil.NomadLogicPrintstationDirectoriesEnum.user_profile) + tentativeFileName;

                        if (!File.Exists(fileName)) // Peut exister si une image de profil d'un usager a déjà été téléchargée.
                        {
                            HttpUtil.DownloadFileFromUrl(post.UserPictureUrl, fileName);

                            BitmapExtentions.MatchExifData(fileName);

                        }
                    }

                    post.UserPictureDownloaded = true;
                    _database.UpdatePost(post);
                }
                catch (Exception ex)
                {
                    log.Info(ex, "Could not download profile picture:\n\t- " + post.UserPictureUrl);
                }
            }

            if (log.IsTraceEnabled)
            {
                timer?.Stop();
                log.Info("DownloadPictures call. Elapsed: " + timer?.ElapsedMilliseconds / 1000.0f + " seconds");
            }
        }
    }
}