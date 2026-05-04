using System;
using System.Xml;

namespace NomadLogic.iBrand.Transformation.FaceDetection.Harr
{
    public class Size
    {
        public Size(int width, int height)
        {
            Width = width;
            Height = height;
        }
        public Size(double width, double height)
            : this((int)width, (int)height)
        {
        }

        public int Width { get; set; }

        public int Height { get; set; }

        public void Read(XmlTextReader xmlReader)
        {
            xmlReader.Read();
            var values = xmlReader.Value;
		    var indexSpace = values.IndexOf(" ", StringComparison.Ordinal);
		    Width = int.Parse(values.Substring(0, indexSpace));
		    Height = int.Parse(values.Substring(indexSpace+1));
	    }
    }
}
