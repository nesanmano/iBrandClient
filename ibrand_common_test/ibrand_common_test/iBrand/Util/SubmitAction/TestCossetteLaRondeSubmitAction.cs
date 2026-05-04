using System.Collections.Generic;
using System.IO;
using NomadLogic.Common.Respondent;
using NomadLogic.iBrandDesktop.Util.SubmitAction;
using NomadLogic.Test.Common.Respondent;
using NUnit.Framework;

namespace NomadLogic.Test.iBrand.Util.SubmitAction
{
    public class TestCossetteLaRondeSubmitAction : TestBaseNunit
    {
        [Test]
        public void TestGifAndPictureTransform()
        {
            var outDir = GetFullOutputDir();
            var gifFile = outDir + "final_test.gif";
            EnsureFileNotExists(gifFile);

            var sa = new CossetteLaRondeSubmitAction();

            const string FILE_DIR = @"C:\Users\François Coupal\Documents\projets\2017\COS-2017-01 La Ronde Chevrolet\";
            const string FONT_FILE = FILE_DIR + @"final\ibrand\LOUIS-BOLDITALIC.OTF";
            const string MAIN_FILE = FILE_DIR + @"28065_IMG_9369.JPG";

            Assert.IsTrue(File.Exists(FONT_FILE), FONT_FILE);
            Assert.IsTrue(File.Exists(MAIN_FILE), MAIN_FILE);

            var overlays = new List<string>();

            for (var i = 1; i <= 6; i++)
            {
                var file = $@"{FILE_DIR}final\ibrand\GIF_{i}_en.png";
                overlays.Add(file);
                Assert.IsTrue(File.Exists(file), file);
            }

            var workDir =  Path.GetTempPath() + Path.DirectorySeparatorChar;
            var resp = new MockRespondent
            {
                MockAnswer = new List<Answer>
                {
                    new Answer {QuestionNo = 1, Response = MAIN_FILE},
                    new Answer {QuestionNo = 7, Response = "Marie and Stéphanie"},
                    new Answer {QuestionNo = 14, ChoiceNo = 1}
                }
            };

            // Exécution
            sa.MakeCossetteGif(FONT_FILE, gifFile, overlays, workDir, resp);

            Assert.IsTrue(File.Exists(gifFile));
        }
    }
}
