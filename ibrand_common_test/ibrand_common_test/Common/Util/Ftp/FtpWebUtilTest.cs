using System;
using System.Globalization;
using System.IO;
using NUnit.Framework;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.DateAndTime;
using NomadLogic.Common.Util.Json;
using NomadLogic.Common.Util.Networking.Ftp;
using WinSCP;

namespace NomadLogic.Test.Common.Util.Ftp
{
    /// <summary>
    /// Classe de test pour les méthodes de l'utilitaire Ftp.
    /// </summary>
    public class FtpWebUtilTest : TestBaseNunit
    {
        private const string LOCAL_SERVEUR_ADDRESS = "192.168.1.207";
        private const string LOCAL_SERVEUR_PORT = "51218";
        private const string LOCAL_SERVEUR_TEST_USER = "dev_test";
        private const string LOCAL_SERVEUR_TEST_USER_PASS = "nomadtest";
        private const string LOCAL_SERVEUR_TEST_BASE_DIR01 = "sub01";
        
        
        // ReSharper disable once NUnit.TestCaseAttributeRequiresExpectedResult
        [TestCase(LOCAL_SERVEUR_TEST_USER, LOCAL_SERVEUR_TEST_USER_PASS, LOCAL_SERVEUR_ADDRESS, LOCAL_SERVEUR_PORT)]
        public SessionOptions ConnectToTestServer(string user = null, string pass = null, string addr = null, string port = null)
        {
            var options = new SessionOptions
            {
                Protocol = Protocol.Ftp,
                HostName = addr ?? LOCAL_SERVEUR_ADDRESS,
                PortNumber = int.Parse(port ?? LOCAL_SERVEUR_PORT),
                UserName = user ?? LOCAL_SERVEUR_TEST_USER,
                Password = pass ?? LOCAL_SERVEUR_TEST_USER_PASS
            };

            using (var session = new Session())
            {
                session.Open(options);
                Assert.IsTrue(FtpWebUtil.CheckIfExists(session, LOCAL_SERVEUR_TEST_BASE_DIR01));
            }

            return options;
        }

        [Test]
        public void CheckDirectoryList()
        {
            var setup = ConnectToTestServer();

            using (var session = new Session())
            {
                session.Open(setup);

                var result = session.ListDirectory("/");
                Assert.IsNotNull(result);

                foreach (var i in result.Files)
                {
                    var rfi = i as RemoteFileInfo;
                    Assert.IsNotNull(rfi);
                    Console.Out.WriteLine(rfi.ToStringPublicProperties());
                }

                Assert.IsTrue(result.Files.Any(f => f.IsDirectory && f.Name.Equals(LOCAL_SERVEUR_TEST_BASE_DIR01)));
            }
        }

        [Test]
        public async Task SynchronizeDirectory()
        {
            var local = GetFullOutputDir();
            EnsureDirEmpty(local);

            var setup = ConnectToTestServer();

            var prog = new Microsoft.Progress<FtpBatchProgress>(b =>
            {
                Console.Out.WriteLine($"Completion : {(int)(b.Completion*100)}, Elapsed: {b.ElapsedTime.ToStringHourMinSec(true, true)}, File List details :\n\t{StringUtil.FormatEnumerable(b.Transfers, true, 1)}");
            });

            var nbr = await FtpWebUtil.SynchronizeDirectory(setup, local, "/", CancellationToken.None, null, null, prog);
            Assert.Greater(nbr, 0);
        }

        [TestCase("", "json", 1)]
        [TestCase("2020-01-01 00:01:00", "", 16)]
        [TestCase("2020-02-04 15:18:00", "", 1)]
        [TestCase("2020-02-04 15:16:30", "png", 2)]
        public async Task SynchronizeDirectoryFilters(string date, string fileType, int expected)
        {
            var local = GetFullOutputDir();
            EnsureDirEmpty(local);

            var setup = ConnectToTestServer();
            
            DateTime? time = null;
            if (!string.IsNullOrWhiteSpace(date))
            {
                time = DateTime.ParseExact(date, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                await Console.Out.WriteLineAsync("Time filter = " + time.Value.ToLongDateString());
            }

            string[] types = null;
            if (!string.IsNullOrWhiteSpace(fileType))
            {
                types = new []{ fileType };
                await Console.Out.WriteLineAsync("Type filter = " + fileType);
            }

            var nbr = await FtpWebUtil.SynchronizeDirectory(setup, local, "/", CancellationToken.None, time, types);
            Assert.AreEqual(expected, nbr);
        }

        [TestCase(1, "json")]
        [TestCase(6, "png")]
        [TestCase(2, "gif", "otf")] // TODO ne fonctionne pas encore.
        public async Task SynchronizeDirectoryFileTypes(int expected, params string[] filters)
        {
            var local = GetFullOutputDir();
            EnsureDirEmpty(local);

            var setup = ConnectToTestServer();
            
            await Console.Out.WriteLineAsync("Type filters = " + StringUtil.FormatEnumerable(filters));

            var nbr = await FtpWebUtil.SynchronizeDirectory(setup, local, "/", CancellationToken.None, null, filters);
            Assert.AreEqual(expected, nbr);
        }

        [TestCase("local_test", "1234", "192.168.3.111", 22)]
        public void TestRebexBaseConnexion(string userName, string password, string url, int port)
        {
            
            var rebexConfigFileInfo = new FileInfo(Path.Combine(BaseDir, "ftp", "rebex", "ftp.json"));
            EnsureFileExists(rebexConfigFileInfo);
            var config = JsonUtil.ReadJsonFile<FtpConfigInfo>(rebexConfigFileInfo);

            var ppkFile = Path.Combine(BaseDir, "ftp", "rebex", config.SshPrivateKeyFilePath);
            EnsureFileExists(ppkFile);

            var local = GetFullOutputDir();
            EnsureDirEmpty(local);
            
            var sessionOptions = new SessionOptions
            {
                Protocol = Protocol.Sftp,
                HostName = url,
                UserName = userName,
                Password = password,
                PortNumber = port,
                SshHostKeyFingerprint = config.SshHostFingerprint,
                SshPrivateKeyPath = ppkFile
            };
            var session = new Session();
            session.Open(sessionOptions);

            Assert.IsTrue(FtpWebUtil.TestSession(session));
        }
    }
}
