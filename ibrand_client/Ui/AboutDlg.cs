using System.Reflection;
using System.Windows.Forms;

namespace NomadLogic.iBrandDesktop.Ui
{
    public partial class AboutDlg : Form
    {
        public AboutDlg()
        {
            InitializeComponent();
            Init();
        }

        private void Init()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var attributes = assembly.GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false);
            if (attributes.Length > 0)
            {
                // On utilise un String plutôt que le très vanille système de versionnage de Visual Studio (Major.Release.Build.Revision)
                var descriptionAttribute = (AssemblyDescriptionAttribute)attributes[0];
                version_label.Text = Resources.iBrand.Version + @" " + descriptionAttribute.Description;
            }
            else
            {
                version_label.Text = Resources.iBrand.Version + @" Unknown. Cannot reach assembly information.";
            }

            
            
            company_label.Text = Resources.iBrand.Company;
            copyright_label.Text = Resources.iBrand.Copyright;
        }
    }
}