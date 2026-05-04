#region License and copyright notice
/*
 * Ported to .NET for use in Kaliko.ImageLibrary by Fredrik Schultz 2014
 *
 * Original License:
 * Copyright 2006 Jerry Huxtable
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 * http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
*/
#endregion

namespace Kaliko.ImageLibrary.Filters {
    using System;
    using System.Drawing;
    using ColorSpace;

    public class ChromaKeyFilter : IFilter {
        public float ToleranceHue { get; set; }

        public float ToleranceSaturnation { get; set; }

        public float ToleranceBrightness { get; set; }

        public Color KeyColor { get; set; }

        public ChromaKeyFilter() {
            KeyColor = Color.FromArgb(0, 255, 0);
            ToleranceHue = 10;
            ToleranceSaturnation = 0.7f;
            ToleranceBrightness = 0.5f;
        }

        public ChromaKeyFilter(Color keyColor) {
            KeyColor = keyColor;
            ToleranceHue = 10;
            ToleranceSaturnation = 0.7f;
            ToleranceBrightness = 0.5f;
        }

        public ChromaKeyFilter(Color keyColor, float toleranceHue, float toleranceSaturnation, float toleranceBrightness) {
            KeyColor = keyColor;
            ToleranceHue = toleranceHue;
            ToleranceSaturnation = toleranceSaturnation;
            ToleranceBrightness = toleranceBrightness;
        }

        public void Run(KalikoImage image) {
            ValidateParameters();

            ApplyChromaKey(image);
        }

        public void ApplyChromaKey(KalikoImage image) {
            var pixels = image.IntArray;
            var keyHsb = ColorSpaceHelper.RGBtoHSB(KeyColor);

            bool in_green = false;
            bool in_participant = false;

            GaussianBlurFilter blur_filter = new GaussianBlurFilter();

            int in_green_since = 0;

            for (int i = 0; i < pixels.Length; i++) {
                int rgb = pixels[i];

                int red = (rgb >> 16) & 0xff;
                int green = (rgb >> 8) & 0xff;
                int blue = rgb & 0xff;
                HSB hsb = ColorSpaceHelper.RGBtoHSB(red, green, blue);

                if (Math.Abs(hsb.Hue - keyHsb.Hue) < ToleranceHue && Math.Abs(hsb.Saturation - keyHsb.Saturation) < ToleranceSaturnation && Math.Abs(hsb.Brightness - keyHsb.Brightness) < ToleranceBrightness) {
                    pixels[i] = rgb & 0xffffff;
                    in_green = true;
                    if (in_participant)
                    {
                        //wait to remove green before blur
                        in_participant = false;
                        in_green_since = 0;
                                     
                    }

                    in_green_since++;        

                    if (in_green_since == 10)
                    {
                        int lenght = 20;
                        int sourceIndex = i - 20;
                        if (i < 20)
                        {
                            lenght = i;
                            sourceIndex = 0;
                        }

                        int[] inPixels = new int[lenght];
                        int[] outPixels = new int[lenght];

                        Array.Copy(pixels, sourceIndex, inPixels, 0, lenght);

                        blur_filter.Test(blur_filter.Kernel, inPixels, outPixels, lenght, 1, blur_filter.UseAlpha, blur_filter.UseAlpha && blur_filter.PremultiplyAlpha, false, Kaliko.ImageLibrary.Filters.ConvolveFilter.EdgeMode.Clamp);

                        Array.Copy(inPixels, 0, pixels, sourceIndex, lenght);
                    }

                }
                else {
                    pixels[i] = rgb;
                    in_participant = true;
                    if (in_green)
                    {
                        int lenght = 20;
                        int sourceIndex = i - 10;
                        if (i < 10)
                        {
                            lenght = i + 10;
                            sourceIndex = 0;
                        }

                        int[] inPixels = new int[lenght];
                        int[] outPixels = new int[lenght];

                        if (sourceIndex + lenght > pixels.Length)
                            lenght = pixels.Length - sourceIndex;
                        Array.Copy(pixels, sourceIndex, inPixels, 0, lenght);

                        blur_filter.Test(blur_filter.Kernel, inPixels, outPixels, lenght, 1, blur_filter.UseAlpha, blur_filter.UseAlpha && blur_filter.PremultiplyAlpha, false, Kaliko.ImageLibrary.Filters.ConvolveFilter.EdgeMode.Clamp);

                        Array.Copy(inPixels, 0, pixels, sourceIndex, lenght);

                        in_green = false;
                        i += 10;
                    }
                }
            }

            image.IntArray = pixels;
        }

        private void ValidateParameters() {
            if (ToleranceHue < 0 || ToleranceHue > 360) {
                throw new ArgumentException("ToleranceHue out of range (0..360)");
            }
            if (ToleranceSaturnation < 0 || ToleranceSaturnation > 1) {
                throw new ArgumentException("ToleranceSaturnation out of range (0..1)");
            }
            if (ToleranceBrightness < 0 || ToleranceBrightness > 1) {
                throw new ArgumentException("ToleranceBrightness out of range (0..1)");
            }
        }
    }
}
