using System.Drawing;
using NetFx;

namespace NomadLogic.Multimedia.XmlTransformation.Filters
{
    public class EarlyBirdFilter : ImageFilter
    {

        public EarlyBirdFilter() { }

        public override Bitmap Filter(Bitmap source)
        {
            var result = new BitmapW(source);
            var effect = new Effects();
            result = effect.EarlyBird(result);
            return result.GetBitmap();
        }

        public override void ParseXml(System.Xml.XmlTextReader reader)
        {
        }

        protected override byte[] FilterRgb(byte alpha, byte r, byte g, byte b) { return null; }
    }
}
