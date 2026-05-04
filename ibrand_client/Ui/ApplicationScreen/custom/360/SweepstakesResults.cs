using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Util.DateAndTime;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.custom._360
{
    public class SweepstakesResults
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        public IList<SweepstakeResult> PreviouSweepstakeResults { get; set; } = new List<SweepstakeResult>();

        public SweepstakeResult DetermineCategory(
            string rfid,
            IRespondent resp, 
            SweepstakesDefinitions fullDefs, 
            IList<SweepdIdTuple> rfids, 
            IList<SweepStakesConditions> sweepstakeCategoryConditions, 
            DateTime now)
        {
            var nom = resp.GetAnswerText(1);

            // Déterminer, pour CHAQUE catégorie (en cas de fall-back) la première auquelle le répondant est éligible ET que le block de temps actuel est libre.
            foreach (var cat in sweepstakeCategoryConditions)
            {
                // Vérifie catégorie
                if (!cat.Condition.Evaluate(resp)) continue;
                log.Trace("Catégorie {0} est possible.", cat.Category);
                var blockDef = IsBlockAllowWinner(cat.Category, fullDefs, now);

                if (blockDef == null) continue;

                return CreateResult(blockDef.CodePrix, nom, now, cat.Category);
            }

            // Déterminer maintenant si on donne le prix RFID (catégorie par défaut)
            // trouver le prix dans la liste de RFID
            var catE = rfids.First(t => t.Rfid.Equals(rfid));
            return CreateResult(catE.CodePrix, nom, now, 'E');
        }

        private static SweepstakeResult CreateResult(int code, string nom, DateTime now, char cat)
        {
            var res = new SweepstakeResult
            {
                CodePrix = code,
                Message = $"Félicitation {nom}, vous avez gagné {SweepdIdTuple.GetText(code)} !",
                ImagePrix = SweepdIdTuple.GetFileName(code),
                Category = cat,
                DateProcessed = now
            };

            log.Info("Résultat concours: Catégorie={0}, Code={1}, Date={2}", cat, code, now.ToString(@"yyyy-MM-dd hh:mm:ss"));
            return res;
        }

        private SweepstakesBlock IsBlockAllowWinner(char category, SweepstakesDefinitions defs, DateTime now)
        {
            // Trouve la configuration actuelle de cette catégorie
            var currConfig = defs.BlockDefinitions.First().Value.FirstOrDefault(sb => sb.MinDate < now && sb.MaxDate >= now && sb.CodeSweepBlock == category);
            if (currConfig == null)
            {
                log.Trace("Pas de configuration de bloc trouvé pour la catégorie {0} à la date {1}", category, now.ToShortDateString());
                return null;
            }

            // Trouver le dernier bloc gagnant de la catégorie.
            var winners = PreviouSweepstakeResults.Where(
                    b => b.Category == category && 
                    currConfig.MinDate <= b.DateProcessed && 
                    currConfig.MaxDate >= b.DateProcessed).
                    OrderBy(b => b.DateProcessed).ToList();

            // S'il y a moins de gagnants que le nombre maximal
            if (winners.Count < currConfig.MaxWinners)
            {
                // Si vide
                if (winners.Count == 0)
                {
                    // Premier gagnant!
                    return currConfig;
                }

                // On détermine maintenant que l'on ne soit pas en "cooldown"
                var lastWinner = winners.Last();
                var nextPossibleWinner = lastWinner.DateProcessed + currConfig.BlockTime;
                if (nextPossibleWinner < now)
                {
                    // On a un gagnant!
                    return currConfig;
                }

                var remain = nextPossibleWinner - now;
                // Trop tôt pour un autre gagnant!
                log.Debug("Trop tôt pour un nouveau gagnant dans cette catégorie ({0}). Temps restant: {1}", currConfig.CodeSweepBlock, remain.ToStringHourMinSec());
                return null;
            }

            log.Debug("Nombre maximum de gagnant atteints ({0}/{1}). On saute.", winners.Count, currConfig.MaxWinners);
            //log.Trace("Definition: {0}", ClassUtil.ToStringPublicProperties(currConfig));
            return null;
        }
    }

    public class SweepstakeResult
    {
        public char Category { get; set; }
        public int CodePrix { get; set; }
        public string ImagePrix { get; set; }
        public string Message { get; set; }
        public DateTime DateProcessed { get; set; }
    }
}