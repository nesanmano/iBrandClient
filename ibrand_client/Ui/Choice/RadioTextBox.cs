using System;
using System.Drawing;
using System.Windows.Forms;

namespace NomadLogic.iBrandDesktop.Ui.Choice
{
    public partial class RadioTextBox : UserControl
    {
        public RadioTextBox()
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

        /*public override Font  Font
        {
	        Get 
	        {
                return this.label.Font;
	        }
	        
            Set 
	        { 
		        this.label.Font = value;
	        }
        }*/

        public void SetFocusable(bool focusable)
        {
            radioButton.Enabled = focusable;
        }
       
        public void SetSize(Size size)
        {
            label.Size = size;
            size.Width = size.Width + radioButton.Width;
            Size = new Size(size.Width, Math.Max(size.Height, radioButton.Height));
        }

        public Point GetLabelLocation()
        {
            return label.Location;
        }

        public Size GetLabelSize()
        {
            return label.Size;
        }

        public RadioButton RadioButton => radioButton;

        public bool Checked
        {
            set
            {
                radioButton.Checked = value;
            }

            get
            {
                return radioButton.Checked;
            }
        }
    }
}
