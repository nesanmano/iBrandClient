using System.Drawing;
using System.Windows.Forms;

namespace NomadLogic.iBrandDesktop.Ui.Choice
{
    public partial class EmailTextBox : UserControl
    {
        public EmailTextBox()
        {
            InitializeComponent();

            InitDomainComboBox();
        }

        private void InitDomainComboBox()
        {
            domainComboBox.Items.Add("aol.com");
            domainComboBox.Items.Add("arczip.com");
            domainComboBox.Items.Add("cogeco.ca");
            domainComboBox.Items.Add("earthlink.net");
            domainComboBox.Items.Add("gawab.com");
            domainComboBox.Items.Add("gmail.com");
            domainComboBox.Items.Add("golden.net");
            domainComboBox.Items.Add("hotmail.com");
            domainComboBox.Items.Add("in.com");
            domainComboBox.Items.Add("inbox.com");
            domainComboBox.Items.Add("live.fr");
            domainComboBox.Items.Add("live.com");
            domainComboBox.Items.Add("msn.com");
            domainComboBox.Items.Add("nomadlogic.com");
            domainComboBox.Items.Add("rogers.com");
            domainComboBox.Items.Add("sbcglobal.net");
            domainComboBox.Items.Add("shaw.ca");
            domainComboBox.Items.Add("sympatico.ca");
            domainComboBox.Items.Add("videotron.ca");
            domainComboBox.Items.Add("yahoo.ca");
            domainComboBox.Items.Add("yahoo.com");
            domainComboBox.Items.Add("yahoo.fr");
            domainComboBox.Items.Add("zenbe.com");
        }

        public override string Text
        {
            get
            {
                var address = addressTextBox.Text.Trim();
                if (address.Length == 0 &&
                    (domainComboBox.Text == null || domainComboBox.Text.Length == 0))
                    return "";
                return address + "@" + domainComboBox.Text;
            }

            set
            {
                try
                {
                    var emailAddress = value;
                    var index = emailAddress.IndexOf("@");
                    if (index != -1)
                    {
                        addressTextBox.Text = emailAddress.Substring(0, index);
                        domainComboBox.Text = emailAddress.Substring(index + 1, emailAddress.Length - index - 1);
                    }
                }
                catch (System.Exception)
                {
                    addressTextBox.Text = "";
                    domainComboBox.Text = "";
                }
            }
        }

        public void SetSize(Size size)
        {
            Size = size;
            domainComboBox.Width = size.Width;
            addressTextBox.Width = size.Width - label.Width;
            
            var initialLocation = label.Location;
            label.Location = new Point(addressTextBox.Width + 2, initialLocation.Y);
        }


        public void Reset()
        {
            addressTextBox.Text = "";
            domainComboBox.SelectedIndex = -1;
            domainComboBox.Text = "";
        }

        public new bool Enabled
        {
            get
            {
                return base.Enabled;
            }
            set 
            {
                domainComboBox.Enabled = value;
                addressTextBox.Enabled = value;
                base.Enabled = value;
                if (base.Enabled)
                {
                    addressTextBox.BackColor = Color.White;
                    domainComboBox.BackColor = Color.White;
                }
                else
                {
                    addressTextBox.BackColor = Color.LightGray;
                    domainComboBox.BackColor = Color.LightGray;
                    Reset();
                }
            }
        }

    }
}
