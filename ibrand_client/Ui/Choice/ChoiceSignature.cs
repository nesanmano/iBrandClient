using System;
using System.Drawing;
using System.Windows.Forms;
using JetBrains.Annotations;
using NomadLogic.Common;
using NomadLogic.Common.Respondent;
using NomadLogic.iBrandDesktop.Util;

namespace NomadLogic.iBrandDesktop.Ui.Choice
{
    public class ChoiceSignature : Choice
    {
        private SignatureControl _signatureControl;

        public SignatureControl SignatureUiControl {
            get => _signatureControl;
            set => _signatureControl = value;
        }

        [PublicAPI]
        public ChoiceSignature() { } // Doit exister pour création par réflexion.

        public override bool Enabled
        {
            set
            {
                _signatureControl.Enabled = value;
            }

            internal get => _signatureControl.Enabled;
        }

        public override int AddToUserControl(Panel questionPanel, int x, int y, Graphics g, int availableWidth)
        {
            _signatureControl.Location = new Point(x + Constants.CHOICE_MARGIN, y);

            _signatureControl.Name = "C_" + No;

            _signatureControl.Size = new Size(availableWidth - (2 * Constants.CHOICE_MARGIN), _signatureControl.Size.Height);

            questionPanel.Controls.Add(_signatureControl);

            return _signatureControl.Size.Height + Constants.CHOICE_PAD; ;
        }

        public override int GetHeight(int availableWidth, Graphics g)
        {
            return _signatureControl.Size.Height + Constants.CHOICE_PAD; ;
        }

        public override bool IsAnswered()
        {
            return _signatureControl.IsAnswered();
        }

        public override string GetAnswerText()
        {
            return _signatureControl.SaveSignatureToDisk();
        }

        public override void ResetAnswer()
        {
            _signatureControl.Reset();
            CheckAndUpdateDefaultAnswer();
        }

        public override void SetFocus()
        {
            _signatureControl.Focus();
        }

        public override bool HasFocus()
        {
            return _signatureControl.Focused;
        }

        public override void SetText()
        {
            //do nothing
        }

        public override void SetAnswer(Answer answer)
        {
            throw new NotImplementedException();
        }

        public override void SetFocusable(bool focusable)
        {
            if (!focusable)
                ResetAnswer();

            _signatureControl.Enabled = focusable;
        }
    }
}
