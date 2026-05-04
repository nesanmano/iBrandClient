using System;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace NomadLogic.Common.Util.Networking.QuestionnaireWebRequest
{
    /// <summary>
    /// Classe qui permet de désérialiser les classes enfants de la classe abstraite "ParameterDefinition"
    /// Voir ici pour plus d'information: http://skrift.io/articles/archive/bulletproof-interface-deserialization-in-jsonnet/
    /// </summary>
    public class ParameterDefinitionConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException("Classe incapable d'écrire");
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jsonObject = JObject.Load(reader);

            // On tente d'identifier chaque classe d'implémentation par une caractéristique unique.
            ParameterDefinition pd = null;

            /* ParameterDefinitionObject */
            if (jsonObject.Properties().Any(p => p.Name.Equals("PropertyMap")))
            {
                pd = new ParameterDefinitionObject();
            }

            if (jsonObject.Properties().Any(p => p.Name.Equals("TokenValue")))
            {
                pd = new ParameterDefinitionValue();
            }

            if (pd == null) throw new Exception("Unknown subclass of 'ParameterDefinition'. Add this to the code of this converter.");

            serializer.Populate(jsonObject.CreateReader(), pd);
            return pd;
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(ParameterDefinition).IsAssignableFrom(objectType);
        }

        public override bool CanWrite => false;
    }
}