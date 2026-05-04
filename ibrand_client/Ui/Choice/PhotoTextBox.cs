using System;
using System.Drawing;
using System.Windows.Forms;
using NomadLogic.Common.Util;

namespace NomadLogic.iBrandDesktop.Ui.Choice
{
    public partial class PhotoTextBox : UserControl
    {
        public PhotoTextBox()
        {
            InitializeComponent();

            browseButton.Click += OnBrowseButtonClick;
            textBox.Enabled = false;
        }

        public void OnBrowseButtonClick(object sender, EventArgs args)
        {
            var dialog = new OpenFileDialog
            {
                InitialDirectory = DirectoryUtil.GetLocalMediaDir(DirectoryUtil.NomadLogicMediaDirectoriesEnum.pictures),
                Filter = @"Picture files (*.jpg)|*.jpg",
                RestoreDirectory = false,
                Title = @"iBrand (tm)"
            };
            
            if (dialog.ShowDialog() == DialogResult.OK)
               textBox.Text = dialog.FileName;
        }

        public override string Text => textBox.Text;

        public void SetSize(Size size)
        {
            Size = size;
            textBox.Width = size.Width - browseButton.Width - 5;

            var initialLocation = browseButton.Location;
            browseButton.Location = new Point(textBox.Width + 5, initialLocation.Y);
        }

        public void Reset()
        {
            textBox.Text = "";
        }

        public new bool Enabled
        {
            get
            {
                return base.Enabled;
            }
            set
            {
                textBox.Enabled = false;
                browseButton.Enabled = value;
                base.Enabled = value;

                textBox.BackColor = Color.LightGray;
                Reset();
            }
        }
    }
}
