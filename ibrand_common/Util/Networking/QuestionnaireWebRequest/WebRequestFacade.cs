using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Newtonsoft.Json;
using NLog;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Ui.NomadScreen;
using NomadLogic.Common.Ui.NomadScreen.ScreenAction;
using NomadLogic.Common.Util.Json;

namespace NomadLogic.Common.Util.Networking.QuestionnaireWebRequest
{
    /// <summary>
    /// Cette classe englobe une façon de lancer des méthode d'exécution REST purement via réflexion.
    /// On instancie un type de classe "IwebRequestExecutor" et on assume que les méthodes à invoquer sont de la forme
    /// suivante "T NomMethode([params1, param2, ...], typeMethode, jetonCancel)".
    /// Donc, pour obtenir la réponse (toutes les méthodes sont asynchrones), il faut donner le nom de la méthode,
    /// un dictionnaire de paramètres (sous forme string-objet), le type de demande REST, et un jeton de cancellation.
    /// La méthode d'appel est générique pour se conformer à tout réseultat sous forme de classe. On assume que les méthodes vont
    /// appeller des commandes REST sur le web et donc tenter de marshaller leurs réponses Http en type de classe prédéfini.
    /// L'usage de cette classe implique que l'on sait à l'avance le nom des méthodes et leur retour, et permet de créer ces méthodes
    /// REST dans des classe différentes en ne couplant pas ces dernière dans l'écran, d'où le concept de façade.
    /// On peut également fournir un arbre pour indiquer selon le type d'objet donné en paramètre, à quel numéro de question
    /// d'un questionnaire associé la valeur d'une propriétée, et son type.
    /// </summary>
    public sealed class WebRequestFacade : IDisposable
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        /// Arborescence pour "mapper" les réponses possibles à un questionnaire.
        [JsonProperty(PropertyName = "questionnaireMapper")]
        public List<WebExecutorMapper> QuestionnaireMapper { get; set; }

        /// Indique le nom de la classe`d'exécution de requête web à instancier par réflexion.
        [JsonProperty(PropertyName = "executorClassName")]
        public string WebExecutorClassName { get; set; }

        /// Indique la configuration à utiliser pour la classe d'exécuteur. Sera convertir en JSON puis relancé
        /// comme "populateur" pour l'instance. Aucune configuration à faire si nul ou vide.
        [JsonProperty(PropertyName = "executorConfiguration")]
        public Dictionary<string, object> WebExecutorConfiguration { get; set; }

        /// <summary>
        /// Définition de l'appel de méthode réflexif à faire sur l'exécuteur web.
        /// Décrit les arguments et comment remplir ces arguments via un questionnaire et un répondent.
        /// </summary>
        [JsonProperty(PropertyName = "executorAction")]
        public WebRequestFacadeAction WebRequestAction { get; set; }

        /// Instanciation du type pour la classe d'exécuteur.
        [JsonIgnore]
        private Type WebRequestType { get; set; }

        /// Instance de l'exécuteur web.
        [JsonIgnore]
        public IWebRequestExecutor WebRequestExecutor { get; private set; }

        /// Liste d'actions d'écran à exécuter sur retour positif.
        [JsonProperty(PropertyName = "screenActions")]
        public IList<IScreenAction> ScreenActions { get; set; }

        /// <summary>
        /// Indique que l'action est optuionnelle. Utilisé à vrai dans des situation où l'action est directement traitée par la classe qui initialise
        /// la façade. La façade devient donc surtout un protail vers un serveur.
        /// </summary>
        [JsonProperty(PropertyName = "webActionOptional")]
        public bool WebRequestActionOptional { get; set; }

        /// <summary>
        /// Indique que si on est incapable de correctement compléter la séquence, on passe quand même au prochain écran.
        /// </summary>
        [JsonProperty(PropertyName = "continueOnFail")]
        public bool ContinueOnFail { get; set; }

        /// <summary>
        /// Fais un appel Http vers un URL avec des paramètres web standard. Englobe l'envoi puis la réception, et le marshalling de la réponse, selon la classe
        /// d'implémentation.
        /// </summary>
        /// <param name="methodToInvoke">Le nom de méthode Web à invoquer.</param>
        /// <param name="parameters">Paramètre web de la requête.</param>
        /// <param name="token">Jeton de cancellation asynchrone. Optionel.</param>
        /// <returns>Une tâche asynchrone qui englobe le résultat.</returns>
        public async Task<T> LaunchMethod<T>(string methodToInvoke, IDictionary<string, object> parameters, CancellationToken? token = null)
        {
            // Vérification des préconditions d'appel.
            if (WebRequestExecutor == null || WebRequestType == null) return default(T);
            return await ExecuteMethod<T>(methodToInvoke, parameters, token);
        }

        /// Exécution de la méthode.
        private async Task<T> ExecuteMethod<T>(string methodToInvoke, IDictionary<string, object> parameters, CancellationToken? token = null)
        {
            if (WebRequestType == null)
            {
                throw new InvalidOperationException("WebRequestExecutor is not initialized.");
            }

            var getMemberIdMethod = WebRequestType.GetMethod(methodToInvoke);

            if (getMemberIdMethod == null || getMemberIdMethod.ReturnType != typeof(Task<T>))
            {
                throw new InvalidOperationException($"The requested method ({methodToInvoke}) does not exists or does not return the correct type. Should be '{typeof(Task<T>).FullName}, but is {getMemberIdMethod?.ReturnType.FullName ?? "NULL (unknown)"}'");
            }
            var mParams = getMemberIdMethod.GetParameters();
            var invokeArguments = new object[mParams.Length];

            // Si le nombre de paramètres donnés n'est pas égal au nombre de la méthode à invoquer plus deux (on ajoute le jeton de cancellation),
            // cela provoque une erreur.
            // TODO Cela devrait être fait plus tôt dans le processus.
            if (parameters.Count + 1 != mParams.Length)
            {
                throw new InvalidOperationException($"The number of parameter given does not match the targeted method parameter list. Should be '{mParams.Length - 2}, but is {parameters.Count}'");
            }

            var notFoundParams = parameters.Where(wp => !mParams.Any(p => p.Name.Equals(wp.Key))).Select(p => p.Key).ToList();
            //Si au moins un argument donné en paramètre n'est pas dans la liste des arguments de la méthode à invoquer, c'est une erreur.
            if (notFoundParams.Any())
            {
                throw new InvalidOperationException($"The following parameters do not exists in the invokee arguments list: {StringUtil.FormatEnumerable(notFoundParams, false, 0, ", ", false)}");
            }

            // Pour tous les paramètres de la méthode à invoquer, on les met dans un tableau au bon indice.
            foreach (var t in mParams)
            {
                if (!parameters.TryGetValue(t.Name, out var val)) continue;
                invokeArguments[t.Position] = val;
            }
            // On ajoute à la fin des paramètres la méthode web et le jeton de cancellation. Puis on invoque.
            // TODO Ajouter plus naturellement le jeton via réflexion.
            invokeArguments[mParams.Length - 1] = token;
            
            // ReSharper disable once CoVariantArrayConversion
            return await (Task<T>)getMemberIdMethod.Invoke(WebRequestExecutor, invokeArguments);
        }
        
        /// <summary>
        /// Cette méthode permet de initialiser un client et de vérifier les valeurs de configuration.Ensuite, un test de connexion pour validaer la présence du serveur.
        /// </summary>
        /// <returns>true si tout est correct</returns>
        public async Task<bool> Init()
        {
            log.Trace("Launching initialization of facade.");
            try
            {
                if (string.IsNullOrWhiteSpace(WebExecutorClassName))
                {
                    log.Error("Web executor class name cannot be null or empty.");
                    return false;
                }

                log.Trace("Getting type info: {0}", WebExecutorClassName);
                WebRequestType = Assembly.GetExecutingAssembly().GetType(WebExecutorClassName);
                if (WebRequestType == null)
                {
                    log.Error("Could not get class type from executing assembly: " + WebExecutorClassName);
                    return false;
                }

                log.Trace("Instancing executor class: {0}", WebExecutorClassName);
                WebRequestExecutor = Activator.CreateInstance(WebRequestType) as IWebRequestExecutor;
                if (WebRequestExecutor == null)
                {
                    log.Error("Could not instanciate WebExecutor class: " + WebExecutorClassName);
                    return false;
                }

                log.Trace("Initializing web request action with type: {0}", WebRequestType);
                if (!WebRequestActionOptional && (WebRequestAction == null || !WebRequestAction.Init(WebRequestType)))
                {
                    log.Error("Executor created, but could not initialize WebRequestAction. Please exactly state which method to use.");
                    return false;
                }

                // Convertit la configuration en JSON, puis met à jour la classe d'exécution.
                string configData = null;
                if (WebExecutorConfiguration != null && WebExecutorConfiguration.Count > 0)
                {
                    configData = JsonConvert.SerializeObject(WebExecutorConfiguration);
                }

                log.Trace("Config data loaded: size={0}", configData?.Length);
                if (string.IsNullOrWhiteSpace(configData) || JsonUtil.UpdateFromJson(configData, WebRequestExecutor))
                {
                    return await WebRequestExecutor.InitializeExecutor();
                }

                log.Error("Could not initialize (with addtional config) WebExecutor class: " + WebExecutorClassName);
                WebRequestExecutor = null;
                return false;
            }
            catch (Exception e)
            {
                log.Error(e);
                throw;
            }
        }

        /// Si on ferme l'instance, et si cette dernière possède des ressources qui doivent être libérée de façon explicite (comme des libraries externes), on le fait ici.
        private void ReleaseUnmanagedResources()
        {
            WebRequestExecutor?.Dispose();
        }

        public void Dispose()
        {
            ReleaseUnmanagedResources();
            GC.SuppressFinalize(this);
        }

        ~WebRequestFacade()
        {
            ReleaseUnmanagedResources();
        }

        /// <summary>
        /// Assigne les valeurs d'un objet (via ses propriétés) au questionnaire selon la configuration de "mapping" donné.
        /// Utilise la réflexion principalement.
        /// </summary>
        /// <param name="saveToQuest">Référence à sauvegarder dedans.</param>
        /// <param name="res">L'objet à sauvegarder.</param>
        public void SaveDataToQuestionnaire([NotNull] ISaveLocalRespondentData saveToQuest, object res)
        {
            if (res == null)
            {
                log.Debug("returned object is null");
                return;
            }

            if (QuestionnaireMapper == null)
            {
                log.Debug("No mapping definition found.");
                return;
            }

            var objType = res.GetType();
            
            log.Trace("Preparing to map. All Mapping config:\n{0}", StringUtil.FormatEnumerable(QuestionnaireMapper));
            log.Trace("Object to map: {0}", res);

            // On teste d'abord le nom complet (qui ne peut pas avoir de clash de nom), puis juste le nom de la classe (si on est sûr qu'il n'y a pad de "quid pro quo".
            foreach (var mapper in QuestionnaireMapper.Where(qm => qm.TargetClass.Equals(objType.FullName, StringComparison.CurrentCultureIgnoreCase) ||
                                                                   qm.TargetClass.Equals(objType.Name, StringComparison.CurrentCultureIgnoreCase)))
            {
                if (!res.FindPropertyValue(mapper.PropertyName, out var propVal))
                {
                    continue;
                }

                if (propVal == null)
                {
                    // Pas de valeur trouvée, on saute.
                    continue;
                }

                var strVal = propVal.ToString();

                // Vérifie quel est la méthode de sauvegarde (Checkbox (booléen), ComboBox (sous-choix), Standard (Texte)
                switch (mapper.SaveMethod)
                {
                    case WebExecutorMapperSaveMethod.CheckBoxIfExists:
                        saveToQuest.SaveAnswerToQuestionCheck(mapper.QuestionNo, mapper.ChoiceNo, !string.IsNullOrWhiteSpace(strVal));
                        break;
                    case WebExecutorMapperSaveMethod.CheckBoxParseValue:
                        saveToQuest.SaveAnswerToQuestionCheck(mapper.QuestionNo, mapper.ChoiceNo, bool.Parse(strVal));
                        break;
                    case WebExecutorMapperSaveMethod.ComboBox:
                        var answer = new Answer //3, mapper.QuestionNo, mapper.ChoiceNo, mapper.SubChoiceNo, strVal);
                        {
                            QuestionType = QuestionTypeEnum.Combo,
                            QuestionNo = mapper.QuestionNo,
                            ChoiceNo = mapper.ChoiceNo,
                            Response = strVal
                        };
                        saveToQuest.SaveAnswerToQuestion(answer);
                        break;
                    case WebExecutorMapperSaveMethod.ComboBoxParseAsIndexZeroBased:
                        if (int.TryParse(strVal, out var intVal1))
                        {
                            saveToQuest.SaveAnswerToQuestion(new Answer //(3, mapper.QuestionNo, mapper.ChoiceNo, intVal1 + 1));
                            {
                                QuestionType = QuestionTypeEnum.Combo,
                                QuestionNo = mapper.QuestionNo,
                                ChoiceNo = mapper.ChoiceNo,
                                Response = (intVal1+1).ToString()
                            });
                        }
                        break;
                    case WebExecutorMapperSaveMethod.ComboBoxParseAsIndex:
                        if (int.TryParse(strVal, out var intVal2))
                        {
                            saveToQuest.SaveAnswerToQuestion(new Answer //(3, mapper.QuestionNo, mapper.ChoiceNo, intVal2));
                            {
                                QuestionType = QuestionTypeEnum.Combo,
                                QuestionNo = mapper.QuestionNo,
                                ChoiceNo = mapper.ChoiceNo,
                                Response = intVal2.ToString()
                            });
                        }
                        break;
                    case WebExecutorMapperSaveMethod.LanguageComboBoxParseAsIndex:
                        if (int.TryParse(strVal, out var intVal3))
                        {
                            saveToQuest.SaveAnswerToQuestion(new Answer //(3, mapper.QuestionNo, mapper.ChoiceNo, intVal3));
                            {
                                QuestionType = QuestionTypeEnum.Combo,
                                QuestionNo = mapper.QuestionNo,
                                ChoiceNo = mapper.ChoiceNo,
                                Response = intVal3.ToString()
                            });
                            if (saveToQuest is ILanguageScreenInterface lang3)
                            {
                                // TODO On assume ici que le choix 1 est anglais, et le 2 est français. C'est pas convention, donc il faudrait une meilleur méthode.
                                lang3.LanguageSelection(intVal3 == 2 ? "fr" : "en");
                            }
                        }
                        break;
                    case WebExecutorMapperSaveMethod.LanguageRadioParseAsIndex:
                        if (int.TryParse(strVal, out var intValRadio))
                        {
                            saveToQuest.SaveAnswerToQuestion(new Answer //(2, mapper.QuestionNo, intValRadio));
                            {
                                QuestionType = QuestionTypeEnum.Radio,
                                QuestionNo = mapper.QuestionNo,
                                ChoiceNo = intValRadio
                            });
                            if (saveToQuest is ILanguageScreenInterface lang3)
                            {
                                // TODO On assume ici que le choix un est anglais, et le 2 est français. C'est pas convention, mais il faudrait une meilleur méthode.
                                lang3.LanguageSelection(intValRadio == 2 ? "fr" : "en");
                            }
                        }
                        break;
                    case WebExecutorMapperSaveMethod.Language:
                        saveToQuest.SaveAnswerToQuestion(new Answer
                        {
                            QuestionNo = mapper.QuestionNo, 
                            ChoiceNo = mapper.ChoiceNo, 
                            Response = strVal, 
                            QuestionType = QuestionTypeEnum.Text
                        });
                        if (saveToQuest is ILanguageScreenInterface lang)
                        {
                            lang.LanguageSelection(strVal.ToLowerInvariant());
                        }
                        break;
                    case WebExecutorMapperSaveMethod.Phone:
                        if (StringUtil.ConvertEuropePhoneToNorthAmericanPhone(strVal, out var tmp))
                            saveToQuest.SaveAnswerToQuestion //(mapper.QuestionNo, mapper.ChoiceNo, tmp, 4);
                                (new Answer {
                                    QuestionNo = mapper.QuestionNo, 
                                    ChoiceNo = mapper.ChoiceNo, 
                                    Response = tmp, 
                                    QuestionType = QuestionTypeEnum.Text
                                });
                        break;
                    default: // Habituellement, texte pur.
                        saveToQuest.SaveAnswerToQuestion //(mapper.QuestionNo, mapper.ChoiceNo, strVal, 4);
                        (new Answer {
                            QuestionNo = mapper.QuestionNo, 
                            ChoiceNo = mapper.ChoiceNo, 
                            Response = strVal, 
                            QuestionType = QuestionTypeEnum.Text
                        });
                        break;
                }

                log.Trace("Question Saved: mapper={0}, value={1}", mapper, strVal);
            }
        }

        /// <summary>
        /// Analyse et construit les arguments de la méthode à invoquer.
        /// S'il est impossible d'invoquer la méthode correctement (probablement à cause que l'on n'utilise pas les bons types ou noms d'arguments), on
        /// lance une exception.
        /// </summary>
        /// <param name="resp">Le répondant actuel.</param>
        /// <param name="dicAnswers">Les réponses actuelles.</param>
        /// <param name="token">Jeton de cancellation optionnel.</param>
        /// <returns>La référence vers l'objet de retour de la commande REST.</returns>
        public async Task<object> ConstructAndInvokeMethod(IRespondent resp, Dictionary<QuestionIdentifier, Answer> dicAnswers, CancellationToken? token = null)
        {
            var t = WebRequestAction.ReturnType;

            // voir ces liens sur la facon d'appeller un méthode générique par réflexion.
            // https://stackoverflow.com/questions/3957817/calling-generic-method-with-type-variable
            // https://stackoverflow.com/questions/39674988/how-to-call-a-generic-async-method-using-reflection
            var methodInfo = GetType().GetMethod("LaunchMethod")?.MakeGenericMethod(t);
            if (methodInfo == null)
            {
                throw new Exception("WebRequestFacade object instance contains no method named 'LaunchMethod'. Did the method name change?");
            }

            var dico = WebRequestAction.CreateInvokeParameters(resp, dicAnswers);

            var tokenNullable = false;
            var tokenParam = methodInfo.GetParameters().FirstOrDefault(p => p.ParameterType == typeof(CancellationToken));
            if (tokenParam == null)
            {
                tokenParam = methodInfo.GetParameters().FirstOrDefault(p => p.ParameterType == typeof(CancellationToken?)); // Version nullable
                tokenNullable = true;
            }

            object[] objParams;
            if (tokenParam != null)
            {
                // TODO Avoir une vérification pour permettre d'insérer le jeton de cancellation dans le bon ordre.
                if (tokenNullable)
                {
                    objParams = new object[]
                    {
                        WebRequestAction.WebRequestMethodName,
                        dico,
                        token
                    };
                }
                else
                {
                    objParams = new object[]
                    {
                        WebRequestAction.WebRequestMethodName,
                        dico,
                        // ReSharper disable once MergeConditionalExpression
                        token.HasValue ? token.Value : CancellationToken.None // Ne pas coalescer. On doit enlever le boxing.
                    };
                }
            }
            else
            {
                objParams = new object[]
                {
                    WebRequestAction.WebRequestMethodName,
                    dico
                };
            }

            dynamic resAsync = methodInfo.Invoke(this, objParams);

            await resAsync;
            return resAsync.GetAwaiter().GetResult();
        }
    }
}
