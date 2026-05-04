using NomadLogic.Common;
using NomadLogic.iBrandDesktop.Ui.Choice;
using System;

namespace NomadLogic.iBrandDesktop.Ui.Question
{
    public class QuestionCameraRing : QuestionFileBase
    {
        public override Type GetDefaultChoiceClassType()
        {
            return typeof(ChoiceCameraRing);
        }

        public override QuestionTypeEnum QuestionType => QuestionTypeEnum.PhotoBundle;
    }
}
