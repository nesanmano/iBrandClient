using System;
using System.Xml;
using JetBrains.Annotations;

namespace NomadLogic.Multimedia.FaceDetection.Harr
{
    [PublicAPI]
    public class HarrSize
    {
        public HarrSize(int width, int height)
        {
            Width = width;
            Height = height;
        }
        
        public HarrSize(double width, double height) : this((int)width, (int)height) { }

        public int Width { get; internal set; }

        public int Height { get; internal set; }

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
