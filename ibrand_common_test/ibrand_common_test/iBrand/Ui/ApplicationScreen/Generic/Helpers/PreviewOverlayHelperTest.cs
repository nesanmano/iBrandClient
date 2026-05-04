using System.Collections.Generic;
using System.Drawing;
using System.IO;
using NomadLogic.Common.Condition;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Json;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers;
using NUnit.Framework;

namespace NomadLogic.Test.iBrand.Ui.ApplicationScreen.Generic.Helpers
{
   public class PreviewOverlayHelperTest : TestBaseNunit
    {
        [Test]
        public void MarshallingPreviewOverlayInfo()
        {
            var output = GetFullOutputDir();
            var fileWrite = output + "data.json";
            EnsureFileNotExists(fileWrite);

            var prev = new PreviewOverlayInfo
            {
                Position = new Point(0, 0),
                FileName = "billi.jpg",
                Condition = new QuestionCondition(5),
                OriginalConstraint = new Rectangle(0, 0, 200, 200)
            };


            var tmpOverlayHelper = new PreviewOverlayHelper();
            var rec = new Rectangle(0, 0, 300, 300);
            tmpOverlayHelper.PreviewRectangle = rec;
            var tempo = new List<PreviewOverlayInfo>
            {
                prev
            };
            tmpOverlayHelper.PreviewOverlayInfoList = tempo;


            JsonUtil.WriteJsonFile(new FileInfo(fileWrite), tmpOverlayHelper);
                
            var fromFile = JsonUtil.ReadJsonFile<PreviewOverlayHelper>(new FileInfo(fileWrite));
             
            Assert.AreEqual(tmpOverlayHelper, fromFile);
        }

        [Test]
        public void PreviewOverlayInfoFromFile()
        {
            var fileToRead = BaseDir + @"PreviewOverlay\PreviewOverlayInfo_minConfig.config";
            EnsureFileExists(fileToRead);

            var fromFile = JsonUtil.ReadJsonFile<PreviewOverlayHelper>(new FileInfo(fileToRead));
            Assert.IsNotNull(fromFile);
        }
    }
}
