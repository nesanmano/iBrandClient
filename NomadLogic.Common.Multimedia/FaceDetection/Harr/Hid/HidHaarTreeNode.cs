namespace NomadLogic.Multimedia.FaceDetection.Harr.Hid
{
    public class HidHaarTreeNode
    {
        public HidHaarTreeNode()
        {
            Feature = new HidHaarFeature();
        }

        public HidHaarFeature Feature { get; set; }

        public float Threshold { get; set; }

        public int Left { get; set; }

        public int Right { get; set; }
    }
}
