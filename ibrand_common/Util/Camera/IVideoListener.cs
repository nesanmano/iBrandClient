namespace NomadLogic.Common.Util.Camera
{
    /// <summary>
    /// Cette interface indique les méthodes de base de "callback" que doivent implémenter les instances pour recevoir
    /// et gérer correctement les vidéos venant d'une caméra externe branchée à l'ordinateur.
    /// On ne prend qu'un vidéo à la fois.
    /// </summary>
    public interface IVideoListener : IForcedUnregister
    {
        /// <summary>
        /// Indique que la requête de vidéo est terminée.
        /// </summary>
        /// <param name="request">La référence vers la requête, qui contient les résultats.</param>
        void VideoRequestCompleted(IVideoRequest request);

        /// <summary>
        /// Indique que la requête de prise vidéo a été cancellée.
        /// </summary>
        /// <param name="request">La référence vers la requête, qui contient les résultats.</param>
        void VideoRequestCancelled(IVideoRequest request);
        
    }
}