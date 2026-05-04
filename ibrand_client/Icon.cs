using NomadLogic.Common.Util;

namespace NomadLogic.iBrandDesktop
{
    public class Icon
    {
        private readonly string _fileSrc;
        private readonly string _fileName;

        public Icon(string fileSrc)
        {
            _fileSrc = fileSrc;

            _fileName = StringUtil.GetFileName(fileSrc);
        }

        public string FileSrc => _fileSrc;

        public string FileName => _fileName;
    }
}
