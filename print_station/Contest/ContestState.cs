using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace NomadLogic.PrintStation.Contest
{
    public class ContestState
    {
        /// Indique la dernière fois que l'état à été mis à jour.
        public DateTime LastUpdate { get; set; }

        /// Période actuelle couverte par cet état.
        public ContestPeriod Period { get; set; }

        /// Liste des gagnants qui ont été générés par cette période de concours.
        public List<ContestWinner> Winners { get; set; }

        /// Indique le nombre total d'événements (ou d'essais) qui ont été roulés.
        public long TotalEvents { get; set; }

        /// Nombre d'événement sans gagnant. Doit revenir à zéro dès qu'il y a un gagnant.
        public int NbrEventsSinceLastWinner { get; set; }

        /// <summary>
        /// Lance un chance de gagner. Se calcule selon l'état actuel et les configurations du concours.
        /// </summary>
        /// <param name="parameters">Les paramètre de concours</param>
        /// <param name="now">La date au moment où est générée l'événement. Influe sur l'algorithme choisi et le résultat des chances.</param>
        /// <returns>TRUE si un gagnant a été produit.</returns>
        public bool RollEvent([NotNull] ContestParameters parameters, DateTime now)
        {
            var act = ContestAlgorithm.GetAlgorithm(parameters);

            return act.Invoke(parameters, this, now);
        }
    }

    /// <summary>
    /// Structure pour définir un gagnant dans un concours.
    /// </summary>
    public struct ContestWinner
    {
        public DateTime WinnerTime { get; set; }
        public int RespondentId { get; set; }
        public int FeatureId { get; set; }
    }
}