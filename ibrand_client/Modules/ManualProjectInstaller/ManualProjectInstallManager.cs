using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Ionic.Zip;
using NLog;
using NomadLogic.Common.Serveur;
using NomadLogic.Common.Services.Configuration;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Json;
using NomadLogic.Common.Util.Math;
using NomadLogic.iBrandDesktop.Synchro.DownloadRestRequest;

namespace NomadLogic.iBrandDesktop.Modules.ManualProjectInstaller
{
    /// <summary>
    /// Cette classe vérifie si des paquetages d'installation manuels sont présent sur disque lors du chargement de l'application.
    /// Si oui, le gestionnaire va tenter de soit créer le projet dans la BD et de déplacer les fichiers dans une arborescence adéquate, ou alors de mettre à jour un projet déjà existant.
    /// Si après analyse le paquetage n'est pas valide, il sera déplacé dans un répertoire d'erreur.
    /// Un succès implique que le paquetage est détruit, car son contenu est désormais extrait localement.
    ///
    /// Ce système se veut une façon de contourner la dépendance à un serveur distant. Utilisé dans les circonstances de passage du modèle de Nomad Logic à Kapture de chez ConnectNGo.
    /// </summary>
    public class ManualProjectInstallManager
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        private readonly NomadConfiguration _config;
        private readonly IDatabaseProjectFromServer _db;
        private DirectoryInfo _inputDir;
        private DirectoryInfo _outputDir;
        private List<ManualInstallJobs> _validJobs;
        private List<FileInfo> _invalidJobFiles;

        public ManualProjectInstallManager(IDatabaseProjectFromServer db, NomadConfiguration configuration)
        {
            _db = db;
            _config = configuration;
        }
        
        public DirectoryInfo InputDir { get; set; }
        
        public DirectoryInfo OutputDir { get; set; }

        public bool DebugKeepOriginalFiles { get; set; } = false;

        /// <summary>
        /// Valide que les informations de base sont données via la configuration données.
        /// </summary>
        public bool Init()
        {
            if (_config == null)
            {
                log.Error("No valid config file given."); 
                return false;
            }

            // test de l'endroit de détection des fichiers/paquetages
            _inputDir = null;
            if (InputDir?.Exists ?? false)
            {
                _inputDir = new DirectoryInfo(InputDir.FullName);
            } 
            else 
            {
                _inputDir = new DirectoryInfo(_config.ManualInstallInputDir);
                if (!Directory.Exists(_config.ManualInstallInputDir))
                {
                    _inputDir = Directory.CreateDirectory(_config.ManualInstallInputDir);
                    _inputDir.Refresh();
                }
                
                if (!_inputDir.Exists)
                {
                    log.Error("No input directory manually given or in config file.");
                    return false;
                }
            }

            // test de l'endroit de où mettre les nouveau projets créés.
            _outputDir = null;
            if (OutputDir?.Exists ?? false)
            {
                _outputDir = new DirectoryInfo(OutputDir.FullName);
            } 
            else
            {
                _outputDir = new DirectoryInfo(DirectoryUtil.GetLocalIbrandProjectsBaseDir());
            }

            return true;
        }

        /// <summary>
        /// Cherche un ou plusieurs paquetages d'installations. Si on trouve au moins un fichier de définition d'installation ("install.json"),
        /// on assume que le répertoire ne contient qu'un seul projet à installer, non-paqueté, et que l'ensemble des fichiers sont dans ce répertoire.
        /// Cette méthode identifie et prépare les tâches à effectuer. C'est donc une étape essentielle, préalable à l'exécution des installations.
        /// </summary>
        /// <returns>Le nombre de projets à installer.</returns>
        public int Search()
        {
            // Initialisation des variables de travail.
            _validJobs = new List<ManualInstallJobs>();
            _invalidJobFiles = new List<FileInfo>();

            if (!_inputDir?.Exists ?? true)
            {
                log.Warn("Cannot launch install directory analisys without à defined directory to scan.");
                return 0;
            }

            var files = _inputDir.GetFiles();
            var installNbr = 0;

            // 0- Vérification rapide qu'il n'y a aucun fichier.
            if (!files.Any())
            {
                return 0;
            }

            // 1- Vérification s'il existe une installation "flat", c'est à dire pas compressé dans un ZIP.
            installNbr += AnalyseTerminalFile(files, _inputDir, out var findIt);

            if (findIt) return installNbr;

            // 2- Sinon, on vérifie s'il existe des fichiers ZIP dans le répertoire, et que ces derniers contiennent un fichier d'installation valide.
            var zipFiles = files.Where(f => f.Extension.Equals(".zip", StringComparison.InvariantCultureIgnoreCase)).ToList();
            if (zipFiles.Any())
            {
                foreach (var fi in zipFiles)
                {
                    log.Trace("Verifying detected zip file: {0}", fi);

                    var workDir = DirectoryUtil.ExtractZipToLocalTemporaryDirectory(fi.FullName);
                    
                    try
                    {
                        installNbr += AnalyseTerminalFile(workDir.GetFiles(), workDir, out findIt, true);
                    }
                    finally
                    {

                        if (!findIt)
                        {
                            try
                            {
                                Thread.Sleep(100);
                                FileUtil.DeleteFolderContent(workDir.FullName);
                            }
                            catch (System.Exception)
                            {
                                // Die nice
                            }
                        }
                    }
                }

                return _validJobs.Count;
            }

            // Sinon, il n'y a rien, mais il y a des fichiers inutiles.
            log.Warn("Superfluous files in the install directory. Verify this.");

            return 0;
        }

        private int AnalyseTerminalFile(FileInfo[] files, DirectoryInfo workDir, out bool validFile, bool delDirectory = false)
        {
            validFile = false;
            if (!files.Any())
            {
                return 0;
            }

            // 1- On vérifie si un fichier explicite d'installation existe (et qu'il est valide). Si oui, il y a un seul projet à installer.

            var terminalFile = files.FirstOrDefault(f => f.Name.Equals("install.json", StringComparison.InvariantCultureIgnoreCase));
            if (terminalFile == null) return 0;


            var tdi = JsonUtil.ReadJsonFile<TerminalDownloadInfo>(terminalFile);
            if (tdi == null) return 0;

            tdi.FilesToDownload = files.Select(f => new ResourceFileToDownload(f)).ToList();
            if (tdi.Validate(out _))
            {
                log.Trace("Added install file as workable job: {0}", terminalFile);
                _validJobs.Add(new ManualInstallJobs(workDir, terminalFile, delDirectory));
                {
                    validFile = true;
                    return 1;
                }
            }

            log.Trace("Detected install manifest file as unworkable: {0}", terminalFile);
            _invalidJobFiles.Add(terminalFile);
            {
                return 0;
            }

        }

        /// <summary>
        /// Exécute les jobs valides trouvées dans l'étape de recherche (Search).
        /// </summary>
        /// <param name="logger">Référence vers un logger spécifique pour logger les informations. Si null, va utiliser celui par défaut.</param>
        public ManualProjectInstallResults Process(Logger logger)
        {
            var useLog = logger ?? log;
            
            // 0- Rien à faire.
            if (_validJobs == null || _validJobs.Count == 0)
            {
                useLog.Warn("No job identified. Returning nothing.");
                return new ManualProjectInstallResults();
            }

            var failedJobs = new List<ManualInstallJobs>();
            var succesfulJobs = new List<ManualInstallJobs>();

            // Pour chaque job
            foreach (var job in _validJobs)
            {
                var dir = job.BaseDirectory;
                if (dir == null) continue;
                var files = dir.GetFiles();

                job.TotalJobFileSize = ProcessJob(files, useLog, job, succesfulJobs, failedJobs);
            }
            
            // Cleanup
            // 1- Vide la liste de job faites.
            _validJobs.Clear();

            var rez = new ManualProjectInstallResults
            {
                TotalJobs = failedJobs.Count + succesfulJobs.Count
            };
            foreach (var jobRez in from j in succesfulJobs let tdi = j.TerminalInfo select new ManualProjectInstallResult
                     {
                         Success = true,
                         Message = $"Terminal '{tdi.TerminalName}' ({tdi.ProjectId}/{tdi.QuestionnaireId}/{tdi.TerminalId}) successfully installed.",
                         FilesCount = j.TerminalInfo.FilesToDownload.Count,
                         FilesTotalSize = j.TotalJobFileSize,
                         TerminalInstallDirectory = j.FinalOutputDirectory
                     })
            {
                rez.Results.Add(jobRez);
                rez.SuccessCount++;
            }

            foreach (var jobRez in from j in failedJobs let tdi = j.TerminalInfo select new ManualProjectInstallResult
                     {
                         Success = false,
                         Message = $"Terminal '{tdi.TerminalName}' ({tdi.ProjectId}/{tdi.QuestionnaireId}/{tdi.TerminalId}) failed installation.",
                         ErrorCause = j.Exception,
                         FilesCount = j.TerminalInfo.FilesToDownload.Count,
                         FilesTotalSize = j.TotalJobFileSize
                     })
            {
                rez.Results.Add(jobRez);
                rez.FailuresCount++;
            }

            if (!DebugKeepOriginalFiles && rez.FailuresCount == 0 && !_invalidJobFiles.Any())
            {
                // input directory should be empty.
                _inputDir.Refresh();
                var leftovers = _inputDir.GetFileSystemInfos();
                if (leftovers.Length > 0)
                {
                    useLog.Warn("There are files left over since all installation happened. Please verify this.");
                }
            }

            _invalidJobFiles.Clear();

            return rez;
        }

        private long ProcessJob(FileInfo[] files, Logger useLog, ManualInstallJobs job, List<ManualInstallJobs> succesfulJobs, List<ManualInstallJobs> failedJobs)
        {
            long totalFileSize = 0;

            // Get install manifest
            var terminalFile = files.FirstOrDefault(f => f.Name.Equals("install.json", StringComparison.InvariantCultureIgnoreCase));
            var tdi = JsonUtil.ReadJsonFile<TerminalDownloadInfo>(terminalFile);

            // Give files to manifest (overrides if previous)
            tdi.FilesToDownload = files.Select(f => new ResourceFileToDownload(f)).ToList();

            job.TerminalInfo = tdi;

            try
            {
                // validate
                if (!tdi.Validate(out var errorMessage))
                {
                    useLog.Error("Manual install job failed at validation: " + errorMessage);
                    job.Exception = new FileNotFoundException(errorMessage);
                    failedJobs.Add(job);
                    return 0;
                }

                var terminalDirPath = Path.Combine(_outputDir.FullName, tdi.ProjectId.ToString(), tdi.QuestionnaireId.ToString(), tdi.TerminalId.ToString());
                if (!Directory.Exists(terminalDirPath))
                {
                    Directory.CreateDirectory(terminalDirPath);
                }

                job.FinalOutputDirectory = new FileInfo(terminalDirPath);

                foreach (var ftd in tdi.FilesToDownload)
                {
                    var originalFile = ftd.LocalFileInfo;

                    // set destination file
                    var outputFile = Path.Combine(terminalDirPath, originalFile.Name);

                    useLog.Trace("Install: Moved file to => {0}", outputFile);
                    File.Copy(originalFile.FullName, outputFile, true);

                    totalFileSize += originalFile.SizeInBytes();
                }
                
                log.Trace("Job file move finished: {0} moved.", MathUtil.ByteNumberReadable(totalFileSize));

                // Si tout s'est bien passé.
                if (!_db.IsTerminalAlreadyInserted(tdi.TerminalId))
                {
                    // Sauvegarde dans la BD
                    var res = _db.InsertTerminal(tdi);
                    if (res == 0)
                    {
                        throw new System.Exception("Could not add terminal in database: " + tdi.TerminalId);
                    }
                }
            
                _db.ManageRessourceFiles(tdi.TerminalId, tdi.FilesToDownload);
                _db.ResetTerminalMetadata(tdi);

                succesfulJobs.Add(job);

                // Cleanup
                if (DebugKeepOriginalFiles) return totalFileSize;
                
                foreach (var ftd in tdi.FilesToDownload)
                {
                    ftd.LocalFileInfo.Delete();
                    ftd.LocalFileInfo.Refresh();
                }

                if (job.DeleteBaseDirectory)
                {
                    FileUtil.DeleteFolderContent(job.BaseDirectory.FullName);
                }

                return totalFileSize;
            }
            catch (System.Exception e)
            {
                useLog.Error(e, "Installation failed. Job main file = " + job.MainFile);
                job.Exception = e;
                failedJobs.Add(job);
                return 0;
            }
        }
    }

    internal class ManualInstallJobs
    {
        public FileInfo MainFile { get; }
        public TerminalDownloadInfo TerminalInfo { get; set; }
        public long TotalJobFileSize { get; set; }
        public System.Exception Exception { get; set; }
        public FileInfo FinalOutputDirectory { get; set; }
        public DirectoryInfo BaseDirectory { get; }
        public bool DeleteBaseDirectory { get; set; }

        public ManualInstallJobs(DirectoryInfo baseDir, FileInfo terminalFile, bool deleteBaseDir)
        {
            MainFile = terminalFile;
            BaseDirectory = baseDir;
            DeleteBaseDirectory = deleteBaseDir;
        }

    }
}