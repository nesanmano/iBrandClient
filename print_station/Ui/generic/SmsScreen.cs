using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using NomadLogic.Common.Ui;
using NomadLogic.Common.Ui.Forms;
using NomadLogic.Common.Ui.Keyboard;
using NomadLogic.Common.Util;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers;

namespace NomadLogic.PrintStation.Ui.generic
{
    /// <summary>
    /// Écran pour saisir un numéro de téléphone, généralement pour fin de Sms, d'où son nom. Il agit comme un écran, pseudo modal,
    /// en ce sens qu'il ne permet pas d'avancer dans la liste d'écran, mais qu'il est obligatoire de revenir en arrière d'un certain nombre
    /// d'écran.
    /// </summary>
    public class SmsScreen : AbstractPrintStationScreen
    {
        readonly Rectangle _previousArea;
        readonly Rectangle? _privacyArea;

        private TextBox _smsTextBox;
        
        private readonly int _nbrPageToGoBack;

        private readonly Bitmap _checkmark;
        private readonly List<ScreenCheckOption> _zones = new List<ScreenCheckOption>();
        private readonly string _privacyFileName;

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
        /// <param name="keyboard">Optionnel. Clavier custom pour la forme.</param>
        /// <param name="optInOptions">Optionnel. Zones d'option par checkbox.</param>
        /// <param name="privacyArea">Optionel. Un rectangle où on affiche un message de disclaimer via un dialogue HTML. Le contenu est géré par le prochain paramètre.</param>
        /// <param name="privacyHtmlFileName">Fichier Html simple à afficher par dialogue avec la zone de "privacyArea". Optionel. Par défaut "privacy.html" dans le répertoire de "photobooth_ui"</param>
        public SmsScreen(string backgroundImageEnSrc, string backgroundImageFrSrc, PrintStationScreenConfig sc, int nbrPageToGoBack,
            Rectangle textBoxLocation, Rectangle previousLocation, KeyboardControl keyboard = null, IList<ScreenCheckOption> optInOptions = null,
            Rectangle? privacyArea = null, string privacyHtmlFileName = "privacy.html")
            : base(backgroundImageEnSrc, backgroundImageFrSrc, sc)
        {
            AddTextBoxs(textBoxLocation);
            AddKeyboard(keyboard);

            //PictureBox.MouseClick += MouseDownHandler;

            _previousArea = previousLocation;
            _nbrPageToGoBack = nbrPageToGoBack;
            _privacyArea = privacyArea;
            _privacyFileName = privacyHtmlFileName;

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

            _smsTextBox.Text = "";
            
            foreach (var zone in _zones)
            {
                zone.Recycle();
            }
        }

        private void AddKeyboard(KeyboardControl keyboard)
        {
            var key = keyboard ?? new KeyboardControl(0, 335, 1239, 355);
            
            key.Parent = PictureBox;
            key.BringToFront();
            key.Show();
        }

        private bool SetAnswersToQuestionnaire()
        {
            if (_smsTextBox.Text.Length <= 0) return true;

            if (!AllMandatoryChecks(_zones))
            {
                var mBox = new MessageDialog();
                mBox.ShowMessage(Resources.printstation.NotAllMandatoryCheckEnabled);
                return false;
            }

            if (StringUtil.IsValidPhoneNumber(_smsTextBox.Text))
            {
                //if (_screenConfig.QuestionSmsNo != null) SaveAnswerToQuestion(_screenConfig.QuestionSmsNo.Value, 1, _smsTextBox.Text);
            }
            else
            {
                var mBox = new MessageDialog();
                mBox.ShowMessage(_activeLg.Equals("fr") ? "Numéro de téléphone non valide." : "Invalid phone number");
                return false;
            }

            return true;
        }

        private void AddTextBoxs(Rectangle textBoxLocation)
        {
            _smsTextBox = new TextBox
            {
                BorderStyle = BorderStyle.FixedSingle,
                Location = textBoxLocation.Location,
                Size = textBoxLocation.Size,
                TabIndex = 1,
                Font = new Font(FontFamily.GenericSansSerif, 26)
            };
            _smsTextBox.TextChanged += OnTextChanged;

            Controls.Add(_smsTextBox);
            _smsTextBox.BringToFront();
        }

        protected override bool OnMouseDownHandler(int x, int y)
        {
            if (!base.OnMouseDownHandler(x, y)) return false;

            if (InArea(x, y, _previousArea))
            {
                if (!SetAnswersToQuestionnaire()) return false;

                Back(_nbrPageToGoBack);
                return false;
            }
            if (InArea(x, y, CancelArea))
            {
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

            _smsTextBox.Focus();
        }
    }
}
