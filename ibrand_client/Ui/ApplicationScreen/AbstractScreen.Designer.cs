using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen
{
    partial class AbstractScreen
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
            this.PictureBox = new PictureBox();
            ((ISupportInitialize)(this.PictureBox)).BeginInit();
            this.SuspendLayout();
            // 
            // pictureBox
            // 
            this.PictureBox.BackColor = Color.White;
            this.PictureBox.Location = new Point(0, 0);
            this.PictureBox.Margin = new Padding(4, 4, 4, 4);
            this.PictureBox.Name = "PictureBox";
            this.PictureBox.Size = new Size(504, 446);
            this.PictureBox.TabIndex = 0;
            this.PictureBox.TabStop = false;
            // 
            // AbstractScreen
            // 
            this.AutoScaleDimensions = new SizeF(8F, 16F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.BackColor = Color.Gray;
            this.Controls.Add(this.PictureBox);
            this.Margin = new Padding(4, 4, 4, 4);
            this.Name = "AbstractScreen";
            this.Size = new Size(727, 569);
            ((ISupportInitialize)(this.PictureBox)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
    }
}
