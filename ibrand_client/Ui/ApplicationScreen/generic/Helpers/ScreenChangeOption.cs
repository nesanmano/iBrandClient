using System;
using JetBrains.Annotations;
using NomadLogic.Common.Condition;
using NomadLogic.Common.Util;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers
{
    /// <summary>
    /// Définie une condition sur laquelle agir.
    /// Si la condition est vrai, alors on utilise la valeur de "Step" pour changer l'index d'écran. Une valeur
    /// négative implique un retour arrière.
    /// Une condition nulle est considérée comme vrai.
    /// </summary>
    [PublicAPI]
    public class ScreenChangeOption : IEquatable<ScreenChangeOption>, IComparable<ScreenChangeOption>
    {
        /// <summary>
        /// La condition pour activer ce changement d'écran. Une condition nulle est tout le temps vrai. Nul par défaut. 
        /// </summary>
        public ICondition Condition { get; set; }

        /// <summary>
        /// Le nombre d'indice à bouger. 1 par défaut. Un nombre négatif est valide, et signifit un retour arrière.
        /// </summary>
        public int Step { get; set; } = 1;

        /// Indique si le changement choisi déclenche une validation venant de la page. Si cette validation est fausse,
        /// l'écran refusera de bouger et normalement affiche un message à l'usager en ce sens. 
        public bool ValidationTrigger { get; set; }

        /// Cliquer sur cette zonne provoque une cancellation, et non un changement d'écran. Si vrai, (faux par défaut), ignore
        /// La valeur de "Step".
        public bool Cancel { get; set; }

        #region Equality

        public int CompareTo(ScreenChangeOption other)
        {
            if (other == null) return 1;
            var res = Step.CompareTo(other.Step);
            if (res != 0) return res;

            res = Cancel.CompareTo(other.Cancel);
            if (res != 0) return res;

            res = ValidationTrigger.CompareTo(other.ValidationTrigger);
            if (res != 0) return res;

            res = ClassUtil.CompareObjects(Condition, other.Condition);
            return res;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is ScreenChangeOption sco)) return false;
            return Equals(sco);
        }

        public bool Equals(ScreenChangeOption other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(Condition, other.Condition) 
                   && Step == other.Step 
                   && ValidationTrigger == other.ValidationTrigger 
                   && Cancel == other.Cancel;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Condition != null ? Condition.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ Step;
                hashCode = (hashCode * 397) ^ ValidationTrigger.GetHashCode();
                hashCode = (hashCode * 397) ^ Cancel.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(ScreenChangeOption left, ScreenChangeOption right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ScreenChangeOption left, ScreenChangeOption right)
        {
            return !Equals(left, right);
        }

        #endregion
    }
}