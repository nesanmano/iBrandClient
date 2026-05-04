using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using NLog;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Json;
using NomadLogic.PrintStation.Json;

namespace NomadLogic.PrintStation.Contest
{
    public class ContestUtil
    {
        /// Le répertoire où on sauvegarde les fichiers d'état du concours associé à l'événement actuel.
        public static string ContestDir { get; set; } =
            DirectoryUtil.GetLocalPrintstationDefaultDir(
                DirectoryUtil.NomadLogicPrintstationDefaultDirectoriesEnum.contest);

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private static ContestUtil _contestConfig;
        private bool _winner;

        public static ContestUtil GetInstance()
        {
            return _contestConfig ?? (_contestConfig = new ContestUtil());
        }

        /// On génère une chance de générer un gagnant, et on met à jour l'état du concours, s'il y a une période d'active.
        public void NewEvent(PrintStationUser user, DateTime now)
        {
            _winner = false;

            if (Parameters == null) return;

            var state = GetCurrentState(user, now);

            if (state == null) // pas de concour dans cette période
            {
                return;
            }
            
            try
            {
                _winner = state.RollEvent(Parameters, now);

                // Met à jour l'état
                state.LastUpdate = now;
                state.TotalEvents++;
                if (_winner)
                {
                    state.NbrEventsSinceLastWinner = 0;
                    state.Winners.Add(new ContestWinner
                    {
                        FeatureId = user.FeatureId,
                        RespondentId = user.Id,
                        WinnerTime = now
                    });
                }
                else
                {
                    state.NbrEventsSinceLastWinner++;
                }

                // Écrit l'état
                SaveCurrentState(user, state);
            }
            catch (Exception e)
            {
                Log.Error(e, "Error during roll.");
                _winner = false;
            }
        }

        private static void SaveCurrentState([NotNull] PrintStationUser user, [NotNull] ContestState state)
        {
            var periodStateFile = GetStateFileName(user, state.Period);
            
            var success = JsonUtil.WriteJsonFile(periodStateFile, state);

            if (!success)
            {
                Log.Error("Could not save state for period: " + periodStateFile);
            }
        }

        public bool IsLastEventWinner()
        {
            return _winner;
        }

        /// Les paramètres du concours.
        public ContestParameters Parameters { get; set; } = new ContestParameters();

        /// <summary>
        /// Retourne la première période qui contient la date exprimée.
        /// Si le concours est valide, il ne peut y en avoir qu'une seule.
        /// </summary>
        /// <param name="date">La date à vérifier.</param>
        /// <returns>La période sélectionnée, si elle existe. Null sinon.</returns>
        public ContestPeriod GetCurrentPeriod(DateTime date)
        {
            return Parameters?.Periods?.FirstOrDefault(cp => cp.InPeriod(date));
        }

        /// <summary>
        /// Retourne le temps restant de la période courante. S'il n'y a pas de période courante, retourne null.
        /// </summary>
        /// <param name="date">La date à utiliser comme référence.</param>
        /// <returns>Null si on n'est pas dans une période valide. Sinon, un TimeSpan exprimant le temps restant.</returns>
        public TimeSpan? GetCurrentPeriodTimeLeft(DateTime date)
        {
            var curPer = GetCurrentPeriod(date);
            if (curPer == null) return null;
            return curPer.EndTime - date;
        }

        /// <summary>
        /// Teste si l'état actuel du concours est valide. Test principalement ses paramètres.
        /// </summary>
        /// <returns></returns>
        public bool IsValid()
        {
            var set = Parameters.Periods;
            if (set == null) return true;
            if (!set.Any()) return false;

            // On s'assure qu'aucune période n'est invalide, et qu'aucune ne soit conjointe.
            // Comme la liste est triée, on n'a qu'à comparer avec son voisin d'avant.
            using (var iter = set.GetEnumerator())
            {
                ContestPeriod last = null;
                while (iter.MoveNext())
                {
                    var curr = iter.Current;
                    if (curr == null || !curr.IsValid()) return false;

                    // Skip le premier élément, car il ne peut se comparer avec celui d'avant.
                    if (last != null)
                    {
                        if (last.Overlap(curr))
                            return false;
                    }
                    
                    last = curr;
                }
            }

            return true;
        }

        /// <summary>
        ///  Obtient le nom de fichier d'état du concours en prenant l'état actuel de l'utilitaire, une période et un usager Printstation.
        /// </summary>
        /// <param name="user">L'usager printstation.</param>
        /// <param name="period">La période à tester. Doit être valide</param>
        /// <returns>Le nom de fichier qui contient l'état, peut être inexistant. Null, si les paramètres sont inutilisables.</returns>
        public static FileInfo GetStateFileName([NotNull] PrintStationUser user, [NotNull] ContestPeriod period)
        {
            return !period.IsValid() ? null : new FileInfo(ContestDir + user.Id + Path.DirectorySeparatorChar + period.GetFileName());
        }

        /// Obtient le dernier état du concours pour un usager précis (par feature ID), pour une date précise.
        public ContestState GetCurrentState(PrintStationUser user, DateTime date)
        {
            if (user == null) return null;

            var period = GetCurrentPeriod(date);

            if (period == null) return null;

            var periodStateFile = GetStateFileName(user, period);
            ContestState state;

            if (!periodStateFile.Exists)
            {
                state = new ContestState
                {
                    Period = period,
                    Winners = new List<ContestWinner>(),
                    LastUpdate = date
                };
                return state;
            }

            state = JsonUtil.ReadJsonFile<ContestState>(periodStateFile);
            if (state != null) return state;
            Log.Error("Could not load contest state from disk file: " + periodStateFile.FullName);
            return null;
        }
    }
}