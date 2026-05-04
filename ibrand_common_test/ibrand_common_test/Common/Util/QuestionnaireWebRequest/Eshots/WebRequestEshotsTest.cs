using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NomadLogic.Common;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Services;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Json;
using NomadLogic.Common.Util.Networking;
using NomadLogic.Common.Util.Networking.QuestionnaireWebRequest;
using NomadLogic.Common.Util.Networking.QuestionnaireWebRequest.Eshots;
using NUnit.Framework;

namespace NomadLogic.Test.Common.Util.QuestionnaireWebRequest.Eshots
{
    /// <summary>
    /// Test les appels vers Eshots sans le support de service (on fait une authentification à chaque fois).
    /// </summary>
    public class WebRequestEshotsTest : TestBaseNunit
    {
        public T CheckRequestResult<T>(RestRequestJsonResponse<T> resRefresh, FileInfo saveTo = null, HttpStatusCode expectedStatus = HttpStatusCode.OK) where T : class
        {
            Assert.IsNotNull(resRefresh?.InstancedBody);

            Assert.That(resRefresh.StatusCode == expectedStatus);

            if (saveTo == null) return null;
            JsonUtil.WriteJsonFile(saveTo, resRefresh.InstancedBody);
            saveTo.Refresh();
            Assert.IsTrue(saveTo.Exists);

            return resRefresh.InstancedBody;
        }

        [Test]
        public void TestEventDayEquality()
        {
            var v1A = new EshotsEventDay
            {
                EventDayId = 25,
                EventDayCode = "test"
            };

            var v1B = new EshotsEventDay
            {
                EventDayId = 25,
                EventDayCode = "test"
            };

            var v2 = new EshotsEventDay
            {
                EventDayId = 25,
                EventDayCode = "another"
            };

            var v3 = new EshotsEventDay
            {
                EventDayId = 17,
                EventDayCode = "another"
            };

            Assert.AreEqual(v1A, v1B);
            Assert.AreEqual(v1B, v1B);
            Assert.AreEqual(v1B, v1A);
            Assert.AreEqual(v1A, v1A);
            Assert.AreNotEqual(v1A, v2);
            Assert.AreNotEqual(v1B, v2);
            Assert.AreNotEqual(v2, v3);

            Assert.IsTrue(v1A == v1B);
            Assert.IsTrue(v1A != v2);
            Assert.IsTrue(v1A != v3);
            Assert.IsTrue(v2 != v3);
        }

        [Test]
        public async Task TestAuthificationAndRenew()
        {
            var tokens = await AuthificationAndRenew();
            Assert.IsNotNull(tokens?.AccessToken);
            Assert.IsNotNull(tokens.RefreshToken);
        }

        public async Task<EshotsAuthentificationResultInfo> AuthificationAndRenew()
        {
            var resultAuthJson = GetFullOutputDir() + "request_auth.json";
            var resultRenewJson = GetFullOutputDir() + "request_renew.json";
            EnsureFileNotExists(resultAuthJson);
            EnsureFileNotExists(resultRenewJson);

            var executor = new WebRequestEshots();
            Assert.IsTrue(await executor.InitializeExecutor());

            var authInfo = new EshotsAuthentificationRequestInfo
            {
                UserName = "devs@nomadlogic.com",
                Password = "pn3HgAfmKeFAKG"
            };

            var res = await executor.GetFullAuthentication(authInfo, CancellationToken.None);
            CheckRequestResult(res, new FileInfo(resultAuthJson));

            // Tentative de renouveler le jeton immédiatement
            var resRefresh = await executor.RenewToken(res.InstancedBody.RefreshToken, CancellationToken.None);
            CheckRequestResult(resRefresh, new FileInfo(resultRenewJson));

            return resRefresh.InstancedBody;
        }

        [TestCase("eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJpZGVudGl0eSI6eyJhY3RpdmVGbGFnIjp0cnVlLCJwaG9uZSI6bnVsbCwiYWdyZWVkRXVsYUZsYWciOmZhbHNlLCJva3RhRXh0VXNlcklkIjoiMDB1NHl2YTRqN2twYnpnOFoyOTciLCJkZWxldGVkRmxhZyI6ZmFsc2UsImNsaWVudElkIjoxODYsIm9rdGFTeW5jRmxhZyI6ZmFsc2UsImZpcnN0TmFtZSI6IlVTQUEiLCJva3RhVXBkYXRlRmxhZyI6ZmFsc2UsImxhc3ROYW1lIjoiQWNlIiwibG9naW5OYW1lIjoidXNhYWFjZUBub21hZGxvZ2ljLmNvbSIsInVzZXJJZCI6NTAwMDAwNDcsImVtYWlsIjoidXNhYWFjZUBub21hZGxvZ2ljLmNvbSJ9LCJmcmVzaCI6ImZhbHNlIiwidHlwZSI6InJlZnJlc2giLCJleHAiOjE2MDYzMjI2NzEuODM5NDY5LCJpYXQiOjE2MDM3MzA2NzEuODM5NDY5LCJuYmYiOjE2MDM3MzA2NzEuODM5NDY5LCJqdGkiOiI3NzY4MGZlMi0wOWNhLTQ0ZmItYjgyYy03M2M5MWU4ODc2MjEifQ.okCICojR995H4v8EUJp3pd1K8nAA0qZGy3prQ8aD0OA")]
        public async Task TestRenewOnSpecificToken(string token)
        {
            Console.Out.WriteLine("Token used: " + token);

            var resultRenewJson = GetFullOutputDir() + "request_renew.json";
            EnsureFileNotExists(resultRenewJson);

            var executor = new WebRequestEshots();
            Assert.IsTrue(await executor.InitializeExecutor());

            // Tentative de renouveler le jeton immédiatement
            var resRefresh = await executor.RenewToken(token, CancellationToken.None);
            var body = CheckRequestResult(resRefresh, new FileInfo(resultRenewJson));

            Assert.IsNotNull(body.AccessToken, "No Access token value detected.");
            Assert.IsNotNull(body.RefreshToken, "No Refresh token value detected.");
        }

        [Test]
        public async Task TestFetchCampaignEvent()
        {
            var resultEventJson = GetFullOutputDir() + "request_event.json";
            EnsureFileNotExists(resultEventJson);

            var resultEventDaysJson = GetFullOutputDir() + "request_eventdays.json";
            EnsureFileNotExists(resultEventDaysJson);

            var resultEventDaysPresentJson = GetFullOutputDir() + "request_eventdays_present.json";
            EnsureFileNotExists(resultEventDaysPresentJson);

            var executor = new WebRequestEshots();
            Assert.IsTrue(await executor.InitializeExecutor());

            var authInfo = new EshotsAuthentificationRequestInfo
            {
                UserName = "devs@nomadlogic.com",
                Password = "pn3HgAfmKeFAKG"
            };
            var res = await executor.GetFullAuthentication(authInfo, CancellationToken.None);
            CheckRequestResult(res);

            var token = res.InstancedBody.AccessToken;

            // Obtient les informations d'un événement complet.
            var resEvent = await executor.GetEventList(token, true, CancellationToken.None);
            CheckRequestResult(resEvent, new FileInfo(resultEventJson));

            var resEventDays = await executor.GetEventDaysList(token, true, CancellationToken.None);
            CheckRequestResult(resEventDays, new FileInfo(resultEventDaysJson));

            var resEventDaysPresent = await executor.GetEventDaysList(token, false, CancellationToken.None);
            CheckRequestResult(resEventDaysPresent, new FileInfo(resultEventDaysPresentJson));

            // À vérifier. Retourne exactement la même information.
            //Assert.AreNotEqual(resEventDays.InstancedBody.Events.Count, resEventDaysPresent.InstancedBody.Events.Count);
        }

        [TestCase("swing_green.mp4", "ABNHYUJKKO", HttpStatusCode.Created, null)]
        [TestCase("swing_green.mp4", "ABNHYUJKKO", HttpStatusCode.Created, "other@email.com")]
        [TestCase("overlay.png", "FPYECYC74A", HttpStatusCode.BadRequest, "")]
        [TestCase("overlay.png", null, HttpStatusCode.Created, "other@email.com")]
        [TestCase("intro.jpg", "F76ZQ6QGHJ", HttpStatusCode.Created, "devs@blabla.com")]
        [TestCase("modify_file.jsx", "ABNHYUJKKO", HttpStatusCode.BadRequest, null)]
        [TestCase("Louis-Regular.otf", "FPYECYC74A", HttpStatusCode.BadRequest, "")]
        [TestCase("video_data.txt", "F76ZQ6QGHJ", HttpStatusCode.BadRequest, "devs@blabla.com")]
        public async Task TestUploadFile(string uploadFileName, string customerId, HttpStatusCode expected, string email)
        {
            var uploadFile = GetFullOutputDir() + "base64." + uploadFileName;
            if (!File.Exists(uploadFile))
            {
                var sourceFile = BaseDir + uploadFileName;
                EnsureFileExists(sourceFile);
                Assert.IsTrue(await FileUtil.ConvertFiletoBase64(new FileInfo(sourceFile), new FileInfo(uploadFile), CancellationToken.None), 
                    "Could not convert source file.");
                EnsureFileExists(uploadFile);
            }

            var executor = new WebRequestEshots();
            Assert.IsTrue(await executor.InitializeExecutor());

            // Auth
            var authInfo = new EshotsAuthentificationRequestInfo
            {
                UserName = "devs@nomadlogic.com",
                Password = "pn3HgAfmKeFAKG"
            };
            var res = await executor.GetFullAuthentication(authInfo, CancellationToken.None);
            CheckRequestResult(res);

            // Event days
            var resEventDays = await executor.GetEventDaysList(res.InstancedBody.AccessToken, true, CancellationToken.None);
            CheckRequestResult(resEventDays);

            var firsteventDay = resEventDays.InstancedBody.Events.FirstOrDefault()?.EventDays?.FirstOrDefault().Value ?? -1;
            Assert.Greater(firsteventDay, 0);

            // Création du payload

            var utcDate = DateTime.UtcNow.ToString("yyyyMMdd_hhmmss", CultureInfo.InvariantCulture);
            Console.Out.WriteLine("Test timed: " + utcDate);

            var uploadResultJson = GetFullOutputDir() + uploadFileName + "." + utcDate +".uploadResult.json";
            EnsureFileNotExists(uploadResultJson);
            var uploadJson = GetFullOutputDir() + uploadFileName + "." + utcDate + ".payload.json";

            var uploadConfig = new EshotsFileUploadsRequest
            {
                EventDaysUploads = new List<EshotsEventDayFileUploads>
                {
                    new EshotsEventDayFileUploads
                    {
                        EventDayId = firsteventDay,
                        EventDaysUploads = new List<EshotsFileUpload>
                        {
                            new EshotsFileUpload
                            {
                                FileName = StringUtil.GetFileName(uploadFile),
                                CreationDateUtc = DateTime.UtcNow.ToString("yyyy-MM-dd hh:mm:ss", CultureInfo.InvariantCulture),
                                FileContent = File.ReadAllText(uploadFile, Encoding.UTF8),
                                ConsumersToAssociate = new List<EshotsConsumerUploadData>
                                {
                                    new EshotsConsumerUploadData
                                    {
                                        Email = email,
                                        ConsumerUniqueId = customerId
                                    }
                                }
                            }
                        }
                    }
                }
            };
            
            Assert.IsNotNull(uploadConfig);

            var uploadResult = await executor.UploadFileLinkedToConsumer(uploadConfig, res.InstancedBody.AccessToken, CancellationToken.None);

            uploadConfig.EventDaysUploads[0].EventDaysUploads[0].FileContent = "REMOVED";
            JsonUtil.WriteJsonFile(new FileInfo(uploadJson), uploadConfig);

            CheckRequestResult(uploadResult, new FileInfo(uploadResultJson), expected);
        }

        // tests basés sur le projet ESH-2020-05
        [TestCase("IMG_9332.JPG", "AH3DAWTKNG", HttpStatusCode.Created, "usaaace@nomadlogic.com", "7yfNW6jnVNJ25K")]
        [TestCase("photobooth_image.jpg", "AXK3JYFECW", HttpStatusCode.Created, "usaaace@nomadlogic.com", "7yfNW6jnVNJ25K")]
        [TestCase("statue_model_01.png", "AAEKM6DZQY", HttpStatusCode.Created, "usaaace@nomadlogic.com", "7yfNW6jnVNJ25K")]
        [TestCase("statue_model_02.png", "AJZE2T3946", HttpStatusCode.Created, "usaaace@nomadlogic.com", "7yfNW6jnVNJ25K")]
        [TestCase("statue_model_03.png", "AW4Q6ZNH2E", HttpStatusCode.Created, "usaaace@nomadlogic.com", "7yfNW6jnVNJ25K")]
        [TestCase("statue_model_04.png", "AD2JKQX2YC", HttpStatusCode.Created, "usaaace@nomadlogic.com", "7yfNW6jnVNJ25K")]
        [TestCase("foreground.png", "A3PME4AP6Q", HttpStatusCode.Created, "usaaace@nomadlogic.com", "7yfNW6jnVNJ25K")]
        public async Task TestUploadFileWithTokenId(string uploadFileName, string eventTokenId, HttpStatusCode expected, string user, string pass)
        {
            var uploadFile = GetFullOutputDir() + "base64." + uploadFileName;
            if (!File.Exists(uploadFile))
            {
                var sourceFile = BaseDir + uploadFileName;
                EnsureFileExists(sourceFile);
                Assert.IsTrue(await FileUtil.ConvertFiletoBase64(new FileInfo(sourceFile), new FileInfo(uploadFile), CancellationToken.None), 
                    "Could not convert source file.");
                EnsureFileExists(uploadFile);
            }

            var executor = new WebRequestEshots();
            Assert.IsTrue(await executor.InitializeExecutor());

            // Auth
            var authInfo = new EshotsAuthentificationRequestInfo
            {
                UserName = user,
                Password = pass
            };
            var res = await executor.GetFullAuthentication(authInfo, CancellationToken.None);
            CheckRequestResult(res);

            // Event days
            var resEventDays = await executor.GetEventDaysList(res.InstancedBody.AccessToken, true, CancellationToken.None);
            CheckRequestResult(resEventDays);

            var firsteventDay = resEventDays.InstancedBody.Events.FirstOrDefault()?.EventDays?.FirstOrDefault().Value ?? -1;
            Assert.Greater(firsteventDay, 0);

            // Création du payload

            var utcDate = DateTime.UtcNow.ToString("yyyyMMdd_hhmmss", CultureInfo.InvariantCulture);
            Console.Out.WriteLine("Test timed (UTC): " + utcDate);

            var uploadResultJson = GetFullOutputDir() + uploadFileName + "." + utcDate +".uploadResult.json";
            EnsureFileNotExists(uploadResultJson);
            var uploadJson = GetFullOutputDir() + uploadFileName + "." + utcDate + ".payload.json";

            var uploadConfig = new EshotsFileUploadsRequest
            {
                EventDaysUploads = new List<EshotsEventDayFileUploads>
                {
                    new EshotsEventDayFileUploads
                    {
                        EventDayId = firsteventDay,
                        EventDaysUploads = new List<EshotsFileUpload>
                        {
                            new EshotsFileUpload
                            {
                                FileName = StringUtil.GetFileName(uploadFile),
                                CreationDateUtc = DateTime.UtcNow.ToString("yyyy-MM-dd hh:mm:ss", CultureInfo.InvariantCulture),
                                FileContent = File.ReadAllText(uploadFile, Encoding.UTF8),
                                ConsumersToAssociate = new List<EshotsConsumerUploadData>
                                {
                                    new EshotsConsumerUploadData
                                    {
                                        EventTokenId = eventTokenId
                                    }
                                }
                            }
                        }
                    }
                }
            };
            
            Assert.IsNotNull(uploadConfig);

            var uploadResult = await executor.UploadFileLinkedToConsumer(uploadConfig, res.InstancedBody.AccessToken, CancellationToken.None);

            uploadConfig.EventDaysUploads[0].EventDaysUploads[0].FileContent = "REMOVED";
            JsonUtil.WriteJsonFile(new FileInfo(uploadJson), uploadConfig);

            CheckRequestResult(uploadResult, new FileInfo(uploadResultJson), expected);
        }

        [TestCase("ABNHYUJKKO", EshotsConsumerRequestIdType.EventTokenId, HttpStatusCode.OK, true)]
        [TestCase("FPYECYC74A", EshotsConsumerRequestIdType.EventTokenId, HttpStatusCode.OK, true)]
        [TestCase("F76ZQ6QGHJ", EshotsConsumerRequestIdType.EventTokenId, HttpStatusCode.OK, true)]
        [TestCase("INVALID", EshotsConsumerRequestIdType.EventTokenId, HttpStatusCode.OK, false)]
        [TestCase("test1@eshots.com", EshotsConsumerRequestIdType.Email, HttpStatusCode.OK, true)]
        [TestCase("none@eshots.com", EshotsConsumerRequestIdType.Email, HttpStatusCode.OK, false)]
        public async Task TestCustomerId(string custTokenId, EshotsConsumerRequestIdType tokenType, HttpStatusCode expectedCode, bool isFound)
        {
            var resultGetCustomerRequest = GetFullOutputDir() + $"request_customer_{custTokenId}.json";
            EnsureFileNotExists(resultGetCustomerRequest);

            var executor = new WebRequestEshots();
            Assert.IsTrue(await executor.InitializeExecutor());

            // Auth
            var authInfo = new EshotsAuthentificationRequestInfo
            {
                UserName = "devs@nomadlogic.com",
                Password = "pn3HgAfmKeFAKG"
            };
            var res = await executor.GetFullAuthentication(authInfo, CancellationToken.None);
            CheckRequestResult(res);

            var consumerCheck = await executor.GetConsumerFromId(custTokenId, tokenType, res.InstancedBody.AccessToken, CancellationToken.None);
            CheckRequestResult(consumerCheck, new FileInfo(resultGetCustomerRequest), expectedCode);
            var consList = consumerCheck?.InstancedBody?.ConsumerIdentificationResult?.ConsumerData;

            Assert.AreEqual(isFound, consList != null);
        }

        [Test]
        public async Task TestCustomerMapping()
        {
            var baseDirEshots = BaseDir + "WebRequestFacade" + Path.DirectorySeparatorChar + "eshots" + Path.DirectorySeparatorChar;

            var reqResult01 = baseDirEshots + "request_customer_ABNHYUJKKO.json"; // Contient tous les champs
            var reqResult02 = baseDirEshots + "request_customer_F76ZQ6QGHJ.json"; // Juste courriel
            var reqResult03 = baseDirEshots + "request_customer_FPYECYC74A.json"; // Vide mais valide
            var reqResultInvalid = baseDirEshots + "request_customer_INVALID.json"; // Ne contient pas de réponse (aucun "consumer")
            var configFile = baseDirEshots + "correct_configuration_eshots.json";
            EnsureFileExists(reqResult01, reqResult02, reqResult03, reqResultInvalid, configFile);

            var facade = JsonUtil.ReadJsonFile<WebRequestFacade>(new FileInfo(configFile));
            Assert.IsTrue(await facade.Init());

            var res01 = JsonUtil.ReadJsonFile<EshotsConsumerResultInfo>(new FileInfo(reqResult01));
            var res02 = JsonUtil.ReadJsonFile<EshotsConsumerResultInfo>(new FileInfo(reqResult02));
            var res03 = JsonUtil.ReadJsonFile<EshotsConsumerResultInfo>(new FileInfo(reqResult03));
            var resInv = JsonUtil.ReadJsonFile<EshotsConsumerResultInfo>(new FileInfo(reqResultInvalid));

            Assert.IsNotNull(res01);
            Assert.IsNotNull(res02);
            Assert.IsNotNull(res03);
            Assert.IsNotNull(resInv);
            Assert.AreEqual("Pierre", res01.ConsumerIdentificationResult.ConsumerData.FirstName);
            Assert.AreEqual("test1@eshots.com", res02.ConsumerIdentificationResult.ConsumerData.Email);
            Assert.IsNotNull(res03.ConsumerIdentificationResult.ConsumerData);
            Assert.IsNull(resInv.ConsumerIdentificationResult.ConsumerData);

            // Test du résultat 1
            {
                var mockQuestionnaire01 = new Mock<ISaveLocalRespondentData>();
                mockQuestionnaire01.Setup(m => m.SaveAnswerToQuestion(new QuestionIdentifier(2,1,QuestionTypeEnum.Text), res01.ConsumerIdentificationResult.ConsumerData.Email));
                mockQuestionnaire01.Setup(m => m.SaveAnswerToQuestion(new QuestionIdentifier(3,1,QuestionTypeEnum.Text), res01.ConsumerIdentificationResult.ConsumerData.FirstName));
                mockQuestionnaire01.Setup(m => m.SaveAnswerToQuestion(new QuestionIdentifier(4,1,QuestionTypeEnum.Text), res01.ConsumerIdentificationResult.ConsumerData.LastName));
                mockQuestionnaire01.Setup(m => m.SaveAnswerToQuestion(new QuestionIdentifier(5,1,QuestionTypeEnum.Text), res01.ConsumerIdentificationResult.ConsumerData.PhoneNumber));
                facade.SaveDataToQuestionnaire(mockQuestionnaire01.Object, res01.ConsumerIdentificationResult);
                mockQuestionnaire01.VerifyAll();
            }

            // Test du résultat 2
            {
                var mockQuestionnaire02 = new Mock<ISaveLocalRespondentData>();
                mockQuestionnaire02.Setup(m => m.SaveAnswerToQuestion(new QuestionIdentifier(2,1,QuestionTypeEnum.Text), res02.ConsumerIdentificationResult.ConsumerData.Email));
                facade.SaveDataToQuestionnaire(mockQuestionnaire02.Object, res02.ConsumerIdentificationResult);
                mockQuestionnaire02.VerifyAll();
            }

            // Test du résultat 3
            {
                var mockQuestionnaire03 = new Mock<ISaveLocalRespondentData>();
                facade.SaveDataToQuestionnaire(mockQuestionnaire03.Object, res03.ConsumerIdentificationResult);
                mockQuestionnaire03.VerifyAll();
            }

            // Test du résultat 4
            {
                var mockQuestionnaire04 = new Mock<ISaveLocalRespondentData>();
                facade.SaveDataToQuestionnaire(mockQuestionnaire04.Object, resInv.ConsumerIdentificationResult); // Null, fait un no-op
                mockQuestionnaire04.VerifyAll();
            }

        }

        [TestCase("ABNHYUJKKO", "EventTokenId", "email", "name")]
        [TestCase("FPYECYC74A", "EventTokenId", "email", "name")]
        [TestCase("F76ZQ6QGHJ", "EventTokenId", "email", "name")]
        public async Task TestFacadeFromFileConfiguration(string id, string typeStr, string emailAnswer, string nameAnswer)
        {
            // Préparation. Ce test requiert que la "bd" et le "service" soient correctement configurés.
            var tokens = await AuthificationAndRenew();

            var eshotConfig = new EshotsDataConfiguration
            {
                ConfigurationDate = DateTime.Now,
                EventDaysCached = new List<EshotsEventDay>(),
                AllowAllDays = true,
                AccessToken = tokens.AccessToken,
                RefreshToken = tokens.RefreshToken
            };

            var mockService = new Mock<EshotsApiService>();
            var mockDatabase = new Mock<IDatabaseEshots>();

            mockDatabase.Setup(m => m.GetLastEshotsConfiguration()).Returns(eshotConfig);

            // Préparation du service
            mockService.Setup(m => m.Init()).Returns(true);
            mockService.SetupGet(m => m.LocalDatabase).Returns(mockDatabase.Object);
            mockService.SetupGet(m => m.ApiState).Returns(EshotsApiConfigState.CanSend);

            Assert.IsTrue(await ServicesContainer.AddService(mockService.Object));

            // Exécution

            var configFile = BaseDir + "WebRequestFacade" + Path.DirectorySeparatorChar + "eshots" + Path.DirectorySeparatorChar + "correct_configuration_eshots.json";
            Assert.That(File.Exists(configFile));
            var facade = JsonUtil.ReadJsonFile<WebRequestFacade>(new FileInfo(configFile));
            Assert.IsTrue(await facade.Init());
            
            var webParams = new Dictionary<string, object> { { "id", id }, { "typeStr", typeStr } };
            var res = await facade.LaunchMethod<EshotsConsumerIdentificationResult>("GetConsumerRequestFromFacade", webParams);
            
            Assert.IsNotNull(res);
            Assert.AreEqual(EshotsComplexRequestResultEnum.Success, res.ExecutionResult, "Was not a successful request.");

            var mockQuestionnaire = new Mock<ISaveLocalRespondentData>();
            mockQuestionnaire.SetupGet(m => m.SaveAnswerToQuestion(new QuestionIdentifier(2,1,QuestionTypeEnum.Text), emailAnswer)).Returns(true);
            mockQuestionnaire.SetupGet(m => m.SaveAnswerToQuestion(new QuestionIdentifier(3,1,QuestionTypeEnum.Text), nameAnswer)).Returns(true);

            facade.SaveDataToQuestionnaire(mockQuestionnaire.Object, res);

            Console.Out.WriteLine($"Answer from server: {ClassUtil.ToStringPublicProperties(res)}");

            mockQuestionnaire.Verify((m => m.SaveAnswerToQuestion(new QuestionIdentifier(2,1,QuestionTypeEnum.Text), emailAnswer)), Times.Once());
            mockQuestionnaire.Verify((m => m.SaveAnswerToQuestion(new QuestionIdentifier(3,1,QuestionTypeEnum.Text), nameAnswer)), Times.Once());
        }
    }
}