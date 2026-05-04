using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;

namespace NomadLogic.Common.Util.Json
{
    #region Classes Utilitaires ou d'extensions

    /// <summary>
    /// Cette classe permet de contourner un limitation de format JSON qui commencent par une liste sans définir un "nom" pour cette propriété, une peut
    /// comme la marshallisation d'une liste générique où d'un dictionnaire.
    /// Voir ici pour plus de détails: https://stackoverflow.com/questions/18192357/deserializing-json-object-array-with-json-net
    /// </summary>
    [PublicAPI]
    public class JsonGenericListConverter : JsonConverter
    {
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var token = JToken.Load(reader);
            var list = Activator.CreateInstance(objectType) as System.Collections.IList;
            var itemType = objectType.GenericTypeArguments[0];
            foreach (var child in token.Values())
            {
                var childToken = child.Children().First();
                var newObject = Activator.CreateInstance(itemType);
                serializer.Populate(childToken.CreateReader(), newObject);
                list?.Add(newObject);
            }
            return list;
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType.IsGenericType && (objectType.GetGenericTypeDefinition() == typeof(List<>));
        }
        public override bool CanWrite => false;
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) => throw new NotImplementedException();
    }

    #endregion

    /// <summary>
    /// Classe utilitaire pour la manipulation de configuration JSON vers des instance complètes de POCO.
    /// </summary>
    [PublicAPI]
    public static class JsonUtil
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Configuration de base de (dé)sérialisation.
        /// On ignore les éléments nulls.
        /// </summary>
        public static readonly JsonSerializerSettings BASE_SERIALIZER_SETTINGS = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore
        };

        /// <summary>
        /// Permet d'initialiser une classe via un fichier de configuration par défaut, spécifié par l'interface.
        /// On peut utiliser un chemin de fichier précis par paramètre si l'on veut. S'il n'existe pas, on retourne une instance par défaut.
        /// Si on utilise le chemin par défaut, et que le chemin pointe vers un fichier inexistant, on va créer une instance par défaut de ce fichier.
        /// </summary>
        /// <typeparam name="T">Le type de la classe. Doit posséder un constructeur vide et doit implémenté une méthode pour chercher le fichier config par défaut.</typeparam>
        /// <param name="filePath">Optionel. Un chemin vers un fichier précis.</param>
        /// <param name="settings">Optionel. La configuration de désérialisation. Par défaut, les options BASE_SERIALIZER_SETTINGS de cette classe.</param>
        /// <returns>Dans tout les cas, une instance valide est retournée (sauf si erreur de lecture). Si le fichier existe, cette instance aura les configuration du fichier.</returns>
        public static T CreateInstanceFromJsonConfigFile<T>(string filePath = null, JsonSerializerSettings settings = null) where T : IJsonConfig, new()
        {
            var tmp = new T();

            var file = new FileInfo(filePath ?? DirectoryUtil.GetLocalApplicationDir() + tmp.DefaultConfigFileName);

            if (file.Exists)
            {
                UpdateFromJson(file, tmp, settings);
                
            }
            if (!file.Exists)
            {
                log.Trace($"No default file found: {file.FullName}");
            }

            return tmp;
        }

        /// <summary>
        /// Permet d'initialiser une classe via un fichier de configuration par défaut, spécifié par l'interface.
        /// On peut utiliser un chemin de fichier précis par paramètre si l'on veut. S'il n'existe pas, on retourne une instance par défaut.
        /// Si on utilise le chemin par défaut, et que le chemin pointe vers un fichier inexistant, on va créer une instance par défaut de ce fichier.
        /// </summary>
        /// <typeparam name="T">Le type de la classe. Doit posséder un constructeur vide et doit implémenté une méthode pour chercher le fichier config par défaut.</typeparam>
        /// <param name="file">Optionel. Un chemin vers un fichier précis.</param>
        /// <param name="settings">Optionel. La configuration de désérialisation. Par défaut, les options BASE_SERIALIZER_SETTINGS de cette classe.</param>
        /// <returns>Dans tout les cas, une instance valide est retournée (sauf si erreur de lecture). Si le fichier existe, cette instance aura les configuration du fichier.</returns>
        public static T CreateInstanceFromJsonConfigFile<T>(FileInfo file, JsonSerializerSettings settings = null) where T : IJsonConfig, new()
        {
            return CreateInstanceFromJsonConfigFile<T>(file?.FullName, settings);
        }

        /// <summary>
        /// Retourne une instance complète de la classe demandée à partir d'un fichier sur disque de type JSON.
        /// </summary>
        /// <param name="file">Le fichier à lire comme source.</param>
        /// /// <param name="settings">Optionel. La configuration de désérialisation. Par défaut, les options BASE_SERIALIZER_SETTINGS de cette classe.</param>
        /// <returns>Retourne une instance de la classe, mais si fichier inexistant ou erreur, la valeur par défaut du type.</returns>
        public static T ReadJsonFile<T>(FileInfo file, JsonSerializerSettings settings = null)
        {
            var data = default(T);
            
            if (file != null && file.Exists)
            {
                string fileTxt = null;
                try
                {
                    fileTxt = File.ReadAllText(file.FullName);
                    data = JsonConvert.DeserializeObject<T>(fileTxt, settings ?? BASE_SERIALIZER_SETTINGS);
                }
                catch (Exception e)
                {
                    log.Error(e, "Could not read JSON file for type {0}: {1}", typeof(T).FullName, file);
                    if (log.IsDebugEnabled && !string.IsNullOrWhiteSpace(fileTxt))
                    {
                        log.Debug(fileTxt);
                    }
                }
            }
            else
            {
                log.Warn("File does not exists. Returning default value for type: " + typeof(T).FullName);
                
            }

            return data;
        }

        /// <summary>
        /// Méthode pour écrire cette instance dans un fichier JSON.
        /// </summary>
        /// <param name="file">Le fichier à écrire</param>
        /// <param name="instance">L'instance à analyser.</param>
        /// <param name="settings">Optionel. La configuration de désérialisation. Par défaut, les options BASE_SERIALIZER_SETTINGS de cette classe.</param>
        /// <returns>TRUE si aucune erreur ne s'est produite.</returns>
        public static bool WriteJsonFile<T>(FileInfo file, T instance, JsonSerializerSettings settings = null)
        {
            if (file == null || instance == null) { return false; }

            var error = false;

            try
            {
                // Création des répertoires intermédiares
                // ReSharper disable once AssignNullToNotNullAttribute
                Directory.CreateDirectory(file.DirectoryName);
                
                if (settings == null) { settings = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }; }

                var serializer = JsonSerializer.CreateDefault(settings);

                using (var sw = new StreamWriter(file.FullName))
                {
                    using (JsonWriter writer = new JsonTextWriter(sw))
                    {
                        writer.Formatting = Formatting.Indented;
                        serializer.Serialize(writer, instance);
                    }
                }
            }
            catch (Exception e)
            {
                error = true;
                log.Error(e, "Could not write JSON file for type {0}: {1}", typeof(T).FullName, file);
            }

            return !error;
        }

        /// <summary>
        /// Convertit un objet non null en string JSON.
        /// </summary>
        /// <typeparam name="T">Le type de l'objet à convertir.</typeparam>
        /// <param name="instance">L'instance à convertir.</param>
        /// <param name="settings">Les configurations de sérialisation JSON.</param>
        /// <returns>NULL s'il y a eu une erreur ou si l'instance est elle-même nulle. Sinon, un référence string JSON valide.</returns>
        public static string CreateJsonString<T>(T instance, JsonSerializerSettings settings = null)
        {
            if (instance == null) { return null; }
            
            try
            {
                if (settings == null) { settings = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }; }
                
                var str = JsonConvert.SerializeObject(instance, settings);
                return str;
            }
            catch (Exception e)
            {
                log.Error(e, "Could not convert to JSON string for type {0}", typeof(T).FullName);
                return null;
            }
        }

        /// <summary>
        /// Met à jour une instance déjà créée avec le contenu d'un fichier JSON.
        /// </summary>
        /// <typeparam name="T">Le type, inféré, de l'instance.</typeparam>
        /// <param name="file">Le nom de fichier d'où lire l'information.</param>
        /// <param name="instance">L'instance à mettre à jour.</param>
        /// <param name="settings">Optionel. La configuration de désérialisation. Par défaut, les options BASE_SERIALIZER_SETTINGS de cette classe.</param>
        /// <returns>TRUE si on a réussi à mettre à jour l'instance. False si c'était impossible (mauvais paramètres) ou si une erreur s'est produite.</returns>
        public static bool UpdateFromJson<T>(FileInfo file, T instance, JsonSerializerSettings settings = null)
        {
            if (file == null || !file.Exists || instance == null) { return false; }
            var error = false;
            try
            {
                var jsonString = File.ReadAllText(file.FullName);
                JsonConvert.PopulateObject(jsonString, instance, settings ?? BASE_SERIALIZER_SETTINGS);
            }
            catch (Exception e)
            {
                error = true;
                log.Error(e, "Could not interpret JSON file for type {0}: {1}", typeof(T).FullName, file);
            }

            return !error;
        }

        /// <summary>
        /// Met à jour une instance déjà créée avec le contenu d'une chaîne de caractères en format JSON.
        /// </summary>
        /// <typeparam name="T">Le type, inféré, de l'instance.</typeparam>
        /// <param name="jsonData">Les données JSON.</param>Se
        /// <param name="instance">L'instance à mettre à jour.</param>
        /// <param name="settings">Optionel. La configuration de désérialisation. Par défaut, les options BASE_SERIALIZER_SETTINGS de cette classe.</param>
        /// <returns>TRUE si on a réussi à mettre à jour l'instance. False si c'était impossible (mauvais format JSON ou instance nulle) ou si une erreur s'est produite.</returns>
        public static bool UpdateFromJson<T>(string jsonData, T instance, JsonSerializerSettings settings = null)
        {
            if (string.IsNullOrWhiteSpace(jsonData) || jsonData.Equals("null", StringComparison.InvariantCultureIgnoreCase) || instance == null) { return false; }
            var error = false;
            try
            {
                JsonConvert.PopulateObject(jsonData, instance, settings ?? BASE_SERIALIZER_SETTINGS);
            }
            catch (Exception e)
            {
                error = true;
                log.Error(e, "Could not interpret JSON data for type {0}: \n***********\n{1}\n************", typeof(T).FullName, jsonData);
            }

            return !error;
        }

        /// <summary>
        /// Créer une instance de type spécifique avec une référence de String..
        /// </summary>
        /// <typeparam name="T">Le type à désérialiser</typeparam>
        /// <param name="jsonData">La chaîne de caractères à désérialiser.</param>
        /// <param name="instance">La référence vers la nouvelle instance.</param>
        /// <param name="settings">Les configuration de sérialisation.</param>
        /// <returns>TRUE si la désérialisation à bien fonctionnée. Si FALSE, la référence sera DEFAULT.</returns>
        public static bool CreateInstanceFromJson<T>([NotNull] string jsonData, out T instance, JsonSerializerSettings settings = null)
        {
            if (string.IsNullOrWhiteSpace(jsonData) || jsonData.Equals("null", StringComparison.InvariantCultureIgnoreCase))
            {
                instance = default;
                return false;
            }

            using (var reader = new StringReader(jsonData))
            using (var jsonReader = new JsonTextReader(reader))
            {
                var suc = CreateInstanceFromJson<T>(jsonReader, out var inner, settings);
                instance = inner;
                return suc;
            }
        }

        /// <summary>
        /// Créer une instance de type spécifique avec un Stream Json.
        /// </summary>
        /// <typeparam name="T">Le type à désérialiser</typeparam>
        /// <param name="jsonStream">Le flux Json.</param>
        /// <param name="instance">La référence vers la nouvelle instance.</param>
        /// <param name="settings">Les configuration de sérialisation.</param>
        /// <returns>TRUE si la désérialisation à bien fonctionnée. Si FALSE, la référence sera DEFAULT.</returns>
        public static bool CreateInstanceFromJson<T>([NotNull] JsonTextReader jsonStream, out T instance, JsonSerializerSettings settings = null)
        {
            var serializer = JsonSerializer.Create(settings ?? BASE_SERIALIZER_SETTINGS);

            try
            {
                instance = serializer.Deserialize<T>(jsonStream);
            }
            catch(JsonReaderException e)
            {
                log.Error(e, $"Could not interpret JSON stream for type '{typeof(T).FullName}'");
                instance = default;
                return false;
            }

            return true;
        }

        /// <summary>
        /// Cette méthode générique va cherche une instance distincte sur disque, et va mettre à jour l'instance données en paramètre
        /// avec les valeur chargée sur disque, avec exceptions: les infos sur le fichier de chargement et les propriétés
        /// explicitement ignorée par l'engin de JSON de NewtonSoft.
        /// La méthode retourne FAUX uniquement si un fichier est détectée et que l'on n'a pas pu faire une mise à jour. On ne vérifie pas l'égalité;
        /// si on trouve une fichier valide, et que l'on peut transférer des données, on retourne vrai si aucun problème n'est arrivé.
        /// </summary>
        /// <typeparam name="T">Le type générique. Souvent implicite.</typeparam>
        /// <param name="instance">L'instance à oberver et à mettre à jour.</param>
        /// <param name="baseDirectory">Le répertoire de base pour chercher les fichiers.</param>
        /// <returns>TRUE uniquement si des modifications ont été apportée.</returns>
        public static bool UpdateInstanceFromFile<T>(T instance, DirectoryInfo baseDirectory) where T : IJsonConfig, IExternalFileConfig
        {
            return UpdateInstanceFromFile(instance, baseDirectory?.FullName);
        }

        /// <summary>
        /// Cette méthode générique va cherche une instance distincte sur disque, et va mettre à jour l'instance données en paramètre
        /// avec les valeur chargée sur disque, avec exceptions: les infos sur le fichier de chargement et les propriétés
        /// explicitement ignorée par l'engin de JSON de NewtonSoft.
        /// La méthode retourne FAUX si un fichier est détectée et que l'on n'a pas pu faire une mise à jour. On ne vérifie pas l'égalité;
        /// si on trouve une fichier valide, et que l'on peut transférer des données, on retourne vrai si aucun problème n'est arrivé.
        /// </summary>
        /// <typeparam name="T">Le type générique. Souvent implicite.</typeparam>
        /// <param name="instance">L'instance à oberver et à mettre à jour.</param>
        /// <param name="baseDirectory">Le répertoire de base pour chercher les fichiers.</param>
        /// <param name="debug">Si vrai, on va écrire sur disque dans un nouveau fichier, sans collisions, le contenu de l'instance modifiée.</param>
        /// <returns>TRUE uniquement si des modifications ont été apportée.</returns>
        public static bool UpdateInstanceFromFile<T>(T instance, string baseDirectory, bool debug = false) where T : IJsonConfig, IExternalFileConfig
        {
            if (instance == null || instance.ConfigFileName == null && instance.DefaultConfigFileName == null) return false;

            // On détermine si un fichier précis existe.
            var file = FileUtil.FindFirstFilePath(baseDirectory, instance.ConfigFileName, instance.DefaultConfigFileName);
            if (file == null)
            {
                log.Debug("No file exist to update. Exiting.");
                return false; // Pas de fichier découvert.
            }

            // Charge le fichier en mémoire
            var fromFileInstance = ReadJsonFile<T>(file);
            if (fromFileInstance == null) return false; // JSON invalide

            var typeFromFile = fromFileInstance.GetType();
            var typeFromCall = instance.GetType();

            if (!typeFromCall.IsAssignableFrom(typeFromFile)) return false;

            // Toutes les propriétés, sauf celle ignorées explicitement par l'engin JSON
            var props = typeFromFile.GetProperties().Where(p => !Attribute.IsDefined(p, typeof(JsonIgnoreAttribute)));
            var propsAvoid = typeof(IExternalFileConfig).GetProperties().Select(p => p.GetMethod.Name).ToList().Concat(
                typeof(IJsonConfig).GetProperties().Select(p1 => p1.GetMethod.Name)).ToList();

            // Toutes les propriétés, sauf celles qui sont à propos des interface de chargement
            foreach (var p in props.Where(prop => !propsAvoid.Contains(prop.GetMethod.Name)))
            {
                if (p.SetMethod == null) continue; // Pas de setter
                // Transfert de la valeur vers l'instance originale
                var val = p.GetMethod.Invoke(fromFileInstance, null);
                p.SetMethod.Invoke(instance, new []{val});
            }

            if (!debug) return true;

            // Débogage
            var newFile = FileUtil.FileNameValidation(file);
            WriteJsonFile(newFile, instance);

            return true;
        }

        /// <summary>
        /// Tente d'initialiser via disque toutes les propriétés d'un objets qui partagent les interfaces suivantes: IJsonConfig, IExternalFileConfig
        /// Va invoquer la méthode UpdateInstanceFromFile sur cahque valeur non-null détectée.
        /// </summary>
        /// <param name="obj">L'objet à analyser.</param>
        /// <param name="baseDirectory"></param>
        /// <param name="debug">Si en mode déboguage, on va écrire sur disque, sous format JSON, le résultat de la mise à jour.</param>
        /// <returns>True si toutes les tentatives valides de mise à jour ont fonctionnées.</returns>
        public static bool UpdatePropertiesFromFiles(object obj, DirectoryInfo baseDirectory, bool debug = false)
        {
            return UpdatePropertiesFromFiles(obj, baseDirectory?.FullName, debug);
        }

        /// <summary>
        /// Tente d'initialiser via disque toutes les propriétés d'un objet qui possède les interfaces suivantes: IJsonConfig, IExternalFileConfig
        /// Va invoquer la méthode UpdateInstanceFromFile sur chaque valeurs non-nulles détectées.
        /// </summary>
        /// <param name="obj">L'objet à analyser.</param>
        /// <param name="baseDirectory">Répertoire de base où cherche le fichier à charger.</param>
        /// <param name="debug">Si en mode déboguage, on va écrire sur disque, sous format JSON, le résultat de la mise à jour.</param>
        /// <returns>True si toutes les tentatives valides de mise à jour ont fonctionnées.</returns>
        public static bool UpdatePropertiesFromFiles(object obj, string baseDirectory, bool debug = false)
        {
            if (obj == null) return true;

            // On identifie les propriétés implémentant les interfaces désirées.
            var props = ClassUtil.GetPropertiesFromTypes(obj, typeof(IJsonConfig), typeof(IExternalFileConfig));

            return props.Select(p => p.GetMethod.Invoke(obj, null)).
                Where(propObject => propObject != null).
                Aggregate(true, (current, propObject) => (bool) (current & UpdateInstanceFromFile((dynamic) propObject, baseDirectory, debug)));
        }

        /// <summary>
        /// Écrit sur disque les propriétés d'une instance sous format Json.
        /// Cette instance doit implémenter les interfaces suivantes: IJsonConfig et IExternalFileConfig
        /// </summary>
        /// <param name="obj">L'instance à sérialiser.</param>
        /// <param name="baseDirectory">Le répertoire de base où écrire le fichier résultant.</param>
        /// <param name="dontOverwrite">Si à vrai, la méthode va faire attention à ne pas détruire des fichiers existants.</param>
        /// <returns>TRUE si on a écrit sur disque l'état de l'instance.</returns>
        [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
        public static bool WritePropertiesToFiles(object obj, string baseDirectory, bool dontOverwrite = false)
        {
            if (obj == null) return true;

            // On identifie les propriétés implémentant les interfaces désirées.
            var props = ClassUtil.GetPropertiesFromTypes(obj, typeof(IJsonConfig), typeof(IExternalFileConfig));

            var debugSettings = new JsonSerializerSettings
            {
                DefaultValueHandling = DefaultValueHandling.Ignore,
                NullValueHandling = NullValueHandling.Ignore
            };

            var success = true;
            foreach (var p in props)
            {
                var propObject = p.GetMethod.Invoke(obj, null);
                if (propObject == null) continue; // Si null, pas de problème, on saute.

                var cfgJson = propObject as IJsonConfig;
                var efcObject = propObject as IExternalFileConfig;

                var fileInfo = !string.IsNullOrWhiteSpace(efcObject.ConfigFileName) ? 
                    FileUtil.FindFilePaths(baseDirectory, efcObject.ConfigFileName) : 
                    new FileInfo(baseDirectory + cfgJson.DefaultConfigFileName);

                if (dontOverwrite) //  Créer un nouveau fichier
                {
                    var newFile = new FileInfo(baseDirectory + efcObject.ConfigFileName);
                    fileInfo = FileUtil.FileNameValidation(newFile); // No-Op si n'existe pas.
                }

                success &= WriteJsonFile(fileInfo, propObject, debugSettings);
            }

            return success;
        }
    }
}