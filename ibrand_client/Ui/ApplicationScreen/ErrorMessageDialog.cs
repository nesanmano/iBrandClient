using System;
using System.Drawing;
using System.Windows.Forms;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen
{
    public partial class ErrorMessageDialog : Form
    {
        public ErrorMessageDialog(string message)
        {
            InitializeComponent();

            error_picture_box.Image = SystemIcons.Error.ToBitmap();

            ok_button.Click += OnOk;

            this.message.Text = message;
        }

        private void OnOk(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
