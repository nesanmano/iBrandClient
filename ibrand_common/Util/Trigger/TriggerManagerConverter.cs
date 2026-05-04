using System;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NomadLogic.Common.Phidgets;
using NomadLogic.Common.Util.Trigger.LaserBeam;

namespace NomadLogic.Common.Util.Trigger
{
    public class TriggerManagerConverter : JsonConverter<TriggerManagerBase>
    {
        public override void WriteJson(JsonWriter writer, TriggerManagerBase value, JsonSerializer serializer)
        {
            throw new NotImplementedException("Class cannot write to JSON.");
        }
        
        public override TriggerManagerBase ReadJson(JsonReader reader, Type objectType, TriggerManagerBase existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var jsonObject = JObject.Load(reader);

            // On tente d'identifier chaque classe d'implémentation par une caractéristique unique.
            TriggerManagerBase tmb;

            /* ParameterDefinitionObject */
            if (jsonObject.Properties().Any(p => p.Name.Equals("TriggerButton")))
            {
                tmb = new LaserBeamManager();
            }
            else if (jsonObject.Properties().Any(p => p.Name.Equals("TriggerChannel")))
            {
                tmb = new PhidgetTriggerManager();
            }
            else
            {
                tmb = new TriggerManagerFormKeyboard();
            }

            serializer.Populate(jsonObject.CreateReader(), tmb);
            return tmb;
        }

        public override bool CanWrite => false;
    }
}