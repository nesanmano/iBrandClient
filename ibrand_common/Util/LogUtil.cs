using System.IO;
using System.Text;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace NomadLogic.Common.Util
{
    /// <summary>
    /// Classe pour la configuration et la maintenance de l'utilitaire NLog. On utilise NLog 4.
    /// Documentation: http://nlog-project.org/documentation/v4.0.0/html/R_Project_NLog.htm
    /// Comment utiliser: http://nlog-project.org/2015/05/28/nlog-4-release-candidate.html
    /// <p>ATTENTION: Contrairement à LogBack sur JAVA, l'exception doit être le premier paramètre!
    /// Exemple: log.(exception, "Message avec format fprint: {0}, id={1}", msg, respondent.id);</p>
    /// <p>Pour utiliser la variable de log dans une classe:
    /// <b>private static Logger log = LogManager.GetCurrentClassLogger();</b></p>
    /// </summary>
    public static class LogUtil
    {
        private const string LOG_FILE_NAME = "log_config.xml";
        private static LoggingConfiguration _config;

        /// <summary>
        /// Initialise Nlog avec le fichier "NLog.xml" qui se trouve dans le répertoire d'installation de l'application.
        /// </summary>
        public static void InitNLog()
        {
            if (_config != null) return;
            var initFile = DirectoryUtil.GetCurrentApplicationInstallDir() + LOG_FILE_NAME;

            // Test pour trouver le fichier localement
            if (!File.Exists(initFile))
            {
                initFile = LOG_FILE_NAME;
            }
            
            var initFileLocal = DirectoryUtil.GetLocalApplicationLogsDir() + LOG_FILE_NAME;
            if (!File.Exists(initFileLocal))
            {
                // Cas rare où le fichier template de configuration n'existe pas non plus dans le répertoire de l'exécutable.
                // Par exemple, les projets de tests unitaires. Ne devrait pas être accédé dans d'autres situations.
                if (!File.Exists(initFile))
                {
                    var sb = new StringBuilder();
                    sb.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\" ?>");
                    sb.AppendLine("<!-- THIS IS AN AUTOMATED FILE CREATED BECAUSE THE BASE LOG FILE DO NOT EXISTS AT EXECUTABLE LEVEL. PLEASE CORRECT THIS BY DESTROYING THIS FILE AND MAKING SURE A FILE WITH SAME NAME EXISTS IN EXECUTABLE DIRECTORY. -->");
                    sb.AppendLine("<nlog xmlns=\"http://www.nlog-project.org/schemas/NLog.xsd\"");
                    sb.AppendLine("\t\txmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"");
                    sb.AppendLine("\t\tautoReload=\"false\">");
                    sb.AppendLine();
                    sb.AppendLine("\t<targets>");
                    sb.AppendLine("\t\t<target name=\"main\" xsi:type=\"File\"");
                    sb.AppendLine("\t\t\tlayout=\"[${level}] ${longdate} [${processid}:${processname} - ${callsite:className=true:includeSourcePath=false}] => ${message} ${exception:format=ToString}\"");
                    sb.AppendLine("\t\t\tfileName=\"main.log\"");
                    sb.AppendLine("\t\t\tarchiveFileName=\"archives /main.{#}.zip\"");
                    sb.AppendLine("\t\t\tarchiveEvery=\"Day\"");
                    sb.AppendLine("\t\t\tarchiveNumbering=\"Rolling\"");
                    sb.AppendLine("\t\t\tmaxArchiveFiles=\"7\"");
                    sb.AppendLine("\t\t\tenableArchiveFileCompression=\"true\" />");
                    sb.AppendLine("\t</targets>");
                    sb.AppendLine();
                    sb.AppendLine("\t<rules>");
                    sb.AppendLine("\t\t<logger name = \" * \" minlevel = \"Info\" writeTo = \"main\" />");
                    sb.AppendLine("\t</rules>");
                    sb.AppendLine("</nlog> ");

                    File.WriteAllText(initFileLocal, sb.ToString());
                }
                else
                {
                    File.Copy(initFile, initFileLocal);
                }
            }

            // On utilise le fichier dans la zone locale (pour pouvoir la modifier au besoin)
            if (File.Exists(initFileLocal))
            {
                // Lit la configuration initiale de l'application. Ce fichier est propre à chaque application.
                var tmpConfig = new XmlLoggingConfiguration(initFileLocal);
                if (tmpConfig.InitializeSucceeded == true)
                {
                    tmpConfig.AutoReload = false; // TODO Pas été capable de lier l'événement de recharge de fichier...

                    LogManager.Configuration = tmpConfig;

                    // Reconfigure le chemin des Targets NLog pour changer le chemin des fichiers.
                    RedirectAllTargets(tmpConfig);
                    LogManager.ReconfigExistingLoggers();

                    LogManager.ConfigurationReloaded += OnConfigurationReloadEvent; // Lance l'événement quand le fichier de configuration est modifié.

                    _config = tmpConfig;
                }
                else
                {
                    // Si fichier existe mais invalide.
                    BasicConfig();
                }
            }
            else // Le fichier n'existe pas. On fait une configuration de base!
            {
                BasicConfig();
            }
        }

        private static void BasicConfig()
        {
            if (_config != null) return;
            var target = new FileTarget
            {
                Layout = "${longdate} ${logger} ${message}",
                FileName = DirectoryUtil.GetLocalApplicationLogsDir() + @"no_config.log",
                KeepFileOpen = false,
                Encoding = Encoding.UTF8
            };

            SimpleConfigurator.ConfigureForTargetLogging(target, LogLevel.Debug);

            LogManager.GetLogger("root").Warn("Nlog.xml not found. Reverting to basic log file.");

            _config = LogManager.Configuration;
        }

        private static void RedirectAllTargets(LoggingConfiguration tmpConfig)
        {
            if (tmpConfig?.AllTargets == null) return;
            foreach (var t in tmpConfig.AllTargets)
            {
                var ft = t as FileTarget;
                if (ft == null) continue;
                ft.FileName = ReplaceFileMainDirectory(ft.FileName.ToString());

                if (ft.ArchiveFileName == null) continue;
                ft.ArchiveFileName = ReplaceFileMainDirectory(ft.ArchiveFileName.ToString());
            }
        }

        private static string ReplaceFileMainDirectory(string relativeFilePath)
        {
            var tmp = relativeFilePath; // Contient des "single quotes"
            tmp = tmp.Substring(1, tmp.Length - 2);
            tmp = tmp.Replace("/", @"\");
            return DirectoryUtil.GetLocalApplicationLogsDir() + tmp;
        }

        private static void OnConfigurationReloadEvent(object sender, LoggingConfigurationReloadedEventArgs e)
        {
            if (LogManager.Configuration == null) return;
            RedirectAllTargets(LogManager.Configuration);
            LogManager.ReconfigExistingLoggers();
        }

        /// <summary>
        /// Arrête NLog et détruit sa configuration.
        /// </summary>
        public static void StopNLog()
        {
            if (_config == null) return;
            LogManager.Shutdown();
            _config = null;
        }
    }
}
