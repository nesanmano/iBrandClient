using System.Drawing;
using System.IO;
using System.Xml;
using NomadLogic.Common.Respondent;

namespace NomadLogic.Multimedia.XmlTransformation.Layer
{
    class ImageLayer : FileLayer
    {
        private static readonly NLog.Logger log = NLog.LogManager.GetCurrentClassLogger();
        private string _src;
        private string _fileFormat;
        private string _dir;

        private bool _addLanguage;

        public ImageLayer(int x, int y, int[] scale)
            : base(x, y, scale)
        {
        }

        public ImageLayer(ImageLayer original)
            : base(original)
        {
            _src = original._src;
            _fileFormat = original._fileFormat;
            _dir = original._dir;
            _addLanguage = original._addLanguage;
        }

        public ImageLayer(int x, int y, int[] scale, XmlTextReader reader)
            : this(x, y, scale)
        {
            var fileFormat = reader.GetAttribute("file_format");
            if (fileFormat != null)
                FileFormat = fileFormat;

            var addLanguage = reader.GetAttribute("add_language");

            if (addLanguage != null)
                if (addLanguage == "true")
                    AddLanguage = true;

            var src = reader.GetAttribute("src");
            if (src != null)
                Src = src;

            var dir = reader.GetAttribute("dir");
            if (dir != null)
                Dir = dir;

            var blending = reader.GetAttribute("blending_mode");
            if (blending == null) return;
            if (blending.Equals("normal"))
            {
                _blendingMode = BLENDING_NORMAL;
            }
            else if (blending.Equals("multiply"))
            {
                _blendingMode = BLENDING_MULTIPLY;
            }
        }

        public string Src
        {
            get { return _src; }
            set { _src = value; SettingsHaveChanged(); }
        }

        public string FileFormat
        {
            get { return _fileFormat; }
            set { _fileFormat = value; SettingsHaveChanged(); }
        }

        public string Dir
        {
            get { return _dir; }
            set { _dir = value; SettingsHaveChanged(); }
        }

        public bool AddLanguage
        {
            get { return _addLanguage; }
            set { _addLanguage = value; SettingsHaveChanged(); }
        }

        public override string GetFile(IRespondent respondent)
        {
            if (_src != null)
            {
                var fileName = "";

                if (_dir != null)
                    fileName += _dir + Path.DirectorySeparatorChar;

                fileName += _src;

                if (_addLanguage)
                    fileName += "_" + respondent.Language;

                if (_fileFormat != null)
                    fileName += "." + _fileFormat;

                var contextDir = respondent.TerminalDirectory;

                return contextDir + fileName;
            }

            return null;
        }

        public override void GetImage(IRespondent respondent)
        {
            if (_finalImage == null)
            {
                var file = GetFile(respondent);

                if (file == null || !File.Exists(file))
                    throw new FileNotFoundException("File not found: " + file);

                try
                {
                    _finalImage = new Bitmap(file);
                    _finalImage.SetResolution(300, 300);
                } catch(System.Exception e) {
                    log.Error(e);
                }
            }
        }

        public override AbstractLayer GetCopy()
        {
            return new ImageLayer(this);
        }
    }
}
