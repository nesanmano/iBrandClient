using Emgu.CV;

namespace NomadLogic.Multimedia.Gpu.OpenCV.Filters
{
    internal class OpenCvFilterMedianBlur : OpenCvFilter
    {
        public OpenCvFilterMedianBlur() { }
        
        public OpenCvFilterMedianBlur(int radius)
        {
            Radius = radius;
        }
        
        public int Radius { get; set; }

        #region Overrides of OpenCvFilter

        /// <inheritdoc />
        public override uint MinChannels => 1;

        /// <inheritdoc />
        public override bool Execute(Mat inMat, Mat outMat) {
            CvInvoke.MedianBlur(inMat, outMat, Radius);
            return true;
        }

        #endregion
    }
}