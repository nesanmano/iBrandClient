using System;
using System.Threading;
using JetBrains.Annotations;

namespace NomadLogic.Common.Util.ThreadPool
{
    /// <summary>
    /// Classe d'exécution abstraite pour le traitement d'une tâche via un bassin de fil d'exécution limité
    /// <typeparam name="TP">Le type d'objet pour les paramètres de traitement d'un travailleur.</typeparam>
    /// <typeparam name="TR">Le type d'objet pour le retour du traitement. Le résultat quoi.</typeparam>
    /// </summary>
    [PublicAPI]
    public abstract class CustomWorker<TP, TR, T> where T : CustomWorkerPool<TP, TR>
    {
        /// <summary>
        /// Indique si le traitement à été accomplit avec succès un fois la méthode DoWork() exécutée.
        /// </summary>
        public bool Success
        {
            get;
            protected set;
        }

        /// <summary>
        /// Référence vers l'objet de retour du traitement, une fois effectué. Selon la classe, peut être nul
        /// même avec un traitement réussi, que l'on peut confirmer avec la propriété Success
        /// </summary>
        public TR WorkerResult
        {
            get;
            protected set;
        }

        /// <summary>
        /// Pour tout message d'erreur significatif à la fin du traitement.
        /// </summary>
        public string Error
        {
            get;
            protected set;
        }

        /// <summary>
        /// Méthode d'appel dès que le bassin d'exécution reçoit le travailleur.
        /// <br/>Cette méthode est habituellement vide, mais dans le cas où un travail doit
        /// être effectué au moment où le bassin reçoit le travailleur, on peut mettre le code
        /// dans une extension de cette méthode.
        /// <p/>Attention: Ce code est exécuté de façon synchrone dès que l'on ajoute le travailleur.
        /// Ne pas faire de traitement lourd ou lent (comme une requête web). Si le traitement provoque
        /// une exception, le travailleur ne sera PAS introduit dans la file d'exécution.
        /// </summary>
        public virtual void DoPreparationWork()
        {
            // Vide par défaut.
        }

        /// <summary>
        /// Méthode d'action principale.
        /// </summary>
        public abstract void DoWork(CancellationToken ct);

        /// <summary>
        /// Identifiant du travailleur.
        /// </summary>
        public int Id
        {
            get;
            set;
        }

        /// <summary>
        /// Paramètre du travailleur.
        /// </summary>
        public TP Parameter
        {
            get;
            set;
        }

        /// <summary>
        /// Gestionnaire d'exécution du travaileur.
        /// </summary>
        public T Pool
        {
            get;
            set;
        }

        /// <summary>
        /// Instance qui enregistre les messages et événement à journaliser ou simplement afficher.
        /// <br/>Peut être nul. À vérifier avant d'utiliser.
        /// </summary>
        public IEventLogger LoggerReference
        {
            get;
            set;
        }

        /// <summary>
        /// Si un travailleur ne doit pas être actif avant un moment précis, on met cette valeur ici.
        /// Si on peut travailler dessus immédiatement, il faut que cette valeur soit inférieure à "maintenant".
        /// </summary>
        public DateTime TimeDelay { get; set; } = DateTime.MinValue;
    }
}