using NomadLogic.Common.Util;

namespace NomadLogic.iBrandDesktop
{
    public class Logo
    {
        readonly string _verticalFileSrc;
        readonly string _verticalFileName;

        readonly string _horizontalFileSrc;
        readonly string _horizontalFileName;
        
        public Logo(string verticalFileSrc, string horizontalFileSrc)
        {
            _verticalFileSrc = verticalFileSrc;
            if (_verticalFileSrc == null)
                _verticalFileSrc = "";
            else
                _verticalFileName = StringUtil.GetFileName(verticalFileSrc);

            _horizontalFileSrc = horizontalFileSrc;
            if (_horizontalFileSrc == null)
                _horizontalFileSrc = "";
            else
                _horizontalFileName = StringUtil.GetFileName(horizontalFileSrc);
        }

        public string VerticalFileName => _verticalFileName;

        public string VerticalFileSrc => _verticalFileSrc;

        public string HorizontalFileName => _horizontalFileName;

        public string HorizontalFileSrc => _horizontalFileSrc;
    }
}
