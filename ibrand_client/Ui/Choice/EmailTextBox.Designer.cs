using System.ComponentModel;
using System.Windows.Forms;

namespace NomadLogic.iBrandDesktop.Ui.Choice
{
    partial class EmailTextBox
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.addressTextBox = new System.Windows.Forms.TextBox();
            this.label = new System.Windows.Forms.Label();
            this.domainComboBox = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // addressTextBox
            // 
            this.addressTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F);
            this.addressTextBox.Location = new System.Drawing.Point(0, 4);
            this.addressTextBox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.addressTextBox.Name = "addressTextBox";
            this.addressTextBox.Size = new System.Drawing.Size(212, 22);
            this.addressTextBox.TabIndex = 0;
            // 
            // label
            // 
            this.label.Font = new System.Drawing.Font("Tahoma", 11F, System.Drawing.FontStyle.Bold);
            this.label.Location = new System.Drawing.Point(220, 0);
            this.label.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label.Name = "label";
            this.label.Size = new System.Drawing.Size(22, 25);
            this.label.TabIndex = 3;
            this.label.Text = "@";
            // 
            // domainComboBox
            // 
            this.domainComboBox.DropDownWidth = 197;
            this.domainComboBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F);
            this.domainComboBox.Location = new System.Drawing.Point(0, 33);
            this.domainComboBox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.domainComboBox.Name = "domainComboBox";
            this.domainComboBox.Size = new System.Drawing.Size(245, 24);
            this.domainComboBox.TabIndex = 2;
            // 
            // EmailTextBox
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(120F, 120F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.Controls.Add(this.domainComboBox);
            this.Controls.Add(this.label);
            this.Controls.Add(this.addressTextBox);
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "EmailTextBox";
            this.Size = new System.Drawing.Size(246, 67);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private TextBox addressTextBox;
        private Label label;
        private ComboBox domainComboBox;
    }
}
