using System.IO;

namespace Canon.Eos.Framework.Eventing
{
    public class EosFileEventArgs : EosImageEventArgs
    {
        internal EosFileEventArgs(string filePath)
        {
            FilePath = filePath;
        }

        public string FilePath { get; }

        public override Stream GetStream()
        {
            return new FileStream(FilePath, FileMode.Open,
                FileAccess.Read, FileShare.Read);
        }        
    }
}
