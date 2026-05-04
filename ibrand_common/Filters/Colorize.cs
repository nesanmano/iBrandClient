using System;
using System.Xml;

namespace NomadLogic.iBrand.Transformation.Filters
{
    class Colorize : ImageFilter
    {
        private int hSet = 0;
        private float sSet = 0.0f;
        private float lShift = 0.0f;
        private float opacity = 1f;


        protected override byte[] FilterRgb(byte alpha, byte r, byte g, byte b)
        {
            byte[] argb = new byte[4];
            float h, s, l;
            float[] hsl = RGBToHSL(r, g, b);
            h = hsl[0];
            s = hsl[1];
            l = hsl[2];

            h = hSet;
            if (h > 360)
                h = 360;
            if (h < 0)
                h = 0;
            s = sSet;
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
            byte newR = (byte)nouveauRGB[0];
            byte newG = (byte)nouveauRGB[1];
            byte newB = (byte)nouveauRGB[2];

            argb[0] = alpha;
            argb[1] = (byte)(r * (1f - opacity) + newR * opacity);
            argb[2] = (byte)(g * (1f - opacity) + newG * opacity);
            argb[3] = (byte)(b * (1f - opacity) + newB * opacity);

            return argb;
        }

        public override void ParseXml(XmlTextReader reader)
        {
            string string_h = reader.GetAttribute("h");
            if (string_h != null && !string_h.Equals(""))
                hSet = Int32.Parse(string_h);

            string string_s = reader.GetAttribute("s");
            if (string_s != null && !string_s.Equals(""))
                sSet = float.Parse(string_s);

            string string_l = reader.GetAttribute("l");
            if (string_l != null && !string_l.Equals(""))
                lShift = float.Parse(string_l);

            string string_opacity = reader.GetAttribute("opacity");
            if (string_opacity != null && !string_opacity.Equals(""))
                opacity = float.Parse(string_opacity);
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
