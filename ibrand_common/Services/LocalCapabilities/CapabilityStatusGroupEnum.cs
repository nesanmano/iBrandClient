namespace NomadLogic.Common.Services.LocalCapabilities
{
    /// <summary>
    /// Liste des groupements de capacité
    /// </summary>
    public enum CapabilityStatusGroupEnum
    {
        /// Groupe représentant les fonctions FFMPEG du système
        Ffmpeg,
        /// Groupe représentant les fonctions OpenCv, via Emgu CV, du système.
        OpenCv,
        /// Test pour les libraries externes VLC
        VlcLibs,
        /// Groupe pour les fichiers communs aux applications Nomad
        Assets,
        /// Groupe pour l'analyse des fonctionalités OpenGl du système
        OpenGlAnalysis,
    }
}