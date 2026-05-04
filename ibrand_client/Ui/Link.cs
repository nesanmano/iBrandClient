using System;
using System.Collections;
using System.Drawing;
using System.Windows.Forms;
using NomadLogic.Common;
using NomadLogic.Common.Services;
using NomadLogic.Common.Services.Configuration;
using NomadLogic.Common.Ui;
using NomadLogic.Common.Util;

namespace NomadLogic.iBrandDesktop.Ui
{
    public class Link
    {
        protected Hashtable _text_table;
        protected string _active_language;

        PopupDlg _popupDlg;
        HtmlPopupDlg _htmlPopupDlg;

        private readonly LinkLabel _label;

        public Link()
        {
            _text_table = new Hashtable();
            _popupDlg = null;
            _label = new LinkLabel();
            _label.Click += LinkLabelClick;
        }

        private void LinkLabelClick(object sender, EventArgs e)
        {
            if (_popupDlg != null)
            {
                _popupDlg.SetText();
                _popupDlg.ShowDialog();
            }
            else if (_htmlPopupDlg != null)
            {
                _htmlPopupDlg.SetHtml();
                _htmlPopupDlg.ShowDialog();
            }
        }

        public string Language
        {
            set
            {
                _active_language = value;

                if (_popupDlg != null)
                    _popupDlg.Language = value;
            }
        }

        public void SetPopUpDlg(PopupDlg popupDlg)
        {
            _popupDlg = popupDlg;
        }

        public void SetPopUpDlg(HtmlPopupDlg htmlPopupDlg)
        {
            _htmlPopupDlg = htmlPopupDlg;
        }

        public void SetText(string language, string text)
        {
            _text_table.Add(language, text);
        }

        public void SetText()
        {
            var text = GetText() ?? "";

            _label.Text = text;
        }

        public int AddToUserControl(Panel questionPanel, int x, int y, Graphics g, int availableWidth)
        {
            SetText();

            _label.Location = new Point(x + Constants.CHOICE_MARGIN, y);

            var labelWidth = availableWidth - _label.Size.Width - (2 * Constants.CHOICE_MARGIN);

            var nbrOfLines = StringUtil.GetNumberOfLines(g, _label.Font, labelWidth, GetText());

            _label.Size = new Size(labelWidth, (int)_label.Font.GetHeight() * nbrOfLines);

            questionPanel.Controls.Add(_label);

            return Math.Max((int)_label.Font.GetHeight() * nbrOfLines, _label.Height) + Constants.CHOICE_PAD;
        }

        public int GetHeight(int availableWidth, Graphics g)
        {
            var labelWidth = availableWidth - _label.Size.Width - (2 * Constants.CHOICE_MARGIN);

            var nbrOfLines = StringUtil.GetNumberOfLines(g, _label.Font, labelWidth, GetText());

            return Math.Max(((int)_label.Font.GetHeight() * nbrOfLines), _label.Height) + Constants.CHOICE_PAD;
        }

        public string GetText()
        {
            if (_active_language == null)
            {
                var config = ServicesContainer.GetService<NomadConfiguration>();
                _active_language = config.PreferedLanguage;
            }

            var text = (string)_text_table[_active_language];

            if (text == null)
            {
                //Get the first supported language which we have text for.
                foreach (var lg in Constants.SUPPORTED_LANGUAGES)
                {
                    text = (string)_text_table[lg];

                    if (text != null)
                        return text;
                }

                return "";
            }

            return text;
        }
    }
}
