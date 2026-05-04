using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Web;
using System.Windows.Forms;
using NomadLogic.Common;
using NomadLogic.Common.Services;
using NomadLogic.Common.Services.Configuration;
using NomadLogic.Common.Util;
using NomadLogic.iBrandDesktop.Ui;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic;
using TweetSharp;

namespace NomadLogic.iBrandDesktop.Util.Social.Twitter
{
    public class UploadPictureToTwitterLeNomadPseudomodal
    {
        private const string TOKEN_REQUEST_ABORTED = "Request for token has aborted due to unresponsiveness. The internet connection may be too slow or down.";
        private static readonly NLog.Logger log = NLog.LogManager.GetCurrentClassLogger();

        public event OnUploadPictureFinishedDelegate OnUploadFinished;
        private readonly TouchScreenController _screensController;
        private readonly string _tag;
        private readonly WebBrowserScreen _webscreen;
        private TwitterService _service;
        private OAuthRequestToken _requestToken;
        private readonly ScreenConfig _sc;
        private Thread _thread;

        public UploadPictureToTwitterLeNomadPseudomodal(ScreenConfig sc, TouchScreenController screensController, WebBrowserScreen webScreen, string tag)
        {
            _sc = sc;
            _tag = tag;
            _webscreen = webScreen;
            _screensController = screensController;
        }

        public string CustomText { get; set; }

        public string VimeoId
        {
            get;
            set;
        }

        public void ShareVimeoFromServer()
        {
            try
            {
                var sb = new StringBuilder();
                var configuration = ServicesContainer.GetService<NomadConfiguration>();

                sb.Append("http://");
                sb.Append(configuration.CurrentServerConfiguration.Address); // to do
                sb.Append("/ibrand/twitter_from_csharp_vimeo.jsp?resp_no=");
                sb.Append(RespondentNo);
                if (CustomText != null)
                {
                    sb.Append("&message=");
                    var text = HttpUtility.UrlEncode(CustomText);
                    sb.Append(text);
                }

                var twitterUrl = sb.ToString();

                ActivateDisplay(twitterUrl, "https://mobile.twitter.com");
            }
            catch (System.Exception e)
            {
                log.Error(e);
            }

            OnUploadFinished?.Invoke(true);

            Cursor.Current = Cursors.Default;
        }

        protected void ActivateDisplay(string twitterUrl, string interceptUrl)
        {
            _webscreen.ShowClose(true);
            _webscreen.NaviguateToUrl(twitterUrl);
            _webscreen.UrlToIntercept = "https://mobile.twitter.com";
            _screensController.ChangeToPseudoModalScreen(_tag);
        }

        public void DoWork()
        {
            if (ShareFromRespondentNo)
            {
                ShareVimeoFromServer();
                return;
            }

            // Empêche le spam de la fonction si un traitement est déjà en cours.
            if (_thread != null) return;

            try
            {
                TwitterService service = null;
                OAuthRequestToken requestToken = null;
                Uri uri = null;

                _thread = new Thread(() =>
                {
                    try
                    {
                        service = new TwitterService(Constants.TWITTER_APPS_ID, Constants.TWITTER_SECRET);

                        // Step 1 - Retrieve an OAuth Request Token
                        requestToken = service.GetRequestToken();

                        // Step 2 - Redirect to the OAuth Authorization URL
                        uri = service.GetAuthorizationUri(requestToken);
                    }
                    catch (ThreadAbortException)
                    {
                        // On ne fait rien dans ce cas car il y a des messages d'erreur plus spécifique pour cette
                        // Situation ailleur.
                    }
                    catch (System.Exception ex)
                    {
                        log.Error(ex);
                    }
                });
                _thread.Start();

                // Donne trentes secondes pour que la requête se termine.
                if (!_thread.Join(30 * 1000))
                {
                    _thread.Abort();
                    log.Error(TOKEN_REQUEST_ABORTED);

                    _webscreen.ShowClose(true);
                    _webscreen.NaviguateToErrorPage("Request error", TOKEN_REQUEST_ABORTED);
                    _screensController.ChangeToPseudoModalScreen(_tag);

                    OnUploadFinished?.Invoke(false);
                    AfterPost(false);
                }

                _thread = null;

                if (service != null && requestToken != null && uri != null)
                {
                    ActivateDisplay(service, requestToken, uri);
                }
            }
            catch (System.Exception err)
            {
                log.Error(err, "Twitter access thread check did not complete normally.");
            }
            finally
            {
                _thread = null;
            }
            Cursor.Current = Cursors.Default;
        }

        protected void ActivateDisplay(TwitterService service, OAuthRequestToken requestToken, Uri uri)
        {
            _service = service;
            _requestToken = requestToken;

            // On attend l'événement asynchrone de retour pour voir le résultat.
            // Est désactivé quand on revient, pour ne pas être en permanence dans le callback du webbrowser.
            _webscreen.OnReturn += WhenWebScreenFinish;

            _webscreen.ShowClose(true);
            _webscreen.TagToIntercept = "code";
            _webscreen.UrlToIntercept = "https://twitter.com";
            _webscreen.NaviguateToUrl(uri.ToString());
            _screensController.ChangeToPseudoModalScreen(_tag);
        }

        private void WhenWebScreenFinish(int pageIndex)
        {
            // On appelle maintenant la méthode de traitement du post.
            PostWithAccessToken(_service, _requestToken, _webscreen.TagToInterceptResult);
        }

        protected void AfterPost(bool success)
        {
            _service = null;
            _requestToken = null;
            _webscreen.OnReturn -= WhenWebScreenFinish;
            
        }

        protected void PostWithAccessToken(TwitterService service, OAuthRequestToken requestToken, string token)
        {
            var success = false;

            try
            {
                var verifier = token; // <-- This is input into your application by your user

                if (verifier != null)
                {
                    Cursor.Current = Cursors.WaitCursor;
                    var access = service.GetAccessToken(requestToken, verifier);

                    // Step 4 - User authenticates using the Access Token
                    service.AuthenticateWith(access.Token, access.TokenSecret);

                    var configuration = ServicesContainer.GetService<NomadConfiguration>();

                    var tmpText = CustomText;

                    //tmpText  = Uri.EscapeDataString(tmpText);

                    TwitterStatus status;

                    if (_sc.SendMediaToTwitter)
                    {
                        if (StringUtil.ContainsNonAscii(CustomText))
                        {
                            throw new System.Exception("Twitter text contains non-ASCII characters. Due to a bug in TwitterSharp, this cannot work when sharing multimedia content: " + CustomText);
                        }

                        //On fait une copie du fichier car il est probable que le fichier original soit bloqué par un autre processus.
                        var tmp = DirectoryUtil.CreateLocalTemporaryFile(StringUtil.GetFileName(FileName));
                        if (File.Exists(tmp))
                        {
                            File.Delete(tmp);
                        }
                        File.Copy(FileName, tmp);

                        var stream = new FileStream(tmp, FileMode.Open);
                        IDictionary<string, Stream> image = new Dictionary<string, Stream>();
                        image.Add("picture", stream);
                        status =
                            service.SendTweetWithMedia(new SendTweetWithMediaOptions { Status = tmpText, Images = image });
                        stream.Close();
                        File.Delete(tmp);
                    }
                    else
                    {
                        string url = null;
                        if (_sc.VideoLink != null)
                        {
                            url = _sc.VideoLink + "?project_no=" + _sc.ProjectNo +
                                      "&video_id=" + VimeoId;
                        }
                        else if (_sc.QuestionFirstPictureNo != null)
                        {
                            var questionNo = _sc.QuestionPictureTransformedNo ?? _sc.QuestionFirstPictureNo.Value;

                            if (configuration.Printstation.LinkToGallery)
                            {
                                url = "http://www.nomadlogicapp.com/le-nomad/gallery.jsp?project_no=" + _sc.ProjectNo +
                                      "&question_no=" + questionNo + "&response=" +
                                      StringUtil.GetFileName(FileName);
                            }
                            else
                            {
                                url = "http://www.nomadlogicapp.com/ibrand/GetPicture?project_no=" + _sc.ProjectNo +
                                      "&question_no=" + questionNo + "&response=" +
                                      StringUtil.GetFileName(FileName);
                            }
                        }

                        status = service.SendTweet(new SendTweetOptions { Status = tmpText + " " + url });

                    }

                    if (status != null)
                    {
                        success = true;
                    }
                    else
                    {
                        log.Warn("Twitter send message failed: " + service.Response.Response);
                    }
                }
            }
            catch (System.Exception e)
            {
                log.Error(e);
            }

            OnUploadFinished?.Invoke(success);

            AfterPost(success);
        }

        /// <summary>
        /// Le nom de fichier à utiliser pour envoyer à Twitter, si existant.
        /// </summary>
        public string FileName { get; set; }

        // TODO Idéalement, faire une classe distincte pour cette situation

        /// <summary>
        /// Si vrai, va tenter de faire un partage Twitter à partir de l'identifiant du répondant sur le serveur.
        /// N'est possible que dans un Printstation de type Fusion car c'est la seule façon d'avoir un répondant déjà créé
        /// sur le serveur avec un vidéo Viméo associé.
        /// </summary>
        public bool ShareFromRespondentNo { get; set; }

        /// <summary>
        /// L'identifiant numérique du répondant auquel le partage doit s'effectuer.
        /// </summary>
        public int RespondentNo { get; set; }

        /// <summary>
        /// Le numéro de question qui contient le ID du répondant.
        /// </summary>
        public int RespondentNoQuestionNo { get; set; }
    }
}
