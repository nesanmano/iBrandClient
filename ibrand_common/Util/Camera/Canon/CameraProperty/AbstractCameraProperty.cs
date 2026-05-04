using System.Collections.Generic;
using System.Linq;
using Canon.Eos.Framework;
using JetBrains.Annotations;
using NLog;

namespace NomadLogic.Common.Util.Camera.Canon.CameraProperty
{
    /// <summary>
    /// Class abstraite pour un configuration de propriété Canon
    /// </summary>
    [PublicAPI]
    public abstract class AbstractCameraProperty
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        /// <summary>
        /// Le dictionnaire de valeurs valides de toutes les propriétés.
        /// </summary>
        private static readonly Dictionary<uint, Dictionary<uint, string>> globalPropertyValuesDictionary = new Dictionary<uint, Dictionary<uint, string>>();
        private readonly List<string> _supportedDisplayValues = new List<string>();
        private readonly List<uint> _supportedNumericValues = new List<uint>();

        /// <summary>
        /// Constrcuteur de base.
        /// </summary>
        /// <param name="camera"></param>
        protected AbstractCameraProperty(EosCamera camera)
        {
            PropertyId = InitPropertyId();
            Init(camera);
        }

        /// <summary>
        /// Chaque class d'implémentation doit fournir l'identifiant numérique de cette propriété via cette méthode.
        /// </summary>
        /// <returns></returns>
        protected abstract uint InitPropertyId();

        /// <summary>
        /// Initialize les propriété de cette classe via une instance de caméra.
        /// </summary>
        /// <param name="camera"></param>
        protected void Init(EosCamera camera)
        {
            if (!globalPropertyValuesDictionary.ContainsKey(PropertyId))
            {
                var valuesDictionary = new Dictionary<uint, string>();
                InitGlobalProperties(valuesDictionary);
                globalPropertyValuesDictionary.Add(PropertyId, valuesDictionary);
                if (log.IsTraceEnabled)
                {
                    log.Trace("Initialized global property values ({3}) for {0}:{1} => [{2}]",
                        GetType().Name,
                        PropertyId,
                        StringUtil.FormatEnumerable(valuesDictionary.Values, false, 0, ", "),
                        valuesDictionary.Values.Count);
                }
            }

            InitSupportedValues(camera);

            GetSelectedValue(camera);
        }

        public IReadOnlyCollection<string> SupportedDisplayValues => _supportedDisplayValues.AsReadOnly();

        public IReadOnlyCollection<uint> SupportedNumericValues => _supportedNumericValues.AsReadOnly();

        /// <summary>
        /// Valeur d'affichage sélectionnée de la valeur de propriété de caméra..
        /// </summary>
        public string SelectedValue { get; private set; }

        /// <summary>
        /// Valeur numérique sélectionnée de la propriété de caméra.
        /// </summary>
        public uint PropertyValue { get; private set; }

        /// <summary>
        /// Identifiant numérique de la propriété de caméra.
        /// </summary>
        public uint PropertyId { get; protected set; }

        /// <summary>
        /// Indique si une valeur est sélectionnée.
        /// </summary>
        /// <returns></returns>
        public bool IsValid()
        {
            return SelectedValue != null;
        }

        /// <summary>
        /// Met à jour une propriété de caméra via sa valeur d'affichage. Met à jour également la propriété sur une
        /// instance de caméra active si on la passe en paramètre.
        /// </summary>
        /// <param name="camera">L'instance caméra à mettre à jour simultanément.</param>
        /// <param name="value">La valeur à sélectionner. Doit exister dans les valeurs possibles.</param>
        public virtual void UpdateSelectedValue(EosCamera camera, string value)
        {
            try
            {
                if (SelectedValue.Equals(value)) return;
                var valuesDir = globalPropertyValuesDictionary[PropertyId];
                if (!valuesDir.ContainsValue(value)) return;
                foreach (var pair in valuesDir.Where(pair => value.Equals(pair.Value)))
                {
                    PropertyValue = pair.Key;
                    camera?.SetPropertyIntegerData(PropertyId, PropertyValue);
                    SelectedValue = valuesDir[PropertyValue];
                    break;
                }
            }
            catch(System.Exception ex)
            {
                log.Fatal(ex, "Error while setting property.");
            }
        }

        public virtual void UpdateSelectedValue(EosCamera camera, uint value)
        {
            if (camera == null) return;

            try
            {
                if (PropertyValue == value) return;
                if (!globalPropertyValuesDictionary[PropertyId].ContainsKey(value)) return;

                PropertyValue = value;

                camera.SetPropertyIntegerData(PropertyId, PropertyValue);
                SelectedValue = globalPropertyValuesDictionary[PropertyId][PropertyValue];
            }
            catch (System.Exception ex)
            {
                log.Fatal(ex, "Error while setting property.");
            }
        }

        protected virtual void GetSelectedValue(EosCamera camera)
        {
            if (camera == null) return;

            PropertyValue = camera.GetPropertyIntegerData(PropertyId);
            SelectedValue = null;

            if (!globalPropertyValuesDictionary[PropertyId].ContainsKey(PropertyValue))
            {
                var stringValue = camera.GetPropertyStringData(PropertyValue);
                log.Debug("Unknown property value for {0}: [{1}] {2}", GetType().Name, PropertyValue, stringValue ?? "null");
                return;
            }
            SelectedValue = globalPropertyValuesDictionary[PropertyId][PropertyValue];

            if (!_supportedDisplayValues.Contains(SelectedValue))
                SelectedValue = null;
        }

        /// <summary>
        /// Initialize all the possible values for this instance of camera.
        /// </summary>
        /// <param name="camera">The camera to read the data from.</param>
        protected virtual void InitSupportedValues(EosCamera camera)
        {
            if (camera == null) return;

            try
            {
                var propertyDesc = camera.GetPropertyDescription(PropertyId);
                var possibleValues = globalPropertyValuesDictionary[PropertyId];
                _supportedDisplayValues.Clear();
                _supportedNumericValues.Clear();

                for (var i = 0; i < propertyDesc.NumElements; i++)
                {
                    var numValue = (uint)propertyDesc.PropDesc[i];
                    possibleValues.TryGetValue(numValue, out var displayValue);
                    /* // On ne fait pas d'ajouts dynamique, trop risqué.
                    if (displayValue == null)
                    {
                        var stringValue = camera.GetPropertyStringData(numValue);
                        possibleValues.Add(numValue, stringValue);
                        log.Trace("Added new global value for {0}: [{1}] {2}", GetType().Name, numValue, stringValue);
                        displayValue = stringValue;
                    }
                    */
                    
                    if (displayValue == null) continue;

                    _supportedDisplayValues.Add(displayValue);
                    _supportedNumericValues.Add(numValue);
                }

                if (log.IsTraceEnabled)
                {
                    log.Trace("Initialized supported values ({4}) for camera a={0} for {1}:{2} => [{3}]",
                        camera.Author,
                        GetType().Name,
                        PropertyId,
                        StringUtil.FormatEnumerable(_supportedDisplayValues, false, 0, ", "),
                        _supportedDisplayValues.Count);
                }
            }
            catch (System.Exception ex)
            {
                log.Fatal(ex, "Error while getting property.");
            }
        }

        /// <summary>
        /// Remplit le dictionnaire de propriété donné en paramètre avec les valeurs propre à l'implémentation de cette classe.
        /// </summary>
        /// <param name="dic">Le dictionnaire à remplir.</param>
        public abstract void InitGlobalProperties(Dictionary<uint, string> dic);
    }
}
