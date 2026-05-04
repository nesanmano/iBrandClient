using System.Collections.Generic;
using System.Windows.Forms;
using JetBrains.Annotations;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Ui.NomadScreen;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Json;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers.ScreenDefinitions
{
    public class ScreenDefinitionDebug: IJsonConfig, IExternalFileConfig, IActivateScreen
    {

        public IList<Answer> DefaultAnswers { get; set; }

        public string DefaultConfigFileName { get; set; } = "DebugConfigFileName";

        public string ConfigFileName { get; set; }
        
        public void Activate([NotNull] Control parent, IRespondent respondent)
        {
            if (!(parent is GenericScreen screen)) return;

            screen.SaveAnswers(DefaultAnswers);

        }
    }
}
