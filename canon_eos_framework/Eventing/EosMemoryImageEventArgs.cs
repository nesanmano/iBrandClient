using System.IO;

namespace Canon.Eos.Framework.Eventing
{
    public class EosMemoryImageEventArgs : EosImageEventArgs
    {
        internal EosMemoryImageEventArgs(byte[] imageData)
        {
            ImageData = imageData;
        }

        public byte[] ImageData { get; }

        public override Stream GetStream()
        {
            return new MemoryStream(ImageData);
        }
    }    
}
