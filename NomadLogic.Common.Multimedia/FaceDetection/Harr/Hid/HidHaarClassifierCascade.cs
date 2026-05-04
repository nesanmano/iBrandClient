using System;
using System.Collections.Generic;
using JetBrains.Annotations;

// ReSharper disable NotAccessedVariable

namespace NomadLogic.Multimedia.FaceDetection.Harr.Hid
{
    [PublicAPI]
    public class HidHaarClassifierCascade
    {

        private static float _stageThresholdBias = 0.0001f;
        private static double _epsilon = 2.2204460492503131e-016;

        public HidHaarClassifierCascade()
        {
            SquaredPoint0 =
            SquaredPoint1 =
            SquaredPoint2 =
            SquaredPoint3 = new HaarPoint();

            Point0 =
            Point1 =
            Point2 =
            Point3 = new HaarPoint();
        }

        public int CalculateSum(HaarPoint offset)
        {
            return Sum.Get(HaarPoint.Add(Point0, offset)) - Sum.Get(HaarPoint.Add(Point1, offset)) - Sum.Get(HaarPoint.Add(Point2, offset)) + Sum.Get(HaarPoint.Add(Point3, offset));
        }

        public double CalculateSquaredSum(HaarPoint offset)
        {
            return SquaredSum.Get(HaarPoint.Add(SquaredPoint0, offset)) -
        	    SquaredSum.Get(HaarPoint.Add(SquaredPoint1, offset)) -
        	    SquaredSum.Get(HaarPoint.Add(SquaredPoint2, offset)) +
        	    SquaredSum.Get(HaarPoint.Add(SquaredPoint3, offset));
        }

        public static HidHaarClassifierCascade Compute(HaarClassifierCascade hcc) {
            var hhcc = new HidHaarClassifierCascade();
            int maxCount = 0, totalClassifiers = 0, totalNodes = 0;
            var hasTiltedFeatures = false;
            var originalWindowSize = hcc.OriginalWindowSize;

            foreach(var stageClassifer in hcc.StageClassifiers) {
                maxCount = Math.Max(maxCount, stageClassifer.Classifiers.Count);
                totalClassifiers += stageClassifer.Classifiers.Count;

                foreach(var classifier in stageClassifer.Classifiers)
                {
                    totalNodes += classifier.HaarFeatures.Count;

                    foreach(var feature in classifier.HaarFeatures)
                    {
                        for (var i = 0; i < 3; i++) {
                            if (null != feature.Rectangles[i] &&
                        		    feature.Rectangles[i].Rectangle.Width != 0) {
                                var r = feature.Rectangles[i].Rectangle;
                                var tilted = feature.Tilted;
                                hasTiltedFeatures |= tilted != 0;

                                if (r.Width >= 0 && r.Height >= 0 && r.Y >= 0 && r.X + r.Width <= originalWindowSize.Width &&
                                    (tilted != 0 || (r.X >= 0 && r.Y + r.Height <= originalWindowSize.Height)) &&
                                    (tilted == 0 || (r.X - r.Height >= 0 && r.Y + r.Width + r.Height <= originalWindowSize.Height))) continue;
                                const string ERROR = "rectangle of the classifier of the stage classifier is not inside the reference (original) cascade window";
                                throw new Exception(ERROR);
                            }
                        }
                    }
                }
            }

            //initialize the header info
            {
                hhcc.Count = hcc.StageClassifiers.Count;
                hhcc.StageClassifiers = new List<HidHaarStageClassifier>();
                for (var i = 0; i < hhcc.Count; i++)
                    hhcc.StageClassifiers.Add(new HidHaarStageClassifier());

                hhcc.IsStumpBased = true;
                hhcc.IsTree = false;
            }

            //initialize the internal representation
            {
                for (var i = 0; i < hcc.StageClassifiers.Count; i++)
                {
                    var stageClassifier = hcc.StageClassifiers[i];
                    var hidStageClassifier = hhcc.StageClassifiers[i];

                    hidStageClassifier.Count = stageClassifier.Classifiers.Count;
                    hidStageClassifier.Threshold = stageClassifier.Threshold - _stageThresholdBias;
                    hidStageClassifier.Classifier = new List<HidHaarClassifier>();
                    for (var j = 0; j < hidStageClassifier.Count; j++)
                        hidStageClassifier.Classifier.Add(new HidHaarClassifier());

                    hidStageClassifier.HasTwoRectangles = true;
                    hidStageClassifier.Parent = (stageClassifier.Parent == -1)
                        ? null : hhcc.StageClassifiers[stageClassifier.Parent];
                    hidStageClassifier.Next = (stageClassifier.Next == -1)
                        ? null : hhcc.StageClassifiers[stageClassifier.Next];
                    hidStageClassifier.Child = (stageClassifier.Child == -1)
                        ? null : hhcc.StageClassifiers[stageClassifier.Child];

                    hhcc.IsTree |= (hidStageClassifier.Next != null);

                    for (var j = 0; j < stageClassifier.Classifiers.Count; j++)
                    {
                        var classifier = stageClassifier.Classifiers[j];
                        var hidClassifier = hidStageClassifier.Classifier[j];
                        var nodeCount = classifier.HaarFeatures.Count;

                        hidClassifier.Count = nodeCount;
                        hidClassifier.Node = new List<HidHaarTreeNode>();
                        for (var l = 0; l < nodeCount; l++)
                            hidClassifier.Node.Add(new HidHaarTreeNode());

                        for (var l = 0; l < nodeCount; l++)
                        {
                            var node = hidClassifier.Node[l];
                            var feature = classifier.HaarFeatures[l];

                            node.Threshold = classifier.Threshold[l];
                            node.Left = classifier.Left[l];
                            node.Right = classifier.Right[l];

                            if ((null == feature.Rectangles[2]) ||
                        		    (Math.Abs(feature.Rectangles[2].Weight) < _epsilon ||
                        		    feature.Rectangles[2].Rectangle.Width == 0 ||
                        		    feature.Rectangles[2].Rectangle.Height == 0))
                                node.Feature.Rectangles[2] = null;
                            else
                                hidStageClassifier.HasTwoRectangles = false;
                        }

                        hidClassifier.Alpha = classifier.Alpha;
                        hhcc.IsStumpBased &= (nodeCount == 1);
                    }
                }
            }

            return hhcc;
        }

        public int Count { get; set; }

        public bool IsStumpBased { get; set; }

        public bool IsTree { get; set; }

        public double InverseWindowArea { get; set; }

        public Picture Sum { get; set; }

        public PictureDouble SquaredSum { get; set; }

        public List<HidHaarStageClassifier> StageClassifiers { get; set; }

        public HaarPoint SquaredPoint0 { get; set; }

        public HaarPoint SquaredPoint1 { get; set; }

        public HaarPoint SquaredPoint2 { get; set; }

        public HaarPoint SquaredPoint3 { get; set; }

        public HaarPoint Point0 { get; set; }

        public HaarPoint Point1 { get; set; }

        public HaarPoint Point2 { get; set; }

        public HaarPoint Point3 { get; set; }
    }
}
