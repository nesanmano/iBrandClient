using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using NomadLogic.Common.Extentions;
using NomadLogic.Common.Util;
using NUnit.Framework;

namespace NomadLogic.Test.iBrand.Util.SubmitAction
{
    public class TestChevroletHockeyCard : TestBaseNunit
    {

        /* Pour le test de la transformation du texte, voir ImageUtilTest.PutRotatedCustomFontTextOnImage() */

        [Test]
        public void CreateHockeyCardPrintImage()
        {
            var outDir = Directory.GetCurrentDirectory() + @"\result\" + GetCurrentMethodName();
            EnsureDirExists(outDir);

            var inputFileBack = BaseDir + Path.DirectorySeparatorChar + "image_font.png";
            Assert.IsTrue(File.Exists(inputFileBack));
            var inputFileFront = BaseDir + Path.DirectorySeparatorChar + "image_font2.png";
            Assert.IsTrue(File.Exists(inputFileFront));

            var output = outDir + Path.DirectorySeparatorChar + "print.jpg";
            EnsureFileNotExists(output);

            using (var frontBmp = FileUtil.LoadBitmap(inputFileFront))
            {
                frontBmp.SetResolution(300, 300);
                using (var backBmp = FileUtil.LoadBitmap(inputFileBack))
                {
                    backBmp.SetResolution(300, 300);

                    backBmp.RotateFlip(RotateFlipType.Rotate180FlipNone);

                    // Faut créer un bitmap de 2550x3300, soit 8.5x11 pouces en 300 DPI
                    using (var baseBmp = new Bitmap(2550, 3300, PixelFormat.Format32bppArgb))
                    {
                        baseBmp.SetResolution(300, 300);

                        using (var g = Graphics.FromImage(baseBmp))
                        {
                            const int leftCol = 201;
                            const int rightCol = 1247;
                            const int topRow = 283;
                            const int bottomRow = 1558;

                            // Fond blanc
                            g.FillRectangle(new SolidBrush(Color.White), 0, 0, baseBmp.Width, baseBmp.Height);

                            g.DrawImageUnscaled(frontBmp, leftCol, topRow);
                            g.DrawImageUnscaled(frontBmp, rightCol, topRow);

                            g.DrawImageUnscaled(backBmp, leftCol, bottomRow);
                            g.DrawImageUnscaled(backBmp, rightCol, bottomRow);
                        }

                        baseBmp.SaveAsJpeg(95, output);

                        Assert.IsTrue(File.Exists(output));
                    }
                }
            }
        }
    }
}