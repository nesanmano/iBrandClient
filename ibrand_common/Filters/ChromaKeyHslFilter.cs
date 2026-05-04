using NomadLogic.iBrand.Common.Util.Math;

namespace NomadLogic.iBrand.Transformation.Filters
{
    public class ChromaKeyHslFilter : AbstractChromaKeyFilter
    {
        protected override byte[] FilterRgb(byte alpha, byte r, byte g, byte b)
        {
            byte[] argb = { alpha, r, g, b };

            if (argb[0] == 0) return argb; // Si pixel transparent on ne fait aucune transformation dessus

            var hsl = new float[4];
            MathUtil.RgbtoHsl(r, g, b, hsl);

            if (hsl[0] <= HigherHueInt &&
                hsl[0] >= LowerHueInt &&
                hsl[2] <= HigherLuminanceFloat &&
                hsl[2] >= LowerLuminanceFloat &&
                hsl[1] <= HigherSaturationFloat &&
                hsl[1] >= LowerSaturationFloat)
            {
                argb[0] = 0;
            }
            else
            {
                /*
                    Ici, on a déterminé que le pixel est non-transparent, mais qu'il faut légèrement modifier la couleur pour gommer le vert
                    détecté.
                */
                //argb[0] = 255;

                var couleurSimilaire = false;
                if (hsl[0] >= SemiLowerHueInt &&
                    hsl[0] <= LowerHueInt)
                {
                    hsl[0] = SemiLowerHueInt;
                    couleurSimilaire = true;

                }
                else if (hsl[0] >= HigherHueInt &&
                         hsl[0] <= SemiHigherHueInt)
                {
                    hsl[0] = SemiHigherHueInt;
                    couleurSimilaire = true;

                }
                else if (hsl[0] < HigherHueInt &&
                         hsl[0] > LowerHueInt)
                {
                    couleurSimilaire = true;
                    hsl[1] = 0;
                }

                if (couleurSimilaire)
                {
                    if (hsl[1] >= SemiLowerSaturationFloat &&
                        hsl[1] <= LowerSaturationFloat)
                    {
                        hsl[1] = SemiLowerSaturationFloat;
                    }
                    else if (hsl[1] >= HigherSaturationFloat &&
                             hsl[1] <= SemiHigherSaturationFloat)
                    {
                        hsl[1] = SemiHigherSaturationFloat;
                    }

                    if (hsl[2] >= SemiLowerLuminanceFloat &&
                        hsl[2] <= LowerLuminanceFloat)
                    {
                        hsl[2] = SemiLowerLuminanceFloat;
                    }
                    else if (hsl[2] >= HigherLuminanceFloat &&
                             hsl[2] <= SemiHigherLuminanceFloat)
                    {
                        hsl[2] = SemiHigherLuminanceFloat;
                    }
                }

                MathUtil.HsLtoRgb(hsl, argb);

            }

            return argb;
        }
    }
}
