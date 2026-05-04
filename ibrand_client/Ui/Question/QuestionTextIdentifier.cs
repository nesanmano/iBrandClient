using JetBrains.Annotations;
using NomadLogic.Common;

namespace NomadLogic.iBrandDesktop.Ui.Question
{
    [PublicAPI]
    public class QuestionTextIdentifier : QuestionText
    {
        public override QuestionTypeEnum QuestionType => QuestionTypeEnum.ExternalUniqueIdentifier;
    }
}
