using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using NomadLogic.Common.Util.Math;
using NUnit.Framework;

namespace NomadLogic.Test.Common.Util
{
    public class MathUtilTest : TestBaseNunit
    {
        // ReSharper disable once UnusedMember.Local
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        [Test]
        public void ByteFormatTest()
        {
            var input = new ulong[]
            {
                1000,
                1025,
                2600,
                3900678,
                4567987543,
                354678654321,
                890576243678890,
                890576243678890567
            };

            foreach (var val in input)
            {
                Console.Out.WriteLine($"{MathUtil.FormatByteNumber(val)} - {MathUtil.FormatByteNumber(val, true)}");
            }
        }

        [Test]
        public void ByteFormatFileListSizeTest()
        {
            var fileList = Directory.GetFiles(BaseDir).Select(f => new FileInfo(f));

            foreach (var val in fileList.Where(f => f.Exists))
            {
                Console.Out.WriteLine($"{val.Name} : {MathUtil.FormatByteNumber(val.Length, true)}");
            }
        }

        /// <summary>
        /// Test pour la très complexe méthode de calcul d'un bounding box interne suite à une rotation.
        /// </summary>
        [Test]
        public void CalculateInternalBoundingBoxTest()
        {
            // Angle zéro
            Assert.IsTrue(MathUtil.CalculateRotationInternalBoundingBox(100, 200, 0, out var res));
            Assert.IsFalse(res.Width != 100 || res.Height != 200 || res.X != 0 || res.Y != 0);

            for (var i = 0; i < 360; i += 5)
            {
                // angle normal avec des valeurs complexes.
                Assert.IsTrue(MathUtil.CalculateRotationInternalBoundingBox(2592, 1728, i, out res));
                Assert.IsFalse(res.Width > 2592 || res.Height > 1728 || res.X < 0 || res.Y < 0,
                    "Assert failed on angle= " + i + ", rec= " + res);

                // Angle négatif
                Assert.IsTrue(MathUtil.CalculateRotationInternalBoundingBox(2592, 1728, -i, out res));
                Assert.IsFalse(res.Width > 2592 || res.Height > 1728 || res.X < 0 || res.Y < 0,
                    "Assert failed on angle -" + i + ", rec= " + res);
            }

            for (var i = 0; i < 360; i += 5)
            {
                // Égalité de superficie entre un angle positif et négatif, sur un carré.
                Assert.IsTrue(MathUtil.CalculateRotationInternalBoundingBox(200, 200, i, out res));
                Assert.IsTrue(MathUtil.CalculateRotationInternalBoundingBox(200, 200, -i, out var res2));
                Assert.IsTrue(res.Equals(res2), "Assert failed on angle " + i + ", rec1= " + res + ", rec2= " + res2);
            }
        }

        [Test]
        public void TestGetNewSizeToFitInsideContainer()
        {
            var testValues = new []
            {
                new double[] // Test d'égalité des rectangles
                {
                    100, 100, 100, 100, // Original
                    100, 100, 1 // Résultat
                }
                ,new double[] // Test d'un redimensionnement carré < 1, sur la longeur
                {
                    100, 1000, 200, 200, // Original
                    100, 100, 2 // Résultat
                }
                ,new double[] // Test d'un redimensionnement carré > 1, sur la longeur
                {
                    300, 1000, 200, 200, // Original
                    300, 300, 3 // Résultat
                }
                // ratio contenant > 1
                ,new double[] 
                {
                    1000, 250, 100, 500, // Original
                    50, 250, 4 // Résultat
                }
                ,new double[]
                {
                    1000, 250, 10, 50, // Original
                    50, 250, 5 // Résultat
                },new double[]
                {
                    1000, 250, 1500, 1000, // Original
                    375, 250, 6 // Résultat
                },new[]
                {
                    1000, 250, 1500, 100, // Original
                    1000, 66.6666, 7 // Résultat
                }
                ,new[]
                {
                    1000, 250, 1000, 1500, // Original
                    166.6666, 250, 8 // Résultat
                },new double[]
                {
                    1000, 250, 500, 100, // Original
                    1000, 200, 9 // Résultat
                },new double[]
                {
                    1000, 250, 100, 500, // Original
                    50, 250, 10 // Résultat
                },new[]
                {
                    1000, 250, 100, 150, // Original
                    166.6666, 250, 11 // Résultat
                }
                // ratio contenant < 1
                ,new double[]
                {
                    250, 1000, 100, 500, // Original
                    200, 1000, 12 // Résultat
                }
                ,new double[]
                {
                    250, 1000, 10, 50, // Original
                    200, 1000, 13 // Résultat
                },new[]
                {
                    250, 1000, 1500, 1000, // Original
                    250, 166.6666, 14 // Résultat
                },new[]
                {
                    250, 1000, 1500, 100, // Original
                    250, 16.6666, 15 // Résultat
                }
                ,new double[]
                {
                    250, 1000, 1000, 1500, // Original
                    250, 375, 16 // Résultat
                },new double[]
                {
                    250, 1000, 500, 100, // Original
                    250, 50, 17 // Résultat
                },new double[]
                {
                    250, 1000, 100, 500, // Original
                    200, 1000, 18 // Résultat
                },new double[]
                {
                    250, 1000, 100, 150, // Original
                    250, 375, 19 // Résultat
                }
                // Autres
                ,new[]
                {
                    500, 400, 100, 150, // Original
                    266.6666, 400, 20 // Résultat
                }
                ,new[]
                {
                    500, 400, 150, 100, // Original
                    500, 333.3333, 21 // Résultat
                },new[]
                {
                    500, 400, 10, 150, // Original
                    26.6666, 400, 22 // Résultat
                }
                ,new[]
                {
                    500, 400, 150, 10, // Original
                    500, 33.3333, 23 // Résultat
                },new[]
                {
                    500, 40, 100, 150, // Original
                    26.6666, 40, 24 // Résultat
                }
                ,new double[]
                {
                    500, 40, 150, 100, // Original
                    60, 40, 25 // Résultat
                },new[]
                {
                    500, 40, 10, 150, // Original
                    2.6666, 40, 26 // Résultat
                }
                ,new[]
                {
                    500, 40, 150, 10, // Original
                    500, 33.3333, 27 // Résultat
                }
            };

            // On teste voir si tous les tests ont l'indice 6 (le numéro de test) est unique.
            var set = new HashSet<int>();
            foreach (var test in testValues)
            {
                set.Add((int)test[6]);
            }
            Assert.IsTrue(set.Count == testValues.Length);

            foreach (var tab in testValues)
            {
                // Pour fin de tests, isoler le test problématique. À commenter pour tout faire.
                //if (tab[6] != 12) continue;

                var newSize = MathUtil.GetNewSizeToFitInsideContainer(tab[0], tab[1], tab[2], tab[3]);
                var newSizeRatio = MathUtil.GetNewSizeToFitInsideContainer(tab[0], tab[1], tab[2]/tab[3]);

                Assert.IsTrue(Math.Abs(newSize.Width - newSizeRatio.Width) < MathUtil.TOLERANCE_FLOATING_POINT || Math.Abs(newSize.Height - newSizeRatio.Height) < MathUtil.TOLERANCE_FLOATING_POINT,
                    "Error[{2}]: (Size = SizeByRatio) => ({0} == {1})",
                    newSize, newSizeRatio, tab[6]);
                Assert.IsTrue(Math.Abs(newSize.Width - tab[0]) < MathUtil.TOLERANCE_FLOATING_POINT || Math.Abs(newSize.Height - tab[1]) < MathUtil.TOLERANCE_FLOATING_POINT,
                    "Error[{4}]: (Xp == C || Yp == V) => ({0} == {1} || {2} == {3})",
                    newSize.Width, tab[0], newSize.Height, tab[1], tab[6]);
                Assert.IsTrue(newSize.Width <= tab[0] && newSize.Height <= tab[1],
                    "Error[{4}]: (Xp <= C && Yp <= V) => ({0} <= {1} && {2} <= {3})",
                    newSize.Width, tab[0], newSize.Height, tab[1], tab[6]);
                Assert.IsTrue(Math.Abs(tab[2]/(float) tab[3] - newSize.Width/newSize.Height) < MathUtil.TOLERANCE_FLOATING_POINT,
                    "Error[{4}]: (X/Y == Xp/Yp) => ({0}/{1} == {2}/{3})",
                    tab[2], tab[3], newSize.Width, newSize.Height, tab[6]);
                Assert.IsTrue(Math.Abs(newSize.Width - tab[4]) < MathUtil.TOLERANCE_FLOATING_POINT && Math.Abs(newSize.Height - tab[5]) < MathUtil.TOLERANCE_FLOATING_POINT,
                    "Error[{4}]: expected ({0},{1}), got ({2},{3})",
                    tab[4], tab[5], newSize.Width, newSize.Height, tab[6]);
            }
        }
    }
}
