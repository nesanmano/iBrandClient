using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using NLog;
using NomadLogic.Common.Ui.Localization;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Networking;

namespace NomadLogic.iBrandDesktop.Ui
{
    public partial class CustomPrintstationAddressDialog : LocalizedForm
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
        
            var ipList = SsdpUtil.GetCurrentCustomPrintstations();

            listViewCurrentAddresses.MultiSelect = true;
            listViewCurrentAddresses.FullRowSelect = true;
            listViewCurrentAddresses.View = View.List;
            listViewCurrentAddresses.Sorting = SortOrder.Ascending;
            foreach (var ip in ipList)
            {
                var tmp = new[] {ip};
                var item = new ListViewItem(tmp) { Name = textBox1.Text };
                listViewCurrentAddresses.Items.Add(item);
            }

            OnChangeLanguage(this, null);
        }

        private void btnAddAddress_Click(object sender, EventArgs e)
        {
            if (textBox1.Text.Length == 0)
            {
                textBox1.ForeColor = Color.Black;
                return;
            }

            try
            {
                if (StringUtil.IsValidIPAddressWithPort(textBox1.Text, true))
                {
                    if (listViewCurrentAddresses.Items.ContainsKey(textBox1.Text))
                    {
                        textBox1.ForeColor = Color.Black;
                        return;
                    }
                    var tmp = new[] { textBox1.Text };
                    var item = new ListViewItem(tmp) {Name = textBox1.Text};
                    listViewCurrentAddresses.Items.Add(item);
                    textBox1.ForeColor = Color.Black;
                    UpdateSsdpPrintstations();
                }
                else
                {
                    textBox1.ForeColor = Color.Red;
                }
            }
            catch (System.Exception err)
            {
                log.Error(err);
                textBox1.ForeColor = Color.Red;
            }
        }

        private void UpdateSsdpPrintstations()
        {
            var updateList = (from ListViewItem item in listViewCurrentAddresses.Items select item.Text).ToList();
            SsdpUtil.UpdateCustomPrintstations(updateList);
        }

        private void btnRemoveSelected_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listViewCurrentAddresses.CheckedItems)
            {
                 item.Remove();
            }
            UpdateSsdpPrintstations();
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            listViewCurrentAddresses.Clear();
            UpdateSsdpPrintstations();
        }

        private void btnQuit_Click(object sender, EventArgs e)
        {
            Close();
        }

        /// <inheritdoc />
        protected override void InitializeControlTags()
        {
            // Pour la traduction automatique de LocalizedForm
            btnClear.Tag = "UI_CameraRingChoice_Label_Clear";
            btnAddAddress.Tag = "Add";
            btnQuit.Tag = "CloseMenu";
            btnRemoveSelected.Tag = "RemoveSelected";
        }

        /// <inheritdoc />
        protected override string GetLocalizedTextFromManager(string tag)
        {
            return Resources.iBrand.ResourceManager.GetString(tag, Resources.iBrand.Culture);
        }
    }
}
