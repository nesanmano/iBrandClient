using System.IO;
using JetBrains.Annotations;
using Newtonsoft.Json;
using NomadLogic.Common.Util;

namespace NomadLogic.Common.Serveur
{
    public class ResourceFileToDownload
    {
        public ResourceFileToDownload() {}

        public ResourceFileToDownload([NotNull] FileInfo f)
        {
            LocalFileInfo = f;
            Src = f.Name;
            Type = "file";
            ServerModifiedTime = 1;
            FileSize = f.SizeInBytes();
        }

        /// <summary>
        /// Référence locale (au besoin, quand on utilise du matériel local au lieu d'une définition de fichier externe).
        /// </summary>
        [JsonIgnore]
        public FileInfo LocalFileInfo { get; }

        /// <summary>
        /// le nom du fichier comme il existe sur le serveur
        /// </summary>
        [JsonProperty("src")]
        public string Src { get; set; }
        
        /// <summary>
        /// Le type du fichier. Méta-donnée de l'utilisation du fichier.
        /// </summary>
        [JsonProperty("type")]
        public string Type { get; set; }

        /// <summary>
        /// Valeur de la date de dernière modification sur le serveur (en format JAVA Epoch).
        /// </summary>
        [JsonProperty("lastModifiedDate")]
        public long ServerModifiedTime { get; set; }
        
        /// <summary>
        /// Nombre d'octets du fichier.
        /// </summary>
        [JsonProperty("size")]
        public long FileSize { get; set; }

        /// <summary>
        /// Action à faire pour mettre à jour cet élément.
        /// </summary>
        public RessourceFileAction Action { get; set; } = RessourceFileAction.Unknown;

        public override string ToString()
        {
            return this.ToStringPublicProperties();
        }

        /// <summary>
        /// Retourne une nouvelle instance avec les même valeurs.
        /// </summary>
        public ResourceFileToDownload Copy()
        {
            var tmp = new ResourceFileToDownload
            {
                FileSize = FileSize,
                Action = Action,
                Type = Type,
                ServerModifiedTime = ServerModifiedTime,
                Src = Src
            };

            return tmp;
        }

        public static RessourceFileAction PrioritizeAction(RessourceFileAction newAction, RessourceFileAction oldAction)
        {
            return (int)newAction > (int)oldAction ? newAction : oldAction;
        }
    }

    /// <summary>
    /// Actions possibles pour faire converger deux listes.
    /// Si deux actions doivent être priorisées entre elles, on choisit celle qui est plus haut dans cette liste,
    /// donc, celle qui a la valeur la plus élevée.
    /// </summary>
    public enum RessourceFileAction
    {
        Add = 4,
        Update = 3,
        Remove = 2,
        Ignore = 1,
        Unknown = 0
    }
}
