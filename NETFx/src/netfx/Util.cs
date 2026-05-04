using System;
using System.Drawing;

/*
 * This class provides common toolkit amongst modules. Exports 3 methods and 1 class. 
// ``` clamp(val, min, max) ```
// 
// Ensures a value is between min and max.
// 
// ``` dist(x1, x2) ```
//
// Calculates the absolute distance between two values.
//
// ``` normalize(val, dmin, dmax, smin, smax) ```
//
// Projects a value in the source range into the corresponding
// value in the destination range.
//
// ``` Bezier(C1, C2, C3, C4) ```
//
// A Bezier curve implementation.
 */

namespace NetFx
{
    class Util
    {
        public static float clamp(float val, float min, float max)
        {
            return Math.Min(max, Math.Max(min, val));
        }

        public static float dist(float x1, float x2)
        {
            return (float)Math.Sqrt(Math.Pow(x2 - x1, 2));
        }

        public static float normalize(float val, float dmin, float dmax, float smin, float smax)
        {
            float sdist = dist(smin, smax);
            float ddist = dist(dmin, dmax);
            float ratio = ddist / sdist;
            val = clamp(val, smin, smax);
            return dmin + (val-smin) * ratio;
        }

        public static float level(int val, int inLeft, float gamma, int inRight, int outLeft, int outRight)
        {
            int outMid = (outRight + outLeft) / 2;
            int inMid = (int)(outMid / gamma);
            inMid = (int)normalize(inMid, inLeft, inRight, 0, 255);
            if (val == inMid)
                return outMid;
            else if (val <= inLeft)
            {
                return outLeft;
            }
            else if (val >= inRight)
            {
                return outRight;
            }
            else if (val < inMid)
            {
                return normalize(val, outLeft, outMid, inLeft, inMid);
            }
            else
            {
                // ça veut dire que val > inMid
                return normalize(val, outMid, outRight, inMid, inRight);
            }

        }


        // **Adapted from (with special thanks)** <br>
        // 13thParallel.org Beziér Curve Code <br>
        // *by Dan Pupius (www.pupius.net)*
        public class Bezier
        {
            Point C1,C2,C3,C4;

            public Bezier(Point c1, Point c2, Point c3, Point c4)
            {
                C1 = c1; C2 = c2; C3 = c3; C4 = c4;
                
            }

            float B1(float t) { return t * t * t; }
            float B2(float t) { return 3 * t * t * (1 - t); }
            float B3(float t) { return 3 * t * (1 - t) * (1 - t); }
            float B4(float t) { return (1 - t) * (1 - t) * (1 - t); }

            Point getPoint(float t)
            {
                return new Point(
                    (int)(C1.X * B1(t) + C2.X * B2(t) + C3.X * B3(t) + C4.X * B4(t)),
                    (int)(C1.Y * B1(t) + C2.Y * B2(t) + C3.Y * B3(t) + C4.Y * B4(t))
                    );
            }

            // Creates a color table for 1024 points. To create the table
            // 1024 bezier points are calculated with t = i/1024 in every
            // loop iteration and map is created for [x] = y. This is then
            // used to project a color RGB value (x) to another color RGB
            // value (y).

            public int[] genColorTable()
            {
                int[] points = new int[1024];

                for (int i = 0; i < 1024; i++)
                {
                    Point point = getPoint(i / 1024.0f);
                    points[point.X] = point.Y;
                }

                return points;
            }
        }

        public static float[] RGBToHSL(int r, int g, int b)
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

        public static int[] HSLtoRGB(float h, float s, float l)
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
        public static int coverlay(int b, int t) { return (b > 128) ? 255 - 2 * (255 - t) * (255 - b) / 255 : (b * t * 2) / 255; }

    }
}
