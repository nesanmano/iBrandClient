using System;
using System.Drawing;
using JetBrains.Annotations;
using NomadLogic.Common.Extentions;
using NomadLogic.Common.Respondent;

// ReSharper disable CompareOfFloatsByEqualityOperator

namespace NomadLogic.Multimedia.XmlTransformation.Layer
{
    [PublicAPI]
    public abstract class FileLayer : AbstractLayer
    {
        private int[] _scale;
        private readonly int[] _scaleOriginal;

        private readonly int _widthFromConfig = -1;
        private readonly int _heightFromConfig = -1;

        protected Bitmap _finalImage;
        protected Bitmap _fileImage;
        protected float _nonScaleFactor = 1f;

        protected FileLayer(int x, int y, int[] scale)
            : base(x, y)
        {

            if (scale != null)
            {
                Width = scale[0];
                Height = scale[1];
                _widthFromConfig = Width;
                _heightFromConfig = Height;

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
            Width = original.Width;
            Height = original.Height;
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

            Width = (int)(Width * factor);
            Height = (int)(Height * factor);
        }

        public abstract string GetFile(IRespondent respondent);

        public abstract void GetImage(IRespondent respondent);

        public override int GetImageWidth(IRespondent respondent)
        {
            GetBitmapFromFile(respondent);

            return _finalImage?.Width ?? 0;
        }

        public override int GetImageHeight(IRespondent respondent)
        {
            GetBitmapFromFile(respondent);

            return _finalImage?.Height ?? 0;
        }

        /// <summary>
        /// Retourne le Bitmap présentement utilisé par ce layer.
        /// <br/>Ne PAS détruire (Dispose) la référence donnée, car elle est cruciale au Layer. Pour correctement s'en débarraser, invoquez plutôt
        /// Dispose sur le layer directement.
        /// </summary>
        /// <returns>La référence de Bitmap du fichier présentement active.</returns>
        public Bitmap GetCurrentBitmap()
        {
            return _finalImage;
        }

        public override void Draw(Bitmap image, IRespondent respondent)
        {
            GetBitmapFromFile(respondent);

            switch (_blendingMode)
            {
                case BLENDING_NORMAL:
                    var g2D = Graphics.FromImage(image);
                    g2D.DrawImageUnscaled(_finalImage, X, Y, Width, Height);
                    g2D.Flush();
                    g2D.Dispose();
                    break;
                case BLENDING_MULTIPLY:
                    DrawMultiply(image, _finalImage, X, Y);
                    break;
            }
        }

        /// <summary>
        /// Lit le fichier d'image et applique toute les transformations nécessaire dessus, comem redimensionnement et rotation.
        /// <br/>Ne fait rien si cette lecture a déjà été faite et aucune opération invalidante n'a été demandée.
        /// </summary>
        /// <param name="respondent">Le répondant contenant les informations sur le fichier.</param>
        public void GetBitmapFromFile(IRespondent respondent)
        {
            if (_finalImage != null)
            {
                return;
            }

            GetImage(respondent);
            Bitmap tmp;

            if (_finalImage == null)
            {
                return;
            }

            if (_scale != null)
            {
                tmp = _finalImage;
                var wValue = _scale[0];

                if (wValue <= 0)
                {
                    wValue = (int)(tmp.Width * (_scale[1] / (float)tmp.Height));
                }

                _finalImage = tmp.Resize(wValue, _scale[1]);
                tmp.Dispose();
            }
            else if (_nonScaleFactor != 1f)
            {
                tmp = _finalImage;
                _finalImage = tmp.Scale((int)(tmp.Width * _nonScaleFactor), (int)(tmp.Height * _nonScaleFactor));
                tmp.Dispose();
            }

            // La rotation doit être après le "scaling", car la rotation peut affecter la taille du fichier et on veut garder le même "ratio" visuel de l'image.
            if (Rotation != 0)
            {
                tmp = _finalImage;
                _finalImage = Rotate(tmp, Rotation);
                tmp.Dispose();
            }

            if (Width == -1)
                Width = _finalImage.Width;
            if (Height == -1)
                Height = _finalImage.Height;

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
        public int Width { get; set; } = -1;

        /// <summary>
        /// La hauteur, une fois lue du fichier.
        /// </summary>
        public int Height { get; set; } = -1;

        public int[] Scale
        {
            get => _scale;
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
            if (_finalImage == null) return;
            _finalImage.Dispose();
            _finalImage = null;
        }

        public override void Dispose()
        {
            if (_finalImage != null)
            {
                _finalImage.Dispose();
                _finalImage = null;
            }

            if (_fileImage == null) return;
            _fileImage.Dispose();
            _fileImage = null;
        }

        public override void ResetValues()
        {
            base.ResetValues();
            Height = _heightFromConfig;
            Width = _widthFromConfig;
            if (_scaleOriginal != null && _scale != null)
                Array.Copy(_scaleOriginal, _scale, _scaleOriginal.Length);
            _nonScaleFactor = 1f;
        }
    }
}
