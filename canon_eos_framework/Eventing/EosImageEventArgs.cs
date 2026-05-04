using System;
using System.Drawing;
using System.IO;

namespace Canon.Eos.Framework.Eventing
{
    public abstract class EosImageEventArgs : EventArgs
    {
        private static NLog.Logger log = NLog.LogManager.GetCurrentClassLogger();
        public virtual Image GetImage()
        {
            Image result = null;
            try
            {

                using (var stream = GetStream())
                {
                    result = Image.FromStream(stream);
                }
            }
            catch(Exception e)
            {
                log.Warn(e);
            }

            return result;
        }

        public virtual Bitmap GetBitmap()
        {
            using (var stream = GetStream())
                return new Bitmap(stream);
        }

        public abstract Stream GetStream();
    }
}
