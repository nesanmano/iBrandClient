using System.Collections.Generic;
using System.IO;
using System.Xml;
using Newtonsoft.Json;
using NLog;
using NomadLogic.iBrand.Common.Util;
using System.Text;
using NomadLogic.iBrand.Common.Util.Math;

namespace NomadLogic.iBrand.Transformation.Filters
{
    /// <summary>
    /// Classe de base pour un filtre pixel par pixel de clé chromatique.
    /// </summary>
    public abstract class AbstractChromaKeyFilter : ImageFilter
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        public static readonly string DefaultConfigFileLocation = DirectoryUtil.GetLocalApplicationDir() + "AbstractChromaKeyFilter.config";

        private byte _minHue;
        private byte _maxHue;
        private byte _minHueSemi;
        private byte _maxHueSemi;

        private byte _minSaturation;
        private byte _maxSaturation;
        private byte _minSaturationSemi;
        private byte _maxSaturationSemi;

        private byte _minLuminance;
        private byte _maxLuminance;
        private byte _minLuminanceSemi;
        private byte _maxLuminanceSemi;

        #region Valeurs entieres
        public byte LowerHueInt { get { return _minHue; } set { _minHue = MathUtil.Clamp(byte.MinValue, byte.MaxValue, value); } }
        public byte HigherHueInt { get { return _maxHue; } set { _maxHue = MathUtil.Clamp(byte.MinValue, byte.MaxValue, value); } }
        public float LowerHueFloat { get { return MathUtil.ToRatioByte(_minHue); } set { _minHue = MathUtil.ToRatioFloat(value); } }
        public float HigherHueFloat { get { return MathUtil.ToRatioByte(_maxHue); } set { _maxHue = MathUtil.ToRatioFloat(value); } }

        public byte LowerSaturationInt { get { return _minSaturation; } set { _minSaturation = MathUtil.Clamp(byte.MinValue, byte.MaxValue, value); } }
        public byte HigherSaturationInt { get { return _maxSaturation; } set { _maxSaturation = MathUtil.Clamp(byte.MinValue, byte.MaxValue, value); } }
        public float LowerSaturationFloat { get { return MathUtil.ToRatioByte(_minSaturation); } set { _minSaturation = MathUtil.ToRatioFloat(value); } }
        public float HigherSaturationFloat { get { return MathUtil.ToRatioByte(_maxSaturation); } set { _maxSaturation = MathUtil.ToRatioFloat(value); } }

        public byte LowerLuminanceInt { get { return _minLuminance; } set { _minLuminance = MathUtil.Clamp(byte.MinValue, byte.MaxValue, value); } }
        public byte HigherLuminanceInt { get { return _maxLuminance; } set { _maxLuminance = MathUtil.Clamp(byte.MinValue, byte.MaxValue, value); } }
        public float LowerLuminanceFloat { get { return MathUtil.ToRatioByte(_minLuminance); } set { _minLuminance = MathUtil.ToRatioFloat(value); } }
        public float HigherLuminanceFloat { get { return MathUtil.ToRatioByte(_maxLuminance); } set { _maxLuminance = MathUtil.ToRatioFloat(value); } }
        #endregion Valeurs entieres

        #region Valeurs partielles
        public byte SemiLowerHueInt { get { return _minHueSemi; } set { _minHueSemi = MathUtil.Clamp(byte.MinValue, byte.MaxValue, value); } }
        public byte SemiHigherHueInt { get { return _maxHueSemi; } set { _maxHueSemi = MathUtil.Clamp(byte.MinValue, byte.MaxValue, value); } }
        public float SemiLowerHueFloat { get { return MathUtil.ToRatioByte(_minHueSemi); } set { _minHueSemi = MathUtil.ToRatioFloat(value); } }
        public float SemiHigherHueFloat { get { return MathUtil.ToRatioByte(_maxHueSemi); } set { _maxHueSemi = MathUtil.ToRatioFloat(value); } }

        public byte SemiLowerSaturationInt { get { return _minSaturationSemi; } set { _minSaturationSemi = MathUtil.Clamp(byte.MinValue, byte.MaxValue, value); } }
        public byte SemiHigherSaturationInt { get { return _maxSaturationSemi; } set { _maxSaturationSemi = MathUtil.Clamp(byte.MinValue, byte.MaxValue, value); } }
        public float SemiLowerSaturationFloat { get { return MathUtil.ToRatioByte(_minSaturationSemi); } set { _minSaturationSemi = MathUtil.ToRatioFloat(value); } }
        public float SemiHigherSaturationFloat { get { return MathUtil.ToRatioByte(_maxSaturationSemi); } set { _maxSaturationSemi = MathUtil.ToRatioFloat(value); } }

        public byte SemiLowerLuminanceInt { get { return _minLuminanceSemi; } set { _minLuminanceSemi = MathUtil.Clamp(byte.MinValue, byte.MaxValue, value); } }
        public byte SemiHigherLuminanceInt { get { return _maxLuminanceSemi; } set { _maxLuminanceSemi = MathUtil.Clamp(byte.MinValue, byte.MaxValue, value); } }
        public float SemiLowerLuminanceFloat { get { return MathUtil.ToRatioByte(_minLuminanceSemi); } set { _minLuminanceSemi = MathUtil.ToRatioFloat(value); } }
        public float SemiHigherLuminanceFloat { get { return MathUtil.ToRatioByte(_maxLuminanceSemi); } set { _maxLuminanceSemi = MathUtil.ToRatioFloat(value); } }
        #endregion Valeurs partielles

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.Append("{Hue=(");
            sb.Append(_minHue);
            sb.Append(" - ");
            sb.Append(_maxHue);
            sb.Append("), Saturation=(");
            sb.Append(_minSaturation);
            sb.Append(" - ");
            sb.Append(_maxSaturation);
            sb.Append("), Luminance=(");
            sb.Append(_minLuminance);
            sb.Append(" - ");
            sb.Append(_maxLuminance);
            sb.Append("), SemiHue=(");
            sb.Append(_minHueSemi);
            sb.Append(" - ");
            sb.Append(_maxHueSemi);
            sb.Append("), SemiSaturation=(");
            sb.Append(_minSaturationSemi);
            sb.Append(" - ");
            sb.Append(_maxSaturationSemi);
            sb.Append("), SemiLuminance=(");
            sb.Append(_minLuminanceSemi);
            sb.Append(" - ");
            sb.Append(_maxLuminanceSemi);
            sb.Append(")}");

            return sb.ToString();
        }

        protected AbstractChromaKeyFilter()
        {
            SetGreenScreenDefaults();
        }

        /// <summary>
        /// Met les valeur par défaut dans toutes les propriétés de l'instance.
        /// <br/>Cette série de valeurs est pour un écran vert avec une bonne variance.
        /// </summary>
        public void SetGreenScreenDefaults()
        {
            LowerHueInt = 75;
            HigherHueInt = 180;
            LowerSaturationFloat = 0.3f;
            HigherSaturationFloat = 1f;
            LowerLuminanceFloat = 0.15f;
            HigherLuminanceFloat = 0.95f;
            SemiLowerHueInt = 60;
            SemiHigherHueInt = 190;
            SemiLowerSaturationFloat = 0.15f;
            SemiHigherSaturationFloat = 1f;
            SemiLowerLuminanceFloat = 0.15f;
            SemiHigherLuminanceFloat = 0.97f;
        }

        /// <summary>
        /// Met les valeur par défaut dans toutes les propriétés de l'instance.
        /// <br/>Cette série de valeurs est pour un écran bleu avec une bonne variance.
        /// </summary>
        public void SetBlueScreenDefaults()
        {
            LowerHueInt = 200;
            HigherHueInt = 220;
            LowerSaturationFloat = 54 / 100f;
            HigherSaturationFloat = 88 / 100f;
            LowerLuminanceFloat = 71 / 255f;
            HigherLuminanceFloat = 100 / 255f;
            SemiLowerHueInt = 190;
            SemiHigherHueInt = 230;
            SemiLowerSaturationFloat = 40 / 100f;
            SemiHigherSaturationFloat = 100 / 100f;
            SemiLowerLuminanceFloat = 60 / 100f;
            SemiHigherLuminanceFloat = 100 / 100f;
        }

        public override void ParseXml(XmlTextReader reader)
        {

        }

        /// <summary>
        /// Retourne une instance complète de la classe demandée à partir d'un fichier sur disque.
        /// </summary>
        /// <typeparam name="T">Le type exact de la sous-classe voulue.</typeparam>
        /// <param name="file">Le fichier à lire comme source.</param>
        /// <returns>Une instance valide, null, si une erreur s'est produite (fichier inexistant ou contenu incompatible).</returns>
        public static T ReadJsonFile<T>(string file) where T : AbstractChromaKeyFilter
        {
            T data = null;

            try
            {
                data = JsonConvert.DeserializeObject<T>(File.ReadAllText(file));
            }
            catch (System.Exception e)
            {
                log.Error(e, "Could not read JSON file for type {0}: {1}", typeof(T).GetGenericTypeDefinition().FullName, file);
            }

            return data;
        }

        /// <summary>
        /// Méthode pour écrire la configuration actuelle de cette instance dans un fichier JSON.
        /// </summary>
        /// <typeparam name="T">Le type exact de la sous-classe.</typeparam>
        /// <param name="file">Le fichier à écrire</param>
        /// <param name="instance">L'instance à analyser.</param>
        /// <returns>TRUE si aucune erreur ne s'est produite.</returns>
        public static bool WriteJsonFile<T>(string file, T instance) where T : AbstractChromaKeyFilter
        {
            var error = false;

            try
            {
                var serializer = new JsonSerializer();

                using (var sw = new StreamWriter(file))
                using (JsonWriter writer = new JsonTextWriter(sw))
                {
                    serializer.Serialize(writer, instance);
                }
            }
            catch (System.Exception e)
            {
                error = true;
                log.Error(e, "Could not wrtite JSON file for type {0}: {1}", typeof(T).GetGenericTypeDefinition().FullName, file);
            }

            return !error;
        }

        /// <summary>
        /// Méthode pour configurer la classe d'un coup avec un dictionnaire de paramètres.
        /// </summary>
        /// <param name="parameters">Le dictionnaire de valeur, avec clés en format "XXX_YYY"</param>
        public virtual void SetParameters(Dictionary<string, float> parameters)
        {
            if (parameters.ContainsKey("LOWER_HUE"))
            {
                LowerHueInt = (byte)parameters["LOWER_HUE"];
            }

            if (parameters.ContainsKey("HIGHER_HUE"))
            {
                HigherHueInt = (byte)parameters["HIGHER_HUE"];
            }

            if (parameters.ContainsKey("LOWER_SATURATION"))
            {
                LowerSaturationFloat = parameters["LOWER_SATURATION"];
            }

            if (parameters.ContainsKey("HIGHER_SATURATION"))
            {
                HigherSaturationFloat = parameters["HIGHER_SATURATION"];
            }

            if (parameters.ContainsKey("LOWER_LUMINANCE"))
            {
                LowerLuminanceFloat = parameters["LOWER_LUMINANCE"];
            }

            if (parameters.ContainsKey("HIGHER_LUMINANCE"))
            {
                HigherLuminanceFloat = parameters["HIGHER_LUMINANCE"];
            }

            if (parameters.ContainsKey("SEMI_LOWER_HUE"))
            {
                SemiLowerHueInt = (byte)parameters["SEMI_LOWER_HUE"];
            }

            if (parameters.ContainsKey("SEMI_HIGHER_HUE"))
            {
                SemiHigherHueInt = (byte)parameters["SEMI_HIGHER_HUE"];
            }

            if (parameters.ContainsKey("SEMI_LOWER_SATURATION"))
            {
                SemiLowerSaturationFloat = parameters["SEMI_LOWER_SATURATION"];
            }

            if (parameters.ContainsKey("SEMI_HIGHER_SATURATION"))
            {
                SemiHigherSaturationFloat = parameters["SEMI_HIGHER_SATURATION"];
            }

            if (parameters.ContainsKey("SEMI_LOWER_LUMINANCE"))
            {
                SemiLowerLuminanceFloat = parameters["SEMI_LOWER_LUMINANCE"];
            }

            if (parameters.ContainsKey("SEMI_HIGHER_LUMINANCE"))
            {
                SemiHigherLuminanceFloat = parameters["SEMI_HIGHER_LUMINANCE"];
            }
        }
    }
}
