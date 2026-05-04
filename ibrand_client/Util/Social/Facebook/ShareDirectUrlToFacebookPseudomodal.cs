using System.Collections.Generic;
using System.Text;
using System.Web;
using NomadLogic.iBrandDesktop.Ui;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic;

namespace NomadLogic.iBrandDesktop.Util.Social.Facebook
{
    /// <summary>
    /// Partage un fichier d'image JPG ou GIF avec 
    /// </summary>
    public class ShareDirectUrlToFacebookPseudomodal : ShareUrlToFacebookPseudomodal
    {
        private static readonly NLog.Logger log = NLog.LogManager.GetCurrentClassLogger();
        
        /// <summary>
        /// Constructeur principal.
        /// </summary>
        /// <param name="screensController">Le contrôlleur d'écrans du questionnaire.</param>
        /// <param name="webScreen">L'écran iBrand d'affichage web avec clavier.</param>
        /// <param name="tag">L'identifiant de l'écran pseudo-modal.</param>
        public ShareDirectUrlToFacebookPseudomodal(TouchScreenController screensController, WebBrowserScreen webScreen, string tag) : 
            base(screensController, webScreen, tag) { }
        
        /// <summary>
        /// Méthode d'action principale.
        /// </summary>
        protected override void DoWorkImplementation()
        {
            if (QuotesMap == null) throw new System.Exception("Null quotes dictionary.");
            if (!QuotesMap.ContainsKey(Language)) {
                log.Warn("Facebook Direct Share does not contain a quote for this language code: {0}", Language);
                return;
            }

            var quote = QuotesMap[Language];
            var sb = new StringBuilder();

            // Fix rapide 27 mars 2017. Suite à un update côté facebook, seul la version mobile marche avec javascript. On a remplacé le "www" par "m".
            sb.Append("https://m.facebook.com/v2.1/dialog/share?app_id=");
            sb.Append(FacebookAppId);
            sb.Append("&quote=");
            sb.Append(HttpUtility.UrlEncode(quote));
            sb.Append("&href=");
            sb.Append(HttpUtility.UrlEncode(MultimediaItemUrl));
            
            ActivateDisplay(sb.ToString());
        }

        /// <summary>
        /// Dictionnaire des quotations voulues, indexées par code de langue.
        /// </summary>
        public Dictionary<string, string> QuotesMap { get; set; }
    }
}
