using System.Collections.Generic;
using System.Linq;
using NomadLogic.Common.Util;

namespace NomadLogic.iBrandDesktop
{
    public class Project
    {
        public Project(int id, string name)
        {
            Id = id;
            Name = name;
        }

        public Project(int id, string name, string no)
        {
            Id = id;
            Name = name;
            No = no;
        }

        public string Name { get; }

        public string No { get; }

        public int Id { get; }
        
        public string ProjectDir => DirectoryUtil.GetLocalIbrandProjectDir(Id);

        public void AddQuestionnaireInfo(QuestionnaireInfo qi)
        {
            if (QuestionnaireInfos == null)
                QuestionnaireInfos = new List<QuestionnaireInfo>();

            QuestionnaireInfos.Add(qi);
        }

        public List<QuestionnaireInfo> QuestionnaireInfos { get; private set; }

        /// <summary>
        /// On ne veut pas d'autres références à des questionnaires que celui spécifier. S'il n'existe pas, on vide la liste et
        /// on retourne nul.
        /// </summary>
        /// <param name="terQuestionnaireId">l'identifiant du questionnaire à conserver, à l'exclusion de tout autre.</param>
        /// <returns>La référence vers ce questionnaire.</returns>
        public QuestionnaireInfo KeepOnlySpecificQuestionnaire(int terQuestionnaireId)
        {
            var newList = new List<QuestionnaireInfo>();

            if (QuestionnaireInfos == null)
            {
                QuestionnaireInfos = newList;
                return null;
            }

            newList.AddRange(QuestionnaireInfos.Where(qi => qi.Id == terQuestionnaireId)); // Normalement, un seul.
            QuestionnaireInfos = newList;

            return newList.FirstOrDefault();
        }
    }
}
