using System;
using System.Threading;
using System.Windows.Forms;

namespace NomadLogic.ShowPicturesOnTV.DisplayControllers
{
    interface IDisplayController
    {
        ConfigurationShowPictureOnTv DisplayConfiguration { get; set; }

        /// <summary>
        /// Le 'Panel' dans lequel le contrôleur peut insérer des éléments au besoin.
        /// </summary>
        Panel DisplayPanel { get; set; }

        /// <summary>
        /// Le répertoire où chercher les nouveaux items d'affichages.
        /// </summary>
        string MainFolder { get; set; }

        /// <summary>
        /// Le répertoire ou mettre les items affichés au moins une fois.
        /// </summary>
        string ShownFolder { get; set; }

        /// <summary>
        /// Pour tester quand on clique sur les images.
        /// </summary>
        MouseEventHandler OnMouseEvent { get; set; }

        /// <summary>
        /// Action à exécuter quand la boucle principale est arrêter. C'est le "callback" d'arrêt.
        /// </summary>
        event Action OnCompleteStop;

        /// <summary>
        /// Méthode pour finaliser la configuration. Toutes les propriétés critiques doivent être assignée avant
        /// d'Invoquer cette méthode. Une fois complétée avec succès, des invocations subséquentes ne doivent rien faire (no-op).
        /// </summary>
        void Init();

        /// <summary>
        /// Boucle principale pour la recherche et l'affichage des items à afficher.
        /// </summary>
        /// <param name="token">Le jeton d'arrêt.</param>
        void LookForDisplayItems(CancellationToken token);

        /// Arrête les opérations le plus vite possible.
        void Stop();
    }
}
