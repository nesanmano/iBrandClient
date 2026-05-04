using System;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using Awesomium.Core;
using Awesomium.Windows.Forms;
using NLog;
using NomadLogic.Common.Ui.Keyboard;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic
{
    /// <summary>
    /// Écran avec un browser web tout usage.
    /// On revient immédiatement vers la page appellante quand on appuis sur le bouton X rouge, ou si une commande
    /// javascript de la page demande une fermeture.
    /// On peut aussi demander une interception de code, qui si trouvé, provoque aussi la fermeture. Cette interception de code
    /// n'est faite qu'une fois, puis est remise à zéro.
    /// La session est désactivée par défaut, car ce genre d'écran est réutilisable par de multiples demandes.
    /// </summary>
    public class WebBrowserScreen : AbstractPseudoModalScreen
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private const int MARGIN = 10;

        private delegate void SetBooleanCallback(bool value);
        private delegate void SetStringCallback(string value);
        private delegate void SetErrorCallback(string arg1, string arg2);

        private readonly WebControl _webControl;
        private readonly PictureBox _hourglassPictureBox;
        private readonly PictureBox _closePictureBox;

        /// <summary>
        /// Événement lancé quand un usager du browser clique sur un bouton, tel que détecté par Javascript.
        /// </summary>
        public EventHandler ButtonClicked;

        //private readonly int _cutoff;
        private static readonly Uri blank = new Uri("about:blank");
        private Timer _timer;
        private const string HTML_ERROR_PART_01 = "<html><head><title>";
        private const string HTML_ERROR_PART_02 = "</title></head><body>";
        private const string HTML_ERROR_PART_03 = "</body></html>";

        /// <summary>
        /// Temps maximal d'attente avant d'afficher un timeout et de bloquer la naviguation.
        /// En millisecondes. Par défaut 45 secondes d'attente.
        /// </summary>
        public int ConnectionTimeOut { get; set; } = 45 * 1000;

        private static string BuildErrorHtml(string header, string message)
        {
            return HTML_ERROR_PART_01 + header + HTML_ERROR_PART_02 + message + HTML_ERROR_PART_03;
        }

        /// <summary>
        /// Si on veut intercepter un élément texte dans le résultat HTML d'une des pages potentielle.
        /// Quand une page est chargée, on analyse son contenu pour un TAG avec le nom exact de cette propriété.
        /// Puis on met dans la Proriété TagToInterceptResult le résultat obtenu. Ce résultat peut être nul.
        /// </summary>
        public string TagToIntercept { get; set; }

        /// <summary>
        /// Si la page web à afficher devient la page indiquée dans cette propriété, on assume une fin de fonctionalité et on revient
        /// à la page précédente.
        /// Dans certains cas, comme la présence de certains mot-clés comme erreur dans le URL, on ne fait rien.
        /// Moins "fool-proof" que l'interception d'une valeur de "tag" spécifique.
        /// </summary>
        public string UrlToIntercept { get; set; }

        /// <summary>
        /// Résultat d'une interception.
        /// </summary>
        public string TagToInterceptResult
        {
            get;
            private set;
        }

        /// <summary>
        /// Indique le niveau de zoom que l'on veut avoir pour le rendu HTML. Est un entier, et représente des centièmes de pourcentage.
        /// En gros, le défaut est 100 (pour 100%).
        /// </summary>
        public int Zoom
        {
            get;
            set;
        }

        public WebBrowserScreen(ScreenConfig sc, Size screensize) : this(sc, screensize, null)
        {
            // Rien de spécial
        }

        public WebBrowserScreen(ScreenConfig sc, Size screensize, Size keyboardSize) : this(sc, screensize, new KeyboardControl { Size = keyboardSize} )
        {
            // Rien de spécial
        }

        public WebBrowserScreen(ScreenConfig screenConfig, Size screensize, KeyboardControl keyboard, bool disableJavascript = false) : base(screensize, screenConfig)
        {
            BackColor = Color.White;
            Zoom = 100;
            Size = screensize;

            var cutoff = Size.Height;

            if (keyboard != null)
            {
                cutoff -= keyboard.Height + MARGIN;
            }

            /*
             * Controlleur web
             */
            var session = WebCore.CreateWebSession( new WebPreferences {
                              Javascript = !disableJavascript,
                              Plugins = false
                          });

            _webControl = new WebControl {WebSession = session};
            Controls.Add(_webControl);
            _webControl.BringToFront();
           
            _webControl.Size = new Size(Size.Width, cutoff);
            _webControl.DocumentReady += LoadCompletedEventHandler;
            _webControl.WindowClose += ClosingEvent;
            _webControl.LoadingFrameComplete += ApplyZoom;
            _webControl.AddressChanged += OnWebAdressChanged;

            /*
             * Autres contrôles
             */
            var hourglass = GetNoneLanguageBitmap("hourglass_black.png");

            _hourglassPictureBox = new PictureBox
            {
                Image = hourglass,
                Width = hourglass.Width,
                Height = hourglass.Height,
                Location = new Point((Size.Width / 2) - (hourglass.Width / 2), Size.Height / 2)
            };
            Controls.Add(_hourglassPictureBox);

            var closeBitmap = GetNoneLanguageBitmap("x_browser.png");

            _closePictureBox = new PictureBox
            {
                Location = new Point(Size.Width - closeBitmap.Width - MARGIN - 7, 50),
                Image = closeBitmap,
                Width = closeBitmap.Width,
                Height = closeBitmap.Height
            };
            Controls.Add(_closePictureBox);
            _closePictureBox.BringToFront();
            _closePictureBox.MouseDown += MouseDownHandlerClose;

            if (keyboard == null) return;

            keyboard.SetLocation(new Point((Size.Width - keyboard.Width) / 2, cutoff));

            Controls.Add(keyboard);
            keyboard.BringToFront();
            keyboard.BackColor = Color.White;
            keyboard.Show();
        }

        private void OnWebAdressChanged(object sender, UrlEventArgs e)
        {
            log.Trace("New web page event.");

            if (UrlToIntercept != null)
            {
                if (e.Url.ToString().IndexOf("error", StringComparison.InvariantCulture) != -1 || e.Url.ToString().IndexOf("check", StringComparison.InvariantCulture) != -1)
                {
                    return;
                }
                if (e.Url.ToString().IndexOf(UrlToIntercept, StringComparison.InvariantCulture) != -1)
                {
                    StopTimer();
                    ReturnToLastScreen();
                }
            }

            if (e.Url.ToString().Equals(blank.ToString())) return;

            StartTimer();
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        private void StartTimer()
        {
            if (_timer != null) return;

            _timer = new Timer
            {
                Enabled = true,
                Interval = ConnectionTimeOut
            };

            _timer.Tick += (s, ev) =>
            {
                if (_timer == null) return;
                StopTimer();
                ShowClose(true);
                _webControl.LoadHTML(BuildErrorHtml("Timeout", "Could not load web page due to slow or cut connection to the internet."));
                log.Trace("Timeout detected. Showing error message and Exit button.");
            };
            _timer.Start();
            log.Trace("Timer started");
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        private void StopTimer()
        {
            _timer?.Stop();
            _timer = null;
            log.Trace("Timer stopped");
        }

        private void ApplyZoom(object sender, FrameEventArgs e)
        {
            log.Trace("Zoom applied: " + Zoom);
            _webControl.Zoom = Zoom;
        }

        private void ClosingEvent(object sender, WindowCloseEventArgs e)
        {
            log.Trace("Closing event");
            StopTimer();
            ReturnToLastScreen();
        }

        private void LoadCompletedEventHandler(object sender, DocumentReadyEventArgs e)
        {
            log.Trace("Load completed event");
            StopTimer();
            _hourglassPictureBox.Visible = false;
            _hourglassPictureBox.SendToBack();
            Invalidate();

            // Si on doit intercepter le texte d'un résultat HTML, on le fait ici.
            if (TagToIntercept != null)
            {
                var html = _webControl.HTML;

                try
                {
                    var doc = new HtmlDocument();
                    doc.LoadHtml(html);

                    var codeNodes = doc.DocumentNode.Descendants(TagToIntercept);
                    //On prend le premier élément texte du parent.
                    foreach (var node in codeNodes)
                    {
                        log.Trace("Interception node detected. Closing view: " + TagToIntercept);
                        TagToInterceptResult = node.InnerText;
                        ReturnToLastScreen();
                    }
                }
                catch (System.Exception ex)
                {
                    log.Error(ex);
                }
            }

            if (_webControl.WebSession.Preferences.Javascript) {
                JSObject jsobject = _webControl.CreateGlobalJavascriptObject("jsobject");
                jsobject.Bind("buttonClicked", JSHandler);
            }

        }

        private JSValue JSHandler(object sender, JavascriptMethodEventArgs e)
        {
            ButtonClicked?.Invoke(this, EventArgs.Empty);

            return null;
        }

        protected void MouseDownHandlerClose(object sender, MouseEventArgs e)
        {
            if (_flagForClosing) return;

            if (sender.Equals(_closePictureBox))
            {
                ReturnToLastScreen();
            }
        }

        /// <inheritdoc/>
        protected override void BeforeShowPseudoModal()
        {
            // Rieng
        }

        /// <inheritdoc/>
        protected override void BeforeOnReturnEvent()
        {
            StopTimer();
            // Stoppe le chargement de la page en cours.
            _webControl.Stop();
            _webControl.Source = blank;
            ClearCookies();
        }

        /// <summary>
        /// Indique s'il faut afficher ou non le "X" de sortie, qui ne dépend pas de la page web affichée.
        /// </summary>
        /// <param name="value">true si on veut que ce "X" soit visible et disponible.</param>
        public void ShowClose(bool value)
        {
            // Peut être invoqué de thread non-ui.
            if (InvokeRequired)
            {
                SetBooleanCallback d = ShowClose;
                Invoke(d, value);
            }
            else
            {
                if (value)
                {
                    _closePictureBox.BringToFront();
                    _closePictureBox.Visible = true;
                }
                else
                {
                    _closePictureBox.SendToBack();
                    _closePictureBox.Visible = false;
                }
            }
        }

        /// <summary>
        /// Indique au browser qu'il faut aller vers une nouvelle page.
        /// </summary>
        /// <param name="url">L'adresse à se diriger vers.</param>
        public void NaviguateToUrl(string url)
        {
            // Peut être invoqué de thread non-ui.
            if (InvokeRequired)
            {
                SetStringCallback d = NaviguateToUrl;
                Invoke(d, url);
            }
            else
            {
                log.Trace("New URL to go to: " + url);
                StartTimer();
                _webControl.Source = new Uri(url);

                _hourglassPictureBox.Visible = true;
                _hourglassPictureBox.BringToFront();
                Invalidate();
            }
        }

        /// <summary>
        /// Affiche une page d'erreur le browser. Utile si on se dirigait vers la page et qu'une erreur s'est produite.
        /// </summary>
        /// <param name="header">Le header HTML.</param>
        /// <param name="messageToShow">Le message en texte clair à afficher à l'utilisateur.</param>
        public void NaviguateToErrorPage(string header, string messageToShow)
        {
            // Peut être invoqué de thread non-ui.
            if (InvokeRequired)
            {
                SetErrorCallback d = NaviguateToErrorPage;
                Invoke(d, header, messageToShow);
            }
            else
            {
                try
                {
                    _webControl.LoadHTML(BuildErrorHtml(header, messageToShow));
                }
                finally
                {
                    ShowClose(true);
                    log.Trace("Error page show from start on browser.");
                }
            }
        }

        private void ClearCookies()
        {
            try
            {
                _webControl.WebSession.ClearCookies();
                log.Trace("Cookies cleared");
            }
            catch (System.Exception e)
            {
                log.Error(e);
            }
        }
    }
}
