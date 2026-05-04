using System.Collections.Generic;

namespace NomadLogic.Multimedia.FaceDetection.Harr.Hid
{
    public class HidHaarClassifier
    {
        public int Count { get; set; }

        public List<HidHaarTreeNode> Node { get; set; }

        public List<float> Alpha { get; set; }

        public double Evaluate(double varianceNormalizationFactor, HaarPoint pointOffset)
        {
            var index = 0;
            do
            {
                var node = Node[index];
                var t = node.Threshold * varianceNormalizationFactor;

                double sum = node.Feature.Rectangles[0].CalculateSum(pointOffset) *
                    node.Feature.Rectangles[0].Weight;
                sum += node.Feature.Rectangles[1].CalculateSum(pointOffset) * node.Feature.Rectangles[1].Weight;

                if (null != node.Feature.Rectangles[2])
                    sum += node.Feature.Rectangles[2].CalculateSum(pointOffset) * node.Feature.Rectangles[2].Weight;

                index = sum < t ? node.Left : node.Right;
            }
            while (index > 0);

            return Alpha[-index];
        }
    }
}
