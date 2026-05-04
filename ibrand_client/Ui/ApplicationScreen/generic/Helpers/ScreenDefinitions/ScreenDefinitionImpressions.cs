using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using JetBrains.Annotations;
using NLog;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Ui.NomadScreen;
using NomadLogic.Common.Util.Print;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers.ScreenDefinitions
{
    [PublicAPI]
    public class ScreenDefinitionImpressions : IOnLoad, IDisposable, IActivateScreen
    {
        public IList<ScreenDefinitionImpressionBase> ImpressionDefinitions { get; set; }
        
        public bool OnLoad(AbstractScreen screen)
        {
            return ImpressionDefinitions.Aggregate(true, (current, defs) => current & (defs?.OnLoad(screen) ?? false));
        }

        public void Dispose()
        {
            foreach (var defs in ImpressionDefinitions)
            {
                defs?.Dispose();
            }
        }

        public void Activate(Control parent, IRespondent respondent)
        {
            foreach (var defs in ImpressionDefinitions)
            {
                defs?.Activate(parent, respondent);
            }
        }
    }

    [PublicAPI]
    public class ScreenDefinitionImpressionBase : ScreenDefinitionButtonClicBase
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        protected string _fileToPrint;

        protected override bool PrepareExecution()
        {
            _fileToPrint = null;
            _fileToPrint = DataModelSource.GetActivationTextValue(_curResp);

            if (File.Exists(_fileToPrint)) return true;
            log.Warn("File does not exists: " + _fileToPrint);
            return false;

        }

        protected override bool ExecuteMainLogic()
        {
            if (File.Exists(_fileToPrint)) {
                log.Warn("File does not exists: " + _fileToPrint);
                return false;
            }

            var exec = PrintUtil.Print(_fileToPrint, null, _btn);
            _fileToPrint = null;
            return exec;
        }
    }
}