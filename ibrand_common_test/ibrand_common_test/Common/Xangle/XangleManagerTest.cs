using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Json;
using NomadLogic.Common.Xangle;
using NUnit.Framework;
using static NomadLogic.Common.QuestionTypeEnum;

namespace NomadLogic.Test.Common.Xangle
{
    /// <summary>
    /// Teste les capacités du manager Xangle à détecter des répertoires qui contiennent des informations de transformations Xangle
    /// et par la suite à fournir ces informations aux instances qui les demandent.
    /// </summary>
    public class XangleManagerTest : TestBaseNunit
    {
        /// <summary>
        /// Fait un test de détection de répertoires Xangle basé sur un répertoire préparé pour ce test dans les fichiers de ce projet de test.
        /// Teste la totalité des fonctionalités critiques du manager de détection Xangle.
        /// </summary>
        [Test]
        [SuppressMessage("ReSharper", "AccessToModifiedClosure")]
        public async Task DetectXangleItems()
        {
            var outputdir = GetFullOutputDir();
            // Ce répertoire contient 5 sous-répertoire, dont seulement deux devrait être détectés comme candidats.
            var baseDir = new DirectoryInfo(Path.Combine(BaseDir, "Xangle"));

            var manager = new XangleDetectionManager
            {
                BaseDirectory = baseDir,
                TimeToCheck = DateTime.Parse("2018-01-01"),
                CheckInterval = 100
            };

            var startedSignal = 0;
            var stoppedSignal = 0;
            var detectedSignal = 0;
            var lastDateCheck = DateTime.MinValue;
            manager.Started += (sender, args) =>
            {
                Console.Out.WriteLine("Started event received.");
                startedSignal++;
            };
            manager.Stopped += (sender, date) => {
                Console.Out.WriteLine("Stopped event received.");
                stoppedSignal++;
                lastDateCheck = date;
            };
            manager.NewXangleItem += (sender, detected) =>
            {
                detectedSignal += detected.NewItems.Count;
                Console.Out.WriteLine(ClassUtil.ToStringPublicProperties(detected));
            };

            var prog = new Progress<XangleReportEvent>(xEvent =>
            {
                Console.Out.WriteLine(xEvent.NewItems != null
                    ? $"Xangle items detected: Item={StringUtil.FormatEnumerable(xEvent.NewItems)}"
                    : $"Xangle report: Message={xEvent.Message}");
            });

            manager.AddProgressObserver(prog);

            Assert.IsTrue(manager.Init());

            // Écriture de la configuration initiale, pour le fun.
            Assert.IsTrue(JsonUtil.WriteJsonFile(new FileInfo(outputdir + "manager.config"), manager));
            
            await LaunchAndStopManager(manager);

            // Test sur toute la liste
            var list = manager.FoundItems();

            Assert.AreEqual(2, list.Count, "Incorrect number of found items");
            Assert.AreEqual(1, startedSignal, "Should have sent 'started' signal once.");
            Assert.AreEqual(1, stoppedSignal, "Should have sent 'stopped' signal once.");
            Assert.AreEqual(2, detectedSignal, "Should have detected 2 items.");
            
            Assert.AreEqual(lastDateCheck, manager.LastCheckDate, "Last checked date should be the EXACT same as the one sent by event.");

            var sortedList = list.OrderBy(x => x.Date).ToList();

            var tested = sortedList[0];
            Assert.That(
                tested.Date.Equals(DateTime.Parse("2018-11-23 11:17:47")) &&
                tested.Id.Equals("181123-111747.107") &&
                tested.FrameNumber == 36 &&
                tested.GetVideoFile().Exists &&
                tested.GetGifFile() == null &&
                tested.GetThumbnails().Length == 2);

            tested = sortedList[1];
            Assert.That(
                tested.Date.Equals(DateTime.Parse("2019-11-24 14:10:59")) &&
                tested.Id.Equals("191124-141059.108") &&
                tested.FrameNumber == 35 &&
                tested.GetVideoFile().Exists &&
                tested.GetGifFile().Exists &&
                tested.GetThumbnails().Length == 1);

            var rejected = manager.RejectedDirectories();

            Assert.AreEqual(1, rejected.Count, "Must be one. Two are false positives, one is BELOW the cut-off timestamp and thus should not appear.");

            /* On réinitialise maintenant le manager avec la dernière date de vérification. Il ne devrait RIEN trouver, aucun valide, et uniquement les répertoires qui ne sont pas
             des répertoires valides. */
            
            manager.Reset();
            manager.TimeToCheck = lastDateCheck;

            startedSignal = 0;
            stoppedSignal = 0;
            detectedSignal = 0;
            lastDateCheck = DateTime.MinValue;
            
            await LaunchAndStopManager(manager);

            // Test sur toute la liste (encore)
            list = manager.FoundItems();

            Assert.AreEqual(0, list.Count, "Incorrect number of found items");
            Assert.AreEqual(1, startedSignal, "Should have sent 'started' signal once.");
            Assert.AreEqual(1, stoppedSignal, "Should have sent 'stopped' signal once.");
            Assert.AreEqual(0, detectedSignal, "No new items should be detected.");
            
            Assert.AreEqual(lastDateCheck, manager.LastCheckDate, "Last checked date should be the EXACT same as the one sent by event.");
            Assert.AreEqual(1, rejected.Count, "Must be one.");
        }

        [Test]
        public void MarshallJsonConfigXangleManager()
        {
            var output = GetFullOutputDir();

            EnsureDirEmpty(output);

            var manager = new XangleDetectionManager
            {
                CheckInterval = 100,
                BaseDirectory = new DirectoryInfo(BaseDir)
            };

            var outFile = new FileInfo(output + "manager.config");

            Assert.IsTrue(JsonUtil.WriteJsonFile(outFile, manager));

            var fromFile = JsonUtil.ReadJsonFile<XangleDetectionManager>(outFile);

            Assert.AreEqual(manager.CheckInterval, fromFile.CheckInterval);
            Assert.AreEqual(manager.BaseDirectory, fromFile.BaseDirectory);
            Assert.AreEqual(manager.DatePrefix, fromFile.DatePrefix);
            Assert.AreEqual(manager.TimeToCheck, fromFile.TimeToCheck);
        }

        /// <summary>
        /// Test plus complet qui lit un fichier de configuration avec une configuration déjà établie, puis détecte des éléments,
        /// et ensuite on met ces résultats dans un questionnaire. On teste que la source du questionnaire est bel et bien modifiée.
        /// </summary>
        [Test]
        public async Task QuestionnaireMapping()
        {
            // ReSharper disable once UnusedVariable
            var outputdir = GetFullOutputDir();
            // Ce répertoire contient 5 sous-répertoire, dont seulement deux devrait être détectés comme candidats.
            var baseDir = new DirectoryInfo(Path.Combine(BaseDir, "Xangle"));

            var baseConfig = new FileInfo(baseDir.FullName + Path.DirectorySeparatorChar + "manager.config");
            Assert.IsTrue(baseConfig.Exists);

            var manager = JsonUtil.ReadJsonFile<XangleDetectionManager>(baseConfig);
            // Set la valeur de BaseDir manuellement.
            manager.BaseDirectory = baseDir;

            Assert.IsTrue(manager.Init());

            await LaunchAndStopManager(manager);

            await Console.Out.WriteLineAsync("Base directory: " + baseDir);

            var list = manager.FoundItems();

            Assert.AreEqual(2, list.Count, "Incorrect number of found items"); // Similaire au test plus haut.

            var xangleItem = list.First();

            var mockSource = new Mock<ISaveLocalRespondentData>();

            mockSource.Setup(m => m.SaveAnswerToQuestion(new Answer {QuestionType = Video, QuestionNo = 3, Response = xangleItem.GetVideoFile().FullName})).Returns(true);
            mockSource.Setup(m => m.SaveAnswerToQuestion(new Answer {QuestionType = PhotoText, QuestionNo = 4, Response = xangleItem.GetSmallThumbnail().FullName})).Returns(true);
            mockSource.Setup(m => m.SaveAnswerToQuestion(new Answer {QuestionType = PhotoText, QuestionNo = 5, Response = xangleItem.GetThumbnails()[0].FullName})).Returns(true);
            mockSource.Setup(m => m.SaveAnswerToQuestion(new Answer {QuestionType = PhotoText, QuestionNo = 6, Response = xangleItem.GetThumbnails()[1].FullName})).Returns(true);
            var questSource = mockSource.Object;

            var mockResp = new Mock<IRespondent>();
            mockResp.Setup(m => m.GetAnswer(1, 1)).Returns(new Answer{QuestionNo = 1});
            var resp = mockResp.Object;

            var mockRespSource = new Mock<ICurrentRespondentSource>();
            mockRespSource.Setup(m => m.GetValidatedCurrentRespondent()).Returns(resp);
            var respSource = mockRespSource.Object;

            Assert.IsTrue(XangleDetectionManager.SaveToQuestionnaire(questSource, respSource, manager.Mapping, xangleItem));

            // On doit avoir tenté de sauvegarder le vidéo et son thumbnail
            mockSource.Verify(m => m.SaveAnswerToQuestion(new Answer {QuestionType = Video, QuestionNo = 3, Response = xangleItem.GetVideoFile().FullName}));
            mockSource.Verify(m => m.SaveAnswerToQuestion(new Answer {QuestionType = PhotoText, QuestionNo = 4, Response = xangleItem.GetSmallThumbnail().FullName}));
            mockSource.Verify(m => m.SaveAnswerToQuestion(new Answer {QuestionType = PhotoText, QuestionNo = 5, Response = xangleItem.GetThumbnails()[0].FullName}));
            mockSource.Verify(m => m.SaveAnswerToQuestion(new Answer {QuestionType = PhotoText, QuestionNo = 6, Response = xangleItem.GetThumbnails()[1].FullName}));
        }

        [SuppressMessage("ReSharper", "ConvertToLocalFunction")]
        private static async Task LaunchAndStopManager(XangleDetectionManager manager)
        {
            await manager.Start();

            var stop = false;
            EventHandler<long> updateHandler = (sender, l) =>
            {
                manager.Stop();
            };

            EventHandler<DateTime> stopHandler = (sender, d) =>
            {
                stop = true;
            };

            manager.UpdateCompleted += updateHandler;
            manager.Stopped += stopHandler;

            var maxTime = DateTime.Now + TimeSpan.FromSeconds(5);
            while (!stop)
            {
                if (maxTime < DateTime.Now)
                {
                    Assert.Fail("Manager did not managed any update in 5 seconds.");
                }

                await Task.Delay(300);
            }
            
            manager.UpdateCompleted -= updateHandler;
            manager.Stopped -= stopHandler;

        }
    }
}