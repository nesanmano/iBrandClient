using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NomadLogic.Common.Serveur.Requests;
using NomadLogic.Common.Serveur.Structures;
using NomadLogic.Common.Services;
using NomadLogic.Common.Services.Configuration;
using NUnit.Framework;
using static NomadLogic.Common.Serveur.Requests.ServerRestRequestResultType;

namespace NomadLogic.Test.Common.Serveur
{
    public class ServerRestRequestMultipartFormTest : ServerRestRequestUtilTestBase
    {
        [Test]
        public async Task GetRespondentDataTest()
        {
            var baseDirOut = GetFullOutputDir();
            EnsureDirEmpty(baseDirOut);

            var outFile = new FileInfo(baseDirOut + "output.json");

            var conf = ServicesContainer.GetService<NomadConfiguration>().CurrentServerConfiguration;

            var authToken = await GetAuthorizationToken(conf);

            var filterDyn = new RespondentDataDynamicValueRequestFilter
            {
                MinimalDate = DateTimeOffset.Now - TimeSpan.FromDays(20),
                MaximumDate = DateTimeOffset.Now,
                DynamicKeysList = new List<string> {"test", "test01"}
            };
            var strat = new RespondentIdentificationStrategyDefinition
            {
                StrategyType = RequestRespondentDataIdentificationStrategyEnum.ExternalIdentifier,
                FailOnEmpty = false,
                ExternalUniqueIdentifier = "alloTestTest",
                QuestionnaireId = 1,
                QuestionNo = 1
            };

            var payload = new RequestGetRespondentDataPayload
            {
                DynamicValuesFilter = filterDyn,
                QuestionDescriptionsFilter = new List<string> {"q1", "q2"},
                RespondentIdentification = new RespondentIdentificationStrategy
                {
                    Strategy = strat
                }
            };

            var res = await ServerRestRequestsUtil.GetRespondentData(CancellationToken.None, conf, authToken, payload, true, outFile);

            Assert.IsNotNull(res);
            Assert.IsTrue(outFile.Exists);
            Assert.AreEqual(Success, res.Result);

            // TODO Tests sur données retournées (même mock)
        }

        [Test]
        public async Task SendRespondentDataTest()
        {
            var outDir = OutputBaseDir;
            var baseDir = BaseDir;

            // Quelques fichiers pas trop long à envoyer
            var file01 = baseDir + "1080x1080_overlay.png";
            var file02 = baseDir + "Opencvpic3sample.png";
            EnsureDirEmpty(outDir);
            EnsureFileExists(file01);
            EnsureFileExists(file02);

            var conf = ServicesContainer.GetService<NomadConfiguration>().CurrentServerConfiguration;

            var authToken = await GetAuthorizationToken(conf);

            var payload = new RequestSendRespondentDataPayload
            {
                RequestConfig = new RequestSendRespondentDataConfig
                {
                    SendRespondentIdentification = new RequestSendRespondentDataIdentification
                    {
                        SearchStrategy = new RequestSendRespondentDataIdentificationStrategy
                        {
                            SearchStrategy = RequestRespondentDataIdentificationStrategyEnum.ExternalIdentifier,
                            ServerIdQuestionnaire = 1,
                            ServerIdTerminal = 1,
                            TextValue = "Attaboy!"
                        }
                    },
                    AnswersToSend = new List<RequestSendRespondentDataAnswer>
                    {
                        new RequestSendRespondentDataAnswer
                        {
                            TextValue = "ref:file_test_01",
                            QuestionNumber = 1
                        },
                        new RequestSendRespondentDataAnswer
                        {
                            TextValue = "ref:file_test_02",
                            QuestionNumber = 2
                        },
                        new RequestSendRespondentDataAnswer
                        {
                            TextValue = "MegaHot",
                            QuestionNumber = 3
                        }
                    },
                    DynamicValuesToSend = new List<RequestSendRespondentDataDynamicValue>
                    {
                        new RequestSendRespondentDataDynamicValue
                        {
                            DataKey = "booth_09",
                            DataValues = new List<string>
                            {
                                "ref:file_test_01",
                                "ref:file_test_02",
                                "SomeData",
                                "null_text",
                                "null"
                            }
                        }
                    }
                },
                RequestFiles = new Dictionary<string, FileInfo>
                {
                    { "file_test_01", new FileInfo(file01) },
                    { "file_test_02", new FileInfo(file02) }
                }
            };

            var res = ServerRestRequestsUtil.SendRespondentData(CancellationToken.None, conf, authToken, payload, true);

            Assert.IsNotNull(res);
            Assert.AreEqual(Success, res.Result);

            // TODO Tests sur données retournées (même mock)
        }
    }
}