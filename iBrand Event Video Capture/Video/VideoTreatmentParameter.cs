using System.Collections.Generic;
using System.IO;

namespace NomadLogic.App.EventVideoCapture.Video
{
    public class VideoTreatmentParameter
    {
        /// <summary>
        /// Le répertoire de travail. Contient les fichiers originaux et devrait contenir tous les fichiers temporaires aussi.
        /// </summary>
        public DirectoryInfo WorkDirectory { get; set; }

        /// <summary>
        /// Identifiant unique externe pour lier les fichiers créés à un répondant unique.
        /// </summary>
        public string RespondentUniqueId { get; set; }

        /// <summary>
        /// Données supplémentaire, de forme "clé=valeur", pour envoyer des informations supplémentaires au travailleur.
        /// </summary>
        public Dictionary<string, string> AdditionalData { get; set; }
    }
}