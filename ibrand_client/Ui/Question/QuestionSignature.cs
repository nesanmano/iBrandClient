using System;
using NomadLogic.Common;
using NomadLogic.iBrandDesktop.Ui.Choice;

namespace NomadLogic.iBrandDesktop.Ui.Question
{
    public class QuestionSignature : QuestionFileBase
    {
        public override Type GetDefaultChoiceClassType()
        {
            return typeof(ChoiceSignature);
        }

        public override QuestionTypeEnum QuestionType => QuestionTypeEnum.Signature;
    }
}
