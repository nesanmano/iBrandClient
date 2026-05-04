using System;
using System.Collections.Generic;
using System.Drawing;
using System.Xml;
using JetBrains.Annotations;
using NomadLogic.Multimedia.FaceDetection.Harr.Hid;

namespace NomadLogic.Multimedia.FaceDetection.Harr
{
    [PublicAPI]
    public class HaarClassifierCascade
    {
        private const string SIZE_ID = "size";
        private const string STAGE_ID = "stages";
        private const string DOCUMENT_ID = "opencv_storage";
        private const string STAGE_ELEMENT_ID = "_";

        // liste de stage

        public HaarClassifierCascade()
        {
            OriginalWindowSize = new HarrSize(0, 0);
            RealWindowSize = new HarrSize(0, 0);
            StageClassifiers = new List<HaarStageClassifier>();
        }
        public HaarClassifierCascade(int stageCount)
        {
            for (var i = 0; i < stageCount; i++)
                StageClassifiers.Add(new HaarStageClassifier());

            Count = stageCount;
        }

        public int Flags { get; set; }

        public int Count { get; set; }

        public HarrSize OriginalWindowSize { get; set; }

        public HarrSize RealWindowSize { get; set; }

        public double Scale { get; set; }

        public List<HaarStageClassifier> StageClassifiers { get; set; }

        public HidHaarClassifierCascade HidCascade { get; set; }

        public int Run(HaarPoint point, int startStage)
        {
            var result = -1;
            int i, j;
            var cascade = HidCascade;

            if (null == cascade)
                throw new System.Exception("Hidden cascade has not been created.");

            // si la fenêtre n'est pas completement dans l'image, il n'y a pas de face
            if (point.X < 0 || point.Y < 0 ||
                point.X + RealWindowSize.Width >= cascade.Sum.Width - 2 ||
                point.Y + RealWindowSize.Height >= cascade.Sum.Height - 2)
                return result;

            // moyenne = tout ce qu'il y a dans le carré * inverseWindowArea
            var mean = cascade.CalculateSum(point) * cascade.InverseWindowArea;

            // varianceNormalizationFactor = somme des carrés * inverseWindowArea - moyenne^2
            var varianceNormalizationFactor = cascade.CalculateSquaredSum(point);
            varianceNormalizationFactor = varianceNormalizationFactor * cascade.InverseWindowArea - mean * mean;

            varianceNormalizationFactor = varianceNormalizationFactor >= 0 ? Math.Sqrt(varianceNormalizationFactor) : 1f;

            if (cascade.IsTree)
            {
                if (startStage == 0)
                {
                    var ptr = cascade.StageClassifiers[0];

                    while (null != ptr)
                    {
                        double stageSum = 0;

                        for (j = 0; j < ptr.Count; j++)
                        {
                            stageSum += ptr.Classifier[j].Evaluate(varianceNormalizationFactor, point);
                        }

                        if (stageSum >= ptr.Threshold)
                        {
                            ptr = ptr.Child;
                        }
                        else {
                            while (null != ptr && null == ptr.Next)
                                ptr = ptr.Parent;

                            if (null == ptr)
                            {
                                result = 0;
                                return result;
                            }

                            ptr = ptr.Next;
                        }
                    }
                }
            }
            else if (cascade.IsStumpBased)
            {
                for (i = startStage; i < cascade.Count; i++)
                {
                    double stageSum = 0;

                    if (cascade.StageClassifiers[i].HasTwoRectangles)
                    {
                        for (j = 0; j < cascade.StageClassifiers[i].Count; j++)
                        {
                            var classifier = cascade.StageClassifiers[i].Classifier[j];
                            var node = classifier.Node[0];
                            var t = node.Threshold * varianceNormalizationFactor;

                            double sum = node.Feature.Rectangles[0].CalculateSum(point) *
                                         node.Feature.Rectangles[0].Weight;
                            sum += node.Feature.Rectangles[1].CalculateSum(point) *
                                node.Feature.Rectangles[1].Weight;

                            double a = classifier.Alpha[0];
                            double b = classifier.Alpha[1];
                            stageSum += sum < t ? a : b;
                        }
                    }
                    else {
                        for (j = 0; j < cascade.StageClassifiers[i].Count; j++)
                        {
                            var classifier = cascade.StageClassifiers[i].Classifier[j];
                            var node = classifier.Node[0];
                            var t = node.Threshold * varianceNormalizationFactor;

                            double sum = node.Feature.Rectangles[0].CalculateSum(point) *
                                         node.Feature.Rectangles[0].Weight;
                            sum += node.Feature.Rectangles[1].CalculateSum(point) *
                                node.Feature.Rectangles[1].Weight;

                            if (null != node.Feature.Rectangles[2])//.p0 != (int*)0)
                                sum += node.Feature.Rectangles[2].CalculateSum(point) *
                                    node.Feature.Rectangles[2].Weight;

                            double a = classifier.Alpha[0];
                            double b = classifier.Alpha[1];
                            stageSum += sum < t ? a : b;
                        }
                    }

                    if (stageSum < cascade.StageClassifiers[i].Threshold)
                    {
                        result = -i;
                        return result;
                    }
                }
            }
            else {
                for (i = startStage; i < cascade.Count; i++)
                {
                    double stageSum = 0;

                    for (j = 0; j < cascade.StageClassifiers[i].Count; j++)
                    {
                        stageSum += cascade.StageClassifiers[i].Classifier[j].Evaluate(varianceNormalizationFactor, point);
                    }

                    if (stageSum < cascade.StageClassifiers[i].Threshold)
                    {
                        result = -i;
                        return result;
                    }
                }
            }

            result = 1;

            return result;
        }

        public void SetImages(Picture picture, PictureDouble sqsum, double scale)
        {
            if (picture == null) throw new ArgumentNullException(nameof(picture));
            var sum = picture;
            var squaredSum = sqsum;
            int i;
            var equRect = new Rectangle();

            if (scale <= 0)
                throw new System.Exception("Scale must be positive");

            if (sum.Width != squaredSum.Width || sum.Height != squaredSum.Height)
                throw new System.Exception("All integral images must have the same size");

            var cascade = HidCascade;

            Scale = scale;
            RealWindowSize.Width = (int)Math.Round(OriginalWindowSize.Width * scale);
            RealWindowSize.Height = (int)Math.Round(OriginalWindowSize.Height * scale);

            cascade.Sum = sum;
            cascade.SquaredSum = squaredSum;

            equRect.X = equRect.Y = (int)Math.Round(scale);
            equRect.Width = (int)Math.Round((OriginalWindowSize.Width - 2) * scale);
            equRect.Height = (int)Math.Round((OriginalWindowSize.Height - 2) * scale);
            double weightScale = 1.0f / (equRect.Width * equRect.Height);
            cascade.InverseWindowArea = weightScale;

            cascade.Point0 = new HaarPoint(equRect.X, equRect.Y);
            cascade.Point1 = new HaarPoint(equRect.X + equRect.Width, equRect.Y);
            cascade.Point2 = new HaarPoint(equRect.X, equRect.Y + equRect.Height);
            cascade.Point3 = new HaarPoint(equRect.X + equRect.Width, equRect.Y + equRect.Height);

            cascade.SquaredPoint0 = new HaarPoint(equRect.X, equRect.Y);
            cascade.SquaredPoint1 = new HaarPoint(equRect.X + equRect.Width, equRect.Y);
            cascade.SquaredPoint2 = new HaarPoint(equRect.X, equRect.Y + equRect.Height);
            cascade.SquaredPoint3 = new HaarPoint(equRect.X + equRect.Width, equRect.Y + equRect.Height);

            /* init pointers in haar features according to real window size and
               given image pointers */
            // pour chaque stage
            for (i = 0; i < Count; i++)
            {
                int j;
                // pour chaque arbre
                for (j = 0; j < cascade.StageClassifiers[i].Count; j++)
                {
                    // pour chaque node
                    int l;
                    for (l = 0; l < cascade.StageClassifiers[i].Classifier[j].Count; l++)
                    {
                        var feature = StageClassifiers[i].Classifiers[j].HaarFeatures[l];
                        var hidfeature = cascade.StageClassifiers[i].Classifier[j].Node[l].Feature;
                        double sum0 = 0, area0 = 0;
                        var r = new Rectangle[3];
                        int baseWidth = -1, baseHeight = -1;
                        int newBaseWidth = 0, newBaseHeight = 0;
                        int flagX = 0, flagY = 0;
                        int x0 = 0, y0 = 0;

                        /* align blocks */
                        int k;
                        for (k = 0; k < 3; k++)
                        {
                            if (null == hidfeature.Rectangles[k])//.p0 == (int*)0)
                                break;

                            r[k] = feature.Rectangles[k].Rectangle;
                            baseWidth = (int)CV_IMIN(baseWidth, (r[k].Width - 1));
                            baseWidth = (int)CV_IMIN(baseWidth, (r[k].X - r[0].X - 1));
                            baseHeight = (int)CV_IMIN(baseHeight, (r[k].Height - 1));
                            baseHeight = (int)CV_IMIN(baseHeight, (r[k].Y - r[0].Y - 1));
                        }

                        // Set le nombre de rectangles
                        var nr = k;

                        baseWidth += 1;
                        baseHeight += 1;
                        var kx = r[0].Width / baseWidth;
                        var ky = r[0].Height / baseHeight;

                        if (kx < 0)
                        {
                            flagX = 1;
                            newBaseWidth = (int)Math.Round(r[0].Width * scale) / kx;
                            x0 = (int)Math.Round(r[0].X * scale);
                        }

                        if (ky < 0)
                        {
                            flagY = 1;
                            newBaseHeight = (int)Math.Round(r[0].Height * scale) / ky;
                            y0 = (int)Math.Round(r[0].Y * scale);
                        }

                        for (k = 0; k < nr; k++)
                        {
                            var tr = new Rectangle();

                            if (flagX != 0)
                            {
                                tr.X = (r[k].X - r[0].X) * newBaseWidth / baseWidth + x0;
                                tr.Width = r[k].Width * newBaseWidth / baseWidth;
                            }
                            else {
                                tr.X = (int)Math.Round(r[k].X * scale);
                                tr.Width = (int)Math.Round(r[k].Width * scale);
                            }

                            if (flagY != 0)
                            {
                                tr.Y = (r[k].Y - r[0].Y) * newBaseHeight / baseHeight + y0;
                                tr.Height = r[k].Height * newBaseHeight / baseHeight;
                            }
                            else {
                                tr.Y = (int)Math.Round(r[k].Y * scale);
                                tr.Height = (int)Math.Round(r[k].Height * scale);
                            }

                            var correctionRatio = weightScale * (feature.Tilted == 0 ? 1 : 0.5);
                            if (feature.Tilted == 0)
                            {
                                hidfeature.Rectangles[k].Sum = sum;
                                hidfeature.Rectangles[k].Point0 = new HaarPoint(tr.X, tr.Y);
                                hidfeature.Rectangles[k].Point1 = new HaarPoint(tr.X + tr.Width, tr.Y);
                                hidfeature.Rectangles[k].Point2 = new HaarPoint(tr.X, tr.Y + tr.Height);
                                hidfeature.Rectangles[k].Point3 = new HaarPoint(tr.X + tr.Width, tr.Y + tr.Height);
                            }

                            hidfeature.Rectangles[k].Weight = (float)(feature.Rectangles[k].Weight * correctionRatio);

                            if (k == 0)
                                area0 = tr.Width * tr.Height;
                            else
                                sum0 += hidfeature.Rectangles[k].Weight * tr.Width * tr.Height;
                        }

                        hidfeature.Rectangles[0].Weight = (float)(-sum0 / area0);
                    } /* l */
                } /* j */
            }
        }

        public long CV_IMIN(int a, int b)
        {
            var longA = a & 0xffffffffL;
            var longB = b & 0xffffffffL;
            return longA ^ ((longA ^ longB) & (((longA < longB) ? 1 : 0) - 1));
        }

        public static HaarClassifierCascade Parse(XmlTextReader xmlReader)
        {
            var cascade = new HaarClassifierCascade();
            var cascadeFinished = false;

            while (!cascadeFinished)
            {
                xmlReader.Read();
                switch (xmlReader.NodeType)
                {
                    case XmlNodeType.Element: // The node is an element.
                        if (xmlReader.Name.Equals(SIZE_ID))
                        {
                            cascade.OriginalWindowSize.Read(xmlReader);
                        }
                        else if (xmlReader.Name.Equals(STAGE_ID))
                        {
                            cascade.ReadStages(xmlReader);
                        }
                        break;
                    case XmlNodeType.EndElement: //Display the end of the element.
                        if (xmlReader.Name.Equals(DOCUMENT_ID))
                        {
                            cascadeFinished = true;
                        }
                        break;
                }
            }

            return cascade;
        }

        private void ReadStages(XmlTextReader xmlReader)
        {
            var stageFinished = false;

            if (StageClassifiers == null)
            {
                StageClassifiers = new List<HaarStageClassifier>();
            }

            while (!stageFinished)
            {
                xmlReader.Read();
                switch (xmlReader.NodeType)
                {
                    case XmlNodeType.Element: // The node is an element.
                        if (xmlReader.Name.Equals(STAGE_ELEMENT_ID))
                        {
                            var hsc = new HaarStageClassifier();
                            hsc.Read(xmlReader);
                            StageClassifiers.Add(hsc);
                            var parent = hsc.Parent;
                            if (parent >= 0)
                            {
                                StageClassifiers[parent].Child = StageClassifiers.IndexOf(hsc);
                            }
                        }
                        break;
                    case XmlNodeType.EndElement: //Display the end of the element.
                        if (xmlReader.Name.Equals(STAGE_ID))
                        {
                            stageFinished = true;
                        }
                        break;
                }
            }

            Count = StageClassifiers.Count;
        }

    }
}
