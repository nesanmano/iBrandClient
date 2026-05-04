using System.Drawing;
using JetBrains.Annotations;

namespace NomadLogic.Multimedia
{
    [PublicAPI]
    public enum ColorSubstitutionFilterGroup
    {
        All,
        Countdown,
        PreviewWindow,
        Icon
    }

    [PublicAPI]
    public struct ColorSubstitutionFilter
    {
        public ColorSubstitutionFilter(int threshold, Color source, Color newColor, ColorSubstitutionFilterGroup group = ColorSubstitutionFilterGroup.All)
        {
            ThresholdValue = threshold;
            SourceColor = source;
            NewColor = newColor;
            Group = group;
        }

        public ColorSubstitutionFilterGroup Group { get; private set; }

        public int ThresholdValue {
            get;
        }
        
        public Color SourceColor
        {
            get;
        }
        
        public Color NewColor
        {
            get;
        }
    }
}