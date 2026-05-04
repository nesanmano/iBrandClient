using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using NLog;
using NomadLogic.Common.Ui;
using NomadLogic.Common.Ui.Forms;
using NomadLogic.Common.Ui.Keyboard;
using NomadLogic.Common.Util;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers;

namespace NomadLogic.PrintStation.Ui.generic
{
    /// <summary>
    /// Écran pour saisir un courriel, d'où son nom.
    /// </summary>
    public class MailScreen : AbstractPrintStationScreen
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        private readonly Rectangle _previousArea;
        private readonly Rectangle? _privacyArea;

        /// <summary>
        /// Boîte de saisie du texte.
        /// </summary>
        protected TextBox _mailTextBox;
        
        private readonly int _nbrPageToGoBack;

        private readonly Bitmap _checkmark;
        private readonly List<ScreenCheckOption> _zones = new List<ScreenCheckOption>();
        private readonly string _privacyFileName;

        /// <summary>
        /// Indique, si vrai, s'il faut aller par en avant au lieu de revenir à un écran antérieur quand on active le bouton
        /// de confirmation.
        /// </summary>
        public bool MoveForward { get; set; }


        public bool Mandatory { get;set;}

        /// <summary>
        /// Indique si on a une zone pour canceller dans cette page. Nul par défaut.
        /// </summary>
        public Rectangle? CancelArea { get; set; }

        /// <summary>
        /// Constructeur principal.
        /// </summary>
        /// <param name="backgroundImageEnSrc">Fond en anglais</param>
        /// <param name="backgroundImageFrSrc">Fond en français</param>
        /// <param name="sc">Paramètre d'écrans globaux</param>
        /// <param name="nbrPageToGoBack">Nombre de pages à revenir en arrière.</param>
        /// <param name="textBoxLocation">Rectangle de position de la boîte de saisie du texte.</param>
        /// <param name="previousLocation">Rectangle de position pour revenir en arrière.</param>
        /// <param name="keyboard">Rectangle (approximatif) de position pour le clavier.</param>
        /// <param name="optInOptions">Optionnel. Zones d'option par checkbox.</param>
        /// <param name="privacyArea">Optionel. Un rectangle où on affiche un message de disclaimer via un dialogue HTML. Le contenu est géré par le prochain paramètre.</param>
        /// <param name="privacyHtmlFileName">Fichier Html simple à afficher par dialogue avec la zone de "privacyArea". Optionel. Par défaut "privacy.html" dans le répertoire de "photobooth_ui"</param>
        public MailScreen(string backgroundImageEnSrc, string backgroundImageFrSrc, PrintStationScreenConfig sc, int nbrPageToGoBack, Rectangle textBoxLocation, Rectangle previousLocation,
            KeyboardControl keyboard = null, IList<ScreenCheckOption> optInOptions = null, Rectangle? privacyArea = null, string privacyHtmlFileName = "privacy.html")
            : base(backgroundImageEnSrc, backgroundImageFrSrc, sc)
        {
            AddTextBox(textBoxLocation);
            AddKeyboard(keyboard);
            _previousArea = previousLocation;
            _privacyArea = privacyArea;
            _privacyFileName = privacyHtmlFileName;
            _nbrPageToGoBack = nbrPageToGoBack;

            if (optInOptions == null) return;
            _zones.AddRange(optInOptions);
            if (_zones.Any())
            {
                _checkmark = GetNoneLanguageBitmap("checkmark.png");
            }
        }

        protected override void OnPaintEvent(Graphics g)
        {
            base.OnPaintEvent(g);
            PaintCheckBoxList(g, _zones, _checkmark);
        }

        /// <inheritdoc/>
        public override void Recycle()
        {
            base.Recycle();
            _mailTextBox.Text = "";
            
            foreach (var zone in _zones)
            {
                zone.Recycle();
            }
        }

        private void AddKeyboard(KeyboardControl keyboard)
        {
            var key = keyboard ?? new KeyboardControl(0, 355, 1239, 335);
            
            key.Parent = PictureBox;
            key.BringToFront();
            key.Show();
        }

        /// <summary>
        /// Sauvegarde les informations dans le questionnaire.
        /// </summary>
        /// <returns>TRUE si toute la validation est correcte.</returns>
        protected virtual bool SetAnswersToQuestionnaire()
        {
            //if (_mailTextBox.Text.Length <= 0) return true;

            if (!AllMandatoryChecks(_zones))
            {
                var mBox = new MessageDialog();
                mBox.ShowMessage(Resources.printstation.NotAllMandatoryCheckEnabled);
                return false;
            }

            if(Mandatory && string.IsNullOrWhiteSpace(_mailTextBox.Text))
            {
                var mBox = new MessageDialog();
                mBox.ShowMessage(_activeLg.Equals("en", StringComparison.CurrentCultureIgnoreCase)?"Email Is Mandatory":"Courriel est requis");
                return false;
            }

            if (Mandatory && StringUtil.IsValidEmail(_mailTextBox.Text))
            {
                //if (_screenConfig.QuestionEmailNo != null) SaveAnswerToQuestion(_screenConfig.QuestionEmailNo.Value, 1,_mailTextBox.Text);
            }
            else
            {
                var mBox = new MessageDialog();
                mBox.ShowMessage(_activeLg.Equals("en") ? Resources.printstation.InvalidEmail:"Le courriel est invalide");
                return false;
            }

            return true;
        }

        private void AddTextBox(Rectangle textBoxLocation)
        {
            _mailTextBox = new TextBox
            {
                BorderStyle = BorderStyle.FixedSingle,
                Location = textBoxLocation.Location,
                Size = textBoxLocation.Size,
                TabIndex = 1,
                Font = new Font(FontFamily.GenericSansSerif, 26)
            };
            _mailTextBox.TextChanged += OnTextChanged;

            Controls.Add(_mailTextBox);
            _mailTextBox.BringToFront();
        }

        protected override bool OnMouseDownHandler(int x, int y)
        {
            if (!base.OnMouseDownHandler(x, y)) return false;

            if (InArea(x, y, _previousArea))
            {
                if (!SetAnswersToQuestionnaire()) return false;

                if (!MoveForward)
                {
                    Back(_nbrPageToGoBack);
                }
                else
                {
                    Next(_nbrPageToGoBack);
                }

                return false;
            }

            if (InArea(x, y, CancelArea))
            {
                log.Trace("Cancel Area clicked");
                Close();
                return false;
            }

            if (_privacyArea != null && InArea(x, y, _privacyArea))
            {
                SuspendTimer();
                var file = GetTerminalDirectoryDataFile(_privacyFileName, DirectoryUtil.ProjectDataDirectoriesEnum.photobooth_ui);
                new WaiverHtmlDialog(file).ShowDialog();
                ResumeTimer();
                return false;
            }

            return EvaluateCheckBoxsMouseDown(x, y, _zones);
        }

        /// <inheritdoc/>
        public override void SetActivated()
        {
            base.SetActivated();
            _mailTextBox.Focus();
        }
    }
}
