using System.Collections;
using System.Windows.Forms;
using NomadLogic.Common.Services;
using NomadLogic.Common.Services.Configuration;

namespace NomadLogic.Common.Ui
{
    public partial class PopupDlg : Form
    {
        protected Hashtable _text_table;

        protected string _active_language;

        public PopupDlg()
        {
            InitializeComponent();

            _text_table = new Hashtable();
        }

        public void SetText(string language, string text)
        {
            _text_table.Add(language, text);
        }

        public string GetText()
        {
            if (_active_language == null)
            {
                var configuration = ServicesContainer.GetService<NomadConfiguration>();
                _active_language = configuration.PreferedLanguage;
            }

            var text = (string)_text_table[_active_language];

            if (text != null) return text;
            //Get the first supported language which we have text for.
            foreach (var lg in Constants.SUPPORTED_LANGUAGES)
            {
                text = (string)_text_table[lg];

                if (text != null)
                    return text;
            }

            return "";
        }

        public void SetText()
        {
            var text = GetText() ?? "";

            textBox.Text = text;
        }

        public string Language
        {
            set
            {
                _active_language = value;
            }
        }
    }
}