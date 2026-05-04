namespace NomadLogic.Multimedia.Gpu.OpenCV
{
    partial class ImageBundleCalibrationPickerDialog
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
            this.mainPictureBox = new System.Windows.Forms.PictureBox();
            this.vScroll = new System.Windows.Forms.VScrollBar();
            this.hScroll = new System.Windows.Forms.HScrollBar();
            this.tbZoom = new System.Windows.Forms.TrackBar();
            this.panel2 = new System.Windows.Forms.Panel();
            this.label7 = new System.Windows.Forms.Label();
            this.boxC2R = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.boxC2H = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.boxC2Y = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.boxC2W = new System.Windows.Forms.TextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.boxC2X = new System.Windows.Forms.TextBox();
            this.label12 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.label6 = new System.Windows.Forms.Label();
            this.boxC1R = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.boxC1H = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.boxC1Y = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.boxC1W = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.boxC1X = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.progBar = new System.Windows.Forms.ProgressBar();
            this.btnClose = new System.Windows.Forms.Button();
            this.btnPreview = new System.Windows.Forms.Button();
            this.btnClear = new System.Windows.Forms.Button();
            this.btnGenerate = new System.Windows.Forms.Button();
            this.panel3 = new System.Windows.Forms.Panel();
            this.btnReset = new System.Windows.Forms.Button();
            this.lblBaseDir = new System.Windows.Forms.Label();
            this.btnChangeDir = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.mainPictureBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbZoom)).BeginInit();
            this.panel2.SuspendLayout();
            this.panel1.SuspendLayout();
            this.panel3.SuspendLayout();
            this.SuspendLayout();
            // 
            // mainPictureBox
            // 
            this.mainPictureBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.mainPictureBox.Cursor = System.Windows.Forms.Cursors.Cross;
            this.mainPictureBox.Location = new System.Drawing.Point(13, 13);
            this.mainPictureBox.Name = "mainPictureBox";
            this.mainPictureBox.Size = new System.Drawing.Size(750, 500);
            this.mainPictureBox.TabIndex = 0;
            this.mainPictureBox.TabStop = false;
            // 
            // vScroll
            // 
            this.vScroll.Location = new System.Drawing.Point(766, 13);
            this.vScroll.Name = "vScroll";
            this.vScroll.Size = new System.Drawing.Size(56, 500);
            this.vScroll.SmallChange = 10;
            this.vScroll.TabIndex = 1;
            this.vScroll.Scroll += new System.Windows.Forms.ScrollEventHandler(this.ScrollEvent);
            // 
            // hScroll
            // 
            this.hScroll.Location = new System.Drawing.Point(13, 516);
            this.hScroll.Name = "hScroll";
            this.hScroll.Size = new System.Drawing.Size(750, 43);
            this.hScroll.SmallChange = 10;
            this.hScroll.TabIndex = 2;
            this.hScroll.Scroll += new System.Windows.Forms.ScrollEventHandler(this.ScrollEvent);
            // 
            // tbZoom
            // 
            this.tbZoom.AutoSize = false;
            this.tbZoom.LargeChange = 0;
            this.tbZoom.Location = new System.Drawing.Point(13, 563);
            this.tbZoom.Maximum = 200;
            this.tbZoom.Minimum = 10;
            this.tbZoom.Name = "tbZoom";
            this.tbZoom.Size = new System.Drawing.Size(806, 45);
            this.tbZoom.SmallChange = 0;
            this.tbZoom.TabIndex = 3;
            this.tbZoom.TabStop = false;
            this.tbZoom.TickStyle = System.Windows.Forms.TickStyle.Both;
            this.tbZoom.Value = 10;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.label7);
            this.panel2.Controls.Add(this.boxC2R);
            this.panel2.Controls.Add(this.label8);
            this.panel2.Controls.Add(this.boxC2H);
            this.panel2.Controls.Add(this.label9);
            this.panel2.Controls.Add(this.boxC2Y);
            this.panel2.Controls.Add(this.label10);
            this.panel2.Controls.Add(this.boxC2W);
            this.panel2.Controls.Add(this.label11);
            this.panel2.Controls.Add(this.boxC2X);
            this.panel2.Controls.Add(this.label12);
            this.panel2.Location = new System.Drawing.Point(4, 105);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(198, 96);
            this.panel2.TabIndex = 15;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(3, 71);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(40, 13);
            this.label7.TabIndex = 12;
            this.label7.Text = "Radius";
            // 
            // boxC2R
            // 
            this.boxC2R.Enabled = false;
            this.boxC2R.Location = new System.Drawing.Point(44, 68);
            this.boxC2R.MaxLength = 5;
            this.boxC2R.Name = "boxC2R";
            this.boxC2R.Size = new System.Drawing.Size(47, 20);
            this.boxC2R.TabIndex = 10;
            this.boxC2R.Text = "0";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(101, 45);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(38, 13);
            this.label8.TabIndex = 10;
            this.label8.Text = "Height";
            // 
            // boxC2H
            // 
            this.boxC2H.Enabled = false;
            this.boxC2H.Location = new System.Drawing.Point(142, 42);
            this.boxC2H.MaxLength = 5;
            this.boxC2H.Name = "boxC2H";
            this.boxC2H.Size = new System.Drawing.Size(47, 20);
            this.boxC2H.TabIndex = 9;
            this.boxC2H.Text = "0";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(101, 19);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(14, 13);
            this.label9.TabIndex = 8;
            this.label9.Text = "Y";
            // 
            // boxC2Y
            // 
            this.boxC2Y.Enabled = false;
            this.boxC2Y.Location = new System.Drawing.Point(142, 16);
            this.boxC2Y.MaxLength = 5;
            this.boxC2Y.Name = "boxC2Y";
            this.boxC2Y.Size = new System.Drawing.Size(47, 20);
            this.boxC2Y.TabIndex = 7;
            this.boxC2Y.Text = "0";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(3, 45);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(35, 13);
            this.label10.TabIndex = 6;
            this.label10.Text = "Width";
            // 
            // boxC2W
            // 
            this.boxC2W.Enabled = false;
            this.boxC2W.Location = new System.Drawing.Point(44, 42);
            this.boxC2W.MaxLength = 5;
            this.boxC2W.Name = "boxC2W";
            this.boxC2W.Size = new System.Drawing.Size(47, 20);
            this.boxC2W.TabIndex = 8;
            this.boxC2W.Text = "0";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(3, 19);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(14, 13);
            this.label11.TabIndex = 2;
            this.label11.Text = "X";
            // 
            // boxC2X
            // 
            this.boxC2X.Enabled = false;
            this.boxC2X.Location = new System.Drawing.Point(44, 16);
            this.boxC2X.MaxLength = 5;
            this.boxC2X.Name = "boxC2X";
            this.boxC2X.Size = new System.Drawing.Size(47, 20);
            this.boxC2X.TabIndex = 6;
            this.boxC2X.Text = "0";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(3, 0);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(67, 13);
            this.label12.TabIndex = 0;
            this.label12.Text = "Coordinate 2";
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.label6);
            this.panel1.Controls.Add(this.boxC1R);
            this.panel1.Controls.Add(this.label3);
            this.panel1.Controls.Add(this.boxC1H);
            this.panel1.Controls.Add(this.label5);
            this.panel1.Controls.Add(this.boxC1Y);
            this.panel1.Controls.Add(this.label4);
            this.panel1.Controls.Add(this.boxC1W);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.boxC1X);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Location = new System.Drawing.Point(3, 3);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(198, 96);
            this.panel1.TabIndex = 14;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(3, 71);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(40, 13);
            this.label6.TabIndex = 12;
            this.label6.Text = "Radius";
            // 
            // boxC1R
            // 
            this.boxC1R.Enabled = false;
            this.boxC1R.Location = new System.Drawing.Point(44, 68);
            this.boxC1R.MaxLength = 5;
            this.boxC1R.Name = "boxC1R";
            this.boxC1R.Size = new System.Drawing.Size(47, 20);
            this.boxC1R.TabIndex = 5;
            this.boxC1R.Text = "0";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(101, 45);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(38, 13);
            this.label3.TabIndex = 10;
            this.label3.Text = "Height";
            // 
            // boxC1H
            // 
            this.boxC1H.Enabled = false;
            this.boxC1H.Location = new System.Drawing.Point(142, 42);
            this.boxC1H.MaxLength = 5;
            this.boxC1H.Name = "boxC1H";
            this.boxC1H.Size = new System.Drawing.Size(47, 20);
            this.boxC1H.TabIndex = 4;
            this.boxC1H.Text = "0";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(101, 19);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(14, 13);
            this.label5.TabIndex = 8;
            this.label5.Text = "Y";
            // 
            // boxC1Y
            // 
            this.boxC1Y.Enabled = false;
            this.boxC1Y.Location = new System.Drawing.Point(142, 16);
            this.boxC1Y.MaxLength = 5;
            this.boxC1Y.Name = "boxC1Y";
            this.boxC1Y.Size = new System.Drawing.Size(47, 20);
            this.boxC1Y.TabIndex = 2;
            this.boxC1Y.Text = "0";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(3, 45);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(35, 13);
            this.label4.TabIndex = 6;
            this.label4.Text = "Width";
            // 
            // boxC1W
            // 
            this.boxC1W.Enabled = false;
            this.boxC1W.Location = new System.Drawing.Point(44, 42);
            this.boxC1W.MaxLength = 5;
            this.boxC1W.Name = "boxC1W";
            this.boxC1W.Size = new System.Drawing.Size(47, 20);
            this.boxC1W.TabIndex = 3;
            this.boxC1W.Text = "0";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 19);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(14, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "X";
            // 
            // boxC1X
            // 
            this.boxC1X.Enabled = false;
            this.boxC1X.Location = new System.Drawing.Point(44, 16);
            this.boxC1X.MaxLength = 5;
            this.boxC1X.Name = "boxC1X";
            this.boxC1X.Size = new System.Drawing.Size(47, 20);
            this.boxC1X.TabIndex = 1;
            this.boxC1X.Text = "0";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(67, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Coordinate 1";
            // 
            // progBar
            // 
            this.progBar.Location = new System.Drawing.Point(4, 207);
            this.progBar.Name = "progBar";
            this.progBar.Size = new System.Drawing.Size(198, 37);
            this.progBar.TabIndex = 26;
            // 
            // btnClose
            // 
            this.btnClose.Location = new System.Drawing.Point(4, 552);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(198, 40);
            this.btnClose.TabIndex = 25;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // btnPreview
            // 
            this.btnPreview.Location = new System.Drawing.Point(3, 433);
            this.btnPreview.Name = "btnPreview";
            this.btnPreview.Size = new System.Drawing.Size(198, 37);
            this.btnPreview.TabIndex = 23;
            this.btnPreview.Text = "Preview";
            this.btnPreview.UseVisualStyleBackColor = true;
            this.btnPreview.Click += new System.EventHandler(this.btnPreview_Click);
            // 
            // btnClear
            // 
            this.btnClear.Location = new System.Drawing.Point(3, 280);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(198, 37);
            this.btnClear.TabIndex = 22;
            this.btnClear.Text = "Clear";
            this.btnClear.UseVisualStyleBackColor = true;
            this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
            // 
            // btnGenerate
            // 
            this.btnGenerate.Location = new System.Drawing.Point(4, 323);
            this.btnGenerate.Name = "btnGenerate";
            this.btnGenerate.Size = new System.Drawing.Size(198, 37);
            this.btnGenerate.TabIndex = 24;
            this.btnGenerate.Text = "Generate Normalization file";
            this.btnGenerate.UseVisualStyleBackColor = true;
            this.btnGenerate.Click += new System.EventHandler(this.btnGenerate_Click);
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.btnChangeDir);
            this.panel3.Controls.Add(this.panel1);
            this.panel3.Controls.Add(this.progBar);
            this.panel3.Controls.Add(this.panel2);
            this.panel3.Controls.Add(this.btnGenerate);
            this.panel3.Controls.Add(this.btnClose);
            this.panel3.Controls.Add(this.btnClear);
            this.panel3.Controls.Add(this.btnPreview);
            this.panel3.Location = new System.Drawing.Point(825, 15);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(206, 595);
            this.panel3.TabIndex = 27;
            // 
            // btnReset
            // 
            this.btnReset.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnReset.Location = new System.Drawing.Point(767, 517);
            this.btnReset.Name = "btnReset";
            this.btnReset.Size = new System.Drawing.Size(52, 40);
            this.btnReset.TabIndex = 28;
            this.btnReset.Text = "X";
            this.btnReset.UseVisualStyleBackColor = true;
            this.btnReset.Click += new System.EventHandler(this.btnReset_Click);
            // 
            // lblBaseDir
            // 
            this.lblBaseDir.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblBaseDir.Location = new System.Drawing.Point(13, 613);
            this.lblBaseDir.Name = "lblBaseDir";
            this.lblBaseDir.Size = new System.Drawing.Size(1018, 54);
            this.lblBaseDir.TabIndex = 29;
            // 
            // btnChangeDir
            // 
            this.btnChangeDir.Location = new System.Drawing.Point(4, 509);
            this.btnChangeDir.Name = "btnChangeDir";
            this.btnChangeDir.Size = new System.Drawing.Size(198, 37);
            this.btnChangeDir.TabIndex = 27;
            this.btnChangeDir.Text = "Change directory";
            this.btnChangeDir.UseVisualStyleBackColor = true;
            this.btnChangeDir.Click += new System.EventHandler(this.btnChangeDir_Click);
            // 
            // ImageBundleCalibrationPickerDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1040, 676);
            this.ControlBox = false;
            this.Controls.Add(this.lblBaseDir);
            this.Controls.Add(this.btnReset);
            this.Controls.Add(this.panel3);
            this.Controls.Add(this.tbZoom);
            this.Controls.Add(this.hScroll);
            this.Controls.Add(this.vScroll);
            this.Controls.Add(this.mainPictureBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "ImageBundleCalibrationPickerDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Image Bundle Calibration Dialog";
            ((System.ComponentModel.ISupportInitialize)(this.mainPictureBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbZoom)).EndInit();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel3.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox mainPictureBox;
        private System.Windows.Forms.VScrollBar vScroll;
        private System.Windows.Forms.HScrollBar hScroll;
        private System.Windows.Forms.TrackBar tbZoom;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox boxC2R;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox boxC2H;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox boxC2Y;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.TextBox boxC2W;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.TextBox boxC2X;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox boxC1R;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox boxC1H;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox boxC1Y;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox boxC1W;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox boxC1X;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ProgressBar progBar;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Button btnPreview;
        private System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.Button btnGenerate;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Button btnReset;
        private System.Windows.Forms.Label lblBaseDir;
        private System.Windows.Forms.Button btnChangeDir;
    }
}