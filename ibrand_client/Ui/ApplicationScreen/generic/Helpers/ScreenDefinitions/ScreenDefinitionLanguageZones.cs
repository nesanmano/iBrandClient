using System.Collections.Generic;
using NLog;
using NomadLogic.Common.Ui.NomadScreen;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers.ScreenDefinitions
{
    public class ScreenDefinitionLanguageZones : ScreenDefinitionSelectionBase<LanguageZone>
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        public override string DefaultConfigFileName { get; set; } = "language_definition_screen.json";

        protected override void AddActions(AbstractScreen screen, LanguageZone hit, List<ScreenEvaluationResult> addTo)
        {
            if (screen is ILanguageScreenInterface languageScreen)
            {
                addTo.Add(new ScreenEvaluationResult(() =>
                {
                    languageScreen.LanguageSelection(hit.LanguageCode);
                }));
            }
            else
            { 
                log.Warn("Tentative de changer la langue dans un écran qui ne supporte pas cette fonctionalité.");
            }
        }
    }
}