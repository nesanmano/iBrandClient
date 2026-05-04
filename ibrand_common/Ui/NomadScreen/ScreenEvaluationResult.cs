using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace NomadLogic.Common.Ui.NomadScreen
{
    public class ScreenEvaluationResult
    {
        /// <summary>
        /// Constructeur vide.
        /// Le résultat créé est valide et ne contient pas de messages destinés à l'usager ni d'actions à exécuter.
        /// </summary>
        public ScreenEvaluationResult(){}

        /// <summary>
        /// Constructeur rapide, avec booléen.
        /// Si l'argument est vrai, le résultat sera "Valid". Sinon, "Invalid".
        /// </summary>
        /// <param name="v"></param>
        public ScreenEvaluationResult(bool v)
        {
            Result = v ? EvaluationResult.Valid : EvaluationResult.Invalid;
        }

        /// <summary>
        /// Nouvelle instance valide avec une seule action.
        /// </summary>
        /// <param name="a">L'action paramétrée.</param>
        public ScreenEvaluationResult(Action a)
        {
            if (a == null) return;
            ActionsToExecute = new List<Action> {a};
        }

        /// <summary>
        /// Génère une nouvelle instance par aggrégat de différents résultats.
        /// </summary>
        /// <param name="results">Liste d'autre résultats.</param>
        public ScreenEvaluationResult([NotNull] IEnumerable<ScreenEvaluationResult> results)
        {
            AggregateResults(results, this);
        }

        public EvaluationResult Result { get; set; } = EvaluationResult.Valid;

        public IList<Action> ActionsToExecute { get; set; } = new List<Action>();

        public IList<string> UserMessages { get; set; } = new List<string>();

        /// <summary>
        /// Concatène une liste de résultats dans un seul. Voici les règles d'aggrégation:
        /// - Si un seul élément est invalide, le résultat sera invalide.
        /// - Tous les éléments doivent être valides pour avoir un résultat valide.
        /// - Tous les messages sont ajouté dans une seule liste.
        /// - Les actions seront compilées seulement si l'aggrégat est valide.
        /// </summary>
        /// <param name="results">Éléments à aggréger.</param>
        /// <returns>Une nouvelle instance.</returns>
        public static ScreenEvaluationResult AggregateResults(params ScreenEvaluationResult[] results)
        {
            return AggregateResults(results, null);
        }

        /// <summary>
        /// Concatène une liste de résultats dans un seul. Voici les règles d'aggrégation:
        /// - Si un seul élément est invalide, le résultat sera invalide.
        /// - Tous les éléments doivent être valides pour avoir un résultat valide.
        /// - Tous les messages sont ajouté dans une seule liste.
        /// - Les actions seront compilées seulement si l'aggrégat est valide.
        /// </summary>
        /// <param name="results">Liste de résultats à aggréger.</param>
        /// <param name="agg">Si on donne une référence valide, cette instance sera complétement modifiée par l'aggrégation, plutôt que de générer une nouvelle instance.
        /// Le retour sera le même objet que cette valeur.</param>
        /// <returns>Une nouvelle instance ou la même que passé en paramètre, si non-nulle.</returns>
        public static ScreenEvaluationResult AggregateResults([NotNull] IEnumerable<ScreenEvaluationResult> results, ScreenEvaluationResult agg = null)
        {
            var newResult = agg ?? new ScreenEvaluationResult();
            foreach (var res in results)
            {
                if (res == null) continue;
                // Vérification de l'état Si incomplet, on ne change plus. Puis s'il devient invalide, ne change plus.
                if (newResult.Result != EvaluationResult.Incomplete)
                {
                    if (newResult.Result != EvaluationResult.Invalid)
                    {
                        newResult.Result = res.Result;
                    }
                }

                foreach (var mes in res.UserMessages)
                {
                    newResult.UserMessages.Add(mes);
                }

                foreach (var act in res.ActionsToExecute)
                {
                    newResult.ActionsToExecute.Add(act);
                }
            }

            if (newResult.Result != EvaluationResult.Valid) newResult.ActionsToExecute.Clear();

            return newResult;
        }
    }
}