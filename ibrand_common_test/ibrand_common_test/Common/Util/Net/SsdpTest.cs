using System;
using System.IO;
using System.Linq;
using NLog;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Net;
using NUnit.Framework;
using Assert = NUnit.Framework.Assert;

namespace NomadLogic.Test.Common.Util.Net
{
    public class SsdpTest : TestBaseNunit
    {
        // ReSharper disable once UnusedMember.Local
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        [Test]
        public void ListenAndAnswerToBroadcast()
        {
            const string cMess01 = "Yarg01";
            const string cMess02 = "Flu02";
            const string cId = "INFINIT1";

            // Préparation du Provider
            var provider = new SsdpServiceProvider
            {
                CacheTime = TimeSpan.FromMinutes(1),
                Message01 = cMess01,
                Message02 = cMess02,
                ServiceId = cId
            };

            provider.InitAndStart();

            // Préparation de chercheur
            var locator = new SsdpServiceLocator
            {
                IdToCheck = cId,
                TimeOut = TimeSpan.FromSeconds(2)
            };

            var result = locator.SearchForServicesSync(null);
            provider.StopPublishingSsdpService();

            Assert.IsTrue(result.Exists(r => r.Message01.Equals(cMess01) && r.Message02.Equals(cMess02)));
        }

        [Test]
        public void SetupAndOperateMultipleServiceUsingBroadcast()
        {
            // Préparation des répertoires pour les services printstation
            var dirsPs = new[] {
                new DirectoryInfo(DirectoryUtil.GetLocalApplicationWorkDir() + GenerateUniqueId.GetId()),
                new DirectoryInfo(DirectoryUtil.GetLocalApplicationWorkDir() + GenerateUniqueId.GetId())
            };
            foreach (var dir in dirsPs)
                EnsureDirExists(dir.FullName);

            // Préparation pour les services de CameraRing
            var dirsCr = new[] {
                new DirectoryInfo(DirectoryUtil.GetLocalApplicationWorkDir() + GenerateUniqueId.GetId()),
                new DirectoryInfo(DirectoryUtil.GetLocalApplicationWorkDir() + GenerateUniqueId.GetId())
            };
            foreach (var dir in dirsCr)
                EnsureDirExists(dir.FullName);

            var printServices = dirsPs.Select(dir => SsdpUtil.PrepareAndLaunchPrintstationImageListener(dir, null)).ToList();
            var crServices = dirsPs.Select(dir => SsdpUtil.PrepareAndLaunchCameraRingImageListener(dir, null)).ToList();

            try
            {
                // On localise chaque service
                var ps = SsdpUtil.FindAllListeners(SsdpUtil.TcpServiceType.Printstation);
                Assert.IsTrue(ps.Count == 2, "Not enough Printstation services detected.");
                var cr = SsdpUtil.FindAllListeners(SsdpUtil.TcpServiceType.CameraRing);
                Assert.IsTrue(cr.Count == 2, "Not enough Camera ring services detected.");
            }
            finally
            {
                foreach (var dir in dirsPs)
                {
                    FileUtil.DeleteFolderContent(dir.FullName);
                }
                foreach (var dir in dirsCr)
                {
                    FileUtil.DeleteFolderContent(dir.FullName);
                }

                // On ferme les services
                foreach (var service in printServices)
                {
                    Assert.IsTrue(service.BroadcastSsdpService.IsRunning);
                    Assert.IsTrue(service.TcpFileListenerService.IsRunning);
                    service.StopAllService();
                    Assert.IsNull(service.BroadcastSsdpService);
                    Assert.IsNull(service.TcpFileListenerService);
                }
                foreach (var service in crServices)
                {
                    Assert.IsTrue(service.BroadcastSsdpService.IsRunning);
                    Assert.IsTrue(service.TcpFileListenerService.IsRunning);
                    service.StopAllService();
                    Assert.IsNull(service.BroadcastSsdpService);
                    Assert.IsNull(service.TcpFileListenerService);
                }
            }
        }
    }
}
