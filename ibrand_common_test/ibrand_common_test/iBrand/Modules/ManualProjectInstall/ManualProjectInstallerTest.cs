using System.Collections.Generic;
using System.IO;
using System.Linq;
using Moq;
using NLog;
using NomadLogic.Common.Serveur;
using NomadLogic.Common.Services;
using NomadLogic.Common.Services.Configuration;
using NomadLogic.Common.Util;
using NomadLogic.iBrandDesktop.Modules.ManualProjectInstaller;
using NomadLogic.iBrandDesktop.Synchro.DownloadRestRequest;
using NUnit.Framework;

namespace NomadLogic.Test.iBrand.Modules.ManualProjectInstall
{
    public class ManualProjectInstallerTest : TestBaseNunit
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        [TestCase(@"ManualInstall\newProject")]
        [TestCase(@"ManualInstall\newProjectZip")]
        public void InstallFreshProjects(string testInstallDirectory)
        {
            var config = ServicesContainer.GetService<NomadConfiguration>();

            var outputBase = GetFullOutputDir();
            var inputBaseFlatDir = new DirectoryInfo(BaseDir + testInstallDirectory);
            var outputBaseFlatDir = new DirectoryInfo(outputBase + testInstallDirectory);
            
            LaunchManualInstallTest(outputBaseFlatDir, inputBaseFlatDir, config);
        }

        private void LaunchManualInstallTest(DirectoryInfo outputBaseFlatDir, DirectoryInfo inputBaseFlatDir, NomadConfiguration config)
        {
            EnsureDirEmpty(outputBaseFlatDir.FullName);
            Assert.IsTrue(inputBaseFlatDir.Exists);
            
            /*
            _db.GetResourceFiles(terminalInfo.TerminalId);
            _db.IsTerminalAlreadyInserted(terminalInfo.TerminalId)    
            _db.ManageRessourceFiles(terminalInfo.TerminalId, deltaFiles);
            _db.ResetTerminalMetadata(terminalInfo);
            */

            var dbMock = new Mock<IDatabaseProjectFromServer>();

            dbMock.Setup(db => db.IsTerminalAlreadyInserted(It.IsAny<int>())).Returns(false);
            dbMock.Setup(db => db.InsertTerminal(It.IsAny<TerminalDownloadInfo>())).Returns(1);
            dbMock.Setup(db => db.ManageRessourceFiles(It.IsAny<int>(), It.IsAny<IEnumerable<ResourceFileToDownload>>())).Returns(1);
            dbMock.Setup(db => db.ResetTerminalMetadata(It.IsAny<TerminalInfo>())).Returns(1);

            var mpi = new ManualProjectInstallManager(dbMock.Object, config)
            {
                InputDir = inputBaseFlatDir,
                OutputDir = outputBaseFlatDir,
                DebugKeepOriginalFiles = true
            };

            Assert.IsTrue(mpi.Init());

            Assert.AreEqual(1, mpi.Search());

            var rez = mpi.Process(log);

            Assert.AreEqual(0, rez.FailuresCount);
            Assert.AreEqual(1, rez.SuccessCount);
            Assert.AreEqual(1, rez.TotalJobs);
            var jobRez = rez.Results.First();

            Assert.IsNotNull(jobRez.TerminalInstallDirectory);

            var fileListOut = Directory.GetFileSystemEntries(jobRez.TerminalInstallDirectory.FullName).Length;

            Assert.AreEqual(jobRez.FilesCount, fileListOut);
            
            dbMock.VerifyAll();
        }
        
    }
}