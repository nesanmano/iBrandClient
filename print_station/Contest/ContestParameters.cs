using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using static NomadLogic.PrintStation.Contest.ContestParametersPeriodSmoothingEnum;
using static NomadLogic.PrintStation.Contest.ContestParametersWinnerSelectionEnum;

namespace NomadLogic.PrintStation.Contest
{
    /// Différents algorithmes pour sélectionner le gagnant.
    public enum ContestParametersWinnerSelectionEnum
    {
        /// Provoque un gagnant à chaque X participant. Ne gère pas des périodes de temps.
        FixedWinnerEveryCycle,
        /// Pendant une période particulière, va avoir au maximum X gagnant. Chaque période est séparée des autres. Si -1, aucune limite.
        MaxWinnerInPeriod
    }

    /// Différents algorithmes pour espacer les gagnants dans une période.
    public enum ContestParametersPeriodSmoothingEnum
    {
        /// Aucun espacement artificiel des gagnants.
        NoSmoothing,
        /// Tente d'espacer les gagnants également durant une période. Empêche les gagnants d'être trop concentrés.
        EvenOverTime
    }

    /// Différents algorithmes pour calculer les chances de gagner
    public enum ContestParametersRandomElasticityEnum
    {
        /// Chaque événement est unique et différents des autres.
        Random,
        /// La chance de gagner varie si un événement positif est survenu dans une période de temps. Les chances de gagner deux fois de suite sont plus faibles
        /// que la base indique, et plus on a d'événement négatifs à la chaîne, plus les prochains événements ont des chances élevées de gagner.
        /// Avec cet algorithme, il faut spécifier une élasticité à la chance de base.
        PseudoRandom
    }

    /// <summary>
    /// Cette classe définie les paramètres qui détermine le concours d'un projet précis. Normalement, cette classe est utilisée comme
    /// source pour un fichier JSON lié à un projet PrintStation précis.
    /// </summary>
    public class ContestParameters : IEquatable<ContestParameters>
    {
        /// Selon la méthode de sélection d'un gagnant, cette valeur peut être utilisée différemment.
        public long WinnerCycleValue { get; set; } = 60;

        /// Algorithme de sélection d'un gagnant.
        public ContestParametersWinnerSelectionEnum WinnerSelectionSystem { get; set; } = FixedWinnerEveryCycle;

        /// Si on utilise une période, l'algorithme pour espacer les gagnants.
        public ContestParametersPeriodSmoothingEnum PeriodSmoothing { get; set; } = EvenOverTime;

        /// L'élasticité de la chance de gagner. Dit simplement, si on manipule la chance de gagner.
        public ContestParametersRandomElasticityEnum RandomElasticity { get; set; } = ContestParametersRandomElasticityEnum.Random;

        /// Le pourcentage (valeur entre zéro et un) de la chance de gagner.
        public double BaseChance = 0.1;

        /// Indique le maximum de variation que l'on veut donner (en pourcentage) à la chance de base, quand on la manipule.
        /// En exprimée en pourcentage. Zéro indique aucune variation possible.
        public double BaseChanceElasticity = 0;

        /// Ensemble des périodes durant laquelle le concours est actif.
        public SortedSet<ContestPeriod> Periods { get; set; }

        /// Optionel. Mot de passe pour fermer le dialogue du gagnant quand on tente d'imprimer.
        public string WinnerDialogPassword { get; set; }

        public bool Equals(ContestParameters other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return BaseChance.Equals(other.BaseChance) && 
                   BaseChanceElasticity.Equals(other.BaseChanceElasticity) && 
                   WinnerCycleValue == other.WinnerCycleValue && 
                   WinnerSelectionSystem == other.WinnerSelectionSystem && 
                   PeriodSmoothing == other.PeriodSmoothing && 
                   RandomElasticity == other.RandomElasticity && 
                   Periods != null && other.Periods != null && Periods.SequenceEqual(other.Periods);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((ContestParameters) obj);
        }

        [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = BaseChance.GetHashCode();
                hashCode = (hashCode * 397) ^ BaseChanceElasticity.GetHashCode();
                hashCode = (hashCode * 397) ^ (int)WinnerCycleValue;
                hashCode = (hashCode * 397) ^ (int) WinnerSelectionSystem;
                hashCode = (hashCode * 397) ^ (int) PeriodSmoothing;
                hashCode = (hashCode * 397) ^ (int) RandomElasticity;
                hashCode = (hashCode * 397) ^ (Periods != null ? Periods.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}
