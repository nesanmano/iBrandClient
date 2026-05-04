using NLog;
using NomadLogic.iBrand.Common.Util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Xml;

namespace NomadLogic.iBrand.Transformation.Filters
{
    public class EdgeBlurFilter : ImageFilter
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        public EdgeBlurFilter(int innerRadius, int outerRadius)
        {
            InnerRadius = innerRadius;
            OuterRadius = outerRadius;
        }

        public EdgeBlurFilter()
        {
            InnerRadius = 2;
            OuterRadius = 4;
        }

        protected override byte[] FilterRgb(byte alpha, byte r, byte g, byte b)
        {
            //Rien
            return null;
        }

        public override void ParseXml(XmlTextReader reader)
        {
            //rien
        }

        public override Bitmap Filter(Bitmap source)
        {
            if (source == null || source.PixelFormat != PixelFormat.Format32bppArgb) {
                return null;
            }

            var finale = new Bitmap(source.Width, source.Height, PixelFormat.Format32bppArgb);
            finale.SetResolution(300, 300);
            var rect = new Rectangle(0, 0, source.Width, source.Height);
            var bmData = source.LockBits(rect, ImageLockMode.ReadOnly,
                source.PixelFormat);
            var bmDataDest = finale.LockBits(rect, ImageLockMode.WriteOnly,
                PixelFormat.Format32bppArgb);

            // Stride peut être négatif (vertical-row order)
            var stride = Math.Abs(bmData.Stride);

            var scan0 = bmData.Scan0;
            var scan0Dest = bmDataDest.Scan0;

            var bytes = Math.Abs(bmData.Stride) * source.Height;
            var bytesDest = Math.Abs(bmDataDest.Stride) * source.Height;

            var rgbValues = new byte[bytes];
            var rgbValuesDest = new byte[bytesDest];

            System.Runtime.InteropServices.Marshal.Copy(scan0, rgbValues, 0, bytes);

            // Boucle principale
            for (var i = 0; i < rgbValues.Length; i += 4)
            {
                var alpha = rgbValues[i + 3];

                var modAlpha = alpha;

                if (alpha == 255) 
                {
                    int[] usedIndex;
                    var inner = GetInnerCircle(out usedIndex, rgbValues, i, stride, InnerRadius);

                    var innerCalc = CalcPlot(inner);

                    if (innerCalc < 255 * inner.Length)
                    {
                        var outer = GetOuterCircle(usedIndex, rgbValues, i, stride, OuterRadius);
                        var outerCalc = CalcPlot(outer);

                        outerCalc += innerCalc;

                        modAlpha = ImageUtil.ColorClamp((int)(outerCalc / ((outer.Length + inner.Length) * 255f) * 255));
                    }
                }

                rgbValuesDest[i + 3] = modAlpha; // A
                rgbValuesDest[i + 2] = rgbValues[i + 2]; // R
                rgbValuesDest[i + 1] = rgbValues[i + 1]; // G
                rgbValuesDest[i] = rgbValues[i]; // B
            }

            System.Runtime.InteropServices.Marshal.Copy(rgbValuesDest, 0, scan0Dest, bytesDest);

            // Unlock the bits.
            source.UnlockBits(bmData);
            finale.UnlockBits(bmDataDest);

            return finale;
        }

        private static byte[] GetOuterCircle(int[] usedIndex, byte[] rgbValues, int i, int stride, int radius)
        {
            //line position of i
            var linePos = i < stride ? i / 4 : i % stride / 4;
            var mH = Math.Max(0, linePos - radius);
            var xH = Math.Min(stride / 4 - 1, linePos + radius);
            var mV = Math.Max(linePos * 4, i - radius * stride);
            var xV = Math.Min(rgbValues.Length - stride + linePos * 4, i + radius * stride);

            var retour = new byte[(1 + (xH - mH)) * ((xV - mV) / stride + 1) - usedIndex.Length];

            var nbrPixelRead = 0;
            var usedRead = 0;
            var colonne = 0;
            var ligne = 0;
            var index = 0;
            try
            {
                for (colonne = mV; colonne <= xV; colonne += stride) // boucle colonne
                {
                    for (ligne = mH; ligne <= xH; ligne++) //  boucle ligne
                    {
                        index = colonne + (ligne - linePos) * 4;
                        if (usedRead < usedIndex.Length && index <= usedIndex[usedRead])
                        {
                            usedRead++;
                        }
                        else
                        {
                            retour[nbrPixelRead] = rgbValues[index + 3]; // On met la valeur alpha dans le tableau
                            nbrPixelRead++;
                        }
                    }
                }
            }
            catch (System.Exception e)
            {
                var sb = new StringBuilder();
                sb.Append("Loop values:");
                sb.Append("\n\t-radius = "); sb.Append(radius);
                sb.Append("\n\t-stride = "); sb.Append(stride);
                sb.Append("\n\t-mH = "); sb.Append(mH);
                sb.Append("\n\t-xH = "); sb.Append(xH);
                sb.Append("\n\t-mV = "); sb.Append(mV);
                sb.Append("\n\t-xV = "); sb.Append(xV);
                sb.Append("\n\t-nbrPixelRead = "); sb.Append(nbrPixelRead);
                sb.Append("\n\t-colonne = "); sb.Append(colonne);
                sb.Append("\n\t-ligne = "); sb.Append(ligne);
                sb.Append("\n\t-i = "); sb.Append(i);
                sb.Append("\n\t-index = "); sb.Append(index);
                sb.Append("\n\t-usedIndex = "); sb.Append(String.Join(", ", usedIndex));
                sb.Append("\n\t-retour = "); sb.Append(String.Join(", ", retour));

                log.Error(e, sb.ToString());
                throw;
            }

            return retour;
        }

        private static byte[] GetInnerCircle(out int[] usedIndex, byte[] rgbValues, int i, int stride, int radius)
        {
            //line position of i
            var linePos = i < stride ? i/4 : i % stride/4;
            var mH = Math.Max(0, linePos - radius);
            var xH = Math.Min(stride / 4 - 1, linePos + radius);
            var mV = Math.Max(linePos*4, i - radius * stride);
            var xV = Math.Min(rgbValues.Length - stride + linePos * 4, i + radius * stride);

            var retour = new byte[(1 + (xH-mH)) * ((xV - mV) / stride + 1)];
            usedIndex = new int[retour.Length];
            var index = 0;
            var ligne = 0;
            var colonne = 0;
            var nbrPixelRead = 0;

            try
            {    
                for (colonne = mV; colonne <= xV; colonne += stride) // boucle colonne
                {
                    for (ligne = mH; ligne <= xH; ligne++) //  boucle ligne
                    {
                        index = colonne + (ligne - linePos) * 4;
                        usedIndex[nbrPixelRead] = index;
                        retour[nbrPixelRead] = rgbValues[index + 3]; // On met la valeur alpha dans le tableau
                        nbrPixelRead++;
                    }
                }
            } catch(System.Exception e) {
                var sb = new StringBuilder();
                sb.Append("Loop values:");
                sb.Append("\n\t-radius = "); sb.Append(radius);
                sb.Append("\n\t-stride = "); sb.Append(stride);
                sb.Append("\n\t-mH = "); sb.Append(mH);
                sb.Append("\n\t-xH = "); sb.Append(xH);
                sb.Append("\n\t-mV = "); sb.Append(mV);
                sb.Append("\n\t-xV = "); sb.Append(xV);
                sb.Append("\n\t-nbrPixelRead = "); sb.Append(nbrPixelRead);
                sb.Append("\n\t-colonne = "); sb.Append(colonne);
                sb.Append("\n\t-ligne = "); sb.Append(ligne);
                sb.Append("\n\t-i = "); sb.Append(i);
                sb.Append("\n\t-index = "); sb.Append(index);
                sb.Append("\n\t-usedIndex = "); sb.Append(String.Join(", ", usedIndex));
                sb.Append("\n\t-retour = "); sb.Append(String.Join(", ", retour));

                log.Error(e, sb.ToString());
                throw;
            }

            return retour;
        }

        private static long CalcPlot(IEnumerable<byte> inner)
        {
            return inner.Aggregate<byte, long>(0, (current, b) => current + b);
        }

        public int InnerRadius { get; set; }

        public int OuterRadius { get; set; }
    }
}
