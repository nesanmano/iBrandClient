namespace NomadLogic.iBrandDesktop
{
    public class FileToDownload
    {
        public FileToDownload(string src, string type)
        {
            this.Src = src;
            this.Type = type;
        }

        public string Src { get; }

        public string Type { get; }

        public float Size { get; set; }

        public long LastModifiedDate { get; set; }
    }
}
