using Newtonsoft.Json;

namespace NomadLogic.Common.Util
{
    public interface IExternalFileConfig
    {
        [JsonIgnore]
        string ConfigFileName { get; set; }
    }
}