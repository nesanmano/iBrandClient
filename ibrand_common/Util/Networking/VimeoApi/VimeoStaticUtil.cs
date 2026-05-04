using System.IO;
using System.Linq;
using JetBrains.Annotations;
using NLog;
using NomadLogic.Common.Util.Json;

namespace NomadLogic.Common.Util.Networking.VimeoApi
{
    [PublicAPI]
    public static class VimeoStaticUtil
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Méthode pour envoyer un fichier vers Vimeo. On peut donner un nom de d'Album pour catégoriser ce vidéo. L'album est créé
        /// automatiquement le première fois que l'on détecte qu'il n'existe pas.
        /// Comme le vidéo et l'album peuvent exister indépendemment, et que un vidéo sans album est parfaitement valide pour nos outils, on laisse
        /// le choix d'indiquer si c'est une erreur fatale quand ce lien n'a pu être établi.
        /// </summary>
        /// <param name="vimeo">L'instance d'accès de notre compte vers Vimeo.</param>
        /// <param name="fileToUpload">Le vidéo a envoyer.</param>
        /// <param name="albumName">Optionnel. Le nom de l'album.</param>
        /// <param name="failLinkToAlbumIsError">Indique si on considère l'opération comme un échec si le lien entre le vidéo et l'album échoue.</param>
        /// <returns>Une instance complète vers les objets descriptifs générés par les appels vers l'API Vimeo, en plus des résultats d'opération.</returns>
        public static UploadVideoResult UploadVideoToVimeo([NotNull] VimeoApi vimeo, [NotNull] FileInfo fileToUpload, string albumName = null, bool failLinkToAlbumIsError = false)
        {
            var uploadResult = new UploadVideoResult();

            if (!File.Exists(fileToUpload.FullName))
            {
                uploadResult.Success = false;
                return uploadResult;
            }
            
            uploadResult.VideoInfo = vimeo.UploadVideo(fileToUpload);

            if (uploadResult.VideoInfo == null)
            {
                var uploadFailed = "Vimeo upload call failed. No info exists.";
                log.Error(uploadFailed);
                uploadResult.ErrorMessage = uploadFailed;
                uploadResult.Success = false;
                return uploadResult;
            }

            // Est-ce un long? Si oui et plus que zéro, valide.
            var validId = long.TryParse(uploadResult.VideoInfo.Id, out var val);
            validId &= val > 0;

            if (!validId)
            {
                var noValidIdMessage = "No valid Video ID found. Upload failed. Check info object for more detail.";
                log.Error(noValidIdMessage);
                uploadResult.ErrorMessage = noValidIdMessage;
                uploadResult.Success = false;
                return uploadResult;
            }

            if (!string.IsNullOrWhiteSpace(albumName))
            {
                var resultListAlbums = vimeo.ListAlbums();
                uploadResult.AlbumInfo = resultListAlbums.FirstOrDefault(a => a.Name.Equals(albumName)) ??
                               vimeo.CreateAlbum(albumName, null, GenerateUniqueId.GetId()); // Génère un mot de passe automatiquement

                var successAssignToAlbum = vimeo.AddVideoToAlbum(uploadResult.VideoInfo, uploadResult.AlbumInfo);
                if (!successAssignToAlbum)
                {
                    // Étape non-critique, mais on flag quand même le problème.
                    var message = $"Could not assign video '{uploadResult.VideoInfo.Id}' to album '{uploadResult.AlbumInfo.Id}'. Please verify Vimeo and album information. Video still exists and is accessible.";
                    log.Warn(message);
                    uploadResult.ErrorMessage = message;
                    uploadResult.Success = !failLinkToAlbumIsError;
                    return uploadResult;
                }
            }

            uploadResult.Success = true;
            return uploadResult;
        }
    }

    public class UploadVideoResult : IJsonConfig, IExternalFileConfig
    {
        public bool Success { get; set; }
        public VimeoVideoInfo VideoInfo { get; set; }
        public VimeoAlbumInfo AlbumInfo { get; set; }
        public string ErrorMessage { get; set; }
        public string DefaultConfigFileName { get; } = "upload_video_result.json";
        public string ConfigFileName { get; set; }
    }
}