using System;
using System.Xml;

namespace NomadLogic.Multimedia.FaceDetection.Harr
{
    public class HaarFeature
    {

        private const string RECTS_ID = "rects";
	    private const string TILTED_ID = "tilted";
	    private const string STAGE_ELEMENT_ID = "_";
	    private const string FEATURE_ID = "feature";

	    private int _tilted;
        private HaarRectangle[] _rectangles;
        
	    public HaarFeature() {
		    _rectangles = new HaarRectangle[3];
	    }

        public void Read(XmlTextReader  xmlReader) 
        {
            var i = 0;
            var featureFinished = false;

		    while (!featureFinished) 
            {
                xmlReader.Read();
                switch (xmlReader.NodeType) 
                {
                    case XmlNodeType.Element: // The node is an element.
                        if( xmlReader.Name.Equals(RECTS_ID) )
                        {
                            var rectsFinished = false;
                            
                            while(!rectsFinished)
                            {
                                xmlReader.Read();
                                if(xmlReader.NodeType == XmlNodeType.Element)
                                {
                                    if(xmlReader.Name.Equals(STAGE_ELEMENT_ID))
                                    {
                                        xmlReader.Read();
                                        var rec = new HaarRectangle();
                                        rec.SetValues(xmlReader.Value);
								        _rectangles[i] = rec;
								        i++;
                                    }
                                } 
                                else if (xmlReader.NodeType == XmlNodeType.EndElement && xmlReader.Name.Equals(RECTS_ID))
                                {
                                    rectsFinished = true;
                                }
                            }
                        }
                        else if ( xmlReader.Name.Equals(TILTED_ID) )
                        {
                            xmlReader.Read();
                            _tilted = Int32.Parse(xmlReader.Value);
                        }
                        break;
                    case XmlNodeType.EndElement: //Display the end of the element.
                        if(xmlReader.Name.Equals(FEATURE_ID))
                        {
                            featureFinished = true;
                        }
                        break;
                }
		    }
	    }

        public HaarRectangle[] Rectangles
        {
            get { return _rectangles; }
            set { _rectangles = value; }
        }

        public int Tilted
        {
            get { return _tilted; }
            set { _tilted = value; }
        }
    }
}
