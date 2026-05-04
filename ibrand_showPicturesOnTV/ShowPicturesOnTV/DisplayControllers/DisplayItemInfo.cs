using System.Drawing;
using JetBrains.Annotations;

namespace NomadLogic.ShowPicturesOnTV.DisplayControllers
{
    [PublicAPI]
    public enum DisplayItemTypeEnum
    {
        Unknown,
        Image,
        Video,
        Bundle
    }
    
    [PublicAPI]
    public class DisplayItemInfo
    {
        // ReSharper disable once EmptyConstructor
        public DisplayItemInfo() { }

        public DisplayItemTypeEnum DisplayType { get; set; } = DisplayItemTypeEnum.Unknown;

        public string Path { get; set; }

        public Size Size { get; set; }

        public Point Location { get; set; }
    }
}