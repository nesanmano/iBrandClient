using System.Drawing;
using NetFx;

namespace NomadLogic.iBrand.Transformation.Filters
{
    class SunsetFilter : ImageFilter
    {

        public SunsetFilter() { }

        public override Bitmap Filter(Bitmap source)
        {
            BitmapW result = new BitmapW(source);
            Effects effect = new Effects();
            result = effect.Sunset(result);
            return result.GetBitmap();
        }

        protected override byte[] FilterRgb(byte alpha, byte r, byte g, byte b) { return null; }

        public override void ParseXml(System.Xml.XmlTextReader reader)
        {
        }
    }
}
