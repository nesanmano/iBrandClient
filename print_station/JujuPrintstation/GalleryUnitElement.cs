using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace NomadLogic.PrintStation.JujuPrintstation
{
    public class GalleryUnitElement
    {
        [JsonProperty("token")]
        public string Token { get; set; }
        [JsonProperty("imgUrl")]
        public string ImgUrl { get; set; }
    }
}
