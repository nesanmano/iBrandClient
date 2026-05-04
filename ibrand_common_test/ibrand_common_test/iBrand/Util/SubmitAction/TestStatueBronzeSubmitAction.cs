using System.IO;
using NUnit.Framework;

namespace NomadLogic.Test.iBrand.Util.SubmitAction
{
    public class TestStatueBronzeSubmitAction : TestBaseNunit
    {

        /* Pour le test de la transformation du texte, voir ImageUtilTest.PutRotatedCustomFontTextOnImage() */

        [Test]
        public void CreateBronzeStatuePrintImage()
        {
            var outDir = Directory.GetCurrentDirectory() + @"\result\" + GetCurrentMethodName();
            EnsureDirExists(outDir);

            var inputFileBack = BaseDir + Path.DirectorySeparatorChar + "background.jpg";
            Assert.IsTrue(File.Exists(inputFileBack));
            var inputFileFront = BaseDir + Path.DirectorySeparatorChar + "foreground.png";
            Assert.IsTrue(File.Exists(inputFileFront));
            

            var output = outDir + Path.DirectorySeparatorChar + "result.jpg";
            EnsureFileNotExists(output);

            var inputFile = BaseDir + Path.DirectorySeparatorChar + "image_statue_front.png";
            Assert.IsTrue(File.Exists(inputFileFront));

            //BronzeStatueSubmitAction.TransformPicture(inputFileBack, inputFileFront, inputFile, output, 492, 232, 773, 1160, "Statue v3a", "statue v3a", null);

            Assert.IsTrue(File.Exists(output));
        }
    }
}