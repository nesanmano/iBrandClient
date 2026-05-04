using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using NomadLogic.Common;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Serveur;

namespace NomadLogic.Test.Common.Respondent
{
    public class MockQuestionnaire : ISaveLocalRespondentData
    {

        private readonly Random _rand = new Random(DateTime.Now.Millisecond);
        private readonly IList<MockQuestionDef> _defList = new List<MockQuestionDef>();
        private int _currentRespId;
        private TerminalInfo _terminalInfo = new TerminalInfo
        {
            QuestionnaireId = 0,
            TerminalId = 0,
            ProjectId = 0,
            ProjectName = "Mock Project",
            QuestionnaireName = "Mock Questionnaire"
        };

        public MockQuestionnaire SetNumberOfQuestions(int nbr)
        {
            _defList.Clear();

            var salt = _rand.Next(0, nbr);

            for (var i = 0; i < nbr; i++)
            {
                _defList.Add(new MockQuestionDef
                {
                    Definition = $"Mock Question {salt+i}",
                    Number = salt+i,
                    QuestionType = 4 // Par défaut, 4 (texte) au début.
                });
            }

            return this;
        }

        public MockQuestionnaire SetNumberOfQuestionsAsFile(params string[] filesToInject)
        {
            var nbr = filesToInject.Length;

            if (nbr == 0) return this;

            if (nbr > _defList.Count) nbr = _defList.Count;
            var copy = _defList.ToList();

            for (var i = 0; i < nbr; i++)
            {
                var index = _rand.Next(0, copy.Count);

                var sel = copy[index];
                copy.RemoveAt(index);

                // On modifie la nature de la question.
                sel.QuestionType = 11;
                sel.Definition += " (file)";
                sel.FilePath = filesToInject[i];
            }

            return this;
        }

        public MockQuestionnaire SetNumberOfQuestionsAsOptional(int nbr)
        {
            if (nbr == 0) return this;

            if (nbr > _defList.Count) nbr = _defList.Count;
            var copy = _defList.ToList();

            for (var i = 0; i < nbr; i++)
            {
                var index = _rand.Next(0, copy.Count);

                var sel = copy[index];
                copy.RemoveAt(index);

                // On modifie la nature de la question.
                sel.Optional = true;
                sel.Definition += " (opt)";
            }

            return this;
        }

        public IList<Answer> GenerateMockAnswers()
        {
            var res = new List<Answer>();

            foreach (var qDef in _defList)
            {
                if (qDef.Optional)
                {
                    if (_rand.NextDouble() < 0.5) continue;
                }

                var answer = new Answer
                {
                    QuestionType = (QuestionTypeEnum) qDef.QuestionType,
                    QuestionNo = qDef.Number,
                    Response = qDef.IsSpecific ? qDef.SpecificTextValue : Get8CharacterRandomString(),
                    ChoiceNo = qDef.ChoiceNumber
                };

                if (qDef.FilePath != null)
                {
                    answer.Response = qDef.FilePath;
                }

                res.Add(answer);
            }

            return res;
        }

        // https://stackoverflow.com/questions/1344221/how-can-i-generate-random-alphanumeric-strings
        public string Get8CharacterRandomString()
        {
            string path = Path.GetRandomFileName();
            path = path.Replace(".", ""); // Remove period.
            return path.Substring(0, 8);  // Return 8 character string
        }

        public TerminalInfo GetTerminalInfo()
        {
            return _terminalInfo;
        }

        public void SetTerminalInfo([NotNull] TerminalInfo tdi)
        {
            _terminalInfo = tdi;
        }

        public int GetNextRespondentId()
        {
            return ++_currentRespId;
        }
        
        protected class MockQuestionDef
        {
            public string Definition { get; set; }
            public int Number { get; set; }
            public int QuestionType { get; set; }
            public bool Optional { get; set; }
            public string FilePath { get; set; }
            public bool IsSpecific { get; set; }
            public string SpecificTextValue { get; set; }
            public int ChoiceNumber { get; set; } = 1;
        }

        public MockQuestionnaire SetSpecificAnswer(Answer answer)
        {
            _defList.Add(new MockQuestionDef
            {
                QuestionType = (int)answer.QuestionType,
                IsSpecific = true,
                Optional = false,
                SpecificTextValue = answer.Response,
                Number = answer.QuestionNo,
                ChoiceNumber = answer.ChoiceNo
            });

            return this;
        }

        /// <summary>
        /// Ajoute une réponse texte à la fin de la liste existante avec une valeur spécifique.
        /// </summary>
        public MockQuestionnaire SetSpecificAnswer(string textAnswer)
        {
            var nbr = _defList.Count + 1;
            _defList.Add(new MockQuestionDef
            {
                QuestionType = 4,
                IsSpecific = true,
                Optional = false,
                SpecificTextValue = textAnswer,
                Number = nbr
            });

            return this;
        }

        public bool SaveAnswerToQuestion(QuestionIdentifier qi, string responseValue = null)
        {
            throw new NotImplementedException();
        }

        public bool SaveAnswerToQuestion(Answer answer)
        {
            throw new NotImplementedException();
        }

        public bool SaveAnswerToQuestionCheck(int questionNo, int choiceNo, bool check)
        {
            throw new NotImplementedException();
        }

        public bool SaveAnswerToQuestionCheck(QuestionIdentifier qi, bool check)
        {
            throw new NotImplementedException();
        }

        public bool SaveDynamicHeader(DynamicHeader dh)
        {
            throw new NotImplementedException();
        }

        public bool SaveDynamicHeaderCollection(ICollection<DynamicHeader> dhc)
        {
            throw new NotImplementedException();
        }

        public bool ClearDynamicHeaderCollection(params string[] dataKeyPurge)
        {
            throw new NotImplementedException();
        }

        public bool AddToFirstMatchingKeyHeader(string dataKey, DataValue dv)
        {
            throw new NotImplementedException();
        }

        public bool AddToFirstMatchingKeyHeader(string dataKey, List<DataValue> dvList)
        {
            throw new NotImplementedException();
        }
    }
}