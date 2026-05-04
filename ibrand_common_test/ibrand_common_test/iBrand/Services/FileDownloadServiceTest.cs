using System.Threading.Tasks;
using NomadLogic.iBrandDesktop.Services;
using NUnit.Framework;

namespace NomadLogic.Test.iBrand.Services
{
    public class FileDownloadServiceTest : TestBaseNunit
    {
        public async Task DownloadSingleFile()
        {
            var outputDir = GetFullOutputDir();
            EnsureDirEmpty(outputDir);

            var service = new FileDownloadService(1);
            Assert.IsTrue(service.Init());

            Assert.IsTrue(await service.StartLongRunning());


        }
    }
}