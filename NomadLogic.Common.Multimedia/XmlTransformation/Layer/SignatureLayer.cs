using System.Drawing;
using System.IO;
using System.Xml;
using NomadLogic.Common.Extentions;
using NomadLogic.Common.Respondent;
using NomadLogic.Multimedia.XmlTransformation.Filters;

namespace NomadLogic.Multimedia.XmlTransformation.Layer
{
    class SignatureLayer : FileLayer
    {
        private bool _greenScreen;

        public SignatureLayer(int x, int y, int[] scale)
            : base(x, y, scale)
        {
        }

        public SignatureLayer(SignatureLayer original)
            : base(original)
        {
            _greenScreen = original._greenScreen;
        }

        public SignatureLayer(int x, int y, int[] scale, XmlTextReader reader)
            : this(x, y, scale)
        {
            var questionNo = reader.GetAttribute("question_no");
            if (questionNo != null)
                QuestionNo = int.Parse(questionNo);

            var greenScreen = reader.GetAttribute("green_screen");
            if (greenScreen == "true")
                GreenScreen = true;
        }

        public bool GreenScreen
        {
            get => _greenScreen;
            set { _greenScreen = value; SettingsHaveChanged(); }
        }

        public override string GetFile(IRespondent respondent)
        {
            return respondent.GetAnswerText(QuestionNo);
        }

        public override AbstractLayer GetCopy()
        {
            return new SignatureLayer(this);
        }

        public override void GetImage(IRespondent respondent)
        {
            if (_finalImage == null)
            {
                var file = GetFile(respondent);

                if (file == null)
                    throw new FileNotFoundException();

                if (_greenScreen)
                {
                    AbstractChromaKeyFilter filtre = File.Exists(AbstractChromaKeyFilter.DefaultConfigFileLocation) ?
                        AbstractChromaKeyFilter.ReadJsonFile<ChromaKeyHslFilter>(AbstractChromaKeyFilter.DefaultConfigFileLocation) :
                        new ChromaKeyHslFilter();
                    var tmp = new Bitmap(file);
                    tmp.SetResolution(300, 300);
                    _finalImage = filtre.Filter(tmp);
                    tmp.Dispose();

                }
                else
                {
                    _finalImage = new Bitmap(file);
                    _finalImage.SetResolution(300, 300);
                }

                var tmp2 = _finalImage;
                _finalImage = tmp2.Scale( Width, Height);
                tmp2.Dispose();
            }
        }
    }
}
