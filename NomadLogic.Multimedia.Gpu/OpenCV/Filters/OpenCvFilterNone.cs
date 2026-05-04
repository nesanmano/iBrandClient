using Emgu.CV;

namespace NomadLogic.Multimedia.Gpu.OpenCV.Filters
{
    public class OpenCvFilterNone : OpenCvFilter
    {
        #region Overrides of OpenCvFilter

        /// <inheritdoc />
        public override uint MinChannels => 1;

        /// <inheritdoc />
        public override bool Execute(Mat inMat, Mat outMat) {
            return false;
        }

        #endregion
    }
}