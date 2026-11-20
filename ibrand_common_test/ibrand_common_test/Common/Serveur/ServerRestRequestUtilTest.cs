using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using NomadLogic.Common.Serveur;
using NomadLogic.Common.Serveur.Requests;
using NomadLogic.Common.Serveur.Structures;
using NomadLogic.Common.Services;
using NomadLogic.Common.Services.Configuration;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Json;
using NomadLogic.Test.Common.Respondent;
using NUnit.Framework;
using static NomadLogic.Common.Serveur.Requests.ServerRestRequestResultType;

namespace NomadLogic.Test.Common.Serveur
{
    public class ServerRestRequestUtilTest : ServerRestRequestUtilTestBase
    {
        protected static RequestSyncRespondent CreateSimpleRespondent(MockQuestionnaire mquest)
        {
            var rand = new Random((int)DateTime.Now.Ticks);

            var now = DateTime.Now;

            var res = new RequestSyncRespondent
            {
                CreationDate = now - TimeSpan.FromMinutes(rand.Next(20000)), 
                Language = rand.Next(0, 1) == 0 ? "fr" : "en", 
                RespondentId = mquest.GetNextRespondentId(), 
                Answers = mquest.GenerateMockAnswers()
            };
            
            return res;
        }

        protected static async Task<ServerRestRequestResult<RequestSyncResponse>> SyncStep1(ServerConfiguration conf, string authToken, RequestSyncRespondent resp, TerminalInfo tdi, ServerRestRequestResultType expectedResult, bool useMock = true)
        {
            var res = await ServerRestRequestsUtil.Sync(CancellationToken.None, conf, authToken, resp, tdi, useMock);

            // Doit détecter que des fichiers sont à envoyer.
            Assert.IsNotNull(res);
            Assert.AreEqual(expectedResult, res.Result, res.MessageUi);
            
            return res;
        }

        protected static Task SyncStep2(IList<RequestSyncFileUpload> fileList, ServerConfiguration conf, string authToken, string sessionId, ServerRestRequestResultType? expectedResult = Success)
        {
            Assert.IsNotNull(fileList);
            Assert.IsNotNull(authToken);
            Assert.IsNotNull(sessionId);

            foreach (var f in fileList)
            {
                var fi = new FileInfo(f.LocalFilePath);
                Assert.That(fi.Exists);
                Assert.That(fi.Length > 0);

                var up = ServerRestRequestsUtil.SyncUpload(conf, authToken, sessionId, f.LocalFilePath, f.AnswerIdentifier);

                Assert.IsNotNull(up);
                Assert.AreEqual(expectedResult ?? Success, up.Result);
            }

            return Task.CompletedTask;
        }

        protected static Task<ServerRestRequestResult<RequestSyncResponse>> SyncStep3(ServerConfiguration conf, string authToken, string sessionId, ServerRestRequestResultType expectedResult)
        {
            var final = ServerRestRequestsUtil.SyncFinalize(conf, authToken, sessionId);

            Assert.IsNotNull(final);
            Assert.AreEqual(expectedResult, final.Result, "Wrong state.");
            return Task.FromResult(final);
        }

        [Test]
        public async Task SyncSimpleRespondentTest()
        {
            var conf = ServicesContainer.GetService<NomadConfiguration>().CurrentServerConfiguration;

            var authToken = await GetAuthorizationToken(conf);
            
            var mquest = new MockQuestionnaire()
                .SetNumberOfQuestions(5)
                .SetNumberOfQuestionsAsFile() // No-Op
                .SetNumberOfQuestionsAsOptional(2);

            var resp = CreateSimpleRespondent(mquest);

            var tdi = mquest.GetTerminalInfo();

            var res = await SyncStep1(conf, authToken, resp, tdi, Success);

            Assert.IsNotNull(res.Data.RespondentCreationDateStr);
            Assert.IsTrue(res.Data.RespondentServerId != 0);
        }

        private ServerRestRequestResult<RequestSyncResponse> PrepareFileSync(out ServerConfiguration conf, out string authToken)
        {
            var fileTest01 = BaseDir + "001.jpg";
            var fileTest02 = BaseDir + "002.jpg";
            var fileTest03 = BaseDir + "003.jpg";

            EnsureFileExists(fileTest01, fileTest02, fileTest03);

            conf = ServicesContainer.GetService<NomadConfiguration>().CurrentServerConfiguration;

            authToken = GetAuthorizationToken(conf).GetAwaiter().GetResult();

            var mquest = new MockQuestionnaire()
                .SetNumberOfQuestions(6)
                .SetNumberOfQuestionsAsFile(fileTest01, fileTest02, fileTest03)
                .SetNumberOfQuestionsAsOptional(0); // No-Op

            var resp = CreateSimpleRespondent(mquest);

            var tdi = mquest.GetTerminalInfo();

            var res = SyncStep1(conf, authToken, resp, tdi, SuccessPartial).GetAwaiter().GetResult();
            Assert.AreEqual(DateTime.MinValue, DateTime.Parse(res.Data.RespondentCreationDateStr));
            Assert.AreEqual(0, res.Data.RespondentServerId);

            return res;
        }

        /// <summary>
        /// Test complet pour initialiser une session, envoyer les fichiers et finaliser la session.
        /// Assume que toutes les opérations se passent sans problèmes.
        /// </summary>
        [Test]
        public async Task SyncMultimediaRespondentTest()
        {
            var res = PrepareFileSync(out var conf, out var authToken);

            var sessionId = res.Data.SyncSessionId;
            var files = res.Data.FilesToUpload;
            
            await SyncStep2(files, conf, authToken, sessionId);

            // Maintenant on finalize
            var final = await SyncStep3(conf, authToken, sessionId, Success);

            Assert.AreNotEqual(DateTime.MinValue, DateTime.Parse(final.Data.RespondentCreationDateStr));
            Assert.AreNotEqual(0, final.Data.RespondentServerId);
        }

        /// <summary>
        /// Test ce qui doit arriver quand on tente d'envoyer des fichiers avec un mauvais numéro de session.
        /// Ils doivent être refusés.
        /// </summary>
        [Test]
        public async Task SyncMultimediaRespondentBadSessionUploadTest()
        {
            var res = PrepareFileSync(out var conf, out var authToken);

            const string sessionId = "absolumentIllegalSessionId";
            var files = res.Data.FilesToUpload;
            
            await SyncStep2(files, conf, authToken, sessionId, Error); // On S'attend à une erreur ici

            // Comme on ne peut pas continuer, on cancelle.
            var canc = ServerRestRequestsUtil.SyncCancel(conf, authToken, sessionId);
            Assert.IsNotNull(canc);
            Assert.AreEqual(Unauthorized, canc.Result);
        }

        private async Task<ServerRestRequestResult<RequestSyncResponse>> CheckFinalizationStatus(ServerConfiguration conf, string authToken, string sessionId, [CanBeNull] IList<RequestSyncFileUpload> filesToCompare, ServerRestRequestResultType expected)
        { 
            var final = await SyncStep3(conf, authToken, sessionId, expected);

            if (expected == Success)
            {
                Assert.AreNotEqual(DateTime.MinValue, DateTime.Parse(final.Data.RespondentCreationDateStr)); // Il doit y avoir une valeur d'entrée.
                Assert.AreNotEqual(0, final.Data.RespondentServerId); // Le répondant doit être finalisé
            }
            else if (expected == SuccessPartial)
            {
                Assert.AreEqual(sessionId, final.Data.SyncSessionId); // La session doit bien sûr être la même
                Assert.Greater(final.Data.FilesToUpload.Count, 0); // Il doit rester au moins un fichier à envoyer.
            }

            // Exactement la même liste, si donnée
            if (filesToCompare != null && filesToCompare.Any())
            {
                CollectionAssert.AreEquivalent(filesToCompare, final.Data.FilesToUpload);
            }

            return final;
        }

        /// <summary>
        /// Ce test vérifie que la finalization ne peut se faire s'il reste des fichiers à envoyer.
        /// Ce test valide que la liste restante diminue au fur et à mesure que les fichiers sont envoyés.
        /// On se rend jusqu'à la finalization complète.
        /// </summary>
        [Test]
        public async Task SyncFinalizeStepByStepToSuccessTest()
        {
            var res = PrepareFileSync(out var conf, out var authToken);

            // Il y aurait normalement des fichiers à synchroniser ici.
            var sessionId = res.Data.SyncSessionId;
            var files = res.Data.FilesToUpload;
            Assert.AreEqual(3, files.Count);

            // Finalisation incomplète
            var final = await CheckFinalizationStatus(conf, authToken, sessionId, files, SuccessPartial);

            // On envoi les fichiers au compte goutte, en ordre inverse.
            var listToSend = final.Data.FilesToUpload.Reverse().ToList();

            while (listToSend.Any())
            {
                var elemToSend = listToSend.First();
                Assert.IsNotNull(elemToSend);
                listToSend.RemoveAt(0);

                var partialList = new List<RequestSyncFileUpload>
                {
                    elemToSend
                };

                await SyncStep2(partialList, conf, authToken, sessionId, Success);

                var expected = listToSend.Count == 0 ? Success : SuccessPartial;
                await CheckFinalizationStatus(conf, authToken, sessionId, listToSend, expected);
            }
        }

        /// <summary>
        /// Ce test vérifie que la finalization ne peut se faire s'il reste des fichiers à envoyer.
        /// Ce test valide que la liste restante diminue au fur et à mesure que les fichiers sont envoyés.
        /// On se rend jusqu'au dernier fichier, puis on cancelle l'opération.
        /// </summary>
        [Test]
        public async Task SyncFinalizeStepByStepToCancelTest()
        {
            var res = PrepareFileSync(out var conf, out var authToken);

            // Il y aurait normalement des fichiers à synchroniser ici.
            var sessionId = res.Data.SyncSessionId;
            var files = res.Data.FilesToUpload;
            Assert.AreEqual(3, files.Count);

            // Finalisation incomplète
            var final = await CheckFinalizationStatus(conf, authToken, sessionId, files, SuccessPartial);

            // On envoi les fichiers au compte goutte, en ordre inverse.
            var listToSend = final.Data.FilesToUpload.Reverse().ToList();

            while (listToSend.Count > 1)
            {
                var elemToSend = listToSend.First();
                Assert.IsNotNull(elemToSend);
                listToSend.RemoveAt(0);

                var partialList = new List<RequestSyncFileUpload>
                {
                    elemToSend
                };

                await SyncStep2(partialList, conf, authToken, sessionId, Success);

                await CheckFinalizationStatus(conf, authToken, sessionId, listToSend, SuccessPartial);
            }

            // On annule la requête au dernier fichier.
            var canc = ServerRestRequestsUtil.SyncCancel(conf, authToken, sessionId);

            Assert.IsNotNull(canc);
            Assert.AreEqual(Success, canc.Result);
        }

        [Test]
        public void TestDirectoryContentAsDownloadRessources()
        {
            var baseDir = new DirectoryInfo(BaseDir + "ftp_test");
            Assert.IsTrue(baseDir.Exists);

            var sepChar = Path.DirectorySeparatorChar;

            // Avec écrasement des noms de fichier.
            var results = ServerRestRequestsUtil.DirectoryContentAsDownloadRessources(CancellationToken.None, baseDir);

            Console.Out.WriteLine("\r\nList of files, flattened:\r\n");
            foreach (var res in results)
            {
                Console.Out.WriteLine(res);
            }

            // Aucun ne doit avoir la mention d'aucun répertoire, absolu ou relatif.
            Assert.That(results.All(f => !f.Src.Contains(sepChar)));

            // Sans écrasement des sous-répertoires.
            results = ServerRestRequestsUtil.DirectoryContentAsDownloadRessources(CancellationToken.None, baseDir, false);

            Console.Out.WriteLine("\r\nList of files, with subs:\r\n");
            foreach (var res in results)
            {
                Console.Out.WriteLine(res);
            }

            Assert.That(results.All(f => !f.Src.StartsWith(sepChar.ToString())));
            Assert.That(results.Any(f => f.Src.Contains("pictures" + sepChar)));
            Assert.That(results.Any(f => f.Src.Contains("videos" + sepChar)));
        }

        /// <summary>
        /// Ce test tente d'envoyer un répondant, pas un mock, vers un projet inexistant.
        /// </summary>
        [Test]
        public async Task SyncBadTerminalTest()
        {
            var conf = ServicesContainer.GetService<NomadConfiguration>().CurrentServerConfiguration;

            var authToken = await GetAuthorizationToken(conf);

            var mquest = new MockQuestionnaire()
                .SetNumberOfQuestions(5)
                .SetNumberOfQuestionsAsFile() // No-Op
                .SetNumberOfQuestionsAsOptional(2);

            var resp = CreateSimpleRespondent(mquest);

            var tdi = mquest.GetTerminalInfo();

            await SyncStep1(conf, authToken, resp, tdi, Failed, false); // Pas mock
        }

        [TestCase(ServerConfigurationSwitch.Preprod)]
        public async Task GetQuestionnaireDataTestMock(ServerConfigurationSwitch serverConfigKey)
        {
            var conf = ServicesContainer.GetService<NomadConfiguration>().ServerInformations[serverConfigKey];

            var authToken = await GetAuthorizationToken(conf);

            var payload = new RequestQuestionnaireDataPayload();

            var res = await ServerRestRequestsUtil.GetQuestionnaireData(CancellationToken.None, conf, authToken, payload, true);

            Assert.IsNotNull(res);
            Assert.AreEqual(Success, res.Result);

            // N'est pas une requête vide en mock
            Assert.Greater(res.Data.AnswersNumber, 0);
            Assert.Greater(res.Data.Elements.Count, 0);
            // Date du dernier élément égale
            //Assert.AreEqual(res.Data.LastElementDate, res.Data.Elements.Last().CreationDate); // Peut être différent d'un centième. Pas répétable.
            // Même nombre de réponses total au nombre d'éléments envoyés.
            Assert.AreEqual(res.Data.Elements.Sum(e => e.Answers.Count), res.Data.AnswersNumber);
        }

        [TestCase(ServerConfigurationSwitch.DevAWS, Success, "test10", 229, 294)]
        [TestCase(ServerConfigurationSwitch.DevAWS, Success, "test", 229)]
        [TestCase(ServerConfigurationSwitch.DevAWS, NoContent, "test", 229, 295)] // Terminal n'a pas envoyé de répondants.
        public async Task GetRespondentDataTest(ServerConfigurationSwitch serverConfigKey, ServerRestRequestResultType expected, string identifier, int qId, int? tId = null)
        {
            var conf = ServicesContainer.GetService<NomadConfiguration>().ServerInformations[serverConfigKey];

            var authToken = await GetAuthorizationToken(conf);

            var payload = new RequestGetRespondentDataPayload
            {
                RespondentIdentification = new RespondentIdentificationStrategy
                {
                    Strategy = new RespondentIdentificationStrategyDefinition
                    {
                        StrategyType = RequestRespondentDataIdentificationStrategyEnum.ExternalIdentifier,
                        ExternalUniqueIdentifier = identifier,
                        QuestionnaireId = qId
                    }
                },
                QuestionDescriptionsFilter = new List<string>
                {
                    "q1", "q2"
                },
                DynamicValuesFilter = new RespondentDataDynamicValueRequestFilter
                {
                    MinimalDate = DateTimeOffset.MinValue,
                    DynamicKeysList = new List<string>
                    {
                        "roaming", "ride"
                    }
                }
            };
            if (tId.HasValue)
            {
                payload.RespondentIdentification.Strategy.TerminalId = tId.Value;
            }

            var res = await ServerRestRequestsUtil.GetRespondentData(CancellationToken.None, conf, authToken, payload);

            Assert.IsNotNull(res);
            Assert.AreEqual(expected, res.Result);
            
            await Console.Out.WriteLineAsync(res.Data?.ToStringPublicProperties());
        }

        /// <summary>
        /// Teste que un terminal inexistant retourne un code spécifique qu'il n'y a rien, mais que la requête est valide (204).
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task GetEmptyTerminalInfo()
        {
            var conf = ServicesContainer.GetService<NomadConfiguration>().CurrentServerConfiguration;

            var authToken = await GetAuthorizationToken(conf);

            var res = await ServerRestRequestsUtil.DownloadProjectInfo(authToken,CancellationToken.None, conf, "nada", "nothing");

            Assert.IsNotNull(res);
            Assert.AreEqual(NoContent, res.Result);
            Assert.IsNull(res.Data);
        }

        /// <summary>
        /// Ce test agit sur le end-point "debug-sync", dont le travail est de renvoyer le contenu JSON de répondant comme tel comme réponse.
        /// Comme il s'agit d'un "dé-marshal/ré-marshal", le contenu reçu peut être différent, mais fonctionnelement similaire.
        /// </summary>
        [TestCase("test")]
        [TestCase("complicated:This9476783bhdfgIYGBkk")]
        [TestCase("accent:ëéàûÛç")]
        [TestCase("symbols://--_+-*@£¢¤%&*!#|~<>^;:{}^()[]`")]
        [TestCase("long:jklasdhfuiwebskjfhkusahdiofhkjnf;asjfiuasioydfisfjkbasdkjfhkasdfuytysdfgshfsd78f678435jhjh3grsvbjahbmhbhqwewpoeiwpowipwirpocbzshgfdbasjdfjksdfh7863443h543hjg786fgdfu74y3643gr4jhrejhwgfjdg786f56w56545r7436ghde")]
        //Cas Spécifiques
        [TestCase("1600721952-Hfjfjfj@gjjjiffr.ghj-62241473686__BBF97032-7CE0-4822-8D36-E21B96A5366F.MOV")] // projet ESH-2020-04
        public async Task SyncDebugTest(string valueToTest)
        {
            var conf = ServicesContainer.GetService<NomadConfiguration>().CurrentServerConfiguration;

            var authToken = await GetAuthorizationToken(conf);

            var mquest = new MockQuestionnaire()
                .SetNumberOfQuestions(5)
                .SetNumberOfQuestionsAsFile() // No-Op
                .SetNumberOfQuestionsAsOptional(2)
                .SetSpecificAnswer(valueToTest);

            var resp = CreateSimpleRespondent(mquest);

            var tdi = mquest.GetTerminalInfo();

            await Console.Out.WriteLineAsync("Original construct:");
            await Console.Out.WriteLineAsync(resp.ToStringPublicProperties());

            var res = ServerRestRequestsUtil.DebugSync(conf, authToken, resp, tdi);

            Assert.IsNotNull(res.Data);
            await Console.Out.WriteLineAsync("After round-trip to server:");
            await Console.Out.WriteLineAsync(resp.ToStringPublicProperties());

            // res et resp ne sont pas nécessairement égaux, mais devraient avoir des tonnes de similités, comme par exemple dans les réponses.
            CollectionAssert.AreEquivalent(resp.Answers, res.Data.Respondent.Answers);
        }

        /// <summary>
        /// Cette méthode unitaire envoi des valeurs dans un répondant dans un questionnaire, incluant valeurs dynamique, réponse de questionnaire et fichiers,
        /// puis va tenter de chercher ces valeurs et fichiers avec des requêtes subséquentes.
        /// </summary>
        [TestCase("12121", 222, 286, ServerConfigurationSwitch.Preprod, false)]
        [TestCase("noexist", 227, 291, ServerConfigurationSwitch.DevAWS, true)]
        [TestCase("noexist", 222, 286, ServerConfigurationSwitch.Preprod, true)]
        [TestCase("noexist", 227, 291, ServerConfigurationSwitch.Local, true)]
        [TestCase("unitTest", 229, 294, ServerConfigurationSwitch.DevAWS, true)]
        public async Task CompleteRespondentDataLoop(string respUei, int qId, int tId, ServerConfigurationSwitch serverSwitch, bool randomizeUei)
        {
            var outDir = GetFullOutputDir();
            var contentGet = outDir + "get_request.json";
            EnsureDirEmpty(outDir);
            
            var nomadConfig = ServicesContainer.GetService<NomadConfiguration>();
            
            var conf = nomadConfig.ServerInformations[serverSwitch];
            Assert.IsNotNull(conf);

            var dateCreated = DateTime.Now;
            var dateMinSearch = dateCreated.AddSeconds(-1);
            var dateMaxSearch = dateCreated.AddSeconds(1);
            Assert.IsTrue(dateMinSearch < dateMaxSearch);

            // #1 Envoyer les information sur ce répondant.
            
            var baseDir = BaseDir;
            var dynamicFileValues = new string[8];
            var dicoFiles = new Dictionary<string, FileInfo>();
            var originalFiles = new List<FileInfo>(); // Pour vérification finale

            for (var i = 1; i <= 8; i++)
            {
                var index = i - 1;
                var filePath = new FileInfo(baseDir + $"image_0{i}.jpg");
                var fileTag = $"file{i}";
                dicoFiles.Add(fileTag, filePath);
                originalFiles.Add(filePath);
                dynamicFileValues[index] = RequestSendRespondentDataConfig.FILE_REFERENCE_PREFIX + fileTag;
                Assert.IsTrue(filePath.Exists);
            }

            var valueList = new List<string>
            {
                "4", "vimeoTest"
            };
            valueList.AddRange(dynamicFileValues);

            var authToken = await GetAuthorizationToken(conf);
            
            var dataKeyUnique = StringUtil.GenerateGuidString();

            var dynamicValues = new List<RequestSendRespondentDataDynamicValue>
            {
                new RequestSendRespondentDataDynamicValue
                {
                    DateCreated = dateCreated,
                    DataKey = dataKeyUnique,
                    DataValues = valueList
                }
            };

            var uei = respUei;
            if (randomizeUei)
            {
                uei += $"_{GenerateUniqueId.GetId()}";
            }

            var sendParameters = new RequestSendRespondentDataPayload
            {
                RequestConfig = new RequestSendRespondentDataConfig
                {
                    SendRespondentIdentification = new RequestSendRespondentDataIdentification
                    {
                        SearchStrategy = new RequestSendRespondentDataIdentificationStrategy
                        {
                            SearchStrategy = RequestRespondentDataIdentificationStrategyEnum.ExternalIdentifier,
                            TextValue = uei,
                            ServerIdQuestionnaire = qId,
                            ServerIdTerminal = tId
                        }
                    }
                    ,AnswersToSend = new List<RequestSendRespondentDataAnswer>
                    {
                        new RequestSendRespondentDataAnswer
                        {
                            QuestionNumber = 1,
                            ChoiceNumber = 1,
                            DontOverrideData = true,
                            TextValue = uei
                        }
                    }
                    ,DynamicValuesToSend = dynamicValues
                },
                RequestFiles = dicoFiles
            };
            var sendResult = ServerRestRequestsUtil.SendRespondentData(CancellationToken.None, conf, authToken, sendParameters)?.Data;
            Assert.IsNotNull(sendResult);
            Assert.AreEqual(uei, sendResult.TargetRespondent.UniqueExternalIdentifier);

            // #2 Aller chercher les informations de ce répondant.
            var getParameters = new RequestGetRespondentDataPayload
            {
                RespondentIdentification = new RespondentIdentificationStrategy
                {
                    Strategy = new RespondentIdentificationStrategyDefinition
                    {
                        StrategyType = RequestRespondentDataIdentificationStrategyEnum.ExternalIdentifier,
                        FailOnEmpty = true,
                        ExternalUniqueIdentifier = uei,
                        QuestionnaireId = qId,
                        TerminalId = tId
                    }
                },
                DynamicValuesFilter = new RespondentDataDynamicValueRequestFilter
                {
                    MinimalDate = dateMinSearch,
                    MaximumDate = dateMaxSearch,
                    DynamicKeysList = new List<string>()
                },
                QuestionDescriptionsFilter = new List<string>
                {
                    "q1c1"
                }
            };
            
            var getResultTask = await ServerRestRequestsUtil.GetRespondentData(CancellationToken.None, conf, authToken, getParameters, false, new FileInfo(contentGet));
            var getResult = getResultTask?.Data;
            Assert.IsNotNull(getResult);

            Assert.AreEqual(sendResult.TargetRespondent.UniqueExternalIdentifier, getResult.TargetRespondent.UniqueExternalIdentifier);

            Assert.AreEqual(1, getResult.DynamicValuesFound.Count);
            var resultValueList = getResult.DynamicValuesFound[0].Values;
            Assert.AreEqual(valueList[0], resultValueList[0]);
            Assert.AreEqual(valueList[1], resultValueList[1]);

            // Vérification des données de questionnaire
            Assert.IsTrue(getResult.AnswersFound.Count > 0); // TODO Éventuellement regarder qu'il n'y ait que UNE seule réponse....
            Assert.AreEqual(uei, getResult.AnswersFound[0].TextValue);

            // #3 On tente maintenant de télécharger les fichiers en bunch.

            var tokenToDownload = new List<string>();
            foreach (var dv in getResult.DynamicValuesFound)
            {
                tokenToDownload.AddRange(dv.Values.Skip(2));
            }
            Assert.AreEqual(dynamicFileValues.Length, tokenToDownload.Count);

            var downloadParameters = new RequestDownloadZipFilePayload
            {
                FilesToDownload = tokenToDownload
            };
            var fileDownload = new FileInfo(outDir + StringUtil.GenerateGuidString() + ".zip");

            var downloadResultTask = ServerRestRequestsUtil.DownloadMultipleFiles(CancellationToken.None, conf, authToken, downloadParameters, fileDownload);
            Assert.IsTrue(downloadResultTask != null && downloadResultTask.Data);
            fileDownload.Refresh();
            Assert.IsTrue(fileDownload.Exists);
            Assert.IsTrue(fileDownload.Length > 0);

            // #4 extraction des fichiers et vérifications
            var extractDir = new DirectoryInfo(outDir);
            var extractCheck = DirectoryUtil.ExtractZipToLocalTemporaryDirectory(fileDownload.FullName, extractDir);
            Assert.AreEqual(extractCheck, extractDir);

            var extractedFiles = extractDir.GetFiles();
            // Il faut un fichier nommé "association.json", plus le ZIP lui-même, qui est explosé dans le même répertoire.
            // Il y a donc 8 fichiers: 4 images, leur 4 thumbnails, plus le ZIP et le fichier JSON "association.json", plus le fichier d'analyse du GET.
            Assert.AreEqual(dynamicFileValues.Length + 3, extractedFiles.Length);
            Assert.IsTrue(extractedFiles.All(f => f.Exists && f.Length > 0));

            var jsonDictFile = extractedFiles.First(f => f.Name.Equals("association.json", StringComparison.InvariantCultureIgnoreCase));
            Assert.IsNotNull(jsonDictFile);

            // Tester la conformité des fichiers.
            var fileDic = JsonUtil.ReadJsonFile<Dictionary<string, string>>(jsonDictFile);
            Assert.IsNotNull(fileDic);
            Assert.AreEqual(8, fileDic.Count);

            // Dans ce cas-ci, on sait que tous les fichiers existent
            Assert.IsTrue(fileDic.Keys.All(k => tokenToDownload.Contains(k))); // Tous les tokens sont représentés
            Assert.IsTrue(fileDic.Values.All(v => extractedFiles.Any(ef => ef.Name.Equals(v)))); // Tous les fichiers apparaissent

            // On teste maintenant que tous les fichiers sont égaux (à part le nom) au niveau binaire
            for (var i = 2; i < resultValueList.Count; i++) // Tableau de valeurs du GET
            {
                var t = tokenToDownload[i-2];
                var nomFileZip = fileDic[t];
                var downFile = extractedFiles.First(ef => ef.Name.Equals(nomFileZip));

                var oriFile = originalFiles[i - 2].FullName;

                Assert.IsTrue(FileUtil.BinaryFileEqualCheck(oriFile, downFile.FullName), "Files are not equal:\n\t'{0}'\n\t'{1}'", oriFile, downFile.FullName);
            }
        }
    }
}