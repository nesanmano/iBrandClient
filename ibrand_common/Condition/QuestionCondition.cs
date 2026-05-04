using System;
using JetBrains.Annotations;
using Newtonsoft.Json;
using NLog;
using NomadLogic.Common.Respondent;

// ReSharper disable NonReadonlyMemberInGetHashCode

namespace NomadLogic.Common.Condition
{
    /// <summary>
    /// Condition basée sur les identifiants (nombres) d'une question, son choix, et son sous-choix.
    /// Ne teste pas la valeur texte de la réponse, juste l'existence de la réponse.
    /// On peut inverser le résultat de l'évaluation booléene, pour rendre vrai la non-existence d'une réponse.
    /// </summary>
    [PublicAPI]
    public class QuestionCondition : ICondition, IEquatable<QuestionCondition>, IComparable<QuestionCondition>
    {
        // ReSharper disable once InconsistentNaming
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Constructeur principal.
        /// </summary>
        /// <param name="questionNo">Numéro de question.</param>
        /// <param name="choiceNo">Numéro de choix. 1 par défaut.</param>
        /// <param name="not">Inverse le résultat booléen de l'évaluation. Faux par défaut.</param>
        public QuestionCondition(int questionNo, int choiceNo = 1, bool not = false)
        {
            QuestionNo = questionNo;
            ChoiceNo = choiceNo;
            Not = not;
        }

        public QuestionCondition(QuestionIdentifier qi, bool not = false)
        {
            QuestionNo = qi.QuestionNo;
            ChoiceNo = qi.ChoiceNo;
            Not = not;
        }

        public QuestionCondition() { }

        /// Numéro de question. -1 par défaut, ce qui est toujours faux.
        public int QuestionNo { get; set; } = -1;

        /// Numéro du choix. 1 par défaut.
        public int ChoiceNo { get; set; } = 1;
        
        /// <summary>
        /// Indique qu'il faut inverser le résultat de la vérification booléenne.
        /// Est utile dans les cas où il faut tester que la question n'est PAS répondue comme vrai.
        /// On active l'inversion en mettant cette propriété à vrai (je sais, ce n'est pas très intuitif).
        /// </summary>
        public bool Not { get; set; }

        #region Overrides of Object

        public int CompareTo(ICondition other)
        {
            if (!(other is QuestionCondition qc)) return 1;

            var res = QuestionNo.CompareTo(qc.QuestionNo);
            if (res != 0) return res;

            return ChoiceNo.CompareTo(qc.ChoiceNo);
        }

        public override string ToString()
        {
            return $"[QuestionCondition:qNo={QuestionNo},cNo={ChoiceNo},Not={Not}]";
        }
        
        /// <inheritdoc />
        public int CompareTo(QuestionCondition other) {
            if (other == null) return 1;
            if (QuestionNo != other.QuestionNo) return QuestionNo - other.QuestionNo;
            if (ChoiceNo != other.ChoiceNo) return ChoiceNo - other.ChoiceNo;
            return (Not ? 0 : 1) - (other.Not ? 0 : 1);
        }

        /// <inheritdoc />
        public override bool Equals(object obj) {
            var args = obj as QuestionCondition;
            return Equals(args);
        }

        #endregion

        #region Equality members

        /// <inheritdoc />
        public bool Equals(QuestionCondition other) {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return QuestionNo == other.QuestionNo && 
                   ChoiceNo == other.ChoiceNo && 
                   Not == other.Not;
        }

        /// <inheritdoc />
        public override int GetHashCode() {
            unchecked {
                var hashCode = QuestionNo;
                hashCode = (hashCode * 397) ^ ChoiceNo;
                hashCode = (hashCode * 397) ^ Not.GetHashCode();
                return hashCode;
            }
        }
        
        #endregion

        #region Condition Members

        /// <inheritdoc/>
        public bool Evaluate(IRespondent respondent)
        {
            try
            {
                var evaluation = respondent.GetAnswer(QuestionNo, ChoiceNo) != null;

                // Vérification de la négation.
                if (Not)
                {
                    evaluation = !evaluation;
                }

                return evaluation;
            }
            catch (Exception e)
            {
                // Une exception est toujour fausse.
                log.Error(e, "Exception thrown while evaluating respondent.");
                return false;
            }
        }

        #endregion
    }
}
