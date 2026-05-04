using System.Collections.Generic;
using System.Globalization;
using System.Xml;

namespace NomadLogic.iBrand.Transformation.FaceDetection.Harr
{
    public class HaarStageClassifier
    {
        private const string TREE_ID = "trees";
	    private const string STAGE_THRESHOLD_ID = "stage_threshold";
	    private const string PARENT_ID = "parent";
	    private const string NEXT_ID = "next";
	    private const string STAGE_ELEMENT_ID = "_";

        // liste d'arbres

        public HaarStageClassifier()
        {
            Classifiers = new List<HaarClassifier>();
        }

        public int Count { get; set; }

        public float Threshold { get; set; }

        public List<HaarClassifier> Classifiers { get; set; }

        public int Next { get; set; }

        public int Child { get; set; }

        public int Parent { get; set; }

        public void Read(XmlTextReader  xmlReader)
        {
            if(Classifiers == null) {
    		    Classifiers = new List<HaarClassifier>();
    	    }

    	    Parent = -1;
    	    Next = -1;
    	    Child = -1;

            var stageFinished = false;

            while(!stageFinished)
            {
                xmlReader.Read();
                switch (xmlReader.NodeType) 
                {
                    case XmlNodeType.Element: // The node is an element.
                        if( xmlReader.Name.Equals(TREE_ID) )
                        {
                            var treeFinished = false;

                            while(!treeFinished)
                            {
                                xmlReader.Read();
                                switch (xmlReader.NodeType) 
                                {
                                    case XmlNodeType.Element:
                                        if(xmlReader.Name.Equals(STAGE_ELEMENT_ID)) 
                                        {
		    					            var hc = new HaarClassifier();
                                            hc.Read(xmlReader);
		        				            Classifiers.Add(hc);
	    					            }
                                        break;
                                    case XmlNodeType.EndElement: //Display the end of the element.
                                        if (xmlReader.Name.Equals(TREE_ID))
                                        {
                                            treeFinished = true;
                                        }
                                        break;
                                }
                            }
                        }
                        else if (xmlReader.Name.Equals(STAGE_THRESHOLD_ID)) 
                        {
                            xmlReader.Read();
                            Threshold = float.Parse(xmlReader.Value, CultureInfo.InvariantCulture);
                        }
                        else if (xmlReader.Name.Equals(PARENT_ID)) 
                        {
                            xmlReader.Read();
                            Parent = int.Parse(xmlReader.Value, CultureInfo.InvariantCulture);
                        }
                        else if (xmlReader.Name.Equals(NEXT_ID)) 
                        {
                            xmlReader.Read();
                            Next = int.Parse(xmlReader.Value, CultureInfo.InvariantCulture);
    			        }
                        break;
                    case XmlNodeType.EndElement: //Display the end of the element.
                        if(xmlReader.Name.Equals(STAGE_ELEMENT_ID))
                        {
                            stageFinished = true;
                        }
                        break;
                }
            }

    	    Count = Classifiers.Count;
        }


    }
}
