using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NomadLogic.Common.Condition;
using NomadLogic.Common.Respondent;

namespace NomadLogic.Common.Questionnaire
{
    /// <summary>
    /// Convertisseur JSON pour les modèles de données.
    /// </summary>
    public class DataModelConverter : JsonConverter
    {
        public override bool CanWrite => false;

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jsonObject = JObject.Load(reader);

            // On tente d'identifier chaque classe d'implémentation par une caractéristique unique.
            DataModelBase baseInst;

            /* Questionnire */
            if (jsonObject.Properties().Any(p => p.Name.Equals("Identifier")))
            {
                baseInst = new DataModelQuestionnaire();
                //return (EventCondition)jsonObject.ToObject(typeof(EventCondition));
            }

            /* Dynamic Header */
            else if (jsonObject.Properties().Any(p => p.Name.Equals("DataKey")))
            {
                baseInst = new DataModelDynamicHeader();
                //return jsonObject.ToObject<MultipleCondition>();
            }

            else
            {
                throw new Exception("Unknown subclass of ICondition. Add this to the code.");
            }
            
            serializer.Populate(jsonObject.CreateReader(), baseInst);
            return baseInst;
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(ICondition).IsAssignableFrom(objectType);
        }
    }

    /// <summary>
    /// Classe de base pour les méthode d'un modèle de données.
    /// </summary>
    [JsonConverter(typeof(DataModelConverter))]
    [PublicAPI]
    public abstract class DataModelBase
    {
        /// <summary>
        /// Obtient une valeur à afficher dans le contrôle quand on est en activation du contrôle.
        /// </summary>
        /// <param name="respondent">Le répondant source de données.</param>
        /// <returns>Un texte (si applicable) à afficher.</returns>
        public abstract string GetActivationTextValue([NotNull] IRespondent respondent);

        /// <summary>
        /// Obtient une valeur booléenne quand on est en activation.
        /// Habituellement, représente si une réponse/état existe.
        /// </summary>
        public abstract bool GetActivationBoolValue([NotNull] IRespondent respondent);

        /// <summary>
        /// Sauvegarde d'un texte à ce modèle de données.
        /// </summary>>
        /// <returns></returns>
        public abstract bool SaveData([NotNull] ISaveLocalRespondentData saveTo, string text);

        /// <summary>
        /// Sauvegarde d'un texte à ce modèle de données.
        /// </summary>>
        /// <returns></returns>
        public abstract bool SaveData([NotNull] ISaveLocalRespondentData saveTo, IEnumerable<string> text);

        /// <summary>
        /// Sauvegarde de l'instance avec un état booléen.
        /// </summary>
        public abstract bool SaveData([NotNull] ISaveLocalRespondentData saveTo, bool state);

        public new abstract string ToString();
    }

    /// <summary>
    /// Modèle de données où l'on sauve dans un questionnaire, avec un identifiant de question unique.
    /// Ce modèle normalement contient une et une seule réponse à un identifiant.
    /// </summary>
    [PublicAPI]
    public class DataModelQuestionnaire : DataModelBase
    {
        public DataModelQuestionnaire(){}

        public DataModelQuestionnaire(int questionNo, int choiceNo = 1, QuestionTypeEnum questionType = QuestionTypeEnum.Text) :
            this(new QuestionIdentifier(questionNo, choiceNo, questionType)) { }

        public DataModelQuestionnaire(QuestionIdentifier qi)
        {
            Identifier = qi;
        }

        /// <summary>
        /// L'identifiant de question, composé d'un n uméro de question et d'un numéro de choix.
        /// </summary>
        [JsonRequired]
        public QuestionIdentifier Identifier { get; set; }

        /// <summary>
        /// Dans le cas où il faut sauvegarder une liste de valeur dans un question, si cette propriété est à vrai, on concatène les valeurs.
        /// Sinon on prend la première de la liste, comme un question ne peut contenir qu'une seule réponse.
        /// </summary>
        public bool ConcatenateEnumerativeValues { get; set; }

        /// <summary>
        /// Retourne simplement l'unique possible réponse associée à l'identifiant.
        /// </summary>
        public override string GetActivationTextValue(IRespondent respondent)
        {
            return respondent.GetAnswerText(Identifier);
        }

        /// <summary>
        /// Retourne vrai si et seulement si une réponse existe avec le même identifiant.
        /// </summary>
        public override bool GetActivationBoolValue(IRespondent respondent)
        {
            return respondent.GetAnswer(Identifier) != null;
        }

        public override bool SaveData(ISaveLocalRespondentData saveTo, string text)
        {
            return saveTo.SaveAnswerToQuestion(Identifier, text);
        }

        public override bool SaveData(ISaveLocalRespondentData saveTo, IEnumerable<string> text)
        {
            string valueToSave;

            if (text == null) valueToSave = "";
            else
            {
                if (ConcatenateEnumerativeValues)
                {
                    var sb = new StringBuilder();
                    foreach (var str in text)
                    {
                        sb.Append(str);
                        sb.Append(", ");
                    }

                    if (sb.Length > 2)
                    {
                        sb.Remove(sb.Length, 2); // Enlène le dernier ", " si présent.
                    }

                    valueToSave = sb.ToString();
                }
                else
                {
                    valueToSave = text.FirstOrDefault() ?? "";
                }
            }

            return saveTo.SaveAnswerToQuestion(Identifier, valueToSave);
        }

        

        public override bool SaveData(ISaveLocalRespondentData saveTo, bool state)
        {
            return saveTo.SaveAnswerToQuestionCheck(Identifier, state);
        }

        public override string ToString()
        {
            return $"DataModelQuestionnaire: {Identifier}";
        }
    }

    /// <summary>
    /// Le mode de données est basée sur une liste liée à une "clé". Cette liste peut soit être remplacée ou grandir selon la configuration.
    /// En théorie, on peut ajouter des données au même groupe à l'infini.
    /// </summary>
    [PublicAPI]
    public class DataModelDynamicHeader : DataModelBase
    {
        /// <summary>
        /// L'identifiant de groupe dynamique à utiliser pour ces valeurs dynamiques.
        /// </summary>
        public string DataKey { get; set; }

        /// <summary>
        /// Indique si on ajoute une information à la liste de valeurs, ou si l'on remplace la liste de valeur. à chaque fois.
        /// </summary>
        public DynamicHeaderInsertMode InsertMode { get; set; } = DynamicHeaderInsertMode.Add;
        
        /// <summary>
        /// Force l'association à un type de valeur précis. Habituellement, vaut mieux laisser la classe le détecter automatiquement.
        /// </summary>
        public DataValueType? ForceValueType { get; set; }

        public override string GetActivationTextValue(IRespondent respondent)
        {
            var found = respondent.DynamicHeaders.FirstOrDefault(dh => dh.DataKey.Equals(DataKey));
            if (found == null) return null;

            var val = found.DataValues.FirstOrDefault();

            return val?.ValueRaw;
        }

        /// <summary>
        /// Retourne vrai si au moins un groupe dynamique existe avec la clé de cette instance.
        /// </summary>
        public override bool GetActivationBoolValue(IRespondent respondent)
        {
            return respondent.DynamicHeaders.Any(c => c.DataKey.Equals(DataKey));
        }

        public override bool SaveData(ISaveLocalRespondentData saveTo, string text)
        {
            if (!AllowNullOrEmptyText && string.IsNullOrWhiteSpace(text)) return true; // On saute si text est null ou vide.
            var dv = ForceValueType.HasValue ? new DataValue(text, ForceValueType.Value) : new DataValue(text);
            
            switch (InsertMode)
            {
                case DynamicHeaderInsertMode.Replace:
                    saveTo.ClearDynamicHeaderCollection(DataKey);
                    return saveTo.SaveDynamicHeader(new DynamicHeader
                    {
                        DataKey = DataKey,
                        DateCreated = DateTime.Now,
                        DataValues = new List<DataValue>
                        {
                            dv
                        }
                    });
                case DynamicHeaderInsertMode.Add:
                    return saveTo.AddToFirstMatchingKeyHeader(DataKey, dv);
                default:
                    throw new Exception(@"Undefined insertion mode.");
            }
        }

        public override bool SaveData(ISaveLocalRespondentData saveTo, IEnumerable<string> text)
        {
            var dhl = text.Select(txt => ForceValueType.HasValue ? new DataValue(txt, ForceValueType.Value) : new DataValue(txt)).ToList();

            switch (InsertMode)
            {
                case DynamicHeaderInsertMode.Replace:
                    saveTo.ClearDynamicHeaderCollection(DataKey);
                    return saveTo.SaveDynamicHeader(new DynamicHeader
                    {
                        DataKey = DataKey,
                        DateCreated = DateTime.Now,
                        DataValues = dhl
                    });
                case DynamicHeaderInsertMode.Add:
                    return saveTo.AddToFirstMatchingKeyHeader(DataKey, dhl);
                default:
                    throw new Exception(@"Undefined insertion mode.");
            }
        }

        /// <summary>
        /// Sauvegarde la version "string" d'un booléen.
        /// </summary>
        public override bool SaveData(ISaveLocalRespondentData saveTo, bool state)
        {
            return SaveData(saveTo, state.ToString());
        }

        /// <summary>
        /// Si vrai, va introduire des chaînes vides ou nulles comme réponses dans la liste.
        /// </summary>
        public bool AllowNullOrEmptyText { get; set; }

        public override string ToString()
        {
            return $"DataModelDynamicHeader: Key={DataKey}, Mode={InsertMode}, AllowEmpty={AllowNullOrEmptyText}, ForceType={ForceValueType}";
        }
    }

    [PublicAPI]
    public enum DynamicHeaderInsertMode
    {
        Replace,
        Add
    }
    
}