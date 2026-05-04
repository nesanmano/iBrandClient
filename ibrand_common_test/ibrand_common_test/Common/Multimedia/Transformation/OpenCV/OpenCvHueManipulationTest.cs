using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using NomadLogic.Common.Extentions;
using NomadLogic.Multimedia.Gpu.OpenCV;
using NomadLogic.Multimedia.XmlTransformation;
using NUnit.Framework;

namespace NomadLogic.Test.Common.Multimedia.Transformation.OpenCV
{
    public class OpenCvHueManipulationTest : TestBaseNunit
    {
        [Test]
        public void OpenCvDoubleExpositionTest()
        {
            var originalImageFile1 = new FileInfo(BaseDir + Path.DirectorySeparatorChar + "statue_model_01.png");
            var originalImageFile2 = new FileInfo(BaseDir + Path.DirectorySeparatorChar + "statue_model_02.png");
            Assert.IsTrue(originalImageFile1.Exists);
            Assert.IsTrue(originalImageFile2.Exists);
            var outputDir = GetFullOutputDir();
            var outputFile = outputDir + "result.png";
            EnsureFileNotExists(outputFile);

            var file1Hue = Color.BlueViolet;
            var file2Hue = Color.DarkSeaGreen;
            var blendFactor = 0.5;
            var blendFunction = BlendMode.Normal;
            
            var bmp = OpenCvUtil.DoubleExpositionEffect(originalImageFile1, originalImageFile2, file1Hue, file2Hue, blendFactor, blendFunction);
            Assert.IsNotNull(bmp);

            bmp.Save(ImageFormat.Png, 9, outputFile);
            Assert.That(File.Exists(outputFile));
        }

        [Test]
        public void OpenCvMakeHuedImage()
        {
            var originalImageFile1 = new FileInfo(BaseDir + Path.DirectorySeparatorChar + "statue_model_01.png");
            Assert.IsTrue(originalImageFile1.Exists);
            var outputDir = GetFullOutputDir();
            var outputFinal = outputDir + "result_rgb.png";
            EnsureFileNotExists(outputFinal);
            var outputFile2 = outputDir + "alpha_before.png";
            EnsureFileNotExists(outputFile2);
            var outputFile3 = outputDir + "alpha_after.png";
            EnsureFileNotExists(outputFile3);

            using (var splitImage = OpenCvUtil.SplitImage<ChannelSplitImageHsl>(originalImageFile1))
            {
                Assert.IsTrue(splitImage.GetWorkingAlphaImage().ToBitmap().Save(ImageFormat.Png, 9, outputFile2));

                OpenCvUtil.ConvertImageToHuedEffect(splitImage, Color.Green, 0.75);

                Assert.IsTrue(splitImage.GetWorkingAlphaImage().ToBitmap().Save(ImageFormat.Png, 9, outputFile3));

                Assert.IsTrue(splitImage.AssembleImageDataFromSplit().ToBitmap().Save(ImageFormat.Png, 9, outputFinal));
            }
        }
    }
}
