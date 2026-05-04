using System.IO;
using System.Windows.Forms;

namespace NomadLogic.iBrandDesktop.Ui
{
    public partial class HtmlPopupDlg : Form
    {
        private string _file;

        private bool _initialized;
        
        public HtmlPopupDlg()
        {
            InitializeComponent();
        }

        public string HtmlFile
        {
            set
            {
                _file = value;
            }
        }

        public void SetHtml()
        {
            if (!_initialized)
            {

                if (_file != null)
                {
                    if (File.Exists(_file))
                    {
                        webBrowser.Navigate(_file);
                        _initialized = true;
                    }
                }
            }
        }

    }
}
