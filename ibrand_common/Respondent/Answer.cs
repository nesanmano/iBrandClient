using System;
using Newtonsoft.Json;
using NomadLogic.Common.Util;

// ReSharper disable NonReadonlyMemberInGetHashCode

namespace NomadLogic.Common.Respondent
{
    /// <summary>
    /// Classe complète pour contenir une réponse.
    /// Si on veut faire un dictionnaire de réponses, il faut utiliser l'élément de QuestionIdentifier comme clé.
    /// </summary>
    public class Answer : IEquatable<Answer>, IComparable<Answer>
    {
        private readonly QuestionIdentifier _qi = new QuestionIdentifier();

        /// <summary>
        /// Constructeur vide pour la sérialisation.
        /// </summary>
        public Answer() { }

        /// <summary>
        /// Constructeur par copie. Il s'agit d'une copie profonde.
        /// </summary>
        /// <param name="original">La référence originale.</param>
        public Answer(Answer original)
        {
            QuestionIdentifier = original.QuestionIdentifier;
            Response = original.Response;
        }
        
        [JsonProperty("type")]
        public QuestionTypeEnum QuestionType
        {
            get => _qi.QuestionType;
            set => _qi.QuestionType = value;
        }

        /// Réponse associée à la question. Nul par défaut.
        [JsonProperty("answer_text")]
        public string Response { get; set; }
        
        /// Valeur entière du numéro de question.
        [JsonProperty("question_no")]
        [JsonRequired]
        public int QuestionNo
        {
            get => _qi.QuestionNo;
            set => _qi.QuestionNo = value;
        }

        /// Valeur entière du choix. Par défaut, 1;
        [JsonProperty("choice_no")]
        public int ChoiceNo
        {
            get => _qi.ChoiceNo;
            set => _qi.ChoiceNo = value;
        }

        /// <summary>
        /// Retourne l'identifiant de la réponse. On retourne une copie quand on lit l'information.
        /// Si on passe une valeur nulle, on remet les valeur au défaut (qui est invalide avec un identifiant de question à zéro).
        /// </summary>
        public QuestionIdentifier QuestionIdentifier
        {
            get => new QuestionIdentifier(_qi); // On fait une copie.

            set
            {
                QuestionNo = value?.QuestionNo ?? 0;
                ChoiceNo = value?.ChoiceNo ?? 1;
                QuestionType = value?.QuestionType ?? QuestionTypeEnum.Text;
            }
        }

        #region Overrides of Object

        /// <inheritdoc />
        public override bool Equals(object obj) {
            var arg = obj as Answer;
            return Equals(arg);
        }

        #region Equality members

        /// <inheritdoc />
        public bool Equals(Answer other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return _qi.Equals(other._qi) && string.Equals(Response, other.Response);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Response?.GetHashCode() ?? 0;
                return (hashCode * 397) ^ QuestionIdentifier.GetHashCode();
            }
        }

        #endregion

        #endregion

        /// <inheritdoc />
        public int CompareTo(Answer other) {
            if (other == null) return 1;
            var comp = _qi.CompareTo(other._qi);
            return comp != 0 ? comp : string.Compare(Response, other.Response, StringComparison.InvariantCulture);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return ClassUtil.ToStringPublicProperties(this);
        }
    }
}
