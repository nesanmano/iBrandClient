using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using JetBrains.Annotations;
using Newtonsoft.Json;
using NLog;

namespace NomadLogic.Common.Util
{
    /// <summary>
    /// Cette classe d'aide contient des algorithme pour manipuler ou lister les composantes de classes ou d'instances de classes via les concepts de réflections
    /// fournits par C#.
    /// </summary>
    [PublicAPI]
    public static class ClassUtil
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        public static readonly int PointerSize = Environment.Is64BitOperatingSystem ? sizeof(long) : sizeof(int);

        /// <summary>
        /// Cette méthode prend une instance de n'importe quelle classe est va produire un String semblable à un "ToString" custom, en énumérant
        /// chaque propriété et sa valeur, dans le format suivant: "[Prop1=Prop1Value, Prop2=Prop2Value, ...]"
        /// On peut choisir un format multiligne en donnant le nombre de tabulation à mettre pour chaque élément. Cela donne quelque chose comme (avec 2 tabulations)
        /// "\n\t\tProp1=Prop1Value\n\t\tProp2=Prop2Value"
        /// On saute par dessus les propriétés qui sont ignorée par la configuration JSON appropriée de Newtonsoft, soit: "JsonIgnoreAttribute".
        /// Voir ici pour plus d'info: http://stackoverflow.com/questions/737151/how-to-get-the-list-of-properties-of-a-class
        /// </summary>
        /// <param name="instance">L'objet à anlyser.</param>
        /// <param name="multiline">Le nombre de tabulation pour décrire chaque propriété de l'objet, avec un saut de ligne dès le départ. 0 indique sur un seule ligne, entouré de 'brackets'.</param>
        /// <param name="newLine">Force une séquence de caractères spécifique pour le retour à la ligne. Par défaut: "\n". Utilisez "\r\n" pour un TextBox multiligne</param>
        /// <param name="multilineSeparator">La chaîne de caractères pour espacer, durant un multiligne. Par déafut: une tabulation. Cette chaîne sera utilisé de multiple fois pour représenter les différents espacements.</param>
        /// <returns>Retourne nul si l'argument est nul.</returns>
        [NotNull]
        public static string ToStringPublicProperties([NotNull] this object instance, ushort multiline = 0, string newLine = "\n", string multilineSeparator = "\t") {
            var sb = new StringBuilder();

            if (multiline == 0) sb.Append('[');
            var first = true;

            foreach(var prop in instance.GetType().GetProperties())
            {
                // On saute par dessus les propriétés qui sont ignorée par la configuration JSON appropriée.
                if (Attribute.IsDefined(prop, typeof(JsonIgnoreAttribute))) continue;

                if (multiline != 0) {
                    sb.Append(newLine);
                    for (var i = 0; i < multiline; i++) { sb.Append(multilineSeparator); }
                }
                else {
                    if (!first) {
                        sb.Append(',');
                        sb.Append(' ');
                    }
                }

                sb.Append(prop.Name);
                sb.Append('=');
                var propValue = prop.GetValue(instance, null);
                if (propValue != null)
                {
                    switch (propValue)
                    {
                        case Array e1:
                        {
                            if (multiline > 0)
                            {
                                sb.Append(newLine);
                                for (var i = 0; i < multiline + 1; i++)
                                {
                                    sb.Append(multilineSeparator);
                                }
                            }
                            sb.Append(StringUtil.FormatEnumerable(e1, multiline > 0, multiline > 0 ? multiline + 1 : 0, multiline > 0 ? multilineSeparator : ", "));
                            break;
                        }
                        // Check pour format dictionnaire
                        case IEnumerable<object> objects:
                        {
                            var e = objects;
                            if (multiline > 0)
                            {
                                sb.Append(newLine);
                                for (var i = 0; i < multiline + 1; i++)
                                {
                                    sb.Append(multilineSeparator);
                                }
                            }
                            sb.Append(StringUtil.FormatEnumerable(e, multiline > 0, multiline > 0 ? multiline + 1 : 0, multiline > 0 ? multilineSeparator : ", "));
                            break;
                        }
                        default:
                        {
                            if (propValue.GetType().Name.StartsWith("Dictionary"))
                            {
                                var dic = propValue as IDictionary;
                                if (multiline > 0)
                                {
                                    sb.Append(newLine);
                                    for (var i = 0; i < multiline + 1; i++)
                                    {
                                        sb.Append(multilineSeparator);
                                    }
                                }
                                sb.Append(StringUtil.FormatDictionary(dic, multiline > 0, multiline > 0 ? multiline + 1 : 0, multiline > 0 ? multilineSeparator : ", "));
                            }
                            // Spécification pour nombres à virgule flottante.
                            // https://docs.microsoft.com/en-us/dotnet/standard/base-types/standard-numeric-format-strings
                            else switch (propValue)
                            {
                                case float value:
                                {
                                    double f = value;
                                    sb.Append(f.ToString("R",
                                        CultureInfo.InvariantCulture)); // Format d'écriture spéciale applicable juste pour double
                                    break;
                                }
                                case double value:
                                {
                                    var d = value;
                                    sb.Append(d.ToString("R", CultureInfo.InvariantCulture));
                                    break;
                                }
                                default:
                                    sb.Append(propValue);
                                    break;
                            }

                            break;
                        }
                    }
                }
                else
                {
                    sb.Append("");
                }

                first = false;
            }

            if (multiline == 0) sb.Append(']');

            return sb.ToString();
        }

        /// <summary>
        /// Désérialise les propriétés publiques d'un objet en dictionnaire string-string. Est approximativement une désésiralisation en format JSON.
        /// N'est pratique qu'avec des propriétés simples (i.e. qui ne sont pas elles-mêmes
        /// des objets complexes, à moins que leur "ToString" donne un dictionnaire string-string).
        /// Si la propriété est annotée avec l'attribut de type "JsonPropertyAttribute", on prend le nom de cette propriété à la place.
        /// </summary>
        /// <param name="instance">L'instance à sérialiser.</param>
        /// <param name="toLowerCase">Indique qu'il faut mettre les propriétés en case minuscule. n'affecte pas les propriétés avec l'attribut JsonPropertyAttribute.</param>
        /// <returns></returns>
        public static Dictionary<string, string> ConvertPropertiesToDictionary([NotNull] object instance, bool toLowerCase = true)
        {
            var resultDic = new Dictionary<string, string>();

            foreach (var prop in instance.GetType().GetProperties())
            {
                var propName = prop.Name;
                if (toLowerCase) propName = propName.ToLower();

                if (prop.GetCustomAttributes(typeof(JsonPropertyAttribute), false).FirstOrDefault() is JsonPropertyAttribute attr) propName = attr.PropertyName;

                if (!prop.CanRead) continue;
                var value = prop.GetValue(instance, null);
                // ReSharper disable once AssignNullToNotNullAttribute
                resultDic.Add(propName, value?.ToString() ?? "");
            }
            
            return resultDic;
        }

        /// <summary>
        /// Appelle "Dispose" sur tout les éléments d'un énumérable.
        /// </summary>
        /// <param name="disposableList">La liste des éléments à détruire.</param>
        public static void DisposeList(IEnumerable<IDisposable> disposableList)
        {
            if (disposableList == null) return;
            foreach (var idisp in disposableList)
            {
                idisp?.Dispose();
            }
        }

        /// <summary>
        /// Pour obtenir le type d'une énumération. On dirait que le getType de base ne fonctionne quand dans un sous-ensemble défini.
        /// Voir ici: https://stackoverflow.com/questions/25404237/how-to-get-enum-type-by-specifying-its-name-in-string
        /// </summary>
        /// <param name="name">Le nom du type énumératif.</param>
        /// <returns>Le type, si existant.</returns>
        public static Type GetEnumType(string name)
        {
            return
                (from assembly in AppDomain.CurrentDomain.GetAssemblies()
                    let type = assembly.GetType(name)
                    where type != null
                          && type.IsEnum
                    select type).FirstOrDefault();
        }

        /// <summary>
        /// Cherche une valeur de propriété dans un instance, et dans les instances des valeurs de ces propriétés via le nom d'une propriété.
        /// Par exemple, un objet "A" qui contient une proriété nommé "val" de type B, qui contient lui-même une propriété nommé "val2" de type C.
        /// On donne en paramètre l'instance de A, avec la 'clé' de propriété "val.val2" pour aller chercher la valeur de la propriété val2
        /// de l'objet B contenu par A.
        /// La clé de propriété est séparée par des points (comme dans un nom de namespace). On fait toujours la recherche via le côté gauche du premier point,
        /// Et s'il existe une partie de droite (sous propriétés), on va appeller procéder à un appel "résursif" de cette même méthode pour aller plus loin,
        /// du moment que la propriété à chercher existe et contienne un valeur non null. On répète jusqu'à ce que l'on atteigne la fin de cette chaîne.
        /// Par exemple, si une clé contient trois points (comme "Prop1.Prop2.Prop3.Prop4"), cela fait quatre recherches successives, en assument qu'aucune valeur
        /// ne soit nulle en chemin.
        /// Une valeur nulle est valide. Elle sera retournée via le paramètre out "outValue" avec le retour "true".
        /// Le seul cas où cette méthode retourne "false" est quand un nom de propriété n'existe pas. Ce nom n'est pas obligé de respecter la case.
        /// Arrête la recherche récursive dès qu'une valeur nulle est rencontrée dans le cas d'une recherche profonde.
        /// Cas spécial: si une propriété est de type 'IDictionary(string, object)', alors une clé nommée "prop.index.XYZ" fait référence comme suit:
        /// "prop" est le nom de la propriété de type dictionnaire, et le reste ("index.XYZ") est la clé de recherche dans ce dictionnaire.
        /// Dans ce cas, même si ce reste contient d'autre points, il est considéré comme une clé complète et aucune recherche subséquente n'aura lieu.
        /// TODO Il serait possible de faire en sorte que ce reste peut référencer un index, puis continuer. Par exemple "[index.XYZ].Prop2"
        /// </summary>
        /// <param name="instance">L'instance à analyser.</param>
        /// <param name="propertyKey">La clé de noms de propriété à analyser.</param>
        /// <param name="outValue">La valeur trouvée de la propriété finale. Peut être nulle si à tout moment l'une des propriété de la chaîne est d'une valeur nulle.</param>
        /// <returns>Ne retourne FALSE que si la clé contient un nom de propriété inexistant.</returns>
        public static bool FindPropertyValue([NotNull] this object instance, [NotNull] string propertyKey, out object outValue)
        {
            while (true) // Pseudo-récursif
            {
                var splitIndex = propertyKey.IndexOf('.');
                var leftProp = splitIndex < 1 ? propertyKey : propertyKey.Substring(0, splitIndex);
                var nextProp = splitIndex < 1 ? null : propertyKey.Substring(splitIndex + 1);

                var objType = instance.GetType();
                var objPropList = objType.GetProperties();

                var props = objPropList.Where(p => p.Name.Equals(leftProp, StringComparison.InvariantCultureIgnoreCase)).ToList();
                if (props.Count == 0)
                {
                    log.Error("Did not find property named '{0}' in object type '{1}'", leftProp, instance.GetType().FullName);
                    outValue = null;
                    return false;
                }

                var propObjValue = props[0].GetValue(instance, null);

                if (propObjValue is IDictionary<string, object> propDic)
                {
                    if (nextProp != null && propDic.ContainsKey(nextProp))
                    {
                        outValue = propDic[nextProp];
                    }
                    else
                    {
                        log.Trace("Dictionary property ({0}) in type ({1}) does not contain the following key ({2})", leftProp, instance.GetType().FullName, nextProp);
                        outValue = null;
                        return true; // c'est une éventualité très possible.
                    }
                }

                if (nextProp != null)
                {
                    if (propObjValue == null)
                    {
                        log.Debug("There is a follow-up property to search ({0}), but property value is null for property named '{1}'. Aborting search.", nextProp, leftProp);
                        outValue = null;
                        return true; // Still normal use-case
                    }

                    // Il faut chercher dans une propriété de l'objet référencé par l'instance. Appel pseudo-récursif.
                    propertyKey = nextProp;
                    instance = propObjValue;
                    continue;
                }

                outValue = propObjValue;
                return true;
            }
        }

        /// <summary>
        /// Cette méthode va chercher les propriétés d'une instance, via réflexion, qui implémentent tous les types données en paramètres.
        /// Un objets peut avoir plusieurs types (héritage, interfaces) malgré le fait qu'une classe ne peut hériter que d'un type en C#.
        /// </summary>
        /// <param name="instance">L'objet à tester.</param>
        /// <param name="typesToMatch">Les types à vérifier. À noter que les propriétés doivent implémenter TOUT les types simultanément.</param>
        /// <returns>Un énumérable non-nul, mais possiblement vide.</returns>
        public static IEnumerable<PropertyInfo> GetPropertiesFromTypes(object instance, params Type[] typesToMatch)
        {
            if (instance == null || !(typesToMatch?.Any() ?? false)) return Array.Empty<PropertyInfo>();
            var type = instance.GetType();

            return type.GetProperties().Where(p => typesToMatch.All(t => t.IsAssignableFrom(p.PropertyType)));
        }

        /// <summary>
        /// Cette méthode tente de trouver, par réflexion, la première instance non nulle d'un type spécifique sur un objet.
        /// Si l'objet lui-même est de ce type, il sera retourné en tant que ce type spécifique par référence.
        /// Sinon, on tente de trouver parmi ses propriétés la première qui correspond au type recherché et qui retourne
        /// une valeur non nulle.
        /// Si malgré ceci aucune référence n'est trouvée, retourne nul.
        /// On ne peut faire de recherche sur de multiples type simultanément. Pour cette fonctionalité, utilisez la méthode "GetPropertiesFromTypes".
        /// </summary>
        /// <typeparam name="T">Le type inféré à chercher.</typeparam>
        /// <param name="instance">L'instance sur laquelle faire la recherche.</param>
        /// <returns></returns>
        public static T GetFirstValidPropertyValueFromInstance<T>([NotNull] object instance)
        {
            if (instance is T val) return val;

            var genType = typeof(T);
            var tmp = GetPropertiesFromTypes(instance, genType)
                ?.Select(p => p?.GetValue(instance, null))
                .FirstOrDefault(p => p != null);

            if (tmp == null)
            {
                return default;
            }

            return (T) tmp;
        }
        
        /// <summary>
        /// Vérifie si une classe existe, avec vérification de namespace dans les assemblages chargées en mémoire.
        /// On peut donner une liste de types pour vérifier la parentée de la classe (I.E. étent-elle une classe spécifique? Implémente-t-elle une interface spécifique?)
        /// </summary>
        /// <param name="className">Le nom de la classe. Peut contenir le namespace complet.</param>
        /// <param name="baseNamespace">En cas d'écehec, donne une namespace précis pour chercher l'assemblage contenant.</param>
        /// <param name="implementedTypeCheck">Types parents ou interfaces qui doivent être implémentée.</param>
        /// <returns>Une instance du Type, si existant. NUll sinon.</returns>
        public static Type CheckIfClassExists(string className, string baseNamespace = null, params Type[] implementedTypeCheck)
        {
            if (className == null) return null;
            var clazz = StringUtil.GetClassName(className);
            
            // Vérification 1: assembly actuelle
            var t = Type.GetType(clazz) ?? Type.GetType($"{baseNamespace}.{clazz}");

            // Avant la deuxième vérification, si on n'a pas donnée de namespace, on vérifie si on peut en déterminer un via le nom de classe direct.
            if (string.IsNullOrWhiteSpace(baseNamespace))
            {
                var li = className.LastIndexOf('.');
                if (li >= 0)
                {
                    baseNamespace = className.Substring(0, li);
                }
            }

            // Vérification 2: assembly chargée en mémoire. Requiert le deuxième paramètre pour fonctionner.
            if (!string.IsNullOrWhiteSpace(baseNamespace))
            {
                var assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetTypes().Any(t1 => t1.Namespace?.StartsWith(baseNamespace) ?? false));
                t = assembly?.GetType($"{baseNamespace}.{clazz}");
            }

            if (t == null) return null;

            if (implementedTypeCheck?.Any() ?? false)
            {
                if (!t.InheritsOrImplements(implementedTypeCheck))
                {
                    return null;
                }
            }

            return t;
        }

        /// <summary>
        /// Vérifie si un Type hérite de plusieurs autres types, incluant le même type.
        /// Tous doivent être vrai.
        /// </summary>
        /// <param name="child">Type de base.</param>
        /// <param name="parents">Liste de Type parents à vérifier.</param>
        /// <returns>True si les parents sont tous effectivement un ancêtre du type de base.</returns>
        public static bool InheritsOrImplements(this Type child, Type[] parents)
        {
            return parents.All(p => InheritsOrImplements(child, p));
        }

        /// <summary>
        /// Vérifie si un Type hérite d'une autre type, ou est le même type.
        /// </summary>
        /// <param name="child">Type de base.</param>
        /// <param name="parent">Type parent à vérifier.</param>
        /// <returns>True si le parent est effectivement un ancêtre du type de base.</returns>
        public static bool InheritsOrImplements(this Type child, Type parent)
        {
            parent = ResolveGenericTypeDefinition(parent);

            var currentChild = child.IsGenericType
                ? child.GetGenericTypeDefinition()
                : child;

            while (currentChild != typeof (object))
            {
                if (parent == currentChild || HasAnyInterfaces(parent, currentChild))
                    return true;

                currentChild = currentChild.BaseType != null
                               && currentChild.BaseType.IsGenericType
                    ? currentChild.BaseType.GetGenericTypeDefinition()
                    : currentChild.BaseType;

                if (currentChild == null)
                    return false;
            }
            return false;
        }

        /// <summary>
        /// Vérifie si le type parent est une interface du type enfant.
        /// </summary>
        /// <param name="parent">Le type à vérifier. Si ce n'est pas une interface, retourne immédiatement faux.</param>
        /// <param name="child">Le type de base.</param>
        /// <returns>TRUE si le parent est une interface et fait partie des interfaces de l'enfant.</returns>
        public static bool HasAnyInterfaces(Type parent, Type child)
        {
            if (!parent.IsInterface) return false;

            return child.GetInterfaces()
                .Any(childInterface =>
                {
                    var currentInterface = childInterface.IsGenericType
                        ? childInterface.GetGenericTypeDefinition()
                        : childInterface;

                    return currentInterface == parent;
                });
        }

        /// <summary>
        /// Vérifie si on doit utiliser la définition de type générique d'un parent, dans la vérification d'héritage de type.
        /// </summary>
        /// <param name="parent">Le type à analyser.</param>
        /// <returns>Le type qui doit être utilisé comme test d'héritage. Peut être le même que le paramètre si n'est pas générique.</returns>
        private static Type ResolveGenericTypeDefinition(Type parent)
        {
            var shouldUseGenericType = !(parent.IsGenericType && parent.GetGenericTypeDefinition() != parent);

            if (parent.IsGenericType && shouldUseGenericType)
                parent = parent.GetGenericTypeDefinition();
            return parent;
        }

        /// <summary>
        /// Fait une comparaison élément par élément de deux énumérables dont les types peuvent se comparer entre eux (IComparable).
        /// S'il est possible de savoir si les deux liste ne contiennent pas le même nombre d'éléments, il faudrait ne pas invoquer cette méthode pour éviter O(N)
        /// comparaisons.
        /// Dès qu'un seul élément n'est pas strictement égal, retourne le résultat de la comparaison.
        /// </summary>
        /// <typeparam name="T">Le type contenu dans les énumérables.</typeparam>
        /// <param name="enuA">Énumérable A</param>
        /// <param name="enuB">Énum.rable B</param>
        /// <returns>Selon les règles de comparaison.</returns>
        public static int CompareEnumerables<T>([NotNull] this IEnumerable<T> enuA, [NotNull] IEnumerable<T> enuB) where T : IComparable<T>
        {
            using (IEnumerator<T> iteA = enuA.GetEnumerator(), iteB = enuB.GetEnumerator())
            {
                while (iteA.MoveNext())
                {
                    if (!iteB.MoveNext())
                    {
                        return 1; // S'il manque des éléments dans A, A est "moins grand"
                    }

                    var curA = iteA.Current;
                    var curB = iteB.Current;

                    if (curA == null && curB == null) continue;
                    if (curA == null) return 1;
                    if (curB == null) return -1;

                    var res = curA.CompareTo(curB);
                    if (res != 0) return res;
                }

                if (iteB.MoveNext()) return -1; // S'il reste des éléments dans B, B est "plus grand"
                return 0;
            }
        }

        /// <summary>
        /// Fait une comparaison élément par élément de deux énumérables dont les types peuvent se comparer entre eux (IComparable).
        /// Dès qu'un seul élément n'est pas strictement égal, retourne le résultat de la comparaison.
        /// </summary>
        /// <typeparam name="T">Le type contenu dans les énumérables.</typeparam>
        /// <param name="listA">Énumérable A</param>
        /// <param name="listB">Énum.rable B</param>
        /// <returns>Selon les règles de comparaison.</returns>
        public static int CompareEnumerables<T>([NotNull] this IList<T> listA, [NotNull] IList<T> listB) where T : IComparable<T>
        {
            var count = listA.Count - listB.Count;
            return count != 0 ? count : CompareEnumerables((IEnumerable<T>)listA, listB);
        }

        /// <summary>
        /// Fait une comparaison élément par élément de deux énumérables avec l'aide d'un comparateur explicite.
        /// S'il est possible de savoir si les deux liste ne contiennent pas le même nombre d'éléments, il faudrait ne pas invoquer cette méthode pour éviter O(N)
        /// comparaisons.
        /// Dès qu'un seul élément n'est pas strictement égal, retourne le résultat de la comparaison.
        /// </summary>
        /// <typeparam name="T">Le type contenu dans les énumérables.</typeparam>
        /// <param name="enuA">Énumérable A</param>
        /// <param name="enuB">Énum.rable B</param>
        /// <param name="comparer">Le comparateur d'élément.</param>
        /// <returns>Selon les règles de comparaison.</returns>
        public static int CompareEnumerables<T>([NotNull] this IEnumerable<T> enuA, [NotNull] IEnumerable<T> enuB, [NotNull] IComparer<T> comparer)
        {
            using (IEnumerator<T> iteA = enuA.GetEnumerator(), iteB = enuB.GetEnumerator())
            {
                while (iteA.MoveNext())
                {
                    if (!iteB.MoveNext())
                    {
                        return 1; // S'il manque des éléments dans A, A est "moins grand"
                    }

                    var curA = iteA.Current;
                    var curB = iteB.Current;
                    
                    var res = comparer.Compare(curA, curB);
                    if (res != 0) return res;
                }

                if (iteB.MoveNext()) return -1; // S'il reste des éléments dans B, B est "plus grand"
                return 0;
            }
        }

        /// <summary>
        /// Fait une comparaison élément par élément de deux énumérables avec l'aide d'un comparateur explicite.
        /// Dès qu'un seul élément n'est pas strictement égal, retourne le résultat de la comparaison.
        /// </summary>
        /// <typeparam name="T">Le type contenu dans les énumérables.</typeparam>
        /// <param name="listA">Énumérable A</param>
        /// <param name="listB">Énum.rable B</param>
        /// <param name="comparer">Le comparateur explicite.</param>
        /// <returns>Selon les règles de comparaison.</returns>
        public static int CompareEnumerables<T>([NotNull] this IList<T> listA, [NotNull] IList<T> listB, [NotNull] IComparer<T> comparer)
        {
            var count = listA.Count - listB.Count;
            return count != 0 ? count : CompareEnumerables((IEnumerable<T>)listA, listB, comparer);
        }

        /// <summary>
        /// Effectue une comparaison entre objets nullables, si ces objets sont comparables entre eux.
        /// Un objet null est considéré "plus petit", et on compare a envers b.
        /// </summary>
        /// <typeparam name="T">Le type de objets (inférés)</typeparam>
        /// <param name="a">instance A</param>
        /// <param name="b">instance B</param>
        /// <returns>Le résultat de la comparaison</returns>
        public static int CompareObjects<T>([CanBeNull] T a, [CanBeNull] T b) where T : IComparable<T>
        {
            if (a == null && b == null) return 0;
            if (a == null) return -1;
            return b == null ? 1 : a.CompareTo(b);
        }

        // TODO: À activer quand on augmente à C# 8.0
        /*
        public static int CompareObjects<T>(T a, T b) where T : notnull, IComparable<T>
        {
            if (a == null && b == null) return 0;
            if (a == null) return -1;
            return b == null ? 1 : a.CompareTo(b);
        }
        */

        /// <summary>
        /// Effectue une comparaison entre objets nullables, si ces objets sont comparables entre eux.
        /// Un objet null est considéré "plus petit", et on compare a envers b.
        /// </summary>
        /// <typeparam name="T">Le type de objets (inférés)</typeparam>
        /// <param name="a">instance A</param>
        /// <param name="b">instance B</param>
        /// <returns>Le résultat de la comparaison</returns>
        public static int CompareObjects<T>([CanBeNull] IEnumerable<T> a, [CanBeNull] IEnumerable<T> b) where T : IComparable<T>
        {
            if (a == null && b == null) return 0;
            if (a == null) return -1;
            return b == null ? 1 : a.CompareEnumerables(b);
        }

        /// <summary>
        /// Effectue une comparaison entre objets nullables, si ces objets sont comparables entre eux.
        /// Un objet null est considéré "plus petit", et on compare a envers b.
        /// </summary>
        /// <typeparam name="T">Le type de objets (inférés)</typeparam>
        /// <param name="a">instance A</param>
        /// <param name="b">instance B</param>
        /// <returns>Le résultat de la comparaison</returns>
        public static int CompareObjects<T>([CanBeNull] IList<T> a, [CanBeNull] IList<T> b) where T : IComparable<T>
        {
            if (a == null && b == null) return 0;
            if (a == null) return -1;
            return b == null ? 1 : a.CompareEnumerables(b);
        }

        /// <summary>
        /// Pour calculer l'empreinte mémoire d'une instance d'objet.
        /// Voir ici pour plus d'explications: https://www.codeproject.com/Tips/876878/Calculating-Optimistic-Memory-Footprint-of-Managed
        /// </summary>
        /// <param name="someObject">L'instance à analyser.</param>
        /// <returns></returns>
        public static long SizeInBytes<T>(this T someObject)
        {
            var temp = new Size<T>(someObject);
            var tempSize = temp.GetSizeInBytes();
            return tempSize;
        }

        /// <summary>
        /// Instancie une classe via son nom complet (ou son nom simple mais avec un "namespace" candidat), et "cast"
        /// son type immédiatement en prenant soin de vérifier que c'est possible de le faire.
        /// </summary>
        /// <typeparam name="T">Le type à "caster" l'instance.</typeparam>
        /// <param name="fullClassName">Le nom complet de la classe.</param>
        /// <param name="candidateNameSpace">Si le nom complet est invalide, on tente de trouver la classe avec un namespace candidat fournit ici, de façon optionnel.
        /// Ne doit pas se terminer par un point.</param>
        /// <returns>Si tout va bien, l'instance crée.</returns>
        public static T InstanciateClassFromName<T>(string fullClassName, string candidateNameSpace = null)
        {
            if (string.IsNullOrWhiteSpace(fullClassName)) return default;



            var typesCandidate = GetTypeByName(fullClassName);

            if (!typesCandidate.Any())
            {
                if (string.IsNullOrWhiteSpace(candidateNameSpace)) // Pas de test candidat
                {
                    log.Warn("No instanciable class with the following class descriptor: " + fullClassName);
                    return default;
                }

                var onlyName = StringUtil.GetClassName(fullClassName);

                //var fullNamespaceName = candidateNameSpace + onlyName;
                var fullNamespaceName = candidateNameSpace;
                if (!fullNamespaceName.EndsWith("."))
                {
                    fullNamespaceName += ".";
                }
                fullNamespaceName += onlyName;
                typesCandidate = GetTypeByName(fullNamespaceName);

                if (!typesCandidate.Any())
                {
                    log.Warn("No instanciable class with the following class descriptor: " + fullNamespaceName);
                    return default;
                }
            }

            // Vérifie si on a plusieurs types possibles.
            if (typesCandidate.Length > 1)
            {
                log.Warn("Too many possible classes found: " + StringUtil.FormatEnumerable(typesCandidate.GetEnumerator()));
                return default;
            }

            var type = typesCandidate[0];

            // Vérification du type cast.
            var wanted = typeof(T);
            if (!wanted.IsAssignableFrom(type))
            {
                log.Warn($"Found type cannot be cast to predetermined final type.\n\t-Wanted: {wanted.AssemblyQualifiedName}. But was:{type.AssemblyQualifiedName}");
                return default;
            }

            var inst = (T)Activator.CreateInstance(type);

            return inst;
        }

        /// <summary>
        /// Gets a all Type instances matching the specified class name with just non-namespace qualified class name.
        /// See: https://stackoverflow.com/questions/9273629/avoid-giving-namespace-name-in-type-gettype
        /// </summary>
        /// <param name="className">Name of the class sought.</param>
        /// <returns>Types that have the class name specified. They may not be in the same namespace.</returns>
        public static Type[] GetTypeByName(string className)
        {
            var returnVal = new List<Type>();

            foreach (var a in AppDomain.CurrentDomain.GetAssemblies())
            {
                var assemblyTypes = a.GetTypes();
                returnVal.AddRange(assemblyTypes.Where(t => t.Name == className || t.AssemblyQualifiedName == className));
            }

            return returnVal.ToArray();
        }

        /// <summary>
        /// Méthode pour implémenter efficacement une façon de faire une vérification d'unicité d'une propriété, à la "LinQ".
        /// Voir ici pour une discussion de cette méthode, modifiée:
        /// https://stackoverflow.com/questions/489258/linqs-distinct-on-a-particular-property
        /// </summary>
        /// <typeparam name="TSource">Type de l'énumération.</typeparam>
        /// <typeparam name="TKey">Type de la clée à vérifier l'unicité.</typeparam>
        /// <param name="source">L'énumération source</param>
        /// <param name="keySelector">Ce sélecteur doit retourner un élément qui est unique via HASH.</param>
        /// <returns>TRUE si tous les éléments sont unique (via HASH) entre eux.</returns>
        public static bool AllUniqueBy<TSource, TKey>
            (this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            var seenKeys = new HashSet<TKey>();
            return source.All(element => seenKeys.Add(keySelector(element)));
        }

        /// <summary>
        /// Méthode pour implémenter efficacement une façon de faire un énumérable de propriétés distinctes, à la "LinQ".
        /// Voir ici pour une discussion de cette méthode:
        /// https://stackoverflow.com/questions/489258/linqs-distinct-on-a-particular-property
        /// </summary>
        /// <typeparam name="TSource">Type de l'énumération.</typeparam>
        /// <typeparam name="TKey">Type de la clée.</typeparam>
        /// <param name="source">L'énumération source</param>
        /// <param name="keySelector">Ce sélecteur doit retourner un élément qui est compatible via HASH.</param>
        /// <returns>Une énumération de chaque instance distincte des clés.</returns>
        public static IEnumerable<TSource> DistinctBy<TSource, TKey>
            (this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            var seenKeys = new HashSet<TKey>();
            foreach (var element in source)
            {
                if (seenKeys.Add(keySelector(element)))
                {
                    yield return element;
                }
            }
        }
    }

    /// <summary>
    /// Classe interne pour tenter de calculer l'empreinte mémoire d'une instance de classe.
    /// Utilise la récursivité.
    /// </summary>
    /// <typeparam name="TT"></typeparam>
    internal class Size<TT>
    {
        private readonly TT _obj;
        private readonly HashSet<object> _references;
        // ReSharper disable once StaticMemberInGenericType
        private static readonly int PointerSize = 
        Environment.Is64BitOperatingSystem ? sizeof(long) : sizeof(int);
        public Size(TT obj)
        {
            _obj = obj;
            _references = new HashSet<object>() { _obj };
        }
        public long GetSizeInBytes()
        {
            return GetSizeInBytes(_obj);
        }

        // The core functionality. Recurrently calls itself when an object appears to have fields 
        // until all fields have been  visited, or were "visited" (calculated) already.
        private long GetSizeInBytes<T>(T obj)
        {
            if (obj == null) return sizeof(int);
            var type = obj.GetType();

            if (type.IsPrimitive)
            {
                // ReSharper disable once SwitchStatementMissingSomeCases
                switch (Type.GetTypeCode(type))
                {
                    case TypeCode.Boolean:
                    case TypeCode.Byte:
                    case TypeCode.SByte:
                        return sizeof(byte);
                    case TypeCode.Char:
                        return sizeof(char);
                    case TypeCode.Single:
                        return sizeof(float);
                    case TypeCode.Double:
                        return sizeof(double);
                    case TypeCode.Int16:
                    case TypeCode.UInt16:
                        return sizeof(Int16);
                    case TypeCode.Int32:
                    case TypeCode.UInt32:
                        return sizeof(Int32);
                    case TypeCode.Int64:
                    case TypeCode.UInt64:
                    default:
                        return sizeof(Int64);
                }
            }

            switch (obj)
            {
                case decimal _:
                    return sizeof(decimal);
                case string _:
                    return sizeof(char) * obj.ToString().Length;
                default:
                {
                    if (type.IsEnum)
                    {
                        return sizeof(int);
                    }
                    long size = PointerSize;
                    if (type.IsArray)
                    {
                        
                        var casted = (IEnumerable)obj;
                        size += casted.Cast<object>().Sum(GetSizeInBytes);
                        return size;
                    }
                    if (obj is Pointer)
                    {
                        return PointerSize;
                    }

                    size = 0;
                    var t = type;
                    while (t != null)
                    {
                        size += PointerSize;
                        var fields = t.GetFields(BindingFlags.Instance | BindingFlags.Public | 
                                                 BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
                        foreach (var field in fields)
                        {
                            var tempVal = field.GetValue(obj);
                            if (_references.Contains(tempVal)) continue;

                            _references.Add(tempVal);
                            size += GetSizeInBytes(tempVal);
                        }
                        t = t.BaseType;
                    }
                    return size;
                }
            }
        }
    }
}
