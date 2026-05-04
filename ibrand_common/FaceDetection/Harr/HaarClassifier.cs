using System.Collections.Generic;
using System.Globalization;
using System.Xml;

namespace NomadLogic.iBrand.Transformation.FaceDetection.Harr
{
    public class HaarClassifier
    {
        private const string STAGE_ELEMENT_ID = "_";
	    private const string FEATURE_ID = "feature";
	    private const string THRESHOLD_ID = "threshold";
	    private const string LEFT_NODE_ID = "left_node";
	    private const string RIGHT_NODE_ID = "right_node";
	    private const string LEFT_VAL_ID = "left_val";
        private const string RIGHT_VAL_ID = "right_val";

	    private int _count;
	    private List<HaarFeature> _haarFeatures;
	    private List<float> _threshold;
	    private List<int> _left;
	    private List<int> _right;
	    private List<float> _alpha;

        public HaarClassifier()
        {
            _haarFeatures = new List<HaarFeature>();
            _threshold = new List<float>();
            _left = new List<int>();
            _right = new List<int>();
            _alpha = new List<float>();
        }

        public void Read(XmlTextReader  xmlReader) 
        {
    	    var lastIndex = 0;

            var classifierFinished = false;

            while(!classifierFinished)
            {
                xmlReader.Read();
                switch (xmlReader.NodeType) 
                {
                    case XmlNodeType.Element: // The node is an element.
                        if( xmlReader.Name.Equals(STAGE_ELEMENT_ID) )
                        {
                            lastIndex = ReadNode(xmlReader, lastIndex);
                        }
                        break;
                    case XmlNodeType.EndElement: //Display the end of the element.
                        if(xmlReader.Name.Equals(STAGE_ELEMENT_ID))
                        {
                            classifierFinished = true;
                        }
                        break;
                }
            }

    	    _count = _haarFeatures.Count;
        }

        private int ReadNode(XmlTextReader  xmlReader, int lastIndex)
        {
            var nodeFinished = false;

            while(!nodeFinished)
            {
                xmlReader.Read();
                switch (xmlReader.NodeType) 
                {
                    case XmlNodeType.Element: // The node is an element.
                        if( xmlReader.Name.Equals(FEATURE_ID) )
                        {
                            var hf = new HaarFeature();
                            hf.Read(xmlReader);
				            _haarFeatures.Add(hf);
                        } else if(xmlReader.Name.Equals(THRESHOLD_ID))
                        {
                            xmlReader.Read();
                            _threshold.Add(float.Parse(xmlReader.Value, CultureInfo.InvariantCulture));
                        } else if(xmlReader.Name.Equals(LEFT_NODE_ID))
                        {
                            xmlReader.Read();
                            _left.Add(int.Parse(xmlReader.Value, CultureInfo.InvariantCulture));
                        } else if(xmlReader.Name.Equals(LEFT_VAL_ID))
                        {
                            _left.Add(-lastIndex);
                            xmlReader.Read();
				            _alpha.Add(float.Parse(xmlReader.Value, CultureInfo.InvariantCulture));
                            lastIndex++;
                        } else if(xmlReader.Name.Equals(RIGHT_NODE_ID))
                        {
                            xmlReader.Read();
                            _right.Add(int.Parse(xmlReader.Value, CultureInfo.InvariantCulture));
                        } else if(xmlReader.Name.Equals(RIGHT_VAL_ID))
                        {
                            _right.Add(-lastIndex);
                            xmlReader.Read();
				            _alpha.Add(float.Parse(xmlReader.Value, CultureInfo.InvariantCulture));
                            lastIndex++;
                        }
                        break;
                    case XmlNodeType.EndElement: //Display the end of the element.
                        if(xmlReader.Name.Equals(STAGE_ELEMENT_ID))
                        {
                            nodeFinished = true;
                        }
                        break;
                }
            }

    	    return lastIndex;
        }

        public int Count 
        {
            get{ return _count;}
            set{ _count = value;}
        }

        public List<HaarFeature> HaarFeatures
        {
            get { return _haarFeatures; }
            set { _haarFeatures = value; }
        }

        public List<float> Threshold
        {
            get { return _threshold; }
            set { _threshold = value; }
        }

        public List<int> Left
        {
            get { return _left; }
            set { _left = value; }
        }

        public List<int> Right
        {
            get { return _right; }
            set { _right = value; }
        }

        public List<float> Alpha
        {
            get { return _alpha; }
            set { _alpha = value; }
        }
    }
}
