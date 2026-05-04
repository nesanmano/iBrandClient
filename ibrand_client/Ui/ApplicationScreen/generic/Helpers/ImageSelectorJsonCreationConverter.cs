using System;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NomadLogic.Common.Condition;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers
{
    /// <summary>
    /// Convertisseur de classe pour désérialisation Json, quand on connait l'implémentation exacte de la classe de sélecteur.
    /// En théorie, couvre tout les types d'implémentation de l'interface IImageSelector.
    /// Voir ici pour plus d'information: http://skrift.io/articles/archive/bulletproof-interface-deserialization-in-jsonnet/
    /// </summary>
    public class ImageSelectorJsonCreationConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException("Classe incapable d'écrire");
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {

            var jsonObject = JObject.Load(reader);

            // On tente d'identifier chaque classe d'implémentation par une caractéristique unique.

            /* ImageSelectorSingle */
            if (jsonObject.Properties().Any(p => p.Name.Equals("ImageFile")))
            {
                return (ImageSelectorSingle)jsonObject.ToObject(typeof(ImageSelectorSingle));
            }

            /* ImageSelectorQuestionnaire */
            var first = jsonObject.Properties().First();

            if (first.Name.Equals("Tuples"))
            {
                if (first.Count == 0)
                {
                    return (ImageSelectorQuestionnaire<QuestionCondition>)jsonObject.ToObject(typeof(ImageSelectorQuestionnaire<QuestionCondition>));
                }

                /* Ici on teste la différence entre:
                    "Tuples": [
                     {
                       "Condition": {
                         "EventId": 7897
                       },
                       "File": "event1"
                     }
                   ]
                  et
                   "Tuples": [
                    {
                      "Condition": {
                        "QuestionNo": 56,
                        "ChoiceNo": 2,
                        "SubChoiceNo": -1
                      },
                      "File": "data1"
                    }
                  ]
                 On vise donc le nom de la propriété après Condition, comme c'est ce type (sous-classe de ICondition) qui peut changer.
                 */

                var tmp = first.Value.Children<JObject>();
                if (!tmp.Any()) throw new System.Exception("Unknown subclass of IImageSelector. Add this to the code.");

                var tmp2 = tmp.First().Properties().First();
                if (!tmp2.Any())
                    return (ImageSelectorQuestionnaire<QuestionCondition>)jsonObject.ToObject(
                        typeof(ImageSelectorQuestionnaire<QuestionCondition>));

                var tmp3 = tmp2.Children<JObject>().First().Properties().First();
                if (!tmp3.Any())
                    return (ImageSelectorQuestionnaire<QuestionCondition>)jsonObject.ToObject(
                        typeof(ImageSelectorQuestionnaire<QuestionCondition>));
                switch (tmp3.Name)
                {
                    case "QuestionNo":
                        return (ImageSelectorQuestionnaire<QuestionCondition>)jsonObject.ToObject(
                            typeof(ImageSelectorQuestionnaire<QuestionCondition>));
                    case "EventId":
                        return (ImageSelectorQuestionnaire<EventCondition>)jsonObject.ToObject(
                            typeof(ImageSelectorQuestionnaire<EventCondition>));
                    default: throw new System.Exception("Unknown subclass of ICondition. Add this to the code.");
                }
            }

            throw new System.Exception("Unknown subclass of IImageSelector. Add this to the code.");
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(IImageSelector).IsAssignableFrom(objectType);
        }

        public override bool CanWrite => false;
    }
}
