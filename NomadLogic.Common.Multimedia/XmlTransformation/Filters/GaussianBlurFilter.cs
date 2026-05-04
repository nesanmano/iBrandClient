using System.Drawing;
using NetFx;

namespace NomadLogic.Multimedia.XmlTransformation.Filters
{
    internal class GaussianBlurFilter : ImageFilter
    {

        public GaussianBlurFilter() { }

        public override Bitmap Filter(Bitmap source)
        {
            var result = new BitmapW(source);
            var effect = new Effects();
            result = effect.blur(result, "Gaussian");
            return result.GetBitmap();
        }

        protected override byte[] FilterRgb(byte alpha, byte r, byte g, byte b) { return null; }

        public override void ParseXml(System.Xml.XmlTextReader reader)
        {
        }
    }
}
