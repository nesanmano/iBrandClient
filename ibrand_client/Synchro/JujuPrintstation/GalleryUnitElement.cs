using Newtonsoft.Json;

namespace NomadLogic.iBrandDesktop.Synchro.JujuPrintstation
{
    public class GalleryUnitElement
    {

        [JsonProperty("token")]
        public string Token { get; set; }
        [JsonProperty("imgUrl")]
        public string ImgUrl { get; set; }
    }
}
