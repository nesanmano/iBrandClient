using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Json;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic;
using NUnit.Framework;

namespace NomadLogic.Test.Common.PresentationScreens
{
    public class PresentationScreenTest : TestBaseNunit
    {

        [Test]
        public void UpdateInstanceFromJson()
        {
            var inputFile = BaseDir + @"\presentationScreen\testHomeScreenUpdate.json";

            var home = new HomeScreen(BaseDir + @"\intro.jpg", new ScreenConfig(1,1, 1));
            
            Assert.IsTrue(home.QuestionLanguageNo == null);

            JsonUtil.UpdateFromJson(inputFile, home);

            Assert.IsTrue(home.QuestionLanguageNo == 1);
        }
    }
}
