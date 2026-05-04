using NLog;
using System;
using System.Xml;

namespace NomadLogic.iBrand.Transformation.Filters
{
    class ShiftHSL : ImageFilter
    {
        private static Logger log = LogManager.GetCurrentClassLogger();
        private int hShift = 0;
        private float sShift = 0.0f;
        private float lShift = 0.0f;


        protected override byte[] FilterRgb(byte alpha, byte r, byte g, byte b)
        {
            byte[] argb = new byte[4];
            float h, s, l;
            float[] hsl = RGBToHSL(r, g, b);
            h = hsl[0];
            s = hsl[1];
            l = hsl[2];

            h += hShift;
            if (h > 360)
                h = 360;
            if (h < 0)
                h = 0;
            s += sShift;
            if (s < 0)
                s = 0;
            if (s > 1)
                s = 1;
            l += lShift;
            if (l < 0)
                l = 0;
            if (l > 1)
                l = 1;

            int[] nouveauRGB = HSLtoRGB(h, s, l);
            r = (byte)nouveauRGB[0];
            g = (byte)nouveauRGB[1];
            b = (byte)nouveauRGB[2];

            argb[0] = alpha;
            argb[1] = r;
            argb[2] = g;
            argb[3] = b;

            return argb;
        }

        public override void ParseXml(XmlTextReader reader)
        {
            log.Trace("parsing hsl filter");
            string string_h = reader.GetAttribute("h");
            if (string_h != null && !string_h.Equals(""))
                hShift = Int32.Parse(string_h);
            log.Trace("hshift = " + hShift);
            string string_s = reader.GetAttribute("s");
            if (string_s != null && !string_s.Equals(""))
                sShift = float.Parse(string_s);
            log.Trace("sShift = " + sShift);
            string string_l = reader.GetAttribute("l");
            if (string_l != null && !string_l.Equals(""))
                lShift = float.Parse(string_l);
            log.Trace("lshift = " + lShift);
        }

        private float[] RGBToHSL(int r, int g, int b)
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

        private int[] HSLtoRGB(float h, float s, float l)
        {
            float p, q, hk;
            // r = 0, g = 1, b = 2
            float[] t = new float[3];
            // r = 0, g = 1, b = 2
            float[] adjust = new float[] { 1 / 3f, 0, 0 - (1 / 3f) };
            // r = 0, g = 1, b = 2
            int[] rgb = new int[3];

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

                for (int i = 0; i < t.Length; i++)
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
