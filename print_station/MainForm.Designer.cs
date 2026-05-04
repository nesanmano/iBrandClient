using System.ComponentModel;
using System.Windows.Forms;
using NomadLogic.Common.Ui;

namespace NomadLogic.PrintStation
{
    partial class MainForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.main_panel = new System.Windows.Forms.Panel();
            this.tool_strip = new VS2008ToolStrip();
            this.exit_button = new System.Windows.Forms.ToolStripButton();
            this.logout_button = new System.Windows.Forms.ToolStripButton();
            this.configuration_button = new System.Windows.Forms.ToolStripButton();
            this.full_screen_button = new System.Windows.Forms.ToolStripButton();
            this.clear_button = new System.Windows.Forms.ToolStripButton();
            this.main_panel.SuspendLayout();
            this.tool_strip.SuspendLayout();
            this.SuspendLayout();
            // 
            // main_panel
            // 
            this.main_panel.BackColor = System.Drawing.Color.White;
            this.main_panel.Controls.Add(this.tool_strip);
            this.main_panel.Location = new System.Drawing.Point(0, 0);
            this.main_panel.Name = "main_panel";
            this.main_panel.Size = new System.Drawing.Size(1034, 535);
            this.main_panel.TabIndex = 0;
            // 
            // tool_strip
            // 
            this.tool_strip.AutoSize = false;
            this.tool_strip.Font = new System.Drawing.Font("Segoe UI", 21.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tool_strip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exit_button,
            this.logout_button,
            this.configuration_button,
            this.full_screen_button,
            this.clear_button});
            this.tool_strip.Location = new System.Drawing.Point(0, 0);
            this.tool_strip.Name = "tool_strip";
            this.tool_strip.Size = new System.Drawing.Size(1034, 60);
            this.tool_strip.TabIndex = 0;
            this.tool_strip.Text = "vS2008ToolStrip1";
            this.tool_strip.Visible = false;
            // 
            // exit_button
            // 
            this.exit_button.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.exit_button.Font = new System.Drawing.Font("Segoe UI", 21.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.exit_button.Image = ((System.Drawing.Image)(resources.GetObject("exit_button.Image")));
            this.exit_button.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.exit_button.Name = "exit_button";
            this.exit_button.Size = new System.Drawing.Size(66, 57);
            this.exit_button.Text = "Exit";
            // 
            // logout_button
            // 
            this.logout_button.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.logout_button.Font = new System.Drawing.Font("Segoe UI", 21.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.logout_button.Image = ((System.Drawing.Image)(resources.GetObject("logout_button.Image")));
            this.logout_button.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.logout_button.Name = "logout_button";
            this.logout_button.Size = new System.Drawing.Size(120, 57);
            this.logout_button.Text = "Log out";
            // 
            // configuration_button
            // 
            this.configuration_button.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.configuration_button.Font = new System.Drawing.Font("Segoe UI", 21.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.configuration_button.Image = ((System.Drawing.Image)(resources.GetObject("configuration_button.Image")));
            this.configuration_button.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.configuration_button.Name = "configuration_button";
            this.configuration_button.Size = new System.Drawing.Size(196, 57);
            this.configuration_button.Text = "Configuration";
            // 
            // full_screen_button
            // 
            this.full_screen_button.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.full_screen_button.Font = new System.Drawing.Font("Segoe UI", 21.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.full_screen_button.Image = ((System.Drawing.Image)(resources.GetObject("full_screen_button.Image")));
            this.full_screen_button.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.full_screen_button.Name = "full_screen_button";
            this.full_screen_button.Size = new System.Drawing.Size(157, 57);
            this.full_screen_button.Text = "Full Screen";
            // 
            // clear_button
            // 
            this.clear_button.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.clear_button.Font = new System.Drawing.Font("Segoe UI", 21.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.clear_button.Image = ((System.Drawing.Image)(resources.GetObject("clear_button.Image")));
            this.clear_button.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.clear_button.Name = "clear_button";
            this.clear_button.Size = new System.Drawing.Size(86, 57);
            this.clear_button.Text = "Clear";
            this.clear_button.Visible = true;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1034, 527);
            this.Controls.Add(this.main_panel);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "LeNomad";
            this.main_panel.ResumeLayout(false);
            this.tool_strip.ResumeLayout(false);
            this.tool_strip.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private Panel main_panel;
        private VS2008ToolStrip tool_strip;
        private ToolStripButton logout_button;
        private ToolStripButton exit_button;
        private ToolStripButton configuration_button;
        private ToolStripButton full_screen_button;
        private ToolStripButton clear_button;
    }
}

