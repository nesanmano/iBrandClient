using System;

namespace NomadLogic.Common.Services
{
    /// <summary>
    /// Interface de base pour les classes globales de configuration et de gestion pour les application Nomad.
    /// Comme ils sont disposable, le "teardown" de ces classes se fait dans l'implémentation de la méthode "Dispose()".
    /// Un service "disposé" ne doit plus être utilisé et doit empêcher son utilisation dans ce cas de figure.
    /// </summary>
    public interface IService : IDisposable
    {
        /// <summary>
        /// Teste la classe de service pour s'assurer qu'elle est bel et bien fonctionnelle.
        /// Un retour "false" ici est critique; probablement de niveau fatal même. Si cela arrive, c'est qu'un des
        /// services de base du fonctionnement de iBrand est incapable de s'initialiser et qu'il faut probablement
        /// faire des vérifications plus poussées. Il est primordial d'indiquer la cause dans les journaux.
        /// </summary>
        /// <returns>TRUE si l'initialisation est correcte et que le service peut être utilisé.</returns>
        bool Init();
    }
}
