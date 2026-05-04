namespace NomadLogic.Common.Util.Networking.VimeoApi
{
    /// <summary>
    /// Classe d'information JSON pour des quotas de données avec Vimeo.
    /// </summary>
    public class VimeoSpaceInfo
    {
        public VimeoSpaceDetailInfo Space { get; set; }
    }

    /// <summary>
    /// Détails d'espace disque. En octets.
    /// </summary>
    public class VimeoSpaceDetailInfo
    {
        /// <summary>
        /// Données restantes.
        /// </summary>
        public long Free { get; set; }

        /// <summary>
        /// Données totales. Doit être égal à Restantes + Utilisées.
        /// </summary>
        public long Max { get; set; }

        /// <summary>
        /// Données utilisées.
        /// </summary>
        public long Used { get; set; }
    }
}