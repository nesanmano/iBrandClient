using System;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace NomadLogic.App.EventVideoCapture.Reader
{
    public class ReaderManagerBaseConverter : JsonConverter<ReaderManagerBase>
    {
        public override void WriteJson(JsonWriter writer, ReaderManagerBase value, JsonSerializer serializer)
        {
            throw new NotImplementedException("Class cannot write to JSON.");
        }
        
        public override ReaderManagerBase ReadJson(JsonReader reader, Type objectType, ReaderManagerBase existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var jsonObject = JObject.Load(reader);

            // On tente d'identifier chaque classe d'implémentation par une caractéristique unique.
            ReaderManagerBase tmb;

            /* ParameterDefinitionObject */
            if (jsonObject.Properties().Any(p => p.Name.Equals("BaseUrlCnG")))
            {
                tmb = new ReaderManagerConnectAndGo();
            }
            else
            {
                tmb = new ReaderManagerStandAlone();
            }

            serializer.Populate(jsonObject.CreateReader(), tmb);
            return tmb;
        }

        public override bool CanWrite => false;
    }
}