using System;
using System.Collections.Generic;
using System.Drawing;

namespace NomadLogic.iBrand.Transformation.FaceDetection.Harr.Hid
{
    public class HidHaarClassifierCascade
    {

        private static float _stageThresholdBias = 0.0001f;
        private static double _epsilon = 2.2204460492503131e-016;

        private int _count;
        private bool _isStumpBased;
        private bool _hasTiltedFeatures;
        private bool _isTree;
        private double _inverseWindowArea;
        private Picture _sum;
        private PictureDouble _squaredSum;// tilted;
        private List<HidHaarStageClassifier> _stageClassifiers;
        private HaarPoint _squaredPoint0, _squaredPoint1, _squaredPoint2, _squaredPoint3;
        private HaarPoint _point0, _point1, _point2, _point3;

        public HidHaarClassifierCascade()
        {
            _squaredPoint0 =
            _squaredPoint1 =
            _squaredPoint2 =
            _squaredPoint3 = new HaarPoint();

            _point0 =
            _point1 =
            _point2 =
            _point3 = new HaarPoint();
        }

        public int CalculateSum(HaarPoint offset)
        {
            return _sum.Get(HaarPoint.Add(_point0, offset)) - _sum.Get(HaarPoint.Add(_point1, offset)) - _sum.Get(HaarPoint.Add(_point2, offset)) + _sum.Get(HaarPoint.Add(_point3, offset));
        }

        public double CalculateSquaredSum(HaarPoint offset)
        {
            return _squaredSum.Get(HaarPoint.Add(_squaredPoint0, offset)) -
        	    _squaredSum.Get(HaarPoint.Add(_squaredPoint1, offset)) -
        	    _squaredSum.Get(HaarPoint.Add(_squaredPoint2, offset)) +
        	    _squaredSum.Get(HaarPoint.Add(_squaredPoint3, offset));
        }

        public static HidHaarClassifierCascade Compute(HaarClassifierCascade hcc) {
            HidHaarClassifierCascade hhcc = new HidHaarClassifierCascade();
            int maxCount = 0, totalClassifiers = 0, totalNodes = 0;
            bool hasTiltedFeatures = false;
            Size originalWindowSize = hcc.OriginalWindowSize;

            foreach(HaarStageClassifier stageClassifer in hcc.StageClassifiers) {
                maxCount = Math.Max(maxCount, stageClassifer.Classifiers.Count);
                totalClassifiers += stageClassifer.Classifiers.Count;

                foreach(HaarClassifier classifier in stageClassifer.Classifiers)
                {
                    totalNodes += classifier.HaarFeatures.Count;

                    foreach(HaarFeature feature in classifier.HaarFeatures)
                    {
                        for (int i = 0; i < 3; i++) {
                            if (null != feature.Rectangles[i] &&
                        		    feature.Rectangles[i].Rectangle.Width != 0) {
                                Rectangle r = feature.Rectangles[i].Rectangle;
                                int tilted = feature.Tilted;
                                hasTiltedFeatures |= (tilted != 0 ? true : false);

                                if (r.Width < 0 ||
                            		    r.Height < 0 ||
                            		    r.Y < 0 ||
                            		    r.X + r.Width > originalWindowSize.Width ||
                            		    (tilted == 0 && (r.X < 0 || r.Y + r.Height > originalWindowSize.Height)) ||
                            		    (tilted != 0 && (r.X - r.Height < 0 || r.Y + r.Width + r.Height > originalWindowSize.Height)))
                                {
                                    String error = "rectangle of the classifier of the stage classifier is not inside the reference (original) cascade window";
                                    throw new System.Exception(error);
                                }
                            }
                        }
                    }
                }
            }

            //initialize the header info
            {
                hhcc.Count = hcc.StageClassifiers.Count;
                hhcc.StageClassifiers = new List<HidHaarStageClassifier>();
                for (int i = 0; i < hhcc.Count; i++)
                    hhcc.StageClassifiers.Add(new HidHaarStageClassifier());

                hhcc.IsStumpBased = true;
                hhcc.HasTiltedFeatures = hasTiltedFeatures;
                hhcc.IsTree = false;
            }

            //initialize the internal representation
            {
                for (int i = 0; i < hcc.StageClassifiers.Count; i++)
                {
                    HaarStageClassifier stageClassifier = hcc.StageClassifiers[i];
                    HidHaarStageClassifier hidStageClassifier = hhcc.StageClassifiers[i];

                    hidStageClassifier.Count = stageClassifier.Classifiers.Count;
                    hidStageClassifier.Threshold = stageClassifier.Threshold - _stageThresholdBias;
                    hidStageClassifier.Classifier = new List<HidHaarClassifier>();
                    for (int j = 0; j < hidStageClassifier.Count; j++)
                        hidStageClassifier.Classifier.Add(new HidHaarClassifier());

                    hidStageClassifier.HasTwoRectangles = true;
                    hidStageClassifier.Parent = (stageClassifier.Parent == -1)
                        ? null : hhcc.StageClassifiers[stageClassifier.Parent];
                    hidStageClassifier.Next = (stageClassifier.Next == -1)
                        ? null : hhcc.StageClassifiers[stageClassifier.Next];
                    hidStageClassifier.Child = (stageClassifier.Child == -1)
                        ? null : hhcc.StageClassifiers[stageClassifier.Child];

                    hhcc.IsTree |= (hidStageClassifier.Next != null) ? true : false;

                    for (int j = 0; j < stageClassifier.Classifiers.Count; j++)
                    {
                        HaarClassifier classifier = stageClassifier.Classifiers[j];
                        HidHaarClassifier hidClassifier = hidStageClassifier.Classifier[j];
                        int nodeCount = classifier.HaarFeatures.Count;

                        hidClassifier.Count = nodeCount;
                        hidClassifier.Node = new List<HidHaarTreeNode>();
                        for (int l = 0; l < nodeCount; l++)
                            hidClassifier.Node.Add(new HidHaarTreeNode());

                        for (int l = 0; l < nodeCount; l++)
                        {
                            HidHaarTreeNode node = hidClassifier.Node[l];
                            HaarFeature feature = classifier.HaarFeatures[l];

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
                        hhcc.IsStumpBased &= (nodeCount == 1 ? true : false);
                    }
                }
            }

            return hhcc;
        }

        public int Count
        {
            get { return _count; }
            set { _count = value; }
        }

        public bool IsStumpBased
        {
            get { return _isStumpBased; }
            set { _isStumpBased = value; }
        }

        public bool HasTiltedFeatures
        {
            get { return _hasTiltedFeatures; }
            set { _hasTiltedFeatures = value; }
        }

        public bool IsTree
        {
            get { return _isTree; }
            set { _isTree = value; }
        }

        public double InverseWindowArea
        {
            get { return _inverseWindowArea; }
            set { _inverseWindowArea = value; }
        }

        public Picture Sum
        {
            get { return _sum; }
            set { _sum = value; }
        }

        public PictureDouble SquaredSum
        {
            get { return _squaredSum; }
            set { _squaredSum = value; }
        }

        public List<HidHaarStageClassifier> StageClassifiers
        {
            get { return _stageClassifiers; }
            set { _stageClassifiers = value; }
        }

        public HaarPoint SquaredPoint0
        {
            get { return _squaredPoint0; }
            set { _squaredPoint0 = value; }
        }

        public HaarPoint SquaredPoint1
        {
            get { return _squaredPoint1; }
            set { _squaredPoint1 = value; }
        }

        public HaarPoint SquaredPoint2
        {
            get { return _squaredPoint2; }
            set { _squaredPoint2 = value; }
        }

        public HaarPoint SquaredPoint3
        {
            get { return _squaredPoint3; }
            set { _squaredPoint3 = value; }
        }

        public HaarPoint Point0
        {
            get { return _point0; }
            set { _point0 = value; }
        }

        public HaarPoint Point1
        {
            get { return _point1; }
            set { _point1 = value; }
        }

        public HaarPoint Point2
        {
            get { return _point2; }
            set { _point2 = value; }
        }

        public HaarPoint Point3
        {
            get { return _point3; }
            set { _point3 = value; }
        }
    }
}
