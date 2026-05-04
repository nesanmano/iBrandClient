using System.Drawing;
using Emgu.CV;
using Emgu.CV.CvEnum;

namespace NomadLogic.Multimedia.Gpu.OpenCV.Filters
{
    internal class OpenCvFilterBoxFilter : OpenCvFilter
    {
        public OpenCvFilterBoxFilter() { }

        public OpenCvFilterBoxFilter(Size boxSize, bool normalize)
        {
            BoxSize = boxSize;
            Normalize = normalize;
        }

        public bool Normalize { get; set; }
        
        public Size BoxSize { get; set; }

        #region Overrides of OpenCvFilter

        /// <inheritdoc />
        public override uint MinChannels => 1;

        /// <inheritdoc />
        public override bool Execute(Mat inMat, Mat outMat) {
            CvInvoke.BoxFilter(inMat, outMat, DepthType.Cv8U, BoxSize, new Point(-1, -1), Normalize);
            return true;
        }

        #endregion
    }
}