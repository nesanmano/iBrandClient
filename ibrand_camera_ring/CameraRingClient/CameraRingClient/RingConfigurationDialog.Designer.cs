namespace NomadLogic.iBrand.CameraRingClient
{
    partial class RingConfigurationDialog
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
            this.okButton = new System.Windows.Forms.Button();
            this.lblCustomAddress = new System.Windows.Forms.Label();
            this.lblAddress = new System.Windows.Forms.Label();
            this.modifyCustomAddressButton = new System.Windows.Forms.Button();
            this.lblMissingCamTolerance = new System.Windows.Forms.Label();
            this.nudNbrCameraTolearance = new System.Windows.Forms.NumericUpDown();
            ((System.ComponentModel.ISupportInitialize)(this.nudNbrCameraTolearance)).BeginInit();
            this.SuspendLayout();
            // 
            // okButton
            // 
            this.okButton.Location = new System.Drawing.Point(15, 71);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(75, 23);
            this.okButton.TabIndex = 0;
            this.okButton.Text = "Ok";
            this.okButton.UseVisualStyleBackColor = true;
            this.okButton.Click += new System.EventHandler(this.okButton_Click);
            // 
            // lblCustomAddress
            // 
            this.lblCustomAddress.AutoSize = true;
            this.lblCustomAddress.Location = new System.Drawing.Point(12, 9);
            this.lblCustomAddress.Name = "lblCustomAddress";
            this.lblCustomAddress.Size = new System.Drawing.Size(120, 13);
            this.lblCustomAddress.TabIndex = 1;
            this.lblCustomAddress.Text = "Custom server address: ";
            // 
            // lblAddress
            // 
            this.lblAddress.AutoSize = true;
            this.lblAddress.Location = new System.Drawing.Point(254, 9);
            this.lblAddress.Name = "lblAddress";
            this.lblAddress.Size = new System.Drawing.Size(53, 13);
            this.lblAddress.TabIndex = 2;
            this.lblAddress.Text = "Unknown";
            // 
            // modifyCustomAddressButton
            // 
            this.modifyCustomAddressButton.Location = new System.Drawing.Point(173, 4);
            this.modifyCustomAddressButton.Name = "modifyCustomAddressButton";
            this.modifyCustomAddressButton.Size = new System.Drawing.Size(75, 23);
            this.modifyCustomAddressButton.TabIndex = 3;
            this.modifyCustomAddressButton.Text = "Modify";
            this.modifyCustomAddressButton.UseVisualStyleBackColor = true;
            this.modifyCustomAddressButton.Click += new System.EventHandler(this.modifyCustomAddressButton_Click);
            // 
            // lblMissingCamTolerance
            // 
            this.lblMissingCamTolerance.AutoSize = true;
            this.lblMissingCamTolerance.Location = new System.Drawing.Point(12, 35);
            this.lblMissingCamTolerance.Name = "lblMissingCamTolerance";
            this.lblMissingCamTolerance.Size = new System.Drawing.Size(130, 13);
            this.lblMissingCamTolerance.TabIndex = 4;
            this.lblMissingCamTolerance.Text = "Missing camera tolerance:";
            // 
            // nudNbrCameraTolearance
            // 
            this.nudNbrCameraTolearance.Location = new System.Drawing.Point(173, 35);
            this.nudNbrCameraTolearance.Maximum = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.nudNbrCameraTolearance.Name = "nudNbrCameraTolearance";
            this.nudNbrCameraTolearance.ReadOnly = true;
            this.nudNbrCameraTolearance.Size = new System.Drawing.Size(75, 20);
            this.nudNbrCameraTolearance.TabIndex = 5;
            this.nudNbrCameraTolearance.ValueChanged += new System.EventHandler(this.nudNbrCameraTolearance_ValueChanged);
            // 
            // RingConfigurationDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(419, 106);
            this.Controls.Add(this.nudNbrCameraTolearance);
            this.Controls.Add(this.lblMissingCamTolerance);
            this.Controls.Add(this.modifyCustomAddressButton);
            this.Controls.Add(this.lblAddress);
            this.Controls.Add(this.lblCustomAddress);
            this.Controls.Add(this.okButton);
            this.Name = "RingConfigurationDialog";
            this.Text = "RingConfigurationDialog";
            ((System.ComponentModel.ISupportInitialize)(this.nudNbrCameraTolearance)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Label lblCustomAddress;
        private System.Windows.Forms.Label lblAddress;
        private System.Windows.Forms.Button modifyCustomAddressButton;
        private System.Windows.Forms.Label lblMissingCamTolerance;
        private System.Windows.Forms.NumericUpDown nudNbrCameraTolearance;
    }
}