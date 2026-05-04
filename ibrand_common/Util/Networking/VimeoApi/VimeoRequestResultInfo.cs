using System.Collections.Generic;
using Newtonsoft.Json;

namespace NomadLogic.Common.Util.Networking.VimeoApi
{
    /// <summary>
    /// Classe interne pour contenir la pagination des requêtes d'information vers Vimeo
    /// </summary>
    /// <typeparam name="T">Le type de la classe JSON de la requête originale.</typeparam>
    public class VimeoRequestResultInfo<T>
    {
        /// <summary>
        /// Le nombre total d'élément retournée par la requête.
        /// </summary>
        public int Total { get; set; }

        /// <summary>
        /// L'indice de page actuel de la requête.
        /// </summary>
        public int Page { get; set; }

        /// <summary>
        /// Le nombre d'éléments retourné maximal par page.
        /// </summary>
        [JsonProperty(PropertyName = "per_page")]
        public int PerPage { get; set; }

        /// <summary>
        /// La liste de données de la requête. Contiendra au maximum le nombre d'éléments mentionnés.
        /// </summary>
        public List<T> Data { get; set; }
    }
}