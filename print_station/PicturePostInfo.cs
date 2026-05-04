namespace NomadLogic.PrintStation
{
    /// <summary>
    /// Structure d'information pour une image de PrintStation. Contient les information importantes du Post qui a été utilisé comme base.
    /// </summary>
    public class PicturePostInfo
    {
        /// <summary>
        /// Constructeur principal.
        /// </summary>
        /// <param name="src">Le chemin du fichier d'image principal du Post sur disque.</param>
        /// <param name="userName">Le nom d'usager du profil associé à cette image.</param>
        /// <param name="profilPictureSrc">Chemin local vers l'image associée au profil du service social.</param>
        /// <param name="userId">L'identifiant de l'usager du profil.</param>
        /// <param name="serverPostId">Identifiant du Post qui acontient l'image.</param>
        /// <param name="postId">L'identifiant unique de ce Post, en utilisant la valeur de la BD locale.</param>
        /// <param name="repondantNo">Le numéro de répondant du serveur, si la source d'image est un projet iBrand. -1 si inutile.</param>
        public PicturePostInfo(string src, string userName, string profilPictureSrc, int userId, int serverPostId, long postId, int repondantNo)
        {
            Src = src;
            UserName = userName;
            ProfilPictureSrc = profilPictureSrc;
            UserId = userId;
            ServerPostId = serverPostId;
            PostId = postId;
            RepondantNo = repondantNo;
        }

        /// <summary>
        /// Numéro du répondant si la source d'image est un porjet iBrand.
        /// </summary>
        public int RepondantNo { get; set; }

        /// <summary>
        /// Le chemin sur disque du fichier d'image.
        /// </summary>
        public string Src { get; }
        /// <summary>
        /// L'identifiant du Post qui englobe l'image, en provenance du serveur. Dans le cas d'une image créé à partir d'un
        /// envoi direct, cette valeur est toujours -1.
        /// </summary>
        public int ServerPostId { get; }
        /// <summary>
        /// Le nom d'usager associé au profil parent du Post.
        /// </summary>
        public string UserName { get; }
        /// <summary>
        /// L'identifiant de l'usager Printstation relié à l'événement Printstation..
        /// </summary>
        public int UserId { get; }
        /// <summary>
        /// Le chemin sur disque du fichier d'image de présentation du profil du service social associé.
        /// </summary>
        public string ProfilPictureSrc { get; }
        /// <summary>
        /// L'identifiant Unique local du Post. Contrairement à ServerPostId, il est toujours unique.
        /// </summary>
        public long PostId { get; }
    }
}