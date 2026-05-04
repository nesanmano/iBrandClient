using System;
using System.Drawing;
using NomadLogic.iBrand.Common.Respondent;
using NomadLogic.iBrand.Common.Util;
// ReSharper disable CompareOfFloatsByEqualityOperator

namespace NomadLogic.iBrand.Transformation.Layer
{
    public abstract class FileLayer : AbstractLayer
    {
        private int[] _scale;
        private readonly int[] _scaleOriginal;

        private int _width = -1;
        private int _height = -1;

        private readonly int _widthFromConfig = -1;
        private readonly int _heightFromConfig = -1;

        protected Bitmap _final_image;
        protected Bitmap _file_image;
        protected float _nonScaleFactor = 1f;

        protected FileLayer(int x, int y, int[] scale)
            : base(x, y)
        {

            if (scale != null)
            {
                _width = scale[0];
                _height = scale[1];
                _widthFromConfig = _width;
                _heightFromConfig = _height;

                _scale = new int[scale.Length];
                Array.Copy(scale, _scale, scale.Length);
                _scaleOriginal = new int[scale.Length];
                Array.Copy(scale, _scaleOriginal, scale.Length);
            }
        }

        protected FileLayer(FileLayer original)
            : base(original)
        {
            _scale = original._scale;
            _width = original._width;
            _height = original._height;
            _widthFromConfig = original._widthFromConfig;
            _heightFromConfig = original._heightFromConfig;
        }

        public override void ResizeParameters(float factor)
        {
            base.ResizeParameters(factor);

            if (_scale != null)
            {
                _scale[0] = (int)(_scale[0] * factor);
                _scale[1] = (int)(_scale[1] * factor);
            }
            else
            {
                _nonScaleFactor = factor;
            }

            _width = (int)(_width * factor);
            _height = (int)(_height * factor);
        }

        public abstract string GetFile(IRespondent respondent);

        public abstract void GetImage(IRespondent respondent);

        public override int GetImageWidth(IRespondent respondent)
        {
            GetBitmapFromFile(respondent);

            return _final_image?.Width ?? 0;
        }

        public override int GetImageHeight(IRespondent respondent)
        {
            GetBitmapFromFile(respondent);

            return _final_image?.Height ?? 0;
        }

        /// <summary>
        /// Retourne le Bitmap présentement utilisé par ce layer.
        /// <br/>Ne PAS détruire (Dispose) la référence donnée, car elle est cruciale au Layer. Pour correctement s'en débarraser, invoquez plutôt
        /// Dispose sur le layer directement.
        /// </summary>
        /// <returns>La référence de Bitmap du fichier présentement active.</returns>
        public Bitmap GetCurrentBitmap()
        {
            return _final_image;
        }

        public override void Draw(Bitmap image, IRespondent respondent)
        {
            GetBitmapFromFile(respondent);

            switch (_blendingMode)
            {
                case BLENDING_NORMAL:
                    var g2D = Graphics.FromImage(image);
                    g2D.DrawImageUnscaled(_final_image, X, Y, Width, Height);
                    g2D.Flush();
                    g2D.Dispose();
                    break;
                case BLENDING_MULTIPLY:
                    DrawMultiply(image, _final_image, X, Y);
                    break;
            }
        }

        /// <summary>
        /// Lit le fichier d'image et applique toute les transformations nécessaire dessus, comem redimensionnement et rotation.
        /// <br/>Ne fait rien si cette lecture a déjà été faite et aucune opération invalidante n'a été demandée.
        /// </summary>
        /// <param name="respondent">Le répondant contenant les informations sur le fichier.</param>
        public virtual void GetBitmapFromFile(IRespondent respondent)
        {
            if (_final_image != null)
            {
                return;
            }

            GetImage(respondent);
            Bitmap tmp;

            if (_final_image == null)
            {
                return;
            }

            if (_scale != null)
            {
                tmp = _final_image;
                var wValue = _scale[0];

                if (wValue <= 0)
                {
                    wValue = (int)(tmp.Width * (_scale[1] / (float)tmp.Height));
                }

                _final_image = ImageUtil.GetScaledInstance(tmp, wValue, _scale[1]);
                tmp.Dispose();
            }
            else if (_nonScaleFactor != 1f)
            {
                tmp = _final_image;
                _final_image = ImageUtil.GetScaledInstance(tmp, (int)(tmp.Width * _nonScaleFactor), (int)(tmp.Height * _nonScaleFactor));
                tmp.Dispose();
            }

            // La rotation doit être après le "scaling", car la rotation peut affecter la taille du fichier et on veut garder le même "ratio" visuel de l'image.
            if (Rotation != 0)
            {
                tmp = _final_image;
                _final_image = Rotate(tmp, Rotation);
                tmp.Dispose();
            }

            if (_width == -1)
                _width = _final_image.Width;
            if (_height == -1)
                _height = _final_image.Height;

        }

        protected void DrawMultiply(Bitmap imageA, Bitmap imageB, int x, int y)
        {

            for (var i = 0; i < imageB.Width; i++)
            {
                for (var j = 0; j < imageB.Height; j++)
                {
                    var colorImageA = imageA.GetPixel(i + x, j + y);
                    var colorImageB = imageB.GetPixel(i, j);

                    var r = ((colorImageA.R / 255f) * (colorImageB.R / 255f)) * 255;
                    var g = ((colorImageA.G / 255f) * (colorImageB.G / 255f)) * 255;
                    var b = ((colorImageA.B / 255f) * (colorImageB.B / 255f)) * 255;

                    r = r * (colorImageB.A / 255f) + colorImageA.R * ((255 - colorImageB.A) / 255f);
                    r = Math.Min(255, r);
                    r = Math.Max(0, r);
                    g = g * (colorImageB.A / 255f) + colorImageA.G * ((255 - colorImageB.A) / 255f);
                    g = Math.Min(255, g);
                    g = Math.Max(0, g);
                    b = b * (colorImageB.A / 255f) + colorImageA.B * ((255 - colorImageB.A) / 255f);
                    b = Math.Min(255, b);
                    b = Math.Max(0, b);
                    int alpha = colorImageA.A;

                    imageA.SetPixel(i + x, j + y, Color.FromArgb(alpha, (int)r, (int)g, (int)b));
                }
            }
        }

        /// <summary>
        /// La largeur, une fois lue du fichier.
        /// </summary>
        public virtual int Width
        {
            get { return _width; }
            set { _width = value; }
        }

        /// <summary>
        /// La hauteur, une fois lue du fichier.
        /// </summary>
        public virtual int Height
        {
            get { return _height; }
            set { _height = value; }
        }

        public virtual int[] Scale
        {
            get { return _scale; }
            set
            {
                _scale = value;
                if (EnableSettingChangeEventOnScaleChange)
                    SettingsHaveChanged();
            }
        }

        protected bool EnableSettingChangeEventOnScaleChange { get; set; } = true;

        protected override void OnLayerSettingsChanged()
        {
            if (_final_image != null)
            {
                _final_image.Dispose();
                _final_image = null;
            }
        }

        public override void Dispose()
        {
            if (_final_image != null)
            {
                _final_image.Dispose();
                _final_image = null;
            }

            if (_file_image != null)
            {
                _file_image.Dispose();
                _file_image = null;
            }
        }

        public override void ResetValues()
        {
            base.ResetValues();
            _height = _heightFromConfig;
            _width = _widthFromConfig;
            if (_scaleOriginal != null && _scale != null)
                Array.Copy(_scaleOriginal, _scale, _scaleOriginal.Length);
            _nonScaleFactor = 1f;
        }
    }
}
