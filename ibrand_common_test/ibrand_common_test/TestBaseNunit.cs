using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using JetBrains.Annotations;
using NomadLogic.Common.Serveur;
using NomadLogic.Common.Services;
using NomadLogic.Common.Services.Configuration;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.DateAndTime;
using NomadLogic.Common.Util.Json;
using NomadLogic.Common.Util.Print;
using NUnit.Framework;

namespace NomadLogic.Test
{
    /// <summary>
    /// Classe de base pour des tests.
    /// Calcule le temps pris pour chaque test avec son "Setup-Teardown" de base.
    /// </summary>
    [TestFixture]
    [PublicAPI]
    public abstract class TestBaseNunit
    {
        private UnitTestConfig _testConfig;
        
        /// <summary>
        /// Répertoire où sont stockés les fichiers utilisés pour les tests.
        /// </summary>
        protected string BaseDir => _testConfig?.TestFilesDirectory;

        /// <summary>
        /// Répertoire d'écriture des tests.
        /// </summary>
        protected string OutputBaseDir { get; private set; }

        /// <summary>
        /// S'assure qu'un répertoire existe, avec assertion.
        /// </summary>
        /// <param name="dir">Le répertoire à vérifier.</param>
        protected void EnsureDirExists(string dir)
        {
            if (!Directory.Exists(dir))
            {
                Assert.IsNotNull(Directory.CreateDirectory(dir));
            }
            Assert.IsTrue(Directory.Exists(dir));
        }

        /// <summary>
        /// S'assure qu'une série de chemin de fichiers pointent vers des fichiers existants, via assertions.
        /// </summary>
        /// <param name="paths">Les chemins à tester.</param>
        protected void EnsureFileExists(params string[] paths)
        {
            Assert.IsNotNull(paths);

            foreach (var path in paths)
            {
                Assert.IsTrue(File.Exists(path), "File path does not exists: {0}", path);
            }
        }

        /// <summary>
        /// S'assure qu'une série de chemin de fichiers pointent vers des fichiers existants, via assertions.
        /// </summary>
        /// <param name="files">Les chemins à tester.</param>
        protected void EnsureFileExists(params FileInfo[] files)
        {
            Assert.IsNotNull(files);

            foreach (var path in files)
            {
                Assert.IsTrue(File.Exists(path.FullName), "File path does not exists: {0}", path);
            }
        }

        /// <summary>
        /// S'assure qu'une série de chemin de fichiers pointent vers des fichiers existants, via assertions.
        /// </summary>
        /// <param name="pathsGroup">Les groupes de chemins à tester.</param>
        protected void EnsureFileExists(params string[][] pathsGroup)
        {
            Assert.IsNotNull(pathsGroup);

            foreach (var path in pathsGroup)
            {
                EnsureFileExists(path);
            }
        }

        /// <summary>
        /// S'assure qu'un répertoire existe, mais ne contient rien.
        /// </summary>
        /// <param name="dir">Le répertoire à vérifier.</param>
        protected void EnsureDirEmpty(string dir)
        {
            EnsureDirExists(dir);

            foreach (var d in Directory.EnumerateDirectories(dir))
            {
                FileUtil.DeleteFolderContent(d);
            }

            foreach (var f in Directory.EnumerateFiles(dir))
            {
                File.Delete(f);
            }

            Assert.That(!Directory.GetFiles(dir).Any());
            Assert.That(!Directory.GetDirectories(dir).Any());
        }

        /// <summary>
        /// S'assure qu'un fichier n'existe pas. Si oui, on le détruit, puis on asserte que le fichier n'existe pas.
        /// </summary>
        /// <param name="file">Le fichier à "vider".</param>
        protected void EnsureFileNotExists(params string[] file)
        {
            foreach (var f in file)
            {
                if (File.Exists(f))
                {
                    File.Delete(f);
                }

                Assert.IsFalse(File.Exists(f));
            }
        }

        protected void EnsureFileNotExists(params FileInfo[] file)
        {
            foreach (var f in file)
            {
                if (File.Exists(f.FullName))
                {
                    File.Delete(f.FullName);
                    f.Refresh();
                }

                Assert.IsFalse(f.Exists);
            }
        }

        /// <summary>
        /// Obtient le nom de la méthode courante.
        /// Voir:
        /// https://msdn.microsoft.com/en-us/library/system.runtime.compilerservices.callermembernameattribute.aspx
        /// https://stackoverflow.com/questions/44153/can-you-use-reflection-to-find-the-name-of-the-currently-executing-method/15310053#15310053
        /// </summary>
        /// <param name="memberName">Automatiquement donné par le framework.</param>
        /// <returns>Le nom de la méthode courante (l'appellante de cette méthode).</returns>
        [Obsolete("Incomplet. Utilisez désormais 'GetFullOutputDirs' pour avoir les sous-répertoires en plus, pour plus de lisibilité.")]
        protected string GetCurrentMethodName([CallerMemberName] string memberName = "")
        {
            return memberName;
        }

        /// <summary>
        /// Génère une arborescence pour les tests selon le nom du name space de la classe de test (qui DEVRAIT réfléter le namespace de la classe
        /// testée) et plus du nom de la méthode testée. S'assure que le répertoire existe avant de sortir.
        /// </summary>
        /// <param name="memberName">Ne pas toucher. Est généré automatiquement au moment de l'exécution.</param>
        /// <returns>Le path complet, et la méthode s'assure qu'il existe.</returns>
        protected string GetFullOutputDir([CallerMemberName] string memberName = "")
        {
            var type = GetType();
            var ns = type.Namespace?.Substring(16); // Enlève "NomadLogic.Test." au début
            var sb = new StringBuilder();

            sb.Append(OutputBaseDir);
            if (ns != null)
            {
                sb.Append(ns);
                sb.Append(Path.DirectorySeparatorChar);
            }
            sb.Append(type.Name);
            sb.Append(Path.DirectorySeparatorChar);
            sb.Append(memberName);
            sb.Append(Path.DirectorySeparatorChar);

            var final = sb.ToString();
            
            EnsureDirExists(final);
            
            return final;
        }

        [OneTimeSetUp]
        public virtual void InitConfigurationAndNlog()
        {
            var init = ServicesContainer.GetService<NomadConfiguration>();

            if (init != null) return;

            var appDir = DirectoryUtil.GetLocalApplicationDir(DirectoryUtil.ApplicationNameEnum.Test);

            var config = new NomadConfiguration();
            config.InitWithDefaultValue();
            config.ApplicationName = DirectoryUtil.ApplicationNameEnum.Test;
            config.PreferedLanguage = "en";
            
            // Configuration pour les serveurs
            var serverConfigFile = new FileInfo(appDir + "servers.json");
            if (!serverConfigFile.Exists)
            {
                Assert.IsTrue(JsonUtil.WriteJsonFile(serverConfigFile, config.ServerInformations));
                serverConfigFile.Refresh();
            }

            var sers = JsonUtil.ReadJsonFile<IDictionary<ServerConfigurationSwitch, ServerConfiguration>>(serverConfigFile);
            Assert.IsNotNull(sers);
            config.ServerInformations = sers;

            // Configuration du serveur choisi
            var serverChosenFile = new FileInfo(appDir + "current_server.json");
            if (!serverChosenFile.Exists)
            {
                Assert.IsTrue(JsonUtil.WriteJsonFile(serverChosenFile, config.CurrentServerConfigurationKey));
                serverChosenFile.Refresh();
            }

            var chosenServ = JsonUtil.ReadJsonFile<ServerConfigurationSwitch>(serverChosenFile);
            config.CurrentServerConfigurationKey = chosenServ;

            ServicesContainer.AddService(config);
            Console.Out.WriteLine("Active (default) server configuration: " + config.CurrentServerConfigurationKey);

            LogUtil.InitNLog();

            // On assume que les tests ont été initialisés de façon globale.
            OutputBaseDir = appDir + @"results\";
            var testConfigFile = new FileInfo(DirectoryUtil.GetLocalApplicationDir() + "testConfig.json");

            if (!testConfigFile.Exists)
            {
                var tmpTestConfig = new UnitTestConfig
                {
                    TestFilesDirectory = @"C:\Users\François Coupal\Documents\git\iBrand_Client_C_Sharp_Desktop\test_files\"
                };
                JsonUtil.WriteJsonFile(testConfigFile, tmpTestConfig);
            }

            _testConfig = JsonUtil.ReadJsonFile<UnitTestConfig>(testConfigFile);

            Assert.That(File.Exists(testConfigFile.FullName));
            Assert.That(Directory.Exists(_testConfig.TestFilesDirectory), "Currently assigned directory for test files does not exists: {0}",
                _testConfig.TestFilesDirectory);
        }

        [OneTimeTearDown]
        public void StopNlog()
        {
            LogUtil.StopNLog();
        }
        private Stopwatch _stopWatch;
        [SetUp]
        public void Init()
        {
            _stopWatch = Stopwatch.StartNew();
        }

        [TearDown]
        public void Cleanup()
        {
            _stopWatch.Stop();
            Console.Out.WriteLine("Temps - {0}", _stopWatch.Elapsed.ToMinimumReadableTime());
        }

        /// <summary>
        /// Efface le fichier de configuration global d'impression pour cette application (Test) pour une imprimnate précise.
        /// Utile si on veut avoir une configuration fonctionnele ou délibérément incorrecte (pour tester en cas d'erreur).
        /// </summary>
        /// <param name="printerPartialName">Le nom d'imprimante voulu. Va causer une erreur d'assertion si elle n'existe pas.</param>
        /// <param name="paperSizePartialName">Le nom du papier, imcomplet. Par défaut "6x4"</param>
        /// <param name="forceErrorState">Force une configuration à être INUTILISABLE. Cela provoquera une erreur venant de la classe utilitaire.</param>
        protected void InitializePrintConfigToPrinterName(string printerPartialName, string paperSizePartialName = "6x4", bool forceErrorState = false)
        {
            var defFile = PrintUtil.DEFAULT_PRINTER_CONFIG_FILE;
            EnsureFileNotExists(defFile.FullName);

            var printerName = PrintUtil.GetPrinterNamesList().FirstOrDefault(p => p.Contains(printerPartialName));
            Assert.IsNotNull(printerName);

            var settings = PrintUtil.FindPrinterSettingsFromSystemList(printerName, true);

            var paper = PrintUtil.FindPaperSizeFromPrinterSettings(settings, paperSizePartialName);
            Assert.IsNotNull(paper);

            var pjInfo = new PrintJobInfo()
            {
                PrinterName = printerName,
                PaperSize = paper,
                DrawOptions = new PrintJobImageOption() // Options par défaut.
            };

            var success = JsonUtil.WriteJsonFile(PrintUtil.DEFAULT_PRINTER_CONFIG_FILE, pjInfo);
            Assert.IsTrue(success);
            Assert.IsTrue(File.Exists(PrintUtil.DEFAULT_PRINTER_CONFIG_FILE.FullName));

            Assert.IsTrue(PrintUtil.IsPrintReady());
        }
    }
}
