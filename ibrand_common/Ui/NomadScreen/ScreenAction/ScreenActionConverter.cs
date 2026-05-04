using System;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace NomadLogic.Common.Ui.NomadScreen.ScreenAction
{
    public class ScreenActionConverter :JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException("Classe incapable d'écrire");
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jsonObject = JObject.Load(reader);

            // On tente d'identifier chaque classe d'implémentation par une caractéristique unique.
            IScreenAction pd = null;

            /* ParameterDefinitionObject */
            if (jsonObject.Properties().Any(p => p.Name.Equals("QuestionPrintPicture")))
            {
                pd = new PrintScreenAction();
            }

            if (pd == null) throw new Exception("Unknown subclass of 'IScreenAction'. Add this to the code of this converter.");

            serializer.Populate(jsonObject.CreateReader(), pd);
            return pd;
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(IScreenAction).IsAssignableFrom(objectType);
        }

        public override bool CanWrite => false;
    }
}