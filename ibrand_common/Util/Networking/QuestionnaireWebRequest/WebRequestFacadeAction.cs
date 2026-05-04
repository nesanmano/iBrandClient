using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using JetBrains.Annotations;
using Newtonsoft.Json;
using NLog;
using NomadLogic.Common.Respondent;

namespace NomadLogic.Common.Util.Networking.QuestionnaireWebRequest
{
    /// <summary>
    /// Décrit une signature de méthode à invoquer, ainsi que comment remplir les arguments via des objets provenant de notre application, à savoir un
    /// questionnaire remplit et un répondant.
    /// </summary>
    [PublicAPI]
    public class WebRequestFacadeAction
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        
        /// Version instanciée du type de la classe, trouvé par réflexion une fois initialisée.
        [JsonIgnore]
        public Type ReturnType { get; private set; }

        /// Le nom, avec ou sans Namespace, du type de retour.
        public string ReturnTypeName { get; set; }

        /// La nom de la méthode à exécuter de ce type.
        public string WebRequestMethodName { get; set; }

        /// La définition de chaque paramètre de la méthode, et comment la remplir.
        /// Il faut définir TOUS les paramètres, sauf les deux derniers (Méthode Web et Jeton de cancellation), qui sont automatisés par la facade.
        /// L'ordre de définition n'est pas important, l'ordre des arguments est vérifié lors de l'initialisation.
        public List<ParameterDefinition> WebRequestMethodParameterDefinitions { get; set; } = new List<ParameterDefinition>();
        
        /// Il faut fournir ici un objet qui fasse partie du même "assembly" (DLL, ou projet Visual Studio) que la classe que l'on veut instancier.
        /// Comme normalement ces classes font partie du même projet qu'une facade, on passe habituellement une instance de facade.
        /// On assume également que l'exécuteur à été initialisé, et que la liste de paramètres à codifier n'inclu pas le
        /// jeton de cancellation.
        public bool Init([NotNull] Type webRequestType)
        {
            if (WebRequestMethodParameterDefinitions == null) return false;
            if (string.IsNullOrWhiteSpace(ReturnTypeName)) return false;
            
            var type = webRequestType.Assembly.GetType(ReturnTypeName, false, true);

            if (type == null) return false;

            ReturnType = type;

            // Confirme que la méthode à invoquer contient les mêmes paramètres que ceux précisé dans la classe, sauf le jeton de cancellation.
            var mInfo = webRequestType.GetMethod(WebRequestMethodName);
            if (mInfo == null)
            {
                log.Error("Requsted method ({0}) does not exist in the following type ({1})", WebRequestMethodName, webRequestType.FullName);
                return false;
            }

            var par = mInfo.GetParameters();

            var hasToken = par.Any(p => p.ParameterType == typeof(CancellationToken)) ||
                           par.Any(p => p.ParameterType == typeof(CancellationToken?));
            if (!hasToken)
            {
                log.Warn("The method definition for {0} has no cancelation token specified. It will run without cancellation possibilities. Please review.");
            }

            var expectedNbrOfParams = par.Length - (hasToken ? 1 : 0);

            // Test sur le nombre (Count - 2)
            if (expectedNbrOfParams != WebRequestMethodParameterDefinitions.Count)
            {
                log.Error("The target method needs to declare {0} arguments in it's configuration file (without cancel token). But {1} have been declared.",
                    expectedNbrOfParams, WebRequestMethodParameterDefinitions.Count);
                return false;
            }

            // On cherche les disparités de noms.
            var notFound = WebRequestMethodParameterDefinitions.Where(p => !par.Any(arg => arg.Name.Equals(p.Name))).Select(n => n.Name).ToList();
            if (notFound.Any())
            {
                log.Error("The following parameters do not exist in target method ({0}): {1}", 
                    mInfo.Name, StringUtil.FormatEnumerable(notFound, false, 0, ", ", false));
                return false;
            }

            // On teste les types de chaque paramètres.
            foreach (var mpd in WebRequestMethodParameterDefinitions)
            {
                // Comme certains types sont très fréquents (string, int, float, double, bool), on peut utiliser quelques noms raccourcis,
                // plutôt que les noms complets (comme 'System.String')
                // Tous les autres doivent être le nom complet, avec Namespace.
                Type mType;
                switch (mpd.NameType)
                {
                    case "string":
                        mType = typeof(string);
                        break;
                    case "int":
                        mType = typeof(int);
                        break;
                    case "float":
                        mType = typeof(float);
                        break;
                    case "double":
                        mType = typeof(double);
                        break;
                    case "bool":
                        mType = typeof(bool);
                        break;
                    default:
                        mType = Assembly.GetExecutingAssembly().GetType(mpd.NameType);
                        if (mType == null)
                        {
                            mType = ClassUtil.GetEnumType(mpd.NameType);
                            if (mType != null)
                            {
                                mpd.IsEnum = true;
                            }
                        }
                        break;
                }

                if (mType == null)
                {
                    log.Error("The following parameter type could not be instanciated: {0}", mpd.NameType);
                    return false;
                }

                // Types interdits
                if (mType == typeof(CancellationToken?) || mType == typeof(CancellationToken))
                {
                    log.Error("The following parameter type is forbidden (automatically added): {0}", mType.Name);
                    return false;
                }

                var curPar = par.First(p => p.Name.Equals(mpd.Name));
                if (curPar.ParameterType != mType)
                {
                    log.Error("The declared parameter type do not match the one found in the signature. The parameter '{0}' type should be '{1}', but is described in the configuration as '{2}'.",
                        mType.Name, curPar.ParameterType, mType);
                    return false;
                }
            }

            return true;
        }
        
        /// <summary>
        /// Créer un dictionnaire de paramètres pour l'invoquation de méthodes par réflexion, basé sur les configurations de l'instance.
        /// Assume que l'instance a été initialisée et que les vérifications sont déjà faite.
        /// </summary>
        /// <param name="resp">Le répondant, source de données.</param>
        /// <param name="dicAnswers">Les réponse courantes.</param>
        /// <returns>Le dictionnaire de paramètres.</returns>
        public IDictionary<string, object> CreateInvokeParameters(IRespondent resp, Dictionary<QuestionIdentifier, Answer> dicAnswers)
        {
            var returnDic = new Dictionary<string, object>();

            foreach (var wrmpn in WebRequestMethodParameterDefinitions)
            {
                returnDic.Add(wrmpn.Name, wrmpn.CreateParameterValue(resp, dicAnswers));
            }

            return returnDic;
        }
    }
}
