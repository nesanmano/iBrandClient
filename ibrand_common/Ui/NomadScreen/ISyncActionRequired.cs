using JetBrains.Annotations;

namespace NomadLogic.Common.Ui.NomadScreen
{
    /// <summary>
    /// Indique que cet élément d'écran peut demander à l'écran de provoquer une synchronisation immédiate vers le serveur de l'état du répondant actuel.
    /// Faire ceci va également provoquer une ré-initialisation de l'état de l'écran.
    /// </summary>
    [PublicAPI]
    public interface ISyncActionRequired
    {
        /// <summary>
        /// Indique que l'élément va demander un synchronisation du répondant suite à certains événements internes, si mis à vrai.
        /// </summary>
        bool SyncActionRequired { get; set; }
    }
}