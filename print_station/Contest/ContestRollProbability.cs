using System;

namespace NomadLogic.PrintStation.Contest
{
    /// <summary>
    /// Cette structure représente une chance de gagner avec un rouler aléatoire.
    /// Contient plusieures méthode d'affichage différent pour représenter la chance de gagner.
    /// </summary>
    public struct ContestRollProbability
    {
        /// <summary>
        /// Lance le roulé, avec ou sans semançage du générateur aléatoire.
        /// </summary>
        /// <param name="seed">Optionel, la semence d'initialisation aléatoire.</param>
        /// <returns></returns>
        public bool Roll(int? seed = null)
        {
            var rand = seed.HasValue ? new Random(seed.Value) : new Random();

            return rand.NextDouble() < RollChance;
        }

        // La chance du rouler. Valeur normalisée (entre zéro inclusif et un exclusif).
        public double RollChance { get; set; }
        
        public override string ToString()
        {
            return ToString(true);
        }

        /// <summary>
        /// Affiche la chance de gagner sous format "1 sur X" ou par pourcentage.
        /// Si on veut voir la chance normalisée, on peut utiliser la valeur de RollChance directement.
        /// </summary>
        /// <param name="showAsTotalChance"></param>
        /// <returns>Valeur formatté de la chance de rouler.</returns>
        public string ToString(bool showAsTotalChance)
        {
            if (showAsTotalChance)
            {
                var denominator = (int)Math.Ceiling(1.0 / RollChance);
                return $"1 / {denominator}";
            }

            return $"{RollChance:P}";
        }
    }
}