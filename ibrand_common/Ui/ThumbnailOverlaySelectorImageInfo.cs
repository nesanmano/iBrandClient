using System;
using System.Drawing;
using System.IO;
using JetBrains.Annotations;
using NomadLogic.Common.Util;

namespace NomadLogic.Common.Ui
{
    [PublicAPI]
    public class ThumbnailOverlaySelectorImageInfo: IDisposable
    {
        private Image _instance;

        public string ImageFileName { get; set; }

        public bool InitImage(string parentDirectoryPath)
        {
            var fullFilePath = Path.Combine(parentDirectoryPath, ImageFileName);
            if (!File.Exists(fullFilePath))
            {
                return false;
            }

            if (_instance != null)
            {
                var tmp = _instance;
                _instance = null;
                tmp.Dispose();
            }
            
            _instance = FileUtil.LoadImage(fullFilePath);
            return _instance != null;
        }
        
        public Image GetImageInstance()
        {
            return _instance;
        }

        public void Dispose()
        {
            _instance?.Dispose();
        }
    }
}