namespace NomadLogic.iBrand.Transformation.FaceDetection.Harr
{
    public class HaarPoint
    {
        public static HaarPoint Empty = new HaarPoint();

        /**
         * Constructeur
         */
        public HaarPoint() { }

        /**
         * Constructeur
         * @param x la position en x du point
         * @param y la position en y du point
         */
        public HaarPoint(int x, int y)
        {
            X = x;
            Y = y;
        }

        public static HaarPoint Add(HaarPoint left, HaarPoint right)
        {
            return new HaarPoint(left.X + right.X, left.Y + right.Y);
        }

        public int X { get; set; }

        public int Y { get; set; }


        // override object.Equals
        public override bool Equals(object obj)
        {

            if (this == obj)
                return true;
            if (obj == null)
                return false;
            if (GetType() != obj.GetType())
                return false;
            var other = (HaarPoint)obj;
            if (X != other.X)
                return false;
            if (Y != other.Y)
                return false;
            return true;
        }

        public override int GetHashCode()
        {
            return (X+Y) ^ X ^ Y;
        }
    }
}
