using System;

namespace iBrand_SnapShot.Trigger
{
    /// <summary>
    /// Classe abstraite pour une classe de mécanisme de déclencheur.
    /// Note: Pour être valide, il faut absolument que les classes d'implémentations soient dans le
    /// l'espace de nommage "Nomadlogic.iBrand.SnapShot.Trigger.Impl"
    /// </summary>
    internal abstract class TriggerAbstract
    {
        /// <summary>
        /// Indique quand l'instance provoque l'événement de déclenchement.
        /// </summary>
        public event EventHandler Triggered;

        /// <summary>
        /// Lance l'événement de déclenchement. Lié cette méthode à tout Event qui ne prend aucun paramètres.
        /// Sinon, il faudra créer un EventHandler qui appellera cet méthode.
        /// </summary>
        protected void FireTriggerEvent()
        {
            if (Triggered != null)
            {
                Triggered(this, null);
            }
        }

        /// <summary>
        /// Démarre le service du déclencheur. Si l'initialisation ne sait pas faite au préalable,
        /// invoquer cette méthode ne fera rien.
        /// </summary>
        public abstract void Start();
        /// <summary>
        /// Stoppe, mais ne détruit pas, les ressources liées à ce déclencheur.
        /// Ne fait rien si IsRunning est faux.
        /// </summary>
        public abstract void Stop();
        /// <summary>
        /// Détruit les ressources liées à ce déclencheur. Il devrait être possible de
        /// ré-initialiser la classe via "Init" dans cette situation.
        /// </summary>
        public abstract void Dispose();
        /// <summary>
        /// Initialise le déclencheur via une série de paramètres String.
        /// <param name="parameters">La liste des paramètres à donner au Trigger.</param>
        /// <returns>TRUE si et seulement si l'initialisation s'est bien passé.</returns>
        /// </summary>
        public abstract bool Init(string[] parameters);

        /// <summary>
        /// Destructeur invoke Dispose au besoin.
        /// </summary>
        ~TriggerAbstract()
        {
            Dispose();
        }

        /// <summary>
        /// Indique si l'instance est en train de fonctionner.
        /// </summary>
        public abstract bool IsRunning();
        
        /// <summary>
        /// Indique si l'instance est initialisée et peut être lancée.
        /// </summary>
        public abstract bool IsInitialized();
    }
}
