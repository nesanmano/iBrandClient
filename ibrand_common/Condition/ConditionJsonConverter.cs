using System;
using System.CodeDom;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace NomadLogic.Common.Condition
{
    /// <summary>
    /// Classe pour désérialiser les classes d'implémentation de ICondition
    /// </summary>
    public class ConditionJsonConverter : JsonConverter
    {
        public override bool CanWrite => false;

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jsonObject = JObject.Load(reader);
            ICondition cond;

            // On tente d'identifier chaque classe d'implémentation par une caractéristique unique.

            /* EventCondition */
            if (jsonObject.Properties().Any(p => p.Name.Equals("EventId")))
            {
                cond = new EventCondition();
                //return (EventCondition)jsonObject.ToObject(typeof(EventCondition));
            }

            /* MultipleCondition */
            else if (jsonObject.Properties().Any(p => p.Name.Equals("Operator")))
            {
                cond = new MultipleCondition();
                //return jsonObject.ToObject<MultipleCondition>();
            }

            /* QuestionCondition */
            else if (jsonObject.Properties().Any(p => p.Name.Equals("QuestionNo")))
            {
                cond = new QuestionCondition();
                //return jsonObject.ToObject<QuestionCondition>();
            }

            else
            {
                throw new Exception("Unknown subclass of ICondition. Add this to the code.");
            }
            
            serializer.Populate(jsonObject.CreateReader(), cond);
            return cond;
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(ICondition).IsAssignableFrom(objectType);
        }
    }
}
