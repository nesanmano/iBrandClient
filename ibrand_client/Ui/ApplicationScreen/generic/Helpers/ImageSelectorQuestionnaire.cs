using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using NomadLogic.Common.Condition;
using NomadLogic.Common.Respondent;

// ReSharper disable NonReadonlyMemberInGetHashCode

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers {
    /// <summary>
    /// Implémentation d'un sélecteur selon une liste de conditions, chacune associée à une image unique.
    /// Il est possible que la même image soit associée au plus qu'une condition. De parle fait que les
    /// conditions sont contenues dans un dictionnaire, elles doivent être uniques, quelles que soit l'implémentation.
    /// Le fait que l'on utilise une classe générique est pour utiliser un seul type de ICondition à la fois.
    /// Si l'état du répondant ne correspoend à aucune condition, le retour de l'évaluation sera nul.
    /// </summary>
    [PublicAPI]
    public class ImageSelectorQuestionnaire<T> : IImageSelector, IEquatable<ImageSelectorQuestionnaire<T>> where T : ICondition {
        
        public ISet<ConditionFileTuple<T>> Tuples { get; set; }

        /// Constructeur principal.
        public ImageSelectorQuestionnaire(ISet<ConditionFileTuple<T>> tuples) {
            Tuples = tuples;
        }

        public ImageSelectorQuestionnaire() { }

        #region Implementation of IImageSelector

        /// <inheritdoc />
        public string GetImageFrame(IRespondent respondent, int? index = null) {
            if (Tuples == null || Tuples.Count == 0 || respondent == null) return null;

            using (var iter = Tuples.GetEnumerator()) {
                while (iter.MoveNext()) {
                    var entry = iter.Current;
                    if (entry.Condition.Evaluate(respondent)) { return entry.File; }
                }
            }
            return null;
        }

        #endregion

        public bool Equals(ImageSelectorQuestionnaire<T> other)
        {
            if (ReferenceEquals(null, other)) return false;
            return ReferenceEquals(this, other)
                   || (Tuples == null && other.Tuples == null)
                   || (Tuples != null && other.Tuples != null && Tuples.SequenceEqual(other.Tuples));
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((ImageSelectorQuestionnaire<T>) obj);
        }

        public override int GetHashCode()
        {
            return (Tuples != null ? Tuples.GetHashCode() : 0);
        }
    }
}