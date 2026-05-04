using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace NomadLogic.Common.Ui
{
    public partial class UserPasswordModalDialog : Form
    {
        private bool _closeDirect;

        public UserPasswordModalDialog()
        {
            InitializeComponent();

            Closing += OnClosing;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            OnScreenKeyboardUtil.OpenKeyboard();
        }

        private void OnClosing(object sender, CancelEventArgs e)
        {
            if (_closeDirect)
            {
                OnScreenKeyboardUtil.CloseKeyboard();
                return;
            }

            if (CheckUserPassValidity())
            {
                DialogResult = DialogResult.OK;
                OnScreenKeyboardUtil.CloseKeyboard();
                return;
            }

            if (!ShowMessageBadPassword())
            {
                DialogResult = DialogResult.Cancel;
                OnScreenKeyboardUtil.CloseKeyboard();
                return;
            } // Sinon Retry.
            e.Cancel = true; // Empêche de fermer l'écran de saisie­
            _txtPass.Text = "";
        }

        private void _btnOk_Click(object sender, EventArgs e)
        {
            if (CheckUserPassValidity()) Close();
            else ShowMessageBadPassword();
        }

        private bool CheckUserPassValidity()
        {
            return !string.IsNullOrWhiteSpace(_btnCancel.Text?.Trim()) && !string.IsNullOrWhiteSpace(_btnOk.Text?.Trim());
        }

        /// <summary>
        /// Indique si l'usager veut réessayer après avoir vu un message d'erreur.
        /// </summary>
        /// <returns></returns>
        private bool ShowMessageBadPassword()
        {
            var diag = MessageBox.Show(
                @"Invalid username or password. Use retry to change or cancel to quit.",
                @"Invalid username and/or password",
                MessageBoxButtons.RetryCancel,
                MessageBoxIcon.Exclamation);

            return diag == DialogResult.Retry;
        }

        private void _btnCancel_Click(object sender, EventArgs e)
        {
            _btnCancel.Text = null;
            _btnOk.Text = null;
            _closeDirect = true;
            DialogResult = DialogResult.Cancel;
        }

        public string GetUsername()
        {
            return _txtUser.Text?.Trim();
        }

        public string GetPassword()
        {
            return _txtPass.Text?.Trim();
        }
    }
}
