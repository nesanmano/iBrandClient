namespace NomadLogic.Common.Util.Camera
{
    /// <summary>
    /// Liste les états possibles d'un contrôleur de caméra générique.
    /// </summary>
    public enum CameraControllerStateEnum
    {
        /// <summary>
        /// Indique que le contrôleur n'est pas initialisé et que la plupart, sinon toutes, ses fonctionnalités ne sont pas actives.
        /// </summary>
        NotInitialized,
        /// <summary>
        /// État indéfini. Ne devrait exister que quand la classe vient d'être créée en mémoire et qu'aucune action n'a pu être encore effectuée.
        /// </summary>
        Undefined,
        /// <summary>
        /// Le manager à été détruit après une première initialisation. N'est présentement pas utilisable.
        /// </summary>
        Disposed,
        /// <summary>
        /// Une erreur a été détectée lors de l'initialisation. N'est pas utilisable dans son état actuel et il est possible que toute autre tentative
        /// d'initialisation échoue.
        /// </summary>
        ErrorInitialization,
        /// <summary>
        /// Le contrôleur est initialisé et attend une commande.
        /// </summary>
        Initialized,
        /// <summary>
        /// Le contrôleur est en train de gérer une demande de prise photo.
        /// </summary>
        TakingPictures,
        /// Le contrôlleur est en train de gérer une prise vidéo.
        TakingVideo
    }
}