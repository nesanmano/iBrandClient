using System.Collections.Generic;
using System.IO;

namespace NomadLogic.Multimedia.Cli.ImageMagickCLI
{
    /// <summary>
    /// Cette structure regroupe les différents types de fichiers lors d'une transformation en jetons d'une commande en ligne de commande.
    /// Permet de regrouper les différents fichier intrans extrant (ou simplement valeur intrantes) utilisées lors d'une exécution d'un
    /// script par ligne de commandes paramétrables.
    /// </summary>
    public class CliTmpFileDictionaryRefs
    {
        internal Dictionary<string, FileInfo> TempRefs { get; } = new Dictionary<string, FileInfo>();
        internal Dictionary<string, FileInfo> OutRefs { get; } = new Dictionary<string, FileInfo>();
        internal Dictionary<string, string> InRefs { get; set; } = new Dictionary<string, string>();
    }
}