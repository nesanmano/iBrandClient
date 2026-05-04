using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Imaging;
using System.Xml;
using JetBrains.Annotations;
using NomadLogic.Common.Util.Math;

namespace NomadLogic.Multimedia.XmlTransformation.Filters
{
    [PublicAPI]
    public abstract class ImageFilter
    {
        protected Bitmap _mask;
        protected float _maskXOffset;
        protected float _maskYOffset;

        [SuppressMessage("ReSharper", "UnusedVariable")]
        public virtual Bitmap Filter(Bitmap source)
        {
            var finale = new Bitmap(source.Width, source.Height, PixelFormat.Format32bppArgb);
            finale.SetResolution(300, 300);
            var rect = new Rectangle(0, 0, source.Width, source.Height);
            var bmData = source.LockBits(rect, ImageLockMode.ReadOnly,
                source.PixelFormat);
            var bmDataDest = finale.LockBits(rect, ImageLockMode.WriteOnly,
                PixelFormat.Format32bppArgb);

            var stride = bmData.Stride;
            var strideDest = bmDataDest.Stride;
            var scan0 = bmData.Scan0;
            var scan0Dest = bmDataDest.Scan0;

            var pixelLenght = source.PixelFormat == PixelFormat.Format24bppRgb ? 3 : 4;

            var bytes = Math.Abs(bmData.Stride) * source.Height;
            var bytesDest = Math.Abs(bmDataDest.Stride) * source.Height;

            var rgbValues = new byte[bytes];
            var rgbValuesDest = new byte[bytesDest];

            System.Runtime.InteropServices.Marshal.Copy(scan0, rgbValues, 0, bytes);

            for (var i = 0; i < rgbValues.Length; i += pixelLenght)
            {
                var blue = rgbValues[i];
                var green = rgbValues[i + 1];
                var red = rgbValues[i + 2];

                var alpha = pixelLenght == 3 ? (byte) 255 : rgbValues[i + 3];

                var color = FilterRgbPosition(alpha, red, green, blue, (i % stride) / pixelLenght, i / stride, source.Width, source.Height);

                var destPosition = (i / pixelLenght) * 4;
                rgbValuesDest[destPosition + 3] = color[0];
                rgbValuesDest[destPosition + 2] = color[1];
                rgbValuesDest[destPosition + 1] = color[2];
                rgbValuesDest[destPosition] = color[3];
            }

            System.Runtime.InteropServices.Marshal.Copy(rgbValuesDest, 0, scan0Dest, bytesDest);

            // Unlock the bits.
            source.UnlockBits(bmData);
            finale.UnlockBits(bmDataDest);

            return finale;
        }

        protected byte[] FilterRgbPosition(byte alpha, byte r, byte g, byte b, int x, int y, int originalWidth, int originalHeight)
        {
            var alphaNewPixel = 1f;
            var alphaOldPixel = 0f;

            if (_mask != null)
            {
                var mapX = (int)(((x + 0f) / originalWidth) * _mask.Width);
                var mapY = (int)(((y + 0f) / originalHeight) * _mask.Height);
                var mapColor = _mask.GetPixel(mapX, mapY);

                alphaNewPixel = mapColor.R / 255f;
                alphaOldPixel = 1f - alphaNewPixel;
            }

            var newPixel = FilterRgb(alpha, r, g, b);

            var newA = MathUtil.ColorClamp((int)(alpha * alphaOldPixel + newPixel[0] * alphaNewPixel));
            var newR = MathUtil.ColorClamp((int)(r * alphaOldPixel + newPixel[1] * alphaNewPixel));
            var newG = MathUtil.ColorClamp((int)(g * alphaOldPixel + newPixel[2] * alphaNewPixel));
            var newB = MathUtil.ColorClamp((int)(b * alphaOldPixel + newPixel[3] * alphaNewPixel));

            byte[] finalPixel = { newA, newR, newG, newB };

            return finalPixel;
        }

        public Bitmap Mask
        {
            get => _mask;
            set => _mask = value;
        }

        protected abstract byte[] FilterRgb(byte alpha, byte r, byte g, byte b);
        public abstract void ParseXml(XmlTextReader reader);
    }
}
