using System.Drawing;
using NetFx;

namespace NomadLogic.iBrand.Transformation.Filters
{
    public class EarlyBirdFilter : ImageFilter
    {

        public EarlyBirdFilter() { }

        public override Bitmap Filter(Bitmap source)
        {
            BitmapW result = new BitmapW(source);
            Effects effect = new Effects();
            result = effect.EarlyBird(result);
            return result.GetBitmap();
        }

        public override void ParseXml(System.Xml.XmlTextReader reader)
        {
        }

        protected override byte[] FilterRgb(byte alpha, byte r, byte g, byte b) { return null; }
    }
}
