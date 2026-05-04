namespace NomadLogic.iBrand.Transformation.FaceDetection.Harr.Hid
{
    public class HidHaarFeature
    {
        public class HidHaarRectangle
        {
            public HidHaarRectangle()
            {
                Point0 = Point1 = Point2 = Point3 = new HaarPoint();
            }

            public Picture Sum { get; set; }

            public int CalculateSum(HaarPoint offset)
            {
                return
                    Sum.Get(Point0.Y + offset.Y, Point0.X + offset.X)
                    - Sum.Get(Point1.Y + offset.Y, Point1.X + offset.X)
                    - Sum.Get(Point2.Y + offset.Y, Point2.X + offset.X)
                    + Sum.Get(Point3.Y + offset.Y, Point3.X + offset.X);

            }
            
            public HaarPoint Point0 { get; set; }
            
            public HaarPoint Point1 { get; set; }
            
            public HaarPoint Point2 { get; set; }
            
            public HaarPoint Point3 { get; set; }
            
            public float Weight { get; set; }
        }

        public HidHaarFeature()
        {
            Rectangles = new HidHaarRectangle[3];
            for (var i = 0; i < 3; i++)
            {
                Rectangles[i] = new HidHaarRectangle();
            }
        }

        public HidHaarRectangle[] Rectangles { get; set; }
    }
}
