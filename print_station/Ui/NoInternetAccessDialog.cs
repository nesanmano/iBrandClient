using System;
using System.Drawing;
using System.Windows.Forms;

namespace NomadLogic.PrintStation.Ui
{
    public partial class NoInternetAccessDialog : Form
    {
        public NoInternetAccessDialog()
        {
            InitializeComponent();

            error_picture_box.Image = SystemIcons.Error.ToBitmap();

            ok_button.Click += OnOk;
        }

        private void OnOk(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        public bool HasToShowAgain()
        {
            return !dont_show_again_checkbox.Checked;
        }
    }
}