using System;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NomadLogic.Common.Util.RfidReader;

namespace NomadLogic.App.EventVideoCapture.Capture
{
    public class CaptureVideoBaseConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException("Classe incapable d'écrire");
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jsonObject = JObject.Load(reader);

            // On tente d'identifier chaque classe d'implémentation par une caractéristique unique.
            CaptureVideoBase pd;

            /* ParameterDefinitionObject */
            /*
            if (jsonObject.Properties().Any(p => p.Name.Equals("UNIQUE PROPERTY_NAME")))
            {
                pd = new SUB_CLASS();
            } else
            */

            pd = new CaptureFfmpegScript();

            serializer.Populate(jsonObject.CreateReader(), pd);
            return pd;
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(IRfidReader).IsAssignableFrom(objectType);
        }

        public override bool CanWrite => false;
    }
}