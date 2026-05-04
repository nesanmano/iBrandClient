using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NLog;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Json;
using NomadLogic.Common.Util.Networking.QuestionnaireWebRequest;
using NomadLogic.Common.Util.Networking.QuestionnaireWebRequest.iBrand;
using NUnit.Framework;

// ReSharper disable LocalizableElement

namespace NomadLogic.Test.Common.Util.QuestionnaireWebRequest.Ibrand
{
    public class WebExecutorIbrandTest : TestBaseNunit
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        
        [Test]
        public async Task TestFacadeInitializationFromFile()
        {
            var configFile = BaseDir + "WebRequestFacade" + Path.DirectorySeparatorChar + "ibrand_local.json";
            Assert.That(File.Exists(configFile));

            var facade = JsonUtil.ReadJsonFile<WebRequestFacade>(new FileInfo(configFile)); // Voir Test TestDictionaryDeserialization ci-haut
            Assert.IsNotNull(facade);
            Assert.IsTrue(await facade.Init());
            
            // Teste les configurations de la classe d'exécuteur derrière
            var executor = facade.WebRequestExecutor as WebExecutorIbrand;
            Assert.IsNotNull(executor);

            Assert.AreEqual("ibrand_app", executor.AppName);
            Assert.AreEqual("localhost:8080", executor.Url);
        }

        [Test]
        public async Task TestCondensedCall()
        {
            var configFile = BaseDir + "WebRequestFacade" + Path.DirectorySeparatorChar + "correct_configuration_ibrand.json";
            Assert.That(File.Exists(configFile));
            var facade = JsonUtil.ReadJsonFile<WebRequestFacade>(new FileInfo(configFile));
            Assert.IsTrue(await facade.Init());

            var webParams = new Dictionary<string, object>{ { "questionnaireId", "987" }, { "lastRespondentNumber", "0" } };
            var res = await facade.LaunchMethod<List<IBrandCondensedQuestionnaireAnswer>>("GetCondensedQuestionnaire", webParams);

            Assert.IsNotNull(res);
            if (!res.Any())
            {
                Assert.Ignore("Questionnaire queried has no respondents. Cannot test further. qId=987");
            }
            await Console.Out.WriteLineAsync($"Respondents number: {res.Count}");
            await Console.Out.WriteLineAsync("Examples:");
            var partial = res.Take(10 > res.Count ? res.Count : 10);
            await Console.Out.WriteLineAsync("\t\t" + StringUtil.FormatEnumerable(partial, true, 2));
        }
    }
}
