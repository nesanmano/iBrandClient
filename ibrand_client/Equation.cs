using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NomadLogic.iBrandDesktop.Ui.Choice;

namespace NomadLogic.iBrandDesktop
{
    public class Equation
    {
        private readonly List<Operand> _operandList;

        private bool _focusableState;
        private bool _mandatoryState;
        private bool _enabledState;

        private readonly string _operator;

        private readonly Questionnaire _questionnaire;

        public Equation(string state, string operatorArg, Questionnaire questionnaire)
        {
            InitState(state);
            _operator = operatorArg ?? "or";
            _operandList = new List<Operand>();

            _questionnaire = questionnaire;
        }

        private void InitState(string state)
        {
            if (state.IndexOf("focusable", StringComparison.InvariantCulture) != -1)
                _focusableState = true;

            if (state.IndexOf("mandatory", StringComparison.InvariantCulture) != -1)
                _mandatoryState = true;

            if (state.IndexOf("enabled", StringComparison.InvariantCulture) != -1)
                _enabledState = true;
        }

        public bool HasMandatoryState()
        {
            return _mandatoryState;
        }

        public bool HasFocusableState()
        {
            return _focusableState;
        }

        public bool HasEnabledState()
        {
            return _enabledState;
        }

        public void AddOperand(Operand operand)
        {
            _operandList.Add(operand);
        }

        public Choice GetFirstOperandChoice()
        {
            return _operandList.Select(o =>
            {
                var question = _questionnaire.GetQuestion(o.QuestionNoInt);
                return question.GetChoice(o.ChoiceNo);
            }).First();
        }

        public bool Evaluate()
        {
            IEnumerator operands = _operandList.GetEnumerator();

            // On assume vrai tant que l'on ne falsifie pas.
            var conditionMet = true;

            while (operands.MoveNext() && operands.Current is Operand operand)
            {
                var question = _questionnaire.GetQuestion(operand.QuestionNoInt);
                
                if (!operand.Evaluate(question))
                {
                    conditionMet = false;
                    if (_operator.Equals("and"))
                        break;
                }
                else
                {
                    if (!_operator.Equals("or")) continue;
                    conditionMet = true;
                    break;
                }
            }

            return conditionMet;
        }
    }
}
