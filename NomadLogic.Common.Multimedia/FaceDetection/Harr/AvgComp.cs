using System.Drawing;

namespace NomadLogic.Multimedia.FaceDetection.Harr
{
    public class AvgComp
    {

        public AvgComp()
        {
            Neighbors = 0;
            Rectangle = new Rectangle();
        }

        public AvgComp(Rectangle rectangle, int neighbors)
        {
            Rectangle = rectangle;
            Neighbors = neighbors;
        }

        public Rectangle Rectangle { get; set; }


        public int Neighbors { get; set; }
    }
}
