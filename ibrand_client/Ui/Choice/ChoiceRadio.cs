using System;
using System.Drawing;
using System.Windows.Forms;
using JetBrains.Annotations;
using NomadLogic.Common;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Util;
using NomadLogic.iBrandDesktop.Ui.Question;

namespace NomadLogic.iBrandDesktop.Ui.Choice
{
    [UsedImplicitly]
    public class ChoiceRadio : Choice
    {
        private RadioTextBox _radioButton;

        public ChoiceRadio()
        {
            Init();
        }

        public override bool Enabled
        {
            set => _radioButton.Enabled = value;
            internal get => _radioButton.Enabled;
        }

        private void Init()
        {
            _radioButton = new RadioTextBox();
            _radioButton.RadioButton.Click += OnRadioButtonClicked;
            _radioButton.RadioButton.CheckedChanged += OnResponseChanged;
        }

        public override void SetFocusable(bool focusable)
        {
            _radioButton.Enabled = focusable;
        }

        public override int AddToUserControl(Panel questionPanel, int x, int y, Graphics g, int availableWidth)
        {
            SetText();

            _radioButton.Location = new Point(x + Constants.CHOICE_MARGIN, y);

            _radioButton.Name = "C_" + No;

            var radioWidth = availableWidth - _radioButton.GetLabelLocation().X - (2 * Constants.CHOICE_MARGIN);

            var hauteurTexte = GetTextHeight(g, _radioButton.Font, radioWidth, GetText());

            _radioButton.SetSize(new Size(
                radioWidth, 
                hauteurTexte));

            questionPanel.Controls.Add(_radioButton);

            return Math.Max(hauteurTexte, _radioButton.Height) + Constants.CHOICE_PAD;
        }

        public override int GetHeight(int availableWidth, Graphics g)
        {
            var radioWidth = availableWidth - _radioButton.GetLabelLocation().X - (2 * Constants.CHOICE_MARGIN);

            var nbrOfLines = StringUtil.GetNumberOfLines(g, _radioButton.Font, radioWidth, GetText());

            return Math.Max(((int)_radioButton.Font.GetHeight() * nbrOfLines), _radioButton.Height) + Constants.CHOICE_PAD;
        }

        public override void SetText()
        {
            var text = GetText() ?? "";

            _radioButton.Text = text;
            //_radio_button.Font = new Font("Arial", 13F, FontStyle.Regular);
        }

        public bool IsSelected()
        {
            return _radioButton.Checked;
        }

        public void SetSelected(bool selected)
        {
            _radioButton.Checked = selected;
        }

        public override void ResetAnswer()
        {
            _radioButton.RadioButton.Click -= OnRadioButtonClicked;
            _radioButton.RadioButton.CheckedChanged -= OnResponseChanged;
            _radioButton.Checked = false;
            _radioButton.RadioButton.Click += OnRadioButtonClicked;
            _radioButton.RadioButton.CheckedChanged += OnResponseChanged;
        }

        public override void SetFocus()
        {
            //_radio_button.Focus();
        }

        public override bool HasFocus()
        {
            return _radioButton.Focused;
        }

        public RadioButton GetRadioButton()
        {
            return _radioButton.RadioButton;
        }

        private void OnRadioButtonClicked(object sender, EventArgs e) {
            ((QuestionRadio) _question)?.OnRadioButtonClicked(sender, e);
        }

        public override void SetAnswer(Answer answer)
        {
            if (_radioButton.InvokeRequired)
            {
                _radioButton.Invoke(new SetAnswerDelegate(SetAnswer), answer);
                return;
            }

            if(_question.No == answer.QuestionNo)
                SetSelected(answer.ChoiceNo == _no);
        }
    }
}
