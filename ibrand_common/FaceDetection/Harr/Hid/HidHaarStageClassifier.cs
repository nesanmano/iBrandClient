using System.Collections.Generic;

namespace NomadLogic.iBrand.Transformation.FaceDetection.Harr.Hid
{
    public class HidHaarStageClassifier
    {
        public HidHaarStageClassifier()
        {
        }
        
        public HidHaarStageClassifier(int count, float threshold,
                List<HidHaarClassifier> classifier, bool hasTwoRectangles,
                HidHaarStageClassifier next, HidHaarStageClassifier child,
                HidHaarStageClassifier parent)
        {
            Count = count;
            Threshold = threshold;
            Classifier = classifier;
            HasTwoRectangles = hasTwoRectangles;
            Next = next;
            Child = child;
            Parent = parent;
        }

        public int Count { get; set; }

        public float Threshold { get; set; }

        public List<HidHaarClassifier> Classifier { get; set; }

        public bool HasTwoRectangles { get; set; }

        public HidHaarStageClassifier Next { get; set; }

        public HidHaarStageClassifier Child { get; set; }

        public HidHaarStageClassifier Parent { get; set; }
    }
}
