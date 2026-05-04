using System;
using System.Linq;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace NomadLogic.Common.Util.RfidReader
{
    /// <summary>
    /// Classe qui permet de désérialiser les classes enfants de la classe abstraite "IRfidReader"
    /// Voir ici pour plus d'information: http://skrift.io/articles/archive/bulletproof-interface-deserialization-in-jsonnet/
    /// </summary>
    [PublicAPI]
    public class RfidReaderConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException("Classe incapable d'écrire");
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jsonObject = JObject.Load(reader);

            // On tente d'identifier chaque classe d'implémentation par une caractéristique unique.
            IRfidReader pd;

            /* ParameterDefinitionObject */
            if (jsonObject.Properties().Any(p => p.Name.Equals("ModelName")))
            {
                pd = new MR800_RfidReader();
            }
            else
            {
                pd = new Simple_RfidReader();
            }

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