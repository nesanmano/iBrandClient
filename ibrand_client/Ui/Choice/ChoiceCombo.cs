using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using JetBrains.Annotations;
using NLog;
using NomadLogic.Common.Respondent;
using NomadLogic.iBrandDesktop.Datasource;
using NomadLogic.iBrandDesktop.Ui.Question;

namespace NomadLogic.iBrandDesktop.Ui.Choice
{
    public class ChoiceCombo : Choice
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private QuestionComboItem _assignedComboItem;
        private QuestionCombo _refQuestion; //  Pour éviter de faire un cast à tout bout de champ...
        
        [PublicAPI]
        public ChoiceCombo() { }

        public override Question.Question Question
        {
            set
            {
                if (!(value is QuestionCombo qc)) throw new System.Exception("Can only assign a QuestionCombo to a ChoiceCombo.");
                _question = qc;
                _refQuestion = qc;
            }

            get => _question;
        }

        public void AssignComboItem(QuestionComboItem tmp)
        {
            if (tmp == null) return;
            _assignedComboItem = tmp;
            _assignedComboItem.AssignedChoiceNo = No;
            foreach (var t in _textTable)
            {
                _assignedComboItem.SetText(t.Key, t.Value);
            }
        }
        
        public override bool Enabled
        {
            set => _refQuestion?.SetEnabledStatus(value);

            internal get => _refQuestion?.GetEnabledStatus() ?? false;
        }

        public override void SetFocusable(bool focusable)
        {
            Enabled = focusable;
        }
        
        public override string Language
        {
            set
            {
                _activeLanguage = value;
                _refQuestion?.SetLanguage(value, _assignedComboItem);
            }
        }
        
        [Obsolete]
        public void AddSubChoicesFromDatasource(AbstractDatasource datasource, string fieldName, string language)
        {
            throw new NotImplementedException("Datasourcing a combo box must be re-done with a better format.");
        }

        [Obsolete]
        public void AddSubChoices(string language, IEnumerable<string> values)
        {
            throw new NotImplementedException("Sub-Choice do not exist anymore...");
        }

        public override int AddToUserControl(Panel questionPanel, int x, int y, Graphics g, int availableWidth)
        {
            return 0;
        }

        public override int GetHeight(int availableWidth, Graphics g)
        {
            return 0;
        }

        public override void SetText()
        {
            // Rien
        }
        
        public override void SetAnswer([NotNull] Answer answer)
        {
            _refQuestion?.SetAnswer(answer);
        }
        
        public override string GetAnswerText()
        {
            if (_refQuestion.GetCurrentSelectionIndex() != No) return null;
            var txt = _assignedComboItem?.ToString();
            log.Trace("Choice returned text value: " + txt);
            return txt;
        }

        public override bool IsAnswered()
        {
            return _refQuestion.GetCurrentSelectionIndex() == No;
        }
        
        public override void ResetAnswer()
        {
            // rien.
        }
        
        public override void SetFocus()
        {
            // Rien
        }

        public override bool HasFocus()
        {
            return _refQuestion.HasFocus();
        }
    }
}
