using System.ComponentModel;
using System.Windows.Forms;

namespace NomadLogic.iBrandDesktop.Ui.Choice
{
    partial class RadioTextBox
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
            this.radioButton = new System.Windows.Forms.RadioButton();
            this.label = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // radioButton
            // 
            this.radioButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.radioButton.Location = new System.Drawing.Point(0, -1);
            this.radioButton.Name = "radioButton";
            this.radioButton.Size = new System.Drawing.Size(32, 30);
            this.radioButton.TabIndex = 0;
            // 
            // label
            // 
            this.label.Location = new System.Drawing.Point(28, 3);
            this.label.Name = "label";
            this.label.Size = new System.Drawing.Size(172, 26);
            this.label.TabIndex = 0;
            this.label.Text = "all";
            // 
            // RadioTextBox
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.Controls.Add(this.label);
            this.Controls.Add(this.radioButton);
            this.Name = "RadioTextBox";
            this.Size = new System.Drawing.Size(200, 29);
            this.ResumeLayout(false);

        }

        #endregion

        private RadioButton radioButton;
        private Label label;
    }
}
