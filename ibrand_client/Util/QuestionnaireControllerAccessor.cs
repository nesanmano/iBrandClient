using System;
using NomadLogic.iBrandDesktop.Ui;

namespace NomadLogic.iBrandDesktop.Util
{
    /// <summary>
    /// Accesseur statique. Très mauvais design. À enl;ever dès que possible.
    /// </summary>
    [Obsolete]
    public static class QuestionnaireControllerAccessor
    {
        public static ScreenController QuestionnaireController { get; set; }
        public static TouchScreenController TouchQuestionnaireController { get; set; }
    }
}
