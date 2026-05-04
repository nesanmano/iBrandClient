namespace NomadLogic.Common.Services.LocalCapabilities
{
    /// <summary>
    /// Divers types de résultat pour chaque capacités.
    /// </summary>
    public enum StatusEnum
    {
        /// <summary>
        /// Capacité manquante ou non installée.
        /// </summary>
        Missing,
        /// <summary>
        /// Capacité non-testée.
        /// </summary>
        Pending,
        /// <summary>
        /// Il n'est pas possible d'utiliser cette fonctionnalité.
        /// </summary>
        Invalid,
        /// <summary>
        /// La fonctionnalité fonctionne correctement.
        /// </summary>
        Valid
    }
}