using System.Collections.Generic;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace NomadLogic.Common.Util.Networking.QuestionnaireWebRequest.Emisys
{
    /// <summary>
    /// Classe utilisée pour stocker les informations d'une requête de création de panier d'achat avec Emisys.
    /// </summary>
    [PublicAPI]
    public class BasketCreationInfo
    {
        [JsonProperty(PropertyName = "pos_id")]
        public int PointOfServiceId { get; set; }

        [JsonProperty(PropertyName = "terminal_id")]
        public string TerminalId { get; set; }

        [JsonProperty(PropertyName = "products")]
        public IList<ProductInfo> Products { get; set; }

        public override string ToString()
        {
            return ClassUtil.ToStringPublicProperties(this);
        }
    }
}