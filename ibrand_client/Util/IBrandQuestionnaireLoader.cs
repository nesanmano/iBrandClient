using System;

namespace NomadLogic.iBrandDesktop.Util
{
    /// <summary>
    /// TODO: Cette classe ne devrait plus exister. Seul le loader Juju devrait désormais.
    /// </summary>
    [Obsolete]
    public class IBrandQuestionnaireLoader : QuestionnaireLoaderAbstract
    {

        public IBrandQuestionnaireLoader(QuestionnaireInfo questionnaireInfo, Project project)
        {
            QuestionnaireLoader(questionnaireInfo, project);
        }

        public override Questionnaire Load(bool value)
        {
            return LoadQuestionnaire(value);
        }
    }
}
