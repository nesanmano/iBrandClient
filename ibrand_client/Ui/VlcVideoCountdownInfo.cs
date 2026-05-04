using System.Collections.Generic;
using System.Linq;

namespace NomadLogic.iBrandDesktop.Ui
{
    public class VlcVideoCountdownInfo
    {
        /// Le moment du décompte auquel il faut lancer la commande.
        /// Si le décompte ne peut atteindre cette valeur, la commande ne sera jamais lancée.
        public int StartTime { get; set; }

        /// Fichiers à jouer, indexés par le code de langue actif. Utiliser la méthode "GetFileToPlay(string)"
        /// pour la logique d'avoir un fichier par défaut.
        public IDictionary<string, string> FilesToPlay { get; set; }

        /// <summary>
        /// Retourne le fichier indiqué par le code de langue. Si inexistant, prend le premier fichier spécifié dans le dictionnaire.
        /// Retourne null uniquement si le dictionanire est nul ou vide.
        /// </summary>
        /// <param name="languageCode">Le code de langue (clé) du fichier désiré.</param>
        /// <returns>Le fichier associé à la langue. SI pas d'association, retourne le premier fichier indexé par les clés.
        /// Peut retourner nul si dictionnaire ne contient rien.</returns>
        public string GetFileToPlay(string languageCode)
        {
            if (FilesToPlay == null || FilesToPlay.Keys.Count == 0) return null;

            var success = FilesToPlay.TryGetValue(languageCode, out var file);

            if (success)
            {
                return file;
            }

            // On fait un retour sur le premier indice du dictionnaire.
            var firstKey = FilesToPlay.Keys.First();
            return FilesToPlay[firstKey];
        }
    }
}