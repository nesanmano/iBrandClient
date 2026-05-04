using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace NomadLogic.Common.Services.Configuration
{
    /// <summary>
    /// Extension de la configuration de base, pour l'application iBrand Desktop.
    /// </summary>
    [DataContract]
    public class IbrandConfiguration
    {
        /// <summary>
        /// Indique s'il faut copier un fichier vers le répertoire de réception de l'application ShowPictureOnTv, quand possible.
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public bool ShowPreviewOnTv { get; set; }

        /// <summary>
        /// Le nom du fichier de la base de données de iBrand.
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public string DbIbrandFileName { get; set; }

        public void InitWithDefaultValue()
        {
            DbIbrandFileName = "ibrand.db3";
            ShowPreviewOnTv = false;
        }
    }
}
