using System;
using System.Xml;

namespace NomadLogic.Multimedia.XmlTransformation.Filters
{
    internal class Colorize : ImageFilter
    {
        private int _hSet;
        private float _sSet;
        private float _lShift;
        private float _opacity = 1f;


        protected override byte[] FilterRgb(byte alpha, byte r, byte g, byte b)
        {
            var argb = new byte[4];
            float h, s, l;
            var hsl = RgbtoHsl(r, g, b);
            h = hsl[0];
            s = hsl[1];
            l = hsl[2];

            h = _hSet;
            if (h > 360)
                h = 360;
            if (h < 0)
                h = 0;
            s = _sSet;
            if (s < 0)
                s = 0;
            if (s > 1)
                s = 1;
            l += _lShift;
            if (l < 0)
                l = 0;
            if (l > 1)
                l = 1;

            var nouveauRgb = HsLtoRgb(h, s, l);
            var newR = (byte)nouveauRgb[0];
            var newG = (byte)nouveauRgb[1];
            var newB = (byte)nouveauRgb[2];

            argb[0] = alpha;
            argb[1] = (byte)(r * (1f - _opacity) + newR * _opacity);
            argb[2] = (byte)(g * (1f - _opacity) + newG * _opacity);
            argb[3] = (byte)(b * (1f - _opacity) + newB * _opacity);

            return argb;
        }

        public override void ParseXml(XmlTextReader reader)
        {
            var stringH = reader.GetAttribute("h");
            if (stringH != null && !stringH.Equals(""))
                _hSet = Int32.Parse(stringH);

            var stringS = reader.GetAttribute("s");
            if (stringS != null && !stringS.Equals(""))
                _sSet = float.Parse(stringS);

            var stringL = reader.GetAttribute("l");
            if (stringL != null && !stringL.Equals(""))
                _lShift = float.Parse(stringL);

            var stringOpacity = reader.GetAttribute("opacity");
            if (stringOpacity != null && !stringOpacity.Equals(""))
                _opacity = float.Parse(stringOpacity);
        }

        private float[] RgbtoHsl(int r, int g, int b)
        {
            float r2 = r / 255f, g2 = g / 255f, b2 = b / 255f;
            float min, max, h, s, l;

            min = Math.Min(r2, g2);
            min = Math.Min(min, b2);

            max = Math.Max(r2, g2);
            max = Math.Max(max, b2);

            //h
            if (min == max)
            {
                h = 0;
            }
            else if (max == r2)
            {
                h = (60 * ((g2 - b2) / (max - min)) + 360) % 360;
            }
            else if (max == g2)
            {
                h = 60 * ((b2 - r2) / (max - min)) + 120;
            }
            else
            {
                h = 60 * ((r2 - g2) / (max - min)) + 240;
            }

            //l
            l = (max + min) / 2;

            //s
            if (max == min)
            {
                s = 0;
            }
            else if (l <= 0.5)
            {
                s = (max - min) / (2 * l);
            }
            else
            {
                s = (max - min) / (2 - 2 * l);
            }

            return new float[] { h, s, l };
        }

        private int[] HsLtoRgb(float h, float s, float l)
        {
            float p, q, hk;
            // r = 0, g = 1, b = 2
            var t = new float[3];
            // r = 0, g = 1, b = 2
            var adjust = new float[] { 1 / 3f, 0, 0 - (1 / 3f) };
            // r = 0, g = 1, b = 2
            var rgb = new int[3];

            if (s != 0)
            {
                if (l < 0.5)
                {
                    q = l * (1 + s);
                }
                else
                {
                    q = l + s - (l * s);
                }

                p = 2 * l - q;

                hk = h / 360;

                for (var i = 0; i < t.Length; i++)
                {
                    t[i] = hk + adjust[i];
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
                        rgb[i] = (int)((p + ((q - p) * 6 * t[i])) * 255);
                    }
                    else if (2 * t[i] < 1)
                    {
                        // q et on met en int sur 255
                        rgb[i] = (int)(q * 255);
                    }
                    else if (3 * t[i] < 2)
                    {
                        // p+((q-p)*6*((2/3)-tc)) et on met en int sur 255
                        rgb[i] = (int)((p + ((q - p) * 6 * ((2 / 3f) - t[i]))) * 255);
                    }
                    else
                    {
                        rgb[i] = (int)(p * 255);
                    }
                }
            }
            else
            {
                rgb = new int[] { (int)(l * 255), (int)(l * 255), (int)(l * 255) };
            }

            return rgb;
        }
    }
}
