namespace NomadLogic.iBrand.CameraRingClient
{
    partial class CustomServerConfigurationDialog
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.lblEnterAddress = new System.Windows.Forms.Label();
            this.txtAddress = new System.Windows.Forms.TextBox();
            this.cmdValidate = new System.Windows.Forms.Button();
            this.cmdClear = new System.Windows.Forms.Button();
            this.cmdCancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lblEnterAddress
            // 
            this.lblEnterAddress.AutoSize = true;
            this.lblEnterAddress.Location = new System.Drawing.Point(12, 9);
            this.lblEnterAddress.Name = "lblEnterAddress";
            this.lblEnterAddress.Size = new System.Drawing.Size(208, 13);
            this.lblEnterAddress.TabIndex = 0;
            this.lblEnterAddress.Text = "Enter custom Camera Ring server address:";
            // 
            // txtAddress
            // 
            this.txtAddress.Location = new System.Drawing.Point(12, 25);
            this.txtAddress.MaxLength = 22;
            this.txtAddress.Name = "txtAddress";
            this.txtAddress.Size = new System.Drawing.Size(240, 20);
            this.txtAddress.TabIndex = 1;
            this.txtAddress.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // cmdValidate
            // 
            this.cmdValidate.Location = new System.Drawing.Point(15, 51);
            this.cmdValidate.Name = "cmdValidate";
            this.cmdValidate.Size = new System.Drawing.Size(237, 23);
            this.cmdValidate.TabIndex = 2;
            this.cmdValidate.Text = "Validate and set";
            this.cmdValidate.UseVisualStyleBackColor = true;
            this.cmdValidate.Click += new System.EventHandler(this.cmdValidate_Click);
            // 
            // cmdClear
            // 
            this.cmdClear.Location = new System.Drawing.Point(15, 80);
            this.cmdClear.Name = "cmdClear";
            this.cmdClear.Size = new System.Drawing.Size(237, 23);
            this.cmdClear.TabIndex = 3;
            this.cmdClear.Text = "Clear and use SSDP";
            this.cmdClear.UseVisualStyleBackColor = true;
            this.cmdClear.Click += new System.EventHandler(this.cmdClear_Click);
            // 
            // cmdCancel
            // 
            this.cmdCancel.Location = new System.Drawing.Point(15, 109);
            this.cmdCancel.Name = "cmdCancel";
            this.cmdCancel.Size = new System.Drawing.Size(237, 23);
            this.cmdCancel.TabIndex = 4;
            this.cmdCancel.Text = "Cancel";
            this.cmdCancel.UseVisualStyleBackColor = true;
            this.cmdCancel.Click += new System.EventHandler(this.cmdCancel_Click);
            // 
            // CustomServerConfigurationDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(266, 144);
            this.ControlBox = false;
            this.Controls.Add(this.cmdCancel);
            this.Controls.Add(this.cmdClear);
            this.Controls.Add(this.cmdValidate);
            this.Controls.Add(this.txtAddress);
            this.Controls.Add(this.lblEnterAddress);
            this.Name = "CustomServerConfigurationDialog";
            this.Text = "Specific server configuration";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblEnterAddress;
        private System.Windows.Forms.TextBox txtAddress;
        private System.Windows.Forms.Button cmdValidate;
        private System.Windows.Forms.Button cmdClear;
        private System.Windows.Forms.Button cmdCancel;
    }
}