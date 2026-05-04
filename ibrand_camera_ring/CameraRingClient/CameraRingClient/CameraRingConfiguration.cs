using System.Runtime.Serialization;
using Newtonsoft.Json;
using NomadLogic.Common.Util;

namespace NomadLogic.iBrand.CameraRingClient
{
    /// <summary>
    /// Classe qui contient les configurations générales pour un CameraRing
    /// </summary>
    [DataContract]
    public class CameraRingConfiguration
    {
        /// <summary>
        /// Indique le nombre de caméra manquante tolérée pour envoyer vers le serveur.
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public int MissingCameraToleranceNumber { get; set; }

        /// <summary>
        /// Emplacement par défaut du fichier de configuration de l'application de Camera Ring.
        /// </summary>
        [JsonIgnore]
        public static string DefaultFileLocation => DirectoryUtil.GetLocalApplicationDir() + "cameraRingConfig.json";

        /// <summary>
        /// Indique le nombre de secondes maximal à attendre pour les fichiers d'une séquence.
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public int CheckFileTimerSeconds { get; set; } = 6;
    }
}