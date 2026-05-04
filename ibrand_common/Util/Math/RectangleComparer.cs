using System.Collections.Generic;
using System.Drawing;
using JetBrains.Annotations;

namespace NomadLogic.Common.Util.Math
{
    [PublicAPI]
    public class RectangleComparer : IComparer<Rectangle>, IEqualityComparer<Rectangle>
    {
        public int Compare(Rectangle a, Rectangle b)
        {
            var zoneComp = a.X - b.X;
            if (zoneComp != 0) return zoneComp;
            zoneComp = a.Y - b.Y;
            if (zoneComp != 0) return zoneComp;
            zoneComp = a.Width - b.Width;
            if (zoneComp != 0) return zoneComp;
            return a.Height - b.Height;
        }

        public bool Equals(Rectangle x, Rectangle y)
        {
            return x.Equals(y);
        }

        public int GetHashCode(Rectangle obj)
        {
            return obj.GetHashCode();
        }
    }
}