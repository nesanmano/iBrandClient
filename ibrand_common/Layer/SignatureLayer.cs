using System.Drawing;
using System.IO;
using System.Xml;
using NomadLogic.iBrand.Common.Respondent;
using NomadLogic.iBrand.Common.Util;
using NomadLogic.iBrand.Transformation.Filters;

namespace NomadLogic.iBrand.Transformation.Layer
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
            get { return _greenScreen; }
            set { _greenScreen = value; SettingsHaveChanged(); }
        }

        public override string GetFile(IRespondent respondent)
        {
            return respondent.GetResponse(QuestionNo);
        }

        public override AbstractLayer GetCopy()
        {
            return new SignatureLayer(this);
        }

        public override void GetImage(IRespondent respondent)
        {
            if (_final_image == null)
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
                    _final_image = filtre.Filter(tmp);
                    tmp.Dispose();

                }
                else
                {
                    _final_image = new Bitmap(file);
                    _final_image.SetResolution(300, 300);
                }

                var tmp2 = _final_image;
                _final_image = ImageUtil.GetScaledInstance(tmp2, Width, Height);
                tmp2.Dispose();
            }
        }
    }
}
