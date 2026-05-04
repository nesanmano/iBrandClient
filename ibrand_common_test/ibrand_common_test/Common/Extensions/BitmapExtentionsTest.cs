using System;
using System.Collections.Specialized;
using System.Drawing;
using System.IO;
using NomadLogic.Common.Extentions;
using NomadLogic.Common.Util;
using NUnit.Framework;

namespace NomadLogic.Test.Common.Extensions
{
    public class BitmapExtentionsTest : TestBaseNunit
    {
        [Test]
        public void TestExifMetadataDetection()
        {
            var inputFile1 = BaseDir + Path.DirectorySeparatorChar + "EXIF" + Path.DirectorySeparatorChar + "rot_0.JPG";
            var inputFile2 = BaseDir + Path.DirectorySeparatorChar + "EXIF" + Path.DirectorySeparatorChar + "rot_90.JPG";
            var inputFile3 = BaseDir + Path.DirectorySeparatorChar + "EXIF" + Path.DirectorySeparatorChar + "rot_270.JPG";
            var png = BaseDir + Path.DirectorySeparatorChar + "overlay.png";
            Assert.That(File.Exists(inputFile1));
            Assert.That(File.Exists(inputFile2));
            Assert.That(File.Exists(inputFile3));
            Assert.That(File.Exists(png));

            // Aucune rotation
            using (var bmp1 = FileUtil.LoadBitmap(inputFile1))
            {
                Assert.That(bmp1.PropertyItems.Length > 0);
                Console.Out.WriteLine("Bmp1 nbr Properties -> " + bmp1.PropertyItems.Length);
                Assert.That(bmp1.GetExifRotationValue() == ExifOrientations.TopLeft);
            }

            // Rotation avec 90 degrés
            using (var bmp2 = FileUtil.LoadBitmap(inputFile2))
            {
                Assert.That(bmp2.PropertyItems.Length > 0);
                Console.Out.WriteLine("Bmp2 nbr Properties -> " + bmp2.PropertyItems.Length);
                Assert.That(bmp2.GetExifRotationValue() == ExifOrientations.RightTop);
            }

            // Rotation avec 270 degrés (ou -90)
            using (var bmp3 = FileUtil.LoadBitmap(inputFile3))
            {
                Assert.That(bmp3.PropertyItems.Length > 0);
                Console.Out.WriteLine("Bmp3 nbr Properties -> " + bmp3.PropertyItems.Length);
                Assert.That(bmp3.GetExifRotationValue() == ExifOrientations.LeftBottom);
            }

            // Pas de EXIF
            using (var bmp4 = FileUtil.LoadBitmap(png))
            {
                Assert.That(bmp4.PropertyItems.Length > 0);
                Console.Out.WriteLine("Bmp4 (PNG file) nbr Properties -> " + bmp4.PropertyItems.Length);
                Assert.That(bmp4.GetExifRotationValue() == ExifOrientations.None);
            }
        }

        [Test]
        public void MatchExifDataTest()
        {
            var outDir = GetFullOutputDir();
            EnsureDirExists(outDir);

            var inputFile2 = BaseDir + Path.DirectorySeparatorChar + "EXIF" + Path.DirectorySeparatorChar + "rot_90.JPG";
            var original = outDir + "original.jpg";
            var modified = outDir + "modified.jpg";
            EnsureFileNotExists(modified);

            if (!File.Exists(original))
            {
                File.Copy(inputFile2, original);
                Assert.That(File.Exists(original));
            }

            File.Copy(original, modified);
            Assert.That(File.Exists(modified));

            // Même taille de fichier.
            Assert.AreEqual(new FileInfo(original).Length, new FileInfo(modified).Length);

            using (var load1Ori = FileUtil.LoadBitmap(original))
            {
                var oriSize = new Size(load1Ori.Size.Width, load1Ori.Size.Height);
                Assert.AreEqual(ExifOrientations.RightTop, load1Ori.GetExifRotationValue());

                using (var load1Mod = FileUtil.LoadBitmap(modified))
                {
                    // Même nombre de propriétés de fichiers
                    Assert.AreEqual(load1Ori.PropertyItems.Length, load1Mod.PropertyItems.Length);
                    // Rotation 
                    Assert.AreEqual(ExifOrientations.RightTop, load1Mod.GetExifRotationValue());
                    // Dimensions de pixels
                    Assert.AreEqual(oriSize, load1Mod.Size);
                }

                // Correctif Exif sur modified. Le fichier sera modifié.
                BitmapExtentions.MatchExifData(modified);

                Assert.AreNotEqual(new FileInfo(original).Length, new FileInfo(modified).Length);

                var expectedModSize = new Size(oriSize.Height, oriSize.Width); // Inverse X et Y (rotation de 90).
                Assert.AreNotEqual(oriSize, expectedModSize); // Empêche un test sur une image carrée.

                using (var load2Mod = FileUtil.LoadBitmap(modified))
                {
                    // Même nombre de propriétés de fichiers
                    Assert.AreEqual(load1Ori.PropertyItems.Length, load2Mod.PropertyItems.Length);
                    // Dimensions de pixels
                    Assert.AreEqual(expectedModSize, load2Mod.Size);
                    // Rotation
                    Assert.AreEqual(ExifOrientations.TopLeft, load2Mod.GetExifRotationValue()); // TopLeft est une rotation normale
                }
            }
        }

        [Test]
        public void BitmapLoadMethod()
        {
            var inputFile2 = BaseDir + Path.DirectorySeparatorChar + "EXIF" + Path.DirectorySeparatorChar + "rot_90.JPG";
            Assert.That(File.Exists(inputFile2));

            // Méthode 1 - Via stream
            // NOTE: On doit garder le stream ouvert tout le long de l'utilisation du bitmap. Ce qui rend cette méthode
            // Dangeureuse pour notre utilisation.
            using (var stream = new FileStream(inputFile2, FileMode.Open))
            {
                using (var result = new Bitmap(stream))
                {
                    Console.Out.WriteLine("Stream load. Nbr of properties: " + result.PropertyItems.Length);
                    Assert.That(result.PropertyItems.Length == 55);
                }
            }

            // Méthode 2 - Via constructeur
            using (var result = new Bitmap(inputFile2))
            {
                Console.Out.WriteLine("Bitmap constructor. Nbr of properties: " + result.PropertyItems.Length);
                Assert.That(result.PropertyItems.Length == 55);
            }

            // Méthode 3 - Via FileUtil
            using (var result = FileUtil.LoadBitmap(inputFile2))
            {
                Console.Out.WriteLine("FileUtil load. Nbr of properties: " + result.PropertyItems.Length);
                Assert.That(result.PropertyItems.Length == 55);
            }
        }
    }
}