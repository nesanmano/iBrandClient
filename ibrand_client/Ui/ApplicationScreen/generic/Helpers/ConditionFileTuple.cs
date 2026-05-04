using System;
using System.Collections.Generic;
using NomadLogic.Common.Condition;
// ReSharper disable NonReadonlyMemberInGetHashCode

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers
{
    /// <summary>
    /// Structure qui contient une condition basée sur un questionnaire, associé à un fichier.
    /// Les méthodes de comparaison sont faites pour vérifier que la condition, et uniquement elle, soit unique, de façon à agir comme
    /// une clé de façon naturelle dans des collections appropriées.
    /// </summary>
    /// <typeparam name="T">Le type de condition.</typeparam>
    public struct ConditionFileTuple<T> : IEquatable<ConditionFileTuple<T>> where T : ICondition
    {
        public T Condition { get; set; }
        public string File { get; set; }

        public bool Equals(ConditionFileTuple<T> other)
        {
            return EqualityComparer<T>.Default.Equals(Condition, other.Condition);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is ConditionFileTuple<T> tuple && Equals(tuple);
        }

        public override int GetHashCode()
        {
            return EqualityComparer<T>.Default.GetHashCode(Condition);
        }
    }
}