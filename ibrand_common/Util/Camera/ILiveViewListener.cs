using System.Drawing;

namespace NomadLogic.Common.Util.Camera
{
    /// <summary>
    /// Cette interface indique les méthodes de base de "callback" que doivent implémenter les instances pour recevoir
    /// et gérer correctement les trames de visionnement venant d'une caméra externe branchée à l'ordinateur.
    /// </summary>
    public interface ILiveViewListener : IForcedUnregister
    {
        /// <summary>
        /// Une nouvelle trame a été créée et est envoyée via cette méthode.
        /// </summary>
        /// <param name="image">Référence vers l'image créée.</param>
        void OnLiveViewUpdate(Image image);
    }
}