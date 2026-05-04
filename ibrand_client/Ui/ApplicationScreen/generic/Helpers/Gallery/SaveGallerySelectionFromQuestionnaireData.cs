using System.Collections.Generic;
using System.IO;
using System.Linq;
using NomadLogic.Common;
using NomadLogic.Common.Questionnaire;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Services.Database;
using NomadLogic.Common.Util;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers.Gallery
{
    /// <summary>
    /// Cette classe assume que les fichiers d'image choisis possède une nomenclature spéciale qui permet d'identifier un identifiant serveur de répondent.
    /// On se fout de l'index ici, uniquement les informations contenues dans les tables de réponses condensées sont utiles.
    /// </summary>
    public class SaveGallerySelectionFromQuestionnaireData : ISaveToQuestionnaireFormatter<ComparableFileInfo>
    {
        public ICondensedAnswerDatabase CondensedAnswerDatabase { get; set; }

        public IDictionary<QuestionIdentifier,QuestionIdentifier> QuestionMapping { get; set; }

        public QuestionIdentifier PivotQuestion { get; set; }

        public bool Save(ISaveLocalRespondentData saveTo, int index, ComparableFileInfo elem)
        {
            var answers = GetData(elem);
            return answers.All(saveTo.SaveAnswerToQuestion);
        }

        private List<Answer> GetData(ComparableFileInfo elem)
        {
            var listFinal = new List<Answer>();

            var id = ExtractServerRespondentIdFromFileName(elem.FileInfoRef);
            var answers = CondensedAnswerDatabase.GetCondensedAnswersFromRespondent(id);

            if (PivotQuestion != null)
            {
                listFinal.Add(PivotQuestion.CreateAnswer(id.ToString()));
            }

            if (QuestionMapping != null)
            {
                using (var ite = QuestionMapping.GetEnumerator())
                {
                    while (ite.MoveNext())
                    {
                        var kvp = ite.Current;
                        var ca = answers.FirstOrDefault(a => a.QuestionNo == kvp.Key.QuestionNo && a.ChoiceNo == kvp.Key.ChoiceNo); // Pas de type, pas de sous-choix
                        if (ca == null) continue;

                        // TODO à modifier si une question récepteur de type fichier à besoin d'autre chose que le fichier local.
                        listFinal.Add(kvp.Value.QuestionType.IsFileType() ? kvp.Value.CreateAnswer(ca.LocalFile) : kvp.Value.CreateAnswer(ca.Response));
                    }
                }
            }

            return listFinal;
        }

        public IList<Answer> CreateAnswers(int index, ComparableFileInfo elem)
        {
            return GetData(elem);
        }

        private int ExtractServerRespondentIdFromFileName(FileSystemInfo file)
        {
            // le format de sauvegarde est "{ticks_date}_{identifiant répondant serveur}_{nom fichier + extension}"
            var parts = file.Name.Split('_');
            return int.TryParse(parts[1], out var srId) ? srId : 0;
        }
    }
}