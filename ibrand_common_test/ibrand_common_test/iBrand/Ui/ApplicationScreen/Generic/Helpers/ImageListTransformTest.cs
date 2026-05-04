using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using NLog;
using NomadLogic.Common.Questionnaire;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Util;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers.ImageTransforms;
using NomadLogic.Test.Common.Respondent;
using NUnit.Framework;

namespace NomadLogic.Test.iBrand.Ui.ApplicationScreen.Generic.Helpers
{
    public class ImageListTransformTest : TestBaseNunit
    {
        // ReSharper disable once UnusedMember.Local
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        [Test]
        public void TestFirstPictureTransformation()
        {
            // Définition fichier.
            var originalPictureFile = BaseDir + Path.DirectorySeparatorChar + "IMG_9332.JPG";
            Assert.IsTrue(File.Exists(originalPictureFile));
            var originalOverlayFile = BaseDir + Path.DirectorySeparatorChar + "GIF_1.png";
            Assert.IsTrue(File.Exists(originalOverlayFile));
            var fontFile = BaseDir + Path.DirectorySeparatorChar + "Louis-BoldItalic.otf";
            Assert.IsTrue(File.Exists(fontFile));
            var outputDir = GetFullOutputDir();
            var finalFile = outputDir + "final.jpg";
            EnsureFileNotExists(finalFile);

            DirectoryUtil.DebugLocalUserDir = outputDir;

            var options = new ImageListTransformOptionsFirstPicture
            {
                SaveDataModel = new DataModelQuestionnaire
                {
                    Identifier = new QuestionIdentifier(3)
                },
                JpegSaveQuality = 90,
                OverlaySelector = new ImageSelectorSingle(originalOverlayFile),
                CanvasSize = new Size(1067, 1598),
                CanvasBaseColor = Color.Black,
                NewPos = new Point(158, 394),
                NewSize = new Size(750, 500),
                NewRotateFlip = RotateFlipType.RotateNoneFlipY,
                TextCase = 1,
                QuestionNoToFetchText = 1,
                TextCentered = true,
                TextPos = new Point(530, 255),
                OverlayPos = new Point(0, 0),
                FontDefinition = new FontDefinitionInfo
                {
                    Color = Color.White,
                    FamilyName = "Louis",
                    Size = 30,
                    Style = FontStyle.Italic.ToString(),
                    FontFile = fontFile
                }
            };

            var mockScreen = new MockScreen
            {
                MockRespondent = new MockRespondent
                {
                    MockAnswer = new List<Answer>
                    {
                        new Answer{QuestionNo = 1, Response = "Stéphanie, Marie-Laure et Ansuh"}
                    }
                }
            };
            var sc = new ScreenConfig(0,0, 0);

            ImageListTransform.CreateFirstPicture(
                mockScreen, 
                new List<string> { originalPictureFile },
                sc, options);

            Assert.That(mockScreen.MockAnswersSaved.Count == 1);
            var ans = mockScreen.MockAnswersSaved.First();
            Assert.That(ans.QuestionNo.Equals("3"));
            Assert.NotNull(ans.Response);
            Assert.That(File.Exists(ans.Response));
            var answerFileInfo = new FileInfo(ans.Response);
            Assert.That(answerFileInfo.Exists);
            Assert.That(answerFileInfo.Length > 100);

            answerFileInfo.MoveTo(finalFile);
            Assert.That(File.Exists(finalFile));
        }
    }
}
