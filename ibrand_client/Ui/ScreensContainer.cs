using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using NomadLogic.Common.Ui;
using NomadLogic.iBrandDesktop.Util;

namespace NomadLogic.iBrandDesktop.Ui
{
    public partial class ScreensContainer : UserControl
    {
        public event EventHandler OnClose;
        public event EventHandler OnLoadLastRespondent;
        public event ChangeLanguageEventHandler OnChangeLanguage;

        private VS2008ToolStrip _toolStrip;

        private ToolStripButton _changeLanguageItem = null;

        public ScreensContainer(Questionnaire questionnaire)
        {
            InitializeComponent();

            InitToolStrip(questionnaire);

            Dock = DockStyle.Fill;

            BackColor = Color.LightGray;

            ScreenPanel.Size = new Size(WorkingArea.Size.Width, WorkingArea.Size.Height - WorkingArea.StatusStrip.Size.Height - SystemInformation.CaptionHeight);
        }

        public ToolStrip ToolStrip => _toolStrip;

        protected override void OnResize(EventArgs e)
        {
            var controls = new List<Control>();
            for (var i = 0; i < Controls.Count; i++)
            {
                controls.Add(Controls[i]);
            }

            Controls.Clear();

            base.OnResize(e);

            foreach (var t in controls)
            {
                Controls.Add(t);
            }
        }

        public new Size ClientSize => base.ClientSize;
        
        private void InitToolStrip(Questionnaire questionnaire)
        {
            _toolStrip = new VS2008ToolStrip
            {
                Location = new Point(0, 24),
                Name = "toolStrip",
                Size = new Size(628, 25)
            };

            var closeItem = new ToolStripButton();

            var closeFileName = "images" + Path.DirectorySeparatorChar + "close_button.png";
            if (File.Exists(closeFileName))
            {
                closeItem.DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;
                closeItem.Image = Image.FromFile(closeFileName);
            }
            else
            {
                closeItem.DisplayStyle = ToolStripItemDisplayStyle.Text;
            }

            closeItem.Text = "Close";
            closeItem.ImageTransparentColor = Color.White;
            closeItem.Name = "toolStripCloseButton";
            closeItem.Size = new Size(23, 22);
            closeItem.Click += OnCloseMenu;

            _toolStrip.Items.Add(closeItem);

            if (questionnaire.CanLoadLastRespondent)
            {
                var toolStripSeparator1 = new ToolStripSeparator
                {
                    Name = "toolStripSeparator1",
                    Size = new Size(6, 25)
                };

                _toolStrip.Items.Add(toolStripSeparator1);

                var loadLastItem = new ToolStripButton
                {
                    Text = "Load last completed respondent",
                    Name = "toolStripLoadLastButton",
                    Size = new Size(23, 22)
                };
                
                loadLastItem.Click += OnLoadLastMenu;

                _toolStrip.Items.Add(loadLastItem);
                
            }

            if (questionnaire.LanguagesCodeAvailiable.Length > 1)
            {
                var toolStripSeparator2 = new ToolStripSeparator
                {
                    Name = "toolStripSeparator2",
                    Size = new Size(6, 25)
                };

                _toolStrip.Items.Add(toolStripSeparator2);

                var text = "Français";
                if (questionnaire.LanguageCurrent.Equals("fr"))
                    text = "English";

                _changeLanguageItem = new ToolStripButton
                {
                    Text = text,
                    Name = "toolStripChangeLanguageButton",
                    Size = new Size(23, 22)
                };

                _changeLanguageItem.Click += OnChangeLanguageMenu;

                _toolStrip.Items.Add(_changeLanguageItem);

            }
        }
        
        private void OnCloseMenu(object sender, EventArgs e)
        {
            OnClose?.Invoke(this, EventArgs.Empty);
        }

        private void OnLoadLastMenu(object sender, EventArgs e)
        {
            OnLoadLastRespondent?.Invoke(this, EventArgs.Empty);
        }

        private void OnChangeLanguageMenu(object sender, EventArgs e)
        {
            if (_changeLanguageItem.Text.Equals("English"))
            {
                OnChangeLanguage?.Invoke("en");
                _changeLanguageItem.Text = "Français";
            }
            else {
                OnChangeLanguage?.Invoke("fr");
                _changeLanguageItem.Text = "English";
            }
        }

        public Panel ScreenPanel { get; private set; }
        
    }
}
