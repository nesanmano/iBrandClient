using Newtonsoft.Json;

namespace NomadLogic.Common.Util.Json
{
    /// <summary>
    /// Interface pour décrire qu'une classe peut se charger à l'aide d'un fichier JSON en définissant un fichier de configuration par défaut.
    /// Utilisé normalement avec FileUtil.CreateInstanceFromJsonConfigFile().
    /// </summary>
    public interface IJsonConfig
    {
        /// Le nom du fichier de configuration par défaut.
        [JsonIgnore]
        string DefaultConfigFileName { get; }
    }
}