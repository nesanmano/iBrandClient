using System;
using System.Drawing;
using System.IO;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Json;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers;
using NomadLogic.Multimedia;
using NUnit.Framework;

namespace NomadLogic.Test.iBrand.Ui.ApplicationScreen.Generic.Helpers
{
    public class CondensedAnswerSelectionConfigTest : TestBaseNunit
    {
        [Test]
        public void MarshallToJsonAndBack()
        {
            var outputDir = GetFullOutputDir();
            var finalFile = outputDir + "config_marshall.json";
            EnsureFileNotExists(finalFile);
            var finalFileEmpty = outputDir + "config_marshall_empty.json";
            EnsureFileNotExists(finalFileEmpty);

            var toMarshall = new CondensedAnswerSelectionScreenConfig
            {
                DebugFacadeInfo = new Rectangle(0, 1, 2, 3),
                DebugCurrentSelectionInfo = new Rectangle(4, 5, 6, 7),
                DebugLastNumberInfo = new Rectangle(8, 9, 10, 11),
                DebugLastCallInfo = new Rectangle(12, 13, 14, 15),
                SourceQuestionnaireId = 987654,
                SourceQuestionnaireQuestionIdentifiers = new[] { 2, 3, 4 },
                SourceQuestionnaireTimeSpanCutOff = new TimeSpan(10, 9, 8, 6),
                TextGenerator = new ProceduralImageTextInfo
                {
                    Color = Color.AntiqueWhite,
                    Center = false,
                    FontSize = 20,
                    BitmapSize = new Size(150, 200),
                    FontStyle = FontStyle.Regular,
                    FontGraphicUnit = GraphicsUnit.Document,
                    BackgroundColor = Color.Transparent,
                    FontFamilly = FontFamily.GenericSerif.Name
                }
            };

            var toMarshallEmpty = new CondensedAnswerSelectionScreenConfig();

            MarshallToJsonFileAndTest(finalFile, toMarshall);
            MarshallToJsonFileAndTest(finalFileEmpty, toMarshallEmpty);
        }

        private static void MarshallToJsonFileAndTest(string file, CondensedAnswerSelectionScreenConfig instance)
        {
            JsonUtil.WriteJsonFile(new FileInfo(file), instance);

            Assert.IsTrue(File.Exists(file));

            var fromMarshall = JsonUtil.ReadJsonFile<CondensedAnswerSelectionScreenConfig>(new FileInfo(file));

            Assert.AreNotSame(instance, fromMarshall);
            Assert.AreEqual(instance, fromMarshall);

            Console.Out.WriteLine(instance);
        }
    }
}