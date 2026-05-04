using System;
using System.Text;
using JetBrains.Annotations;
using Newtonsoft.Json;
using NomadLogic.Common.Util;

namespace NomadLogic.Common.Respondent
{
    /// <summary>
    /// Cette classe représente les identifiants critiques pour une question ou réponse dans un questionnaire Nomad Logic.
    /// Le test d'égalité de cette classe (ainsi que son comparable entier) ne porte que sur les valeurs entière de QuestionNo et ChoiceNo.
    /// La propriété "QuestionType" est plus une métadonnée.
    /// </summary>
    [PublicAPI]
    public class QuestionIdentifier : IEquatable<QuestionIdentifier>, IComparable<QuestionIdentifier>
    {
        public QuestionIdentifier() { }

        public QuestionIdentifier(QuestionIdentifier qi)
        {
            if (qi == null) return;
            QuestionNo = qi.QuestionNo;
            ChoiceNo = qi.ChoiceNo;
            QuestionType = qi.QuestionType;
        }

        public QuestionIdentifier(int questionNo, int choiceNo = 1, QuestionTypeEnum type = QuestionTypeEnum.Text)
        {
            QuestionNo = questionNo;
            ChoiceNo = choiceNo;
            QuestionType = type;
        }

        [JsonRequired]
        public int QuestionNo { get; set; }

        public int ChoiceNo { get; set; } = 1;
        
        public QuestionTypeEnum QuestionType { get; set; } = QuestionTypeEnum.Text;

        #region Object Override

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.Append('q');
            sb.Append(QuestionNo);
            if (ChoiceNo == 1) return sb.ToString();
            sb.Append('c');
            sb.Append(ChoiceNo);

            return sb.ToString();
        }

        public bool Equals(QuestionIdentifier other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return QuestionNo == other.QuestionNo && ChoiceNo == other.ChoiceNo;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((QuestionIdentifier) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = QuestionNo;
                hashCode = (hashCode * 397) ^ ChoiceNo;
                return hashCode;
            }
        }

        #endregion

        #region Comparative overloads

        public static bool operator ==(QuestionIdentifier left, QuestionIdentifier right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(QuestionIdentifier left, QuestionIdentifier right)
        {
            return !Equals(left, right);
        }

        public int CompareTo(QuestionIdentifier other)
        {
            if (ReferenceEquals(this, other)) return 0;
            if (ReferenceEquals(null, other)) return 1;
            var questionNoComparison = QuestionNo.CompareTo(other.QuestionNo);
            if (questionNoComparison != 0) return questionNoComparison;
            return ChoiceNo.CompareTo(other.ChoiceNo);
        }

        #endregion

        public Answer CreateAnswer(string response)
        {
            return new Answer
            {
                QuestionIdentifier = this,
                Response = response
            };
        }

        /// <summary>
        /// Créer une nouvelle instance à partir d'un texte représentant un "jeton" d'identifiant de question. Le regex est "[qQ][0-9]+([cC][0-9]+)".
        /// S'il est impossible de créer une instance, le résultat sera nul.
        /// </summary>
        public static QuestionIdentifier FromTextDefinition(string qiStr)
        {
            return StringUtil.ConvertToQuestionIdentifier(qiStr);
        }
    }
}