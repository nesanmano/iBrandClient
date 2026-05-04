using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NLog;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Json;
using NomadLogic.Common.Util.Networking.QuestionnaireWebRequest;
using NomadLogic.Common.Util.Networking.QuestionnaireWebRequest.Emisys;
using NUnit.Framework;
using RestSharp;

// ReSharper disable LocalizableElement

namespace NomadLogic.Test.Common.Util.QuestionnaireWebRequest.Emisys
{
    public class WebRequestEmisysPayWallTest : TestBaseNunit
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        [Test]
        public void TestDictionaryDeserialization()
        {
            var json = @"{
                        'executorClassName': 'NomadLogic.Common.Util.QuestionnaireWebRequest.Emisys.RestServiceConsumerEmisys',
                        'executorConfiguration': {
                            'Timeout': 2500,
                            'DoNotTestEndPointInit': true,
		                    'Login' : 'TestProject2017'
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
            Assert.AreEqual("NomadLogic.Common.Util.QuestionnaireWebRequest.Emisys.RestServiceConsumerEmisys",
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
            var executor = facade.WebRequestExecutor as WebRequestEmisysPayWall;
            Assert.IsNotNull(executor);
            Assert.AreEqual(2500, executor.Timeout);
            Assert.AreEqual(true, executor.DoNotTestEndPointInit);

            Assert.AreEqual("NomadLogic.Common.Util.QuestionnaireWebRequest.Emisys.WebRequestEmisysPayWall", facade.WebExecutorClassName);

            var mapping = facade.QuestionnaireMapper;
            Assert.IsNotNull(mapping);
            Assert.AreEqual(14, mapping.Count);

            Assert.AreEqual(2, mapping[13].ChoiceNo);
            Assert.AreEqual(2, mapping[13].QuestionNo);
        }

        private Task<FinalizeCardInfo> CoreFinalizeUnitTest(WebRequestFacade facade,string uid)
        {
            var webParams = new Dictionary<string, object> { { "cardUid", uid } };

            return facade.LaunchMethod<FinalizeCardInfo>("FinalizeCard", webParams);

        }

        private Task<ResponseInitCardInfo> CoreInitUnitTest(WebRequestFacade facade, string uid, PostInitCardInfo postData)
        {
            var webParams = new Dictionary<string, object> { { "cardUid", uid }, { "bodyData", postData} };

            return facade.LaunchMethod<ResponseInitCardInfo>("InitCard", webParams);
        }

        /*
        [Test]
        public async Task TestAddItemMethodEmisysFacade()
        {
            var configFile = BaseDir + "WebRequestFacade" + Path.DirectorySeparatorChar + "correct_configuration_emisys.json";
            Assert.That(File.Exists(configFile));

            var facade =
                FileUtil.ReadJsonFile<WebRequestFacade>(
                    new FileInfo(configFile)); // Voir Test TestDictionaryDeserialization ci-haut
            Assert.IsNotNull(facade);
            Assert.IsTrue(await facade.Init());

            PayloadBodyinfo pbi = new PayloadBodyinfo()
            {
                AntennaNum = 2,
                CardUid = "2461F82E"
            };

            var webParams = new Dictionary<string, object> {{"wallId", "608"}, { "bodyData", pbi } };

            var res = await facade.LaunchMethod<HttpStatusCode>("AddItemToCart", webParams);

            Assert.IsNotNull(res);

            Assert.AreEqual(res, HttpStatusCode.OK);
        }
        */
        
        // Init-01
        [Test]
        public async Task TestFacadeWithRealUidEmisysFacadeInit01()
        {
            var configFile = BaseDir + "WebRequestFacade" + Path.DirectorySeparatorChar + "correct_configuration_emisys.json";
            Assert.That(File.Exists(configFile));

            var facade = JsonUtil.ReadJsonFile<WebRequestFacade>(new FileInfo(configFile)); // Voir Test TestDictionaryDeserialization ci-haut
            Assert.IsNotNull(facade);
            Assert.IsTrue(await facade.Init());


            string[] uids = { "2461F82E" };
            ResponseInitCardInfo resultat = null;

            var postdata = new PostInitCardInfo
            {
                Email = "km@gmail.com",
                Language = "EN",
                Amount = 140
            };

            foreach (var uid in uids)
            {
                resultat = await CoreInitUnitTest(facade, uid,postdata);
            }
            // tester que le resulat est 1 et cela uniquement à cette ligne 
            Assert.IsNotNull(resultat);
            Assert.That(resultat.Result == 0 || resultat.Result == 3);
            Assert.AreEqual("success", resultat.Status);
            Assert.GreaterOrEqual(resultat.Balance, 0.0);
            Assert.GreaterOrEqual(resultat.Username,0);  // retourne un user.ID

        }

        [Test]
        public async Task TestEmisysApi()
        {
            var dico = new Dictionary<string, string>
            {
                {"3294951398", "E6F764C4"},
                {"2473911745", "C1E57493"},
                {"2475852305", "11829293"},
                {"2471758481", "910A5493"},
                {"2474210673", "71757993"},
                {"2474086945", "21927793"},
                {"2470960225", "61DC4793"},
                {"2473908657", "B1D97493"},
                {"1112743577", "991E5342"},
                {"2476401409", "01E39A93"},
                {"2476422657", "01369B93"}
            };

            var iter = dico.GetEnumerator();

            while (iter.MoveNext())
            {
                var facade = CreateFacade();
                Assert.That(await facade.Init());
                var res = await TestDia201802InitCard(facade, iter.Current.Value);
                Console.WriteLine(iter.Current.Value+ " request succeeded." + " result returned: "+ res.Result);
                //Assert.AreNotEqual(res.Result,0);
            }

            iter.Dispose();
                
        }

        private WebRequestFacade CreateFacade()
        {
            var configFile = BaseDir + "WebRequestFacade" + Path.DirectorySeparatorChar + "dia201802_emisys.json";
            //Assert.That(File.Exists(configFile));

            WebRequestFacade facade = null;
            if(File.Exists(configFile))
                facade = JsonUtil.ReadJsonFile<WebRequestFacade>(new FileInfo(configFile));

            return facade;
        }

        [Test]
        public async Task TestDia201802InitCard()
        {
            var nbrs = new List<string>
            {
                /*
                "9371DC01",
                "939CA501",
                "936F4431",
                "937819B1",
                "9374D9B1",
                "939991A1",
                "939893E1",
                "9374CDD1",
                "936F3F11",
                "93797571",
                "937492D1",
                "93931EB1",
                "42531E99",
                "937236D1",
                "93524911",
                "93747E91",
                "937554D1",
                "936F43E1",
                "939996E1",
                "93973071",
                "93540A91",
                "93989CD1",
                "422FD4A9",
                "93929AD1",
                "93969011",
                "9347DC61",
                "93972FF1",
                "93792A81",
                "92A48D61",
                */
                "93524911" // D1DC7793 - 3520886675
                // 2474106065 - 9377DCD1
            };
            var facade = CreateFacade();
            Assert.That(await facade.Init());

            foreach (var nbr in nbrs)
            {
                var res = await TestDia201802InitCard(facade, nbr);
                Console.Out.WriteLine($"{nbr} => " + res);
                Assert.IsNotNull(res != null);
            }
        }
   
        private async Task<ResponseInitCardInfo> TestDia201802InitCard(WebRequestFacade facade, string cardUid)
        {
            var postData = new PostInitCardInfo
            {
                Amount = 6789,
                Email = "jslessard@test.com",
                Language = "fr",
                IsAgreeContestRules = "true",
                IsLegalAge = "true",
                IsTdClient = "true",
                Lastname = "Lessard",
                Firstname = "JS",
                LastnameEmisys = "Lessard",
                FirstnameEmisys = "JS",
                LifeStyleCode = 3,
                Sms = "9876543215",
                ZipCode = "a3a3a3"
            };
            var webParams = new Dictionary<string, object>
            {
                { "cardUid", cardUid },
                { "bodyData", postData},
                { "webMethod", Method.Get }
            };
            
            var res = await facade.LaunchMethod<ResponseInitCardInfo>("InitCard", webParams);

            Assert.IsNotNull(res);
            
            return res;
        }

        [Test]
        public async Task TestDia201802FinalizeCard()
        {
            var configFile = BaseDir + "WebRequestFacade" + Path.DirectorySeparatorChar + "dia201802_emisys.json";
            Assert.That(File.Exists(configFile));

            var facade = JsonUtil.ReadJsonFile<WebRequestFacade>(new FileInfo(configFile));
            Assert.IsNotNull(facade);
            Assert.IsTrue(await facade.Init());

            var webParams = new Dictionary<string, object> { { "cardUid", "9377DCD1" } };

            var res = await facade.LaunchMethod<FinalizeCardInfo>("FinalizeCard", webParams);

            Assert.IsNotNull(res);

            Console.Out.WriteLine(res.ToString());
        }
    }
}
