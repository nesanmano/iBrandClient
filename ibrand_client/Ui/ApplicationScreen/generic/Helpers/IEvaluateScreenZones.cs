using System;
using JetBrains.Annotations;
using NomadLogic.Common.Ui.NomadScreen;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers
{
    public interface IEvaluateScreenZones
    {
        ScreenEvaluationResult EvaluateZones([NotNull] AbstractScreen screen, int x, int y, Func<ScreenEvaluationResult> validationMethod);
    }
}