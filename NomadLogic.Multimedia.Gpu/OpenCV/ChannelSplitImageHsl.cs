using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using JetBrains.Annotations;

namespace NomadLogic.Multimedia.Gpu.OpenCV
{
    /// <summary>
    /// Extension d'une image à canaux séparés pour utiliser des matrices de couleur basé sur le code couleur HSL, ou "HLS",
    /// pour illustrer l'ordre des matrices des canaux dans OpenCV.
    /// </summary>
    [PublicAPI]
    public class ChannelSplitImageHsl : ChannelSplitImageRgb
    {
        protected override void FormatConversion()
        {
            /* Transformation vers l'espace de couleur HSL (en format HLS) */
            var tmp = new Mat();
            var tmp2 = new Mat();
            BaseImageBgr.ConvertTo(tmp, DepthType.Cv32F, OpenCvUtil.BYTE_TO_FLOAT_RATIO);
            CvInvoke.CvtColor(tmp, tmp2, ColorConversion.Bgr2Hls);

            WorkingHlsData = tmp2.ToImage<Hls, float>();

            if (WorkingAlpha != null) return;
            var alpha = new Matrix<byte>(tmp.Rows, tmp2.Cols);
            alpha.SetValue(byte.MaxValue);
            WorkingAlpha = alpha.Bytes;
        }

        /// <summary>
        /// Modifie les valeurs des matrices RGB pour représenter la version RGB des pixels contenus dans la propriété "WorkingHlsData"
        /// </summary>
        public void UpdateRgbValuesFromWorkingHls()
        {
            // Convertit maintenant les valeurs de la matrice de base aux nouvelle valeurs.
            using (var tmp2 = new Mat())
            {
                var tmp = new Mat();
                CvInvoke.CvtColor(WorkingHlsData.Mat, tmp2, ColorConversion.Hls2Bgr);
                tmp2.ConvertTo(tmp, DepthType.Cv8U);

                GenerateRgbMats(tmp, true);
            }
        }
        
        /// <summary>
        /// Une référence d'image OpenCv vers les canaux de couleur précalculés en format HLS.
        /// Selon la documentation, l'espace de valeurs pour ces matrices est de:
        /// H -> [0..360] : Qui sont des degrés. Ce qui signifit que quand on dépasse 360, on boucle via zéro.
        /// L -> [0..1]
        /// S -> [0..1]
        /// Voir: https://docs.opencv.org/3.0-beta/modules/imgproc/doc/miscellaneous_transformations.html?highlight=cvtcolor
        /// </summary>
        public Image<Hls, float> WorkingHlsData { get; private set; }

        public override void Dispose()
        {
            base.Dispose();
            WorkingHlsData?.Dispose();
            WorkingHlsData = null;
        }
    }
}