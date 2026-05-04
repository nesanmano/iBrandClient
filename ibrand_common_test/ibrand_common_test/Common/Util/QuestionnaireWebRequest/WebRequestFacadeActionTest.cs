using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NomadLogic.Common;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Ui.NomadScreen.ScreenAction;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Json;
using NomadLogic.Common.Util.Networking.QuestionnaireWebRequest;
using NomadLogic.Common.Util.Networking.QuestionnaireWebRequest.Emisys;
using NomadLogic.Test.Common.Respondent;
using NUnit.Framework;
using RestSharp;

namespace NomadLogic.Test.Common.Util.QuestionnaireWebRequest
{
    /// <summary>
    /// Tests de sérialisation et d'exécution de méthode pour facade via réflexion.
    /// </summary>
    public class WebRequestFacadeActionTest : TestBaseNunit
    {
        /// <summary>
        /// On tente d'invoquer la méthode suivante sur la classe "WebRequestEmisysPayWall":
        /// public async Task-ResponseInitCardInfo- InitCard(string cardUid, PostInitCardInfo bodyData = null, Method webMethod = Method.POST, CancellationToken? cToken = null)
        /// qui a été défini dans la méthode elle-même.
        /// </summary>
        [Test]
        public async Task LoadWebRequestFacadeAction()
        {
            var ouputConfig = GetFullOutputDir() + "action_config_result.json";
            EnsureFileNotExists(ouputConfig);
            var ouputConfigFacade = GetFullOutputDir() + "full_facade_config.json";
            EnsureFileNotExists(ouputConfigFacade);

            var dicAnswers = new List<Answer>
            {
                new Answer{QuestionNo = 12, ChoiceNo = 1, Response = "FAFAFADA"}, // CardUID
                new Answer{QuestionNo = 11, ChoiceNo = 1, Response = "2500"}, // Budget
                new Answer{QuestionNo = 2, ChoiceNo = 1, Response = "Joe"}, // Prénom
                new Answer{QuestionNo = 3, ChoiceNo = 1, Response = "Bleau"}, // Nom
                new Answer{QuestionType = QuestionTypeEnum.Check, QuestionNo = 8}, // I Agree
                new Answer{QuestionNo = 7, ChoiceNo = 1 }, // Legal Age, c1 =  légal
                new Answer{QuestionNo = 9, ChoiceNo = 1 }, // Client TD, c1 =  est client
                new Answer{QuestionNo = 10, Response = "2"}, // Lifestyle (1-3)
                new Answer{QuestionNo = 6, ChoiceNo = 1, Response = "514-966-5487"}, // Sms
            };

            var resp = new MockRespondent
            {
                Language = "fr"
            };

            var def = new WebRequestFacadeAction
            {
                ReturnTypeName = typeof(ResponseInitCardInfo).FullName,
                WebRequestMethodName = "InitCard",
                WebRequestMethodParameterDefinitions = new List<ParameterDefinition>
                {
                    new ParameterDefinitionValue
                    {
                        Name = "cardUid",
                        NameType = "string",
                        TokenValue = "{q12}"
                    },
                    new ParameterDefinitionObject
                    {
                        Name = "bodyData",
                        NameType = "NomadLogic.Common.Util.QuestionnaireWebRequest.Emisys.PostInitCardInfo",
                        PropertyMap = new List<PropertyQuestionnaireMapping>
                        {
                            new PropertyQuestionnaireMapping
                            {
                                PropertyName = "Amount",
                                TokenValue = "{q11}"
                            },
                            new PropertyQuestionnaireMapping
                            {
                                PropertyName = "Language",
                                TokenValue = "{RESPONDENT:Language}"
                            },
                            new PropertyQuestionnaireMapping
                            {
                                PropertyName = "Email",
                                TokenValue = "{q12}@nomadlogic.com"
                            },
                            new PropertyQuestionnaireMapping
                            {
                                PropertyName = "Firstname",
                                TokenValue = "{q2}"
                            },
                            new PropertyQuestionnaireMapping
                            {
                                PropertyName = "Lastname",
                                TokenValue = "{q3}"
                            },
                            new PropertyQuestionnaireMapping
                            {
                                PropertyName = "FirstnameEmisys",
                                TokenValue = "{q2}"
                            },
                            new PropertyQuestionnaireMapping
                            {
                                PropertyName = "LastnameEmisys",
                                TokenValue = "{q3}"
                            },
                            new PropertyQuestionnaireMapping
                            {
                                PropertyName = "IsAgreeContestRules",
                                TokenValue = "{q8:bool}"
                            },
                            new PropertyQuestionnaireMapping
                            {
                                PropertyName = "IsLegalAge",
                                TokenValue = "{q7c1:bool}"
                            },
                            new PropertyQuestionnaireMapping
                            {
                                PropertyName = "IsTdClient",
                                TokenValue = "{q9c1:bool}"
                            },
                            new PropertyQuestionnaireMapping
                            {
                                PropertyName = "LifeStyleCode",
                                TokenValue = "{q10}"
                            },
                            new PropertyQuestionnaireMapping
                            {
                                PropertyName = "Sms",
                                TokenValue = "{q6}"
                            }
                        }
                    },
                    new ParameterDefinitionValue
                    {
                        Name = "webMethod",
                        NameType = "RestSharp.Method",
                        TokenValue = "POST"
                    }
                }
            };

            var facade = new WebRequestFacade()
            {
                WebRequestAction = def,
                WebExecutorClassName = "NomadLogic.Common.Util.QuestionnaireWebRequest.Emisys.WebRequestEmisysPayWall",
                WebExecutorConfiguration = new Dictionary<string, object>()
                {
                    {"Timeout", 2500 },
                    {"DoNotTestEndPointInit", true },
                    {"LoginInfo", new EmisysApiLoginInfo
                    {
                        Apikey = "Nu/9qmq6e38NFpB..vq6",
                        UserEmail = "api@nomadlogic.com",
                        Version = "v1",
                        RootApiUrl = "https://live-test.byemisys.com/api"
                    }},
                }
            };
            await TestFacadeExec(dicAnswers.ToDictionary(a => a.QuestionIdentifier), resp, facade);

            // On écrit la config valide sur disque comme future référence.
            Assert.IsTrue(JsonUtil.WriteJsonFile(new FileInfo(ouputConfig), def));
            Assert.IsTrue(JsonUtil.WriteJsonFile(new FileInfo(ouputConfigFacade), facade));
        }

        private static async Task TestFacadeExec(Dictionary<QuestionIdentifier, Answer> dicAnswers, MockRespondent resp, WebRequestFacade facade)
        {
            // tente de lancer la requête web.
            Assert.IsTrue(await facade.Init());
            var res = await facade.ConstructAndInvokeMethod(resp, dicAnswers);
            Assert.IsNotNull(res);
            Assert.IsTrue(res is ResponseInitCardInfo);
            Console.Out.WriteLine(ClassUtil.ToStringPublicProperties(res, 1));

            // Vérifie le contenu des paramètres créés.
            var resultdico = facade.WebRequestAction.CreateInvokeParameters(resp, dicAnswers);

            Assert.IsTrue(resultdico.Count == 2);
            Assert.IsTrue(resultdico["cardUid"].Equals("FAFAFADA"));
            var body = resultdico["bodyData"] as PostInitCardInfo;
            Assert.IsNotNull(body);
            Assert.AreEqual(2500, body.Amount);
            Assert.AreEqual("FAFAFADA@nomadlogic.com", body.Email);
            Assert.AreEqual("514-966-5487", body.Sms);
            Assert.AreEqual("fr", body.Language);
            Assert.AreEqual("Joe", body.Firstname);
            Assert.AreEqual("Bleau", body.Lastname);
            Assert.AreEqual("Joe", body.FirstnameEmisys);
            Assert.AreEqual("Bleau", body.LastnameEmisys);
            Assert.AreEqual("true", body.IsAgreeContestRules);
            Assert.AreEqual("true", body.IsLegalAge);
            Assert.AreEqual("true", body.IsTdClient);
            Assert.AreEqual(2, body.LifeStyleCode);
        }

        /// <summary>
        /// On tente d'invoquer la méthode suivante sur la classe "WebRequestEmisysPayWall":
        /// public async Task-ResponseInitCardInfo- InitCard(string cardUid, PostInitCardInfo bodyData = null, Method webMethod = Method.POST, CancellationToken? cToken = null)
        /// qui a été défini dans un fichier.
        /// </summary>
        [Test]
        public async Task LoadWebRequestFacadeActionFromJsonFile()
        {
            var configFile = BaseDir + @"WebRequestFacade\correct_configuration_emisys.json";
            Assert.That(File.Exists(configFile));

            var dicAnswers = new List<Answer>
            {
                new Answer{QuestionNo = 12, Response = "FAFAFADA"}, // CardUID
                new Answer{QuestionNo = 11, Response = "2500"}, // Budget
                new Answer{QuestionNo = 2, Response = "Joe"}, // Prénom
                new Answer{QuestionNo = 3, Response = "Bleau"}, // Nom
                new Answer{QuestionNo = 8, QuestionType = QuestionTypeEnum.Check}, // I Agree
                new Answer{QuestionNo = 7, ChoiceNo = 1}, // Legal Age, s1 =  légal
                new Answer{QuestionNo = 9, ChoiceNo = 1}, // Client TD, s1 =  est client
                new Answer{QuestionNo = 10, ChoiceNo = 2}, // Lifestyle (1-3)
                new Answer{QuestionNo = 6, Response = "514-966-5487"}, // Sms
            };

            var resp = new MockRespondent
            {
                Language = "fr"
            };

            var facade = JsonUtil.ReadJsonFile<WebRequestFacade>(new FileInfo(configFile));
            Assert.IsNotNull(facade);
           
            await TestFacadeExec(dicAnswers.ToDictionary(c => c.QuestionIdentifier), resp, facade);
        }

        /// <summary>
        /// Test pour la marshalisation des actions vers/de un fichier JSON.
        /// Devrait idéalement contenir un exemple de chaque type existant.
        /// </summary>
        [Test]
        public void TestScreenActionsSerialization()
        {
            var outPut = GetFullOutputDir();
            var jsonFile = new FileInfo(outPut + "screen_actions.json");
            if (jsonFile.Exists)
            {
                jsonFile.Delete();
            }
            jsonFile.Refresh();

            IList<IScreenAction> actions = new List<IScreenAction>()
            {
                new PrintScreenAction
                {
                    QuestionPrintPicture = 3,
                    QuestionNumberOfPrint = 5,
                    FixedPrintPicture = "fixed.jpg",
                    ErrorMessageEn = "en error",
                    ErrorMessageFr = "erreur fr",
                    IsCritical = true,
                    ShowErrorMessage = true
                }
            };

            Assert.IsTrue(JsonUtil.WriteJsonFile(jsonFile, actions));
            jsonFile.Refresh();
            Assert.That(jsonFile.Exists);

            // Maintenant, on lit
            var fromFile = JsonUtil.ReadJsonFile<IList<IScreenAction>>(jsonFile);
            Assert.IsNotNull(fromFile);

            Assert.That(actions, Is.EquivalentTo(fromFile));

            Console.Out.WriteLine("Success: \n\t" + StringUtil.FormatEnumerable(fromFile));
        }
    }
}