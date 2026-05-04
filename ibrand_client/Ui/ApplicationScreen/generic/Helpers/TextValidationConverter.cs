using System;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers
{
    /// <summary>
    /// Classe qui permet de désérialiser les classes enfants de la classe abstraite "IRfidReader"
    /// Voir ici pour plus d'information: http://skrift.io/articles/archive/bulletproof-interface-deserialization-in-jsonnet/
    /// </summary>
    [PublicAPI]
    public class TextValidationConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException("Classe incapable d'écrire");
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jsonObject = JObject.Load(reader);

            // On tente d'identifier chaque classe d'implémentation par une caractéristique unique.
            ITextValidation tv;

            /* ParameterDefinitionObject */
            // TODO Un seul choix pour l'instant

            tv = new TextValidationBasic();

            serializer.Populate(jsonObject.CreateReader(), tv);
            return tv;
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(ITextValidation).IsAssignableFrom(objectType);
        }

        public override bool CanWrite => false;
    }
}