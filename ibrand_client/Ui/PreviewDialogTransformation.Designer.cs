using System.ComponentModel;
using System.Windows.Forms;

namespace NomadLogic.iBrandDesktop.Ui
{
    partial class PreviewDialogTransformation
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            var resources = new System.ComponentModel.ComponentResourceManager(typeof(PreviewDialogTransformation));
            this.TransformedPictureBox = new System.Windows.Forms.PictureBox();
            this.PanelValues = new System.Windows.Forms.Panel();
            this.ButtonReset = new System.Windows.Forms.Button();
            this.NumRotation = new System.Windows.Forms.NumericUpDown();
            this.NumScale = new System.Windows.Forms.NumericUpDown();
            this.NumY = new System.Windows.Forms.NumericUpDown();
            this.NumX = new System.Windows.Forms.NumericUpDown();
            this.ButtonApply = new System.Windows.Forms.Button();
            this.LabelRotation = new System.Windows.Forms.Label();
            this.SliderRotation = new System.Windows.Forms.TrackBar();
            this.LabelScale = new System.Windows.Forms.Label();
            this.SliderScale = new System.Windows.Forms.TrackBar();
            this.LabelY = new System.Windows.Forms.Label();
            this.SliderY = new System.Windows.Forms.TrackBar();
            this.LabelX = new System.Windows.Forms.Label();
            this.SliderX = new System.Windows.Forms.TrackBar();
            this.ButtonKeepIt = new System.Windows.Forms.Button();
            this.ButtonCancel = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.TransformedPictureBox)).BeginInit();
            this.PanelValues.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.NumRotation)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.NumScale)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.NumY)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.NumX)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.SliderRotation)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.SliderScale)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.SliderY)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.SliderX)).BeginInit();
            this.SuspendLayout();
            // 
            // TransformedPictureBox
            // 
            this.TransformedPictureBox.Location = new System.Drawing.Point(12, 12);
            this.TransformedPictureBox.Name = "TransformedPictureBox";
            this.TransformedPictureBox.Size = new System.Drawing.Size(462, 338);
            this.TransformedPictureBox.TabIndex = 0;
            this.TransformedPictureBox.TabStop = false;
            // 
            // PanelValues
            // 
            this.PanelValues.Controls.Add(this.ButtonCancel);
            this.PanelValues.Controls.Add(this.ButtonKeepIt);
            this.PanelValues.Controls.Add(this.ButtonReset);
            this.PanelValues.Controls.Add(this.NumRotation);
            this.PanelValues.Controls.Add(this.NumScale);
            this.PanelValues.Controls.Add(this.NumY);
            this.PanelValues.Controls.Add(this.NumX);
            this.PanelValues.Controls.Add(this.ButtonApply);
            this.PanelValues.Controls.Add(this.LabelRotation);
            this.PanelValues.Controls.Add(this.SliderRotation);
            this.PanelValues.Controls.Add(this.LabelScale);
            this.PanelValues.Controls.Add(this.SliderScale);
            this.PanelValues.Controls.Add(this.LabelY);
            this.PanelValues.Controls.Add(this.SliderY);
            this.PanelValues.Controls.Add(this.LabelX);
            this.PanelValues.Controls.Add(this.SliderX);
            this.PanelValues.Location = new System.Drawing.Point(480, 12);
            this.PanelValues.Name = "PanelValues";
            this.PanelValues.Size = new System.Drawing.Size(352, 338);
            this.PanelValues.TabIndex = 1;
            // 
            // ButtonReset
            // 
            this.ButtonReset.Location = new System.Drawing.Point(239, 224);
            this.ButtonReset.Name = "ButtonReset";
            this.ButtonReset.Size = new System.Drawing.Size(110, 23);
            this.ButtonReset.TabIndex = 13;
            this.ButtonReset.Text = "Reset values";
            this.ButtonReset.UseVisualStyleBackColor = true;
            this.ButtonReset.Click += new System.EventHandler(this.ButtonReset_Click);
            // 
            // NumRotation
            // 
            this.NumRotation.Location = new System.Drawing.Point(287, 173);
            this.NumRotation.Name = "NumRotation";
            this.NumRotation.Size = new System.Drawing.Size(62, 20);
            this.NumRotation.TabIndex = 12;
            this.NumRotation.ValueChanged += new System.EventHandler(this.NumRotation_change);
            // 
            // NumScale
            // 
            this.NumScale.Location = new System.Drawing.Point(287, 122);
            this.NumScale.Name = "NumScale";
            this.NumScale.Size = new System.Drawing.Size(62, 20);
            this.NumScale.TabIndex = 11;
            this.NumScale.ValueChanged += new System.EventHandler(this.NumScale_change);
            // 
            // NumY
            // 
            this.NumY.Location = new System.Drawing.Point(287, 71);
            this.NumY.Name = "NumY";
            this.NumY.Size = new System.Drawing.Size(62, 20);
            this.NumY.TabIndex = 10;
            this.NumY.ValueChanged += new System.EventHandler(this.NumY_change);
            // 
            // NumX
            // 
            this.NumX.Location = new System.Drawing.Point(287, 20);
            this.NumX.Name = "NumX";
            this.NumX.Size = new System.Drawing.Size(62, 20);
            this.NumX.TabIndex = 9;
            this.NumX.ValueChanged += new System.EventHandler(this.NumX_change);
            // 
            // ButtonApply
            // 
            this.ButtonApply.Location = new System.Drawing.Point(7, 224);
            this.ButtonApply.Name = "ButtonApply";
            this.ButtonApply.Size = new System.Drawing.Size(75, 23);
            this.ButtonApply.TabIndex = 8;
            this.ButtonApply.Text = "Apply";
            this.ButtonApply.UseVisualStyleBackColor = true;
            this.ButtonApply.Click += new System.EventHandler(this.ButtonApply_Click);
            // 
            // LabelRotation
            // 
            this.LabelRotation.AutoSize = true;
            this.LabelRotation.Location = new System.Drawing.Point(4, 157);
            this.LabelRotation.Name = "LabelRotation";
            this.LabelRotation.Size = new System.Drawing.Size(47, 13);
            this.LabelRotation.TabIndex = 7;
            this.LabelRotation.Text = "Rotation";
            // 
            // SliderRotation
            // 
            this.SliderRotation.Location = new System.Drawing.Point(3, 173);
            this.SliderRotation.Name = "SliderRotation";
            this.SliderRotation.Size = new System.Drawing.Size(278, 45);
            this.SliderRotation.TabIndex = 6;
            this.SliderRotation.ValueChanged += new System.EventHandler(this.SliderRotation_change);
            // 
            // LabelScale
            // 
            this.LabelScale.AutoSize = true;
            this.LabelScale.Location = new System.Drawing.Point(4, 106);
            this.LabelScale.Name = "LabelScale";
            this.LabelScale.Size = new System.Drawing.Size(55, 13);
            this.LabelScale.TabIndex = 5;
            this.LabelScale.Text = "Scale size";
            // 
            // SliderScale
            // 
            this.SliderScale.Location = new System.Drawing.Point(3, 122);
            this.SliderScale.Name = "SliderScale";
            this.SliderScale.Size = new System.Drawing.Size(278, 45);
            this.SliderScale.TabIndex = 4;
            this.SliderScale.ValueChanged += new System.EventHandler(this.SliderScale_change);
            // 
            // LabelY
            // 
            this.LabelY.AutoSize = true;
            this.LabelY.Location = new System.Drawing.Point(4, 55);
            this.LabelY.Name = "LabelY";
            this.LabelY.Size = new System.Drawing.Size(54, 13);
            this.LabelY.TabIndex = 3;
            this.LabelY.Text = "Position Y";
            // 
            // SliderY
            // 
            this.SliderY.Location = new System.Drawing.Point(3, 71);
            this.SliderY.Name = "SliderY";
            this.SliderY.Size = new System.Drawing.Size(278, 45);
            this.SliderY.TabIndex = 2;
            this.SliderY.ValueChanged += new System.EventHandler(this.SliderY_change);
            // 
            // LabelX
            // 
            this.LabelX.AutoSize = true;
            this.LabelX.Location = new System.Drawing.Point(4, 4);
            this.LabelX.Name = "LabelX";
            this.LabelX.Size = new System.Drawing.Size(54, 13);
            this.LabelX.TabIndex = 1;
            this.LabelX.Text = "Position X";
            // 
            // SliderX
            // 
            this.SliderX.Location = new System.Drawing.Point(3, 20);
            this.SliderX.Name = "SliderX";
            this.SliderX.Size = new System.Drawing.Size(278, 45);
            this.SliderX.TabIndex = 0;
            this.SliderX.ValueChanged += new System.EventHandler(this.SliderX_change);
            // 
            // ButtonKeepIt
            // 
            this.ButtonKeepIt.Location = new System.Drawing.Point(7, 312);
            this.ButtonKeepIt.Name = "ButtonKeepIt";
            this.ButtonKeepIt.Size = new System.Drawing.Size(75, 23);
            this.ButtonKeepIt.TabIndex = 14;
            this.ButtonKeepIt.Text = "Keep it!";
            this.ButtonKeepIt.UseVisualStyleBackColor = true;
            this.ButtonKeepIt.Click += new System.EventHandler(this.ButtonKeepIt_Click);
            // 
            // ButtonCancel
            // 
            this.ButtonCancel.Location = new System.Drawing.Point(274, 312);
            this.ButtonCancel.Name = "ButtonCancel";
            this.ButtonCancel.Size = new System.Drawing.Size(75, 23);
            this.ButtonCancel.TabIndex = 15;
            this.ButtonCancel.Text = "Cancel";
            this.ButtonCancel.UseVisualStyleBackColor = true;
            this.ButtonCancel.Click += new System.EventHandler(this.ButtonCancel_Click);
            // 
            // PreviewDialogChromaKey
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(834, 362);
            this.Controls.Add(this.PanelValues);
            this.Controls.Add(this.TransformedPictureBox);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(850, 400);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(850, 400);
            this.Name = "PreviewDialogTransformation";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Image Preview (iBrand)";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.PreviewDialogTransformation_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.TransformedPictureBox)).EndInit();
            this.PanelValues.ResumeLayout(false);
            this.PanelValues.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.NumRotation)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.NumScale)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.NumY)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.NumX)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.SliderRotation)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.SliderScale)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.SliderY)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.SliderX)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private PictureBox TransformedPictureBox;
        private Panel PanelValues;
        private Button ButtonApply;
        private Label LabelRotation;
        private TrackBar SliderRotation;
        private Label LabelScale;
        private TrackBar SliderScale;
        private Label LabelY;
        private TrackBar SliderY;
        private Label LabelX;
        private TrackBar SliderX;
        private NumericUpDown NumRotation;
        private NumericUpDown NumScale;
        private NumericUpDown NumY;
        private NumericUpDown NumX;
        private Button ButtonReset;
        private Button ButtonCancel;
        private Button ButtonKeepIt;
    }
}