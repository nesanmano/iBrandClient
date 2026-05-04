using System.Drawing;
using NomadLogic.Common.Util;
using NUnit.Framework;

namespace NomadLogic.Test.Common.Multimedia.Transformation
{
    public class TestGdiBitmap : TestBaseNunit
    {
        public void TestForOverlay()
        {
            var inFile1 = BaseDir + "statue_model_01.png";
            var inFile2 = BaseDir + "statue_model_02.png";

            var bmp1 = FileUtil.LoadBitmap(inFile1);
            var bmp2 = FileUtil.LoadBitmap(inFile2);

            Assert.That(bmp1, Is.Not.Null);
            Assert.That(bmp2, Is.Not.Null);

            using (var g = Graphics.FromImage(bmp2))
            {
                g.DrawImageUnscaled(bmp1, 0, 0);
            }

        }
    }
}