using System;
using System.Collections.Generic;
using System.Xml;

namespace NomadLogic.iBrand.Transformation.Filters
{
    public class GreenScreenFilterHsl : AbstractGreenScreenFilter
    {
        private int _lowerHue = 75;
        private int _higherHue = 180;
        private float _lowerSaturation = 0.3f;
        private float _higherSaturation = 1f;
        private float _lowerLuminance = 0.15f;
        private float _higherLuminance = 0.95f;

        private int _semiLowerHue = 60;
        private int _semiHigherHue = 190;
        private float _semiLowerSaturation = 0.15f;
        private float _semiHigherSaturation = 1f;
        private float _semiLowerLuminance = 0.15f;
        private float _semiHigherLuminance = 0.97f;

        public GreenScreenFilterHsl()
        {
        }

        protected override byte[] filterRGB(byte alpha, byte r, byte g, byte b)
        {
            int a;
            byte[] argb = new byte[4];
            float h, s, l;
            float[] hsl = RgbtoHsl(r, g, b);
            h = hsl[0];
            s = hsl[1];
            l = hsl[2];

            if (h <= _higherHue &&
                            h >= _lowerHue &&
                            l <= _higherLuminance &&
                            l >= _lowerLuminance &&
                            s <= _higherSaturation &&
                            s >= _lowerSaturation)
            {
                a = 0;

            }
            else
            {
                a = 255;

                bool estUnPeuVert = false;
                if (h >= _semiLowerHue &&
                                h <= _lowerHue)
                {
                    //				h -= LOWER_HUE_OFFSET;
                    //				if(h < 0) {
                    //					h += 360;
                    //				}
                    h = _semiLowerHue;
                    estUnPeuVert = true;

                }
                else if (h >= _higherHue &&
                                h <= _semiHigherHue)
                {
                    //				h += HIGHER_HUE_OFFSET;
                    //				if(h > 360) {
                    //					h -= 360;
                    //				}
                    h = _semiHigherHue;
                    estUnPeuVert = true;

                }
                else if (h < _higherHue &&
                              h > _lowerHue)
                {
                    estUnPeuVert = true;
                    s = 0;
                }

                if (estUnPeuVert)
                {
                    if (s >= _semiLowerSaturation &&
                                    s <= _lowerSaturation)
                    {
                        //					s -= LOWER_SATURATION_OFFSET;
                        //					if(s < 0) {
                        //						s = 0;
                        //					}
                        s = _semiLowerSaturation;
                    }
                    else if (s >= _higherSaturation &&
                                    s <= _semiHigherSaturation)
                    {
                        //					s += HIGHER_SATURATION_OFFSET;
                        //					if(s > 1) {
                        //						s = 1;
                        //					}
                        s = _semiHigherSaturation;
                    }

                    if (l >= _semiLowerLuminance &&
                                    l <= _lowerLuminance)
                    {
                        //					l -= LOWER_LUMINANCE_OFFSET;
                        //					if(l < 0) {
                        //						l = 0;
                        //					}
                        l = _semiLowerLuminance;
                    }
                    else if (l >= _higherLuminance &&
                                    l <= _semiHigherLuminance)
                    {
                        //					l += HIGHER_LUMINANCE_OFFSET;
                        //					if(l > 1) {
                        //						l = 1;
                        //					}
                        l = _semiHigherLuminance;
                    }
                }

                int[] nouveauRgb = HsLtoRgb(h, s, l);
                r = (byte)nouveauRgb[0];
                g = (byte)nouveauRgb[1];
                b = (byte)nouveauRgb[2];

            }
            argb[0] = (byte)a;
            argb[1] = (byte)r;
            argb[2] = (byte)g;
            argb[3] = (byte)b;

            return argb;
        }

        public override void ParseXML(XmlTextReader reader)
        {

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

        public override void setParameters(Dictionary<string, float> parameters)
        {
            if (parameters.ContainsKey("LOWER_HUE"))
            {
                _lowerHue = Convert.ToInt32(parameters["LOWER_HUE"]);
            }

            if (parameters.ContainsKey("HIGHER_HUE"))
            {
                _higherHue = Convert.ToInt32(parameters["HIGHER_HUE"]);
            }

            if (parameters.ContainsKey("LOWER_SATURATION"))
            {
                _lowerSaturation = parameters["LOWER_SATURATION"];
            }

            if (parameters.ContainsKey("HIGHER_SATURATION"))
            {
                _higherSaturation = parameters["HIGHER_SATURATION"];
            }

            if (parameters.ContainsKey("LOWER_LUMINANCE"))
            {
                _lowerLuminance = parameters["LOWER_LUMINANCE"];
            }

            if (parameters.ContainsKey("HIGHER_LUMINANCE"))
            {
                _higherLuminance = parameters["HIGHER_LUMINANCE"];
            }

            if (parameters.ContainsKey("SEMI_LOWER_HUE"))
            {
                _semiLowerHue = Convert.ToInt32(parameters["SEMI_LOWER_HUE"]);
            }

            if (parameters.ContainsKey("SEMI_HIGHER_HUE"))
            {
                _semiHigherHue = Convert.ToInt32(parameters["SEMI_HIGHER_HUE"]);
            }

            if (parameters.ContainsKey("SEMI_LOWER_SATURATION"))
            {
                _semiLowerSaturation = parameters["SEMI_LOWER_SATURATION"];
            }

            if (parameters.ContainsKey("SEMI_HIGHER_SATURATION"))
            {
                _semiHigherSaturation = parameters["SEMI_HIGHER_SATURATION"];
            }

            if (parameters.ContainsKey("SEMI_LOWER_LUMINANCE"))
            {
                _semiLowerLuminance = parameters["SEMI_LOWER_LUMINANCE"];
            }

            if (parameters.ContainsKey("SEMI_HIGHER_LUMINANCE"))
            {
                _semiHigherLuminance = parameters["SEMI_HIGHER_LUMINANCE"];
            }
        }
    }
}
