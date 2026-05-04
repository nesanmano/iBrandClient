using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace NomadLogic.Common.Util.Keyboard
{
    public class KeyboardHook
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public VirtualKeys Key { get; set; }

        [JsonProperty (ItemConverterType = typeof(StringEnumConverter))]
        public IList<VirtualKeys> AdditionalFlags { get; set; } = new List<VirtualKeys>();

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("[Keyboard Hook => '");
            sb.Append(Key);
            sb.Append("'");
            if (AdditionalFlags != null && AdditionalFlags.Any())
            {
                sb.Append(" flags={");
                foreach (var flag in AdditionalFlags)
                {
                    sb.Append(flag.ToString());
                    sb.Append(", ");
                }
                sb.Remove(sb.Length -2, 2); // Enlève les derniers caractères extras.
                sb.Append("}");
            }
            sb.Append("]");

            return sb.ToString();
        }
    }
}