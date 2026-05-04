namespace NomadLogic.RfidTest
{
    partial class RfidTesterUi
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
            this._txtInput = new System.Windows.Forms.TextBox();
            this._cmbReader = new System.Windows.Forms.ComboBox();
            this._btnLaunchTest = new System.Windows.Forms.Button();
            this._txtResult = new System.Windows.Forms.TextBox();
            this._cmbApiCall = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this._lblResult = new System.Windows.Forms.Label();
            this._lblReadValue = new System.Windows.Forms.Label();
            this._lblHexValue = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // _txtInput
            // 
            this._txtInput.Location = new System.Drawing.Point(297, 13);
            this._txtInput.Name = "_txtInput";
            this._txtInput.Size = new System.Drawing.Size(182, 20);
            this._txtInput.TabIndex = 0;
            // 
            // _cmbReader
            // 
            this._cmbReader.FormattingEnabled = true;
            this._cmbReader.Items.AddRange(new object[] {
            "Simple",
            "MR800"});
            this._cmbReader.Location = new System.Drawing.Point(12, 13);
            this._cmbReader.Name = "_cmbReader";
            this._cmbReader.Size = new System.Drawing.Size(145, 21);
            this._cmbReader.TabIndex = 1;
            // 
            // _btnLaunchTest
            // 
            this._btnLaunchTest.Location = new System.Drawing.Point(164, 12);
            this._btnLaunchTest.Name = "_btnLaunchTest";
            this._btnLaunchTest.Size = new System.Drawing.Size(127, 22);
            this._btnLaunchTest.TabIndex = 2;
            this._btnLaunchTest.Text = "Test Text";
            this._btnLaunchTest.UseVisualStyleBackColor = true;
            this._btnLaunchTest.Click += new System.EventHandler(this.btnForceTest_Click);
            // 
            // _txtResult
            // 
            this._txtResult.Enabled = false;
            this._txtResult.Font = new System.Drawing.Font("Liberation Mono", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._txtResult.Location = new System.Drawing.Point(12, 118);
            this._txtResult.Multiline = true;
            this._txtResult.Name = "_txtResult";
            this._txtResult.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this._txtResult.Size = new System.Drawing.Size(666, 288);
            this._txtResult.TabIndex = 3;
            // 
            // _cmbApiCall
            // 
            this._cmbApiCall.FormattingEnabled = true;
            this._cmbApiCall.Items.AddRange(new object[] {
            "Simple",
            "MR800"});
            this._cmbApiCall.Location = new System.Drawing.Point(509, 44);
            this._cmbApiCall.Name = "_cmbApiCall";
            this._cmbApiCall.Size = new System.Drawing.Size(169, 21);
            this._cmbApiCall.TabIndex = 4;
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(403, 42);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(100, 23);
            this.label1.TabIndex = 5;
            this.label1.Text = "API Call :";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.label1.Click += new System.EventHandler(this.Label1_Click);
            // 
            // _lblResult
            // 
            this._lblResult.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this._lblResult.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._lblResult.Location = new System.Drawing.Point(297, 68);
            this._lblResult.Name = "_lblResult";
            this._lblResult.Size = new System.Drawing.Size(381, 47);
            this._lblResult.TabIndex = 6;
            this._lblResult.Text = "RESULT";
            this._lblResult.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _lblReadValue
            // 
            this._lblReadValue.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this._lblReadValue.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._lblReadValue.Location = new System.Drawing.Point(12, 68);
            this._lblReadValue.Name = "_lblReadValue";
            this._lblReadValue.Size = new System.Drawing.Size(279, 22);
            this._lblReadValue.TabIndex = 7;
            this._lblReadValue.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // _lblHexValue
            // 
            this._lblHexValue.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this._lblHexValue.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._lblHexValue.Location = new System.Drawing.Point(12, 93);
            this._lblHexValue.Name = "_lblHexValue";
            this._lblHexValue.Size = new System.Drawing.Size(279, 22);
            this._lblHexValue.TabIndex = 8;
            this._lblHexValue.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // RfidTesterUi
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(690, 420);
            this.Controls.Add(this._lblHexValue);
            this.Controls.Add(this._lblReadValue);
            this.Controls.Add(this._lblResult);
            this.Controls.Add(this.label1);
            this.Controls.Add(this._cmbApiCall);
            this.Controls.Add(this._txtResult);
            this.Controls.Add(this._btnLaunchTest);
            this.Controls.Add(this._cmbReader);
            this.Controls.Add(this._txtInput);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "RfidTesterUi";
            this.Text = "Nomad Logic RFID Test App";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox _txtInput;
        private System.Windows.Forms.ComboBox _cmbReader;
        private System.Windows.Forms.Button _btnLaunchTest;
        private System.Windows.Forms.TextBox _txtResult;
        private System.Windows.Forms.ComboBox _cmbApiCall;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label _lblResult;
        private System.Windows.Forms.Label _lblReadValue;
        private System.Windows.Forms.Label _lblHexValue;
    }
}

