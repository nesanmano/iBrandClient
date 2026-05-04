using System;
using System.Drawing;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using Awesomium.Core;
using Awesomium.Windows.Forms;
using JetBrains.Annotations;
using NLog;
using NomadLogic.Common.Serveur;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Win32;
using Timer = System.Windows.Forms.Timer;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers.ScreenDefinitions.MultimediaEventListing
{
    /// <summary>
    /// Affiche un seul élément sur une forme, pour maximiser sa présentation. On peut sélectionner l'élément en cliquant dessus ou
    /// </summary>
    public partial class MultimediaEventItemVimeoDialog : Form
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private readonly MultimediaEventGalleryRowItem _item;

        private readonly MultimediaEventVimeoPresentation _details;
        private readonly bool _initialSelection;
        
        private delegate void SetStringCallback(string value);
        private delegate void SetErrorCallback(string arg1, string arg2);

        private WebControl _webControl;
        private PictureBox _hourglassPictureBox;

        private static readonly Uri blank = new Uri("about:blank");
        private Timer _timer;
        private readonly ServerConfiguration _serverConf;
        private PictureBox _statusButton;
        private const string HTML_ERROR_PART_01 = "<html><head><title>";
        private const string HTML_ERROR_PART_02 = "</title></head><body>";
        private const string HTML_ERROR_PART_03 = "</body></html>";

        private const string VIMEO_PREFIX = "vimeo:";
        private const string BASE_VIMEO_URL_VIEWER = "/vimeo-playback";

        private static string BuildErrorHtml(string header, string message)
        {
            return HTML_ERROR_PART_01 + header + HTML_ERROR_PART_02 + message + HTML_ERROR_PART_03;
        }

        public MultimediaEventItemVimeoDialog([NotNull] MultimediaEventGalleryRowItem item,
            [NotNull] MultimediaEventVimeoPresentation vimeoPresentationDetail, ServerConfiguration serverConf)
        {
            InitializeComponent();
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.DoubleBuffer, true);
            FormBorderStyle =  FormBorderStyle.None;
            WindowState = FormWindowState.Maximized;
            MaximizeBox = false;

            _item = item;
            _initialSelection = item.Selected;
            _details = vimeoPresentationDetail;
            _serverConf = serverConf;

            DialogResult = DialogResult.None;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            BackgroundImage = FileUtil.LoadImage(Path.Combine(_details.AssetsDirectory, _details.BackgroundImageFileName));

            var img = FileUtil.LoadImage(Path.Combine(_details.AssetsDirectory, _details.QuitButtonImageFileName));

            var exitButton = new PictureBox
            {
                Location = _details.QuitButtonPosition.Location,
                SizeMode = PictureBoxSizeMode.AutoSize,
                BackColor = _details.QuitButtonBackColor,
                Image = img
            };

            exitButton.MouseClick += (sender, args) =>
            {
                StopTimer();
                // Stoppe le chargement de la page en cours.
                _webControl.Stop();
                _webControl.Dispose();
                DialogResult = _initialSelection != _item.Selected ? DialogResult.OK : DialogResult.None;
                Close();
            };
            Controls.Add(exitButton);

            var session = WebCore.CreateWebSession( new WebPreferences {
                Javascript = false,
                Plugins = false
            });

            var loc = _details.WebControlZone.Location;
            var siz = _details.WebControlZone.Size;

            _webControl = new WebControl {WebSession = session};
            Controls.Add(_webControl);
            _webControl.BringToFront();

            _webControl.Location = loc;
            _webControl.Size = siz;
            _webControl.DocumentReady += LoadCompletedEventHandler;
            _webControl.WindowClose += ClosingEvent;
            _webControl.LoadingFrameComplete += ApplyZoom;
            _webControl.AddressChanged += OnWebAdressChanged;

            /*
             * Autres contrôles
             */
            Image hourglass;
            if (string.IsNullOrWhiteSpace(_details.LoadingImageFileName))
            {
                var path = Path.Combine(DirectoryUtil.GetLocalNomadLogicAssetsDir(), "hourglass_black.png");
                hourglass = FileUtil.LoadImage(path);
            }
            else
            {
                hourglass = FileUtil.LoadImage(Path.Combine(_details.AssetsDirectory, _details.LoadingImageFileName));
            }
            
            _hourglassPictureBox = new PictureBox
            {
                Image = hourglass,
                Width = hourglass.Width,
                Height = hourglass.Height,
                Location = new Point(loc.X + (siz.Width / 2) - (hourglass.Width / 2), loc.Y + siz.Height / 2)
            };
            Controls.Add(_hourglassPictureBox);

            _statusButton = new PictureBox
            {
                Location = _details.SelectionStatusButtonZone.Location,
                Size = _details.SelectionStatusButtonZone.Size
            };
            _statusButton.MouseClick += (sender,args) => StatusButtonOnMouseClick();
            Controls.Add(_statusButton);
            // Appel de la création de l'état du bouton immédiatement
            StatusButtonOnMouseClick(true);

            /* Création du URL du visionneur. EX: http://192.95.21.208:9188/vimeo-playback?videoID=582239210 */
            var url = _serverConf.GetRestUrl(_details.CustomVimeoViewerUrl ?? BASE_VIMEO_URL_VIEWER);
            var stringVimeoId = _item.FixedMainData.StartsWith(VIMEO_PREFIX) ? _item.FixedMainData.Substring(VIMEO_PREFIX.Length) : _item.FixedMainData;

            // Todo à tester.
            url += $"?videoID={stringVimeoId}";

            //NaviguateToUrl(url);
            NaviguateToUrl("http://translate.google.com");
        }

        private void StatusButtonOnMouseClick(bool ignoreToggle = false)
        {
            if (!ignoreToggle)
            {
                _item.Selected = !_item.Selected;
            }

            var bmp = FileUtil.LoadBitmap(_item.Selected ? _details.SelectionActivatedImageFileName : _details.SelectionUnselectedImageFileName);

            _statusButton.SafeCall(() =>
            {
                var old = _statusButton.Image;
                _statusButton.Image = bmp;
                old?.Dispose();
                _statusButton.Refresh();
                log.Trace("Refreshed Vimeo item selection status.");
            });
        }

        private void OnWebAdressChanged(object sender, UrlEventArgs e)
        {
            log.Trace("New web page event.");
            
            if (e.Url.ToString().Equals(blank.ToString())) return;

            StartTimer();
        }

        private void ApplyZoom(object sender, FrameEventArgs e)
        {
            log.Trace("Zoom applied: " + _details.WebControlZoomFactor);
            _webControl.Zoom = _details.WebControlZoomFactor;
        }

        private void ClosingEvent(object sender, WindowCloseEventArgs e)
        {
            log.Trace("Closing event");
            StopTimer();
            Close();
        }

        private void LoadCompletedEventHandler(object sender, DocumentReadyEventArgs e)
        {
            log.Trace("Load completed event");
            StopTimer();
            _hourglassPictureBox.Visible = false;
            _hourglassPictureBox.SendToBack();
            Invalidate();
        }

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
                    log.Trace("Error page show from start on browser.");
                }
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        private void StartTimer()
        {
            if (_timer != null) return;

            _timer = new Timer
            {
                Enabled = true,
                Interval = _details.WebControlConnectionTimeOutMili
            };

            _timer.Tick += (s, ev) =>
            {
                if (_timer == null) return;
                StopTimer();
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
    }
}
