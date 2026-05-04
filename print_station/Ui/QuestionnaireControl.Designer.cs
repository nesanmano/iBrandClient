using System.ComponentModel;

namespace NomadLogic.PrintStation.Ui
{
    partial class QuestionnaireControl
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
            this.ScreenPanel = new System.Windows.Forms.Panel();
            this.SuspendLayout();
            // 
            // questionnaire_panel
            // 
            this.ScreenPanel.Location = new System.Drawing.Point(0, 0);
            this.ScreenPanel.Name = "ScreenPanel";
            this.ScreenPanel.Size = new System.Drawing.Size(200, 100);
            this.ScreenPanel.TabIndex = 0;
            // 
            // QuestionnaireControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.ScreenPanel);
            this.Name = "QuestionnaireControl";
            this.Size = new System.Drawing.Size(338, 315);
            this.ResumeLayout(false);

        }

        #endregion
    }
}
