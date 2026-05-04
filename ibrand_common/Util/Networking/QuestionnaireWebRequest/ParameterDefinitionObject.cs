using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using NLog;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Util.Json;

namespace NomadLogic.Common.Util.Networking.QuestionnaireWebRequest
{
    /// <summary>
    /// Représente un type de mapping plus complexe, une référence d'objet possédant des propriétés.
    /// </summary>
    [PublicAPI]
    public class ParameterDefinitionObject : ParameterDefinition
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Liste des définitions d'assignation de propriétés à partir d'un questionnaire.
        /// </summary>
        public List<PropertyQuestionnaireMapping> PropertyMap { get; set; }

        public override object CreateParameterValue(IRespondent resp, Dictionary<QuestionIdentifier, Answer> dicAnswers)
        {
            // Création par réflexion de l'instance.
            var objType = Assembly.GetExecutingAssembly().GetType(NameType);
            if (objType == null)
            {
                throw new Exception("Could not get class type from executing assembly: " + NameType);
            }

            var inst = Activator.CreateInstance(objType);

            // Analyse maintenant les propriétés de cette instance
            var props = objType.GetProperties();
            var stringType = typeof(string);

            foreach (var mapping in PropertyMap)
            {
                var prop = props.FirstOrDefault(p => p.Name.Equals(mapping.PropertyName));
                if (prop == null)
                {
                    throw new Exception($"Unknown Property ('{mapping.PropertyName}') for object type '{NameType}': ");
                }

                var isList = false;
                Type genType = null;

                if (prop.PropertyType.IsGenericType)
                {
                    // La propriété est une liste générique. On créé un object via une définition JSON interne à un string.

                    genType = prop.PropertyType.GenericTypeArguments[0];
                    if (genType != null)
                    {
                        var genListType = typeof(IList<>).MakeGenericType(genType);
                        isList = genListType.IsAssignableFrom(prop.PropertyType);
                    }
                    else
                    {
                        throw new Exception($"Cannot infer generic type from: {prop.PropertyType.GenericTypeArguments}");
                    }
                }

                if (isList)
                { 
                    // Vérifie si la liste est nulle ou déjà instanciée.
                    var list = prop.GetValue(inst);
                    var actualListType = typeof(List<>).MakeGenericType(genType);
                    if (list == null)
                    {
                        // Si null, on créé une liste du type générique voulu et on créé une instance (une liste vide).
                        
                        list = Activator.CreateInstance(actualListType);
                        prop.SetValue(inst, list);
                        // La liste est créée et assignée, on peut maintenant y ajouter des objets du bon type.
                    }

                    var subObj = Activator.CreateInstance(genType); // On créé une instance de l'objet à ajouter.
                    var parsedVal = StringUtil.CreateStringFromTokenizedPatternRespondent(mapping.JsonValue, dicAnswers, null, resp);
                    if (JsonUtil.UpdateFromJson(parsedVal, subObj))
                    {
                        // Finalement, ajout dans la liste.
                        actualListType.GetMethod("Add")?.Invoke(list, new []{ subObj });
                    }
                    else
                    {
                        // Erreur, pas été capable d'instancier
                        throw new Exception($"Cannot create object for IList Property ('{mapping.PropertyName}') for object type '{genType}' with (parsed) value '{parsedVal}' from tokenized form '{mapping.JsonValue}'");
                    }
                }
                else
                {
                    var val = StringUtil.CreateStringFromTokenizedPatternRespondent(mapping.TokenValue, dicAnswers, null, resp);
                    if (string.IsNullOrWhiteSpace(val)) continue; // On Assume que l'on saute une valeur inexistante.

                    if (prop.PropertyType != stringType)
                    {
                        // On tente une conversion de la valeur texte vers une possibilité de quelques types.
                        var converter = TypeDescriptor.GetConverter(prop.PropertyType);
                        var propVal = converter.ConvertFromInvariantString(val);

                        // Assignation
                        prop.SetValue(inst, propVal);
                    }
                    else
                    {
                        prop.SetValue(inst, val);
                    }
                }
            }

            return inst;
        }
    }
}