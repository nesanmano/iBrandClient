using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using NomadLogic.Common;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Util;
using NomadLogic.iBrandDesktop.Util.BarcodeScanner;

namespace NomadLogic.iBrandDesktop.Ui.Choice
{
    public class ChoiceText : Choice, IBarcodeScannerListener
    {
        private readonly TextBox _textField;
        private Label _textLabel;

        private bool _appendBarCode;

        public ChoiceText()
        {
            _textField = new TextBox();

            _textField.TextChanged += OnResponseChanged;
            _textField.MaxLength = 256;//default value
        }

        public override bool Enabled
        {
            set
            {
                _textField.Enabled = value;
                
                if (_textLabel != null)
                    _textLabel.Enabled = value;
            }

            internal get => _textField.Enabled;
        } 

        public override int AddToUserControl(Panel questionPanel, int x, int y, Graphics g, int availableWidth)
        {
            int textFieldWidth;
            // IF text in textChoice, then put a label before.
            if (GetText() != null && GetText().Length > 0)
            {
                _textLabel = new Label
                {
                    Font = _question.Font,
                    Text = GetText(),
                    Name = "TC_" + No
                };

                var textLabelWidth = Math.Min(StringUtil.GetStringWidth(g, _textLabel.Font, GetText()), availableWidth - (3 * Constants.CHOICE_MARGIN) / 2);

                _textLabel.Size = new Size(
                    textLabelWidth, 
                    GetTextHeight(g, _textLabel.Font, textLabelWidth, GetText()));

                if (_textLabel.Size.Height < _textField.Size.Height)
                {
                    var topSpace = (_textField.Size.Height - _textLabel.Size.Height) / 2;
                    _textLabel.Location = new Point(x + Constants.CHOICE_MARGIN, y + topSpace);
                }
                else
                {
                    _textLabel.Location = new Point(x + Constants.CHOICE_MARGIN, y);
                }

                questionPanel.Controls.Add(_textLabel);

                x = x + textLabelWidth + Constants.CHOICE_MARGIN;

                textFieldWidth = (availableWidth - (3 * Constants.CHOICE_MARGIN)) - textLabelWidth;
            }
            else
            {
                textFieldWidth = availableWidth - (2 * Constants.CHOICE_MARGIN);
            }

            if (textFieldWidth > Constants.MAX_WIDTH)
                textFieldWidth = Constants.MAX_WIDTH;

            if (_maxLength != null)
            {
                _textField.MaxLength = int.Parse(_maxLength);
            }            
            
            _textField.Name = "C_" + No;

            _textField.Size = new Size(textFieldWidth, _textField.Size.Height);
            
            if (_textLabel != null && _textLabel.Size.Height > _textField.Size.Height)
                y = y + ((_textLabel.Size.Height - _textField.Size.Height) / 2);
            
            _textField.Location = new Point(x + Constants.CHOICE_MARGIN, y);
            
            questionPanel.Controls.Add(_textField);

            if (_textLabel != null)
                return Math.Max(_textField.Size.Height, _textLabel.Size.Height) + Constants.CHOICE_PAD;
            return _textField.Size.Height + Constants.CHOICE_PAD;
        }

        public override int GetHeight(int availableWidth, Graphics g)
        {
            if (GetText() == null || GetText().Length <= 0) return _textField.Size.Height + Constants.CHOICE_PAD;
            var textLabelWidth = Math.Min(StringUtil.GetStringWidth(g, _question.Font, GetText()),
                availableWidth - (4 * Constants.CHOICE_MARGIN) / 2);

            var nbrOfLines = StringUtil.GetNumberOfLines(g, _question.Font, textLabelWidth, GetText());

            var height = nbrOfLines * _textLabel.Font.GetHeight();

            var toAdd = nbrOfLines - 1;

            return Math.Max(_textField.Size.Height, (int)Math.Ceiling(height) + toAdd) + Constants.CHOICE_PAD;
        }

        public bool Focusable
        {
            set
            {
                _textField.Enabled = value;
                if (_textLabel != null)
                    _textLabel.Enabled = value;
            }
        }

        public bool AppendBarCode
        {
            set => _appendBarCode = value;
        }

        public bool BarCodeActivated { get; set; }

        public override void SetFocusable(bool focusable)
        {
            if (!_enabledDefault) return;
            _textField.Enabled = focusable;

            _textField.BackColor = focusable ? Color.White : Color.LightGray;
        }

        public override void SetAnswer(Answer answer)
        {
            if (_textField.InvokeRequired)
            {
                _textField.Invoke(new SetAnswerDelegate(SetAnswer), answer);
                return;
            }
            
            _textField.Text = answer.Response;
        }

        public override bool IsAnswered()
        {
            return _textField.Text.Length > 0;
        }

        public override string GetAnswerText()
        {
            var answer = _textField.Text;

            if (answer.Length == 0)
                return answer;

            if (_prefix != null)
                answer = _prefix + answer;

            if (_suffix != null)
                answer = answer + _suffix;

            if (_titleCase)
                answer = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(answer);

            return answer;
        }

        public override void ResetAnswer()
        {
            _textField.TextChanged -= OnResponseChanged;

            _textField.Text = "";

            AutoPopulateAction?.Invoke(); // Va changer le texte automatiquement hors des événements normalement associés.

            CheckAndUpdateDefaultAnswer();

            _textField.TextChanged += OnResponseChanged;

            Enabled = _enabledDefault;
        }

        public override void SetFocus()
        {
            _textField.Focus();
        }

        public override bool HasFocus()
        {
            return _textField.Focused;
        }

        public override void SetText()
        {
            if (_textLabel == null) return;
            var text = GetText() ?? "";

            _textLabel.Text = text;
        }

        public void Scanner_Read(object sender, ScanEventArgs args)
        {
            if (_textField.InvokeRequired)
            {
                _textField.Invoke(new Action(() => { Scanner_Read(sender, args); }));
                return;
            }

            if (args == null)
            {
                return;
            }

            var barcode = args.BarCode;
            
            if (_appendBarCode)
            {
                if (_textField.Text.IndexOf(barcode, StringComparison.Ordinal) == -1)
                {
                    if (_textField.Text.Length > 0)
                        barcode = _textField.Text + ":" + barcode;
                }
                else
                {
                    barcode = _textField.Text;
                }
            }

            _textField.Text = barcode;
        }
    }
   
}
