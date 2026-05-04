using System.Collections.Generic;
using System.Linq;

namespace NomadLogic.iBrandDesktop.Util.SubmitAction.BaseClasses
{
    /// <summary>
    /// Cette classe englobe une liste qui contient les prochaine étapes pour le prochain worker dans une série de SubmitAction.
    /// Quand un Worker se contruit, il doit lire le premier indice de chacun des éléments de cette classe. Ces éléments doivent avoir avoir au moins un
    /// indice (indice zéro), mais peuvent contenir des éléments nulls ou faux.
    /// Si l'élément lu est vide ou faux, cela indique que ce worker n'est pas requis (no-op), et qu'il doit passer immédiatement au prochain.
    /// Quand il passe au prochain, il doit enlever le premier indice de chaque liste, et re-créer une instance de cette classe avec la nouvelle liste.
    /// Si la liste devient vide, alors il n'appelle pas la prochaine étape: il est effectivement le DERNIER dans la chaîne et doit agir comme tel,
    /// ce qui implique de valider le répondant.
    /// S'il ne l'est pas, il créer le prochain worker et l'assigne comme nouveau chaînon.
    /// </summary>
    public class SubmitActionWorkerNextStepInfo
    {
        /// <summary>
        /// Créé une séquence de chaîne avec deux listes. La liste la plus importante est celle de booléens, indiquant la chaîne complète d'exécution.
        /// Elle peut être du genre {true, false, true}, où le premier chaînon s'exécute, le deuxième non, le troisième oui.
        /// La deuxième liste peut être soit nulle, soit remplie d'éléments non nuls d'une taille similaire à la première.
        /// Elle indique si on lance les chaîne de Worker sur des bassins ou non. Même un "no-op" doit avoir un bassin valide, si on les utilise.
        /// Un eexception sera lancée si le deuxième dbassin n'est pas conforme. La première liste doit contenir au moins un élément.
        /// </summary>
        /// <param name="executeSteps">Liste de booléens indique l'exécution ou non de chaque étape de chaînage.</param>
        /// <param name="pools">Null ou de même taille que le premier indice remplit de bassins d'exécution.</param>
        public SubmitActionWorkerNextStepInfo(IList<bool> executeSteps, IList<SubmitActionWorkerPool> pools)
        {
            if (executeSteps == null || !executeSteps.Any()) throw new System.Exception("First list must containe at least one element.");
            if (pools != null && pools.Count != executeSteps.Count) throw new System.Exception("Pool list must contain same number of elements, if not null.");
            if (pools != null && pools.Any(elem => elem == null)) throw new System.Exception("Pool list must not contains null elements.");

            for (var i = 0; i < executeSteps.Count; i++)
            {
                var tmp = new PairOfPoolExecute(pools?[i], executeSteps[i]);
                _list.Add(tmp);
            }
        }

        /// <summary>
        /// Configuration actuelle.
        /// </summary>
        public PairOfPoolExecute CurrentConfig => _list.First();

        /// <summary>
        /// Constrcuteur privé pour la construction interne de cette classe.
        /// </summary>
        private SubmitActionWorkerNextStepInfo() { }

        /// <summary>
        /// Génère automatiquement une nouvelle instance avec de bonnes références. SI le résultat est null, il n'y a pas de prochaine étape.
        /// </summary>
        /// <returns>NULL si pas de prochaine étape, sinon, une référence pour la prochaine étape de Worker.</returns>
        public SubmitActionWorkerNextStepInfo GetNextWorkerStepInfo()
        {
            if (_list.Count == 1) return null;

            var copy = new List<PairOfPoolExecute>();
            for (var i = 1; i < _list.Count; i++)
            {
                copy.Add(_list[i]);
            }
            return new SubmitActionWorkerNextStepInfo { _list = copy };
            
        }

        /// <summary>
        /// Liste des bassins disponible. Seul celui de l'indic zéro concerne le worker qui le reçoit, les autres sont pour les étapes
        /// subséquentes. Peut être totalement null.
        /// </summary>
        private IList<PairOfPoolExecute> _list = new List<PairOfPoolExecute>();
    }
}