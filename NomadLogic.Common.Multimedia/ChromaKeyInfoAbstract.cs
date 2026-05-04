using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace NomadLogic.Multimedia
{
    /// <summary>
    /// Indique quel type de caméra utiliser pour saisir des photos.
    /// Plus utile dans le UI.
    /// </summary>
    public enum ChromaKeyInfoCameraVendorEnum
    {
        /// <summary>
        /// Caméra du manufacturier Canon
        /// </summary>
        Canon,
        /// <summary>
        /// Caméra du manufacturier Sony
        /// </summary>
        Sony
    }

    /// <summary>
    /// Classe de base pour toutes les configurations de ChromaKeying
    /// </summary>
    [DataContract]
    public abstract class ChromaKeyInfoAbstract : IEquatable<ChromaKeyInfoAbstract>
    {
        /// <summary>
        /// Le vendeur du type de caméra à utiliser pour saisir les images.
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        [JsonConverter(typeof(StringEnumConverter))]
        public ChromaKeyInfoCameraVendorEnum CameraVendor { get; set; }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            return obj is ChromaKeyInfoAbstract arg && arg.CameraVendor == CameraVendor;
        }

        /// <inheritdoc/>
        public bool Equals(ChromaKeyInfoAbstract other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return CameraVendor.Equals(other.CameraVendor);
        }

        /// <inheritdoc/>
        [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
        public override int GetHashCode()
        {
            return CameraVendor.GetHashCode();
        }
    }

}