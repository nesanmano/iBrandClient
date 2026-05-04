using System;
using System.Collections.Generic;
using System.Linq;
using NomadLogic.Common.Ui.NomadScreen;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Json;
using NomadLogic.Common.Util.Math;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers.ScreenDefinitions
{
    public class ScreenDefinitionNavigation : IJsonConfig, IExternalFileConfig, IEvaluateScreenZones
    {
        public string DefaultConfigFileName { get; set; } = "screen_navigation_definition.json";

        public string ConfigFileName { get; set; }

        public IList<SectionSkipScreenData> ZoneList { get; set; }
        
        public ScreenEvaluationResult EvaluateZones(AbstractScreen screen, int x, int y, Func<ScreenEvaluationResult> validationMethod)
        {
            var hit = ZoneList?.FirstOrDefault(data => MathUtil.InArea(x, y, data.Zone));
            if (hit == null) return null;
            if (!hit.SkipValidation)
            {
                var result = validationMethod?.Invoke();
                if (result != null && result.Result != EvaluationResult.Valid)
                {
                    return result;
                }
            }

            if (hit.CustomActionOnActivation != null)
            {
                return new ScreenEvaluationResult
                {
                    ActionsToExecute = new List<Action> {hit.CustomActionOnActivation}
                };
            }

            return new ScreenEvaluationResult
            {
                ActionsToExecute = new List<Action> {() => ExecuteSco(screen, hit)}
            };
        }

        public void ExecuteSco(AbstractScreen screen, SectionSkipScreenData hit)
        {
            var sco = hit.PageChangeOptions.Evaluate(screen.GetCurrentRespondentWithAnswers());
            if (sco == null)
            {
                return;
            }

            if (sco.Cancel)
            {
                screen.FirstScreen();
                return;
            }

            screen.Next(sco.Step);
        }
    }
}