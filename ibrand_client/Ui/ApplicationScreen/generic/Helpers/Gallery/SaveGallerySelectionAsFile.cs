using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using NomadLogic.Common;
using NomadLogic.Common.Questionnaire;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Util;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers.Gallery
{
    /// <summary>
    /// Cette classe sauvegarde ou créer une réponse de sélection de gallerie en référençant le fichier d'image directement,
    /// mais au préalable en fait une copie dans le répertoire du projet, puis pointe sur cette copie.
    /// Cette façon de faire empêche que la réponse pointe sur un fichier qui peut ne plus exister, car le fichier
    /// original est géré par la source du questionnaire, et cette source ne fait pas de garantie que les images
    /// existent toujours pour la gallerie (afin de sauver de la mémoire). Donc, les fichiers sélectionnés sont avant tout
    /// copiés dans le répertoire du projet (en esayant de ne pas créer de doublons).
    /// </summary>
    public class SaveGallerySelectionAsFile : ISaveToQuestionnaireFormatter<ComparableFileInfo>
    {
        /// <summary>
        /// Répertoire de sauvegarde des fichiers copiés.
        /// Habituellement réfère à un répertoire de questionnaire, mais ce n'est pas obligatoire.
        /// </summary>
        public DirectoryInfo FinalDataDirectory { get; set; }

        /// <summary>
        /// Le numéro de question où placer la réponse.
        /// </summary>
        [JsonRequired]
        public int QuestionNo { get; set; }

        /// <summary>
        /// Au besoin, le numéro de choix.
        /// </summary>
        public int ChoiceNo { get; set; } = 1;

        /// <summary>
        /// Au besoin, le type énumératif de la question.
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public QuestionTypeEnum QuestionType { get; set; } = QuestionTypeEnum.Text;
        
        public bool Save(ISaveLocalRespondentData saveTo, int index, ComparableFileInfo elem)
        {
            var move = SendFileToQuestionnaireDataDir(elem, out var copiedFile);
            return move && saveTo.SaveAnswerToQuestion(new Answer{QuestionNo = QuestionNo, ChoiceNo = ChoiceNo, Response = copiedFile.FullName});
        }
        
        public IList<Answer> CreateAnswers(int index, ComparableFileInfo elem)
        {
            var move = SendFileToQuestionnaireDataDir(elem, out var copiedFile);
            return !move ? null : new List<Answer>
            {
                new Answer
                {
                    QuestionType = QuestionType,
                    QuestionNo = QuestionNo, 
                    ChoiceNo = ChoiceNo, 
                    Response = copiedFile.FullName
                }
            };
        }

        /// <summary>
        /// Tente de copier le fichier vers le répertoire du questionnaire.
        /// Si le fichier est copié avec succès, ou s'il existe déjà, on retourne vrai et on donne une instance
        /// de FileInfo dans le paramètre de sortie pour identifier le fichier bougé.
        /// </summary>
        [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
        private bool SendFileToQuestionnaireDataDir(ComparableFileInfo elem, out FileInfo fileInfo)
        {
            if (FinalDataDirectory == null || elem?.FileInfoRef?.Exists == false)
            {
                fileInfo = null;
                return false;
            }

            if (!FinalDataDirectory.Exists)
            {
                FinalDataDirectory.Create();
                if (!FinalDataDirectory.Exists)
                {
                    fileInfo = null;
                    return false;
                }
            }

            var fileName = elem.FileInfoRef.Name;
            fileInfo = elem.FileInfoRef.CopyTo(Path.Combine(FinalDataDirectory.FullName, fileName));

            if (fileInfo.Exists)
            {
                return true;
            }

            fileInfo = null;
            return false;
        }
    }
}