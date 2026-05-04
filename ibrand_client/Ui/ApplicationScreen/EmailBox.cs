using System.Windows.Forms;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen
{
    public partial class EmailBox : UserControl
    {
        public EmailBox()
        {
            InitializeComponent();

            InitEmailComboBox();
        }

        private void InitEmailComboBox()
        {
            comboBox.Items.Add("aol.com");
            comboBox.Items.Add("gmail.com");
            comboBox.Items.Add("hotmail.com");
            comboBox.Items.Add("live.com");
            comboBox.Items.Add("msn.com");
            comboBox.Items.Add("rogers.com");
            comboBox.Items.Add("shaw.ca");
            comboBox.Items.Add("sympatico.ca");
            comboBox.Items.Add("yahoo.ca");
            comboBox.Items.Add("yahoo.com");
        }

        public override string Text
        {
            get
            {
                if (textBox.Text.Length == 0 && comboBox.Text.Length == 0)
                    return "";
                
                return textBox.Text + "@" + comboBox.Text;
            }
        }
    }
}
