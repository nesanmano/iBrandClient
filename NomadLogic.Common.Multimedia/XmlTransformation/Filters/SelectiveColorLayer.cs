using System;
using JetBrains.Annotations;
using NomadLogic.Common.Util.Math;

namespace NomadLogic.Multimedia.XmlTransformation.Filters
{
    [PublicAPI]
    internal class SelectiveColorLayer
    {
        public const int REDS = 0;
        public const int GREENS = 1;
        public const int BLUES = 2;
        public const int CYANS = 3;
        public const int MAGENTAS = 4;
        public const int YELLOWS = 5;
        public const int WHITES = 6;
        public const int NEUTRALS = 7;
        public const int BLACKS = 8;


        private readonly int _colors;

        public SelectiveColorLayer(int colors)
        {
            _colors = colors;
        }

        public float Cyan { get; set; }

        public float Magenta { get; set; }

        public float Yellow { get; set; }

        public float Black { get; set; }

        public byte[] FilterColor(byte alpha, byte r, byte g, byte b)
        {
            var argb = new byte[4];

            argb[0] = alpha;
            
            if (alpha == 0)
            {
                argb[1] = 0;
                argb[2] = 0;
                argb[3] = 0;
            }
            else
            {
                var cmyk = RgBtoCmyk(r, g, b);
                if (IsColor(r, g, b, cmyk[0], cmyk[1], cmyk[2], cmyk[3]))
                {
                    var c = cmyk[0];
                    var m = cmyk[1];
                    var y = cmyk[2];
                    var k = cmyk[3];

                    c += c * Cyan;
                    m += m * Magenta;
                    y += y * Yellow;
                    k += k * Black;

                    var rgb = CmyKtoRgb(c, m, y, k);
                    argb[1] = rgb[0];
                    argb[2] = rgb[1];
                    argb[3] = rgb[2];
                }
                else
                {
                    argb[1] = r;
                    argb[2] = g;
                    argb[3] = b;
                }
            }
            

            return argb;
        }

        private bool IsColor(byte r, byte g, byte b, float c, float m, float y, float k)
        {
            
            switch (_colors)
            {
                case REDS: 
                    if (r >= g + 30 && r >= b + 30)
                    {
                        return true;
                    }
                    break;
                case GREENS :
                    if (g >= r + 30 && g >= b + 30)
                    {
                        return true;
                    }
                    break;
                case BLUES :
                    if (b >= r + 30 && b >= g + 30)
                    {
                        return true;
                    }
                    break;
                case CYANS :
                    if (c >= m + 0.12f && c >= y + 0.12f)
                    {
                        return true;
                    }
                    break;
                case MAGENTAS :
                    if (m >= c + 0.12f && m >= y + 0.12f)
                    {
                        return true;
                    }
                    break;
                case YELLOWS :
                    if (y >= c + 0.12f && y >= m + 0.12f)
                    {
                        return true;
                    }
                    break;
                case WHITES :
                    if (c < 0.12f && m < 0.12f && y < 0.12f && k < 0.17f)
                    {
                        return true;
                    }
                    break;
                case NEUTRALS :
                    if ( k > 0.17f && k < 0.7f)
                    {
                        return true;
                    }
                    break;
                case BLACKS :
                    if ( k > 0.7f )
                    {
                        return true;
                    }
                    break;
                default :
                    return false;
            }
            return false;
        }

        private float[] RgBtoCmyk(byte r, byte g, byte b)
        {
            var computedC = 0f;
            var computedM = 0f;
            var computedY = 0f;
            float computedK;
            var cmyk = new float[4];

            // BLACK
            if (r == 0 && g == 0 && b == 0)
            {
                computedK = 1;
            }
            else
            {
                computedC = 1 - (r / 255f);
                computedM = 1 - (g / 255f);
                computedY = 1 - (b / 255f);

                var minCmy = Math.Min(computedC,
                             Math.Min(computedM, computedY));
                computedC = (computedC - minCmy) / (1 - minCmy);
                computedM = (computedM - minCmy) / (1 - minCmy);
                computedY = (computedY - minCmy) / (1 - minCmy);
                computedK = minCmy;
            }
            cmyk[0] = computedC;
            cmyk[1] = computedM;
            cmyk[2] = computedY;
            cmyk[3] = computedK;
            return cmyk;
             
        }

        private byte[] CmyKtoRgb(float c, float m, float y, float k)
        {
            var rgb = new byte[3];

            var r = (int)((1 - (c * (1 - k) + k)) * 255);
            var g = (int)((1 - (m * (1 - k) + k)) * 255);
            var b = (int)((1 - (y * (1 - k) + k)) * 255);
            rgb[0] = MathUtil.ColorClamp(r);
            rgb[1] = MathUtil.ColorClamp(g);
            rgb[2] = MathUtil.ColorClamp(b);

            return rgb;
        }
    }
}
