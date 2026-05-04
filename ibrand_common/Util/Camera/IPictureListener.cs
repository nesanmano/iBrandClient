namespace NomadLogic.Common.Util.Camera
{
    /// <summary>
    /// Cette interface indique les méthodes de base de "callback" que doivent implémenter les instances pour recevoir
    /// et gérer correctement les images venant d'une caméra externe branchée à l'ordinateur.
    /// </summary>
    public interface IPictureListener : IForcedUnregister
    {
        /// <summary>
        /// Indique que la requête de prise d'image est terminée.
        /// </summary>
        /// <param name="request">La référence vers la requête, qui contient les résultats.</param>
        void PictureRequestCompleted(IPictureRequest request);

        /// <summary>
        /// Indique que la requête de prise d'image a été cancellée. Probablement à la demande de l'usager dûe
        /// à de trop nombreuses tentative de prise photo manquées.
        /// </summary>
        /// <param name="request">La référence vers la requête, qui contient les résultats.</param>
        void PictureRequestCancelled(IPictureRequest request);

        /// <summary>
        /// Indique qu'une image a été prise librement par une caméra.
        /// </summary>
        /// <param name="filePath">Le chemin vers le fichier créé.</param>
        /// <param name="cameraId">L'identifiant de la caméra. Habituellement le numéro de série.</param>
        void PictureTaken(string filePath, string[] cameraId);
    }
}