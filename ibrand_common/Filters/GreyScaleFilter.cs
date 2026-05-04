using System.Drawing;
using NetFx;

namespace NomadLogic.iBrand.Transformation.Filters
{
    public class GreyScaleFilter : ImageFilter
    {
        public GreyScaleFilter() { }

        public override Bitmap Filter(Bitmap source)
        {
            var result = new BitmapW(source);
            var effect = new Effects();
            result = effect.GreyScale(result);
            return result.GetBitmap();
        }

        protected override byte[] FilterRgb(byte alpha, byte r, byte g, byte b) { return null; }

        public override void ParseXml(System.Xml.XmlTextReader reader)
        {
        }
    }
}
