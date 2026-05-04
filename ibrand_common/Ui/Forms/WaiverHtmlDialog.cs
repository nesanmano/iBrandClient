using System;
using System.Drawing;
using System.Windows.Forms;
using Awesomium.Core;
using Awesomium.Windows.Forms;
using NomadLogic.Common.Util;
using Button = System.Windows.Forms.Button;

namespace NomadLogic.Common.Ui.Forms
{
    /// <summary>
    /// Affiche le contenu d'un fichier ou d'une page web dans une forme. Cette forme possède des contrôles pour fermer immédiatement
    /// la forme elle-même.
    /// </summary>
    public partial class WaiverHtmlDialog : Form
    {
        private static readonly NLog.Logger log = NLog.LogManager.GetCurrentClassLogger();
        private readonly WebControl _webControl;

        /// <summary>
        /// Constructeur principal.
        /// <param name="htmlFile">Le ficheir Html (ou URL complet).</param>
        /// </summary>
        public WaiverHtmlDialog(string htmlFile)
        {
            InitializeComponent();

            var path = DirectoryUtil.GetLocalPhotoBoothDefaultAssetsDir(null) + "close.png";
            var closeButtonImage = FileUtil.LoadBitmap(path);
            
            Size = new Size(Screen.PrimaryScreen.WorkingArea.Width, Screen.PrimaryScreen.WorkingArea.Height);
            Location = new Point(0, 0);

            _webControl = new WebControl();
            Controls.Add(_webControl);
            _webControl.Size = new Size(Size.Width, Size.Height);
            _webControl.WindowClose += ClosingEvent;
            _webControl.Source = new Uri(htmlFile);

            log.Info("Loaded following data for HtmlWaiverDialog: {0}", htmlFile);

            // Doit être après le controlleur web.
            var closeButton = new Button
            {
                Location = new Point(Size.Width - 20 - closeButtonImage.Size.Width, 20),
                Size = closeButtonImage.Size,
                Image = closeButtonImage,
                TabStop = false,
                FlatStyle = FlatStyle.Flat,
            };

            closeButton.FlatAppearance.BorderSize = 0;
            closeButton.FlatAppearance.BorderColor = Color.FromArgb(0, 255, 255, 255);

            closeButton.MouseClick += MouseDownHandler;
            Controls.Add(closeButton);
            closeButton.BringToFront();
        }

        private void ClosingEvent(object sender, WindowCloseEventArgs e)
        {
            _webControl.Stop();
            _webControl.Dispose();
            Close();
        }

        /// <summary>
        /// Pour les événements souris.
        /// </summary>
        /// <param name="sender">L'envoyeur</param>
        /// <param name="e">Les paramètres d'événement.</param>
        private void MouseDownHandler(object sender, MouseEventArgs e)
        {
            _webControl.Stop();
            _webControl.Dispose();
            Close();
        }
    }
}
