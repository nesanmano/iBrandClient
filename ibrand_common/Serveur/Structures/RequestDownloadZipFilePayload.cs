using System.Collections.Generic;
using Newtonsoft.Json;

namespace NomadLogic.Common.Serveur.Structures
{
    public class RequestDownloadZipFilePayload
    {
        [JsonProperty("listOfFiles")]
        [JsonRequired]
        public IList<string> FilesToDownload { get; set; }
    }
}