using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
// ReSharper disable InconsistentNaming

namespace NomadLogic.Common.Util.Networking.QuestionnaireWebRequest.ConnectAndGo.Schemas.v1
{
    [PublicAPI]
    public class Media
    {
        // Ce sont des formats invalide pour utiliser une énumération.
        public static readonly string MEDIA_TYPE_MP4 = "video/mp4";
        public static readonly string MEDIA_TYPE_GIF = "image/gif";
        public static readonly string MEDIA_TYPE_JPG = "image/jpg";
        public static readonly string MEDIA_TYPE_PNG = "image/png";

        [JsonProperty("type")]
        [JsonRequired]
        public string MediaType { get; set; }

        [JsonProperty("url")]
        [JsonRequired]
        public string MediaUrl { get; set; }

        [JsonProperty("format")] 
        public MediaFormatEnum Format { get; set; } = MediaFormatEnum.full_size;
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum MediaFormatEnum
    {
        thumbnail,
        preview,
        full_size
    }
}