using System;
using System.Drawing;
using System.Drawing.Imaging;
using NomadLogic.iBrand.Common.Respondent;
using NomadLogic.iBrand.Transformation.image;

namespace NomadLogic.iBrand.Transformation.Layer
{
    public abstract class AbstractLayer : IDisposable, IResetToOriginalValues
    {
        public const int BLENDING_NORMAL = 1;
        public const int BLENDING_MULTIPLY = 2;

        protected delegate void LayerSettingsChangedHandler();
        protected event LayerSettingsChangedHandler LayerSettingsChanged;

        private int? _rotation;
        private int _rotationOriginal;

        private int _x;
        private int _y;

        private readonly int _xFromConfig;
        private readonly int _yFromConfig;

        protected int _blendingMode = BLENDING_NORMAL;

        protected AbstractLayer(int x, int y)
        {
            _x = x;
            _y = y;

            _xFromConfig = x;
            _yFromConfig = y;

            LayerSettingsChanged += OnLayerSettingsChanged;
        }

        protected AbstractLayer(AbstractLayer original)
        {
            _x = original._x;
            _y = original._y;
            _xFromConfig = original._xFromConfig;
            _yFromConfig = original._yFromConfig;

            LayerSettingsChanged += OnLayerSettingsChanged;
        }

        protected abstract void OnLayerSettingsChanged();

        public int QuestionNo { get; set; } = -1;

        public int ChoiceNo { get; set; } = -1;

        public int Rotation
        {
            get {
                return _rotation ?? 0;
            }
            set
            {
                if (!_rotation.HasValue)
                {
                    _rotationOriginal = value;
                }

                _rotation = value;
                SettingsHaveChanged();
            }
        }

        public virtual int X
        {
            get { return _x; }
            set { _x = value; SettingsHaveChanged(); }
        }

        public virtual int Y
        {
            get { return _y; }
            set { _y = value; SettingsHaveChanged(); }
        }

        public abstract int GetImageWidth(IRespondent respondent);
        public abstract int GetImageHeight(IRespondent respondent);

        public abstract void Draw(Bitmap image, IRespondent respondent);

        protected static Bitmap Rotate(Bitmap image, int rotation)
        {
            float angle = rotation % 360;
            if (angle < -180f)
            {
                angle += 360f;
            }
            else if (angle > 180f)
            {
                angle -= 360f;
            }

            const PixelFormat pf = PixelFormat.Format32bppArgb;

            var sin = (float)Math.Abs(Math.Sin(angle * Math.PI / 180.0)); // this function takes radians
            var cos = (float)Math.Abs(Math.Cos(angle * Math.PI / 180.0)); // this one too
            var newImgWidth = sin * image.Height + cos * image.Width;
            var newImgHeight = sin * image.Width + cos * image.Height;
            var originX = 0f;
            var originY = 0f;

            if (angle > 0)
            {
                if (angle <= 90)
                    originX = sin * image.Height;
                else
                {
                    originX = newImgWidth;
                    originY = newImgHeight - sin * image.Width;
                }
            }
            else
            {
                if (angle >= -90)
                    originY = sin * image.Width;
                else
                {
                    originX = newImgWidth - sin * image.Height;
                    originY = newImgHeight;
                }
            }

            var rotatedImage = new Bitmap((int)newImgWidth, (int)newImgHeight, pf);
            rotatedImage.SetResolution(300, 300);
            var g = Graphics.FromImage(rotatedImage);
            g.TranslateTransform(originX, originY); // offset the origin to our calculated values
            g.RotateTransform(angle); // Set up rotate
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBilinear;
            g.DrawImageUnscaled(image, 0, 0); // draw the image at 0, 0
            g.Dispose();

            return rotatedImage;
        }

        /// <summary>
        /// Génère une nouvelle instance disjointe; une copie profonde.
        /// </summary>
        /// <returns>Une nouvelle instance égale mais disjointe.</returns>
        public abstract AbstractLayer GetCopy();

        public abstract void Dispose();

        public virtual void ResetValues()
        {
            Dispose();
            _x = _xFromConfig;
            _y = _yFromConfig;
            _rotation = _rotationOriginal;
        }

        ~AbstractLayer()
        {
            Dispose();
        }

        protected virtual void SettingsHaveChanged()
        {
            var handler = LayerSettingsChanged;
            handler?.Invoke();
        }

        /// <summary>
        /// Modifie tous les paramètres de cette transformation par un facteur réel.
        /// </summary>
        /// <param name="factor">Le facteur multiplicatif de la transformation.</param>
        public virtual void ResizeParameters(float factor)
        {
            _x = (int)(_x * factor);
            _y = (int)(_y * factor);
        }
    }
}
