using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Util;

// ReSharper disable NonReadonlyMemberInGetHashCode

namespace NomadLogic.Common.Condition
{
    /// <summary>
    /// Permet la comparaison de multiples conditions avec opérateur. Cette classe étant elle-même un ICondition,
    /// on peut chaîner n'importe quelle conditions ensemblent.
    /// Dans le cas d'un XOR multiple, on utilise la pensée de "un seulement un", plutôt qu'une liste séquentielle de portes XOR.
    /// Voir ici pour une explication: https://electronics.stackexchange.com/questions/93713/how-is-an-xor-with-more-than-2-inputs-supposed-to-work
    /// </summary>
    [PublicAPI]
    public class MultipleCondition : ICondition, IEquatable<MultipleCondition>
    {
        /// <summary>
        /// Liste des conditions.
        /// </summary>
        public IList<ICondition> Conditions { get; set; }

        /// Opérateur sur les éléments. AND par défaut.
        public BooleanOperatorEnum Operator { get; set; } = BooleanOperatorEnum.AND;

        public bool Evaluate(IRespondent respondent)
        {
            if (respondent == null) return false;
            if (Conditions == null || !Conditions.Any()) return false;
            if (Conditions.Contains(this))
            {
                throw new Exception("Error: condition sequence references parent condition. This will create a infinite recursion.");
            }
            
            bool result;
            switch (Operator)
            {
                case BooleanOperatorEnum.AND:
                    result = Conditions.Aggregate(true, (current, condition) => current & condition.Evaluate(respondent));

                    //result = Conditions.All(c => c.Evaluate(respondent));
                    break;
                case BooleanOperatorEnum.OR:
                    result = Conditions.Aggregate(false, (current, condition) => current | condition.Evaluate(respondent));

                    //result = Conditions.Any(c => c.Evaluate(respondent));
                    break;
                case BooleanOperatorEnum.XOR:
                    var oneFound = false;
                    foreach (var condition in Conditions)
                    {
                        var tmpRes = condition.Evaluate(respondent);
                        if (!tmpRes) continue;
                        
                        if (oneFound)
                        {
                            oneFound = false;
                            break;
                        }

                        oneFound = true;
                    }

                    result = oneFound;
                    
                    break;
                default:
                    throw new ArgumentOutOfRangeException("Unknown boolean operator: " + Operator);
            }

            return result;
        }

        #region Overrides of Object

        public int CompareTo(ICondition other)
        {
            if (!(other is MultipleCondition mulCond)) return -1;
            var res = Operator.CompareTo(mulCond.Operator);
            return res != 0 ? res : ClassUtil.CompareObjects(Conditions, mulCond.Conditions);
        }

        public override string ToString()
        {
            return $"[MultipleCondition:Op={Operator}, Conditions={StringUtil.FormatEnumerable(Conditions, false, 0, ",", false)}]";
        }
        
        /// <inheritdoc />
        public override bool Equals(object obj) {
            var args = obj as MultipleCondition;
            return Equals(args);
        }

        #endregion

        #region Equality members

        /// <inheritdoc />
        public bool Equals(MultipleCondition other) {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Operator == other.Operator && ((Conditions == null && other.Conditions == null)
                   || (Conditions != null && other.Conditions != null && Conditions.SequenceEqual(other.Conditions)));
        }

        /// <inheritdoc />
        public override int GetHashCode() {
            unchecked {
                var hashCode = Operator.GetHashCode();
                hashCode = (hashCode * 397) ^ (Conditions != null ? Conditions.GetHashCode() : 0);
                return hashCode;
            }
        }
        
        #endregion
    }
}