using System;
using System.Collections.Generic;
using System.Drawing;

namespace NetFx
{
    public class Effects
    {

        BitmapW convolve(BitmapW image, float[,] kernel, int kw, int kh)
        {
            BitmapW temp = image.Clone();

            // int kh = kernel;
            //int kw = kh; //kernel[0].Length / 2;
            int i = 0, j = 0, n = 0, m = 0, cr, cg, cb,
            h = image.Height(),
            w = image.Width();

            for (i = 0; i < h; i++)
            {
                for (j = 0; j < w; j++)
                {
                    //kernel loop
                    float r = 0, g = 0, b = 0;
                    for (n = -kh; n <= kh; n++)
                    {
                        for (m = -kw; m <= kw; m++)
                        {
                            if (i + n >= 0 && i + n < h)
                            {
                                if (j + m >= 0 && j + m < w)
                                {
                                    float f = kernel[m + kw, n + kh];
                                    if (f == 0) { continue; }
                                    Color colortemp = image.GetPixel(j + m, i + n);
                                    cr = colortemp.R; cg = colortemp.G; cb = colortemp.B;

                                    r += cr * f;
                                    g += cg * f;
                                    b += cb * f;
                                }
                            }
                        }
                    }
                    //kernel loop end

                    temp.SetPixel(j, i, Color.FromArgb(255, (int)Util.clamp(r, 0, 255), (int)Util.clamp(g, 0, 255), (int)Util.clamp(b, 0, 255)));
                }
            }
            return temp;

        }

        /// <summary>
        /// #### Adjust [-255,255 for each channel]
        /// </summary>
        /// <param name="image"></param>
        /// <param name="pr"></param>
        /// <param name="pg"></param>
        /// <param name="pb"></param>
        /// <returns></returns>
        public BitmapW adjust(BitmapW image, float pr, float pg, float pb)
        {
            float cr = 0, cg = 0, cb = 0;
            pr /= 100; pg /= 100; pb /= 100;
            int i = 0, j = 0,
            h = image.Height(),
            w = image.Width();

            for (i = 0; i < w; i++)
            {
                for (j = 0; j < h; j++)
                {
                    Color temp = image.GetPixel(i, j);
                    cr = temp.R; cg = temp.G; cb = temp.B;

                    if (pr > 0) cr += (255 - cr) * pr; else cr -= cr * Math.Abs(pr);
                    if (pg > 0) cg += (255 - cg) * pg; else cg -= cg * Math.Abs(pg);
                    if (pb > 0) cb += (255 - cb) * pb; else cb -= cb * Math.Abs(pb);
                    /*
                    cr *= (1.0f + pr);
                    cg *= (1.0f + pg);
                    cb *= (1.0f + pb);
                    */


                    image.SetPixel(i, j, Color.FromArgb((int)Util.clamp(cr, 0, 255), (int)Util.clamp(cg, 0, 255), (int)Util.clamp(cb, 0, 255)));
                }
            }
            return image;
        }

        /// <summary>
        /// #### Hue [0, 360]
        /// #### Saturation [0, 1]
        /// #### Lightness [0, 1]
        /// </summary>
        /// <param name="image"></param>
        /// <param name="pr"></param>
        /// <param name="pg"></param>
        /// <param name="pb"></param>
        /// <returns></returns>
        public BitmapW adjustHSL(BitmapW image, float ph, float ps, float pl)
        {
            float cr = 0, cg = 0, cb = 0, ca = 0;
            
            int i = 0, j = 0,
            h = image.Height(),
            w = image.Width();

            for (i = 0; i < w; i++)
            {
                for (j = 0; j < h; j++)
                {
                    Color temp = image.GetPixel(i, j);
                    cr = temp.R; cg = temp.G; cb = temp.B; ca = temp.A;

                    if (ca != 0)
                    {
                        float dh, ds, dl;
                        float[] hsl = Util.RGBToHSL((int)cr, (int)cg, (int)cb);
                        dh = hsl[0];
                        ds = hsl[1];
                        dl = hsl[2];

                        dh += ph;
                        if (dh < 0)
                            dh += 360;
                        else if (dh >= 360)
                            dh %= 360;

                        ds += ps;
                        ds = Util.clamp(ds, 0f, 1f);

                        dl += pl;
                        dl = Util.clamp(dl, 0f, 1f);

                        int[] nouveauRGB = Util.HSLtoRGB(dh, ds, dl);
                        cr = nouveauRGB[0];
                        cg = nouveauRGB[1];
                        cb = nouveauRGB[2];


                        image.SetPixel(i, j, Color.FromArgb((int)Util.clamp(ca, 0, 255), (int)Util.clamp(cr, 0, 255), (int)Util.clamp(cg, 0, 255), (int)Util.clamp(cb, 0, 255)));
                    }
                    
                }
            }
            return image;
        }

        /// <summary>
        /// #### Brighten [-100, 100]
        /// </summary>
        /// <param name="image"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        public BitmapW brighten(BitmapW image, float p)
        {
            p = Util.normalize(p, -255, 255, -100, 100);
            float cr = 0, cg = 0, cb = 0;
            int i = 0, j = 0,
            h = image.Height(),
            w = image.Width();

            for (i = 0; i < w; i++)
            {
                for (j = 0; j < h; j++)
                {
                    Color temp = image.GetPixel(i, j);
                    cr = temp.R; cg = temp.G; cb = temp.B;

                    cr += (p);
                    cg += (p);
                    cb += (p);

                    image.SetPixel(i, j, Color.FromArgb((int)Util.clamp(cr, 0, 255), (int)Util.clamp(cg, 0, 255), (int)Util.clamp(cb, 0, 255)));
                }
            }
            return image;
        }

        /// <summary>
        /// #### Alpha [-100, 100]
        /// </summary>
        /// <param name="image"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        public BitmapW alpha(BitmapW image, float p)
        {
            p = Util.normalize(p, 0, 255, -100, 100);
            float cr = 0, cg = 0, cb = 0, ca;
            int i = 0, j = 0,
            h = image.Height(),
            w = image.Width();

            for (i = 0; i < w; i++)
            {
                for (j = 0; j < h; j++)
                {
                    Color temp = image.GetPixel(i, j);
                    cr = temp.R; cg = temp.G; cb = temp.B; ca = temp.A;

                    ca = (p);

                    image.SetPixel(i, j, Color.FromArgb((int)Util.clamp(ca, 0, 255), (int)Util.clamp(cr, 0, 255),
                        (int)Util.clamp(cg, 0, 255), (int)Util.clamp(cb, 0, 255)));
                }
            }
            return image;
        }


        /// <summary>
        /// #### Saturate [-100, 100]
        /// </summary>
        /// <param name="image"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        public BitmapW saturate(BitmapW image, float p)
        {

            p = Util.normalize(p, 0, 2, -100, 100);
            float cr = 0, cg = 0, cb = 0, ca;
            int i = 0, j = 0,
            h = image.Height(),
            w = image.Width();

            for (i = 0; i < w; i++)
            {
                for (j = 0; j < h; j++)
                {
                    Color temp = image.GetPixel(i, j);
                    cr = temp.R; cg = temp.G; cb = temp.B; ca = temp.A;
                    float avg = (cr + cg + cb) / 3;

                    cr = avg + p * (cr - avg);
                    cg = avg + p * (cg - avg);
                    cb = avg + p * (cb - avg);

                    image.SetPixel(i, j, Color.FromArgb((int)Util.clamp(ca, 0, 255), (int)Util.clamp(cr, 0, 255),
                        (int)Util.clamp(cg, 0, 255), (int)Util.clamp(cb, 0, 255)));
                }
            }
            return image;
        }

        public BitmapW Brannan(BitmapW image)
        {
            float cr = 0, cg = 0, cb = 0, ca;
            int i = 0, j = 0,
            h = image.Height(),
            w = image.Width();

            // params pour le premier ajustement de levels
            Dictionary<String, LevelParams> parameters = new Dictionary<string, LevelParams>();
            LevelParams param = new LevelParams();
            param.BlackIn = 0;
            param.Gamma = 1f;
            param.WhiteIn = 255;
            param.BlackOut = 9;
            param.WhiteOut = 255;
            parameters.Add("RGB", param);

            param = new LevelParams();
            param.BlackIn = 0;
            param.Gamma = 1f;
            param.WhiteIn = 228;
            param.BlackOut = 23;
            param.WhiteOut = 255;
            parameters.Add("Red", param);

            param = new LevelParams();
            param.BlackIn = 0;
            param.Gamma = 1f;
            param.WhiteIn = 255;
            param.BlackOut = 3;
            param.WhiteOut = 255;
            parameters.Add("Green", param);

            param = new LevelParams();
            param.BlackIn = 0;
            param.Gamma = 1.0f;
            param.WhiteIn = 239;
            param.BlackOut = 12;
            param.WhiteOut = 255;
            parameters.Add("Blue", param);

            // params pour le 2e ajustement de levels
            Dictionary<String, LevelParams> parameters2 = new Dictionary<string, LevelParams>();
            param = new LevelParams();
            param.BlackIn = 9;
            param.Gamma = 0.91f;
            param.WhiteIn = 255;
            param.BlackOut = 7;
            param.WhiteOut = 255;
            parameters2.Add("RGB", param);

            param = new LevelParams();
            param.BlackIn = 0;
            param.Gamma = 1f;
            param.WhiteIn = 255;
            param.BlackOut = 9;
            param.WhiteOut = 255;
            parameters2.Add("Red", param);

            param = new LevelParams();
            param.BlackIn = 0;
            param.Gamma = 1f;
            param.WhiteIn = 224;
            param.BlackOut = 3;
            param.WhiteOut = 255;
            parameters2.Add("Green", param);

            param = new LevelParams();
            param.BlackIn = 0;
            param.Gamma = 0.94f;
            param.WhiteIn = 255;
            param.BlackOut = 15;
            param.WhiteOut = 255;
            parameters2.Add("Blue", param);

            for (i = 0; i < w; i++)
            {
                for (j = 0; j < h; j++)
                {
                    Color temp = image.GetPixel(i, j);
                    cr = temp.R; cg = temp.G; cb = temp.B; ca = temp.A;
                    if (ca != 0)
                    {
                        float saturate = 0.7f;

                        float avg = (cr + cg + cb) / 3;

                        cr = avg + saturate * (cr - avg);
                        cg = avg + saturate * (cg - avg);
                        cb = avg + saturate * (cb - avg);

                        cr = Util.coverlay((int)cr, (int)avg) * 0.37f + cr * 0.63f;
                        cg = Util.coverlay((int)cg, (int)avg) * 0.37f + cg * 0.63f;
                        cb = Util.coverlay((int)cb, (int)avg) * 0.37f + cb * 0.63f;

                        param = parameters["Red"];
                        cr = Util.level((int)cr, param.BlackIn, param.Gamma, param.WhiteIn, param.BlackOut, param.WhiteOut);
                        param = parameters["Green"];
                        cg = Util.level((int)cg, param.BlackIn, param.Gamma, param.WhiteIn, param.BlackOut, param.WhiteOut);
                        param = parameters["Blue"];
                        cb = Util.level((int)cb, param.BlackIn, param.Gamma, param.WhiteIn, param.BlackOut, param.WhiteOut);
                        param = parameters["RGB"];
                        cr = Util.level((int)cr, param.BlackIn, param.Gamma, param.WhiteIn, param.BlackOut, param.WhiteOut);
                        cg = Util.level((int)cg, param.BlackIn, param.Gamma, param.WhiteIn, param.BlackOut, param.WhiteOut);
                        cb = Util.level((int)cb, param.BlackIn, param.Gamma, param.WhiteIn, param.BlackOut, param.WhiteOut);

                        //brigthness / contrast
                        cr = 255 * contrastc((cr - 25) / 255, 1.05f);
                        cg = 255 * contrastc((cg - 25) / 255, 1.05f);
                        cb = 255 * contrastc((cb - 25) / 255, 1.05f);

                        //2e levels
                        param = parameters2["Red"];
                        cr = Util.level((int)cr, param.BlackIn, param.Gamma, param.WhiteIn, param.BlackOut, param.WhiteOut);
                        param = parameters2["Green"];
                        cg = Util.level((int)cg, param.BlackIn, param.Gamma, param.WhiteIn, param.BlackOut, param.WhiteOut);
                        param = parameters2["Blue"];
                        cb = Util.level((int)cb, param.BlackIn, param.Gamma, param.WhiteIn, param.BlackOut, param.WhiteOut);
                        param = parameters2["RGB"];
                        cr = Util.level((int)cr, param.BlackIn, param.Gamma, param.WhiteIn, param.BlackOut, param.WhiteOut);
                        cg = Util.level((int)cg, param.BlackIn, param.Gamma, param.WhiteIn, param.BlackOut, param.WhiteOut);
                        cb = Util.level((int)cb, param.BlackIn, param.Gamma, param.WhiteIn, param.BlackOut, param.WhiteOut);

                        //2e brigthness / contrast
                        cr = 255 * contrastc((cr - 10) / 255, 0.99f);
                        cg = 255 * contrastc((cg - 10) / 255, 0.99f);
                        cb = 255 * contrastc((cb - 10) / 255, 0.99f);

                        //3e brigthness / contrast
                        cr += 36;
                        cg += 36;
                        cb += 36;

                        // multiply
                        cr = (255 * cr) / 255;
                        cg = (248 * cg) / 255;
                        cb = (242 * cb) / 255;

                        image.SetPixel(i, j, Color.FromArgb((int)Util.clamp(ca, 0, 255), (int)Util.clamp(cr, 0, 255),
                            (int)Util.clamp(cg, 0, 255), (int)Util.clamp(cb, 0, 255)));
                    }
                    
                }
            }
            return image;
        }

        public BitmapW EarlyBird(BitmapW image)
        {
            float cr = 0, cg = 0, cb = 0, ca;
            int i = 0, j = 0,
            h = image.Height(),
            w = image.Width(),
            centerw = w / 2,
            centerh = h / 2,
            maxdist = dist(0, 0, centerw, centerh);

            maxdist = (int)(maxdist * 0.7f);
            int radius = maxdist / 2;

            // params pour le premier ajustement de levels
            Dictionary<String, LevelParams> parameters = new Dictionary<string, LevelParams>();
            LevelParams param = new LevelParams();
            param.BlackIn = 0;
            param.Gamma = 1.19f;
            param.WhiteIn = 255;
            param.BlackOut = 0;
            param.WhiteOut = 255;
            parameters.Add("RGB", param);

            param = new LevelParams();
            param.BlackIn = 0;
            param.Gamma = 1f;
            param.WhiteIn = 255;
            param.BlackOut = 27;
            param.WhiteOut = 255;
            parameters.Add("Red", param);

            // params pour le 2e ajustement de levels
            Dictionary<String, LevelParams> parameters2 = new Dictionary<string, LevelParams>();
            param = new LevelParams();
            param.BlackIn = 0;
            param.Gamma = 0.92f;
            param.WhiteIn = 235;
            param.BlackOut = 0;
            param.WhiteOut = 255;
            parameters2.Add("RGB", param);

            for (i = 0; i < w; i++)
            {
                for (j = 0; j < h; j++)
                {
                    Color temp = image.GetPixel(i, j);
                    cr = temp.R; cg = temp.G; cb = temp.B; ca = temp.A;
                    if (ca != 0)
                    {
                        float saturate = 0.7f;

                        float avg = (cr + cg + cb) / 3;

                        // 1er hue / saturation
                        cr = avg + saturate * (cr - avg);
                        cg = avg + saturate * (cg - avg);
                        cb = avg + saturate * (cb - avg);

                        param = parameters["Red"];
                        cr = Util.level((int)cr, param.BlackIn, param.Gamma, param.WhiteIn, param.BlackOut, param.WhiteOut);
                        param = parameters["RGB"];
                        cr = Util.level((int)cr, param.BlackIn, param.Gamma, param.WhiteIn, param.BlackOut, param.WhiteOut);
                        cg = Util.level((int)cg, param.BlackIn, param.Gamma, param.WhiteIn, param.BlackOut, param.WhiteOut);
                        cb = Util.level((int)cb, param.BlackIn, param.Gamma, param.WhiteIn, param.BlackOut, param.WhiteOut);

                        //1er brigthness / contrast
                        cr = 255 * contrastc((cr + 15) / 255, 1.36f);//38, 1.36
                        cg = 255 * contrastc((cg + 15) / 255, 1.36f);
                        cb = 255 * contrastc((cb + 15) / 255, 1.36f);

                        // 2e hue / saturation
                        saturate = 0.83f;
                        avg = (cr + cg + cb) / 3;
                        cr = avg + saturate * (cr - avg);
                        cg = avg + saturate * (cg - avg);
                        cb = avg + saturate * (cb - avg);

                        //2e levels
                        param = parameters2["RGB"];
                        cr = Util.level((int)cr, param.BlackIn, param.Gamma, param.WhiteIn, param.BlackOut, param.WhiteOut);
                        cg = Util.level((int)cg, param.BlackIn, param.Gamma, param.WhiteIn, param.BlackOut, param.WhiteOut);
                        cb = Util.level((int)cb, param.BlackIn, param.Gamma, param.WhiteIn, param.BlackOut, param.WhiteOut);

                        // multiply
                        cr = (252 * cr) / 255;
                        cg = (243 * cg) / 255;
                        cb = (214 * cb) / 255;

                        //vignette
                        int distance = dist(i, j, centerw, centerh);
                        distance = (distance > radius) ? distance - radius : 0;

                        float ratio = ((distance / (float)(maxdist)));
                        float tempColor = (1 - ratio) * 255 + (ratio * 184);
                        //color burn
                        cr = 255 - (((255 - cr) / tempColor) * 255);
                        cg = 255 - (((255 - cg) / tempColor) * 255);
                        cb = 255 - (((255 - cb) / tempColor) * 255);
                        // multiply
                        cr = (tempColor * cr) / 255;
                        cg = (tempColor * cg) / 255;
                        cb = (tempColor * cb) / 255;

                        image.SetPixel(i, j, Color.FromArgb((int)Util.clamp(ca, 0, 255), (int)Util.clamp(cr, 0, 255),
                            (int)Util.clamp(cg, 0, 255), (int)Util.clamp(cb, 0, 255)));
                    }

                }
            }
            return image;
        }

        /// <summary>
        /// #### Invert
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        public BitmapW invert(BitmapW image)
        {
            float cr = 0, cg = 0, cb = 0, ca;
            int i = 0, j = 0,
            h = image.Height(),
            w = image.Width();

            for (i = 0; i < w; i++)
            {
                for (j = 0; j < h; j++)
                {
                    Color temp = image.GetPixel(i, j);
                    cr = temp.R; cg = temp.G; cb = temp.B; ca = temp.A;

                    cr = 255 - cr;
                    cg = 255 - cg;
                    cb = 255 - cb;

                    image.SetPixel(i, j, Color.FromArgb((int)Util.clamp(ca, 0, 255), (int)Util.clamp(cr, 0, 255),
                        (int)Util.clamp(cg, 0, 255), (int)Util.clamp(cb, 0, 255)));
                }
            }
            return image;
        }

        /// <summary>
        /// #### Posterize [1, 255]
        /// </summary>
        /// <param name="image"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        public BitmapW posterize(BitmapW image, float p)
        {
            p = Util.clamp(p, 1, 255);
            int step = (int)Math.Floor(255 / p);
            float cr = 0, cg = 0, cb = 0, ca;
            int i = 0, j = 0,
            h = image.Height(),
            w = image.Width();

            for (i = 0; i < w; i++)
            {
                for (j = 0; j < h; j++)
                {
                    Color temp = image.GetPixel(i, j);
                    cr = temp.R; cg = temp.G; cb = temp.B; ca = temp.A;


                    cr = (float)Math.Floor(cr / (float)(step)) * step;
                    cg = (float)Math.Floor(cg / (float)(step)) * step;
                    cb = (float)Math.Floor(cb / (float)(step)) * step;

                    image.SetPixel(i, j, Color.FromArgb((int)Util.clamp(ca, 0, 255), (int)Util.clamp(cr, 0, 255),
                        (int)Util.clamp(cg, 0, 255), (int)Util.clamp(cb, 0, 255)));
                }
            }
            return image;
        }

        /// <summary>
        /// #### Gamma [-100, 100]
        /// </summary>
        /// <param name="image"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        public BitmapW gamma(BitmapW image, float p)
        {
            p = Util.normalize(p, 0, 2, -100, 100);

            float cr = 0, cg = 0, cb = 0, ca;
            int i = 0, j = 0,
            h = image.Height(),
            w = image.Width();

            for (i = 0; i < w; i++)
            {
                for (j = 0; j < h; j++)
                {
                    Color temp = image.GetPixel(i, j);
                    cr = temp.R; cg = temp.G; cb = temp.B; ca = temp.A;

                    cr = (float)Math.Pow(cr, p);
                    cg = (float)Math.Pow(cg, p);
                    cb = (float)Math.Pow(cb, p);

                    image.SetPixel(i, j, Color.FromArgb((int)Util.clamp(ca, 0, 255), (int)Util.clamp(cr, 0, 255),
                        (int)Util.clamp(cg, 0, 255), (int)Util.clamp(cb, 0, 255)));
                }
            }
            return image;
        }

        /// <summary>
        /// #### blackIn [0, 255]
        /// #### Gamma [0, 10]
        /// #### whiteIn [0, 255]
        /// #### blackOut [0, 255]
        /// #### whiteOut [0, 255]
        /// </summary>
        /// <param name="image"></param>
        /// <param name="blackIn"></param>
        /// <param name="gamma"></param>
        /// <param name="whiteIn"></param>
        /// <param name="blackOut"></param>
        /// <param name="whiteOut"></param>
        /// <returns></returns>
        public BitmapW levels(BitmapW image, Dictionary<String, LevelParams> p )
        {

            float cr = 0, cg = 0, cb = 0, ca;
            int i = 0, j = 0,
            h = image.Height(),
            w = image.Width();

            for (i = 0; i < w; i++)
            {
                for (j = 0; j < h; j++)
                {
                    Color temp = image.GetPixel(i, j);
                    cr = temp.R; cg = temp.G; cb = temp.B; ca = temp.A;

                    if (p.ContainsKey("Red"))
                    {
                        LevelParams param = p["Red"];
                        cr = Util.level((int)cr, param.BlackIn, param.Gamma, param.WhiteIn, param.BlackOut, param.WhiteOut);
                    }
                    if (p.ContainsKey("Green"))
                    {
                        LevelParams param = p["Green"];
                        cg = Util.level((int)cg, param.BlackIn, param.Gamma, param.WhiteIn, param.BlackOut, param.WhiteOut);
                    }
                    if (p.ContainsKey("Blue"))
                    {
                        LevelParams param = p["Blue"];
                        cb = Util.level((int)cb, param.BlackIn, param.Gamma, param.WhiteIn, param.BlackOut, param.WhiteOut);
                    }

                    if (p.ContainsKey("RGB"))
                    {
                        LevelParams param = p["RGB"];
                        cr = Util.level((int)cr, param.BlackIn, param.Gamma, param.WhiteIn, param.BlackOut, param.WhiteOut);
                        cg = Util.level((int)cg, param.BlackIn, param.Gamma, param.WhiteIn, param.BlackOut, param.WhiteOut);
                        cb = Util.level((int)cb, param.BlackIn, param.Gamma, param.WhiteIn, param.BlackOut, param.WhiteOut);
                    }

                    image.SetPixel(i, j, Color.FromArgb((int)Util.clamp(ca, 0, 255), (int)Util.clamp(cr, 0, 255),
                        (int)Util.clamp(cg, 0, 255), (int)Util.clamp(cb, 0, 255)));
                }
            }
            return image;
        }


        float contrastc(float f, float c) { return (f - 0.5f) * c + 0.5f; }
        /// <summary>
        /// #### Constrast [-100, 100]
        /// </summary>
        /// <param name="f"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        public BitmapW contrast(BitmapW image, float p)
        {
            p = Util.normalize(p, 0, 2, -100, 100);

            float cr = 0, cg = 0, cb = 0, ca;
            int i = 0, j = 0,
            h = image.Height(),
            w = image.Width();

            for (i = 0; i < w; i++)
            {
                for (j = 0; j < h; j++)
                {
                    Color temp = image.GetPixel(i, j);
                    cr = temp.R; cg = temp.G; cb = temp.B; ca = temp.A;

                    cr = 255 * contrastc(cr / 255, p);
                    cg = 255 * contrastc(cg / 255, p);
                    cb = 255 * contrastc(cb / 255, p);

                    image.SetPixel(i, j, Color.FromArgb((int)Util.clamp(ca, 0, 255), (int)Util.clamp(cr, 0, 255),
                        (int)Util.clamp(cg, 0, 255), (int)Util.clamp(cb, 0, 255)));
                }
            }
            return image;
        }

        /// <summary>
        /// #### Sepia
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        public BitmapW sepia(BitmapW image)
        {
            float cr = 0, cg = 0, cb = 0, ca;
            int i = 0, j = 0,
            h = image.Height(),
            w = image.Width();

            for (i = 0; i < w; i++)
            {
                for (j = 0; j < h; j++)
                {
                    Color temp = image.GetPixel(i, j);
                    cr = temp.R; cg = temp.G; cb = temp.B; ca = temp.A;
                    float tcr = cr, tcg = cg, tcb = cb;
                    cr = (tcr * 0.393f) + (tcg * 0.769f) + (tcb * 0.189f);
                    cg = (tcr * 0.349f) + (tcg * 0.686f) + (tcb * 0.168f);
                    cb = (tcr * 0.272f) + (tcg * 0.534f) + (tcb * 0.131f);

                    image.SetPixel(i, j, Color.FromArgb((int)Util.clamp(ca, 0, 255), (int)Util.clamp(cr, 0, 255),
                        (int)Util.clamp(cg, 0, 255), (int)Util.clamp(cb, 0, 255)));
                }
            }
            return image;
        }

        /// <summary>
        /// #### Colorize
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        public BitmapW Colorize(BitmapW image, float hSet, float sSet, float lShift)
        {
            float cr = 0, cg = 0, cb = 0, ca, ch, cs, cl;
            int i = 0, j = 0,
            h = image.Height(),
            w = image.Width();

            for (i = 0; i < w; i++)
            {
                for (j = 0; j < h; j++)
                {
                    Color temp = image.GetPixel(i, j);
                    cr = temp.R; cg = temp.G; cb = temp.B; ca = temp.A;
                    float tcr = cr, tcg = cg, tcb = cb;
                    float[] hsl = Util.RGBToHSL((int)cr, (int)cg, (int)cb);

                    ch = hsl[0];
                    cs = hsl[1];
                    cl = hsl[2];

                    ch = hSet;
                    if (ch > 360)
                        ch = 360;
                    if (ch < 0)
                        ch = 0;
                    cs = sSet;
                    if (cs < 0)
                        cs = 0;
                    if (cs > 1)
                        cs = 1;
                    cl += lShift;
                    if (cl < 0)
                        cl = 0;
                    if (cl > 1)
                        cl = 1;

                    int[] nouveauRGB = Util.HSLtoRGB(ch, cs, cl);

                    cr = nouveauRGB[0];
                    cg = nouveauRGB[1];
                    cb = nouveauRGB[2];

                    image.SetPixel(i, j, Color.FromArgb((int)Util.clamp(ca, 0, 255), (int)Util.clamp(cr, 0, 255),
                        (int)Util.clamp(cg, 0, 255), (int)Util.clamp(cb, 0, 255)));
                }
            }
            return image;
        }

        /// <summary>
        /// #### Subtract [No Range]
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        public BitmapW subtract(BitmapW image)
        {
            float cr = 0, cg = 0, cb = 0, ca;
            int i = 0, j = 0,
            h = image.Height(),
            w = image.Width();

            for (i = 0; i < w; i++)
            {
                for (j = 0; j < h; j++)
                {
                    Color temp = image.GetPixel(i, j);
                    cr = temp.R; cg = temp.G; cb = temp.B; ca = temp.A;

                    cr -= cr;
                    cg -= cg;
                    cb -= cb;

                    image.SetPixel(i, j, Color.FromArgb((int)Util.clamp(ca, 0, 255), (int)Util.clamp(cr, 0, 255),
                        (int)Util.clamp(cg, 0, 255), (int)Util.clamp(cb, 0, 255)));
                }
            }
            return image;
        }


        /// <summary>
        /// #### Fill [No Range]
        /// </summary>
        /// <param name="image"></param>
        /// <param name="r"></param>
        /// <param name="g"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public BitmapW fill(BitmapW image, int r, int g, int b)
        {
            float cr = 0, cg = 0, cb = 0, ca;
            int i = 0, j = 0,
            h = image.Height(),
            w = image.Width();

            for (i = 0; i < w; i++)
            {
                for (j = 0; j < h; j++)
                {
                    Color temp = image.GetPixel(i, j);
                    cr = temp.R; cg = temp.G; cb = temp.B; ca = temp.A;

                    cr = r;
                    cg = g;
                    cb = b;

                    image.SetPixel(i, j, Color.FromArgb((int)Util.clamp(ca, 0, 255), (int)Util.clamp(cr, 0, 255),
                        (int)Util.clamp(cg, 0, 255), (int)Util.clamp(cb, 0, 255)));
                }
            }
            return image;
        }



        /// <summary>
        /// #### Blur ['simple', 'gaussian']
        /// </summary>
        /// <param name="image"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        public BitmapW blur(BitmapW image, string p)
        {

            BitmapW result;
            if (p == "simple")
            {
                result = convolve(image, new float[,]{
                {1.0f/9, 1.0f/9, 1.0f/9},
                {1.0f/9, 1.0f/9, 1.0f/9},
                {1.0f/9, 1.0f/9, 1.0f/9}
                }, 1, 1
                );
            }
            else //gaussian
            {
                result = convolve(image, new float[,]{
                {1.0f/273, 4.0f/273, 7.0f/273, 4.0f/273, 1.0f/273},
                {4.0f/273, 16.0f/273, 26.0f/273, 16.0f/273, 4.0f/273},
                {7.0f/273, 26.0f/273, 41.0f/273, 26.0f/273, 7.0f/273},
                {4.0f/273, 16.0f/273, 26.0f/273, 16.0f/273, 4.0f/273},             
                {1.0f/273, 4.0f/273, 7.0f/273, 4.0f/273, 1.0f/273}
                }, 2, 2);
            }

            return result;
        }

        /// <summary>
        /// #### Sharpen
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        public BitmapW sharpen(BitmapW image)
        {
            BitmapW result;

            result = convolve(image, new float[,]{
            {0.0f, -0.2f,  0.0f},
            {-0.2f, 1.8f, -0.2f},
            {0.0f, -0.2f,  0.0f}
            }, 1, 1
            );


            return result;
        }

        public BitmapW curves(BitmapW image, Point s, Point c1, Point c2, Point e)
        {
            Util.Bezier bezier = new Util.Bezier(s, c1, c2, e);
            int[] points = bezier.genColorTable();

            float cr = 0, cg = 0, cb = 0, ca;
            int i = 0, j = 0,
            h = image.Height(),
            w = image.Width();

            for (i = 0; i < w; i++)
            {
                for (j = 0; j < h; j++)
                {
                    Color temp = image.GetPixel(i, j);
                    cr = temp.R; cg = temp.G; cb = temp.B; ca = temp.A;

                    cr = points[(int)cr];
                    cg = points[(int)cg];
                    cb = points[(int)cb];

                    image.SetPixel(i, j, Color.FromArgb((int)Util.clamp(ca, 0, 255), (int)Util.clamp(cr, 0, 255),
                        (int)Util.clamp(cg, 0, 255), (int)Util.clamp(cb, 0, 255)));
                }
            }
            return image;

        }

        public BitmapW GreyScale(BitmapW image)
        {
            float cr = 0, cg = 0, cb = 0, ca;
            int i = 0, j = 0,
            h = image.Height(),
            w = image.Width();

            for (i = 0; i < w; i++)
            {
                for (j = 0; j < h; j++)
                {
                    Color temp = image.GetPixel(i, j);
                    cr = temp.R; cg = temp.G; cb = temp.B; ca = temp.A;

                    if (ca != 0)
                    {
                        float lumaAvg = (cr * 77 + cg * 151 + cb * 28) / 256f;
                        cr = lumaAvg;
                        cg = lumaAvg;
                        cb = lumaAvg;
                    }
                    
                    image.SetPixel(i, j, Color.FromArgb((int)Util.clamp(ca, 0, 255), (int)Util.clamp(cr, 0, 255),
                        (int)Util.clamp(cg, 0, 255), (int)Util.clamp(cb, 0, 255)));
                }
            }
            return image;

        }

        public BitmapW Sunset(BitmapW image)
        {
            float cr = 0, cg = 0, cb = 0, ca;
            int i = 0, j = 0,
            h = image.Height(),
            w = image.Width();
            int[][] correction = new int[3][];
            correction[0] = new int[] { 0, 0, 15 };
            correction[2] = new int[] { 15, 0, -15 };
            correction[3] = new int[] { 0, 0, -15 };

            Dictionary<String, LevelParams> parameters = new Dictionary<string, LevelParams>();
            LevelParams param = new LevelParams();
            param.BlackIn = 0;
            param.Gamma = 0.95f;
            param.WhiteIn = 255;
            param.BlackOut = 0;
            param.WhiteOut = 255;
            parameters.Add("RGB", param);

            param = new LevelParams();
            param.BlackIn = 0;
            param.Gamma = 0.85f;
            param.WhiteIn = 255;
            param.BlackOut = 0;
            param.WhiteOut = 255;
            parameters.Add("Green", param);

            param = new LevelParams();
            param.BlackIn = 25;
            param.Gamma = 0.80f;
            param.WhiteIn = 210;
            param.BlackOut = 0;
            param.WhiteOut = 255;
            parameters.Add("Blue", param);

            for (i = 0; i < w; i++)
            {
                for (j = 0; j < h; j++)
                {
                    Color temp = image.GetPixel(i, j);
                    cr = temp.R; cg = temp.G; cb = temp.B; ca = temp.A;

                    if (ca != 0)
                    {
                        float lumaAvg = (cr * 77 + cg * 151 + cb * 28) / 256f;

                        //overlay b&w
                        float or = coverlay((int)cr, (int)lumaAvg);
                        float og = coverlay((int)cg, (int)lumaAvg);
                        float ob = coverlay((int)cb, (int)lumaAvg);
                        cr = (int)((or * 0.4f) + (cr * (1f - 0.4f)));
                        cg = (int)((og * 0.4f) + (cg * (1f - 0.4f)));
                        cb = (int)((ob * 0.4f) + (cb * (1f - 0.4f)));

                        // curves filter
                        cb = cb * 0.737f + 31;

                        // color correction
                        int[] color = colorCorrection(cr, cg, cb, correction);
                        cr = color[0];
                        cg = color[1];
                        cb = color[2];

                        // photo filter
                        param = parameters["Green"];
                        cg = Util.level((int)cg, param.BlackIn, param.Gamma, param.WhiteIn, param.BlackOut, param.WhiteOut);
                        param = parameters["Blue"];
                        cb = Util.level((int)cb, param.BlackIn, param.Gamma, param.WhiteIn, param.BlackOut, param.WhiteOut);
                        param = parameters["RGB"];
                        cr = Util.level((int)cr, param.BlackIn, param.Gamma, param.WhiteIn, param.BlackOut, param.WhiteOut);
                        cg = Util.level((int)cg, param.BlackIn, param.Gamma, param.WhiteIn, param.BlackOut, param.WhiteOut);
                        cb = Util.level((int)cb, param.BlackIn, param.Gamma, param.WhiteIn, param.BlackOut, param.WhiteOut);

                        // fill soft light
                        cr = csoftLight(cr, 103);
                        cg = csoftLight(cg, 47);
                        cb = csoftLight(cb, 112);
                    }

                    image.SetPixel(i, j, Color.FromArgb((int)Util.clamp(ca, 0, 255), (int)Util.clamp(cr, 0, 255),
                        (int)Util.clamp(cg, 0, 255), (int)Util.clamp(cb, 0, 255)));
                }
            }
            return image;

        }



        public BitmapW ColorCorrection(BitmapW image, int shadowRed, int shadowGreen, int shadowBlue, 
            int midRed, int midGreen, int midBlue, int highRed, int highGreen, int highBlue)
        {
            float cr = 0, cg = 0, cb = 0, ca;
            int i = 0, j = 0,
            h = image.Height(),
            w = image.Width();
            int[][] correction = new int[3][];
            correction[0] = new int[] { shadowRed, shadowGreen, shadowBlue };
            correction[2] = new int[] { midRed, midGreen, midBlue };
            correction[3] = new int[] { highRed, highGreen, highBlue };

            for (i = 0; i < w; i++)
            {
                for (j = 0; j < h; j++)
                {
                    Color temp = image.GetPixel(i, j);
                    cr = temp.R; cg = temp.G; cb = temp.B; ca = temp.A;

                    if (ca != 0)
                    {
                        
                        int[] color = colorCorrection(cr, cg, cb, correction);
                        cr = color[0];
                        cg = color[1];
                        cb = color[2];
                    }

                    image.SetPixel(i, j, Color.FromArgb((int)Util.clamp(ca, 0, 255), (int)cr, (int)cg, (int)cb));
                }
            }
            return image;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="original"></param>
        /// <param name="correction">
        /// [ShadowRed, ShadowGreen, ShadowBlue],
        /// [MidRed,    MidGreen,    MidBlue],
        /// [HighRed,   HighGreen,   HighBlue]</param>
        /// <returns>[Red, Green, Blue]</returns>
        private int[] colorCorrection(float cr, float cg, float cb, int[][] correction)
        {
            int min = (int)Math.Min(cr, Math.Min(cg, cb));
            int max = (int)Math.Max(cr, Math.Max(cg, cb));
            float luminance = (max + min) / 512; // luminance = (max + min) / 2 et on divise le tout par 255 pour avoir une valeur entre 0 et 1
            float shadowCoefficient = 1 - luminance;
            float highlightsCoefficient = luminance;
            float midtonesCoefficient = ((float)Math.Pow((luminance - 0.5f), 2f) * -4f) + 1; // quadratique qui a les points 0,0; 0.5,1; 1,0
            float allCoefficients = shadowCoefficient + midtonesCoefficient + highlightsCoefficient;

            float redOffset = (correction[0][0] * shadowCoefficient + correction[1][0] * midtonesCoefficient + correction[2][0] * highlightsCoefficient) / allCoefficients;
            cr = (cr * (255f / (255 + redOffset)));

            float greenOffset = (correction[0][1] * shadowCoefficient + correction[1][1] * midtonesCoefficient + correction[2][1] * highlightsCoefficient) / allCoefficients;
            cg = (cg * (255f / (255 + greenOffset)));

            float blueOffset = (correction[0][2] * shadowCoefficient + correction[1][2] * midtonesCoefficient + correction[2][2] * highlightsCoefficient) / allCoefficients;
            cb = (cb * (255f / (255 + blueOffset)));

            return new int[] { (int)Util.clamp(cr, 0, 255), (int)Util.clamp(cg, 0, 255), (int)Util.clamp(cb, 0, 255) };
        }

        int coverlay(int b, int t) { return (b > 128) ? 255 - 2 * (255 - t) * (255 - b) / 255 : (b * t * 2) / 255; }
        int csoftLight(float b, float t)
        {
            b /= 255;
            t /= 255;
            return (int)((t < 0.5) ? 255 * ((1 - 2 * t) * b * b + 2 * t * b) : 255 * ((1 - (2 * t - 1)) * b + (2 * t - 1) * (Math.Pow(b, 0.5))));
        }

        /// <summary>
        /// #### Expose [-100, 100]
        /// </summary>
        /// <param name="image"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        public BitmapW expose(BitmapW image, float p)
        {

            p = Util.normalize(p, -1, 1, -100, 100);
            Point c1 = new Point(0, (int)(255 * p));
            Point c2 = new Point((int)(255 - (255 * p)), 255);
            return curves(image, new Point(0, 0), c1, c2, new Point(255, 255));
        }

        /// <summary>
        /// #### Vignette 
        /// (red,green,blue) of the vignette effect to apply
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        public BitmapW vignette(BitmapW image, int r, int g, int b)
        {
            float cr = 0, cg = 0, cb = 0, ca;
            int i = 0, j = 0,
            h = image.Height(),
            w = image.Width(),
            centerw = w / 2,
            centerh = h / 2,
            maxdist = dist(0, 0, centerw, centerh);

            
            maxdist = (int)(maxdist*0.6f);
            int radius = maxdist / 2;

            for (i = 0; i < w; i++)
            {
                for (j = 0; j < h; j++)
                {
                    Color temp = image.GetPixel(i, j);
                    cr = temp.R; cg = temp.G; cb = temp.B; ca = temp.A;
                    int distance = dist(i, j, centerw, centerh);
                    distance = (distance>radius)?distance-radius:0;

                    float ratio = ((distance / (float)(maxdist)));
                    cr = (1 - ratio) * cr + (ratio * r);
                    cg = (1 - ratio) * cg + (ratio * g);
                    cb = (1 - ratio) * cb + (ratio * b);

                    image.SetPixel(i, j, Color.FromArgb((int)Util.clamp(ca, 0, 255), (int)Util.clamp(cr, 0, 255),
                        (int)Util.clamp(cg, 0, 255), (int)Util.clamp(cb, 0, 255)));
                }
            }
            return image;
        }

        int dist(int x1, int y1, int x2, int y2)
        {
            return (int) Math.Sqrt((Math.Abs(x1-x2)*Math.Abs(x1-x2) + Math.Abs(y1-y2)*Math.Abs(y1-y2)));
            //return approx_distance(Math.Abs(x1 - x2), Math.Abs(y1 - y2));
        }

        /*FAST APPROX OF DISTANCE
        http://www.flipcode.com/archives/Fast_Approximate_Distance_Functions.shtml
        */

        int approx_distance(int dx, int dy)
        {
            int min, max, approx;

            if (dx < 0) dx = -dx;
            if (dy < 0) dy = -dy;

            if (dx < dy)
            {
                min = dx;
                max = dy;
            }
            else
            {
                min = dy;
                max = dx;
            }

            approx = (max * 1007) + (min * 441);
            if (max < (min << 4))
                approx -= (max * 40);

            // add 512 for proper rounding
            return ((approx + 512) >> 10);
        }


        /// <summary>
        /// #### Noise [0 - 100]
        /// </summary>
        /// <param name="image"></param>
        /// <param name="r"></param>
        /// <param name="g"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public BitmapW noise(BitmapW image, int p)
        {
            int adjust = (int)(p * 2.55f);
            Random rand = new Random(adjust);
            int temprand = 0;

            float cr = 0, cg = 0, cb = 0, ca;
            int i = 0, j = 0,
            h = image.Height(),
            w = image.Width();

            for (i = 0; i < w; i++)
            {
                for (j = 0; j < h; j++)
                {
                    Color temp = image.GetPixel(i, j);
                    cr = temp.R; cg = temp.G; cb = temp.B; ca = temp.A;

                    temprand = rand.Next(adjust * -1, adjust);

                    cr += temprand;
                    cg += temprand;
                    cb += temprand;

                    image.SetPixel(i, j, Color.FromArgb((int)Util.clamp(ca, 0, 255), (int)Util.clamp(cr, 0, 255),
                        (int)Util.clamp(cg, 0, 255), (int)Util.clamp(cb, 0, 255)));
                }
            }
            return image;
        }

        
    }
}
