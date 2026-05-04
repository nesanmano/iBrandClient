using NomadLogic.Common.Serveur;
// ReSharper disable InconsistentNaming

namespace NomadLogic.iBrandDesktop
{
    public class IBrandQuestionnaire : Questionnaire
    {
        public IBrandQuestionnaire(int id, string languages, string projectPath)
        {
            InitQuestionnaire(id, languages, projectPath);
        }

    }
}
