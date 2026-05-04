using NomadLogic.iBrand.Common.Util;
using System;

namespace NomadLogic.iBrand.Transformation.Filters
{
    class SelectiveColorLayer
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


        private float cyanModif = 0f;
        private float magentaModif = 0f;
        private float yellowModif = 0f;
        private float blackModif = 0f;
        private int colors = WHITES;

        public SelectiveColorLayer(int colors)
        {
            this.colors = colors;
        }

        public float Cyan
        {
            get { return cyanModif; }
            set { cyanModif = value; }
        }

        public float Magenta
        {
            get { return magentaModif; }
            set { magentaModif = value; }
        }

        public float Yellow
        {
            get { return yellowModif; }
            set { yellowModif = value; }
        }

        public float Black
        {
            get { return blackModif; }
            set { blackModif = value; }
        }

        public byte[] FilterColor(byte alpha, byte r, byte g, byte b)
        {
            byte[] argb = new byte[4];

            argb[0] = alpha;
            
            if (alpha == 0)
            {
                argb[1] = 0;
                argb[2] = 0;
                argb[3] = 0;
            }
            else
            {
                float[] CMYK = RGBtoCMYK(r, g, b);
                if (isColor(r, g, b, CMYK[0], CMYK[1], CMYK[2], CMYK[3]))
                {
                    float c = CMYK[0];
                    float m = CMYK[1];
                    float y = CMYK[2];
                    float k = CMYK[3];

                    c += c * cyanModif;
                    m += m * magentaModif;
                    y += y * yellowModif;
                    k += k * blackModif;

                    byte[] rgb = CMYKtoRGB(c, m, y, k);
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

        private bool isColor(byte r, byte g, byte b, float c, float m, float y, float k)
        {
            
            switch (colors)
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

        private float[] RGBtoCMYK(byte r, byte g, byte b)
        {
            float computedC = 0f;
            float computedM = 0f;
            float computedY = 0f;
            float computedK = 0f;
            float[] CMYK = new float[4];

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

                float minCMY = Math.Min(computedC,
                             Math.Min(computedM, computedY));
                computedC = (computedC - minCMY) / (1 - minCMY);
                computedM = (computedM - minCMY) / (1 - minCMY);
                computedY = (computedY - minCMY) / (1 - minCMY);
                computedK = minCMY;
            }
            CMYK[0] = computedC;
            CMYK[1] = computedM;
            CMYK[2] = computedY;
            CMYK[3] = computedK;
            return CMYK;
             
        }

        private byte[] CMYKtoRGB(float c, float m, float y, float k)
        {
            byte[] RGB = new byte[3];

            int r = (int)((1 - (c * (1 - k) + k)) * 255);
            int g = (int)((1 - (m * (1 - k) + k)) * 255);
            int b = (int)((1 - (y * (1 - k) + k)) * 255);
            RGB[0] = ImageUtil.ColorClamp(r);
            RGB[1] = ImageUtil.ColorClamp(g);
            RGB[2] = ImageUtil.ColorClamp(b);

            return RGB;
        }
    }
}
