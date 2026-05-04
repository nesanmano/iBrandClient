using System;
using System.Xml;
using NLog;

namespace NomadLogic.Multimedia.XmlTransformation.Filters
{
    internal class ShiftHsl : ImageFilter
    {
        private static Logger _log = LogManager.GetCurrentClassLogger();
        private int _hShift;
        private float _sShift;
        private float _lShift;


        protected override byte[] FilterRgb(byte alpha, byte r, byte g, byte b)
        {
            var argb = new byte[4];
            float h, s, l;
            var hsl = RgbtoHsl(r, g, b);
            h = hsl[0];
            s = hsl[1];
            l = hsl[2];

            h += _hShift;
            if (h > 360)
                h = 360;
            if (h < 0)
                h = 0;
            s += _sShift;
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
            r = (byte)nouveauRgb[0];
            g = (byte)nouveauRgb[1];
            b = (byte)nouveauRgb[2];

            argb[0] = alpha;
            argb[1] = r;
            argb[2] = g;
            argb[3] = b;

            return argb;
        }

        public override void ParseXml(XmlTextReader reader)
        {
            _log.Trace("parsing hsl filter");
            var stringH = reader.GetAttribute("h");
            if (stringH != null && !stringH.Equals(""))
                _hShift = Int32.Parse(stringH);
            _log.Trace("hshift = " + _hShift);
            var stringS = reader.GetAttribute("s");
            if (stringS != null && !stringS.Equals(""))
                _sShift = float.Parse(stringS);
            _log.Trace("sShift = " + _sShift);
            var stringL = reader.GetAttribute("l");
            if (stringL != null && !stringL.Equals(""))
                _lShift = float.Parse(stringL);
            _log.Trace("lshift = " + _lShift);
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
