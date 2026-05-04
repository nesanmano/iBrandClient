using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace NomadLogic.Common.Serveur
{
    /// <summary>
    /// Structure JSON pour les informations de base d'un terminal. Inclu ses identifiants côté serveur et
    /// les fichiers à aller chercher pour le faire fonctionner localement.
    /// </summary>
    public class TerminalDownloadInfo : TerminalInfo
    {
        // Ne pas oublier de voir les champs de "TerminalInfo"!

        /// <summary>
        /// La liste de fichiers à télécharger pour ce terminal.
        /// </summary>
        [JsonProperty("filesToDownload")]
        public IList<ResourceFileToDownload> FilesToDownload { get; set; }

        /// <summary>
        /// Vérifie si les données reçues sont valides.
        /// </summary>
        /// <param name="errorMessage">Message d'erreur, si jamais la validation échoue.</param>
        /// <returns>TRUE si tout est valide.</returns>
        public bool Validate(out string errorMessage)
        {
            errorMessage = null;

            if (FilesToDownload == null || !FilesToDownload.Any())
            {
                errorMessage = "No files to download.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(LoaderFile) || !FilesToDownload.Any(ftd => ftd.Src.Equals(LoaderFile)))
            {
                errorMessage = "No matching main file in list.";
                return false;
            }

            if (ProjectId == 0 || QuestionnaireId == 0 || TerminalId == 0)
            {
                errorMessage = "Invalid IDs. The project, questionnaire et terminal IDs cannot be zero.";
                return false;
            }

            return true;
        }
    }
}
