using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using NLog;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.DateAndTime;
using NUnit.Framework;
using static NomadLogic.Common.QuestionTypeEnum;
using Assert = NUnit.Framework.Assert;
// ReSharper disable RedundantStringInterpolation

namespace NomadLogic.Test.Common.Util
{
    public class StringUtilTest : TestBaseNunit
    {
        // ReSharper disable once UnusedMember.Local
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// teste la transformation avec jetons d'un nom de répertoire.
        /// </summary>
        [Test]
        public void TestSubDirectoryWithTokens()
        {
            var dicAnswers = new List<Answer>
            {
                new Answer{QuestionNo = 1, ChoiceNo = 1, Response = "0.10"},
                new Answer{QuestionNo = 3, ChoiceNo = 1, Response = "TestValue"},
                new Answer{QuestionNo = 2, ChoiceNo = 2, Response = "0.8"},
                new Answer{QuestionNo = 5, ChoiceNo = 1, Response = "Quest5"},
                new Answer{QuestionNo = 6, ChoiceNo = 1},
                new Answer{QuestionNo = 7, ChoiceNo = 18}
            }.ToDictionary(a => a.QuestionIdentifier);

            var currentQuestion = new QuestionIdentifier(5);
            const string NULL_VALUE = "NA";

            string[,] tests =
            {
                {@"", @""}, // Cas vide
                {@"subdir1", @"subdir1"}, // N'ajoute pas de séparateur à la fin. Devient la responsabilité de l'appellant, si le string final doit être un chemin...
                {@"subdir1/subdir2/", @"subdir1/subdir2/"}, // Aucun jeton
                {@"subdir1/{q3}/subdir2/", @"subdir1/TestValue/subdir2/"}, // Jeton de question
                {@"subdir1/{CURRENT_QUESTION}/{q3}/subdir2/", @"subdir1/Quest5/TestValue/subdir2/"}, // Question actuelle
                {@"subdir1/{DATE:yyyy-MM-dd}/{CURRENT_QUESTION}/{q3}/subdir2/",
                    $@"subdir1/{DateTime.Now.ToString("yyyy-MM-dd", CultureInfo.GetCultureInfo("fr-CA"))}/Quest5/TestValue/subdir2/"}, // Avec date actuelle v1
                {@"subdir1/{DATE:yyyyMMddHHmmss}/{CURRENT_QUESTION}/{q3}/subdir2/",
                    $@"subdir1/{DateTime.Now.ToString("yyyyMMddHHmmss", CultureInfo.GetCultureInfo("fr-CA"))}/Quest5/TestValue/subdir2/"}, // Avec date actuelle v2
            };

            for (var i = 0; i < tests.GetLength(0); i++)
            {
                Console.Out.Write($"Testing index {i}: ");
                var modString = StringUtil.CreateStringFromTokenizedPatternRespondent(tests[i, 0], dicAnswers, currentQuestion, null, null, null, true, NULL_VALUE);
                Console.Out.WriteLine(modString);
                Assert.AreEqual(tests[i, 1], modString);
            }
        }

        /// <summary>
        /// teste la transformation avec jetons d'un nom de répertoire.
        /// </summary>
        [Test]
        public void TestFileNamesWithTokens()
        {
            var dicAnswers = new List<Answer>
            {
                new Answer{QuestionNo = 1, ChoiceNo = 1, Response = "0.10"},
                new Answer{QuestionNo = 3, ChoiceNo = 1, Response = "TestValue"},
                new Answer{QuestionNo = 2, ChoiceNo = 2, Response = "2009-10-02"},
                new Answer{QuestionNo = 5, ChoiceNo = 1, Response = "Quest5"},
                new Answer{QuestionNo = 6, ChoiceNo = 1},
                new Answer{QuestionNo = 7, ChoiceNo = 18, Response = "blah"},
                new Answer{QuestionNo = 8, ChoiceNo = 1, QuestionType = Check},
            }.ToDictionary(a => a.QuestionIdentifier);

            var respondent = new RespondentImpl("en", "", 122)
            {
                Date = DateUtil.GetJavaTimeStringFromNow()
            };

            var currentQuestionValue = new QuestionIdentifier(5);
            const string NULL_VALUE = "NA";

            string[,] tests =
            {
                {@"", @""}, // Cas vide
                {@"filename.txt", @"filename.txt"}, // Aucun jeton
                {@"{Q3}_filename.txt", @"TestValue_filename.txt"}, // Jeton de question
                {@"{CURRENT_QUESTION}_{Q3}_filename.txt", @"Quest5_TestValue_filename.txt"}, // Question actuelle
                {@"{CURRENT_QUESTION}_{q2c2:date:MMddyyyy}_{Q3}_filename.txt",@"Quest5_10022009_TestValue_filename.txt"}, // Avec date dans question
                {@"{CURRENT_QUESTION}_{DATE:MMddyyyy}_{Q3}_filename.txt",$@"Quest5_{DateTime.Now.ToString("MMddyyyy", CultureInfo.GetCultureInfo("fr-CA"))}_TestValue_filename.txt"}, // Avec date actuelle v2
                {@"{RESPONDENT:EventId}_{RESPONDENT:Date:timespan:yyyyMMdd}_filename.txt",$@"122_{DateUtil.FromJavaTimeToDateTime(respondent.Date).ToString("yyyyMMdd", CultureInfo.GetCultureInfo("fr-CA"))}_filename.txt"}, // Date venant du timestamp du répondant.
                {@"{Q7C18}_filename.txt", @"blah_filename.txt"}, // Jeton de question
                {@"{q8:bool}-{q9:bool}.mp4", @"true-false.mp4"}, // booléen. Question 8 répondue, Question neuf pas répondue.
                {@"{Q7c18:bool}_filename.txt", @"true_filename.txt"}, // Jeton de question booléen du choix choisi (vrai)
                {@"{Q7c17:bool}_filename.txt", @"false_filename.txt"}, // Jeton de question booléen du choix choisi (faux)
            };

            for (var i = 0; i < tests.GetLength(0); i++)
            {
                Console.Out.Write($"Testing index {i}: ");
                var modString = StringUtil.CreateStringFromTokenizedPatternRespondent(tests[i, 0], dicAnswers, currentQuestionValue, respondent, null, null, true, NULL_VALUE);
                Console.Out.WriteLine(modString);
                Assert.AreEqual(tests[i, 1], modString);
            }

            // Test avec valeur aléatoire (affichage console, car je ne peux pas tester la valeur finale.
            Console.Out.WriteLine(StringUtil.CreateStringFromTokenizedPatternRespondent("{RANDOM}_t.mp4", dicAnswers, currentQuestionValue, respondent));
        }

        /// Teste la création d'un répertoire et d'un nom de fichier complexe avec les mêmes données (répondant + réponses)
        /// Utilisé dans le projet LOY-2018-01, où le chemin ET le nom de fichier sont critiques.
        /// Permet aussi de tester sans "current amswer";
        [Test]
        public void TestCompletePathWithTokens()
        {
            var dicAnswers = new List<Answer>
            {
                new Answer {QuestionNo = 1, ChoiceNo = 1, Response = "Nom Composé"},
                new Answer {QuestionNo = 2, ChoiceNo = 1, Response = "Prénom"},
                new Answer {QuestionNo = 3, ChoiceNo = 2, QuestionType = Combo, Response = "test"}, // Choix de question multiple répondue
                new Answer {QuestionNo = 4, ChoiceNo = 1, Response = "Department"},
                new Answer {QuestionNo = 5, ChoiceNo = 1, QuestionType = Check} // Type Check
            }.ToDictionary(a => a.QuestionIdentifier);

            var respondent = new RespondentImpl("en", "", 122)
            {
                Date = DateUtil.GetJavaTimeStringFromNow()
            };

            string[,] tests =
            {
                {@"{q3c2}/{DATE:yyyy_MM_dd}/", $@"test/{DateTime.Now.ToString("yyyy_MM_dd", CultureInfo.GetCultureInfo("fr-CA"))}/"}, // Chemin
                {@"{q4}-{q1}-{q2}.mp4", @"Department-Nom Composé-Prénom.mp4"}, // Fichier
                {@"{q5:bool}-{q6:bool}.mp4", @"true-false.mp4"}, // booléen
            };

            var modStringDir = StringUtil.CreateStringFromTokenizedPatternRespondent(tests[0, 0], dicAnswers, null, respondent);
            Assert.AreEqual(tests[0, 1], modStringDir);
            var modStringFile = StringUtil.CreateStringFromTokenizedPatternRespondent(tests[1, 0], dicAnswers, null, respondent);
            Assert.AreEqual(tests[1, 1], modStringFile);
            var modBool = StringUtil.CreateStringFromTokenizedPatternRespondent(tests[2, 0], dicAnswers, null, respondent);
            Assert.AreEqual(tests[2, 1], modBool);

            const string RAND_TEST = "{q4}-{q1}-{q2}-{RANDOM}.mp4";
            var randTestResult = StringUtil.CreateStringFromTokenizedPatternRespondent(RAND_TEST, dicAnswers, null, respondent);
            // Test avec valeur aléatoire (affichage console, car je ne peux pas tester la valeur finale.
            Assert.AreNotEqual(RAND_TEST, randTestResult);

            // Test de création de répertoire et fichier.
            var originalImageFile = new FileInfo(BaseDir + "swing_green.mp4");
            Assert.IsTrue(originalImageFile.Exists);
            var outputDir = GetFullOutputDir();

            // Vide et recréer.
            FileUtil.DeleteFolderContent(outputDir);
            EnsureDirExists(outputDir);

            // Répertoire
            var finalDir = new DirectoryInfo(outputDir + modStringDir);
            Assert.That(!finalDir.Exists);
            finalDir.Create();
            finalDir.Refresh();
            Assert.That(finalDir.Exists);

            // Fichier
            var finalFile = new FileInfo(finalDir + randTestResult);
            Assert.That(!finalFile.Exists);
            File.Copy(originalImageFile.FullName, finalFile.FullName);
            finalFile.Refresh();
            Assert.AreNotEqual(originalImageFile, finalFile.FullName);
            Assert.That(finalFile.Exists);
            Assert.AreEqual(originalImageFile.Length, finalFile.Length);

            Console.WriteLine(finalFile.FullName);
        }

        [TestCase("CEKRJATGWS", "2473712641", "9371DC01", true)]
        [TestCase("FJESAPYQTW", "2476516609", "939CA501", true)]
        [TestCase("BADJCSRHKW", "2473542705", "936F4431", true)]
        [TestCase("TGBHLZCRWQ", "2474121649", "937819B1", true)]
        [TestCase("JSFQRBDACY", "2473908657", "9374D9B1", true)]
        [TestCase("JMGHBQTKPE", "2476315041", "939991A1", true)]
        [TestCase("ZGPSQLWFAT", "2476250081", "939893E1", true)]
        [TestCase("RKZFPTBJAW", "2473905617", "9374CDD1", true)]
        [TestCase("ECWJAQZHMS", "2473541393", "936F3F11", true)]
        [TestCase("SETDBJMCLA", "2474210673", "93797571", true)]
        [TestCase("EYBKFRHAMP", "2473890513", "937492D1", true)]
        [TestCase("ZMKQAPFYWH", "2475892401", "93931EB1", true)]
        [TestCase("BMPFQZSTHC", "1112743577", "42531E99", true)]
        [TestCase("JQEBYFPTZG", "2473735889", "937236D1", true)]
        [TestCase("SLDBCJFWMG", "2471643409", "93524911", true)]
        [TestCase("PEYWMFZJGH", "2473885329", "93747E91", true)]
        [TestCase("WFSLTZRPAH", "2473940177", "937554D1", true)]
        [TestCase("WRTAGMYZPH", "2473542625", "936F43E1", true)]
        [TestCase("AYWFEGTMRH", "2476316385", "939996E1", true)]
        [TestCase("TGYPFDZQKJ", "2476159089", "93973071", true)]
        [TestCase("RAMYETZFBJ", "2471758481", "93540A91", true)]
        [TestCase("AGYEZPRDWM", "2476252369", "93989CD1", true)]
        [TestCase("BAMYDHJLGE", "1110430889", "422FD4A9", true)]
        [TestCase("WZMTADRQHY", "2475858641", "93929AD1", true)]
        [TestCase("WMZPCTQLGF", "2476118033", "93969011", true)]
        [TestCase("TPBDZQEAYK", "2470960225", "9347DC61", true)]
        [TestCase("AJDTSHBWMP", "2476158961", "93972FF1", true)]
        [TestCase("GPCKQRSZJW", "2474191489", "93792A81", true)]
        [TestCase("SWPFZEYRDM", "2460257633", "92A48D61", true)]
        [TestCase("WRQBPASKCT", "2476158369", "93972DA1", true)]
        [TestCase("TBPWQFKJYR", "2474106065", "9377DCD1", true)]
        [TestCase("???", "1112430121", "424E5629", true)]
        [TestCase("???", "0106489243", "9BE55806", false)]
        public void TestReverseHexString(string testId, string inStr, string checkStr, bool sameByteOrder)
        {
            // Hex tests based on following : https://www.binaryhexconverter.com/decimal-to-hex-converter
            var calc = StringUtil.ReverseOrderHexString(inStr, sameByteOrder);
            var success = calc.Equals(checkStr, StringComparison.CurrentCultureIgnoreCase);
            var result = !success ? "ERROR" : "PASS";
            Console.Out.WriteLine($"{result} -> vID={testId}, NumVal={inStr}, ExpectedHex={checkStr}{(success ? "" : ", WrongHexVal=" + calc)}");
            Assert.IsTrue(success);
        }


    }
}