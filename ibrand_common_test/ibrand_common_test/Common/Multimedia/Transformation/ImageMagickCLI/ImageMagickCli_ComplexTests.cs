using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NomadLogic.Common.Extentions;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Services;
using NomadLogic.Common.Services.Configuration;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.DateAndTime;
using NomadLogic.Multimedia.Cli.ImageMagickCLI;
using NomadLogic.Test.Common.Respondent;
using NUnit.Framework;
using static NomadLogic.Common.QuestionTypeEnum;
// ReSharper disable RedundantArgumentDefaultValue

namespace NomadLogic.Test.Common.Multimedia.Transformation.ImageMagickCLI
{
    // ReSharper disable once InconsistentNaming
    public class ImageMagickCli_ComplexTests : TestBaseNunit
    {
        protected static async Task ExecuteImageMagickCli(string baseFileDir, MockRespondent resp, Dictionary<QuestionIdentifier, Answer> answers, ImageMagickCliManager cliManager)
        {
            var report = new Progress<ImageMagickCliUiUpdate>(clireport =>
            {
                Console.Out.WriteLine($"Time:({clireport.StepTime.ToStringHourMinSec(true, true)})/({clireport.RunningTime.ToStringHourMinSec(true, true)}), Steps: {clireport.CurrentStep}/{clireport.StepTotal} - {StringUtil.FormatFloat(2, clireport.PercentCompletion)}%, Description={clireport.CurrentStepDescription}");
            });

            var cts = new CancellationTokenSource();

            var success = await cliManager.ExecuteCommandOnRespondent(resp, answers, baseFileDir, cts.Token, report);

            TestCliResult(success);
        }

        /// <summary>
        /// Vérifie que le résultat du test du CLI donne des résultats corrects.
        /// On vérifie qu'il existe des résultats de fichier d'images, et que ces fichiers existent bel et bien.
        /// </summary>
        /// <param name="success">Le résultat de l'opération.</param>
        /// <param name="specificFileCountResult">SI on veut tester qu'il existe un nombre précis de fichiers résultats.</param>
        protected static void TestCliResult(ImageMagickCliResult success, int? specificFileCountResult = null)
        {
            Assert.IsNotNull(success?.ResultFiles);
            if (!specificFileCountResult.HasValue)
            {
                Assert.Greater(success.ResultFiles.Count, 0); // Au moins une fichier résultat créé.
            }
            else
            {
                Assert.AreEqual(specificFileCountResult.Value, success.ResultFiles.Count);
            }

            foreach (var createdFile in success.ResultFiles)
            {
                var file = createdFile.Value;
                Assert.IsTrue(file.Exists && file.Length > 100);
                using (var bmp = FileUtil.LoadBitmap(file.FullName))
                {
                    Assert.AreNotEqual(Size.Empty, bmp.Size);
                }
            }
        }

        /// <summary>
        /// Teste si l'installation de ImageMagick est détectée sur le système.
        /// </summary>
        [TestCase(null)]
        public void ImageMagickInstallTest(string customImageMagickExec = null)
        {
            var nomadConfig = ServicesContainer.GetService<NomadConfiguration>();

            if (customImageMagickExec != null)
            {
                nomadConfig.ImageMagickLocation = customImageMagickExec;
            }

            var test = ImageMagickCliUtil.TestExecutableLocation(nomadConfig, new VersionInfo {Major = 7});
            if (!test) Assert.Ignore("ImageMagick version 7 is not detected on your system.");
        }

        /// <summary>
        /// Créé un répondant plus questionnaire complet pour exécuter la transformation du projet INF-2018-01.
        /// Qui est l'origine du projet IM-CLI.
        /// </summary>
        [Test]
        public async Task DuotoneWithRespondentFull()
        {
            var output = GetFullOutputDir();
            EnsureDirEmpty(output);
            var gauche = BaseDir + "sujet_00.jpg";
            var droite = BaseDir + "sujet_01.jpg";
            var baseFileDir = BaseDir + @"imageMagick_CLI\";
            var cliFile = baseFileDir + "duotone_im_cli_unoptimized.txt";
            var colorFile = baseFileDir + "overlay blueberry acai.jpg";
            var textFile = baseFileDir + "overlay texte EN.png";
            EnsureFileExists(gauche, droite, cliFile, colorFile, textFile);

            ImageMagickInstallTest();

            var cliManager = new ImageMagickCliManager
            {
                CommandFile = cliFile,
                OutputDir = output,
                DebugKeepTemporaryFiles = true
            };

            Assert.IsTrue(cliManager.Init());

            // Vérification complète des commandes
            Assert.AreEqual(8, cliManager.Commands.Count);

            var resp = new MockRespondent();
            var ans1 = new List<Answer>
            {
                new Answer{QuestionType = PhotoText, QuestionNo = 1, ChoiceNo = 1, Response = gauche},
                new Answer{QuestionType = PhotoText, QuestionNo = 2, ChoiceNo = 1, Response = droite},
                new Answer{QuestionType = Text, QuestionNo = 3, ChoiceNo = 1, Response = "109,93,161"},
                new Answer{QuestionType = Text, QuestionNo = 4, ChoiceNo = 1, Response = "135,200,209"},
                new Answer{QuestionType = Text, QuestionNo = 5, ChoiceNo = 1, Response = colorFile},
                new Answer{QuestionType = Text, QuestionNo = 6, ChoiceNo = 1, Response = textFile}
            };
            var dico1 = ans1.ToDictionary(a => a.QuestionIdentifier);

            await ExecuteImageMagickCli(baseFileDir, resp, dico1, cliManager);
        }

        /// <summary>
        /// Teste plusieurs effets de duotones sur des groupes d'images, avec un effet aléatoire.
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task MultipleDuotones()
        {
            var output = GetFullOutputDir();
            EnsureDirEmpty(output);
            var baseFileDir = BaseDir + @"imageMagick_CLI\";
            var cliFile = baseFileDir + "duotone_im_cli.txt";
            var gauche = new[]
            {
                baseFileDir + "l_00.JPG",
                baseFileDir + "l_01.JPG",
                baseFileDir + "l_02.JPG",
                baseFileDir + "l_03.JPG",
            };
            var droite = new[]
            {
                baseFileDir + "r_00.JPG",
                baseFileDir + "r_01.JPG",
                baseFileDir + "r_02.JPG",
                baseFileDir + "r_03.JPG",
            };
            var colorFile = new[]
            {
                baseFileDir + "overlay blueberry acai.jpg",
                baseFileDir + "overlay tangerine lime.jpg"
            };
            var textFile = new[]
            {
                baseFileDir + "overlay texte EN.png",
                baseFileDir + "overlay texte FR.png"
            };
            EnsureFileExists(gauche, droite, colorFile, textFile);

            var colorGroup = new[,]
            {
                {"109,93,161","135,200,209"},
                {"208,78,40","148,190,87"},
            };

            ImageMagickInstallTest();

            var cliManager = new ImageMagickCliManager
            {
                CommandFile = cliFile,
                OutputDir = output,
                DebugKeepTemporaryFiles = false
            };

            Assert.IsTrue(cliManager.Init());

            var resp = new MockRespondent();
            var seed = DateTime.Now.Ticks;
            gauche.Shuffle(seed);
            droite.Shuffle(seed+1);
            
            for (var i = 0; i < Math.Min(gauche.Length, droite.Length); i++)
            {

                var fileGroupIndex = i % 2;
                /*
                var answers1 = new Dictionary<int, Dictionary<int, Answer>>
                {
                    {1, new Dictionary<int, Answer> {{1, new Answer(11, 1, 1, -1, gauche[i])}}},
                    {2, new Dictionary<int, Answer> {{1, new Answer(11, 2, 1, -1, droite[i])}}},
                    {3, new Dictionary<int, Answer> {{1, new Answer(4, 3, 1, -1, colorGroup[fileGroupIndex, 0]) }}},
                    {4, new Dictionary<int, Answer> {{1, new Answer(4, 4, 1, -1, colorGroup[fileGroupIndex, 1]) }}},
                    {5, new Dictionary<int, Answer> {{1, new Answer(4, 5, 1, -1, colorFile[fileGroupIndex])}}},
                    {6, new Dictionary<int, Answer> {{1, new Answer(4,6,1,-1, textFile[1- fileGroupIndex]) }} }
                };
                */
                var answers1 = new List<Answer>
                {
                    new Answer{QuestionType = PhotoText, QuestionNo = 1, Response = gauche[i]},
                    new Answer{QuestionType = PhotoText, QuestionNo = 2, Response = droite[i]},
                    new Answer{QuestionType = Text, QuestionNo = 3, Response = colorGroup[fileGroupIndex, 0]},
                    new Answer{QuestionType = Text, QuestionNo = 4, Response = colorGroup[fileGroupIndex, 1]},
                    new Answer{QuestionType = Text, QuestionNo = 5, Response = colorFile[fileGroupIndex]},
                    new Answer{QuestionType = Text, QuestionNo = 6, Response = textFile[1- fileGroupIndex]}
                };
                var dico1 = answers1.ToDictionary(a => a.QuestionIdentifier);

                await ExecuteImageMagickCli(baseFileDir, resp, dico1, cliManager);
            }
        }

        /// <summary>
        /// test des commandes avec Offset.
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task CenterOffsetDuotone()
        {
            var output = GetFullOutputDir();
            EnsureDirEmpty(output);
            var baseFileDir = BaseDir + @"imageMagick_CLI\";
            var cliFile = baseFileDir + "duotone_im_cli_center.txt";
            var image = baseFileDir + "center.jpg";
            var colorFile = baseFileDir + "overlay blueberry acai.jpg";
            var textFile = baseFileDir + "overlay texte EN.png";
            EnsureFileExists(image, colorFile, textFile);

            var colorGroup = new[] {"109,93,161","135,200,209"};

            ImageMagickInstallTest();

            var cliManager = new ImageMagickCliManager
            {
                CommandFile = cliFile,
                OutputDir = output,
                DebugKeepTemporaryFiles = false
            };

            Assert.IsTrue(cliManager.Init());

            var resp = new MockRespondent();
            var answers1 = new List<Answer>
            {
                new Answer{QuestionType = PhotoText, QuestionNo = 1, Response = image},
                new Answer{QuestionType = PhotoText, QuestionNo = 2, Response = image},
                new Answer{QuestionType = Text, QuestionNo = 3, Response = colorGroup[0]},
                new Answer{QuestionType = Text, QuestionNo = 4, Response = colorGroup[1]},
                new Answer{QuestionType = Text, QuestionNo = 5, Response = colorFile},
                new Answer{QuestionType = Text, QuestionNo = 6, Response = textFile}
            };
            var dico1 = answers1.ToDictionary(a => a.QuestionIdentifier);

            await ExecuteImageMagickCli(baseFileDir, resp, dico1, cliManager);
        }

        /// <summary>
        /// Test de la commande pour créer des PNG avec les quotations de BMH à partir de textes prédéfinis
        /// </summary>
        [Test]
        public async Task DIA_2019_01_OverlayTextGeneration()
        {
            var output = GetFullOutputDir();
            EnsureDirEmpty(output);
            var baseFileDir = BaseDir + @"imageMagick_CLI\text\";
            var cliFile = baseFileDir + "bmh_text_gen.txt";
            var fontRegular = @"C:\test\TD Graphik-Regular.otf";
            var fontBold = @"C:\test\TD Graphik-Bold.otf";
            var fontMedium = @"C:\test\TD Graphik-Medium.otf";
            var fontSemilight = @"C:\test\TD Graphik-Semilight.otf";

            ImageMagickInstallTest();

            var cliManager = new ImageMagickCliManager
            {
                CommandFile = cliFile,
                OutputDir = output,
                DebugKeepTemporaryFiles = false,
                WorkingDirectory = output,
                UseOutpurIdAsName = false //  Nom aléatoire avec faux.
            };

            Assert.IsTrue(cliManager.Init());

            var resp = new MockRespondent();

            // Common answers
            var common = new List<Answer>
            {
                new Answer {QuestionType = Text, QuestionNo = 1, ChoiceNo = 1, Response = fontRegular}, //(4, 1, 1, -1, fontRegular) },
                new Answer {QuestionType = Text, QuestionNo = 1, ChoiceNo = 2, Response = fontBold}, //(4, 1, 2, -1, fontBold) },
                new Answer {QuestionType = Text, QuestionNo = 1, ChoiceNo = 3, Response = fontMedium}, //(4, 1, 3, -1, fontMedium) },
                new Answer {QuestionType = Text, QuestionNo = 1, ChoiceNo = 4, Response = fontSemilight} //(4, 1, 4, -1, fontSemilight) }
            };

            var itemlist = new List<Dictionary<QuestionIdentifier, Answer>>
            {
                new Dictionary<QuestionIdentifier, Answer>
                {
                    {new QuestionIdentifier(2,1), new Answer{QuestionType = Text, QuestionNo = 2, ChoiceNo = 1, Response = "I've learned that people will forget what you said, people will forget what you did, but people will never forget how you made them feel."}},
                    {new QuestionIdentifier(2,2), new Answer{QuestionType = Text, QuestionNo = 2, ChoiceNo = 2, Response = "Dr. Maya Angelou"}}
                },
                new Dictionary<QuestionIdentifier, Answer>
                {
                    {new QuestionIdentifier(2,1), new Answer{QuestionType = Text, QuestionNo = 2, ChoiceNo = 1, Response = "I had a goal and despite the challenges I did it. Never give up."}},
                    {new QuestionIdentifier(2,2), new Answer{QuestionType = Text, QuestionNo = 2, ChoiceNo = 2, Response = "Henry 'Harry' Winston Jerome"}}
                },
                new Dictionary<QuestionIdentifier, Answer>
                {
                    {new QuestionIdentifier(2,1), new Answer{QuestionType = Text, QuestionNo = 2, ChoiceNo = 1, Response = "A good head and a good heart are always a formidable combination."}},
                    {new QuestionIdentifier(2,2), new Answer{QuestionType = Text, QuestionNo = 2, ChoiceNo = 2, Response = "Nelson Mandela"}}
                },
                new Dictionary<QuestionIdentifier, Answer>
                {
                    {new QuestionIdentifier(2,1), new Answer{QuestionType = Text, QuestionNo = 2, ChoiceNo = 1, Response = "Until all of us have made it, none of us have made it."}},
                    {new QuestionIdentifier(2,2), new Answer{QuestionType = Text, QuestionNo = 2, ChoiceNo = 2, Response = "Rosemary Brown"}}
                },
                new Dictionary<QuestionIdentifier, Answer>
                {
                    {new QuestionIdentifier(2,1), new Answer{QuestionType = Text, QuestionNo = 2, ChoiceNo = 1, Response = "Don’t let life pin you down before you decide to get up."}},
                    {new QuestionIdentifier(2,2), new Answer{QuestionType = Text, QuestionNo = 2, ChoiceNo = 2, Response = "Jully Black"}}
                },
                new Dictionary<QuestionIdentifier, Answer>
                {
                    {new QuestionIdentifier(2,1), new Answer{QuestionType = Text, QuestionNo = 2, ChoiceNo = 1, Response = "Change will not come if we wait for some other person or some other time. We are the ones we’ve been waiting for. We are the change that we seek."}},
                    {new QuestionIdentifier(2,2), new Answer{QuestionType = Text, QuestionNo = 2, ChoiceNo = 2, Response = "Barack Obama"}}
                },
                new Dictionary<QuestionIdentifier, Answer>
                {
                    {new QuestionIdentifier(2,1), new Answer{QuestionType = Text, QuestionNo = 2, ChoiceNo = 1, Response = "After reading the skies, asked the oracles, bathed the baby and doing groceries, I can say, with utter certainty, that this one is IN THE BAG!!!!!"}},
                    {new QuestionIdentifier(2,2), new Answer{QuestionType = Text, QuestionNo = 2, ChoiceNo = 2, Response = "An absurdly long to illustrate an extreme case."}}
                }
            };
            
            foreach (var dic in itemlist)
            {
                // On ajoute les réponse commune au dico actuel avant de lancer le manager.
                foreach (var c in common)
                {
                    dic.Add(c.QuestionIdentifier, c);
                }

                await ExecuteImageMagickCli(baseFileDir, resp, dic, cliManager);
            }
        }

        /// <summary>
        /// Test de la commande pour créer des PNG avec les quotations de BMH à partir de textes prédéfinis
        /// </summary>
        [Test]
        public async Task DIA_2019_01_TransformGeneration()
        {
            var output = GetFullOutputDir();
            EnsureDirEmpty(output);
            var baseFileDir = BaseDir + @"imageMagick_CLI\text\";
            var cliFile = baseFileDir + "bmh_transform.txt";
            var subject = BaseDir + "sujet_00.jpg";
            var overlay = baseFileDir + "overlay.png";

            ImageMagickInstallTest();

            var cliManager = new ImageMagickCliManager
            {
                CommandFile = cliFile,
                OutputDir = output,
                DebugKeepTemporaryFiles = false,
                WorkingDirectory = output
            };

            Assert.IsTrue(cliManager.Init());

            var resp = new MockRespondent();

            var answers1 = new List<Answer>
            {
                new Answer{QuestionNo = 1, Response = subject},//(4, 1, 1, -1, subject)
                new Answer{QuestionNo = 2, Response = overlay},//(4, 2, 1, -1, overlay)
            };

            await ExecuteImageMagickCli(baseFileDir, resp, answers1.ToDictionary(a => a.QuestionIdentifier), cliManager);
        }

        /// <summary>
        /// Test d'une transformation complète (texte custom + image de base) et d'une transformation via overlay avec choix de langue
        /// </summary>
        [Test]
        public async Task DIA_2019_01_FullTest()
        {
            var output = GetFullOutputDir();
            EnsureDirEmpty(output);
            var baseFileDir = BaseDir + @"imageMagick_CLI\text\";
            var cliFileOverlay = baseFileDir + "imgMgk_trans_01.txt";
            var cliFileCustom = baseFileDir + "imgMgk_trans_02.txt";
            var subject = BaseDir + "sujet_00.jpg";

            ImageMagickInstallTest();

            var cliManagerOverlay = new ImageMagickCliManager
            {
                CommandFile = cliFileOverlay,
                OutputDir = output,
                DebugKeepTemporaryFiles = false,
                WorkingDirectory = output
            };

            Assert.IsTrue(cliManagerOverlay.Init());

            var cliManagerCustom = new ImageMagickCliManager
            {
                CommandFile = cliFileCustom,
                OutputDir = output,
                DebugKeepTemporaryFiles = false,
                WorkingDirectory = output
            };

            Assert.IsTrue(cliManagerCustom.Init());

            var resp = new MockRespondent();

            var answers1 = new List<Answer>
            {
                // Image
                new Answer{QuestionNo = 3, Response = subject},
                // Nom
                new Answer{QuestionNo = 4, Response = "Alfred Random" },
                new Answer{QuestionNo = 5, Response = "After reading the skies, asked the oracles, bathed the baby and doing groceries, I can say, with utter certainty, that this one is IN THE BAG!!!!!" },
            };

            var answers2En = new List<Answer>
            {
                // Langue (EN)
                new Answer{QuestionNo = 1, Response = ""},
                // Quote
                new Answer{QuestionNo = 2, Response = "1"},
                // Image
                new Answer{QuestionNo = 3, Response = subject},
            };

            var answers2Fr = new List<Answer>
            {
                // Langue (FR)
                new Answer{QuestionNo = 1, Response = "_fr"},
                // Quote
                new Answer{QuestionNo = 2, Response = "1"},
                // Image
                new Answer{QuestionNo = 3, Response = subject},
            };

            await ExecuteImageMagickCli(baseFileDir, resp, answers1.ToDictionary(c => c.QuestionIdentifier), cliManagerCustom);
            await ExecuteImageMagickCli(baseFileDir, resp, answers2Fr.ToDictionary(c => c.QuestionIdentifier), cliManagerOverlay);
            await ExecuteImageMagickCli(baseFileDir, resp, answers2En.ToDictionary(c => c.QuestionIdentifier), cliManagerOverlay);

            var dirOutput = new DirectoryInfo(output);

            Assert.AreEqual(3, dirOutput.GetFiles().Length); // 3 fichier créés.
        }

        /// <summary>
        /// Tests pour génération de cartes de Hockey pour le projet  TIH-2019-01
        /// </summary>
        [Test]
        public async Task TIH_2019_01_CarteHockey()
        {
            var output = GetFullOutputDir();
            EnsureDirEmpty(output);
            var baseFileDir = BaseDir + @"imageMagick_CLI\text\";
            var script = baseFileDir + "carte_hokey_two_steps.txt";
            var subject = baseFileDir + "subject.jpg";
            var recto = baseFileDir + "recto.png";
            var verso = baseFileDir + "verso.jpg";

            EnsureFileExists(recto);
            EnsureFileExists(verso);

            ImageMagickInstallTest();

            var cli = new ImageMagickCliManager
            {
                CommandFile = script,
                OutputDir = output,
                DebugKeepTemporaryFiles = false,
                WorkingDirectory = output
            };

            Assert.IsTrue(cli.Init());
            
            var resp = new MockRespondent();

            // Longueur exagérée
            var answers1 = new List<Answer>
            {
                new Answer {QuestionNo = 1, Response = subject},
                new Answer {QuestionNo = 2, Response = "Alphonse-Baptiste-Joseph-Dieudonné-Corydon-Achille-Lanthier-Marie-François-Serge"},
                new Answer {QuestionNo = 3, Response = "Lauvergne-Phalangiste-Tremblay"},
                new Answer {QuestionNo = 4, Response = "Les Bûcherons de Saint-Civierge-Sur-Le-Lac"},
                new Answer {QuestionNo = 5, Response = "11"},
            };

            // Longueur normale approximative
            var answers2 = new List<Answer>
            {
                // Image
                new Answer {QuestionNo = 1, Response = subject},
                new Answer {QuestionNo = 2, Response = "Éric"},
                new Answer {QuestionNo = 3, Response = "Ducharme"},
                new Answer {QuestionNo = 4, Response = "Canadiens de Montréal"},
                new Answer {QuestionNo = 5, Response = "08"},
            };

            await ExecuteImageMagickCli(baseFileDir, resp, answers1.ToDictionary(c => c.QuestionIdentifier), cli);
            await ExecuteImageMagickCli(baseFileDir, resp, answers2.ToDictionary(c => c.QuestionIdentifier), cli);

            var dirOutput = new DirectoryInfo(output);

            Assert.AreEqual(4, dirOutput.GetFiles().Length); // 2 fichiers créés par répondant.
        }
    }
}