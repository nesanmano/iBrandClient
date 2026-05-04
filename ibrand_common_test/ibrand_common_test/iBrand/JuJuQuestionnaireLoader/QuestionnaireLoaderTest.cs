using System.IO;
using Newtonsoft.Json;
using NomadLogic.Common.Serveur;
using NomadLogic.Common.Util;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen;
using NUnit.Framework;

namespace NomadLogic.Test.iBrand.JuJuQuestionnaireLoader
{
    class QuestionnaireLoaderTest : TestBaseNunit
    {

        [Test]
        public void LoadJuJuQuestionnaire()
        {

            var sc = new ScreenConfig(70, 132, 0)
            {
                ProjectNo = "m8bv5g5jm1",
                EventId = 1029,
                ForceFileCheck = true,
                QuestionFirstPictureNo = 1
            };

            var qInfo = sc.GenerateQuestionnaireInfo("questionnaire.json", "1.0");

            var project = sc.GenerateProject();

            var eveent = sc.GenerateEvent();

            project.AddQuestionnaireInfo(qInfo);

            // TODO Utiliser une façon complète de tester la classe "JujuQuestionnaireLoader"
            Assert.Fail();

            /*
            var fileName = DirectoryUtil.GetLocalApplicationDir(DirectoryUtil.ApplicationNameEnum.iBrand) + qInfo.FileSrc;
            Assert.IsTrue(File.Exists(fileName));
            string json = FileUtil.ReadFileToStringSingleLine(fileName);

            var obj = JsonConvert.DeserializeObject<TerminalQuestionnaireInfo>(json);

            Assert.NotNull(obj);
            */
        }
    }
}
