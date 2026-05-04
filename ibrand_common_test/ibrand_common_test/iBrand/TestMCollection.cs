using System;
using System.Collections.Generic;
using System.IO;
using MCollections;
using NomadLogic.Common.Util;
using NUnit.Framework;

namespace NomadLogic.Test.iBrand
{
    public class TestMCollection : TestBaseNunit
    {
        [Test]
        public void TestMcollectionSet()
        {
            var baseDir = BaseDir;

            // Fichiers à tester. Cette liste n'est PAS créée dans le bon ordre volontairement.
            var dataList = new List<ComparableFileInfo>
            {
                new ComparableFileInfo(new FileInfo(baseDir + "001.jpg"))           // L=0, Natural index 0, Inverted index 7
                ,new ComparableFileInfo(new FileInfo(baseDir + "image_01.jpg"))     // L=1, Natural index 2, Inverted index 5
                ,new ComparableFileInfo(new FileInfo(baseDir + "card_front.jpg"))   // L=2, Natural index 1, Inverted index 6
                ,new ComparableFileInfo(new FileInfo(baseDir + "image_07.jpg"))     // L=3, Natural index 3, Inverted index 4
                ,new ComparableFileInfo(new FileInfo(baseDir + "sample.gif"))       // L=4, Natural index 5, Inverted index 2
                ,new ComparableFileInfo(new FileInfo(baseDir + "overlay.png"))      // L=5, Natural index 4, Inverted index 3
                ,new ComparableFileInfo(new FileInfo(baseDir + "swing_green.mp4"))  // L=6, Natural index 6, Inverted index 1
                ,new ComparableFileInfo(new FileInfo(baseDir + "video_data.txt"))   // L=7, Natural index 7, Inverted index 0
                ,new ComparableFileInfo(new FileInfo(baseDir + "video_data.txt"))   // DOUBLON. Ne devrait pas être ajouté
            };

            // Test de fenêtre d'index
            const int minIndex = 2;
            const int maxIndex = 5;

            foreach (var fic in dataList)
            {
                Assert.IsTrue(fic.FileInfoRef.Exists);
            }

            // Création des comparateurs
            var natural = Comparer<ComparableFileInfo>.Create((a, b) =>
            {
                if (a == null) return b == null ? 0 : -1;
                if (b == null) return 1;
                return a.CompareTo(b);
            });
            var inverse = Comparer<ComparableFileInfo>.Create((a, b) =>
            {
                if (b == null) return a == null ? 0 : -1;
                if (a == null) return 1;
                return b.CompareTo(a);
            });

            var setNatural = new IndexedSet<ComparableFileInfo>(natural);
            foreach (var fic in dataList)
            {
                setNatural.Add(fic);
            }
            
            #region Natural
            // Affichage
            Console.Out.WriteLine("Liste (naturel)");
            foreach (var cfi in setNatural)
            {
                Console.Out.WriteLine("\t\t" + cfi.FileInfoRef.Name);
            }
            
            // Test minimax
            Assert.AreEqual(dataList[0], setNatural.Min, $"'{dataList[0]}' doit être le plus petit élément d'une liste naturelle.");
            Assert.AreEqual(dataList[7], setNatural.Max, $"'{dataList[7]}' doit être le plus petit élément d'une liste naturelle.");

            Assert.AreEqual(8, setNatural.Count, "Il devrait y avoir 8 éléments, au lieu de 9, à cause du doublon.");

            // Tests d'indexage
            // Ces éléments devraîent être les mêmes après l'indexage.
            Assert.AreEqual(dataList[1], setNatural[2], $"'{dataList[1]}' doit être à l'index 2 d'une liste naturelle.");
            Assert.AreEqual(dataList[2], setNatural[1], $"'{dataList[2]}' doit être à l'index 1 d'une liste naturelle.");
            
            // Pas de méthode spécifique, il faut boucler par index.
            var showList = new List<ComparableFileInfo>();
            for (var i = minIndex; i <= maxIndex; i++)
            {
                showList.Add(setNatural[i]);
            }
            
            // Test sur cette liste:
            Assert.AreEqual(dataList[1], showList[0], $"'{dataList[1]}' doit être à l'index 0 d'une liste partielle [2-5].");
            Assert.AreEqual(dataList[3], showList[1], $"'{dataList[3]}' doit être à l'index 1 d'une liste partielle [2-5].");
            Assert.AreEqual(dataList[5], showList[2], $"'{dataList[5]}' doit être à l'index 2 d'une liste partielle [2-5].");
            Assert.AreEqual(dataList[4], showList[3], $"'{dataList[4]}' doit être à l'index 3 d'une liste partielle [2-5].");
            #endregion

            // ***************** On inverse la liste maintenant
            var setInverse = new IndexedSet<ComparableFileInfo>(inverse);

            // On verse le contenu du set normal dans le nouveau
            foreach (var cfi in setNatural)
            {
                setInverse.Add(cfi);
            }

            #region Inverse
            // Affichage
            Console.Out.WriteLine("Liste (inverse)");
            foreach (var cfi in setInverse)
            {
                Console.Out.WriteLine("\t\t" + cfi.FileInfoRef.Name);
            }

            // Test Min/Max
            Assert.AreEqual(dataList[7], setInverse.Min, $"'{dataList[7]}' doit être le plus petit élément d'une liste inversée.");
            Assert.AreEqual(dataList[0], setInverse.Max, $"'{dataList[0]}' doit être le plus petit élément d'une liste inversée.");

            // Tests d'indexage
            // Ces éléments devraîent être les mêmes après l'indexage.
            Assert.AreEqual(dataList[4], setInverse[2], $"'{dataList[4]}' doit être à l'index 4 d'une liste inversée.");
            Assert.AreEqual(dataList[6], setInverse[1], $"'{dataList[6]}' doit être à l'index 6 d'une liste inversée.");
            
            // Pas de méthode spécifique, il faut boucler par index.
            showList = new List<ComparableFileInfo>();
            for (var i = minIndex; i <= maxIndex; i++)
            {
                showList.Add(setInverse[i]);
            }

            // Test sur cette liste:
            Assert.AreEqual(dataList[4], showList[0], $"'{dataList[4]}' doit être à l'index 0 d'une liste inversée partielle [2-5].");
            Assert.AreEqual(dataList[5], showList[1], $"'{dataList[5]}' doit être à l'index 1 d'une liste inversée partielle [2-5].");
            Assert.AreEqual(dataList[3], showList[2], $"'{dataList[3]}' doit être à l'index 2 d'une liste inversée partielle [2-5].");
            Assert.AreEqual(dataList[1], showList[3], $"'{dataList[1]}' doit être à l'index 3 d'une liste inversée partielle [2-5].");
            #endregion

            // Avec les mêmes références
            Assert.IsTrue(setNatural.IsSubsetOf(setInverse));

            // Tests avec des références différentes
            var copySetInverse = new IndexedSet<ComparableFileInfo>(inverse);
            foreach (var el in setInverse)
            {
                copySetInverse.Add(new ComparableFileInfo(new FileInfo(el.FileInfoRef.FullName)));
            }

            Assert.IsTrue(copySetInverse.IsSubsetOf(setInverse));
            Assert.IsTrue(copySetInverse.IsSubsetOf(setNatural));
        }
    }
}