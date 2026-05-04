using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using NomadLogic.Common.Util;

namespace NomadLogic.Common.Services.LocalCapabilities
{
    /// <summary>
    /// Capacités globales d'un outil à tester.
    /// </summary>
    [DataContract]
    public class CapabilityStatus
    {
        /// <summary>
        /// Le nom du groupe de fonctionalité testé.
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public CapabilityStatusGroupEnum Name { get; set; }

        /// <summary>
        /// Indique si de façon totale la fonctionnalité est disponible.
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public StatusEnum Status { get; set; } = StatusEnum.Pending;

        /// <summary>
        /// Liste des sous-fonctions individuelles de chaque groupes.
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public List<SubCapabilityStatus> SubFunctionsCapabilities { get; set; } = new List<SubCapabilityStatus>();

        /// <summary>
        /// Message général dans le cas d'une erreur. Si vide, tout s'est bien passé.
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public List<string> ErrorMessages { get; set; } = new List<string>();

        /// <inheritdoc />
        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.Append('[');
            sb.Append(Name);
            sb.Append(", Status=");
            sb.Append(Status);
            if (SubFunctionsCapabilities?.Any() ?? false)
            {
                sb.Append(", SubFonctions={");
                StringUtil.FormatEnumerable(SubFunctionsCapabilities, false, 0, ", ", true);
                sb.Append('}');
            }

            if (ErrorMessages?.Any() ?? false)
            {
                sb.Append(", Errors={");
                StringUtil.FormatEnumerable(ErrorMessages, false, 0, ", ", true);
                sb.Append('}');
            }
            
            sb.Append(']');
            
            return sb.ToString();
        } 
    }
}