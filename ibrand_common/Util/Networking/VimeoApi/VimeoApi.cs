using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using NLog;
using NomadLogic.Common.Util.Json;
using VimeoAPI.API;
// ReSharper disable InconsistentNaming

namespace NomadLogic.Common.Util.Networking.VimeoApi
{
    /// <summary>
    /// Liste des droits disponibles avec la session ouverte du client Vimeo.
    /// </summary>
    [PublicAPI]
    public enum VideoClientScopeEnum
    {
        Private,
        Public,
        Purchased,
        Purchase,
        Create,
        Edit,
        Delete,
        Interact,
        Upload,
        Promo_codes,
        Video_files
    }
    
    [PublicAPI]
    public enum VimeoAccountLogin
    {
        /// <summary>
        /// Compte associé à fsimardfournier, celui que l'on utilse par défaut depuis un bout, avec l'application
        /// "Scotia Bank"
        /// </summary>
        MainAccount,
        /// <summary>
        /// Compte secondaire, associé avec le courriel info@nomadlogic.com et l'application DFC.
        /// </summary>
        InfoAccount,
        /// <summary>
        /// Appli de test associé au compte principal.
        /// </summary>
        TestAccount
    }

    /// <summary>
    /// Option de triage pour l'affichage d'un album Vimeo.
    /// </summary>
    [PublicAPI]
    public enum VimeoAlbumSortEnum
    {
        added_first,
        added_last,
        alphabetical,
        arranged,
        comments,
        likes,
        newest,
        oldest,
        plays
    }

    /// <summary>
    /// Classe instanciable pour interfacer avec l'API de VIMEO. Cette classe conserve les informations
    /// de login et d'envoi. Il s'agit d'appels de type REST vers leur serveur principal. Comme il faut garder une configuration constante pour
    /// ces appels, l'instance garde les objets en vie pour effectuer ces tâches.
    /// Pour effectuer des opérations plus complexes, il faut instancier cette classe, mais ces opérations devraient être gérer par des utilitaires.
    /// https://github.com/saeedafshari/VimeoDotNet3
    /// https://developer.vimeo.com/api
    /// 
    /// </summary>
    [PublicAPI]
    public class VimeoApi
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        private readonly VimeoClient _client;

        /// <summary>
        /// Méthode d'instanciation avec des informations de compte très spécifique à chaque combinaison de "jeton/app Id/app Secret"
        /// dans tous nos comptes Vimeo existants.
        /// </summary>
        /// <param name="account">L'énumération du compte voulu. PAr défaut, le compte principal.</param>
        /// <returns>Une nouvelle instance de l'utilitaire Vimeo vers le site web Vimeo.</returns>
        public static VimeoApi GetInstance(VimeoAccountLogin account = VimeoAccountLogin.MainAccount)
        {
            switch (account)
            {
                case VimeoAccountLogin.MainAccount:
                    return new VimeoApi(
                        "6b23620558dfc91248b6eab808d8f078",
                        "d135677736827d29a48157d40f065606e27aeb22",
                        "cf0f0303f89bc616ae05e7196a08072f182a38fd");
                case VimeoAccountLogin.InfoAccount:
                    return new VimeoApi(
                        "25877dc279a28602e6ea91b726521ffe", 
                        "c20fa984f1957599de30375f5b5ca40c784090ba", 
                        "QK2TxRblMJaCAsrOl42YS56DGXQuHbDU1gWK0VNLPgvKbPaMYd1UK/kixwTjJw5vDd9qAwEi6IY90UFhNMDWteBfJ3G+W7CsmUdn8SAPTAnbO8JVQS+A+dxUtt+4kPG5");
                case VimeoAccountLogin.TestAccount:
                    return new VimeoApi(
                        "a44072e8fdc53cc2f045d1d2bcc4f828", 
                        "7f0666c78b4076a58c320a723d67db82f11e8ce9", 
                        "pu8MJzku06SGGaeN5+ZTVCPWK7SHfXsR7d4bHJxWR+fqC8C4nHfL7wWGofTqgnu/q7lst0Ht6Uw15DYuD54uEu/3ugYvCxR8GBoWzhaM3PbPdeAyX/QNhVGmcPV+fCuM");
                default:
                    throw new ArgumentOutOfRangeException(nameof(account), account, null);
            }
        }

        /// <summary>
        /// Créer une instance de l'outil avec une authorisation immédiate vers le serveur Vimeo. On doit fournir un jeton spécifique, ainsi qu'un
        /// identifiant et une clé secrète d'une application Vimeo spécifique.
        /// </summary>
        /// <param name="vimeoToken">Le jeton de connexion. Connait ses droits, mais on ne peut pas les extraire. C'est la responsabilité de celui qui le créer
        /// de connaître les droits associés au jeton.</param>
        /// <param name="clientId">L'identifiant unique de l'application..</param>
        /// <param name="clientSecret">La clé secrète de l'application.</param>
        private VimeoApi(string vimeoToken, string clientId, string clientSecret)
        {
            _client = VimeoClient.ReAuthorize(vimeoToken, clientId, clientSecret);
        }

        /// <summary>
        /// Indique si le client possède les droits d'actions spécifiés en paramètre.
        /// À noter qu'il n'est possible d'avoir les scopes que si on a fait un connexion qui génère un jeton d'accès.
        /// </summary>
        /// <param name="scopes">Les droits à vérifier.</param>
        /// <returns>True si la connexion Vimeo possède tous les droits d'accès spécifiés.</returns>
        public bool HasScopes(params VideoClientScopeEnum[] scopes)
        {
            if (_client.Scope == null) return false;
            if (scopes == null || scopes.Length == 0) return true;

            return scopes.Select(scope => scope.ToString().ToLower()).All(s => _client.Scope.Contains(s));
        }

        /// <summary>
        /// Liste les droits de la connexion Vimeo actuelle.
        /// À noter qu'il n'est possible d'avoir les scopes que si on a fait un connexion qui génère un jeton d'accès.
        /// </summary>
        /// <returns>La liste de droits séparés avec un espace, comme reçus lors de la requête initiale.</returns>
        public string ListScopes()
        {
            return StringUtil.FormatEnumerable(_client.Scope, false, 0, " ");
        }

        /// <summary>
        /// Obtient les informations de base du compte Vimeo.
        /// </summary>
        /// <returns>l'objet info des informations du compte.</returns>
        public VimeoAccountInfo GetAccountInfo()
        {
            var me = _client.Request("/me", null, "GET");
            if (me.StatusCode != HttpStatusCode.OK) return null;
            var info = new VimeoAccountInfo();
            var json = JsonConvert.SerializeObject(me.DataMap, Formatting.Indented, new IsoDateTimeConverter { DateTimeFormat = "yyyy-MM-ddTHH:mm:ss" });
            JsonUtil.UpdateFromJson(json, info);
            return info;
        }

        /// <summary>
        /// Liste les albums liés au compte.
        /// Voir: https://developer.vimeo.com/api/endpoints/albums
        /// </summary>
        /// <returns>La liste des albums gérés par l'usager.</returns>
        public List<VimeoAlbumInfo> ListAlbums()
        {
            var me = _client.Request("/me/albums", null, "GET");
            if (me.StatusCode != HttpStatusCode.OK) return null;
            var json = JsonConvert.SerializeObject(me.DataMap, Formatting.Indented, new IsoDateTimeConverter { DateTimeFormat = "yyyy-MM-ddTHH:mm:ss" });
            var data = new VimeoRequestResultInfo<VimeoAlbumInfo>();
            JsonUtil.UpdateFromJson(json, data);
            return data.Data;
        }

        /// <summary>
        /// Créer un nouvel album.
        /// Voir: https://developer.vimeo.com/api/endpoints/albums
        /// </summary>
        /// <param name="name">Le nom de l'album. Ne peut être null.</param>
        /// <param name="description">La description de l'album. Ne peut être null, mais peut être vide.</param>
        /// <param name="password">Si on veut protéger l'album derrière un mot de passe, l'indiquer ici.</param>
        /// <param name="sortOptions">Le type de tri appliqueé quand on visionne la liste des vidéos.</param>
        /// <returns>Si tout s'est bien passé, on retourne une référence vers l'album créé. Null sinon.</returns>
        public VimeoAlbumInfo CreateAlbum([NotNull] string name, string description = "", string password = null, VimeoAlbumSortEnum sortOptions = VimeoAlbumSortEnum.added_last)
        {
            var requestParams = new Dictionary<string, string>
            {
                { "name", name},
                { "description", description ?? ""},
                { "sort", sortOptions.ToString() }
            };
            if (!string.IsNullOrWhiteSpace(password))
            {
                requestParams.Add("password", password);
                requestParams.Add("privacy", "password");
            }

            var me = _client.Request("/me/albums", requestParams, "POST");

            if (me.StatusCode != HttpStatusCode.Created) return null;

            var album = new VimeoAlbumInfo();
            var json = JsonConvert.SerializeObject(me.DataMap, Formatting.Indented, new IsoDateTimeConverter { DateTimeFormat = "yyyy-MM-ddTHH:mm:ss" });
            JsonUtil.UpdateFromJson(json, album);
            return album;
        }
        
        // ReSharper disable once UnusedMember.Local
        private void VerifyScope(VideoClientScopeEnum scope)
        {
            if (!HasScopes(scope)) throw new Exception("Cannot execute request because the current session do not have the following scope: " + scope);
        }

        /// <summary>
        /// Obtient les information d'un album particulier.
        /// </summary>
        /// <param name="albumId">L'identifiant de l'album.</param>
        /// <returns>Une référence vers l'album si existant. Sinon, retourne nul.</returns>
        public VimeoAlbumInfo GetAlbum(int albumId)
        {
            try
            {
                var me = _client.Request($"/me/albums/{albumId}", null, "GET");

                if (me.StatusCode != HttpStatusCode.OK) return null;
                
                var info = new VimeoAlbumInfo();
                var json = JsonConvert.SerializeObject(me.DataMap, Formatting.Indented);
                JsonUtil.UpdateFromJson(json, info);
                return info;
            }
            catch (Exception e)
            {
                log.Warn(e.Message);
                return null;
            }
        }

        /// <summary>
        /// Modifie un album existant. Nécessite un album valide au départ.
        /// </summary>
        /// <param name="albumInfo">L'album à modifier.</param>
        /// <param name="setPasswordProtected">Indique s'il faut avoir un mot de passe ou non pour la modification.</param>
        /// <param name="password">Si on utilise un mot de passe, il faut le fournir ici. S'il manque, on interprète ce manque par aucune protection.</param>
        /// <param name="sortOptions">L'option de tri de l'album. Null implique aucun changement.</param>
        /// <returns>TRUE si et seulement si l'album à été modifié. La modification peut réussir même si aucun changement n'est apporté, mais que la commande était valide et effectuée avec succès.</returns>
        public bool UpdateAlbum(VimeoAlbumInfo albumInfo, bool setPasswordProtected = false, string password = null, VimeoAlbumSortEnum? sortOptions = null)
        {
            if (albumInfo == null) return false;

            var requestParams = new Dictionary<string, string>();
            if (setPasswordProtected && !string.IsNullOrWhiteSpace(password))
            {
                requestParams.Add("password", password);
                requestParams.Add("privacy", "password");
            }
            else {
                requestParams.Add("privacy", "anybody");
            }
            if (albumInfo.Name != null) requestParams.Add("name", albumInfo.Name);
            if (albumInfo.Description != null) requestParams.Add("description", albumInfo.Description);
            if (sortOptions.HasValue) requestParams.Add("sort", sortOptions.Value.ToString());

            var me = _client.Request($"/me/albums/{albumInfo.Id}", requestParams, "PATCH");

            return me.StatusCode == HttpStatusCode.OK;
        }

        /// <summary>
        /// Détruit un album, s'il existe.
        /// </summary>
        /// <param name="albumId">L'identifiant numérique de l'album.</param>
        /// <returns>TRUE si et seulement si l'album existait et à été détruit avec succès.</returns>
        public bool DestroyAlbum(int albumId)
        {
             var me = _client.Request($"/me/albums/{albumId}", null, "DELETE");
             return me.StatusCode == HttpStatusCode.NoContent;
        }

        /// <summary>
        /// Envoi un vidéo en un trait via l'interface REST de l'API Vimeo.
        /// Cette méthode est synchrone, ce qui peut faire qu'elle peut être longue selon les conditions réseau et la taille du fichier à envoyer.
        /// Voir ici pour plus d'information: https://developer.vimeo.com/api/upload/videos
        /// </summary>
        /// <param name="fileToUpload">Le chemin complet du fichier du vidéo à téléverser.</param>
        /// <returns>Une instance valide si tout s'est bien passé</returns>
        public VimeoVideoInfo UploadVideo(string fileToUpload)
        {
            return string.IsNullOrWhiteSpace(fileToUpload) ? null : UploadVideo(new FileInfo(fileToUpload));
        }

        /// <summary>
        /// Envoi un vidéo en un trait via l'interface REST de l'API Vimeo.
        /// Cette méthode est synchrone, ce qui peut faire qu'elle peut être longue selon les conditions réseau et la taille du fichier à envoyer.
        /// Voir ici pour plus d'information: https://developer.vimeo.com/api/upload/videos
        /// </summary>
        /// <param name="fileToUpload">Le chemin complet du fichier du vidéo à téléverser.</param>
        /// <returns>Une instance valide si tout s'est bien passé</returns>
        public VimeoVideoInfo UploadVideo(FileInfo fileToUpload)
        {
            var requestParams = new Dictionary<string, string> {{"type", "tus" } };
            var me = _client.Request("/me/videos", requestParams, "POST");
            
            // Génération du ticket
            var ticket = new VimeoUploadTicketInfo();
            var json = JsonConvert.SerializeObject(me.DataMap, Formatting.Indented, new IsoDateTimeConverter { DateTimeFormat = "yyyy-MM-ddTHH:mm:ss" });
            try
            {
                JsonUtil.UpdateFromJson(json, ticket);
            }
            catch (Exception e)
            {
                log.Error(e);
                log.Error("Malformed JSON string: " + json);
                return null;
            }

            if (string.IsNullOrWhiteSpace(ticket.UploadLinkSecure))
            {
                return null;
            }

            var result = _client.Upload(fileToUpload, ticket.UploadLinkSecure);
            if (!result) return null;
            
            // Destruction du ticket
            me = _client.Request(ticket.CompleteUri, null, "DELETE");

            if (!(me.HttpHeaderMap?.ContainsKey("Location") ?? false)) return null;

            var loc = me.HttpHeaderMap["Location"];
            var videoId = loc.Substring(loc.LastIndexOf("/", StringComparison.InvariantCulture) + 1);

            // Maintenant que l'on a l'identifiant, on peut aller chercher ses informations sur le serveur vimeo.
            return GetVideo(videoId);
        }

        /// <summary>
        /// Obtient les informations sur un video. On peut utiliser le Id long ou court du vidéo.
        /// </summary>
        /// <param name="videoId">L'identifiant (long ou court) du vidéo sur Vimeo.</param>
        /// <returns>Une référence vers l'information du vidéo. Null s'il y a eu problème.</returns>
        public VimeoVideoInfo GetVideo(string videoId)
        {
            var me = _client.Request($"/videos/{videoId}", null, "GET");

            if (me.StatusCode != HttpStatusCode.OK) return null;

            var vimeoVideo = new VimeoVideoInfo();
            var json = JsonConvert.SerializeObject(me.DataMap, Formatting.Indented, new IsoDateTimeConverter { DateTimeFormat = "yyyy-MM-ddTHH:mm:ss" });
            JsonUtil.UpdateFromJson(json, vimeoVideo);

            return vimeoVideo;
        }

        /// <summary>
        /// Détruit un vidéo sur viméo.
        /// Notez qu'il n'est pas possible de détruire un vidéo qui n'a pas fini d'être processé par Vimeo. On ne peut donc pas immédiatement détruire un
        /// vidéo qui vient à peine d'être téléverser.
        /// </summary>
        /// <param name="videoInfo">Une instance d'info de vidéo.</param>
        /// <returns>TRUE si le vidéo existait et a été détruit avec succès.</returns>
        public bool DestroyVideo(VimeoVideoInfo videoInfo)
        {
            return DestroyVideo(videoInfo?.Uri);
        }

        /// <summary>
        /// Détruit un vidéo sur viméo, par un identifiant, long ou court.
        /// Notez qu'il n'est pas possible de détruire un vidéo qui n'a pas fini d'être processé par Vimeo. On ne peut donc pas immédiatement détruire un
        /// vidéo qui vient à peine d'être téléverser.
        /// </summary>
        /// <param name="videoId">Une instance d'info de vidéo.</param>
        /// <returns>TRUE si le vidéo existait et a été détruit avec succès.</returns>
        public bool DestroyVideo(string videoId)
        {
            if (string.IsNullOrWhiteSpace(videoId)) return false;

            var me = _client.Request($"/videos/{videoId}", null, "DELETE");
            return me.StatusCode == HttpStatusCode.NoContent; // Code 204
        }

        /// <summary>
        /// Ajoute un vidéo à un album.
        /// </summary>
        /// <param name="videoInfo">Info sur le vidéo</param>
        /// <param name="albumFound">Info sur l'album.</param>
        /// <returns>TRUE si et seulement si le vidéo à été ajouté à l'album.</returns>
        public bool AddVideoToAlbum(VimeoVideoInfo videoInfo, VimeoAlbumInfo albumFound)
        {
            if (videoInfo == null || albumFound == null || string.IsNullOrWhiteSpace(videoInfo.Id) || albumFound.Id == -1) return false;

            var me = _client.Request($"/me/albums/{albumFound.Id}/videos/{videoInfo.Id}", null, "PUT");
            return me.StatusCode == HttpStatusCode.NoContent; // Code 204
        }
    }
}
