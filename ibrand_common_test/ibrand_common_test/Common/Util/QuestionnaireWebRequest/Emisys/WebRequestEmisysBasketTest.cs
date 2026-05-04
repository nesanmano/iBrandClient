using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using NLog;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Json;
using NomadLogic.Common.Util.Networking.QuestionnaireWebRequest;
using NomadLogic.Common.Util.Networking.QuestionnaireWebRequest.Emisys;
using NomadLogic.Test.Common.Respondent;
using NUnit.Framework;

namespace NomadLogic.Test.Common.Util.QuestionnaireWebRequest.Emisys
{
    public class WebRequestEmisysBasketTest : TestBaseNunit
    {
        // ReSharper disable once UnusedMember.Local
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        [Test]
        public async Task TestFacadeInitializationFromFile()
        {
            var configFile = BaseDir + "WebRequestFacade" + Path.DirectorySeparatorChar +
                             "emisys_basket_test.json";
            Assert.That(File.Exists(configFile));

            var facade = JsonUtil.ReadJsonFile<WebRequestFacade>(new FileInfo(configFile));
            Assert.IsNotNull(facade);
            Assert.IsTrue(await facade.Init());

            // Teste les configurations de la classe d'exécuteur derrière
            var executor = facade.WebRequestExecutor as WebRequestEmisysBasketTransaction;
            Assert.IsNotNull(executor);
            Assert.AreEqual(2500, executor.Timeout);
            Assert.AreEqual(true, executor.DoNotTestEndPointInit);

            Assert.AreEqual("NomadLogic.Common.Util.QuestionnaireWebRequest.Emisys.WebRequestEmisysBasketTransaction", facade.WebExecutorClassName);
        }

        [Test]
        public async Task TestCreateBasket()
        {
            var configFile = BaseDir + "WebRequestFacade" + Path.DirectorySeparatorChar + "emisys_basket_test.json";
            Assert.That(File.Exists(configFile));

            var facade = JsonUtil.ReadJsonFile<WebRequestFacade>(new FileInfo(configFile)); // Voir Test TestDictionaryDeserialization ci-haut
            Assert.IsNotNull(facade);
            Assert.IsTrue(await facade.Init());

            var resp = new MockRespondent();
            var answers = new Dictionary<QuestionIdentifier, Answer>();

            var resultTmp = await facade.ConstructAndInvokeMethod(resp, answers);

            var result = resultTmp as BasketCommandStatusInfo;

            // tester que le resulat est 1 et cela uniquement à cette ligne 
            Assert.IsNotNull(result);
            
            Console.Out.WriteLine(result);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Id > 0);
            Assert.IsTrue(result.Status == BasketStatusEnum.payment_pending);
            Assert.IsFalse(string.IsNullOrWhiteSpace(result.Signature)); // n'est pas null ni vide.
        }

        [Test]
        public async Task TestFullPrintTransaction()
        {
            InitializePrintConfigToPrinterName("Foxit");

            var imagePrint = BaseDir + "frame_4x6.png";
            EnsureFileExists(imagePrint);

            var webReq = new WebRequestEmisysBasketTransaction()
            {
                DoNotTestEndPointInit = true,
                LoginInfo = new EmisysApiLoginInfo()
                {
                    Apikey = "YhN.R310jD2Sc8kLsPR2",
                    UserEmail = "dev@nomadlogic.com",
                    RootApiUrl = "https://live-test.byemisys.com/api"
                }
            };
            var init = await webReq.InitializeExecutor();

            Assert.IsTrue(init);

            var jsonBody = new BasketCreationInfo()
            {
                PointOfServiceId = 1198,
                TerminalId = "NABOOTH001",
                Products = new List<ProductInfo>
                {
                    new ProductInfo
                    {
                        ProductId = 5797,
                        Quantity = 1
                    }
                }
            };

            var result = await webReq.FullPrintTransaction("B65B5F92", jsonBody, imagePrint);

            // tester que le resulat est 1 et cela uniquement à cette ligne 
            Assert.IsNotNull(result);

            Console.Out.WriteLine(result);

            Assert.IsNotNull(result);
            Assert.IsTrue(result == BasketStatusEnum.payment_accept);
        }
    }
}