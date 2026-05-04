using System;
using JetBrains.Annotations;
using NLog;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.DateAndTime;

namespace NomadLogic.PrintStation.Contest
{
    /// <summary>
    /// Classe "Factory" pour choisir un algorithme d'événement pour un concours.
    /// </summary>
    public class ContestAlgorithm
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Retourne une méthode d'algorithme selon les paramètres actuel.
        /// C'est une méthode dite "Factory", qui gère tous les cas de figures. Les méthodes retournées sont statiques et font partie de cette classe.
        /// La méthode retournée est stateless, on peut l'exécuter à profusion sans changer l'état du concours.
        /// </summary>
        /// <param name="parameters">Les paramètres de concours.</param>
        /// <returns>Une méthode qui retourne un résultat d'événement, gagnant ou non-gagnant (un bool donc).</returns>
        public static Func<ContestParameters, ContestState, DateTime, bool> GetAlgorithm([NotNull] ContestParameters parameters)
        {
            switch (parameters.WinnerSelectionSystem)
            {
                case ContestParametersWinnerSelectionEnum.FixedWinnerEveryCycle:
                    return FixedChance;
                case ContestParametersWinnerSelectionEnum.MaxWinnerInPeriod:
                    switch (parameters.PeriodSmoothing)
                    {
                        case ContestParametersPeriodSmoothingEnum.EvenOverTime:
                            return EvenOverTimeSlice;
                        default:
                            throw new Exception($"Unknown configuration. Please review Facotory method. Parameters: WinnerSelection={ContestParametersWinnerSelectionEnum.MaxWinnerInPeriod}, PeriodSmoothing={parameters.PeriodSmoothing}");
                    }
                default:
                    throw new Exception($"Unknown configuration. Please review Facotory method. Parameters: WinnerSelection={parameters.WinnerSelectionSystem}");
            }
        }

        /// Gagnant fixe à chaque nombre d'essai. Aucun calcul aléatoire.
        public static bool FixedChance([NotNull] ContestParameters def, [NotNull] ContestState state, DateTime eventTime)
        {
            return state.TotalEvents != 0 && state.TotalEvents % def.WinnerCycleValue == 0;
        }

        public static bool EvenOverTimeSlice([NotNull] ContestParameters def, [NotNull] ContestState state, DateTime eventTime)
        {
            if (def.BaseChance == 0.0) return false;

            var elapsed = eventTime - state.Period.StartTime;

            if (state.Winners.Count >= def.WinnerCycleValue)
            {
                log.Debug($"Maximum amount of winner reached. Automatic fail. Total events={state.TotalEvents}, Elapsed time: {elapsed.ToStringHourMinSec(false)}");
                return false;
            }

            var span = state.Period.Span();
            var timeSliceMinutes = Math.Floor(span.TotalMinutes / def.WinnerCycleValue);
            var nbrSlices = (int)Math.Floor(span.TotalMinutes / timeSliceMinutes);
            
            var completedSlice = (int)Math.Floor(elapsed.TotalMinutes / timeSliceMinutes);
            
            // On fait juste un rouler si on n'a pas assez de gagnants pour la tranche actuelle.
            if (state.Winners.Count <= completedSlice)
            {
                // Calcul de l'élasticité.
                /* TODO finaliser l'algorithme. Agit comme fixe pour l'instant, non pas comme comme pseudo-random
                //  Calcule de la fréquence sur un dénominateur (un gagnant par X événement), où X est un entier positif.
                double freq;
                if (state.Winners.Count == 0 || state.TotalEvents == 0)
                {
                    freq = 0;
                }
                else
                {
                    freq = 1.0 / (state.Winners.Count / (double) state.TotalEvents);
                }
                var offset = freq - (int) Math.Ceiling(1 / def.BaseChance);
                log.Trace($"Calculated Frequency");
                */
                ContestRollProbability rollChance;
                if (completedSlice + 1 >= nbrSlices)
                {
                    rollChance = GetRollProbability(def.BaseChance * 2, def.BaseChanceElasticity, def.RandomElasticity, 0);
                }
                else
                {
                    rollChance = GetRollProbability(def.BaseChance, def.BaseChanceElasticity, def.RandomElasticity, 0);
                }
                var result = rollChance.Roll((int) eventTime.Ticks);

                // Si on est en arrière de deux période, on fait un gagnant immédiatement.
                if ((completedSlice - state.Winners.Count) > 1)
                {
                    log.Debug($"Behind in periods vs winners. Automatic win. Total events={state.TotalEvents}, Streak w/o win={state.NbrEventsSinceLastWinner}, Remaining time: {(state.Period.EndTime - eventTime).ToStringHourMinSec(false)}");
                    return true;
                }

                if (result)
                {
                    log.Trace($"EvenOverTimeSlice slice data: time slice={timeSliceMinutes:F}, slice number={nbrSlices}, elapsed={elapsed.ToStringHourMinSec(false)}, completed slice={completedSlice}, winners={state.Winners.Count}, total events={state.TotalEvents}, Streak w/o win={state.NbrEventsSinceLastWinner}");
                    log.Debug("Winner generated roll chance: {0}", rollChance.ToString(false));
                }

                return result;
            }
            return false; // Trop de gagnants déjà roulés implique pas de gagnant possible sur cette tentative.
        }

        /// <summary>
        /// Établit une probabilité de gagner. Cette probabilité est calculée selon les chances de base et le type d'algorithme.
        /// Cette méthode est un "Factory" method en fait, car selon les paramètres l'instance retournée sera différente.
        /// </summary>
        /// <param name="defBaseChance">La chance de base de gagner.</param>
        /// <param name="defBaseChanceElasticity">L'élasticité maximale de cette chance.</param>
        /// <param name="defRandomElasticity">L'algorithme d'élasticité.</param>
        /// <param name="elasticValue">Selon l'algorithme, une valeur qui influe sur l'élasticité. Par exemple, le nombre consécutif d'essais sans gagnant, où le nombre total de gagnant.</param>
        /// <returns>Une instance qui contient les chances de rouler.</returns>
        public static ContestRollProbability GetRollProbability(double defBaseChance, double defBaseChanceElasticity, 
            ContestParametersRandomElasticityEnum defRandomElasticity, double? elasticValue = null)
        {
            var prob = new ContestRollProbability
            {
                RollChance = defBaseChance
            };

            if (defBaseChanceElasticity != 0)
            {

                switch (defRandomElasticity)
                {
                    case ContestParametersRandomElasticityEnum.Random:
                        // no-op
                        break;
                    case ContestParametersRandomElasticityEnum.PseudoRandom:
                        // Si l'élasticité est négative, on réduit la probabilité. Si zéro, aucun changement, si supérieur, on augmente.
                        if (elasticValue.HasValue)
                        {
                            if (elasticValue < 0)
                            {
                                prob.RollChance -= defBaseChanceElasticity * elasticValue.Value;
                            }
                            else if (elasticValue > 0)
                            {
                                prob.RollChance += defBaseChanceElasticity * elasticValue.Value;
                            }
                            log.Trace("Modified probability={0}, elasticty={1}, modifier={2}", prob.RollChance, defBaseChanceElasticity, elasticValue.Value);
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(defRandomElasticity), defRandomElasticity, null);
                }
            }

            return prob;
        }
    }
}