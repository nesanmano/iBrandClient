using System.Drawing;
using System.Text;
using System.Web;
using System.Windows.Forms;
using NomadLogic.Common;
using NomadLogic.Common.Services;
using NomadLogic.Common.Services.Configuration;
using NomadLogic.iBrandDesktop.Ui;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic;

namespace NomadLogic.iBrandDesktop.Util.Social.Facebook
{
    /// <summary>
    /// Envoi vers notre page JSP de partage Facebook, via un écran pseudo-modal (externe à la liste séquentielle d'écran).
    /// </summary>
    public class ShareUrlToFacebookPseudomodal
    {
        private static readonly NLog.Logger log = NLog.LogManager.GetCurrentClassLogger();

        private readonly TouchScreenController _screensController;
        private readonly string _tag;
        private readonly WebBrowserScreen _webscreen;
        /// <summary>
        /// Événement levé quand l'envoi vers Facebook est terminé.
        /// </summary>
        public event OnUploadPictureFinishedDelegate OnUploadFinished;
        /// Taille de l'écran web. Le reste est calculé pour le clavier et les autres éléments visuels.
        protected Size _browserSize = Size.Empty;
        
        /// <summary>
        /// Constructeur principal.
        /// </summary>
        /// <param name="screensController">Le contrôlleur d'écrans du questionnaire.</param>
        /// <param name="webScreen">L'écran iBrand d'affichage web avec clavier.</param>
        /// <param name="tag">L'identifiant de l'écran pseudo-modal.</param>
        public ShareUrlToFacebookPseudomodal(TouchScreenController screensController, WebBrowserScreen webScreen, string tag)
        {
            _tag = tag;
            _webscreen = webScreen;
            _screensController = screensController;
        }

        /// <summary>
        /// Indique si on doit afficher le bouton de sortie du browser web (qui ne fait pas partie de la page).
        /// </summary>
        public bool ShowCloseBitmap { get; set; } = true;

        /// <summary>
        /// Met l'écran web à l'écran, et rafraîchit son lien URL.
        /// </summary>
        /// <param name="facebookUrl"></param>
        protected void ActivateDisplay(string facebookUrl)
        {
            _webscreen.ShowClose(ShowCloseBitmap);
            _webscreen.NaviguateToUrl(facebookUrl);
            _screensController.ChangeToPseudoModalScreen(_tag);
        }

        /// <summary>
        /// Taille de la compasant web de l'écran.
        /// </summary>
        public Size BrowserSize
        {
            set => _browserSize = value;
        }
        /// <summary>
        /// par defaut = false
        /// cette propriete si egal a true cela veut dire qu'on va utiliser la nouvelle version JUJU et le nouvelle App facebook JUJU
        /// sinon on utilise l'ancienne App Facebook 
        /// </summary>
        public bool IsServerJuJu { get; set; }

        /// <summary>
        /// Méthode d'action principale.
        /// </summary>
        public void DoWork()
        {
            if (MultimediaItemUrl == null)
            {
                log.Warn("No Url has been given. Aborting.");
                return;
            }

            try
            {
                DoWorkImplementation();
            }
            catch (System.Exception e)
            {
                log.Error(e);
            }
            
            OnUploadFinished?.Invoke(true);

            Cursor.Current = Cursors.Default;
        }

        /// <summary>
        /// Exécution de l'url de redirection.
        /// </summary>
        protected virtual void DoWorkImplementation()
        {
            var sb = new StringBuilder();
            var configuration = ServicesContainer.GetService<NomadConfiguration>();
            sb.Append("http://");
            sb.Append(configuration.CurrentServerConfiguration.Address);

            if (IsServerJuJu)
            {
                sb.Append("/facebook?project_id=");
                sb.Append(_screensController.Project.Id);
                sb.Append("&questionnaire_id=");
                sb.Append(_screensController.Questionnaire.Id);
                if (Language != null)
                {
                    sb.Append("&language=");
                    sb.Append(Language);
                }

                sb.Append("&picture_url=");
                sb.Append(HttpUtility.UrlEncode(MultimediaItemUrl)); // UTF-8 par défaut

            }
            else
            {

                sb.Append("/ibrand/facebook.jsp?project_no=");
                sb.Append(_screensController.Project.No);
                sb.Append("&questionnaire_id=");
                sb.Append(_screensController.Questionnaire.Id);
                if (Language != null)
                {
                    sb.Append("&lg=");
                    sb.Append(Language);
                }


                sb.Append("&image_url=");
                sb.Append(HttpUtility.UrlEncode(MultimediaItemUrl)); // UTF-8 par défaut
            }


            if (VimeoID != null)
            {
                sb.Append("&vimeo_id=");
                sb.Append(VimeoID); // UTF-8 par défaut
            }

            if (ThumbnailUrl != null)
            {
                sb.Append("&thumb_url=");
                sb.Append(HttpUtility.UrlEncode(ThumbnailUrl));
            }

            if (ImageWidth != 0)
            {
                sb.Append("&img_width=");
                sb.Append(ImageWidth);
            }

            if (ImageHeight != 0)
            {
                sb.Append("&img_height=");
                sb.Append(ImageHeight);
            }

            // Fix rapide 27 mars 2017. Suite à un update côté facebook, seul la version mobile marche avec javascript. On a remplacé le "www" par "m".
            var facebookUrl = "https://m.facebook.com/dialog/share?app_id=" + FacebookAppId + "&href=" + HttpUtility.UrlEncode(sb.ToString());
            
            ActivateDisplay(facebookUrl);
        }

        /// <summary>
        /// Identifiant numérique de l'application Facebook. Utilise celle de "iBrand Desktop" par défaut.
        /// </summary>
        public string FacebookAppId { get; set; } = "625746777495680";

        /// <summary>
        /// Le URL de l'élément multimédia à partager. À utiliser de façon exclusive avec l'identifiant Vimeo.
        /// </summary>
        public string MultimediaItemUrl { get; set; }

        /// <summary>
        /// L'identifiant numérique du vidéo Vimeo à référencer comme élément multimédia.
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public string VimeoID { get; set; }

        /// <summary>
        /// Une image, idéalement plus petite que l'élément multimédia à envoyer, pour fins de présentation.
        /// </summary>
        public string ThumbnailUrl { get; set; }

        /// <summary>
        /// Le code de langue à utiliser.
        /// </summary>
        public string Language { get; set; }

        public int ImageWidth { get; set; }
        public int ImageHeight { get; set; }
    }
}
