using System.ComponentModel;
using System.Windows.Forms;

namespace NomadLogic.iBrandDesktop.Ui
{
    partial class VideoSelectionDialog
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
            var resources = new System.ComponentModel.ComponentResourceManager(typeof(VideoSelectionDialog));
            this.picture_box = new System.Windows.Forms.PictureBox();
            this.previous_button = new System.Windows.Forms.Button();
            this.next_button = new System.Windows.Forms.Button();
            this.select_button = new System.Windows.Forms.Button();
            this.buttons_panel = new System.Windows.Forms.Panel();
            ((System.ComponentModel.ISupportInitialize)(this.picture_box)).BeginInit();
            this.buttons_panel.SuspendLayout();
            this.SuspendLayout();
            // 
            // picture_box
            // 
            this.picture_box.Location = new System.Drawing.Point(13, 5);
            this.picture_box.Name = "picture_box";
            this.picture_box.Size = new System.Drawing.Size(460, 328);
            this.picture_box.TabIndex = 0;
            this.picture_box.TabStop = false;
            // 
            // previous_button
            // 
            this.previous_button.Location = new System.Drawing.Point(101, 3);
            this.previous_button.Name = "previous_button";
            this.previous_button.Size = new System.Drawing.Size(75, 23);
            this.previous_button.TabIndex = 1;
            this.previous_button.Text = "Previous";
            this.previous_button.UseVisualStyleBackColor = true;
            // 
            // next_button
            // 
            this.next_button.Location = new System.Drawing.Point(281, 3);
            this.next_button.Name = "next_button";
            this.next_button.Size = new System.Drawing.Size(75, 23);
            this.next_button.TabIndex = 2;
            this.next_button.Text = "Next";
            this.next_button.UseVisualStyleBackColor = true;
            // 
            // select_button
            // 
            this.select_button.Location = new System.Drawing.Point(191, 2);
            this.select_button.Name = "select_button";
            this.select_button.Size = new System.Drawing.Size(75, 23);
            this.select_button.TabIndex = 3;
            this.select_button.Text = "Select";
            this.select_button.UseVisualStyleBackColor = true;
            // 
            // buttons_panel
            // 
            this.buttons_panel.Controls.Add(this.previous_button);
            this.buttons_panel.Controls.Add(this.select_button);
            this.buttons_panel.Controls.Add(this.next_button);
            this.buttons_panel.Location = new System.Drawing.Point(13, 339);
            this.buttons_panel.Name = "buttons_panel";
            this.buttons_panel.Size = new System.Drawing.Size(460, 42);
            this.buttons_panel.TabIndex = 4;
            // 
            // VideoSelectionDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(485, 393);
            this.Controls.Add(this.buttons_panel);
            this.Controls.Add(this.picture_box);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "VideoSelectionDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "iBrand(tm)";
            ((System.ComponentModel.ISupportInitialize)(this.picture_box)).EndInit();
            this.buttons_panel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private PictureBox picture_box;
        private Button previous_button;
        private Button next_button;
        private Button select_button;
        private Panel buttons_panel;
    }
}