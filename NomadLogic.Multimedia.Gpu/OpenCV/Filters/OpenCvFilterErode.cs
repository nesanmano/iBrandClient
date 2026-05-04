using System.Drawing;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;

namespace NomadLogic.Multimedia.Gpu.OpenCV.Filters
{
    internal class OpenCvFilterErode : OpenCvFilter
    {
        private Size _size;
        private  ElementShape _shape;
        private static readonly Point BASE_POINT = new Point(-1, -1);
        private static readonly MCvScalar BASE_SCALAR = new MCvScalar(0,0,0,0);
        private Mat _elem;

        public OpenCvFilterErode(Size size, int iteration, ElementShape shape = ElementShape.Rectangle)
        {
            BoxSize = size;
            NbrIteration = iteration;
            Shape = shape;
        }

        #region Overrides of OpenCvFilter

        /// <inheritdoc />
        public override uint MinChannels => 1;

        /// <inheritdoc />
        public override bool Execute(Mat inMat, Mat outMat) {
            if (_elem == null) _elem = CvInvoke.GetStructuringElement(Shape, BoxSize, BASE_POINT);
            CvInvoke.Erode(inMat, outMat, _elem, BASE_POINT, NbrIteration, BorderType.Reflect101, BASE_SCALAR);
            return true;
        }

        public Size BoxSize
        {
            get => _size;
            set
            {
                if (Equals(_size, value)) return;
                _size = value;
                _elem?.Dispose();
                _elem = null;
            } 
        }

        public ElementShape Shape
        {
            get => _shape;
            set
            {
                if (Equals(_shape, value)) return;
                _shape = value;
                _elem?.Dispose();
                _elem = null;
            }
        }

        public int NbrIteration { get; set; }

        #endregion
    }
}