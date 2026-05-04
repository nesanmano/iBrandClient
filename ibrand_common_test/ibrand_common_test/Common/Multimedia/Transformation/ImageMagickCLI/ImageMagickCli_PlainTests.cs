using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NomadLogic.Common.Services;
using NomadLogic.Common.Services.Configuration;
using NomadLogic.Multimedia.Cli.ImageMagickCLI;
using NUnit.Framework;

namespace NomadLogic.Test.Common.Multimedia.Transformation.ImageMagickCLI
{
    // ReSharper disable once InconsistentNaming
    public class ImageMagickCli_PlainTests : ImageMagickCli_ComplexTests
    {
        [Test]
        public async Task TestFileDimensions()
        {
            var config = ServicesContainer.GetService<NomadConfiguration>();

            var testFile = new FileInfo(BaseDir + "sujet_00.jpg");

            var dims = await ImageMagickCliUtil.GetImageDimensionsAsync(CancellationToken.None, testFile, config.ImageMagickLocation);

            Assert.AreEqual(new Size(1280, 1920), dims);
        }

        [Test]
        public async Task TestSimpleCommandLine()
        {
            var output = GetFullOutputDir();
            EnsureDirEmpty(output);
            var outputDir = new DirectoryInfo(output);
            var testFile = BaseDir + "sujet_00.jpg";

            var inFiles = new Dictionary<string, string>
            {
                {"c1", testFile},
                {"v1", "gray"}
            };

            ImageMagickInstallTest();

            var commandList = new List<string>
            {
                "convert {IN:c1} -colorspace {IN_VAL:v1} {OUT:r1}"
            };

            var commands = ImageMagickCliUtil.CreateCommands<ImageMagickCliCommand>(commandList);
            var fileRefs = ImageMagickCliUtil.CreateFileRefs(commands, outputDir, null, false, inFiles);

            async Task ExecFunc(ImageMagickCliCommand c, CliTmpFileDictionaryRefs r, CancellationToken ct)
            {
                await c.ExecuteAsync(r, inFiles, ct); //TODO:: A revoir et a faire marcher
            }

            var results = await ImageMagickCliUtil.ExecuteCommands(commands, fileRefs, ExecFunc, CancellationToken.None);

            TestCliResult(results, 1);
        }

        [TestCase(640, 960, false)]
        [TestCase(1280, 1920, false)]
        [TestCase(500, 500, false)]
        [TestCase(500, 500, true)]
        public async Task TestThumbnailGeneration(int sizeX, int sizeY, bool fitInside)
        {
            var output = GetFullOutputDir();
            EnsureDirEmpty(output);
            var outputDir = new DirectoryInfo(output);
            var testFile = new FileInfo(BaseDir + "sujet_00.jpg");

            var newSize = new Size(sizeX, sizeY);

            var bmp = await ImageMagickCliUtil.LoadImageFromThumbnailAsync(testFile, outputDir, newSize, fitInside);
            Assert.IsNotNull(bmp);

            Assert.AreEqual(newSize, bmp.Size);

            var originalSize = await ImageMagickCliUtil.GetImageDimensionsAsync(CancellationToken.None, testFile);

            outputDir.Refresh();
            var files = outputDir.GetFiles();
            if (originalSize.Equals(newSize))
            {
                // Il ne devrait pas avoir de fichier Thumbnail
                Assert.IsFalse(files.Any());
            }
            else
            {
                // Un fichier thumbnail devrait avoir été créé.
                Assert.AreEqual(1, files.Length);
                //Test du nom du fichier
                var sizePrefix = $"{newSize.Width}x{newSize.Height}_";
                Assert.IsTrue(files.First().Name.StartsWith(sizePrefix));
            }

        }
        
    }
}