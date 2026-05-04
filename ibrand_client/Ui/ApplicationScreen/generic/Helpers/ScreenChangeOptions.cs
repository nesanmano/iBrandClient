using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Util;
// ReSharper disable NonReadonlyMemberInGetHashCode

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers
{
    /// <summary>
    /// Cette classe regroupe les options possibles sur une action de changement d'écran. On regroupe les actions en une liste ordonnée,
    /// et cette liste liste, lorsque consultée via la méthode principale, agira sur la première option testée à vrai.
    /// Si une liste non-vide ne contient aucune option évaluable à vrai, on retourne null, et l'appellant doit gérer cette situation.
    /// </summary>
    [PublicAPI]
    public class ScreenChangeOptions : IComparable<ScreenChangeOptions>, IEquatable<ScreenChangeOptions>
    {
        /// <summary>
        /// La liste des options de changements. La liste est consultée en ordre d'insertion, et la première option
        /// à être considérée vraie est utilisée.
        /// Si vide ou nul, assume que l'on veut bouger par en avant de un écran. Ce qui est équivalent à une une liste contenant un
        /// seul élément et que cet élément est celui par défaut (toujours vrai, avance de un).
        /// </summary>
        public IList<ScreenChangeOption> Options { get; set; }

        /// <summary>
        /// Évalue la liste d'options avec le répondant passé en paramètre.
        /// Si la liste est nulle ou vide, retourne l'élément par défaut (aucune condition, avance de un).
        /// Si la liste contient au moins un élément, mais qu'aucun n'est évalué à vrai, retourne la valeur nulle.
        /// L'appellant doit gérer la situation où l'on retourne nul.
        /// </summary>
        /// <param name="respondent">Le répondant à évaluer, avec ses réponses. Ne peut être nul.</param>
        /// <returns>NULL si la liste est non-vide et qu'aucun élément n'est évalué à vrai. Sinon, retourne le premier
        /// élément évalué à vrai.</returns>
        public ScreenChangeOption Evaluate([NotNull] IRespondent respondent)
        {
            if (respondent == null) throw new System.Exception("Invalid respondent to evaluate. Cannot be null.");

            if (Options == null || !Options.Any()) return null;

            return Options.FirstOrDefault(opt => opt.Condition == null || opt.Condition.Evaluate(respondent));
        }

        /// <summary>
        /// Génère une séquence non-nulle, minimale pour une action par en avant.
        /// Est aussi configuré pour provoquer une validation.
        /// On peut donner un nombre de page en option.
        /// </summary>
        /// <returns>Une séquence toujours valide, non-vide, du nombre d'index donné.</returns>
        public static ScreenChangeOptions GenerateBasicNextOptions(int step = 1)
        {
            return new ScreenChangeOptions
            {
                Options = new List<ScreenChangeOption>
                {
                    new ScreenChangeOption
                    {
                        Step = step,
                        ValidationTrigger = true
                    }
                }
            };
        }

        /// <summary>
        /// Génère une séquence d'options pour une action par en arrière de un. Utilisez des valeurs négatives.
        /// Ne provoque pas de validation.
        /// On peut donner un nombre de page en option.
        /// </summary>
        /// <returns>Une séquence toujours vrai, non-vide, du nombre d'index donné.</returns>
        public static ScreenChangeOptions GenerateBasicBackOptions(int step = -1)
        {
            return new ScreenChangeOptions
            {
                Options = new List<ScreenChangeOption>
                {
                    new ScreenChangeOption
                    {
                        Step = step
                    }
                }
            };
        }

        /// <summary>
        /// Génère une séquence non-nulle, minimale pour une action de cancellation. On repart tout à zéro.
        /// Ne provoque aucune validation.
        /// </summary>
        /// <returns>Une séquence toujours valide, non-vide, du nombre d'index donné.</returns>
        public static ScreenChangeOptions GenerateCancelOptions()
        {
            return new ScreenChangeOptions
            {
                Options = new List<ScreenChangeOption>
                {
                    new ScreenChangeOption
                    {
                        ValidationTrigger = false,
                        Cancel = true
                    }
                }
            };
        }

        #region Equality

        public int CompareTo(ScreenChangeOptions other)
        {
            return ClassUtil.CompareObjects(Options, other?.Options);
        }

        public bool Equals(ScreenChangeOptions other)
        {
            var opts = other?.Options;
            if (opts == null && Options == null) return true;
            if (Options == null) return false;
            return opts != null && Options.SequenceEqual(opts);
        }

        public override bool Equals(object other)
        {
            if (!(other is ScreenChangeOptions opts)) return false;
            return Equals(opts);
        }

        public override int GetHashCode()
        {
            return (Options != null ? Options.GetHashCode() : 0);
        }

        public static bool operator ==(ScreenChangeOptions left, ScreenChangeOptions right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ScreenChangeOptions left, ScreenChangeOptions right)
        {
            return !Equals(left, right);
        }

        #endregion
    }
}