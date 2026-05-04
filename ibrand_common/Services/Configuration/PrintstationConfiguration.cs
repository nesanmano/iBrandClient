using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace NomadLogic.Common.Services.Configuration
{
    /// <summary>
    /// Extension de la configuration de base, pour l'application Printstation.
    /// </summary>
    [DataContract]
    public class PrintstationConfiguration
    {
        /// <summary>
        /// Si le questionnaire actif dirige vers une gallerie externe.
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public bool LinkToGallery { get; set; }

        /// <summary>
        /// Spécifie qu'il faut ignorer les images dé¸jà existantes et ne chercher sur le serveur que les images plus récentes que
        /// le démarrage de l'application.
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public bool StartLookingFromLastPost { get; set; }

        /// <summary>
        /// Indique qu'il faut ouvrir les ports de communications TCP pour recevoir des fichiers directement d'une autre application.
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public bool ListenForPicturesFromIBrand { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public bool DontLookForPost { get; set; }

        /// <summary>
        /// Indique qu'il faut immédiatement imprimer le fichier d'impression une fois le répondant créé.
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public bool AutomaticPrint { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public bool AnimatedGif { get; set; }

        /// <summary>
        /// Emplacement du fichier de base de données de l'application Printstation.
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public string DbPrintStationFileName { get; set; }
        
        public void InitWithDefaultValue()
        {
            DbPrintStationFileName = "print_station.db3";
            
            StartLookingFromLastPost = false;
            ListenForPicturesFromIBrand = false;
            DontLookForPost = false;
            AutomaticPrint = false;
            AnimatedGif = false;
            LinkToGallery = false;
        }
    }
}
