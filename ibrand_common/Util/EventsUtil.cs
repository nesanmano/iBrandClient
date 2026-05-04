using System;
using JetBrains.Annotations;

namespace NomadLogic.Common.Util
{
    /// <summary>
    /// Classe de méthode utilitaire et d'extension pour les événements (Events).
    /// </summary>
    [PublicAPI]
    public static class EventsUtil
    {
        /// <summary>
        /// Dans beaucoup de situations, il faut correctement séparer l'objet d'invocation d'événment et les résultats asynchrones.
        /// Dans également beaucoup de cas, il faut faire attention pour ne pas invoquer "Invoke" directement face au risque de bloquer
        /// le fil UI, ce qui est très dangeureux. Mais comme l'exécutant ne devrait pas gérer des erreurs des appellants, appeller
        /// les invocations en mode "Fire & Forget" (I.E. Sans se soucier des résultats et possibles execeptions dans les méthodes d'écoute sur les événements)
        /// est une bonne alternative. Cette méthode encapsule une façon simple (mais sans possibilité d'analyser le retour d'invocation) d'envoyer les "events".
        /// Basé sur une discussion très éclairante sur MSDN:
        /// https://social.msdn.microsoft.com/Forums/vstudio/en-US/bc5c5133-b9ff-4844-8d91-6d15207c9d82/eventshow-to-implement-quotfire-and-forgetquot-?forum=netfxbcl
        /// </summary>
        /// <param name="eventHandler">L'objet de "event".</param>
        /// <param name="sender">L'envoyeur, habituellement "this" mais peut être null au besoin.</param>
        /// <param name="args">Les arguments d'événements. Est null par défaut, ce qui est suffisant dans des événements sans paramètres.</param>
        public static void InvokeFireAndForget(this EventHandler eventHandler, object sender = null, EventArgs args = null)
        {
            var subs = eventHandler?.GetInvocationList();
            if (subs == null) return;
            foreach (var sub in subs)
            {
                if (!(sub is EventHandler handler)) continue;
                handler.BeginInvoke(sender, args, null, null);
            }
        }

        /// <summary>
        /// (Variation générique)
        /// Dans beaucoup de situations, il faut correctement séparer l'objet d'invocation d'événment et les résultats asynchrones.
        /// Dans également beaucoup de cas, il faut faire attention pour ne pas invoquer "Invoke" directement face au risque de bloquer
        /// le fil UI, ce qui est très dangeureux. Mais comme l'exécutant ne devrait pas gérer des erreurs des appellants, appeller
        /// les invocations en mode "Fire & Forget" (I.E. Sans se soucier des résultats et possibles execeptions dans les méthodes d'écoute sur les événements)
        /// est une bonne alternative. Cette méthode encapsule une façon simple (mais sans possibilité d'analyser le retour d'invocation) d'envoyer les "events".
        /// Basé sur une discussion très éclairante sur MSDN:
        /// https://social.msdn.microsoft.com/Forums/vstudio/en-US/bc5c5133-b9ff-4844-8d91-6d15207c9d82/eventshow-to-implement-quotfire-and-forgetquot-?forum=netfxbcl
        /// </summary>
        /// <param name="eventHandler">L'objet de "event".</param>
        /// <param name="sender">L'envoyeur, habituellement "this" mais peut être null au besoin.</param>
        /// <param name="args">Les arguments d'événements. Est null par défaut, ce qui est suffisant dans des événements sans paramètres.</param>
        public static void InvokeFireAndForget<T>(this EventHandler<T> eventHandler, object sender = null, T args = default)
        {
            var subs = eventHandler?.GetInvocationList();
            if (subs == null) return;
            foreach (var sub in subs)
            {
                if (!(sub is EventHandler<T> handler)) continue;
                handler.BeginInvoke(sender, args, null, null);
            }
        }
    }
}