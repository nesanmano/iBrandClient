namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers
{
    /// <summary>
    ///     Contient les différents état de cet écran, et comment on doit passer de l'un à l'autre. En tout temps, si on utilise la zone "cancel", on peut sortir de
    ///     l'écran et revenir à l'écran de bienvenue.
    /// </summary>
    public enum RecordVideoScreenStates
    {
        /// <summary>
        ///     Indique l'état initial, sans vidéo disponible. Soit à cause d'une erreur, soit parce que on vient d'arriver sur la page.
        ///     Quand on clique sur "record", on tombe en mode RECORDING.
        /// </summary>
        StandBy,

        /// <summary>
        ///     On se prépare à enregistrer. Le dernier vidéo prit à été détruit, et on se prépapre à l'enregistrement. On passe en mode RECORDING une fois le
        ///     temps de préparation écoulé, ce qui est automatique.
        /// </summary>
        PreRecording,

        /// <summary>
        ///     On commence un enregistrement. Le dernier vidéo pris à été détruit, et sera remplacé une fois terminé. Si on termine, et qu'il
        ///     existe un fichier sur la caméra, on tombe en TRANSFERING. Si on a pas commencé, donc pas de fichier, on tombe en STAND_BY.
        /// </summary>
        Recording,

        /// <summary>
        ///     Fichier sur la caméra en cours de transfert. On bloque les autres fonctions. Une fois terminé, si le fichier est sur le disque,
        ///     on tombe en VIDEO_FILE_READY, sinon on tombe en STAND_BY
        /// </summary>
        Transfering,

        /// <summary>
        ///     Fichier sur disque et compression faite. On est prêt à passer au prochain écran, via "done" (ou automatiquement si ce bouton n'est pas disponible),
        ///     mais on peut également recommencer la vidéo, auquel cas on tombe en mode PRE_RECORDING.
        /// </summary>
        VideoFileReady,

        /// <summary>
        ///     L'écran est inactif, et n'est pas montré à l'usager. Aucune activité ne sera traitée.
        /// </summary>
        ScreenInactive
    }
}