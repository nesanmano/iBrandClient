using System;
using System.Drawing;
using System.Windows.Forms;

namespace NomadLogic.iBrandDesktop.Ui.Choice
{
    public partial class CheckTextBox : UserControl
    {
        public CheckTextBox()
        {
            InitializeComponent();
        }

        public override string Text
        {
            set
            {
                label.Text = value;
            }
        }

        public new Font  Font
        {
	        get 
	        {
                return label.Font;
	        }
	        
            set 
	        { 
		        label.Font = value;
	        }
        }

        public void SetFocusable(bool focusable)
        {
            checkBox.Enabled = focusable;
        }
       
        public void SetSize(Size size)
        {
            label.Width = size.Width;
            label.Height = size.Height + 1;
            size.Width = size.Width + checkBox.Width;
            Size = new Size(size.Width, Math.Max(size.Height, checkBox.Height));
        }

        public Size GetCheckSize()
        {
            return checkBox.Size;
        }

        public Size GetLabelSize()
        {
            return label.Size;
        }

        public CheckBox CheckBox => checkBox;

        public bool Checked
        {
            set
            {
                checkBox.Checked = value;
            }

            get
            {
                return checkBox.Checked;
            }
        }
    }
}
