using System.Drawing;
using NLog;

namespace NomadLogic.iBrand.Transformation.FaceDetection.Harr
{
    public class HaarRectangle
    {
        private static Logger _log = LogManager.GetCurrentClassLogger();

        public HaarRectangle(Rectangle rectangle, float weight)
        {
            Rectangle = rectangle;
            Weight = weight;
        }

        public HaarRectangle()
        {
        }

        public void SetValues(string values)
        {
            values = values.Replace("\n                  ", "");
            var data = values.Split(' ');

            try
            {
                var x = int.Parse(data[0]);
                var y = int.Parse(data[1]);
                var w = int.Parse(data[2]);
                var h = int.Parse(data[3]);

                /* Franck: Correction d'un bogue. Tous les éléments finissent pas ".", ce qui n'est pas parsable comme Single.  */
                var f = data[4];
                if (f.EndsWith(".")) f = f.Substring(0, f.Length - 1);
                Weight = float.Parse(f);

                Rectangle = new Rectangle(x, y, w, h);
            }
            catch (System.Exception e)
            {
                _log.Error(e, "Invalid rectangle values: {0}", values);
                throw;
            }
        }

        public Rectangle Rectangle { get; set; }

        public float Weight { get; set; }
    }
}
