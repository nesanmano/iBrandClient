using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using JetBrains.Annotations;
using Newtonsoft.Json;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Math;

// ReSharper disable PossibleNullReferenceException

namespace NomadLogic.Multimedia.Gpu.OpenCV
{
    /// <summary>
    /// Implémentation d'un clé chromatique pour OpenCV basé sur le format CIE L*u*v*.
    /// Ce type de clé chromatique est en fait une liste de transformations séquentielle, utilisé par l'algorithme
    /// de OpenCV.
    /// On peut voir ici la formule de conversion RGB -> Luv (Version OpenCV):
    /// http://docs.opencv.org/3.1.0/de/d25/imgproc_color_conversions.html
    /// Petit transformateur de RGB à Luv:
    /// http://colormine.org/convert/rgb-to-luv
    /// Description plus en détail du format:
    /// https://en.wikipedia.org/wiki/CIELUV
    /// Ce format à été choisi à cause qu'il représente bien la couleur en fonction de sa luminosité, et que
    /// le calcul de distance euclédien entre deux vecteurs de couleur Luv représente bien la différence entre
    /// la chromacité (couleur) et son éclat (luminosité). En gros un vert foncé à moins de différence entre un vert
    /// plus pâle qu'un rouge aussi foncé que dans un autre format (RGB ou HSV).
    /// Par exemple, on prend le vert foncé A versus deux autres couleurs: vert pâle B, vert lumineux C, rouge foncé Z.
    /// A = (RGB = [ 13, 88, 13]  , HSV = [120, 85, 35], LUV = [32.0201117407017, -28.2775955806712, 36.5555227071024])
    /// B = (RGB = [ 32,217, 32]  , HSV = [120, 85, 85], LUV = [76.0920081836112, -69.569111949408 , 89.93506385971  ])
    /// C = (RGB = [141,217,141]  , HSV = [120, 35, 85], LUV = [80,2984154793189, -37,157319091759 , 48,028579762098 ])
    /// Z = (RGB = [ 88, 13, 13]  , HSV = [360, 85, 35], LUV = [17.4211690832001,  46.7314564022425, 10.0771680421732])
    /// Les résultats sont:
    /// RGB: 
    ///     A * B = 131.768737
    ///     A * C = 222.281353
    ///     A * Z = 106.066017
    /// HSV: 
    ///     A * B = 50
    ///     A * C = 70.710678
    ///     A * Z = 240
    /// Luv: 
    ///     A * B = 80.602089
    ///     A * C = 50.411062
    ///     A * Z = 80.873916
    /// 
    /// Comme vous voyez, la différence entre RGB est trop variable pour être un bon baromètre. HSV accorde plus d'importance à
    /// la couleur, ce qui est normalement un bon point, mais dans certains cas, n'est pas capable de faire la différence entre
    /// couleurs quand elles sont saturées ou très lumineuses.
    /// Luv, comme son système de référence est beaucoup plus large, est capable de calculer une distance plus représentative, mais
    /// accorde autant ou presque d'importance à la luminance ou changement de chroma. Ce qui permet de cibler des couleurs avec
    /// plus de précision. Du coup par contre, il y a moins de possibilité d'utiliser une tolérance trop élevée.
    /// 
    /// Pour palier à ce problème, on utilise donc plusieures clé chromatiques en même temps.
    /// Ce qui fait que l'on peut donc cibler plusieures couleurs avec plus de précision.
    /// 
    /// De plus, cette classe de clé chromatique est basé sur le fait qu'une représentation RGB est plus facile à déterminer
    /// pour un Humain que Luv. On entre donc les valeurs en RGB, et les propriétés Luv sont des conversions de ces valeurs.
    /// Les conversions sont gardées en mémoire pour éviter de faire des recalculs.
    /// Les valeurs possibles sont:
    /// R, G et B: [0..255]
    /// L: [0..100]
    /// U: [-134..220]
    /// V: [-140..122]
    /// </summary>
    [DataContract]
    public class ChromaKeyInfoOpenCvCieLuv : ChromaKeyInfoAbstract, IEquatable<ChromaKeyInfoOpenCvCieLuv>
    {
        /// <summary>
        /// Location par défaut du fichier de configuration local pour ce type de clé chromatique.
        /// </summary>
        public static readonly string DefaultConfigFileLocation = DirectoryUtil.GetLocalApplicationDir() + "ChromaKeyInfoOpenCvCieLuv.config";

        /// <summary>
        /// Liste des infos de clé chromatique.
        /// </summary>
        [NotNull]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public readonly List<ChromaKeyInfoOpenCvCieLuvData> KeyingData = new List<ChromaKeyInfoOpenCvCieLuvData>();

        /// <inheritdoc />
        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.Append("Vendor=");
            sb.AppendLine(CameraVendor.ToString());
            sb.AppendLine("KeyingData:");
            sb.AppendLine(StringUtil.FormatEnumerable(KeyingData, true, 1, ""));

            return sb.ToString();
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (!base.Equals(obj)) return false;
            return obj is ChromaKeyInfoOpenCvCieLuv arg && KeyingData.SequenceEqual(arg.KeyingData);
        }

        /// <inheritdoc/>
        public bool Equals(ChromaKeyInfoOpenCvCieLuv other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return KeyingData.SequenceEqual(other.KeyingData);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return KeyingData.GetHashCode();
        }

        /// <summary>
        /// Retourne une instance fraîche avec une configuration de chromacité de base pour
        /// la couleur verte.
        /// </summary>
        /// <returns>Une nouvelle instance.</returns>
        [NotNull]
        public static ChromaKeyInfoOpenCvCieLuv NewInstanceChromaKeyGreen()
        {
            return new ChromaKeyInfoOpenCvCieLuv
            {
                CameraVendor = ChromaKeyInfoCameraVendorEnum.Canon,
                KeyingData =
                {
                    new ChromaKeyInfoOpenCvCieLuvData
                    {
                        R = 94,
                        G = 155,
                        B = 95,
                        Tolerance = 35,
                        Smoothing = 5
                    },
                    new ChromaKeyInfoOpenCvCieLuvData
                    {
                        R = 46,
                        G = 91,
                        B = 50,
                        Tolerance = 25,
                        Smoothing = 5
                    }
                }
            };
        }

        /// <summary>
        /// Retourne une instance fraîche avec une configuration de chromacité de base pour
        /// la couleur verte.
        /// </summary>
        /// <returns>Une nouvelle instance.</returns>
        [NotNull]
        public static ChromaKeyInfoOpenCvCieLuv NewInstanceChromaKeyBlue()
        {
            return new ChromaKeyInfoOpenCvCieLuv
            {
                CameraVendor = ChromaKeyInfoCameraVendorEnum.Canon,
                KeyingData =
                {
                    new ChromaKeyInfoOpenCvCieLuvData
                    {
                        R = 55,
                        G = 116,
                        B = 178,
                        Tolerance = 47,
                        Smoothing = 10
                    },
                    new ChromaKeyInfoOpenCvCieLuvData
                    {
                        R = 20,
                        G = 40,
                        B = 72,
                        Tolerance = 16,
                        Smoothing = 2
                    },
                    new ChromaKeyInfoOpenCvCieLuvData
                    {
                        R = 17,
                        G = 32,
                        B = 51,
                        Tolerance = 5,
                        Smoothing = 2
                    }
                }
            };
        }
    }

    /// <summary>
    /// Donnée pour un seul code de couleur de chromacité de CIE L*u*v*
    /// </summary>
    [DataContract]
    public class ChromaKeyInfoOpenCvCieLuvData : IEquatable<ChromaKeyInfoOpenCvCieLuvData>
    {
        private readonly byte[] _rgb = new byte[3];
        private float[] _luv;
        private double _tolerance = 30;
        private double _smoothing = 10;

        /// <summary>
        /// Valeur de distance tolérée pour la similitude de couleur.
        /// Doit être positif.
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public double Tolerance {
            get => _tolerance;
            set => _tolerance = value > 0 ? value : 0;
        }

        /// <summary>
        /// Valeur d'adoucissement pour l'opacité, calculé selon la distance entre 0 et la tolérance.
        /// À partir de (Tolerance - Smoothing) de différence, et jusqu'à la valeur de Tolerance, on calcule l'opacité selon
        /// un ratio linéaire. Une valeur de zéro implique qu'un pixel est soit opaque, soit visible, avec aucune gradation.
        /// Doit être dans la plage de valeur [0..Tolerance]
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public double Smoothing {
            get => _smoothing;
            set => _smoothing = MathUtil.Clamp(0, _tolerance, value);
        }

        /// <summary>
        /// Valeur R.
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public byte R
        {
            get => _rgb[0];

            set
            {
                _rgb[0] = value;
                _luv = null;
            }
        }

        /// <summary>
        /// Valeur R.
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public byte G
        {
            get => _rgb[1];

            set
            {
                _rgb[1] = value;
                _luv = null;
            }
        }

        /// <summary>
        /// Valeur B.
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public byte B
        {
            get => _rgb[2];

            set
            {
                _rgb[2] = value;
                _luv = null;
            }
        }

        /// <summary>
        /// Valeur L.
        /// </summary>
        public float L
        {
            get
            {
                if (_luv == null) Recalculate();
                return _luv[0];
            }
        }

        /// <summary>
        /// Valeur U.
        /// </summary>
        public float U
        {
            get
            {
                if (_luv == null) Recalculate();
                return _luv[1];
            }
        }

        /// <summary>
        /// Valeur V.
        /// </summary>
        public float V
        {
            get
            {
                if (_luv == null) Recalculate();
                return _luv[2];
            }
        }

        private void Recalculate()
        {
            _luv = new float[3];
            MathUtil.RgbToLuv(_rgb, _luv);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.Append("[");
            sb.Append("Tolerance=");
            sb.Append(Tolerance.ToString("0.00000000", CultureInfo.InvariantCulture));
            sb.Append(", Smoothing=");
            sb.Append(Smoothing.ToString("0.00000000", CultureInfo.InvariantCulture));
            sb.Append(", LUV={");
            sb.Append(L.ToString("0.00000000", CultureInfo.InvariantCulture));
            sb.Append(", ");
            sb.Append(U.ToString("0.00000000", CultureInfo.InvariantCulture));
            sb.Append(", ");
            sb.Append(V.ToString("0.00000000", CultureInfo.InvariantCulture));
            sb.Append("}, RGB={");
            sb.Append($"{R}, {G}, {B}");
            sb.Append("}]");

            return sb.ToString();
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            var arg = obj as ChromaKeyInfoOpenCvCieLuvData;
            return Equals(arg);
        }

        /// <inheritdoc/>
        [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = R.GetHashCode();
                hashCode = (hashCode * 397) ^ G.GetHashCode();
                hashCode = (hashCode * 397) ^ B.GetHashCode();
                hashCode = (hashCode*397) ^ _tolerance.GetHashCode();
                hashCode = (hashCode*397) ^ _smoothing.GetHashCode();
                return hashCode;
            }
        }

        /// <inheritdoc/>
        public bool Equals(ChromaKeyInfoOpenCvCieLuvData other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return R.Equals(other.R) &&
                   G.Equals(other.G) &&
                   B.Equals(other.B) &&
                   Smoothing.Equals(other.Smoothing) &&
                   Tolerance.Equals(other.Tolerance);
        }
        
    }
}
