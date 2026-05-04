namespace NomadLogic.Common.Ui
{
    partial class PrinterSettingsDialog
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
        protected override void InitializeComponent()
        {
            this.lblPrinterName = new System.Windows.Forms.Label();
            this.comboPrinterName = new System.Windows.Forms.ComboBox();
            this.comboPaper = new System.Windows.Forms.ComboBox();
            this.lblPaper = new System.Windows.Forms.Label();
            this.btnClose = new System.Windows.Forms.Button();
            this.radioVTop = new System.Windows.Forms.RadioButton();
            this.lblVerticalAlignment = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.radioVBottom = new System.Windows.Forms.RadioButton();
            this.radioVCenter = new System.Windows.Forms.RadioButton();
            this.panel2 = new System.Windows.Forms.Panel();
            this.radioHRight = new System.Windows.Forms.RadioButton();
            this.radioHCenter = new System.Windows.Forms.RadioButton();
            this.lblHorizontalAlignment = new System.Windows.Forms.Label();
            this.radioHLeft = new System.Windows.Forms.RadioButton();
            this.chkScaled = new System.Windows.Forms.CheckBox();
            this.chkPageResolution = new System.Windows.Forms.CheckBox();
            this.chkIgnoreMargins = new System.Windows.Forms.CheckBox();
            this.chkDetectOrientation = new System.Windows.Forms.CheckBox();
            this.chkLandscape = new System.Windows.Forms.CheckBox();
            this.chkMaxSurfaceScale = new System.Windows.Forms.CheckBox();
            this.btnTestPrintSettings = new System.Windows.Forms.Button();
            this.lblPrintCounterText = new System.Windows.Forms.Label();
            this.lblPrintCounterValue = new System.Windows.Forms.Label();
            this.btnResetPrintCounter = new System.Windows.Forms.Button();
            this.nudPrintRoll = new System.Windows.Forms.NumericUpDown();
            this.lblPrintRoll = new System.Windows.Forms.Label();
            this.panel3 = new System.Windows.Forms.Panel();
            this.chkShowPrintCountWarning = new System.Windows.Forms.CheckBox();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudPrintRoll)).BeginInit();
            this.panel3.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblPrinterName
            // 
            this.lblPrinterName.AutoSize = true;
            this.lblPrinterName.Location = new System.Drawing.Point(13, 13);
            this.lblPrinterName.Name = "lblPrinterName";
            this.lblPrinterName.Size = new System.Drawing.Size(68, 13);
            this.lblPrinterName.TabIndex = 0;
            this.lblPrinterName.Text = "Printer Name";
            // 
            // comboPrinterName
            // 
            this.comboPrinterName.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.comboPrinterName.FormattingEnabled = true;
            this.comboPrinterName.Location = new System.Drawing.Point(16, 30);
            this.comboPrinterName.Name = "comboPrinterName";
            this.comboPrinterName.Size = new System.Drawing.Size(415, 33);
            this.comboPrinterName.TabIndex = 1;
            // 
            // comboPaper
            // 
            this.comboPaper.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.comboPaper.FormattingEnabled = true;
            this.comboPaper.Location = new System.Drawing.Point(16, 94);
            this.comboPaper.Name = "comboPaper";
            this.comboPaper.Size = new System.Drawing.Size(415, 33);
            this.comboPaper.TabIndex = 3;
            // 
            // lblPaper
            // 
            this.lblPaper.AutoSize = true;
            this.lblPaper.Location = new System.Drawing.Point(13, 78);
            this.lblPaper.Name = "lblPaper";
            this.lblPaper.Size = new System.Drawing.Size(35, 13);
            this.lblPaper.TabIndex = 2;
            this.lblPaper.Text = "Paper";
            // 
            // btnClose
            // 
            this.btnClose.Location = new System.Drawing.Point(16, 396);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(75, 23);
            this.btnClose.TabIndex = 4;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            // 
            // radioVTop
            // 
            this.radioVTop.AutoSize = true;
            this.radioVTop.Location = new System.Drawing.Point(9, 25);
            this.radioVTop.Name = "radioVTop";
            this.radioVTop.Size = new System.Drawing.Size(44, 17);
            this.radioVTop.TabIndex = 5;
            this.radioVTop.TabStop = true;
            this.radioVTop.Text = "Top";
            this.radioVTop.UseVisualStyleBackColor = true;
            // 
            // lblVerticalAlignment
            // 
            this.lblVerticalAlignment.AutoSize = true;
            this.lblVerticalAlignment.Location = new System.Drawing.Point(3, 9);
            this.lblVerticalAlignment.Name = "lblVerticalAlignment";
            this.lblVerticalAlignment.Size = new System.Drawing.Size(91, 13);
            this.lblVerticalAlignment.TabIndex = 6;
            this.lblVerticalAlignment.Text = "Vertical Alignment";
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.radioVBottom);
            this.panel1.Controls.Add(this.radioVCenter);
            this.panel1.Controls.Add(this.lblVerticalAlignment);
            this.panel1.Controls.Add(this.radioVTop);
            this.panel1.Location = new System.Drawing.Point(16, 147);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(194, 97);
            this.panel1.TabIndex = 7;
            // 
            // radioVBottom
            // 
            this.radioVBottom.AutoSize = true;
            this.radioVBottom.Location = new System.Drawing.Point(9, 71);
            this.radioVBottom.Name = "radioVBottom";
            this.radioVBottom.Size = new System.Drawing.Size(58, 17);
            this.radioVBottom.TabIndex = 8;
            this.radioVBottom.TabStop = true;
            this.radioVBottom.Text = "Bottom";
            this.radioVBottom.UseVisualStyleBackColor = true;
            // 
            // radioVCenter
            // 
            this.radioVCenter.AutoSize = true;
            this.radioVCenter.Location = new System.Drawing.Point(9, 48);
            this.radioVCenter.Name = "radioVCenter";
            this.radioVCenter.Size = new System.Drawing.Size(56, 17);
            this.radioVCenter.TabIndex = 7;
            this.radioVCenter.TabStop = true;
            this.radioVCenter.Text = "Center";
            this.radioVCenter.UseVisualStyleBackColor = true;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.radioHRight);
            this.panel2.Controls.Add(this.radioHCenter);
            this.panel2.Controls.Add(this.lblHorizontalAlignment);
            this.panel2.Controls.Add(this.radioHLeft);
            this.panel2.Location = new System.Drawing.Point(216, 147);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(215, 97);
            this.panel2.TabIndex = 8;
            // 
            // radioHRight
            // 
            this.radioHRight.AutoSize = true;
            this.radioHRight.Location = new System.Drawing.Point(9, 71);
            this.radioHRight.Name = "radioHRight";
            this.radioHRight.Size = new System.Drawing.Size(50, 17);
            this.radioHRight.TabIndex = 8;
            this.radioHRight.TabStop = true;
            this.radioHRight.Text = "Right";
            this.radioHRight.UseVisualStyleBackColor = true;
            // 
            // radioHCenter
            // 
            this.radioHCenter.AutoSize = true;
            this.radioHCenter.Location = new System.Drawing.Point(9, 48);
            this.radioHCenter.Name = "radioHCenter";
            this.radioHCenter.Size = new System.Drawing.Size(56, 17);
            this.radioHCenter.TabIndex = 7;
            this.radioHCenter.TabStop = true;
            this.radioHCenter.Text = "Center";
            this.radioHCenter.UseVisualStyleBackColor = true;
            // 
            // lblHorizontalAlignment
            // 
            this.lblHorizontalAlignment.AutoSize = true;
            this.lblHorizontalAlignment.Location = new System.Drawing.Point(3, 9);
            this.lblHorizontalAlignment.Name = "lblHorizontalAlignment";
            this.lblHorizontalAlignment.Size = new System.Drawing.Size(103, 13);
            this.lblHorizontalAlignment.TabIndex = 6;
            this.lblHorizontalAlignment.Text = "Horizontal Alignment";
            // 
            // radioHLeft
            // 
            this.radioHLeft.AutoSize = true;
            this.radioHLeft.Location = new System.Drawing.Point(9, 25);
            this.radioHLeft.Name = "radioHLeft";
            this.radioHLeft.Size = new System.Drawing.Size(43, 17);
            this.radioHLeft.TabIndex = 5;
            this.radioHLeft.TabStop = true;
            this.radioHLeft.Text = "Left";
            this.radioHLeft.UseVisualStyleBackColor = true;
            // 
            // chkScaled
            // 
            this.chkScaled.AutoSize = true;
            this.chkScaled.Location = new System.Drawing.Point(16, 251);
            this.chkScaled.Name = "chkScaled";
            this.chkScaled.Size = new System.Drawing.Size(124, 17);
            this.chkScaled.TabIndex = 9;
            this.chkScaled.Text = "Scale to surface size";
            this.chkScaled.UseVisualStyleBackColor = true;
            // 
            // chkPageResolution
            // 
            this.chkPageResolution.AutoSize = true;
            this.chkPageResolution.Location = new System.Drawing.Point(16, 274);
            this.chkPageResolution.Name = "chkPageResolution";
            this.chkPageResolution.Size = new System.Drawing.Size(120, 17);
            this.chkPageResolution.TabIndex = 10;
            this.chkPageResolution.Text = "Use page resolution";
            this.chkPageResolution.UseVisualStyleBackColor = true;
            // 
            // chkIgnoreMargins
            // 
            this.chkIgnoreMargins.AutoSize = true;
            this.chkIgnoreMargins.Location = new System.Drawing.Point(16, 297);
            this.chkIgnoreMargins.Name = "chkIgnoreMargins";
            this.chkIgnoreMargins.Size = new System.Drawing.Size(95, 17);
            this.chkIgnoreMargins.TabIndex = 11;
            this.chkIgnoreMargins.Text = "Ignore margins";
            this.chkIgnoreMargins.UseVisualStyleBackColor = true;
            // 
            // chkDetectOrientation
            // 
            this.chkDetectOrientation.AutoSize = true;
            this.chkDetectOrientation.Location = new System.Drawing.Point(216, 296);
            this.chkDetectOrientation.Name = "chkDetectOrientation";
            this.chkDetectOrientation.Size = new System.Drawing.Size(110, 17);
            this.chkDetectOrientation.TabIndex = 14;
            this.chkDetectOrientation.Text = "Detect orientation";
            this.chkDetectOrientation.UseVisualStyleBackColor = true;
            // 
            // chkLandscape
            // 
            this.chkLandscape.AutoSize = true;
            this.chkLandscape.Location = new System.Drawing.Point(216, 273);
            this.chkLandscape.Name = "chkLandscape";
            this.chkLandscape.Size = new System.Drawing.Size(110, 17);
            this.chkLandscape.TabIndex = 13;
            this.chkLandscape.Text = "Landscape image";
            this.chkLandscape.UseVisualStyleBackColor = true;
            // 
            // chkMaxSurfaceScale
            // 
            this.chkMaxSurfaceScale.AutoSize = true;
            this.chkMaxSurfaceScale.Location = new System.Drawing.Point(216, 250);
            this.chkMaxSurfaceScale.Name = "chkMaxSurfaceScale";
            this.chkMaxSurfaceScale.Size = new System.Drawing.Size(135, 17);
            this.chkMaxSurfaceScale.TabIndex = 12;
            this.chkMaxSurfaceScale.Text = "Maximize scale surface";
            this.chkMaxSurfaceScale.UseVisualStyleBackColor = true;
            // 
            // btnTestPrintSettings
            // 
            this.btnTestPrintSettings.Location = new System.Drawing.Point(131, 396);
            this.btnTestPrintSettings.Name = "btnTestPrintSettings";
            this.btnTestPrintSettings.Size = new System.Drawing.Size(137, 23);
            this.btnTestPrintSettings.TabIndex = 15;
            this.btnTestPrintSettings.Text = "Print test image";
            this.btnTestPrintSettings.UseVisualStyleBackColor = true;
            // 
            // lblPrintCounterText
            // 
            this.lblPrintCounterText.Location = new System.Drawing.Point(10, 33);
            this.lblPrintCounterText.Name = "lblPrintCounterText";
            this.lblPrintCounterText.Size = new System.Drawing.Size(85, 29);
            this.lblPrintCounterText.TabIndex = 16;
            this.lblPrintCounterText.Text = "Print counter :";
            this.lblPrintCounterText.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblPrintCounterValue
            // 
            this.lblPrintCounterValue.Location = new System.Drawing.Point(101, 33);
            this.lblPrintCounterValue.Name = "lblPrintCounterValue";
            this.lblPrintCounterValue.Size = new System.Drawing.Size(65, 29);
            this.lblPrintCounterValue.TabIndex = 17;
            this.lblPrintCounterValue.Text = "XXX";
            this.lblPrintCounterValue.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btnResetPrintCounter
            // 
            this.btnResetPrintCounter.Location = new System.Drawing.Point(157, 36);
            this.btnResetPrintCounter.Name = "btnResetPrintCounter";
            this.btnResetPrintCounter.Size = new System.Drawing.Size(80, 23);
            this.btnResetPrintCounter.TabIndex = 18;
            this.btnResetPrintCounter.Text = "Reset";
            this.btnResetPrintCounter.UseVisualStyleBackColor = true;
            // 
            // nudPrintRoll
            // 
            this.nudPrintRoll.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.nudPrintRoll.Location = new System.Drawing.Point(314, 3);
            this.nudPrintRoll.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.nudPrintRoll.Name = "nudPrintRoll";
            this.nudPrintRoll.Size = new System.Drawing.Size(98, 26);
            this.nudPrintRoll.TabIndex = 19;
            // 
            // lblPrintRoll
            // 
            this.lblPrintRoll.Location = new System.Drawing.Point(235, 2);
            this.lblPrintRoll.Name = "lblPrintRoll";
            this.lblPrintRoll.Size = new System.Drawing.Size(73, 29);
            this.lblPrintRoll.TabIndex = 20;
            this.lblPrintRoll.Text = "Print counter :";
            this.lblPrintRoll.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.chkShowPrintCountWarning);
            this.panel3.Controls.Add(this.btnResetPrintCounter);
            this.panel3.Controls.Add(this.lblPrintRoll);
            this.panel3.Controls.Add(this.lblPrintCounterText);
            this.panel3.Controls.Add(this.nudPrintRoll);
            this.panel3.Controls.Add(this.lblPrintCounterValue);
            this.panel3.Location = new System.Drawing.Point(16, 320);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(415, 70);
            this.panel3.TabIndex = 21;
            // 
            // checkBox1
            // 
            this.chkShowPrintCountWarning.AutoSize = true;
            this.chkShowPrintCountWarning.Location = new System.Drawing.Point(15, 9);
            this.chkShowPrintCountWarning.Name = "checkBox1";
            this.chkShowPrintCountWarning.Size = new System.Drawing.Size(80, 17);
            this.chkShowPrintCountWarning.TabIndex = 21;
            this.chkShowPrintCountWarning.Text = "Show warnings";
            this.chkShowPrintCountWarning.UseVisualStyleBackColor = true;
            // 
            // PrinterSettingsDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(450, 435);
            this.Controls.Add(this.panel3);
            this.Controls.Add(this.btnTestPrintSettings);
            this.Controls.Add(this.chkDetectOrientation);
            this.Controls.Add(this.chkLandscape);
            this.Controls.Add(this.chkMaxSurfaceScale);
            this.Controls.Add(this.chkIgnoreMargins);
            this.Controls.Add(this.chkPageResolution);
            this.Controls.Add(this.chkScaled);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.comboPaper);
            this.Controls.Add(this.lblPaper);
            this.Controls.Add(this.comboPrinterName);
            this.Controls.Add(this.lblPrinterName);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "PrinterSettingsDialog";
            this.Text = "Printer Settings";
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudPrintRoll)).EndInit();
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblPrinterName;
        private System.Windows.Forms.ComboBox comboPrinterName;
        private System.Windows.Forms.ComboBox comboPaper;
        private System.Windows.Forms.Label lblPaper;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.RadioButton radioVTop;
        private System.Windows.Forms.Label lblVerticalAlignment;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.RadioButton radioVBottom;
        private System.Windows.Forms.RadioButton radioVCenter;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.RadioButton radioHRight;
        private System.Windows.Forms.RadioButton radioHCenter;
        private System.Windows.Forms.Label lblHorizontalAlignment;
        private System.Windows.Forms.RadioButton radioHLeft;
        private System.Windows.Forms.CheckBox chkScaled;
        private System.Windows.Forms.CheckBox chkPageResolution;
        private System.Windows.Forms.CheckBox chkIgnoreMargins;
        private System.Windows.Forms.CheckBox chkDetectOrientation;
        private System.Windows.Forms.CheckBox chkLandscape;
        private System.Windows.Forms.CheckBox chkMaxSurfaceScale;
        private System.Windows.Forms.Button btnTestPrintSettings;
        private System.Windows.Forms.Label lblPrintCounterText;
        private System.Windows.Forms.Label lblPrintCounterValue;
        private System.Windows.Forms.Button btnResetPrintCounter;
        private System.Windows.Forms.NumericUpDown nudPrintRoll;
        private System.Windows.Forms.Label lblPrintRoll;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.CheckBox chkShowPrintCountWarning;
    }
}