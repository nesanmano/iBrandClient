using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using ColorMine.ColorSpaces;
using JetBrains.Annotations;
using NLog;

// ReSharper disable CompareOfFloatsByEqualityOperator

namespace NomadLogic.Common.Util.Math
{
    /// <summary>
    /// Cette classe contient des méthodes pour effectuer des calculs mathématiques avancés.
    /// </summary>
    [PublicAPI]
    public static class MathUtil
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        public const double KilobyteThreshold = 1024;

        public const double MegabyteThreshold = KilobyteThreshold * 1024;

        public const double GigabyteThreshold = MegabyteThreshold * 1024;

        public const double TerabyteThreshold = GigabyteThreshold * 1024;

        /// <summary>
        /// Tolérance arbitraire pour les calculs de précision en regard à la comparaison avec zéro pour des variables à virgule flottante.
        /// </summary>
        public const double TOLERANCE_FLOATING_POINT = 0.001;

        /// <summary>
        /// Valeurs d'ajustement pour la conservion HSL -> RBG
        /// </summary>
        private static readonly float[] ADJUST_VALUES = {1 / 3f, 0, 0 - (1 / 3f)};

        public static readonly string[] ByteOrderNames = { "Bytes", "KB", "MB", "GB", "TB" };

        private const float HSL_BASE_DEG = 60;

        private const byte MAX_COLOR_INTENSITY = 255;

        /// <summary>
        /// Formate un nombre de bytes en résultat lisible et compacte pour un humain. Avec l'option fractionnaire et si sa valeur est
        /// strictement égale à zéro, elle apparaîtra quand même. L'option fractionnaire n'est pas affiché pour le premier ordre (I.E. en bas de 1024).
        /// Voir: http://www.csharp-examples.net/string-format-double/
        /// Exemples:
        /// 1000 => 1000 Bytes
        /// 2048 => 2 KB
        /// 5000000000 => 4.656 GB (avec option fractionnaire)
        /// </summary>
        /// <param name="bytes">Le nombre de bytes à représenter.</param>
        /// <param name="useFraction">Indique si on veut la partie fractionnaire (réprésentée en milièmes, soit trois chiffres, de l'ordre atteint).</param>
        /// <param name="formatCulture">Culture de formattage pour la partie fractionnaire. Invariant par défaut.</param>
        /// <returns>La chaîne d'affichage.</returns>
        public static string FormatByteNumber(ulong bytes, bool useFraction = false, CultureInfo formatCulture = null)
        {
            short order = 0;
            var tmpModVal = bytes;
            ulong tmpOrderValue = 1;
            while (tmpModVal >= 1024 && order < ByteOrderNames.Length - 1)
            {
                order++;
                tmpModVal >>= 10;
                tmpOrderValue <<= 10;
            }

            string fractionValue = null;
            if (useFraction && order != 0)
            {
                var left = bytes % tmpOrderValue;
                var cultInfo = formatCulture ?? CultureInfo.InvariantCulture;
                fractionValue = string.Format(cultInfo, "{0:#.000}", left / (double)tmpOrderValue);
            }

            return $"{tmpModVal}{fractionValue ?? ""} {ByteOrderNames[order]}";
        }

        public static string FormatByteNumber(long bytes, bool useFraction = false, CultureInfo formatCulture = null)
        {
            return FormatByteNumber((ulong) bytes, useFraction);
        }

        public static string FormatByteNumber(int bytes, bool useFraction = false, CultureInfo formatCulture = null)
        {
            return FormatByteNumber((ulong) bytes, useFraction);
        }

        public static string FormatByteNumber(float bytes, bool useFraction = false, CultureInfo formatCulture = null)
        {
            return FormatByteNumber((ulong) bytes, useFraction);
        }

        public static string ByteNumberReadable(long bytes, bool longFormatName = false)
        {
            return ByteNumberReadable((double) bytes, longFormatName);
        }

        public static string ByteNumberReadable(ulong bytes, bool longFormatName = false)
        {
            return ByteNumberReadable((double) bytes, longFormatName);
        }

        /// <summary>
        /// Calcule une chaîne d'affichage pour un nombre d'octets en format numérique.
        /// </summary>
        public static string ByteNumberReadable(double bytes, bool longFormatName = false)
        {
            if (bytes >= TerabyteThreshold) return $"{StringUtil.FormatFloat(2, bytes / TerabyteThreshold)} " + (longFormatName ? "Terabytes" : "TB");
            if (bytes >= GigabyteThreshold) return $"{StringUtil.FormatFloat(2, bytes / GigabyteThreshold)} " + (longFormatName ? "Gigabytes" : "GB");
            if (bytes >= MegabyteThreshold) return $"{StringUtil.FormatFloat(2, bytes / MegabyteThreshold)} " + (longFormatName ? "Megabytes" : "MB");
            return bytes >= KilobyteThreshold ? $"{StringUtil.FormatFloat(2, bytes / KilobyteThreshold)} " + (longFormatName ? "Kilobytes" : "KB" ): $"{bytes} Bytes";
        }

        /// <summary>
        /// Contraint une valeur entre deux borne. Si plus bas ou plus haut, on remplace par la valeur de la borne.
        /// Genere une exception si les bornes sont incompatibles (min > max)
        /// </summary>
        /// <param name="minValue">Valeur minimale</param>
        /// <param name="maxValue">Valeur Maximale</param>
        /// <param name="value">Valeur a tester.</param>
        /// <returns>La valeur, une foit limitee, si applicable</returns>
        public static byte Clamp(byte minValue, byte maxValue, byte value)
        {
            if (minValue > maxValue)
            {
                throw new ArgumentOutOfRangeException("Minimal value cannot be lower than maximal value: " + minValue +
                                                      "/" + maxValue);
            }

            if (value < minValue)
            {
                return minValue;
            }
            return value > maxValue
                ? maxValue
                : value;
        }

        /// <summary>
        /// Contraint une valeur entre deux borne. Si plus bas ou plus haut, on remplace par la valeur de la borne.
        /// Genere une exception si les bornes sont incompatibles (min > max)
        /// </summary>
        /// <param name="minValue">Valeur minimale</param>
        /// <param name="maxValue">Valeur Maximale</param>
        /// <param name="value">Valeur a tester.</param>
        /// <returns>La valeur, une foit limitee, si applicable</returns>
        public static int Clamp(int minValue, int maxValue, int value)
        {
            if (minValue > maxValue)
            {
                throw new ArgumentOutOfRangeException("Minimal value cannot be lower than maximal value: " + minValue +
                                                      "/" + maxValue);
            }

            if (value < minValue)
            {
                return minValue;
            }
            return value > maxValue
                ? maxValue
                : value;
        }

        /// <summary>
        /// Contraint une valeur entre deux borne. Si plus bas ou plus haut, on remplace par la valeur de la borne.
        /// Genere une exception si les bornes sont incompatibles (min > max)
        /// </summary>
        /// <param name="minValue">Valeur minimale</param>
        /// <param name="maxValue">Valeur Maximale</param>
        /// <param name="value">Valeur a tester.</param>
        /// <returns>La valeur, une foit limitee, si applicable</returns>
        public static float Clamp(float minValue, float maxValue, float value)
        {
            if (minValue > maxValue)
            {
                throw new ArgumentOutOfRangeException("Minimal value cannot be lower than maximal value: " + minValue +
                                                      "/" + maxValue);
            }

            if (value < minValue)
            {
                return minValue;
            }
            return value > maxValue
                ? maxValue
                : value;
        }

        /// <summary>
        /// Contraint une valeur entre deux borne. Si plus bas ou plus haut, on remplace par la valeur de la borne.
        /// Genere une exception si les bornes sont incompatibles (min > max)
        /// </summary>
        /// <param name="minValue">Valeur minimale</param>
        /// <param name="maxValue">Valeur Maximale</param>
        /// <param name="value">Valeur a tester.</param>
        /// <returns>La valeur, une foit limitee, si applicable</returns>
        [PublicAPI]
        public static uint Clamp(uint minValue, uint maxValue, uint value)
        {
            if (minValue > maxValue)
            {
                throw new ArgumentOutOfRangeException("Minimal value cannot be lower than maximal value: " + minValue +
                                                      "/" + maxValue);
            }

            if (value < minValue)
            {
                return minValue;
            }
            return value > maxValue
                ? maxValue
                : value;
        }

        /// <summary>
        /// Contraint une valeur entre deux borne. Si plus bas ou plus haut, on remplace par la valeur de la borne.
        /// Genere une exception si les bornes sont incompatibles (min > max)
        /// </summary>
        /// <param name="minValue">Valeur minimale</param>
        /// <param name="maxValue">Valeur Maximale</param>
        /// <param name="value">Valeur a tester.</param>
        /// <returns>La valeur, une foit limitee, si applicable</returns>
        public static double Clamp(double minValue, double maxValue, double value)
        {
            if (minValue > maxValue)
            {
                throw new ArgumentOutOfRangeException("Minimal value cannot be lower than maximal value: " + minValue +
                                                      "/" + maxValue);
            }

            if (value < minValue)
            {
                return minValue;
            }
            return value > maxValue
                ? maxValue
                : value;
        }

        /// <summary>
        /// Transforme un octet (0 - 255) en pourcentage (0.0f - 1.0f).
        /// <br/>Utilise dans les calculs mathematiques pour les shaders et manipulation de couleur ARGB.
        /// </summary>
        /// <param name="value">L'octet a transformer.</param>
        /// <returns>La valeur de l'octet en pourcentage.</returns>
        public static float ToRatioByte(byte value)
        {
            return value / (float) byte.MaxValue;
        }

        /// <summary>
        /// Transforme un pourcentage (0.0f - 1.0f) en octet (0 - 255).
        /// <br/>Utilise dans les calculs mathematiques pour les shaders et manipulation de couleur ARGB.
        /// <br/>La valeur flottante sera d'abord limitee (clamping).
        /// </summary>
        /// <param name="value">Le pourcentage a transformer.</param>
        /// <returns>La valeur du pourcentage en octet.</returns>
        public static byte ToRatioFloat(float value)
        {
            return (byte) (Clamp(0.0f, 1.0f, value) * byte.MaxValue);
        }

        /// <summary>
        /// Calcule la nouvelle taille du bounding box qui contient l'image rotatée en son centre (width/2, height/2).
        /// <br/>Voir ici pour les formules et leur justifications: http://stackoverflow.com/questions/3231176/how-to-get-size-of-a-rotated-rectangle
        /// </summary>
        /// <param name="imageWidth">Largeur de l'image originale.</param>
        /// <param name="imageHeight">Hauteur de l'image originale.</param>
        /// <param name="angle">L'angle de rotation, en degrés.</param>
        /// <returns>La taille, en valeur entière plafonnée.</returns>
        public static int GetRotationWidth(int imageWidth, int imageHeight, double angle)
        {
            //http://stackoverflow.com/questions/906564/why-is-modulus-operator-not-working-for-double-in-c
            var boundAngle = angle - System.Math.Floor(angle / 360) * 360;
            if (boundAngle < -180)
            {
                boundAngle += 360;
            }
            else if (boundAngle > 180)
            {
                boundAngle -= 360;
            }

            // Cas de base
            if (System.Math.Abs(boundAngle) < TOLERANCE_FLOATING_POINT)
            {
                return imageWidth;
            }

            // Cas de rotations faciles
            if (System.Math.Abs(boundAngle - 90) < TOLERANCE_FLOATING_POINT ||
                System.Math.Abs(boundAngle - (-90)) < TOLERANCE_FLOATING_POINT)
            {
                return imageHeight;
            }
            if (System.Math.Abs(boundAngle - 180) < TOLERANCE_FLOATING_POINT ||
                System.Math.Abs(boundAngle - (-180)) < TOLERANCE_FLOATING_POINT)
            {
                return imageWidth;
            }

            var sinus = System.Math.Sin(boundAngle * System.Math.PI / 180);
            var cossinus = System.Math.Cos(boundAngle * System.Math.PI / 180);

            /*
             * w = abs(x * cos(o)) + abs(y * sin(o))
             */
            var newWidth = System.Math.Abs(imageWidth * cossinus) + System.Math.Abs(imageHeight * sinus);

            return (int) System.Math.Ceiling(newWidth);
        }

        /// <summary>
        /// Calcule la nouvelle hauteur du bounding box qui contient l'image rotatée en son centre (width/2, height/2).
        /// <br/>Voir ici pour les formules et leur justifications: http://stackoverflow.com/questions/3231176/how-to-get-size-of-a-rotated-rectangle
        /// </summary>
        /// <param name="imageWidth">Largeur de l'image originale.</param>
        /// <param name="imageHeight">Hauteur de l'image originale.</param>
        /// <param name="angle">L'angle de rotation, en degrés.</param>
        /// <returns>La taille, en valeur entière plafonnée.</returns>
        public static int GetRotationHeight(int imageWidth, int imageHeight, double angle)
        {
            //http://stackoverflow.com/questions/906564/why-is-modulus-operator-not-working-for-double-in-c
            var boundAngle = angle - System.Math.Floor(angle / 360) * 360;
            if (boundAngle < -180)
            {
                boundAngle += 360;
            }
            else if (boundAngle > 180)
            {
                boundAngle -= 360;
            }

            // Cas de base
            if (System.Math.Abs(boundAngle) < TOLERANCE_FLOATING_POINT)
            {
                return imageHeight;
            }

            // Cas de rotations faciles
            if (System.Math.Abs(boundAngle - 90) < TOLERANCE_FLOATING_POINT ||
                System.Math.Abs(boundAngle - (-90)) < TOLERANCE_FLOATING_POINT)
            {
                return imageWidth;
            }
            if (System.Math.Abs(boundAngle - 180) < TOLERANCE_FLOATING_POINT ||
                System.Math.Abs(boundAngle - (-180)) < TOLERANCE_FLOATING_POINT)
            {
                return imageHeight;
            }

            var sinus = System.Math.Sin(boundAngle * System.Math.PI / 180);
            var cossinus = System.Math.Cos(boundAngle * System.Math.PI / 180);

            /*
             * h = abs(x * sin(o)) + abs(y * cos(o))
             */
            var newWidth = System.Math.Abs(imageWidth * sinus) + System.Math.Abs(imageHeight * cossinus);

            return (int) System.Math.Ceiling(newWidth);
        }

        /// <summary>
        /// Cette méthode fait une rotation sur un point P via une coordonnée C qui peut ne pas être le centre du monde.
        /// Utilise des matrices pour y parvenir.
        /// <br/>voir: http://stackoverflow.com/questions/20744964/rotation-around-an-arbitrary-point-using-transformation-matrix
        /// </summary>
        /// <param name="p">Le point à faire la rotation dessus.</param>
        /// <param name="c">Le point qui agira comme origine.</param>
        /// <param name="angleDegree">L'angle en degrés.</param>
        /// <returns>Une nouvelle de point modifié.</returns>
        public static PointF RotatePointWithArbitraryCenter(PointF p, PointF c, double angleDegree)
        {
            var angleRadians = angleDegree * System.Math.PI / 180;
            var cos = System.Math.Cos(angleRadians);
            var sin = System.Math.Sin(angleRadians);

            // On met au centre du monde
            double modX = p.X - c.X;
            double modY = p.Y - c.Y;

            // On fait la rotation
            var rotX = (modX * cos) - (modY * sin);
            var rotY = (modY * cos) + (modX * sin);

            // On remet au centre arbitraire
            return new PointF((float) rotX + c.X, (float) rotY + c.Y);
        }

        /// <summary>
        /// Transforme trois valeur RGB discrètes (0..255) en valeurs fractionnaires (0..1) HSL. Sauf le Hue, qui est un chiffre en degrés (0..360)
        /// </summary>
        /// <param name="r">Intensitée rouge</param>
        /// <param name="g">Intensité verte</param>
        /// <param name="b">Intensité blueue</param>
        /// <param name="hsl">Les valeurs H, S et L seront mises dans ce tableau.</param>
        [Obsolete("Utilisez la version de ColorMine désormais.")]
        public static void RgbToHsl(byte r, byte g, byte b, float[] hsl)
        {
            if (hsl == null) return;

            float r2 = r / (float) MAX_COLOR_INTENSITY,
                g2 = g / (float) MAX_COLOR_INTENSITY,
                b2 = b / (float) MAX_COLOR_INTENSITY;

            var min = System.Math.Min(r2, g2);
            min = System.Math.Min(min, b2);

            var max = System.Math.Max(r2, g2);
            max = System.Math.Max(max, b2);

            //h
            if (System.Math.Abs(min - max) < TOLERANCE_FLOATING_POINT)
            {
                hsl[0] = 0;
            }
            else if (System.Math.Abs(max - r2) < TOLERANCE_FLOATING_POINT)
            {
                hsl[0] = HSL_BASE_DEG * ((g2 - b2) / (max - min));
                if (hsl[0] > HSL_BASE_DEG * 6) hsl[0] = hsl[0] - HSL_BASE_DEG * 6; // On "clamp" entre 0 et 360
            }
            else if (System.Math.Abs(max - g2) < TOLERANCE_FLOATING_POINT)
            {
                hsl[0] = HSL_BASE_DEG * ((b2 - r2) / (max - min)) + HSL_BASE_DEG * 2;
            }
            else
            {
                hsl[0] = HSL_BASE_DEG * ((r2 - g2) / (max - min)) + HSL_BASE_DEG * 4;
            }

            //l
            hsl[2] = (max + min) / 2;

            //s
            if (System.Math.Abs(max - min) < TOLERANCE_FLOATING_POINT)
            {
                hsl[1] = 0;
            }
            else if (hsl[2] <= 0.5)
            {
                hsl[1] = (max - min) / (2 * hsl[2]);
            }
            else
            {
                hsl[1] = (max - min) / (2 - 2 * hsl[2]);
            }
        }

        /// <summary>
        /// Convertit une série de valeurs normales (0..1 et hue entre 0 et 360) HSL vers des valeurs absolues bytes RGB.
        /// <br/>La valeur Alpha n'est pas touchée, sauf en cas de saturation négative ou nulle.
        /// </summary>
        /// <param name="hsl">Les valeurs H, S et L, dans cet ordre.</param>
        /// <param name="argb">Stocke les valeurs de retour. Les valeurs Alpha, Red, Green, Blue, converties.</param>
        [Obsolete("Utilisez la version de ColorMine désormais.")]
        public static void HsLtoRgb(float[] hsl, byte[] argb)
        {
            if (hsl == null || argb == null) return;

            var t = new float[3];

            if (System.Math.Abs(hsl[1]) > TOLERANCE_FLOATING_POINT)
            {
                float q;
                if (hsl[2] < 0.5)
                {
                    q = hsl[2] * (1 + hsl[1]);
                }
                else
                {
                    q = hsl[2] + hsl[1] - (hsl[2] * hsl[1]);
                }

                var p = 2 * hsl[2] - q;

                var hk = hsl[0] / (HSL_BASE_DEG * 6);

                for (var i = 0; i < t.Length; i++)
                {
                    t[i] = hk + ADJUST_VALUES[i];
                    if (t[i] < 0)
                    {
                        t[i] += 1;
                    }
                    else if (t[i] > 1)
                    {
                        t[i] -= 1;
                    }

                    if (6 * t[i] < 1)
                    {
                        // p + ((q-p)*6*tc) et on met en int sur 255 pour avoir la valeur
                        // rgb.
                        argb[i + 1] = (byte) ((p + ((q - p) * 6 * t[i])) * MAX_COLOR_INTENSITY);
                    }
                    else if (2 * t[i] < 1)
                    {
                        // q et on met en int sur 255
                        argb[i + 1] = (byte) (q * MAX_COLOR_INTENSITY);
                    }
                    else if (3 * t[i] < 2)
                    {
                        // p+((q-p)*6*((2/3)-tc)) et on met en int sur 255
                        argb[i + 1] = (byte) ((p + ((q - p) * 6 * ((2 / 3f) - t[i]))) * MAX_COLOR_INTENSITY);
                    }
                    else
                    {
                        argb[i + 1] = (byte) (p * MAX_COLOR_INTENSITY);
                    }
                }
            }
            else
            {
                // Une saturation négative ou nulle entraîne un pixel opaque et gris selon la valeur de luminance.
                var d = (byte) (hsl[2] * MAX_COLOR_INTENSITY);
                argb[0] = MAX_COLOR_INTENSITY;
                argb[1] = d;
                argb[2] = d;
                argb[3] = d;
            }
        }

        /// <summary>
        /// Calcule l'angle entre deux vecteurs 2D.
        /// </summary>
        /// <param name="v1X">X du vecteur 1</param>
        /// <param name="v1Y">Y du vecteur 1</param>
        /// <param name="v2X">X du vecteur 2</param>
        /// <param name="v2Y">Y du vecteur 2</param>
        /// <param name="inDegrees">Indique si on veut le résultat en degrés, faux par défaut.</param>
        /// <returns>L'angle entre les deux vecteurs.</returns>
        public static double AngleBetween2DVectors(double v1X, double v1Y, double v2X, double v2Y,
            bool inDegrees = false)
        {
            // http://www.euclideanspace.com/maths/algebra/vectors/angleBetween/
            var rads = System.Math.Atan2(v2Y, v2X) - System.Math.Atan2(v1Y, v1X);

            return inDegrees ? RadiansToDegrees(rads) : rads;
        }

        /// <summary>
        /// Convertit une valeur d'angle en radians en degrés.
        /// </summary>
        /// <param name="rads">L'angle en radians.</param>
        /// <returns>L'angle en degrés.</returns>
        public static double RadiansToDegrees(double rads)
        {
            // http://www.rapidtables.com/convert/number/radians-to-degrees.htm
            return rads * 180 / System.Math.PI;
        }

        /// <summary>
        /// Convertit une valeur d'angle en degrés à radians.
        /// </summary>
        /// <param name="degs">L'angle en degrés.</param>
        /// <returns>L'angle en radians.</returns>
        [PublicAPI]
        public static double DegreesToRadians(double degs)
        {
            return degs * System.Math.PI / 180;
        }

        /// <summary>
        /// Calcule la taille théorique d'une zone d'image qui contient uniquement des pixels représentatifs suite à une rotation.
        /// En gros cela calcule un autre rectangle à l'intérieur de celui qui sera affecté par la rotation, qui garde la même
        /// orientation, comme "fenêtre" en prenant soin de ne pas sortir des limites du rectangle rotaté.
        /// En terme d'images, comme un Bitmap ne peut avoir autre chose qu'une rotation nulle (AxB) pour contenir ses données,
        /// cela permet de calculer une taille d'image qui ne contient pas de "zones vides" dûes à la rotation. Il y a
        /// forcément une perte de pixels (sauf si la rotation est un multiple de 180), mais cela est préférable en général
        /// plutôt que d'avoir des zones vides dans le résultat.
        /// Le point de rotation est toujours en plein milieu. Si on veut calculer un point différent, on peut additionner ces valeurs
        /// aux translations du résultat.
        /// </summary>
        /// <param name="width">La largeur pré-rotation.</param>
        /// <param name="height">La hauteur pré-rotation.</param>
        /// <param name="rotation">La rotation à effectuer en degrés.</param>
        /// <param name="result">Un rectangle contenant la nouvelle taille est les informations de translation 
        /// pour trouver le point (0,0) de la nouvelle image dans l'image originale.</param>
        /// /// <returns>TRUE si le calcul s'est effectué. FALSE s'il y a eu un quelconque problème d'arithmétique ou paramètres invalides.</returns>
        [SuppressMessage("ReSharper", "JoinDeclarationAndInitializer")]
        [SuppressMessage("ReSharper", "PossibleInvalidOperationException")]
        public static bool CalculateRotationInternalBoundingBox(int width, int height, double rotation,
            out Rectangle result)
        {
            // Pré-calculs
            var sinus = System.Math.Sin(rotation * System.Math.PI / 180);
            var cossinus = System.Math.Cos(rotation * System.Math.PI / 180);
            var halfWidth = width / 2.0;
            var halfHeight = height / 2.0;

            // On calcule d'abord les coins rotatés, en prenant le point (0,0) comme centre plutôt que comme point en haut à gauche.
            // On corrigera cette translation lors de l'assignation de résultat.
            // https://en.wikipedia.org/wiki/2D_computer_graphics#Rotation
            // En considérant un axe Y inversé.
            var rotatedPoints = new[]
            {
                new Point2D(-halfWidth, halfHeight), // UpperLeft
                new Point2D(halfWidth, halfHeight), // UpperRight
                new Point2D(-halfWidth, -halfHeight), // LowerLeft
                new Point2D(halfWidth, -halfHeight) // LowerRight
            };
            for (var i = 0; i < rotatedPoints.Length; i++)
            {
                var x = rotatedPoints[i].X * cossinus - rotatedPoints[i].Y * sinus;
                var y = rotatedPoints[i].X * sinus + rotatedPoints[i].Y * cossinus;
                rotatedPoints[i].X = x;
                rotatedPoints[i].Y = y;
            }

            // Calcul des valeurs extrêmes
            var extremums = new[]
            {
                rotatedPoints.Min(p => p[0]) // X minimal
                ,
                rotatedPoints.Max(p => p[0]) // X maximal
                ,
                rotatedPoints.Min(p => p[1]) // Y minimal
                ,
                rotatedPoints.Max(p => p[1]) // Y maximal
            };

            // Calcul des coins du "bounding rectangle"
            var boundPoints = new[]
            {
                new Point2D(extremums[0], extremums[3]) // UpperLeft (minX, maxY)
                ,
                new Point2D(extremums[1], extremums[3]) // UpperRight (maxX, maxY)
                ,
                new Point2D(extremums[0], extremums[2]) // LowerLeft (minX, minY)
                ,
                new Point2D(extremums[1], extremums[2]) // LowerRight (maxX, minY)
            };

            // Calcul des intersections
            // https://en.wikipedia.org/wiki/Line%E2%80%93line_intersection
            var intersections = new List<Point2D?>();

            // Définition des lignes en espace 2D
            var diagonales = new List<Line2D>
            {
                // Diagonale1 (lowerLeft (2) -> UpperRight (1))
                new Line2D(boundPoints[2], boundPoints[1]),
                // Diagonale2 (upperLeft (0) -> lowerRight (3))
                new Line2D(boundPoints[0], boundPoints[3])
            };
            var arretes = new List<Line2D>
            {
                // Upper (upperLeft (0) -> upperRight (1))
                new Line2D(rotatedPoints[0], rotatedPoints[1]),
                // Lower (lowerLeft (2) -> lowerRight (3))
                new Line2D(rotatedPoints[2], rotatedPoints[3]),
                // Left (upperLeft (0) -> lowerLeft (2))
                new Line2D(rotatedPoints[0], rotatedPoints[2]),
                // Right (upperRight (1) -> lowerRight (3))
                new Line2D(rotatedPoints[1], rotatedPoints[3])
            };

            // On calcule maintenant les intersections de chaque diagonales avec toutes les arrêtes.
            foreach (var diag in diagonales)
            {
                foreach (var arrete in arretes)
                {
                    diag.CalculateIntersection(arrete, out var newPoint);
                    intersections.Add(newPoint);
                }
            }

            // Pour chaque diagonale, on extrait le plus petit segment issus des collisions entre les arrêtes parrallèles entre elles (top + bottom, left + right).
            // Cela représente les points de collisions à l'intérieur des extrémums. S'il y a des point nuls, c'est que ces arrêtes étaits parfaitement alignées avec l'angle
            // de diagonale.
            var insideSegments = new Line2D[diagonales.Count];
            var maxIndex = diagonales.Count * arretes.Count;
            for (var i = 0; i < maxIndex; i += arretes.Count)
            {
                Line2D? seg1 = null;
                if (intersections[i] != null && intersections[i + 1] != null)
                    seg1 = new Line2D(intersections[i].Value, intersections[i + 1].Value);
                Line2D? seg2 = null;
                if (intersections[i + 2] != null && intersections[i + 3] != null)
                    seg2 = new Line2D(intersections[i + 2].Value, intersections[i + 3].Value);

                Line2D shorter;
                if (seg1 == null)
                {
                    shorter = seg2.Value;
                }
                else if (seg2 == null)
                {
                    shorter = seg1.Value;
                }
                else
                {
                    var l1 = seg1.Value.GetLenght();
                    var l2 = seg2.Value.GetLenght();
                    shorter = l1 > l2 ? seg2.Value : seg1.Value; // Le cas d'égalité importe peu.
                }
                insideSegments[i / arretes.Count] = shorter;
            }

            // Maintenant que l'on a tous les points d'intersections intérieurs, on se créé deux rectangles (un pour chaque diagonale), puis on fait leur intersection (méthode)
            // Pour obtenir le plus petit des deux (comme ils sont superposés par le centre, dans le pire cas ils sont égaux).
            var rectangles = new List<Rectangle>(2);
            foreach (var seg in insideSegments)
            {
                var minX = (int) System.Math.Min(seg.Point1.X, seg.Point2.X);
                var maxX = (int) System.Math.Max(seg.Point1.X, seg.Point2.X);
                var minY = (int) System.Math.Min(seg.Point1.Y, seg.Point2.Y);
                var maxY = (int) System.Math.Max(seg.Point1.Y, seg.Point2.Y);
                rectangles.Add(new Rectangle(
                    minX,
                    maxY,
                    maxX - minX,
                    maxY - minY
                ));
            }

            var rect1 = rectangles[0];
            var rect2 = rectangles[1];

            rect1.Intersect(rect2);

            // Il faut en tout dernier faire une intersection avec le rectangle original, car c'est lui la "fenêtre" réeele pour isoler les pixels
            var rectOriginal = new Rectangle((int) -halfWidth, (int) halfHeight, width, height);

            rectOriginal.Intersect(rect1);

            // On déplace le centre pour que le point (0,0) soit en haut a gauche, plutôt qu'au milieu
            result = new Rectangle(rectOriginal.X + (int) halfWidth, rectOriginal.Y - (int) halfHeight,
                rectOriginal.Width, rectOriginal.Height);
            return true;
        }

        /// <summary>
        /// Calcule une intersection entre lignes (sur un plan 2D), selon la méthode suivante:
        /// https://en.wikipedia.org/wiki/Line%E2%80%93line_intersection
        /// S'il n'y a aucune intersection, ou une infinité d'intersections (lignes co-jointes), on retourne null, et la variable
        /// de retour sera nulle.
        /// </summary>
        /// <param name="ligneA">Ligne A</param>
        /// <param name="ligneB">Ligne B</param>
        /// <param name="result">Variable de retour. NULL si le calcul était impossible à faire pour des raisons trigonométrique.</param>
        /// <return>TRUE si on a réussi à calculer un et un seul point d'intersection.</return>
        public static bool CalculateIntersection(Line2D ligneA, Line2D ligneB, out Point2D? result)
        {
            if (ligneA.Equals(ligneB))
            {
                // lignes égales, donc co-jointes, donc illimité de points commun, donc on sort ici.
                result = null;
                return false;
            }

            try
            {
                var denominateur = (ligneA.Point1.X - ligneA.Point2.X) * (ligneB.Point1.Y - ligneB.Point2.Y) -
                                   (ligneA.Point1.Y - ligneA.Point2.Y) * (ligneB.Point1.X - ligneB.Point2.X);

                // On détermine si assez près de zéro pour ne pas utiliser cette ligne. Cela indique que les lignes sont parallèles.
                if (System.Math.Abs(denominateur) > TOLERANCE_FLOATING_POINT)
                {
                    // Calcul du point
                    // Variables précalculées
                    var calc1 = ligneA.Point1.X * ligneA.Point2.Y - ligneA.Point1.Y * ligneA.Point2.X; // (x1y2 - y1x2)
                    var calc2 = ligneB.Point1.X * ligneB.Point2.Y - ligneB.Point1.Y * ligneB.Point2.X; // (x3y4 - y3x4)

                    // Point 1
                    var px = (calc1 * (ligneB.Point1.X - ligneB.Point2.X)) -
                             ((ligneA.Point1.X - ligneA.Point2.X) * calc2) / denominateur;

                    // Point 2
                    var py = (calc1 * (ligneB.Point1.Y - ligneB.Point2.Y)) -
                             ((ligneA.Point1.Y - ligneA.Point2.Y) * calc2) / denominateur;

                    // On finalise.
                    result = new Point2D(px, py);
                    return true;
                }

                // Dénominateur "égal" à zéro, pas de point unique.
                result = null;
                return false;
            }
            catch (Exception e)
            {
                log.Error(e, "Exception thrown when calculating intersection point.");
                result = null;
                return false;
            }
        }

        /// <summary>
        /// Calcule la taille d'une image pour englober tous les pixels suite à une rotation.
        /// Comme une rotation entraînera un déplacement des pixels par une diagonale, cette taille sera forcément plus grande
        /// (superficie) que la taille originale (sauf pour des multiples de 90). En gros, on calcule la nouvelle taille
        /// pour ne rien perdre, et le point zéro pour garder le tout centré.
        /// </summary>
        /// <param name="width">La nouvelle largeur potentielle</param>
        /// <param name="height">La nouvelle hauteur potentielle</param>
        /// <param name="rotation">La rotation en degrés.</param>
        /// <returns>Un rectangle de dimensionnement qui détermine la nouvelle taille et le point de translation pour dessiner cette rotation.</returns>
        public static Rectangle CalculateRotationBoundingBox(int width, int height, double rotation)
        {
            Rectangle boundingResult;

            var sinus = System.Math.Sin(rotation * System.Math.PI / 180);
            var cossinus = System.Math.Cos(rotation * System.Math.PI / 180);

            var finalWidth = (int) (width * System.Math.Abs(cossinus) + height * System.Math.Abs(sinus));
            var finalHeight = (int) (width * System.Math.Abs(sinus) + height * System.Math.Abs(cossinus));

            // On calcule la translation finale qu'on veut voir, mais à
            // cause qu'on a tourné le plan carthésien avec la rotation, il
            // faut trouver le x et y du nouveau système pour avoir la bonne
            // translation
            if (rotation < 0)
            {
                var finaleTransY = (int) -(sinus * width);
                boundingResult = new Rectangle(
                    (int) (sinus * finaleTransY),
                    (int) (cossinus * finaleTransY),
                    finalWidth,
                    finalHeight);
            }
            else
            {
                var finaleTransX = (int) (sinus * height);
                boundingResult = new Rectangle(
                    (int) (cossinus * finaleTransX),
                    (int) -(sinus * finaleTransX),
                    finalWidth,
                    finalHeight);
            }

            return boundingResult;
        }

        /// <summary>
        /// Calcule la distance euclédienne entre deux couleur de type HSV.
        /// http://stackoverflow.com/questions/35113979/calculate-distance-between-colors-in-hsv-space
        /// Les valeur S et V sont normalisés entre 0 et 1 pour accentuer la différence entre chroma.
        /// </summary>
        /// <param name="h0">Hue de la première couleur. [0-179]</param>
        /// <param name="s0">Saturation de la première couleur. [0-255]</param>
        /// <param name="v0">Value de la première couleur. [0-255]</param>
        /// <param name="h1">Hue de la deuxième couleur. [0-179]</param>
        /// <param name="s1">Saturation de la deuxième couleur. [0-255]</param>
        /// <param name="v1">Value de la deuxième couleur. [0-255]</param>
        /// <returns>La distance euclédienne. Sera une valeur entre 0 et 1</returns>
        [PublicAPI]
        public static double DistanceBetweenHsvColors(byte h0, byte s0, byte v0, byte h1, byte s1, byte v1)
        {
            var absH0H1 = System.Math.Abs(h0 - h1);
            var dh = System.Math.Min(absH0H1, 180 - absH0H1) / 90f;
            var ds = System.Math.Abs(s1 - s0) / 255f;
            var dv = System.Math.Abs(v1 - v0) / 255f;

            return System.Math.Sqrt(dh * dh + ds * ds + dv * dv);
        }

        /// <summary>
        /// Distance euclidienne entre deux vecteurs ou points, P et Q.
        /// </summary>
        /// <param name="p0">Point 1 de P</param>
        /// <param name="p1">Point 2 de P</param>
        /// <param name="p2">Point 3 de P</param>
        /// <param name="q0">Point 1 de Q</param>
        /// <param name="q1">Point 2 de Q</param>
        /// <param name="q2">Point 3 de Q</param>
        /// <returns>La distance entre les deux points.</returns>
        public static double DistanceBetweenVectors(float p0, float p1, float p2, float q0, float q1, float q2)
        {
            var d = new double[] {q0 - p0, q1 - p1, q2 - p2};

            return System.Math.Sqrt(d[0] * d[0] + d[1] * d[1] + d[2] * d[2]);
        }

        /// <summary>
        /// Distance euclidienne entre deux vecteurs ou points, P et Q.
        /// Optimisé pour réduire l'allocation de mémoire.
        /// </summary>
        /// <param name="p0">Point 1 de P</param>
        /// <param name="p1">Point 2 de P</param>
        /// <param name="p2">Point 3 de P</param>
        /// <param name="q0">Point 1 de Q</param>
        /// <param name="q1">Point 2 de Q</param>
        /// <param name="q2">Point 3 de Q</param>
        /// <param name="tmpWorkArray">Instance d'un tableau mémoire de calcul. Ne doit pas être nul et doit au moins posséder trois cases.</param>
        /// <returns>La distance entre les deux points.</returns>
        public static double DistanceBetweenVectors(float p0, float p1, float p2, float q0, float q1, float q2, [NotNull] double[] tmpWorkArray)
        {
            tmpWorkArray[0] = q0 - p0;
            tmpWorkArray[1] = q1 - p1;
            tmpWorkArray[2] = q2 - p2;

            return System.Math.Sqrt(tmpWorkArray[0] * tmpWorkArray[0] + 
                                    tmpWorkArray[1] * tmpWorkArray[1] + 
                                    tmpWorkArray[2] * tmpWorkArray[2]);
        }

        /// <summary>
        /// Convertit une valeur RGB à un vecteur CIE L*u*v* selon la conversion OpenCV.
        /// http://docs.opencv.org/3.1.0/de/d25/imgproc_color_conversions.html
        /// </summary>
        /// <param name="rgb">Le vecteur RGB. Doit être non-null et avoir au moins 3 de long.</param>
        /// <param name="luv">Le vecteur résultant Luv. Doit être non-null et avoir au moins 3 de long.</param>
        /// <returns>TRUE si l'opération s'est bien effectuée. FALSE si les paramètres ne le permettait pas.</returns>
        public static bool RgbToLuv(byte[] rgb, float[] luv)
        {
            if (rgb == null || rgb.Length < 3 || luv == null || luv.Length < 3) return false;

            var rgbData = new Rgb
            {
                R = rgb[0],
                G = rgb[1],
                B = rgb[2]
            };
            var luvData = rgbData.To<Luv>();
            luv[0] = (float) luvData.L;
            luv[1] = (float) luvData.U;
            luv[2] = (float) luvData.V;

            return true;
        }

        /// <summary>
        /// Convertit une valeur RGB à un vecteur HSL selon la conversion OpenCV.
        /// les valeurs sont H=[0..360], S=[0..100], L=[0.100]
        /// </summary>
        /// <param name="rgb">Le vecteur RGB. Doit être non-null et avoir au moins 3 de long.</param>
        /// <param name="hsl">Le vecteur résultant HSL. Doit être non-null et avoir au moins 3 de long.</param>
        /// <param name="normalize">Indique si l'on veut ramener les valeur S et L sur une plage de valeur normalisée, c'est à dire [0..1]</param>
        /// <returns>TRUE si l'opération s'est bien effectuée. FALSE si les paramètres ne le permettait pas.</returns>
        public static bool RgbToHls(byte[] rgb, float[] hsl, bool normalize = false)
        {
            if (rgb == null || rgb.Length < 3 || hsl == null || hsl.Length < 3) return false;

            var rgbData = new Rgb
            {
                R = rgb[0],
                G = rgb[1],
                B = rgb[2]
            };
            var hslData = rgbData.To<Hsl>();
            hsl[0] = (float) hslData.H;
            hsl[1] = (float) hslData.S;
            hsl[2] = (float) hslData.L;

            if (!normalize) return true;
            
            hsl[1] /= 100;
            hsl[2] /= 100;

            return true;
        }

        /// <summary>
        /// Teste une égalité avec tolérance.
        /// </summary>
        /// <param name="arg0">Valeur 1</param>
        /// <param name="arg1">Valeur 2</param>
        /// <param name="tolerance">Tolérance de virgule flottante tolérée.</param>
        /// <returns>Indique si le résultat de la différence absolue est sous la valeur de tolérance.</returns>
        [PublicAPI]
        public static bool IsEqualWithTolerance(float arg0, float arg1, double tolerance = TOLERANCE_FLOATING_POINT)
        {
            return System.Math.Abs((double) arg0 - arg1) < tolerance;
        }

        #region Median Calculation

        /* http://stackoverflow.com/questions/4140719/calculate-median-in-c-sharp */

        private static int Partition<T>(this IList<T> list, int start, int end, Random rnd = null)
            where T : IComparable<T>
        {
            if (rnd != null)
                list.Swap(end, rnd.Next(start, end + 1));

            var pivot = list[end];
            var lastLow = start - 1;
            for (var i = start; i < end; i++)
            {
                if (list[i].CompareTo(pivot) <= 0)
                    list.Swap(i, ++lastLow);
            }
            list.Swap(end, ++lastLow);
            return lastLow;
        }

        public static T NthOrderStatistic<T>(this IList<T> list, int n, Random rnd = null) where T : IComparable<T>
        {
            return NthOrderStatistic(list, n, 0, list.Count - 1, rnd);
        }

        private static T NthOrderStatistic<T>(this IList<T> list, int n, int start, int end, Random rnd)
            where T : IComparable<T>
        {
            while (true)
            {
                var pivotIndex = list.Partition(start, end, rnd);
                if (pivotIndex == n)
                    return list[pivotIndex];

                if (n < pivotIndex)
                    end = pivotIndex - 1;
                else
                    start = pivotIndex + 1;
            }
        }

        public static void Swap<T>(this IList<T> list, int i, int j)
        {
            if (i == j) //This check is not required but Partition function may make many calls so its for perf reason
                return;
            var temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }

        /// <summary>
        /// Calcule la médianne d'une liste de valeur.
        /// </summary>
        /// <typeparam name="T">Le type à comparer.</typeparam>
        /// <param name="list">La liste de valeur</param>
        /// <returns>La valeur médiane.</returns>
        [PublicAPI]
        public static T Median<T>(this IList<T> list) where T : IComparable<T>
        {
            return list.NthOrderStatistic((list.Count - 1) / 2);
        }

        /// <summary>
        /// Calcule la médianne d'une valeur avec méthode d'obtention de donnée en paramètre, dans le cas de LINQ.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sequence"></param>
        /// <param name="getValue"></param>
        /// <returns></returns>
        [PublicAPI]
        public static double Median<T>(this IEnumerable<T> sequence, Func<T, double> getValue)
        {
            var list = sequence.Select(getValue).ToList();
            var mid = (list.Count - 1) / 2;
            return list.NthOrderStatistic(mid);
        }

        #endregion

        /// <summary>
        /// Cette méthode calcule la nouvelle taille que devrait avoir un rectangle de taille (X,Y) pour contenir un maximum d'espace
        /// dans un autre rectangle de taille (C,V), sans perdre son ratio original et qui doit ne pas dépasser la taille du contenant.
        /// De telle sorte que le résultat final (Xp,Yp) respecte les conditions suivantes:
        /// (Xp == C || Yp == V) AND Xp ppOe C AND Yp ppOe V AND X/Y == Xp/Yp ("ppOe" veut dire "plus petit ou égal").
        /// La position du nouveau rectangle dans l'ancien n'est pas calculé, ce calcul de position devrait être laissé à l'appelant.
        /// </summary>
        /// <param name="containerWidth">La largeur du contenant (C)</param>
        /// <param name="containerHeight">La hauteur du contenant (V)</param>
        /// <param name="originalWidth">La largeur du rectangle à recalculer. (X)</param>
        /// <param name="originalHeight">La hauteur du rectangle à recalculer. (Y)</param>
        /// <returns>La nouvelle taille que devrait avoir le rectangle pour respecter les conditions. (Xp,Yp)</returns>
        public static SizeF GetNewSizeToFitInsideContainer(double containerWidth, double containerHeight,
            double originalWidth, double originalHeight)
        {
            if (containerWidth <= 0 || containerHeight <= 0 || originalWidth <= 0 || originalHeight <= 0)
            {
                return new SizeF(0,0);
            }
            
            double xp, yp;

            var ratioOri = originalWidth / originalHeight;
            var ratioCon = containerWidth / containerHeight;

            if (ratioOri >= 1 && ratioCon >= 1)
            {
                xp = containerWidth;
                yp = containerWidth / originalWidth * originalHeight;

                if (!(yp > containerHeight)) return new SizeF((float) xp, (float) yp);
                xp = originalWidth / (originalHeight / containerHeight);
                yp = containerHeight;
            }
            else if (ratioOri >= 1 && ratioCon < 1)
            {
                xp = containerWidth;
                yp = containerWidth / originalWidth * originalHeight;

                if (!(yp > containerHeight)) return new SizeF((float) xp, (float) yp);
                xp = originalHeight / containerHeight * originalWidth;
                yp = containerHeight;
            }
            else if (ratioOri < 1 && ratioCon >= 1)
            {
                xp = containerHeight / originalHeight * originalWidth;
                yp = containerHeight;

                /*
                if ((int)yp > containerHeight)
                {
                    xp = containerHeight / (float)originalHeight * originalWidth;
                    yp = containerHeight;
                }
                */
            }
            else // ratioOri < 1 && ratioCon < 1
            {
                xp = containerWidth;
                yp = containerWidth / originalWidth * originalHeight;

                if (!(yp > containerHeight)) return new SizeF((float) xp, (float) yp);
                xp = containerHeight / (float) originalHeight * originalWidth;
                yp = containerHeight;
            }

            return new SizeF((float) xp, (float) yp);
        }

        /// <summary>
        /// Cette méthode calcule une nouvelle taille selon une zone de taille précise devant contenir une autre zone avec un ratio
        /// potentiellement différent, en minimisant l'espace perdu.
        /// Il n'y a aucune perte que si les ratio sont rigoureusement identiques.
        /// Un ratio est la largeur divisée par la hauteur.
        /// Toute valeur négative ou éagale à zéro retourne un taille nulle: (0,0).
        /// </summary>
        /// <param name="containerWidth">La largeur de la zone contenant.</param>
        /// <param name="containerHeight">La heuteur de la zone contenant.</param>
        /// <param name="ratio">Le ratio de la zone qui doit être contenue.</param>
        /// <returns>Une taille de zone satisfaisant les pré-requis.</returns>
        public static SizeF GetNewSizeToFitInsideContainer(double containerWidth, double containerHeight, double ratio)
        {
            if (containerWidth <= 0 || containerHeight <= 0 || ratio <= 0)
            {
                return new SizeF(0,0);
            }

            return GetNewSizeToFitInsideContainer(containerWidth, containerHeight, containerWidth, containerWidth/ratio);
        }

        /// <summary>
        /// Calcule la nouvelle taille d'un rectangle (l'original) pour qu'il aille remplir la totalité d'un autre rectangle (le contenant),
        /// en redimensionnant pour que au moins une dimension soit égale dans les deux rectangles, mais en gardant le ratio original des deux rectangles.
        /// Cela aura pour effet, s'il n'ont pas un ratio égal, d'élaguer une partie du contenu du rectangle original. Le calcul vise à minimiser cet élaguage.
        /// </summary>
        /// <param name="containerWidth">Largeur du réceptacle.</param>
        /// <param name="containerHeight">Hauteur du réceptacle.</param>
        /// <param name="originalWidth">Largeur de l'original.</param>
        /// <param name="originalHeight">Hauteur de l'original.</param>
        /// <returns>Un rectangle contenant la nouvelle dimension, et aussi la translation nécessaire pour dessiner le résultat par dessus le réceptacle.</returns>
        public static RectangleF GetNewSizeToFitContainerWithMinimalLoss(float containerWidth, float containerHeight,
            float originalWidth, float originalHeight)
        {
            if (containerWidth == originalWidth && containerHeight == originalHeight)
                return new RectangleF(0, 0, originalWidth, originalHeight);

            var factor = System.Math.Max(containerWidth / originalWidth, containerHeight / originalHeight);

            var newWidth = originalWidth * factor;
            var newHeight = originalHeight * factor;

            return new RectangleF(
                (containerWidth - newWidth) / 2f,
                (containerHeight - newHeight) / 2f,
                newWidth,
                newHeight);
        }

        /// <summary>
        /// Calcule la nouvelle taille d'un rectangle (l'original) pour qu'il aille remplir la totalité d'un autre rectangle (le contenant),
        /// en redimensionnant pour que au moins une dimension soit égale dans les deux rectangles, mais en gardant le ratio original des deux rectangles.
        /// Cela aura pour effet, s'il n'ont pas un ratio égal, d'élaguer une partie du contenu du rectangle original. Le calcul vise à minimiser cet élaguage.
        /// </summary>
        /// <param name="containerWidth">Largeur du réceptacle.</param>
        /// <param name="containerHeight">Hauteur du réceptacle.</param>
        /// <param name="originalWidth">Largeur de l'original.</param>
        /// <param name="originalHeight">Hauteur de l'original.</param>
        /// <returns>Un rectangle contenant la nouvelle dimension, et aussi la translation nécessaire pour dessiner le résultat par dessus le réceptacle.</returns>
        [PublicAPI]
        public static Rectangle GetNewSizeToFitContainerWithMinimalLoss(int containerWidth, int containerHeight,
            int originalWidth, int originalHeight)
        {
            var rectF = GetNewSizeToFitContainerWithMinimalLoss((float) containerWidth, containerHeight, originalWidth,
                originalHeight);
            return new Rectangle((int) rectF.X, (int) rectF.Y, (int) rectF.Width, (int) rectF.Height);
        }

        /// <summary>
        /// Construit un rectangle à partir de deux points. On analyse les valeurs de coordonnées pour déterminer quel sera le point en haut
        /// à gauche et quel sera la largeur hauteur par rapport au point en bas à droite.
        /// </summary>
        /// <param name="pX1">X point 1</param>
        /// <param name="pY1">Y point 1</param>
        /// <param name="pX2">X point 2</param>
        /// <param name="pY2">Y point 2</param>
        /// <returns>Un rectangle entier (int) construit à partir des points données.</returns>
        [PublicAPI]
        public static Rectangle MakeRectangle(float pX1, float pY1, float pX2, float pY2)
        {
            var minX = (int) System.Math.Min(pX1, pX2);
            var minY = (int) System.Math.Min(pY1, pY2);
            var maxX = (int) System.Math.Max(pX1, pX2);
            var maxY = (int) System.Math.Max(pY1, pY2);

            return new Rectangle(minX, minY, maxX - minX, maxY - minY);
        }

        /// <summary>
        /// Vérifie si une taille quelconque est égale (avec delta) avec un ratio voulu.
        /// Comme il s'agit d'une quasi égalité sur une valeur à virgule flottante, on doit utiliser une valeur delta.
        /// La comparaison est toujours selon la plus grande dimension divisée par la plus petite, ce qui implique que le
        /// ratio à tester doit être égal ou supérieur à un.
        /// Par exemple, une taille de 900x600 implique un ratio de 1.5 (3:2), mais la taille 600x900, qui normalement devrait être 0.667 (2:3)
        /// est calculée comme son équivalent paysage. Cette façon de faire évite de faire plusieures validations et divisions si
        /// l'orientation de l'image n'est pas importante.
        /// </summary>
        /// <param name="width">Largeur à tester.</param>
        /// <param name="height">Hauteur à tester.</param>
        /// <param name="ratioTest">La valeur de ratio à vérifier.</param>
        /// <param name="tolerance">La tolérance de virgule flottante.</param>
        /// <param name="nbrDecimale">Le nombre de décimale à arrondir. 3 par défaut.</param>
        /// <returns>TRUE si la ratio calculé est extrêment proche de la valeur voulue.</returns>
        public static bool IsSizeCorrectRatio(int width, int height, double ratioTest,
            double tolerance = TOLERANCE_FLOATING_POINT, int nbrDecimale = 3)
        {
            if (ratioTest < 1) return false;
            var ratio = System.Math.Round((float) System.Math.Max(width, height) / System.Math.Min(width, height),
                nbrDecimale, MidpointRounding.AwayFromZero);
            return System.Math.Abs(ratio - ratioTest) < tolerance;
        }

        /// <summary>
        /// Vérifie si une taille quelconque est égale (avec delta) avec un ratio voulu.
        /// Comme il s'agit d'une quasi égalité sur une valeur à virgule flottante, on doit utiliser une valeur delta.
        /// La comparaison est toujours selon la plus grande dimension divisée par la plus petite, ce qui implique que le
        /// ratio à tester doit être égal ou supérieur à un.
        /// Par exemple, une taille de 900x600 implique un ratio de 1.5 (3:2), mais la taille 600x900, qui normalement devrait être 0.667 (2:3)
        /// est calculée comme son équivalent paysage. Cette façon de faire évite de faire plusieures validations et divisions si
        /// l'orientation de l'image n'est pas importante.
        /// </summary>
        /// <param name="size">La taille à tester.</param>
        /// <param name="ratioTest">La valeur de ratio à vérifier.</param>
        /// <param name="tolerance">La tolérance de virgule flottante.</param>
        /// <param name="nbrDecimale">Le nombre de décimale à arrondir. 3 par défaut.</param>
        /// <returns>TRUE si la ratio calculé est extrêment proche de la valeur voulue.</returns>
        public static bool IsSizeCorrectRatio(Size size, double ratioTest, double tolerance = TOLERANCE_FLOATING_POINT,
            int nbrDecimale = 3)
        {
            return IsSizeCorrectRatio(size.Width, size.Height, ratioTest, tolerance, nbrDecimale);
        }

        /// <summary>
        /// Vérifie si une coordonnée se trouve à l'intérieur (inclusivement) de la définition d'un rectangle, sur un plan cartésien.
        /// </summary>
        /// <param name="x">Coordonné X</param>
        /// <param name="y">Coordonnée Y</param>
        /// <param name="area">Définition de la zone à vérifier.</param>
        /// <returns>TRUE si la coordonné se trouve dans le rectangle, inclusivement.</returns>
        public static bool InArea(int x, int y, Rectangle area)
        {
            if (x <= area.Location.X || x >= area.Location.X + area.Width) return false;
            return y < area.Location.Y + area.Height && y > area.Location.Y;
        }

        /// <summary>
        /// Vérifie si une coordonnée se trouve à l'intérieur (inclusivement) de la définition d'un rectangle, sur un plan cartésien.
        /// </summary>
        /// <param name="x">Coordonné X</param>
        /// <param name="y">Coordonnée Y</param>
        /// <param name="area">Définition de la zone à vérifier.</param>
        /// <returns>TRUE si la coordonné se trouve dans le rectangle, inclusivement.</returns>
        public static bool InArea(int x, int y, Rectangle? area)
        {
            return area != null && InArea(x, y, area.Value);
        }
        
        /// <summary>
        /// Limite la valeur de couleur entre 0 et 255 (inclusifvement des deux côtés) pour entrer dans un seul byte.
        /// Utile pour les couleurs RGB.
        /// </summary>
        /// <param name="colorComponent">Valeur de la couleur, entière.</param>
        /// <returns>Un byte de la couleur voulue.</returns>
        public static byte ColorClamp(int colorComponent)
        {
            var color = System.Math.Max(0, colorComponent);
            color = System.Math.Min(255, color);
            return (byte)color;
        }

        public static string IntToHex(long id, bool removeLeadingZeroes = true)
        {
                var hex = id.ToString("X");

                var tmp = "";
                var hextmp = new List<string>();
                for (var i = 0; i < hex.Length; i++)
                {
                    tmp += hex[i];

                    if (i % 2 == 0) continue;
                    hextmp.Add(tmp);
                    tmp = "";
                }

                hextmp.Reverse();

                var result = new StringBuilder();

                foreach (var t in hextmp)
                {
                    if (!(removeLeadingZeroes && t.Equals("00"))) result.Append(t);
                }

                return result.ToString();
        }

        /// Inverse l'ordre des bytes qui composent le paramètre
        /// [B1,B2,B3] donne [B3,B2,B1]
        public static long ReverseByteOrder(long id)
        {
            var byteArrays = BitConverter.GetBytes(id);
            var rev = byteArrays.Reverse().ToArray();
            return BitConverter.ToInt64(rev, 0);
        }

        /// <summary>
        /// Divise un énumérable en X énumérables de taille maximale N.
        /// Les énumérables résultants sont en lecture seule.
        /// Voir: https://stackoverflow.com/questions/438188/split-a-collection-into-n-parts-with-linq
        /// </summary>
        /// <typeparam name="T">Le type inféré de l'énumérable.</typeparam>
        /// <param name="source">Énumérable original.</param>
        /// <param name="length">Nombre d'éléments maximum.</param>
        /// <returns>Un énumérable d'énumérables de taille N maximum.</returns>
        public static IEnumerable<IEnumerable<T>> PartitionEnumerable<T>(this IEnumerable<T> source, int length)
        {
            if (length <= 0)
                throw new ArgumentOutOfRangeException(nameof(length));

            var section = new List<T>(length);

            foreach (var item in source)
            {
                section.Add(item);

                if (section.Count != length) continue;
                yield return section.AsReadOnly();
                section = new List<T>(length);
            }

            if (section.Count > 0)
                yield return section.AsReadOnly();
        }

        /// <summary>
        /// Calcule les valeurs candidates pour le positionnement d'une grille 2D de rectangles (blocs) dans un rectangle qui agit comme zone maximale.
        /// Un "candidat" est une valeur encore inconnue, en terme entier. À savoir, par exemple, le nombre de pixel entre chaque zone, ou la taille de cette
        /// zone, ou alors le nombre de zones lui-même.
        /// On peut donner donner une (et une seule) valeur inférieure à un comme nombre de rangées et nombre de colonnes: l'algorithme va calculer la dimension
        /// indéfinie ppour maximiser l'espace et respecter le nombre d'éléments voulu dans la dimension spécifiée.
        /// On calcule ensuite les valeurs de distance entre chaque blocs (sur les deux axes) pour les espacer, selon un ratio d'espace.
        /// Les blocs doivent également avoir un ratio voulu précis de largeur sur hauteur. Un ratio de 1 implique un bloc parfaitement carré. Une valeur
        /// inférieur à 1 (mais supérieure à zéro) indique une orientation portrait, une valeur supérieure indique une orientation paysage.
        /// Si le calcul des candidat est impossible (à cause de mauvaise valeurs en entrée, principalement), la méthode retourne faux.
        /// Toutes les valeurs candidates sont référencées en sortie (out).
        /// </summary>
        /// <param name="ratioPadding">Le ratio à utiliser comme espacement.</param>
        /// <param name="availableWidth">La largeur maximale de la zone à peupler.</param>
        /// <param name="availableHeight">La heuteur maximale de la zone à peupler.</param>
        /// <param name="blockSizeRatio">Le ratio de chaque rectangle interne.</param>
        /// <param name="nbrRows">Le nombre voulu de rangées (Y). Si inférieur à 1, est considéré comme une valeur à déterminer.</param>
        /// <param name="nbrColumns">Le nombre voulu de colonnes (X). Si inférieur à 1, est considéré comme une valeur à déterminer.</param>
        /// <param name="candidateWidth">Valeur calculée sortante: largeur d'un bloc de présentation.</param>
        /// <param name="candidateHeight">Valeur calculée sortante: hauteur d'un bloc de présentation.</param>
        /// <param name="candidateRowPadding">Valeur calculée sortante: Distance entre chaque rangée (espacement sur l'axe Y).</param>
        /// <param name="candidateColumnPadding">Valeur calculée sortante: Distance entre chaque colonne (espacement sur l'axe X).</param>
        /// <param name="candidateRowNumber">Valeur calculée sortante: Nombre de rangées espacées verticalement.</param>
        /// <param name="candidateColumnNumber">Valeur calculée sortante: Nombre de colonnes espacées latéralement.</param>
        /// <returns></returns>
        public static bool CalculateSizeCandidates(double ratioPadding, int availableWidth, int availableHeight, double blockSizeRatio,
            int nbrRows, int nbrColumns, out int candidateWidth, out int candidateHeight, out int candidateRowPadding, out int candidateColumnPadding,
            out int candidateRowNumber, out int candidateColumnNumber)
        {
            var candidatesFound = false;
            // En cas de candidats non-trouvés.
            candidateRowPadding = -1;
            candidateColumnPadding = -1;

            // Formule pour isoler la largeur/hauteur d'un block
            // totalMetric = (nbrBlock + 1)(ratioPad * blockMetric) + (nbrBlock * blockMetric)
            // blockMetric = totalMetric / ((nbrBlock * ratioPad) + nbrBloc + ratioPad)

            // Pas de "wildcard". Toutes les valeurs sont connus à l'avance.
            if (nbrRows >= 1 && nbrColumns >= 1)
            {
                candidateWidth = (int) ((availableWidth/((nbrColumns*ratioPadding) + nbrColumns + ratioPadding)));
                candidateHeight = (int) (candidateWidth/blockSizeRatio);

                // Une fois qu'on a la taille possible de largeur, on calcule celle de hauteur et on vérifie s'il est possible
                // d'utiliser le hauteur résultante candidate avec le ratio original.
                // Sinon, il faut recalculer une hauteur candidate, et recalculer la largeur candidate basé sur cette nouvelle hauteur
                // candidate, car elle est plus petite.
                if (availableHeight < ((nbrRows + 1)*(ratioPadding*candidateHeight)) + (candidateHeight*nbrRows))
                {
                    candidateHeight = (int) ((availableHeight/((nbrRows*ratioPadding) + nbrRows + ratioPadding)));
                    candidateWidth = (int) (candidateHeight*blockSizeRatio);
                }
                
                candidatesFound = true;
            }
            else if (nbrColumns >= 1) // Le nombre de rangés est laissé ambigue et doit utiliser le maximum possible.
            {
                candidateWidth = (int)((availableWidth / ((nbrColumns * ratioPadding) + nbrColumns + ratioPadding)));
                candidateHeight = (int)(candidateWidth / blockSizeRatio);

                nbrRows = availableHeight/(int) (candidateHeight * (ratioPadding + 1));

                candidatesFound =  true;
            }
            else if (nbrRows >= 1) // Le nombre de colonnes est laissé ambigue et doit utiliser le maximum possible.
            {
                candidateHeight = (int)((availableHeight / ((nbrRows * ratioPadding) + nbrRows + ratioPadding)));
                candidateWidth = (int)(candidateHeight * blockSizeRatio);

                nbrColumns = availableWidth / (int)(candidateWidth * (ratioPadding + 1));

                candidatesFound = true;
            }
            else
            {
                // Erreur, pas asser de variables valides pour effectuer le calcul.
                candidateHeight = -1;
                candidateWidth = 1;
                candidateRowPadding = -1;
                candidateColumnPadding = -1;
            }

            // On finalise avec le calcul du padding, si on peut le calculer.
            if (candidatesFound)
            {
                // Calcul du padding selon la taille finale, basé sur les formules ci-haut
                // padding = (ratioPad * blockMetric)
                // totalMetric = (nbrBlock + 1)(padding) + (nbrBlock * blockMetric)
                // padding = (totalMetric - (nbrBlock * blockMetric)) / (nbrblock + 1)
                candidateColumnPadding = (availableWidth - (nbrColumns * candidateWidth)) / (nbrColumns + 1);
                candidateRowPadding = (availableHeight - (nbrRows * candidateHeight)) / (nbrRows + 1);
            }

            candidateRowNumber = nbrRows;
            candidateColumnNumber = nbrColumns;

            return candidatesFound;
        }

        /// <summary>
        /// Cacule les positions absolues finales d'une séries de zone rectangulaire disposée en grillage dans une zone délimité rectangulaire dans un espace 2D.
        /// Pour l'instant, cette liste représente une matrice ROW-MAJOR. C'est à dire que les X premiers éléments représente la rangée 0, les X prochains la rangée 1, où
        /// X représente le nombre de colonnes.
        /// Si on ne connaisait pas le nombre de rangées ou colonnes en entrée, on peut diviser le total de cette liste par la valeur connue en entrée.
        /// </summary>
        /// <param name="presentationGrid">La grille d'éléments (X -> Colonnes, Y -> rangées). Si au maximum une valeur est inférieure à un, on tente de calculer le nombre maximal que l'on peut disposer.</param>
        /// <param name="presentationZone">La taille et position du rectangle comtenant les éléments à calculer.</param>
        /// <param name="paddingRatio">Le ratio d'espacement, calculé sur la largeur/hauteur totale de l'espace, en chaque élément.</param>
        /// <param name="elementSizeRatio">Le ratio largeur sur hauteur de chaque rectangle à positionner.</param>
        /// <returns>Une liste non-null des rectangles résultants, positionnés de façon absolu, à l'intéreur des limites imposées par le rectangle de zone.</returns>
        public static List<Rectangle> CalculatePresentationCoordinatesOfSubElements(Size presentationGrid, Rectangle presentationZone, double paddingRatio, double elementSizeRatio)
        {
            var candidatesFound = CalculateSizeCandidates(paddingRatio, presentationZone.Width, presentationZone.Height, elementSizeRatio, presentationGrid.Height, presentationGrid.Width, 
                out var candidateWidth, out var candidateHeight, out var candidateRowPadding, out var candidateColumnPadding, out var candidateRowNumber, out var candidateColumnNumber);

            if (!candidatesFound)
            {
                throw new Exception("Invalid presentation data. No candidate size could be computed.");
            }

            // On détermine maintenant leur location absolue
            var nbrElemSize = candidateRowNumber * candidateColumnNumber;
            
            var boxSize = new Size(candidateWidth, candidateHeight);
            var trueSize = GetNewSizeToFitInsideContainer(boxSize.Width, boxSize.Height, elementSizeRatio).ToSize();

            // Calcul de l'espace inutilisé (en prenant du padding et du nombre d'éléments dans chaque axe).
            var missedSpaceX = presentationZone.Width - ((candidateColumnNumber * trueSize.Width) + ((candidateColumnNumber+1)*candidateColumnPadding));
            var missedSpaceY = presentationZone.Height - ((candidateRowNumber * trueSize.Height) + ((candidateRowNumber+1)*candidateRowPadding));

            var offset = new Point(
                presentationZone.X + (missedSpaceX/2),
                presentationZone.Y + (missedSpaceY/2));

            if (log.IsDebugEnabled)
            {
                log.Debug("Calculating coordinates with following base: Gallery Zone=({0}), Element Size=({1}), padding={2}|{3}, selected {4} files",
                    presentationZone, boxSize, candidateRowPadding, candidateColumnPadding, nbrElemSize);
            }
            
            var absoluteCoordinates = new List<Rectangle>();

            var elemIndex = 0;
            // TODO Seulement ROW MAJOR pour l'instant (on remplit les rangées en premier).
            for (var y = 0; elemIndex < nbrElemSize; y++)
            {
                var currentHeight = y * boxSize.Height + ((y+1) * candidateRowPadding) + offset.Y;

                for (var x = 0; x < presentationGrid.Width && elemIndex < nbrElemSize; x++)
                {
                    absoluteCoordinates.Add(new Rectangle(new Point(x*boxSize.Width + ((x+1)*candidateColumnPadding) + offset.X, currentHeight), trueSize));
                    elemIndex++;
                }
            }

            return absoluteCoordinates;
        }

        /// <summary>
        /// Assez similaire à la méthode purement de calcul de présentation, mais celle-ci fait une vérification d'un ancien groupe de données pour savoir si on fait une mise à jour.
        /// Contient également des données pour l'affichage de toutes les zones, comme les lignes de padding et le périmère de la zone complète, pour fin de visualisation en déboguage au besoin.
        /// </summary>
        /// <param name="oldCache">Optionel. Contient les dernières valeurs calculées.</param>
        /// <param name="presentationGrid">La grille d'éléments (X -> Colonnes, Y -> rangées). Si au maximum une valeur est inférieure à un, on tente de calculer le nombre maximal que l'on peut disposer.</param>
        /// <param name="presentationZone">La taille et position du rectangle comtenant les éléments à calculer.</param>
        /// <param name="paddingRatio">Le ratio d'espacement, calculé sur la largeur/hauteur totale de l'espace, en chaque élément.</param>
        /// <param name="elementSizeRatio">Le ratio largeur sur hauteur de chaque rectangle à positionner.</param>
        /// <param name="debugInfo">Indique si l'on veut calculer les points de déboguage. Faux par défaut.</param>
        /// <returns>Une instance de toutes les informations pertinentes sur le résultat de calculs de présentation.</returns>
        public static PresentationCoordinates CalculatePresentationCoordinates(PresentationCoordinates oldCache, Size presentationGrid, Rectangle presentationZone, double paddingRatio, double elementSizeRatio, bool debugInfo = false)
        {
            PresentationCoordinates coords;
            if (oldCache != null)
            {
                // On détermine si on fait les calculs ou si c'est inutile.
                if (oldCache.ElementSizeRatio == elementSizeRatio &&
                    oldCache.PaddingRatio == paddingRatio &&
                    oldCache.PresentationGrid.Equals(presentationGrid) &&
                    oldCache.PresentationZone.Equals(presentationZone))
                {
                    return oldCache;
                }

                coords = oldCache;
            }
            else
            {
                coords = new PresentationCoordinates();
            }

            coords.ElementCoordinates = CalculatePresentationCoordinatesOfSubElements(presentationGrid, presentationZone, paddingRatio, elementSizeRatio);

            if (!debugInfo) return coords;

            // calcul des points de lignes de débogage.

            // Précalcul
            var lX = presentationZone.X;
            var hX = presentationZone.X + presentationZone.Width;
            var lY = presentationZone.Y;
            var hY = presentationZone.Y + presentationZone.Height;

            var dp = new List<Point>
            {
                new Point(lX, lY),
                new Point(hX, lY),
                new Point(lX, lY),
                new Point(lX, hY),
                new Point(hX, hY),
                new Point(lX, hY),
                new Point(hX, hY),
                new Point(hX, lY)
            };
            // Périmètre 4 lignes -> 8 points

            // Pour l'instant, pas besoin de calculer les lignes intermédiaires de chaue rangée et colonne car on peut voir les zones de réponses.

            coords.DebugLines = dp.ToArray();
            return coords;
        }
    }

    /// <summary>
    /// Classe pour servir de "cache" pré-calculée de dimension de présentation en grille.
    /// </summary>
    public class PresentationCoordinates
    {
        public List<Rectangle> ElementCoordinates { get; set; }

        public Point[] DebugLines { get; set; }

        public Rectangle PresentationZone { get; set; }
        public Size PresentationGrid { get; set; }
        public double PaddingRatio { get; set; }
        public double ElementSizeRatio { get; set; }

    }
}
