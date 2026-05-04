using System.Collections.Generic;
using NomadLogic.Common;
using NomadLogic.Common.Condition;
using NomadLogic.Common.Respondent;
using NomadLogic.iBrandDesktop.Ui.Question;
using NomadLogic.Test.Common.Respondent;
using NUnit.Framework;

namespace NomadLogic.Test.Common.Condition
{
    /// Classe de test sur la classe QuestionCondition
    public class TestQuestionCondition : TestBaseNunit
    {
        [Test]
        public void EqualityTests()
        {
            var c1 = new QuestionCondition
            {
                QuestionNo = 1,
                ChoiceNo = 1
            };
            
            var c2 = new QuestionCondition
            {
                QuestionNo = 1,
                ChoiceNo = 1
            };

            var c3 = new QuestionCondition
            {
                QuestionNo = 1,
                ChoiceNo = 2
            };

            var c4 = new QuestionCondition
            {
                QuestionNo = 2,
                ChoiceNo = 1
            };
            
            Assert.AreNotSame(c1, c2);
            
            Assert.AreEqual(c1, c1);
            Assert.AreEqual(c1, c2);
            Assert.AreEqual(c2, c1);
            
            Assert.AreNotEqual(c1, c3);
            Assert.AreNotEqual(c1, c4);
            Assert.AreNotEqual(c3, c4);
            Assert.AreNotEqual(c3, c2);
        }
        
        [Test]
        public void BasicEvaluation()
        {
            var resp = new MockRespondent
            {
                MockAnswer = new List<Answer>
                {
                    new Answer //(QuestionTypeEnum.Text, 1, 1,"some text")
                    {
                        QuestionType = QuestionTypeEnum.Text,
                        QuestionNo = 1,
                        ChoiceNo = 1,
                        Response = "some text"
                    }
                }
            };

            var condition = new QuestionCondition
            {
                QuestionNo = 1,
                ChoiceNo = 1
            };
            Assert.IsTrue(condition.Evaluate(resp), "Should be true. Answers contains a text question 1-1");
            condition.Not = true;
            Assert.IsFalse(condition.Evaluate(resp), "Should be false. Question 1-1 exists, but is negated");
        }
        
        [Test]
        public void EvaluationForTextQuestion()
        {
            // Fill answer
            var resp = new MockRespondent
            {
                MockAnswer = new List<Answer>
                {
                    new Answer //(QuestionTypeEnum.Text, 1, 1, -1, "some text"),
                    {
                        QuestionNo = 1,
                        ChoiceNo = 1,
                        Response = "some text"
                    },
                    new Answer //(QuestionTypeEnum.Text, 1, 2, -1, ""),
                    {
                        QuestionNo = 1,
                        ChoiceNo = 2,
                        Response = ""
                    },
                    new Answer //(QuestionTypeEnum.Text, 1, 3, -1, null)
                    {
                        QuestionNo = 1,
                        ChoiceNo = 3
                    }
                }
            };

            var condition1 = new QuestionCondition
            {
                QuestionNo = 1,
                ChoiceNo = 1
            };
            Assert.IsTrue(condition1.Evaluate(resp), "Should be true. Answers contains a text question 1-1");
            condition1.Not = true;
            Assert.IsFalse(condition1.Evaluate(resp), "Should be false. Question 1-1 exists, but is negated");
            
            var condition2 = new QuestionCondition
            {
                QuestionNo = 1,
                ChoiceNo = 2
            };
            Assert.IsTrue(condition2.Evaluate(resp), "Should be true. Answers contains a text question 1-2");
            condition2.Not = true;
            Assert.IsFalse(condition2.Evaluate(resp), "Should be false. Question 1-2 exists, but is negated");
            
            var condition3 = new QuestionCondition
            {
                QuestionNo = 1,
                ChoiceNo = 3
            };
            Assert.IsTrue(condition3.Evaluate(resp), "Should be true. Answers contains a text question 1-3");
            condition3.Not = true;
            Assert.IsFalse(condition3.Evaluate(resp), "Should be false. Question 1-3 exists, but is negated");
        }
        
        [Test]
        public void EvaluationForCheckQuestion()
        {
            // Fill answer
            var resp = new MockRespondent
            {
                MockAnswer = new List<Answer>
                {
                    new Answer //(QuestionTypeEnum.Check, 1, 1),
                    {
                        QuestionType = QuestionTypeEnum.Check,
                        QuestionNo = 1
                    },
                    new Answer //(QuestionTypeEnum.Check, 2, 1)
                    {
                        QuestionType = QuestionTypeEnum.Check,
                        QuestionNo = 2
                    }
                }
            };

            var condition1 = new QuestionCondition
            {
                QuestionNo = 1
            };
            Assert.IsTrue(condition1.Evaluate(resp), "Should be true. Answers contains a question 1");
            condition1.Not = true;
            Assert.IsFalse(condition1.Evaluate(resp), "Should be false. Question 1-1 exists, but is negated");
            
            var condition2 = new QuestionCondition
            {
                QuestionNo = 2
            };
            Assert.IsTrue(condition2.Evaluate(resp), "Should be true. Answers contains a question 2");
            condition2.Not = true;
            Assert.IsFalse(condition2.Evaluate(resp), "Should be false. Question 1-2 exists, but is negated");
            
            var condition3 = new QuestionCondition
            {
                QuestionNo = 3
            };
            Assert.IsFalse(condition3.Evaluate(resp), "Should be false. Answers not contains a question 3");
            condition3.Not = true;
            Assert.IsTrue(condition3.Evaluate(resp), "Should be true. Question 1-3 does not exists, but is negated");
        }
        
        [Test]
        public void EvaluationForComboQuestion()
        {
            // Fill answer
            var resp = new MockRespondent
            {
                MockAnswer = new List<Answer>
                {
                    new Answer //(QuestionTypeEnum.Combo, 1, 1, 1),
                    {
                        QuestionType = QuestionTypeEnum.Combo,
                        QuestionNo = 1,
                        ChoiceNo = 1
                    },
                    new Answer //(QuestionTypeEnum.Combo, 2, 1, 2),
                    {
                        QuestionType = QuestionTypeEnum.Combo,
                        QuestionNo = 2,
                        ChoiceNo = 2
                    },
                    new Answer //(QuestionTypeEnum.Combo, 3, 1, 3)
                    {
                        QuestionType = QuestionTypeEnum.Combo,
                        QuestionNo = 3,
                        ChoiceNo = 3
                    }
                }
            };

            var condition1 = new QuestionCondition
            {
                QuestionNo = 1,
                ChoiceNo = 1
            };
            Assert.IsTrue(condition1.Evaluate(resp), "Should be true. Answers contains a question 1");
            condition1.Not = true;
            Assert.IsFalse(condition1.Evaluate(resp), "Should be false. Question 1 exists, but is negated");
            
            var condition2 = new QuestionCondition
            {
                QuestionNo = 2,
                ChoiceNo = 2
            };
            Assert.IsTrue(condition2.Evaluate(resp), "Should be true. Answers contains a question 2");
            condition2.Not = true;
            Assert.IsFalse(condition2.Evaluate(resp), "Should be false. Question 2 exists, but is negated");
            
            var condition3 = new QuestionCondition
            {
                QuestionNo = 3,
                ChoiceNo = 3
            };
            Assert.IsTrue(condition3.Evaluate(resp), "Should be true. Answers contains a question 3");
            condition3.Not = true;
            Assert.IsFalse(condition3.Evaluate(resp), "Should be false. Question 3 exists, but is negated");
            
            var condition4 = new QuestionCondition
            {
                QuestionNo = 3,
                ChoiceNo = 2
            };
            Assert.IsFalse(condition4.Evaluate(resp), "Should be false. Answers don't contain a question 3-2");
            condition4.Not = true;
            Assert.IsTrue(condition4.Evaluate(resp), "Should be True. Question 3-2 should not exists, but is negated");
        }
        
    }
}