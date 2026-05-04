using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Globalization;
using System.IO;
using System.Linq;
using NomadLogic.Common.Extentions;
using NomadLogic.Common.Util;
using NomadLogic.Multimedia;
using NUnit.Framework;

// ReSharper disable AssignNullToNotNullAttribute

namespace NomadLogic.Test.Common.Util
{
    public class ImageUtilTests : TestBaseNunit
    {

        #region Iris

        private struct PhotoIrisData
        {
            public readonly string _originalFile;
            public readonly Point _p1;
            public readonly Point _p2;
            public readonly Point? _pc;
            public readonly int _occDistance;

            public PhotoIrisData(string ori, Point p1, Point p2, Point? pc, int occDist)
            {
                _originalFile = ori;
                _p1 = p1;
                _p2 = p2;
                _pc = pc;
                _occDistance = occDist;
            }
        }

        [Test]
        public void TestAddGlassesToResults()
        {
            var oriDir = @"C:\Users\prog\Documents\projets\2015\IRI-2015-01 Iris Pilote\tests\ftp\test_photobooth\";
            var oriResult = @"C:\Users\prog\Documents\projets\2015\IRI-2015-01 Iris Pilote\tests\ftp\testResult\";
            var oriLunetteProfil =
                @"C:\Users\prog\Documents\projets\2015\IRI-2015-01 Iris Pilote\tests\lunettes_profil\"; // Fixe
            var oriLunetteFace = @"C:\Users\prog\Documents\projets\2015\IRI-2015-01 Iris Pilote\tests\lunettes_face\";
            //Fixe

            RecursiveLunetteTransformation(oriDir, oriResult, oriLunetteProfil, oriLunetteFace);
        }

        private static void RecursiveLunetteTransformation(string oriDir, string oriResult, string oriLunetteProfil,
            string oriLunetteFace)
        {
            var subDirs = Directory.GetDirectories(oriDir);

            foreach (var dir in subDirs)
            {
                var resultDir = oriResult + Path.DirectorySeparatorChar + StringUtil.GetFileName(dir);
                Directory.CreateDirectory(resultDir);
                // Recursive call
                RecursiveLunetteTransformation(dir, resultDir, oriLunetteProfil, oriLunetteFace);
            }

            var files = Directory.GetFiles(oriDir, "*.png");
            foreach (var file in files)
            {
                string[] lunetteFiles;
                Point center;
                if (file.Contains("face"))
                {
                    lunetteFiles = Directory.GetFiles(oriLunetteFace, "*.png");
                    center = new Point(275, 145);
                }
                else
                {
                    lunetteFiles = Directory.GetFiles(oriLunetteProfil, "*.png");
                    center = new Point(250 + 20, 145);
                }

                var i = 0;

                foreach (var lunette in lunetteFiles)
                {
                    i++;

                    var result = oriResult + Path.DirectorySeparatorChar + i + "_" + StringUtil.GetFileName(file);

                    if (!File.Exists(result))
                    {
                        using (var over = new Bitmap(lunette))
                        {
                            var xOffset = center.X - (over.Width / 2);
                            var yOffset = center.Y - (over.Height / 2);

                            using (var tmp = new Bitmap(file))
                            {
                                tmp.AddOverlay(over, xOffset, yOffset);
                                tmp.Save(ImageFormat.Png, 9, result);
                            }
                        }
                    }
                }
            }
        }

        [Test]
        public void TestImageUtilNormalizeImage()
        {
            const string oriDir = @"C:\Users\prog\Documents\projets\2015\IRI-2015-01 Iris Pilote\tests\";
            const float yCenter = 0.33f; // Pourcentage du point central en Y.
            const double pixelDistAbs = 0.4128;
            const double minAngle = 2.0;
            var cadreTest = oriDir + "cadre_test.png";

            var listeSujets = new List<PhotoIrisData>
            {
                new PhotoIrisData(oriDir + "fred.JPG", new Point(903, 372), new Point(1025, 367), null, 58),
                new PhotoIrisData(oriDir + "caro.JPG", new Point(594, 378), new Point(684, 370), null, 60),
                new PhotoIrisData(oriDir + "pascale.JPG", new Point(315, 200), new Point(379, 199), null, 54),
                new PhotoIrisData(oriDir + "natasha.JPG", new Point(294, 255), new Point(406, 258), null, 55),
                new PhotoIrisData(oriDir + "mat.JPG", new Point(426, 328), new Point(314, 306), null, 64)
            };

            // Photo rectangulaire, 1920x1280
            // Photo carrée, 1280, 1280
            // Photo carrée, 700x700
            // Photo carrée, cadrée serrée, 700x700
            // Photo carrée, sujet penché sur côté, inversion des points 700x700

            string[] fichiersLunettes =
            {
                oriDir + "lunette_01.png",
                oriDir + "lunette_02.png",
                oriDir + "lunette_03.png",
                oriDir + "lunette_04.png",
            };

            // Boucle principale du test.

            foreach (var sujet in listeSujets)
            {
                // On calcule le point central, puisque l'on a des tailles de fichiers différentes.
                Point center;
                using (var bmp = new Bitmap(sujet._originalFile))
                {
                    center = new Point(bmp.Width / 2, (int)(bmp.Height * yCenter));
                }

                using (var ori = new Bitmap(sujet._originalFile))
                {
                    // On normalize
                    using (
                        var res = ori.NormalizeToStaticCoordinate(sujet._p1, sujet._p2, sujet._pc,
                            sujet._occDistance, center,
                            pixelDistAbs, minAngle))
                    {
                        SaveFile(oriDir, sujet, res, "norm_");

                        using (var cropped = res.Crop(ori.Width, ori.Height))
                        {
                            //saveFile(oriDir, sujet, cropped, "crop_");

                            SaveFileWithOverlay(oriDir, cadreTest, sujet, ref center, cropped, "cadre_");

                            for (var i = 0; i < fichiersLunettes.Length; i++)
                            {
                                SaveFileWithOverlay(oriDir, fichiersLunettes[i], sujet, ref center, cropped,
                                    "lunette_" + i + "_");
                            }
                        }
                    }
                }

            }
        }

        private static void SaveFileWithOverlay(string oriDir, string overlayFile, PhotoIrisData sujet, ref Point center,
            Bitmap baseImage, string prefix)
        {
            using (var over = new Bitmap(overlayFile))
            {
                // On met maintenant le cadre de lunette test
                var xOffset = center.X - (over.Width / 2);
                var yOffset = center.Y - (over.Height / 2);

                using (var tmp = new Bitmap(baseImage))
                {
                    tmp.AddOverlay(over, xOffset, yOffset);
                    SaveFile(oriDir, sujet, tmp, prefix);
                }
            }
        }

        private static void SaveFile(string oriDir, PhotoIrisData sujet, Bitmap image, string prefix)
        {
            var file = oriDir + "result" + Path.DirectorySeparatorChar + prefix +
                       StringUtil.GetFileName(sujet._originalFile) + ".png";
            image.Save(ImageFormat.Png, 9, file);
        }

        #endregion Iris

        #region BitmapModification

        /// <summary>
        /// Ce test transforme tous les fichiers dans le répertoire d'image, pour vérifier s'ils donnent un bon résultat pour la transformation de couleur.
        /// <br/>Quand un fichier ne donne pas de bons résultats, c'est qu'il faut le retravailler. Idéalement dans un logiciel de dessein vectoriel, et le
        /// sauvegarder en PNG (le format JPEG perd des informations, ce qui rend la transformation hasardeuse).
        /// </summary>
        [Test]
        public void TestModifyEveryBitmapAsset() {
            // Préparation des images de base
            var directoryInfo = new DirectoryInfo(BaseDir).Parent;
            Assert.IsTrue(directoryInfo != null);
            var imageDir = directoryInfo.FullName + @"\images\";
            Assert.IsTrue(Directory.Exists(imageDir));

            var outputDir = OutputBaseDir + GetCurrentMethodName() + Path.DirectorySeparatorChar;
            EnsureDirExists(outputDir);
            
            var bmf = new BitmapModificationFilter(new List<ColorSubstitutionFilter>
                                                   {
                                                       new ColorSubstitutionFilter(10, Color.Black, Color.Red),
                                                       new ColorSubstitutionFilter(10, Color.White, Color.Blue)
                                                   }, 1.5f);

            var fileList = new List<FileInfo>();

            FileUtil.RecursiveImageFileFinder(fileList, imageDir);

            var dirPrefixLenght = new DirectoryInfo(imageDir).FullName.Length;

            foreach (var file in fileList)
            {
                using (var bmpOri = FileUtil.LoadBitmap(file.FullName))
                {
                    using (var bmpMod = ImageUtil.ReplaceColors(bmpOri, ColorSubstitutionFilterGroup.All, bmf))
                    {
                        var f = bmpMod.Width * bmf.SizeScale;
                        if (f == null) continue;
                        var o = bmpMod.Height * bmf.SizeScale;
                        if (o == null) continue;
                        using (
                            var bmpResized = bmpMod.Resize((int) f, (int) o))
                        {
                            var resultFile = outputDir + file.FullName.Substring(dirPrefixLenght);
                            var dir = new FileInfo(resultFile).DirectoryName;
                            if (!Directory.Exists(dir))
                            {
                                Directory.CreateDirectory(dir);
                            }
                            Assert.IsTrue(file.Name.EndsWith(".png", StringComparison.OrdinalIgnoreCase)
                                ? bmpResized.Save(ImageFormat.Png, 9, resultFile)
                                : bmpResized.Save(ImageFormat.Jpeg, 90, resultFile));
                        }
                    }
                }
            }
        }

        #endregion BitmapModification

        #region Text On Bitmap

        [Test]
        public void PutRotatedCustomFontTextOnImage()
        {
            var outDir = Directory.GetCurrentDirectory() + @"\result\" + GetCurrentMethodName();
            EnsureDirExists(outDir);

            var inputFile = BaseDir + Path.DirectorySeparatorChar + "image_font.png";
            Assert.IsTrue(File.Exists(inputFile));
            var inputFileRotated = BaseDir + Path.DirectorySeparatorChar + "image_font2.png";
            Assert.IsTrue(File.Exists(inputFileRotated));
            var fontBoldItalicFileName = BaseDir + Path.DirectorySeparatorChar + "Louis-BoldItalic.otf";
            Assert.IsTrue(File.Exists(fontBoldItalicFileName));
            var fontRegularFileName = BaseDir + Path.DirectorySeparatorChar + "Louis-Regular.otf";
            Assert.IsTrue(File.Exists(fontRegularFileName));

            var privateFontCollection = new PrivateFontCollection();
            privateFontCollection.AddFontFile(fontRegularFileName);
            privateFontCollection.AddFontFile(fontBoldItalicFileName);

            var fontFamilly = privateFontCollection.Families.First(fontCheck => fontCheck.Name.Contains("Louis"));
            Assert.IsNotNull(fontFamilly);
            const float scale = 4f;
            const int rotateOffset = -250;
            var fontPrenomFront = new Font(fontFamilly, 10.92f * scale, FontStyle.Bold | FontStyle.Italic, GraphicsUnit.Pixel);
            var fontNomFront = new Font(fontFamilly, 19.94f * scale, FontStyle.Bold | FontStyle.Italic, GraphicsUnit.Pixel);
            var fontPrenomBack = new Font(fontFamilly, 12.33f * scale, FontStyle.Bold | FontStyle.Italic, GraphicsUnit.Pixel);
            var fontNomBack = new Font(fontFamilly, 18.49f * scale, FontStyle.Bold | FontStyle.Italic, GraphicsUnit.Pixel);
            var couleurPrenom = new SolidBrush(Color.White);
            var couleurNom = new SolidBrush(Color.FromArgb(255, 228, 160, 38));

            string[,] names = {
                { "JOHNNY", "SMITHERSONIAN" },
                { "A" , "B" },
                { "JULIE" , "MACHIN" },
                { "AtramayanUiyTayalatut" , "BabouTrimalachikNaya" },
                { "BatMan" , "Spiderman" },
                { "John" , "Doe" },
                { "wwwwwwwwwwwwwww" , "wwwwwwwwwwwwwww" },
                { "àÂéèÊçÇôòùû" , "àÂéèÊçÇôòùû" }
            };

            for (var i = 0; i < names.GetLength(0); i++)
            {
                using (var bmp1 = FileUtil.LoadBitmap(inputFileRotated))
                {
                    bmp1.AddTextToBitmap(names[i, 0].ToUpper(), 0 + rotateOffset, 943, false, couleurPrenom, fontPrenomFront, -22.45f);
                    bmp1.AddTextToBitmap(names[i, 1].ToUpper(), 42 + rotateOffset, 974, false, couleurNom, fontNomFront, -22.45f);

                    var output = outDir + Path.DirectorySeparatorChar + "back_" + $"{i:###}" + ".jpg";
                    EnsureFileNotExists(output);

                    bmp1.SaveAsJpeg(95, output);
                    Assert.IsTrue(File.Exists(output));
                }

                using (var bmp2 = FileUtil.LoadBitmap(inputFile))
                {
                    bmp2.AddTextToBitmap(names[i, 0].ToUpper(), 70, 80, false, couleurPrenom, fontPrenomBack);
                    bmp2.AddTextToBitmap( names[i, 1].ToUpper(), 112, 119, false, couleurNom, fontNomBack);

                    var output = outDir + Path.DirectorySeparatorChar + "front_" + $"{i:###}" + ".jpg";
                    EnsureFileNotExists(output);

                    bmp2.SaveAsJpeg(95, output);
                    Assert.IsTrue(File.Exists(output));
                }
            }
        }

        #endregion Text On Bitmap

        #region Show Transparancy

        [Test]
        public void TestShowTransparency()
        {
            var outDir = Directory.GetCurrentDirectory() + @"\result\" + GetCurrentMethodName();
            EnsureDirExists(outDir);

            var inputFile = BaseDir + Path.DirectorySeparatorChar + "statue_model_01.png";
            Assert.IsTrue(File.Exists(inputFile));
            var outFile = outDir + Path.DirectorySeparatorChar + "statue_model_01_mod.png";
            EnsureFileNotExists(outFile);

            File.Copy(inputFile, outFile);
            Assert.IsTrue(File.Exists(outFile));

            ImageUtil.DebugDisplayTransparencyOnImage(outFile);

            Assert.IsTrue(File.Exists(outFile));
        }

        #endregion

        [Test]
        public void TestCreateCardPrint()
        {
            var outDir = Directory.GetCurrentDirectory() + @"\result\" + GetCurrentMethodName();
            EnsureDirExists(outDir);

            var inputFile1 = BaseDir + Path.DirectorySeparatorChar + "card_front.jpg";
            Assert.IsTrue(File.Exists(inputFile1));
            var inputFile2 = BaseDir + Path.DirectorySeparatorChar + "card_back.jpg";
            Assert.IsTrue(File.Exists(inputFile2));
            var outFile = outDir + Path.DirectorySeparatorChar + "full_card.jpg";
            EnsureFileNotExists(outFile);
            
            var final = ImageUtil.CreateCardPrintBitmap(FileUtil.LoadBitmap(inputFile1), FileUtil.LoadBitmap(inputFile2));
            final.SaveAsJpeg(90, outFile);

            Assert.IsTrue(File.Exists(outFile));
        }

        [Test]
        public void TestImageRotation()
        {
            var originalImageFile = BaseDir + Path.DirectorySeparatorChar + "carte_hockey_lines.png";
            Assert.IsTrue(File.Exists(originalImageFile));
            var outputDir = OutputBaseDir + GetCurrentMethodName() + Path.DirectorySeparatorChar;
            EnsureDirExists(outputDir);
            
            using (var bmp = FileUtil.LoadBitmap(originalImageFile))
            {
                using (var res = bmp.Rotate(12, 1138, 1484))
                    res.SaveAsJpeg(90, outputDir + "01_between_images_12.jpg");

                using (var res = bmp.Rotate(12))
                    res.SaveAsJpeg(90, outputDir + "02_center_12.jpg");

                using (var res = bmp.Rotate(12, 0, 0))
                    res.SaveAsJpeg(90, outputDir + "03_origin0x0_12.jpg");

                for (var angle = 0; angle <= 360; angle += 15)
                {
                    using (var res = bmp.Rotate(angle, 1660, 2110))
                        res.SaveAsJpeg(90, outputDir + $"04_full_rotate_bottom_right_box_{angle}.jpg");
                }

                for (var angle = 0f; angle <= 3; angle += 0.3f)
                {
                    using (var res = bmp.Rotate(angle, 620, 810))
                        res.SaveAsJpeg(90, outputDir + $"05_full_rotate_bottom_right_box_{angle.ToString("0.00", CultureInfo.InvariantCulture)}.jpg");
                }
            }
        }
    }
}
