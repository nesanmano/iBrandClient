using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using JetBrains.Annotations;

namespace NomadLogic.Multimedia.Gpu.OpenCV
{
    /// <summary>
    /// Extension d'une image à canaux séparés pour utiliser des matrices de couleur basé sur le code couleur CIE L*U*V.
    /// </summary>
    [PublicAPI]
    public class ChannelSplitImageLuv : ChannelSplitImageRgb
    {
        /*
        public ChannelSplitImageLuv(Mat baseImage, Size? newSize = null) : base(baseImage, newSize) { }
        public ChannelSplitImageLuv(CvArray<byte> image, Size? newSize = null) : base(image, newSize) { }
        */
        
        protected override void FormatConversion()
        {
            /* Transformation vers l'espace de couleur LUV */
            var tmp = new Mat();
            var tmp2 = new Mat();
            BaseImageBgr.ConvertTo(tmp, DepthType.Cv32F, OpenCvUtil.BYTE_TO_FLOAT_RATIO);
            CvInvoke.CvtColor(tmp, tmp2, ColorConversion.Bgr2Luv);
            WorkingLuvData = tmp2.ToImage<Luv, float>();
        }
        
        /// <summary>
        /// Une référence d'image OpenCv vers les canaux de couleur précalculés en format CIE L*u*v*.
        /// </summary>
        public Image<Luv, float> WorkingLuvData { get; private set; }

        public override void Dispose()
        {
            base.Dispose();
            WorkingLuvData?.Dispose();
            WorkingLuvData = null;
        }
    }
}