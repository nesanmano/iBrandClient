using System;
using System.IO;
using System.Windows.Forms;
using Ionic.Zip;
using JetBrains.Annotations;
using NLog;
using NomadLogic.Common.Serveur;
using NomadLogic.Common.Services;
using NomadLogic.Common.Services.Configuration;

// ReSharper disable InconsistentNaming
// ReSharper disable HeuristicUnreachableCode

namespace NomadLogic.Common.Util
{
    /// <summary>
    /// Cette classe sert à gérer les répertoires correctements entre toutes les applications de la famille iBrand de Nomad Logic.
    /// <br/>Elle vise à utiliser correctement les conventions de répertoire temporaires, liés à l'usager et de répertoires d'installations,
    /// de façon à éviter les situations suivantes:
    /// <br/>-Écrire des fichiers dans des zones protégées (par exemple, dans le répertoire d'installation original).
    /// <br/>-Mettre des fichiers partagés entre appliciations dans des endroits difficiles d'accès, ou dans des endroits où des conflits d'écriture peuvent survenir.
    /// <br/>-Mettre des fichiers temporaires dans des endroits non-temporaires, pouvant mener à des pertes d'espaces disque inutiles.
    /// <br/>-Créer des duplicatas de répertoires à fin similaires entre différentes applications.
    /// <br/><br/>Le but est donc de s'assurer d'écrire les bon fichiers dans les bons répertoires, de posséder un nombre de répertoires minimal et correctement
    /// organiser pour de multiples applications de la famille iBrand. Et le tout, en respectant les conventions Windows de développement logiciel pour prévenir
    /// des problèmes de compatibilités entre versions de Windows.
    /// <br/><br/>Plus précisément, on divisera les fichiers utilisés par nos applications en deux sections, chacune avec ses propres règles:
    /// <br/>-Une section d'installation, où les exécutables et DLLS seront mis. (Program Files)
    /// <br/>-Une section de répertoires dans le AppData pour tous les fichiers créés ou géré par l'application.
    /// Cette dernière section sera par la suite divisée en plusieures sections différentes:
    /// <br/>-Chaque logiciel iBrand distinct aura un répertoire distinct où sera stocké sa propre abstractConfiguration (fichiers de SQLite, fichier de configurations local). Chacun de
    /// ces répertoires auront des sous-répertoires pour stocker les informations de chaque projet qu'il contient, ainsi que des répertoires pour stocker les fichiers multimédias
    /// obtenus par les caméras.
    /// Voir ce lien pour plus d'information: http://stackoverflow.com/questions/867485/c-sharp-getting-the-path-of-appdata
    /// </summary>
    [PublicAPI]
    public static class DirectoryUtil
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        private const string CONST_NOMAD_LOGIC = "Nomad Logic";
        private const string CONST_PROJECTS_DIR = "projects";
        private const string CONST_WORK_DIR = "work";
        private const string CONST_MEDIA_DIR = "media";
        private const string CONST_BKP_DIR = "backup";
        private const string CONST_ASSETS = "assets";
        private const string CONST_LOG = "logs";
        private const string CONST_SUSPECT_MEDIA = "suspect_media";

        /// <summary>
        /// Cette énumération contient les principales applications de Nomad Logic qui utilisent ce système de gestion de répertoire.
        /// </summary>
        [PublicAPI]
        public enum ApplicationNameEnum
        {
            iBrand,
            Printstation,
            SnapShot,
            ShowPictureOnTv,
            Test,
            Iris,
            CameraRingClient,
            Nomad,
            DatabaseExtractor,
            VideoEventCapture
        }

        /// <summary>
        /// Répertoires habituels pour des sous-répertoires d'un répertoire de assets fichiers d'un terminal iBrand.
        /// </summary>
        public enum ProjectDataDirectoriesEnum
        {
            pictures,
            videos,
            signatures,
            printstation_ui,
            photobooth_ui
        }

        /// <summary>
        /// Sous-répertoires pour fichiers modifiés d'un projet iBrand.
        /// </summary>
        public enum ProjectDataDirectoriesModEnum
        {
            none,
            transformed,
            thumbnail,
            gif,
            print,
            socialwall,
            bundle,
            firstpicture
        }

        /// <summary>
        /// Répertoires pour fichiers multimédias d'une application.
        /// </summary>
        [PublicAPI]
        public enum NomadLogicMediaDirectoriesEnum
        {
            pictures,
            videos,
            videos_thumbnail,
            camera_ring,
            pictures_nikon,
            listen_from_ibrand,
            screens,
            show_on_tv
        }

        /// <summary>
        /// Répertoires d'un projet spécifique pour le logiciel PrintStation
        /// </summary>
        public enum NomadLogicPrintstationDirectoriesEnum
        {
            pictures,
            user_profile,
            ui,
            file_bundles,
            print,
            thumbnails
        }

        /// <summary>
        /// Répertoires commun à tous les projets pour l'application PrintStation
        /// </summary>
        [PublicAPI]
        public enum NomadLogicPrintstationDefaultDirectoriesEnum
        {
            default_ui,
            contest
        }

        /// <summary>
        /// Cette méthode s'assure que le répertoire donné en paramètre existe et soit utilisable, sinon lance une
        /// exception.
        /// <br/>Un répertoire utilisable est un répertoire qui: existe, est lisible, peut être écrit.
        /// </summary>
        /// <param name="dir">Le répertoire à vérifier.</param>
        [PublicAPI]
        public static void ValidateDirectory(string dir)
        {
            if (Directory.Exists(dir)) return;

            Directory.CreateDirectory(dir);
        }

        /// <summary>
        /// Obtient le répertoire d'application local de l'usager de Windows actuel.
        /// </summary>
        /// <returns>Le répertoire en question.</returns>
        [PublicAPI]
        public static string GetLocalUserDir()
        {
            var baseDir = DebugLocalUserDir ??  Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + Path.DirectorySeparatorChar;
            return baseDir;
        }

        /// <summary>
        /// Utilisé dans un environement de débogage ou de test, pour changer de façon directe le répertoire "local" de l'usager.
        /// </summary>
        public static string DebugLocalUserDir { get; set; }

        /// <summary>
        /// Obtient le répertoire commun de toutes les applications faites par NomadLogic.
        /// </summary>
        /// <returns>Le répertoire.</returns>
        public static string GetLocalNomadLogicDir()
        {
            var local = GetLocalUserDir() + CONST_NOMAD_LOGIC + Path.DirectorySeparatorChar;
            ValidateDirectory(local);
            return local;
        }

        /// <summary>
        /// Obtient le répertoire de l'application en cours (tel que configuré dans la classe NomadConfiguration)
        /// </summary>
        /// <returns>Le répertoire de l'application.</returns>
        public static string GetLocalApplicationDir()
        {
            var conf = ServicesContainer.GetService<NomadConfiguration>();
            if (conf == null) {
                throw new Exception("NomadConfiguration service has not been configured yet.");
            }
            return GetLocalApplicationDir(conf.ApplicationName);
        }

        /// <summary>
        /// Obtient le répertoire de travail de base pour une application Nomad Logic spécifique.
        /// <br/>Normalement, on utilise plutôt la méthode "GetLocalApplicationDir()", mais dans les cas où il est impossible
        /// d'avoir un objet de NomadConfiguration valide (par exemple, durant l'initialisation de cette classe de NomadConfiguration justement),
        /// il faut utiliser cette méthode en spécifiant l'application voulue.
        /// <br/>De ce fait, cette méthode est utilisée lors de l'initialisation statique d'une application, ou quand on cherche un répertoire
        /// qui n'appartient pas normalement à l'application qui s'exécute.
        /// </summary>
        /// <param name="app">L'application en question.</param>
        /// <returns>Le répertoire de travail.</returns>
        public static string GetLocalApplicationDir(ApplicationNameEnum app)
        {
            var local = GetLocalNomadLogicDir() + app + Path.DirectorySeparatorChar;
            ValidateDirectory(local);
            return local;
        }

        /// <summary>
        /// Obtient le répertoire de travail d'une application en cours. Idéalement utilisé pour stocker des fichiers temporaires, car il n'est pas
        /// en rapport avec un projet en particulier.
        /// </summary>
        /// <returns>Le répertoire de travail de l'application.</returns>
        public static string GetLocalApplicationWorkDir()
        {
            var local = GetLocalApplicationDir() + CONST_WORK_DIR + Path.DirectorySeparatorChar;
            ValidateDirectory(local);
            return local;
        }

        /// <summary>
        /// Obtient le répertoire où sont stockés les fichiers multimédia acquis de sources externes d'une application en cours.
        /// </summary>
        /// <returns>Le répertoire de media de l'application.</returns>
        public static string GetLocalApplicationMediaDir()
        {
            var local = GetLocalApplicationDir() + CONST_MEDIA_DIR + Path.DirectorySeparatorChar;
            ValidateDirectory(local);
            return local;
        }

        /// <summary>
        /// Obtient le répertoire des journaux d'une application. Chaque application à son propre journal.
        /// </summary>
        public static string GetLocalApplicationLogsDir()
        {
            var local = GetLocalApplicationDir() + CONST_LOG + Path.DirectorySeparatorChar;
            ValidateDirectory(local);
            return local;
        }

        /// <summary>
        /// Répertoire qui contient des fichiers suspect, qui doivent être conservés (souvent à fins de débogage) mais également isolé
        /// du reste de l'application.
        /// </summary>
        public static string GetLocalApplicationSuspectMediaDir()
        {
            var local = GetLocalApplicationDir() + CONST_SUSPECT_MEDIA + Path.DirectorySeparatorChar;
            ValidateDirectory(local);
            return local;
        }

        /// <summary>
        /// Obtient le répertoire de base d'un projet spécifique de l'application iBrand.
        /// </summary>
        /// <param name="project_id">L'identifiant du projet.</param>
        public static string GetLocalIbrandProjectDir(int project_id)
        {
            var local = GetLocalIbrandProjectsBaseDir() + project_id + Path.DirectorySeparatorChar;
            ValidateDirectory(local);
            return local;
        }

        /// <summary>
        /// Répertoire de base pour tous les projets contenu dans l'application iBrand.
        /// </summary>
        public static string GetLocalIbrandProjectsBaseDir()
        {
            var local = GetLocalApplicationDir(ApplicationNameEnum.iBrand) + CONST_PROJECTS_DIR + Path.DirectorySeparatorChar;
            ValidateDirectory(local);
            return local;
        }

        /// <summary>
        /// Obtient le répertoire d'un questionnaire précis, d'une projet iBrand précis.
        /// </summary>
        /// <param name="project_id">L'identifiant du projet.</param>
        /// <param name="questionnaire_id">L'identifiant du questionnaire.</param>
        /// <returns></returns>
        [Obsolete("Use a terminal-based lookup-instead.", true)]
        public static string GetLocalIbrandQuestionnaireDir(int project_id, int questionnaire_id)
        {
            var local = GetLocalIbrandProjectDir(project_id) + questionnaire_id + Path.DirectorySeparatorChar;
            ValidateDirectory(local);
            return local;
        }

        /// <summary>
        /// Obtient un répertoire de données d'un questionnaire iBrand.
        /// <br/>On peut faire référence à un sous-répertoire particulier avec cette méthode, qui
        /// correspond à un répertoire qui contient des fichiers modifiés à partir d'un original (qui se trouve dans le répertoire de donnée de base), et ce pour une
        /// tâche précise. Par exemple, un sous répertoire peut faire référence à un fichier fait pour l'impression, un autre pour un GIF, ou une transformation d'un fichier
        /// original. Le but de ces sous-répertoire est de sauvegarder le fichier original, même si les fichiers transformés peuvent avoir un nom totalement différent.
        /// </summary>
        /// <param name="project_id">L'identifiant de projet.</param>
        /// <param name="questionnaire_id">L'identifiant de questionnaire.</param>
        /// <param name="dataDir">Le type de répertoire de données.</param>
        /// <param name="mod">Le sous-répertoire de fichiers modifiés. Si on ne veut pas de sous-répertoire, ce paramètre peut être null, ou l'on peut utiliser l'énumération 'none'.</param>
        /// <returns>Le répertoire de données (modifiés ou non).</returns>
        [Obsolete("Use a terminal-based lookup-instead.", true)]
        public static string GetLocalIbrandQuestionnaireDataDir(int project_id, int questionnaire_id, ProjectDataDirectoriesEnum dataDir, ProjectDataDirectoriesModEnum mod)
        {
            return GetLocalIbrandQuestionnaireCustomDir(project_id, questionnaire_id, dataDir.ToString(), mod);
        }
        
        /// <summary>
        /// Obtient un répertoire de données d'un questionnaire.
        /// <br/>On peut faire référence à un sous-répertoire particulier avec cette méthode, qui
        /// correspond à un répertoire qui contient des fichiers modifiés à partir d'un original (qui se trouve dans le répertoire de donnée de base), et ce pour une
        /// tâche précise. Par exemple, un sous répertoire peut faire référence à un fichier fait pour l'impression, un autre pour un GIF, ou une transformation d'un fichier
        /// original. Le but de ces sous-répertoire est de sauvegarder le fichier original, même si les fichiers transformés peuvent avoir un nom totalement différent.
        /// </summary>
        /// <param name="project_id">L'identifiant de projet.</param>
        /// <param name="questionnaire_id">L'identifiant de questionnaire.</param>
        /// <param name="dataDir">Le type de répertoire de données.</param>
        /// <param name="mod">Le sous-répertoire de fichiers modifiés. Si on ne veut pas de sous-répertoire, ce paramètre peut être null, ou l'on peut utiliser l'énumération 'none'.</param>
        /// <returns>Le répertoire de données (modifiés ou non).</returns>
        [Obsolete("Use a terminal-based lookup-instead.", true)]
        public static string GetLocalIbrandQuestionnaireDataDir(int project_id, string questionnaire_id, ProjectDataDirectoriesEnum dataDir, ProjectDataDirectoriesModEnum mod)
        {
            return GetLocalIbrandQuestionnaireCustomDir(project_id, int.Parse(questionnaire_id), dataDir.ToString(), mod);
        }

        /// <summary>
        /// Dans les cas où les options de l'énumération "ProjectDataDirectoriesEnum" sont insuffisants pour la méthode "GetLocalIbrandQuestionnaireDataDir", on peut utiliser
        /// cette méthode pour faire des recherches dans un répertoire avec un nom précis. On peut tout de même utiliser les options de sous-répertoires.
        /// </summary>
        /// <param name="project_id">L'identifiant de projet.</param>
        /// <param name="questionnaire_id">L'identifiant de questionnaire.</param>
        /// <param name="customDir">Le nom personnalisé du répertoire de données.</param>
        /// <param name="mod">Le sous-répertoire de fichiers modifiés. Si on ne veut pas de sous-répertoire, ce paramètre peut être null, ou l'on peut utiliser l'énumération 'none'.</param>
        /// <returns>Le répertoire de données (modifiés ou non).</returns>
        [Obsolete("Use a terminal-based lookup-instead.", true)]
        public static string GetLocalIbrandQuestionnaireCustomDir(int project_id, int questionnaire_id, string customDir, ProjectDataDirectoriesModEnum mod = ProjectDataDirectoriesModEnum.none)
        {
            var local = GetLocalIbrandQuestionnaireDir(project_id, questionnaire_id) + customDir + Path.DirectorySeparatorChar;
            if (mod != ProjectDataDirectoriesModEnum.none) {
                local += mod.ToString() + Path.DirectorySeparatorChar;
            }
            ValidateDirectory(local);
            return local;
        }

        /// <summary>
        /// Va directement chercher le répertoire d'images de l'UI Photobooth associé au projet.
        /// <br/>Si une langue est spécifiée, elle sera ajoutée dans l'hiérarchie de répertoires de recherche.
        /// </summary>
        /// <param name="project_id">L'identifiant numérique du projet</param>
        /// <param name="questionnaire_id">L'identifiant numérique du questionnaire</param>
        /// <param name="language">Le code de langue. Peut être nul.</param>
        /// <returns>Le répertoire où sont stockés les fichiers.</returns>
        [Obsolete("Use a terminal-based lookup-instead.", true)]
        public static string GetLocalIbrandQuestionnairePhotoBoothDataDir(int project_id, int questionnaire_id, string language)
        {
            var local = GetLocalIbrandQuestionnaireDir(project_id, questionnaire_id) + ProjectDataDirectoriesEnum.photobooth_ui + Path.DirectorySeparatorChar;
            if (!string.IsNullOrEmpty(language)) {
                local += language + Path.DirectorySeparatorChar;
            }
            ValidateDirectory(local);
            return local;
        }

        /// <summary>
        /// Similaire à la méthode "GetLocalIbrandQuestionnairePhotoBoothDataDir(int, int, string)", mais qui va dans le répertoire par défaut, où doivent se trouver
        /// les fichiers du PhotoBooth commun à tous les projets.
        /// </summary>
        /// <param name="language">Optionnel. Le code de langue du répertoire pour une version spécifique d'un fichier.</param>
        /// <returns>Le répertoire final.</returns>
        public static string GetLocalPhotoBoothDefaultAssetsDir(string language = null)
        {
            var local = GetLocalNomadLogicAssetsDir() + ProjectDataDirectoriesEnum.photobooth_ui + Path.DirectorySeparatorChar;
            if (!string.IsNullOrEmpty(language))
            {
                local += language + Path.DirectorySeparatorChar;
            }
            ValidateDirectory(local);
            return local;
        }

        /// <summary>
        /// Similaire à la méthode "GetLocalIbrandQuestionnairePhotoBoothDataDir(int, int, string)", mais qui va dans le répertoire par défaut, où doivent se trouver
        /// les fichiers du PhotoBooth commun à tous les projets.
        /// </summary>
        /// <returns>Le répertoire final.</returns>
        [Obsolete]
        public static string GetPrintstationUiDefaultAssetsDir()
        {
            var local = GetLocalNomadLogicAssetsDir() + ProjectDataDirectoriesEnum.printstation_ui + Path.DirectorySeparatorChar;
            ValidateDirectory(local);
            return local;
        }

        /// <summary>
        /// Obtient le répertoire où sont stockés les fichiers multimédias créés par le matériel de capture. Ces fichiers ne devraient pas être modifiés.
        /// </summary>
        /// <param name="dir">Le sous-réperpertoire voulu.</param>
        /// <returns>Le chemin absolu du répertoire demandé.</returns>
        public static string GetLocalMediaDir(NomadLogicMediaDirectoriesEnum dir)
        {
            var local = GetLocalApplicationDir() + CONST_MEDIA_DIR + Path.DirectorySeparatorChar + dir + Path.DirectorySeparatorChar;
            ValidateDirectory(local);
            return local;
        }

        /// <summary>
        /// Obtient le répertoire de sauvegarde externe, tel que spécifié par la abstractConfiguration en paramètre.
        /// <br/>Il est préférable d'utiliser la méthode "IsBackupDirAvailiable" pour s'assurer que le répertoire existe bel et bien, car souvent ce répertoire
        /// est contenu sur un disque externe qui peut être débranché.
        /// </summary>
        /// <param name="abstractConfiguration">La abstractConfiguration de laquelle aller chercher le répertoire de sauvegarde.</param>
        /// <returns>Le répertoire de sauvegarde de base.</returns>
        public static string GetBackupDir(NomadConfiguration abstractConfiguration)
        {
            var local = abstractConfiguration.BackupDrive + Path.DirectorySeparatorChar + CONST_NOMAD_LOGIC + Path.DirectorySeparatorChar + CONST_BKP_DIR + Path.DirectorySeparatorChar;
            ValidateDirectory(local);
            return local;
        }

        /// <summary>
        /// Indique si le répertoire de sauvegarde est accessible.
        /// </summary>
        /// <param name="conf">La abstractConfiguration de laquelle aller chercher le répertoire de sauvegarde.</param>
        /// <returns>TRUE si le répertoire est accessible.</returns>
        public static bool IsBackupDirAvailiable(NomadConfiguration conf)
        {
            try
            {
                GetBackupDir(conf);
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Créer un chemin de fichier temporaire avec le nom voulu dans le répertoire temporaire de l'usager Windows actuel.
        /// <br/>À noter que le fichier n'est pas créé par cette méthode, uniquement son chemin au besoin.
        /// </summary>
        /// <param name="filename">Le nom de fichier à créer.</param>
        /// <param name="tempPath">Si non-nul, utilise ce chemin plutôt que le répertoire temporaire de la machine (via variable d'environnement).</param>
        /// <returns>Un chemin pour un fichier temporaire.</returns>
        [NotNull]
        public static string CreateLocalTemporaryFile(string filename = null, string tempPath = null)
        {
            if (string.IsNullOrWhiteSpace(filename)) filename = GenerateUniqueId.GetId() + ".tmp";
            var dir = tempPath ?? Path.GetTempPath() + CONST_NOMAD_LOGIC + Path.DirectorySeparatorChar;
            var myTempFile = Path.Combine(dir, filename);
            ValidateDirectory(dir);
            return myTempFile;
        }

        /// <summary>
        /// Retourne un répertoire de travail nouvellement créé. Ce répertoire se veut "temporaire" et devrait être détruit après son usage.
        /// </summary>
        /// <returns>Une référence vers le répertoire nouvellement créé.</returns>
        public static DirectoryInfo CreateTemporaryDirectory()
        {
            var dir = GetLocalApplicationWorkDir() + GenerateUniqueId.GetId() + Path.DirectorySeparatorChar;
            ValidateDirectory(dir);
            return new DirectoryInfo(dir);
        }

        /// <summary>
        /// Prend un fichier ZIP valide et l'explose dans un répertoire temporaire. Méthode facile d'utilisation.
        /// </summary>
        /// <param name="zipFile">Le fichier à exploser.</param>
        /// <param name="directoryextract">On peut donner un répertoire précis pour l'extraction, au besoin.</param>
        /// <returns>Le répertoire valide. Si le fichier n'est pas valide ou si une erreur s'est produite, retourne null.</returns>
        public static DirectoryInfo ExtractZipToLocalTemporaryDirectory(string zipFile, DirectoryInfo directoryextract = null) {

            if (zipFile == null || !File.Exists(zipFile) || !zipFile.EndsWith(".zip", StringComparison.InvariantCultureIgnoreCase)) return null;

            string baseDir;

            if (directoryextract != null)
            {
                // On prend le répertoire donné à la place.
                baseDir = directoryextract.FullName;
            }
            else
            {
                baseDir = GetLocalApplicationWorkDir() + GenerateUniqueId.GetId() + Path.DirectorySeparatorChar;
            }
            ValidateDirectory(baseDir);

            try {
                using (var zip = new ZipFile(zipFile)) {
                    foreach (var e in zip.Entries) { e.Extract(baseDir); }
                }
            }
            catch (Exception e) {
                log.Error(e, $"Could not extract zip file to directory. Zip={zipFile}, Dir={baseDir}");
                return null;
            }

            return directoryextract ?? new DirectoryInfo(baseDir);
        }

        /// <summary>
        /// Retourne le chemin de l'exécution de l'application en cours.
        /// </summary>
        /// <returns>Le répertoire d'exécution.</returns>
        public static string GetCurrentApplicationInstallDir()
        {
            return Application.StartupPath + Path.DirectorySeparatorChar;
        }

        /// <summary>
        /// Retourne le chemin du répertoire de fichiers statiques de l'application en cours. Ces fichiers sont communs
        /// à toutes les applications de Nomad Logic et donc ne devraient être accédés qu'en lecture seule.
        /// </summary>
        /// <returns>Le chemin de fichiers statiques.</returns>
        [PublicAPI]
        public static string GetLocalNomadLogicAssetsDir()
        {
            return GetLocalNomadLogicDir() +
                   ApplicationNameEnum.Nomad + Path.DirectorySeparatorChar +
                   CONST_ASSETS + Path.DirectorySeparatorChar;
        }

        /// <summary>
        /// Retourne le chemin des fichiers multimédias d'un usager particulier pour l'application PrintStation.
        /// </summary>
        /// <param name="userId">L'identifiant de l'usager.</param>
        /// <param name="dir">Le répertoire spécifique d'un type de fichier multimédia.</param>
        /// <returns></returns>
        [Obsolete]
        public static string GetLocalPrintstationMediaDir(int userId, NomadLogicPrintstationDirectoriesEnum dir)
        {
            var local = GetLocalApplicationDir(ApplicationNameEnum.Printstation) + CONST_MEDIA_DIR + Path.DirectorySeparatorChar + userId + Path.DirectorySeparatorChar + dir + Path.DirectorySeparatorChar;
            ValidateDirectory(local);
            return local;
        }

        /// <summary>
        /// Le répertoire de fichiers multimédias propre à tous les usagers de l'application PrintStation.
        /// </summary>
        /// <param name="dir">Le sous-répertoire spécifique à chercher.</param>
        /// <returns>Le répertoire validé.</returns>
        [Obsolete]
        public static string GetLocalPrintstationDefaultDir(NomadLogicPrintstationDefaultDirectoriesEnum dir)
        {
            var local = GetLocalApplicationDir(ApplicationNameEnum.Printstation) + CONST_MEDIA_DIR + Path.DirectorySeparatorChar + dir + Path.DirectorySeparatorChar;
            ValidateDirectory(local);
            return local;
        }

        /// <summary>
        /// Retourne le répertoire par défaut de ShowPicturesOnTv pour les images à afficher avec un nouveau sous-répertoire donné
        /// en paramètre. Ce répertoire est souvent utilisé pour les "pictures bundle".
        /// <br/>C'est le répertoire où ShowPicturesOnTv prend les nouvelles images à afficher (par défaut), plus un sous-répertoire nommé.
        /// </summary>
        /// <param name="subDir">Optionnel. Le sous-répertoire spécifique à chercher. Si null ou vide, on retourne le répertoire de base.</param>
        /// <returns>Le répertoire, validé.</returns>
        public static string GetLocalShowPicturesOnTvPicturesDir(string subDir = null)
        {
            var local = GetLocalApplicationDir(ApplicationNameEnum.ShowPictureOnTv) +
                        NomadLogicMediaDirectoriesEnum.pictures + Path.DirectorySeparatorChar +
                        (!string.IsNullOrEmpty(subDir) ? subDir + Path.DirectorySeparatorChar : "");
            ValidateDirectory(local);
            return local;
        }

        /// <summary>
        /// Obtient un répertoire de données d'un terminal iBrand.
        /// <br/>On peut faire référence à un sous-répertoire particulier avec cette méthode, qui
        /// correspond à un répertoire qui contient des fichiers modifiés à partir d'un original (qui se trouve dans le répertoire de donnée de base), et ce pour une
        /// tâche précise. Par exemple, un sous répertoire peut faire référence à un fichier fait pour l'impression, un autre pour un GIF, ou une transformation d'un fichier
        /// original. Le but de ces sous-répertoire est de sauvegarder le fichier original, même si les fichiers transformés peuvent avoir un nom totalement différent.
        /// </summary>
        /// <param name="project_id">L'identifiant de projet.</param>
        /// <param name="questionnaire_id">L'identifiant de questionnaire.</param>
        /// <param name="terminal_id">L'identifiant du terminal.</param>
        /// <param name="dataDir">Le type de répertoire de données.</param>
        /// <param name="mod">Le sous-répertoire de fichiers modifiés. Si on ne veut pas de sous-répertoire, ce paramètre peut être null, ou l'on peut utiliser l'énumération 'none'.</param>
        /// <returns>Le répertoire de données (modifiés ou non).</returns>
        public static string GetLocalIbrandTerminalDataDir(int project_id, int questionnaire_id, int terminal_id, ProjectDataDirectoriesEnum dataDir, ProjectDataDirectoriesModEnum mod)
        {
            return GetLocalIbrandTerminalCustomDir(project_id, questionnaire_id, terminal_id, dataDir.ToString(), mod);
        }

        /// <summary>
        /// Obtient un répertoire de données d'un terminal iBrand.
        /// <br/>On peut faire référence à un sous-répertoire particulier avec cette méthode, qui
        /// correspond à un répertoire qui contient des fichiers modifiés à partir d'un original (qui se trouve dans le répertoire de donnée de base), et ce pour une
        /// tâche précise. Par exemple, un sous répertoire peut faire référence à un fichier fait pour l'impression, un autre pour un GIF, ou une transformation d'un fichier
        /// original. Le but de ces sous-répertoire est de sauvegarder le fichier original, même si les fichiers transformés peuvent avoir un nom totalement différent.
        /// </summary>
        /// <param name="tInfo">Instance de définition du terminal.</param>
        /// <param name="dataDir">Le type de répertoire de données.</param>
        /// <param name="mod">Le sous-répertoire de fichiers modifiés. Si on ne veut pas de sous-répertoire, ce paramètre peut être null, ou l'on peut utiliser l'énumération 'none'.</param>
        /// <returns>Le répertoire de données (modifiés ou non).</returns>
        public static string GetLocalIbrandTerminalDataDir(TerminalInfo tInfo, ProjectDataDirectoriesEnum dataDir, ProjectDataDirectoriesModEnum mod)
        {
            return GetLocalIbrandTerminalCustomDir(tInfo, dataDir.ToString(), mod);
        }

        /// <summary>
        /// Dans les cas où les options de l'énumération "ProjectDataDirectoriesEnum" sont insuffisants pour la méthode "GetLocalIbrandQuestionnaireDataDir", on peut utiliser
        /// cette méthode pour faire des recherches dans un répertoire avec un nom précis. On peut tout de même utiliser les options de sous-répertoires.
        /// </summary>
        /// <param name="project_id">L'identifiant de projet.</param>
        /// <param name="questionnaire_id">L'identifiant de questionnaire.</param>
        /// <param name="terminal_id">L'identifiant du terminal.</param>
        /// <param name="customDir">Le nom personnalisé du répertoire de données.</param>
        /// <param name="mod">Le sous-répertoire de fichiers modifiés. Si on ne veut pas de sous-répertoire, ce paramètre peut être null, ou l'on peut utiliser l'énumération 'none'.</param>
        /// <returns>Le répertoire de données (modifiés ou non).</returns>
        public static string GetLocalIbrandTerminalCustomDir(int project_id, int questionnaire_id, int terminal_id, string customDir, ProjectDataDirectoriesModEnum mod = ProjectDataDirectoriesModEnum.none)
        {
            var local = GetLocalIbrandTerminalDir(project_id, questionnaire_id, terminal_id) + customDir + Path.DirectorySeparatorChar;
            if (mod != ProjectDataDirectoriesModEnum.none) {
                local += mod.ToString() + Path.DirectorySeparatorChar;
            }
            ValidateDirectory(local);
            return local;
        }

        /// <summary>
        /// Dans les cas où les options de l'énumération "ProjectDataDirectoriesEnum" sont insuffisants pour la méthode "GetLocalIbrandQuestionnaireDataDir", on peut utiliser
        /// cette méthode pour faire des recherches dans un répertoire avec un nom précis. On peut tout de même utiliser les options de sous-répertoires.
        /// </summary>
        /// <param name="tInfo">Instance du terminal.</param>
        /// <param name="customDir">Le nom personnalisé du répertoire de données.</param>
        /// <param name="mod">Le sous-répertoire de fichiers modifiés. Si on ne veut pas de sous-répertoire, ce paramètre peut être null, ou l'on peut utiliser l'énumération 'none'.</param>
        /// <returns>Le répertoire de données (modifiés ou non).</returns>
        public static string GetLocalIbrandTerminalCustomDir(TerminalInfo tInfo, string customDir,
            ProjectDataDirectoriesModEnum mod = ProjectDataDirectoriesModEnum.none)
        {
            return GetLocalIbrandTerminalCustomDir(tInfo.ProjectId, tInfo.QuestionnaireId, tInfo.TerminalId, customDir, mod);
        }

        /// <summary>
        /// Obtient le répertoire d'un terminal, bas. sur son projet et questionnaire.
        /// </summary>
        /// <param name="project_id">L'identifiant du projet.</param>
        /// <param name="questionnaire_id">L'identifiant du questionnaire.</param>
        /// <param name="terminal_id">L'identifiant du terminal.</param>
        public static string GetLocalIbrandTerminalDir(int project_id, int questionnaire_id, int terminal_id)
        {
            var local = Path.Combine(GetLocalIbrandProjectDir(project_id), questionnaire_id.ToString(), terminal_id.ToString()) + Path.DirectorySeparatorChar;
            ValidateDirectory(local);
            return local;
        }

        public static string GetLocalIbrandTerminalDir(TerminalInfo tInfo)
        {
            var local = Path.Combine(GetLocalIbrandProjectDir(tInfo.ProjectId), tInfo.QuestionnaireId.ToString(), tInfo.TerminalId.ToString()) + Path.DirectorySeparatorChar;
            ValidateDirectory(local);
            return local;
        }
    }
}
