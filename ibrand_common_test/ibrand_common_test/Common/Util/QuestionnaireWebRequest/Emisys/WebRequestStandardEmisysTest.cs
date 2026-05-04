using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Moq;
using Newtonsoft.Json;
using NLog;
using NomadLogic.Common;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Json;
using NomadLogic.Common.Util.Networking.QuestionnaireWebRequest;
using NomadLogic.Common.Util.Networking.QuestionnaireWebRequest.Emisys;
using NUnit.Framework;

// ReSharper disable LocalizableElement

namespace NomadLogic.Test.Common.Util.QuestionnaireWebRequest.Emisys
{
    public class WebRequestStandardEmisysTest : TestBaseNunit
    {
        // ReSharper disable once UnusedMember.Local
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        [Test]
        public void TestDictionaryDeserialization()
        {
            var json = @"'executorClassName': 'NomadLogic.Common.Util.QuestionnaireWebRequest.Emisys.WebRequestEmisysStandard',
	                    'executorConfiguration': {
		                    'Timeout': 2500,
		                    'DoNotTestEndPointInit': true,
		                    'Login': {
			                    'RootApiUrl': 'https://live-test.byemisys.com/api',
			                    'Apikey': 'Nu/9qmq6e38NFpB..vq6',
			                    'UserEmail': 'api@nomadlogic.com',
			                    'Version': 1
		                    }
	                    },
                        'questionnaireMapper': [
                            {
                                'TargetClass': 'MemberIndividuInfo', 
                                'PropertyName': 'email', 
                                'SaveMethod': 'Text', 
                                'QuestionNo': 2, 
                                'ChoiceNo': 2
                            }
                        ]
                    }";
            var facade = JsonConvert.DeserializeObject<WebRequestFacade>(json);
            // Tests sur les configuration de la facade.
            Assert.AreEqual("NomadLogic.Common.Util.QuestionnaireWebRequest.Emisys.WebRequestEmisysStandard",
                facade.WebExecutorClassName);

            var configMap = facade.WebExecutorConfiguration;
            Assert.IsTrue(configMap.TryGetValue("Timeout", out var obj));
            Assert.AreEqual(2500, obj);
            Assert.IsTrue(configMap.TryGetValue("DoNotTestEndPointInit", out obj));
            Assert.AreEqual(true, obj);

            var mapperList = facade.QuestionnaireMapper;
            Assert.That(mapperList.Count == 1);
            var mapper = mapperList[0];
            Assert.AreEqual("MemberIndividuInfo", mapper.TargetClass);
            Assert.AreEqual("email", mapper.PropertyName);
            Assert.AreEqual(WebExecutorMapperSaveMethod.Text, mapper.SaveMethod);
            Assert.AreEqual(2, mapper.QuestionNo);
            Assert.AreEqual(2, mapper.ChoiceNo);
        }

        [Test]
        public async Task TestFacadeInitializationFromFile()
        {
            var configFile = BaseDir + "WebRequestFacade" + Path.DirectorySeparatorChar +
                             "correct_configuration_emisys.json";
            Assert.That(File.Exists(configFile));

            var facade =
                JsonUtil.ReadJsonFile<WebRequestFacade>(
                    new FileInfo(configFile)); // Voir Test TestDictionaryDeserialization ci-haut
            Assert.IsNotNull(facade);
            Assert.IsTrue(await facade.Init());

            // Teste les configurations de la classe d'exécuteur derrière
            var executor = facade.WebRequestExecutor as WebRequestEmisysStandard;
            Assert.IsNotNull(executor);
            Assert.AreEqual(2500, executor.Timeout);
            Assert.AreEqual(true, executor.DoNotTestEndPointInit);
            Assert.AreEqual("Nu/9qmq6e38NFpB..vq6", executor.LoginInfo.Apikey);
        }

        [Test]
        public async Task TestLaunchMethodEmisysFacade()
        {
            var configFile = BaseDir + "WebRequestFacade" + Path.DirectorySeparatorChar +
                             "correct_configuration_emisys.json";
            Assert.That(File.Exists(configFile));
            var facade = JsonUtil.ReadJsonFile<WebRequestFacade>(new FileInfo(configFile));
            Assert.IsTrue(await facade.Init());

            var webParams = new Dictionary<string, object> {{"cardUid", "3058AECF"}};
            var res = await facade.LaunchMethod<MemberIndividuInfo>("GetMemberIndividuInfoFromCard", webParams);

            Assert.IsNotNull(res);
            Assert.AreEqual("19022", res.Id);
            Assert.AreEqual("Jean Sebastien", res.FirstName);
            Assert.AreEqual("Lessard", res.LastName);
            Assert.AreEqual("jslessard@nomadlogic.com", res.UserName);
            Assert.AreEqual("jslessard@nomadlogic.com", res.Email);

            // Sauvegarde dans un questionnaire mock
            var test = new Mock<ISaveLocalRespondentData>();
            facade.SaveDataToQuestionnaire(test.Object, res);

            test.Verify(q => q.SaveAnswerToQuestion(new QuestionIdentifier(2,2, QuestionTypeEnum.Text), "jslessard@nomadlogic.com"), Times.Exactly(1));
        }

        [Test]
        public async Task TestLaunchMethodEmisysFacade_SQU201801()
        {
            var configFile = BaseDir + "WebRequestFacade" + Path.DirectorySeparatorChar + "SQU201801.json";
            Assert.That(File.Exists(configFile));
            var facade = JsonUtil.ReadJsonFile<WebRequestFacade>(new FileInfo(configFile));
            Assert.IsTrue(await facade.Init());

            var webParams = new Dictionary<string, object> { { "cardUid", "8793310E" } };
            var res = await facade.LaunchMethod<MemberIndividuInfo>("GetUserIndividuInfoFromCard", webParams);

            Assert.IsNotNull(res);

            /*
            // Sauvegarde dans un questionnaire mock
            var test = new Mock<ISaveAnswerInQuestionnaire>();
            facade.SaveDataToQuestionnaire(test.Object, res);

            test.Verify(q => q.SaveAnswerToQuestion(2, 2, "jslessard@nomadlogic.com", -1), Times.Exactly(1));
            */
        }
    }
}
