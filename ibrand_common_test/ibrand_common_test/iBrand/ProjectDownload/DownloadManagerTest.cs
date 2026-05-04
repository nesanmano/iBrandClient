using System.Collections.Generic;
using System.IO;
using System.Threading;
using Moq;
using NomadLogic.Common.Serveur;
using NomadLogic.Common.Services;
using NomadLogic.Common.Services.Configuration;
using NomadLogic.Common.Util;
using NomadLogic.iBrandDesktop.Synchro.DownloadRestRequest;
using NomadLogic.Test.iBrand.ProjectDownload.Mocks;
using NUnit.Framework;

namespace NomadLogic.Test.iBrand.ProjectDownload
{
    public class DownloadManagerTest : TestBaseNunit
    {
        private Mock<IDatabaseProjectFromServer> _db;
        private List<ResourceFileToDownload> _listResJuju;

        private List<ResourceFileToDownload> _listResLegacy;
        
        [SetUp]
        public void SetUpFixture()
        {
            LogUtil.InitNLog();

           _db = new Mock<IDatabaseProjectFromServer>(MockBehavior.Strict);

           _listResJuju = ServerExpectedResult.JujuExpectedResult();//new List<ResourceFileToDownload>() { r1,r2,r3,r4,r5,r6,r7,r8,r9,r10,r11,r12,r13,r14,r15,r16,r17,r18,r19,r20,r21,r22,r23,r24,r25};
           _listResLegacy = ServerExpectedResult.LegacyExpectedResult();

        }

        [TearDown]
        public void TearDown()
        {
            
            
        }
        
        [Test]
        public void JujuDownloadManagerTest()
        {
            var qId = 27;

            var conf = ServicesContainer.GetService<NomadConfiguration>();
            _db.Setup(p => p.GetResourceFiles(It.IsAny<int>())).Returns(_listResJuju);
            _db.Setup(p => p.UpdateResourceFileData(qId, It.IsAny<ResourceFileToDownload>())).Returns(0);
            _db.Setup(p => p.ManageRessourceFiles(qId, It.IsAny<IList<ResourceFileToDownload>>()));
            using (var downloadManager = new DownloadManager(_db.Object, conf))
            {
                var serverTerminalInfo = new ServerTerminalDownloadRequest()
                {
                    Username = "big",
                    Password = "spin",
                    ProjectId = 24,
                    QuestionnaireId = qId

                };


                var baseDir = DirectoryUtil.GetLocalApplicationWorkDir()+"UnitTest\\iBrandJuJu\\projects\\";
                Directory.CreateDirectory(baseDir);

                Assert.IsTrue(Directory.Exists(baseDir));

                var outputDir = DirectoryUtil.GetLocalApplicationDir()+"results\\";

                downloadManager.Execute(serverTerminalInfo,baseDir, CancellationToken.None);
                Assert.IsTrue(File.Exists(outputDir+24+"\\"+27+ "\\1-Tap to start.jpg"));
            }
        }

        /*
        [Test]
        public async Task LegacyDownloadManagerTest()
        {

            _db.Setup(p => p.GetResourceFiles(It.IsAny<int>())).Returns(_listResLegacy);

            _db.Setup(p => p.UpdateResourceFileData(It.IsAny<ResourceFileToDownload>())).Returns(1);

            //_db.Setup(p => p.InsertRessourceFiles(It.IsAny<int>())).Returns(listResLegacy);

            var downloadManager = new IBrandLegacyDownloader
            {
                Db = _db.Object
            };

            var serverTerminalInfo = new ServerTerminalInfo()
            {
                Username = "mercedez",
                Password = "benz"
            };

           RestRequestElements restRequestElements = new RestRequestElements()
            {
                ServerAddr = "http://localhost:8080/ibrand_app/",
                ApiMethod1 = "listFiles",
                ApiMethod2 = "downloadfic",
                DeviceName = "pc",
                TerminalType = "Android",
                Headers = null

            };

            var baseDir = DirectoryUtil.GetLocalApplicationWorkDir() + "UnitTest\\iBrandLegacy\\projects\\"; 
            var outputDir = DirectoryUtil.GetLocalApplicationDir()+"results\\";

            var progress = new Progress<DownloadReportInfo>(res =>

            {
                Console.WriteLine(@"Downloaded File Name: "+ res?.Name + @"  Bytes downloaded: "+ res?.BytesDownloaded );
            });    
            Console.WriteLine(@"Please wait while we retreive some data!");

            var cts = new CancellationTokenSource();
            await downloadManager.GetResourceFiles(serverTerminalInfo,baseDir,outputDir,cts.Token);

            Assert.IsTrue(File.Exists(outputDir+"1085\\1013"+"\\config.txt"));
        }
        */

    }
}
