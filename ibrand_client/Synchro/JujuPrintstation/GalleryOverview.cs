using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace NomadLogic.iBrandDesktop.Synchro.JujuPrintstation
{
    public class GalleryOverview
    {

        [JsonProperty("galleryRespondentUnit")]
        public List<GalleryUnitElement> GalleryRespondentUnit { get; set; }
        [JsonProperty("totalResp")]
        public int TotalResp { get; set; }
        [JsonProperty("pId")]
        public int PId { get; set; }
        [JsonProperty("qId")]
        public int QId { get; set; }
        [JsonProperty("projectName")]
        public String ProjectName { get; set; }
        [JsonProperty("otherFolders")]
        public List<String> OtherFolders { get; set; }
    }
}
