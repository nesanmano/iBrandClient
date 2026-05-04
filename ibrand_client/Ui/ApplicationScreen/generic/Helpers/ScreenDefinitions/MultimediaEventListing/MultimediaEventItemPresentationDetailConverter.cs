using System;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers.ScreenDefinitions.MultimediaEventListing
{
    /// <summary>
    /// Classe de conversion JSON pour les implémentation de l'interface "IMultimediaEventItemPresentationDetail".
    /// </summary>
    public class MultimediaEventItemPresentationDetailConverter : JsonConverter<IMultimediaEventItemPresentationDetail>
    {
        public override void WriteJson(JsonWriter writer, IMultimediaEventItemPresentationDetail value, JsonSerializer serializer)
        {
            throw new NotImplementedException("Class cannot write to JSON.");
        }
        
        public override IMultimediaEventItemPresentationDetail ReadJson(JsonReader reader, Type objectType, IMultimediaEventItemPresentationDetail existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var jsonObject = JObject.Load(reader);

            // On tente d'identifier chaque classe d'implémentation par une caractéristique unique.
            IMultimediaEventItemPresentationDetail tmb;

            /* ParameterDefinitionObject */
            if (jsonObject.Properties().Any(p => p.Name.Equals("PresentationZone")))
            {
                tmb = new MultimediaEventItemPresentationDetailSingle();
            }
            else if (jsonObject.Properties().Any(p => p.Name.Equals("GridZone")))
            {
                tmb = new MultimediaEventItemPresentationDetailSelector();
            }
            else if (jsonObject.Properties().Any(p => p.Name.Equals("WebControlZone")))
            {
                tmb = new MultimediaEventVimeoPresentation();
            }
            else
            {
                throw new NotImplementedException("No concrete class of type 'IMultimediaEventItemPresentationDetail' can be detected in JSON data. If you added a concrete class recently, modify converter to be able to identify it by a single field.");
            }

            serializer.Populate(jsonObject.CreateReader(), tmb);
            return tmb;
        }

        public override bool CanWrite => false;
    }
}