using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Json;
using NomadLogic.PrintStation.Contest;
using NomadLogic.PrintStation.Json;
using NUnit.Framework;
using Assert = NUnit.Framework.Assert;

namespace NomadLogic.Test.PrintStation.Contest
{
    /// <summary>
    /// Groupe de tests pour les éléments critiques d'un concours.
    /// </summary>
    [TestFixture]
    public class ContestTest : TestBaseNunit
    {
        /// Tests sur la srtucure de période. Tests mathématique pour s'assurer de leur qualité.
        [Test]
        public void TestContestPeriod()
        {
            #region Test constructeur, égalité et opérateur, et overlap avec deux périodes clairement distinctes.

            var period1 = new ContestPeriod
            {
                StartTime = DateTime.Parse("2017-10-01 13:00:00", CultureInfo.InvariantCulture),
                EndTime = DateTime.Parse("2017-10-01 23:00:00", CultureInfo.InvariantCulture)
            };
            var period1Clone = period1.Clone();
            var period2 = new ContestPeriod
            {
                StartTime = DateTime.Parse("2017-10-02 08:00:00", CultureInfo.InvariantCulture),
                EndTime = DateTime.Parse("2017-10-02 15:00:00", CultureInfo.InvariantCulture)
            };
            var period0 = new ContestPeriod
            {
                StartTime = DateTime.Parse("2017-08-01 13:00:00", CultureInfo.InvariantCulture),
                EndTime = DateTime.Parse("2017-08-01 23:00:00", CultureInfo.InvariantCulture)
            };

            var period1Ref = period1;

            Assert.That(period1.IsValid);
            Assert.That(period2.IsValid);
            Assert.That(!period1.Overlap(period2));
            Assert.That(period1 != period2);
            Assert.That(period1 == period1Clone);
            Assert.That(period1 == period1Ref);
            Assert.That(period1.Overlap(period1Clone));
            Assert.AreNotSame(period1, period1Clone);
            Assert.That(period1 < period2);
            Assert.That(period2 > period1);
            Assert.That(period0 < period1);
            Assert.That(period1 > period0);

            // Vérification des noms de fichiers générés.
            Assert.AreEqual(period1.GetFileName(), period1Clone.GetFileName());
            Assert.AreEqual(period1.GetFileName(), period1Ref.GetFileName());

            Assert.AreNotEqual(period1.GetFileName(), period2.GetFileName());
            Assert.AreNotEqual(period1.GetFileName(), period0.GetFileName());

            Console.WriteLine(period1.GetFileName());

            #endregion

            #region Overlap clair, par rapport à la période 1.

            var periodStartOverlap = new ContestPeriod
            {
                StartTime = DateTime.Parse("2017-10-01 13:00:00", CultureInfo.InvariantCulture),
                EndTime = DateTime.Parse("2017-10-01 23:00:00", CultureInfo.InvariantCulture)
            };
            var periodEndOverlap = new ContestPeriod
            {
                StartTime = DateTime.Parse("2017-10-01 22:00:00", CultureInfo.InvariantCulture),
                EndTime = DateTime.Parse("2017-10-01 23:30:00", CultureInfo.InvariantCulture)
            };
            var periodTotallyInside = new ContestPeriod
            {
                StartTime = DateTime.Parse("2017-10-01 22:00:00", CultureInfo.InvariantCulture),
                EndTime = DateTime.Parse("2017-10-01 23:30:00", CultureInfo.InvariantCulture)
            };

            Assert.That(periodStartOverlap.IsValid);
            Assert.That(periodEndOverlap.IsValid);
            Assert.That(periodTotallyInside.IsValid);

            Assert.That(periodStartOverlap.Overlap(period1));
            Assert.That(periodEndOverlap.Overlap(period1));
            Assert.That(periodTotallyInside.Overlap(period1));

            #endregion

            #region Non-Overlap limite, à la seconde près par rapport à la période 1.

            /* Comme la vérification avec Endtime est exclusive, on peut utiliser les même valeurs. */

            var periodBefore1 = new ContestPeriod
            {
                StartTime = period1.StartTime - TimeSpan.FromHours(1),
                EndTime = period1.StartTime
            };
            var periodAfter1 = new ContestPeriod
            {
                StartTime = period1.EndTime,
                EndTime = period1.EndTime + TimeSpan.FromHours(1)
            };

            Assert.That(periodBefore1.IsValid);
            Assert.That(periodAfter1.IsValid);

            Assert.IsFalse(periodBefore1.Overlap(period1));
            Assert.IsFalse(periodAfter1.Overlap(period1));

            #endregion

            #region Tests d'invalidité

            var periodInvalid = new ContestPeriod
            {
                StartTime = DateTime.Parse("2017-10-01 13:00:00", CultureInfo.InvariantCulture),
                EndTime = DateTime.Parse("2017-09-01 23:00:00", CultureInfo.InvariantCulture)
            };

            var notInit = new ContestPeriod();

            Assert.IsFalse(periodInvalid.IsValid());
            Assert.IsFalse(notInit.IsValid());

            #endregion

            #region Span tests

            // zero value timespan
            var now = DateTime.Now;
            var periodInstant = new ContestPeriod
            {
                StartTime = now,
                EndTime = now
            };

            Assert.IsFalse(periodInstant.IsValid());

            var span1 = TimeSpan.FromHours(2);

            periodInstant.EndTime += span1;

            Assert.That(periodInstant.IsValid());
            Assert.AreEqual(periodInstant.Span(), span1);

            var fartherNow = DateTime.Now;

            var spanNow = fartherNow - now;

            var periodVarNow = new ContestPeriod
            {
                StartTime = now,
                EndTime = fartherNow
            };

            Assert.AreEqual(periodVarNow.Span(), spanNow);

            #endregion

            #region Tests pour détecter si la période contient une date.

            var dateAvant = period1.StartTime - TimeSpan.FromMilliseconds(10);
            var dateApres = period1.EndTime + TimeSpan.FromMilliseconds(10);
            var dateDedans = period1.StartTime + TimeSpan.FromMilliseconds(10);

            Assert.IsFalse(period1.InPeriod(dateAvant));
            Assert.IsTrue(period1.InPeriod(dateDedans));
            Assert.IsFalse(period1.InPeriod(dateApres));

            #endregion
        }

        /* Valeurs basées sur le projet STE-2017-01 */
        private List<ContestPeriod> GenerateFixedListContestPeriods()
        {
            // Création d'une liste de période valides (toutes valides et non overlap)
            var liste = new List<ContestPeriod>
            {
                new ContestPeriod
                {
                    StartTime = DateTime.Parse("2017-10-21 9:00:00", CultureInfo.InvariantCulture),
                    EndTime = DateTime.Parse("2017-10-21 17:00:00", CultureInfo.InvariantCulture),
                    Name = "Vancouver, Coquitlam Centre"
                },
                new ContestPeriod
                {
                    StartTime = DateTime.Parse("2017-10-22 9:00:00", CultureInfo.InvariantCulture),
                    EndTime = DateTime.Parse("2017-10-22 17:00:00", CultureInfo.InvariantCulture),
                    Name = "Vancouver, Coquitlam Centre"
                },
                new ContestPeriod
                {
                    StartTime = DateTime.Parse("2017-10-27 9:00:00", CultureInfo.InvariantCulture),
                    EndTime = DateTime.Parse("2017-10-27 17:00:00", CultureInfo.InvariantCulture),
                    Name = "Calgary, Scotiabank Saddledome"
                },
                new ContestPeriod
                {
                    StartTime = DateTime.Parse("2017-10-28 9:00:00", CultureInfo.InvariantCulture),
                    EndTime = DateTime.Parse("2017-10-28 17:00:00", CultureInfo.InvariantCulture),
                    Name = "Calgary, Scotiabank Saddledome"
                },
                new ContestPeriod
                {
                    StartTime = DateTime.Parse("2017-10-29 9:00:00", CultureInfo.InvariantCulture),
                    EndTime = DateTime.Parse("2017-10-29 17:00:00", CultureInfo.InvariantCulture),
                    Name = "Calgary, Scotiabank Saddledome"
                },
                new ContestPeriod
                {
                    StartTime = DateTime.Parse("2017-11-03 12:00:00", CultureInfo.InvariantCulture),
                    EndTime = DateTime.Parse("2017-11-03 18:00:00", CultureInfo.InvariantCulture),
                    Name = "Edmonton, Rogers Place Parking Lot"
                },
                new ContestPeriod
                {
                    StartTime = DateTime.Parse("2017-11-04 10:00:00", CultureInfo.InvariantCulture),
                    EndTime = DateTime.Parse("2017-11-04 16:00:00", CultureInfo.InvariantCulture),
                    Name = "Edmonton, Rogers Place Parking Lot"
                },
                new ContestPeriod
                {
                    StartTime = DateTime.Parse("2017-11-05 10:00:00", CultureInfo.InvariantCulture),
                    EndTime = DateTime.Parse("2017-11-05 14:00:00", CultureInfo.InvariantCulture),
                    Name = "Edmonton, Rogers Place Parking Lot"
                },
                new ContestPeriod
                {
                    StartTime = DateTime.Parse("2017-11-10 15:00:00", CultureInfo.InvariantCulture),
                    EndTime = DateTime.Parse("2017-11-11 00:00:00", CultureInfo.InvariantCulture),
                    Name = "Winnipeg, Bell MTS Centre (inside)"
                },
                new ContestPeriod
                {
                    StartTime = DateTime.Parse("2017-11-11 15:00:00", CultureInfo.InvariantCulture),
                    EndTime = DateTime.Parse("2017-11-12 00:00:00", CultureInfo.InvariantCulture),
                    Name = "Winnipeg, Bell MTS Centre (inside)"
                },
                new ContestPeriod
                {
                    StartTime = DateTime.Parse("2017-11-17 17:00:00", CultureInfo.InvariantCulture),
                    EndTime = DateTime.Parse("2017-11-17 21:00:00", CultureInfo.InvariantCulture),
                    Name = "Montréal, Bell Centre Plaza"
                },
                new ContestPeriod
                {
                    StartTime = DateTime.Parse("2017-11-18 11:00:00", CultureInfo.InvariantCulture),
                    EndTime = DateTime.Parse("2017-11-18 19:00:00", CultureInfo.InvariantCulture),
                    Name = "Montréal, Bell Centre Plaza"
                },
                new ContestPeriod
                {
                    StartTime = DateTime.Parse("2017-11-19 11:00:00", CultureInfo.InvariantCulture),
                    EndTime = DateTime.Parse("2017-11-19 16:00:00", CultureInfo.InvariantCulture),
                    Name = "Montréal, Bell Centre Plaza"
                },
                new ContestPeriod
                {
                    StartTime = DateTime.Parse("2017-12-16 13:00:00", CultureInfo.InvariantCulture),
                    EndTime = DateTime.Parse("2017-12-16 19:00:00", CultureInfo.InvariantCulture),
                    Name = "Ottawa, Aberdeen Pavilion"
                },
                new ContestPeriod
                {
                    StartTime = DateTime.Parse("2017-12-17 10:00:00", CultureInfo.InvariantCulture),
                    EndTime = DateTime.Parse("2017-12-17 14:00:00", CultureInfo.InvariantCulture),
                    Name = "Ottawa, Aberdeen Plaza at Lansdowne Park"
                }
            };

            return liste;
        }

        /* Valeurs basées sur le projet STE-2017-01 */
        private ContestParameters CreateContestParameters()
        {
            var set = new SortedSet<ContestPeriod>();
            foreach (var cp in GenerateFixedListContestPeriods())
            {
                set.Add(cp);
            }

            var contestParam = new ContestParameters
            {
                PeriodSmoothing = ContestParametersPeriodSmoothingEnum.EvenOverTime,
                BaseChance = 0.2,
                BaseChanceElasticity = 0.05,
                RandomElasticity = ContestParametersRandomElasticityEnum.Random,
                WinnerSelectionSystem = ContestParametersWinnerSelectionEnum.MaxWinnerInPeriod,
                WinnerCycleValue = 10,
                Periods = set
            };
            return contestParam;
        }

        /// Ce test ne peut pas échouer. Ne fait qu'écrire des valeurs dans un fichier texte pour voir le formattage.
        [Test]
        public void TestContestPeriodFormatting()
        {
            var baseDir = GetFullOutputDir();

            var outputJsonNoFormat = baseDir + Path.DirectorySeparatorChar + "contestPeriodsNoFormat.txt";
            EnsureFileNotExists(outputJsonNoFormat);
            var outputJsonFormatted = baseDir + Path.DirectorySeparatorChar + "contestPeriodsFormatted.txt";
            EnsureFileNotExists(outputJsonFormatted);

            using (var sw = File.CreateText(outputJsonNoFormat))
            {
                using (var swFormated = File.CreateText(outputJsonFormatted))
                {
                    foreach (var cp in GenerateFixedListContestPeriods())
                    {
                        sw.WriteLine(cp);
                        swFormated.WriteLine(cp.ToStringFormat());
                    }
                }
            }
        }

        [Test]
        public void TestContestPeriodMarshalling()
        {
            var baseDir = GetFullOutputDir();

            var outputJson = baseDir + Path.DirectorySeparatorChar + "contestPeriods.json";
            EnsureFileNotExists(outputJson);

            // Génération d'une liste valide
            var liste = GenerateFixedListContestPeriods();

            var fileInfo = new FileInfo(outputJson);

            JsonUtil.WriteJsonFile(fileInfo, liste);

            fileInfo.Refresh();
            Assert.That(fileInfo.Exists);
            
            var readFromFile = JsonUtil.ReadJsonFile<List<ContestPeriod>>(fileInfo);

            CollectionAssert.AreEqual(liste, readFromFile);
        }

        /// Test l'ordonancement naturel de l'objet
        [Test]
        public void TestOrderingOfPeriods()
        {
            var period1 = new ContestPeriod
            {
                StartTime = DateTime.Parse("2017-10-01 13:00:00", CultureInfo.InvariantCulture),
                EndTime = DateTime.Parse("2017-10-01 23:00:00", CultureInfo.InvariantCulture)
            };
            var period2 = new ContestPeriod
            {
                StartTime = period1.StartTime + TimeSpan.FromDays(1),
                EndTime = period1.EndTime + TimeSpan.FromDays(1)
            };
            var period3 = new ContestPeriod
            {
                StartTime = period1.StartTime + TimeSpan.FromDays(2),
                EndTime = period1.EndTime + TimeSpan.FromDays(2)
            };
            var period4 = new ContestPeriod
            {
                StartTime = period1.StartTime + TimeSpan.FromDays(3),
                EndTime = period1.EndTime + TimeSpan.FromDays(3)
            };

            var compareList = new List<ContestPeriod>{
                period1,
                period2,
                period3,
                period4
            };

            // Insert orderly in sortedset
            var orderly1 = new SortedSet<ContestPeriod>
            {
                period1,
                period2,
                period3,
                period4
            };

            CollectionAssert.IsOrdered(orderly1);
            CollectionAssert.AreEqual(compareList, orderly1);

            // Insert dis-orderly in sortedset
            var orderly2 = new SortedSet<ContestPeriod>
            {
                period4,
                period2,
                period1,
                period3
            };

            CollectionAssert.IsOrdered(orderly2);
            CollectionAssert.AreEqual(compareList, orderly2);
            CollectionAssert.AreEqual(orderly1, orderly2);

            // Test sort methods.
            var unOrderedList = new List < ContestPeriod >{
                period1,
                period3,
                period4,
                period2
            };

            CollectionAssert.AreEquivalent(compareList, unOrderedList);
            CollectionAssert.AreNotEqual(compareList, unOrderedList);

            unOrderedList.Sort();

            CollectionAssert.AreEqual(compareList, unOrderedList);
        }

        [Test]
        public void TestContestMarshallingParameters()
        {
            var baseDir = GetFullOutputDir();

            var outputJson = baseDir + Path.DirectorySeparatorChar + "contestParameters.json";
            EnsureFileNotExists(outputJson);
            var contestParam = CreateContestParameters();

            var fileInfo = new FileInfo(outputJson);

            JsonUtil.WriteJsonFile(fileInfo, contestParam);

            fileInfo.Refresh();
            Assert.That(fileInfo.Exists);

            var readFromFile = JsonUtil.ReadJsonFile<ContestParameters>(fileInfo);

            Assert.AreNotSame(contestParam, readFromFile);
            Assert.AreEqual(contestParam, readFromFile);
        }
        
        /// Assigne un état directement à un ContestUtil, et vérifie si ses calculs sont corrects.
        [Test]
        public void TestContestUtilState()
        {
            var baseDir = GetFullOutputDir();

            var contestParam = CreateContestParameters();

            ContestUtil.ContestDir = baseDir + Path.DirectorySeparatorChar;
            var contest = ContestUtil.GetInstance();
            contest.Parameters = contestParam;
            
            Assert.That(contest.IsValid());

            #region Test de détection de période

            var testPeriod = new ContestPeriod
            {
                StartTime = DateTime.Parse("2017-11-10 15:00:00", CultureInfo.InvariantCulture),
                EndTime = DateTime.Parse("2017-11-11 00:00:00", CultureInfo.InvariantCulture),
                Name = "Winnipeg, Bell MTS Centre (inside)"
            };

            var testDate = testPeriod.StartTime + TimeSpan.FromMinutes(90);

            var currentPeriod = contest.GetCurrentPeriod(testDate);

            Assert.That(currentPeriod == testPeriod);

            #endregion

            #region Test de temps restant à la période

            var leftSpan = currentPeriod.EndTime - testDate;

            var currentTimeLeft = contest.GetCurrentPeriodTimeLeft(testDate);

            Assert.That(currentTimeLeft != null && currentTimeLeft.Value == leftSpan);

            // Désactive la période et vérifie le résultat

            var testDateBefore = testPeriod.StartTime - TimeSpan.FromDays(100);

            Assert.IsNull(contest.GetCurrentPeriod(testDateBefore));
            Assert.IsNull(contest.GetCurrentPeriodTimeLeft(testDateBefore));

            #endregion

            #region Vérif des gagnants et calcul des probabilités

            // Aucun gagnant ne devrait exister car on n'a pas fait d'événement et les données sont vierges.
            var mockUser = new PrintStationUser
            {
                Id = 1,
                EventId = 1,
                FeatureId = 1
            };
            var state = contest.GetCurrentState(mockUser, testDate);

            Assert.IsNotNull(state);
            Assert.That(!state.Winners.Any());
            Assert.AreEqual(testDate, state.LastUpdate);

            #endregion
        }

        /// Teste qu'un concours va sauvegarder correctement les fichiers d'états de périodes
        /// Ne teste pas leur contenu (ce sera un autre test), mais que chaque période est sauvegardée quand on fait un événement dedans,
        /// et qu'elle correctement mise à jour en testant la dernière date de modification.
        [Test]
        public void TestContestUtilSavedFile()
        {
            // Mise ne place
            var baseDir = GetFullOutputDir();

            var contestParam = CreateContestParameters();

            ContestUtil.ContestDir = baseDir + Path.DirectorySeparatorChar;
            var contest = ContestUtil.GetInstance();
            contest.Parameters = contestParam;

            Assert.That(contest.IsValid());

            var mockUser = new PrintStationUser
            {
                Id = 1,
                EventId = 2,
                FeatureId = 3
            };

            // On va provoquer une création d'état sur trois périodes
            // Puis un mise à jour de chaque.

            Assert.That(contest.Parameters.Periods.Count >= 3);

            var testPeriods = contest.Parameters.Periods.Take(3).ToList();

            // On vérifie que les fichiers n'existent pas.
            foreach (var tp in testPeriods)
            {
                var result = ContestUtil.GetStateFileName(mockUser, tp);
                Assert.IsNotNull(result);
                Assert.IsFalse(result.Exists);
            }

            var timeSpan1 = TimeSpan.FromMinutes(10);
            var timeSpan2 = TimeSpan.FromMinutes(5);

            // On provoque maintenant une mise dans chaque, et on teste les fichiers résultants
            foreach (var tp in testPeriods)
            {
                
                var mockDate = tp.EndTime - timeSpan1;

                contest.NewEvent(mockUser, mockDate);

                // Test de fichier d'état existant.
                var result = ContestUtil.GetStateFileName(mockUser, tp);
                Assert.IsNotNull(result);
                Assert.IsTrue(result.Exists);

                // Test de l'état
                var state = contest.GetCurrentState(mockUser, mockDate);
                Assert.AreEqual(state.LastUpdate, mockDate);
                Assert.AreEqual(1, state.TotalEvents);
            }

            // On provoque maintenant une mise dans chaque, et on teste les fichiers résultants
            foreach (var tp in testPeriods)
            {
                var mockDate1 = tp.EndTime - timeSpan1;

                // Test de fichier d'état existant.
                var result = ContestUtil.GetStateFileName(mockUser, tp);
                Assert.IsNotNull(result);
                Assert.IsTrue(result.Exists);

                // Test de l'état avant
                var state1 = contest.GetCurrentState(mockUser, mockDate1);
                Assert.AreEqual(state1.LastUpdate, mockDate1);
                Assert.AreEqual(1, state1.TotalEvents);

                var mockDate2 = tp.EndTime - timeSpan2;

                contest.NewEvent(mockUser, mockDate2);

                // Test de l'état après
                var state2 = contest.GetCurrentState(mockUser, mockDate2);
                Assert.AreEqual(state2.LastUpdate, mockDate2);
                Assert.AreEqual(2, state2.TotalEvents);
            }
        }

        /// <summary>
        /// Test l'algorithme pour une certain nombre de gagnant maximal, étendu également sur une longue période, aléatoire complet.
        /// </summary>
        [TestCase(3)]
        [TestCase(9)]
        [TestCase(16)]
        public void TestContestAlgoritm_MaxWinnerOverTime_EvenOverTime(int eventEveryMinuteX)
        {
            // Mise ne place
            var baseDir = GetFullOutputDir();

            var contestParam = CreateContestParameters();

            ContestUtil.ContestDir = baseDir + Path.DirectorySeparatorChar;
            var contest = ContestUtil.GetInstance();
            contest.Parameters = contestParam;

            Assert.That(contest.IsValid());

            var mockUser = new PrintStationUser
            {
                Id = 1,
                EventId = 2,
                FeatureId = 3
            };

            var file = new FileInfo(baseDir + Path.DirectorySeparatorChar + "contestParams.json");
            if (!file.Exists)
            {
                JsonUtil.WriteJsonFile(file, contestParam);
            }

            // On teste juste sur la première période
            var periodTest = contest.Parameters.Periods.First();
            var span = periodTest.Span();

            var eventTime = periodTest.StartTime + TimeSpan.Zero; // Pour faire une copie

            Console.WriteLine($@"Period lasts for {span.Hours:##} hours, {span.Minutes:##} minutes and {span.Seconds:##} seconds.");
            Console.WriteLine($@"Winner will be rolled every {eventEveryMinuteX}.");
            Console.WriteLine($@"Total winners must be {contest.Parameters.WinnerCycleValue} at the end.");

            var lastHour = span.Hours;
            var timeSliceMin = (int)Math.Floor(span.TotalMinutes / contest.Parameters.WinnerCycleValue);
            var nbrSlices = (int)Math.Floor(span.TotalMinutes / timeSliceMin);
            var averageExpectedWinnerPerHour = 60.0 / timeSliceMin;

            Console.WriteLine();
            Console.WriteLine($@"Each time slice lasts for {timeSliceMin:F} minutes.");
            Console.WriteLine($@"There is a total of {nbrSlices} slices in the time period.");
            Console.WriteLine($@"Each hour, there should be on average {averageExpectedWinnerPerHour} winners per hour.");
            Console.WriteLine();

            while (eventTime < periodTest.EndTime)
            {
                contest.NewEvent(mockUser, eventTime);
                
                // Tests périodiques
                var remaining = periodTest.EndTime - eventTime;
                var elapsed = eventTime - periodTest.StartTime;
                var remainHour = remaining.Hours;
                if (lastHour > remainHour)
                {
                    lastHour = remainHour;
                    Console.WriteLine($@"{remaining.Hours}:{remaining.Minutes} remain. We currently have {contest.GetCurrentState(mockUser, eventTime).Winners.Count} out of {contest.Parameters.WinnerCycleValue}.");
                    Console.WriteLine($@"It should be around {averageExpectedWinnerPerHour * elapsed.TotalHours:F} winners");
                }

                eventTime += TimeSpan.FromMinutes(eventEveryMinuteX);
            }

            var state = contest.GetCurrentState(mockUser, periodTest.StartTime);

            Assert.AreEqual(contest.Parameters.WinnerCycleValue, state.Winners.Count);
        }
    }
}
