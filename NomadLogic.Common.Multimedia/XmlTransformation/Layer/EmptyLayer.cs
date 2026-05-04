using System.Drawing;
using System.Xml;
using NomadLogic.Common.Respondent;

namespace NomadLogic.Multimedia.XmlTransformation.Layer
{
    class EmptyLayer : AbstractLayer
    {
        private readonly int _wOriginal;
        private readonly int _hOriginal;

        public EmptyLayer(int x, int y)
            : base(x, y)
        {
        }

        public EmptyLayer(int x, int y, XmlReader reader)
            : this(x, y)
        {
            var w = reader.GetAttribute("width");
            if (w != null)
                Width = int.Parse(w);

            var h = reader.GetAttribute("height");
            if (h != null)
                Height = int.Parse(h);

            _wOriginal = Width;
            _hOriginal = Height;
        }

        public EmptyLayer(EmptyLayer original)
            : base(original)
        {
            Width = original.Width;
            Height = original.Height;

            _wOriginal = original._wOriginal;
            _hOriginal = original._hOriginal;
        }

        public override void ResizeParameters(float factor)
        {
            base.ResizeParameters(factor);
            Width = (int)(Width * factor);
            Height = (int)(Height * factor);
        }

        public int Width { get; set; }

        public int Height { get; set; }

        public override AbstractLayer GetCopy()
        {
            return new EmptyLayer(this);
        }

        public override int GetImageWidth(IRespondent respondent)
        {
            return Width;
        }

        public override int GetImageHeight(IRespondent respondent)
        {
            return Height;
        }

        public override void Draw(Bitmap image, IRespondent respondent)
        {
            // Rien. Vide. Nada.
        }

        protected override void OnLayerSettingsChanged()
        {
            // Rien
        }

        public override void Dispose()
        {
            // Rien à détruire.
        }

        public override void ResetValues()
        {
            base.ResetValues();
            Width = _wOriginal;
            Height = _hOriginal;
        }
    }
}
