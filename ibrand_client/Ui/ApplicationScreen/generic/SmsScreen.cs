using System;
using System.Drawing;
using System.Windows.Forms;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Ui;
using NomadLogic.Common.Ui.Keyboard;
using NomadLogic.Common.Util;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic
{
    /// <summary>
    /// Écran pour saisir un numéro de téléphone, généralement pour fin de Sms, d'où son nom. Il agit comme un écran, pseudo modal,
    /// en ce sens qu'il ne permet pas d'avancer dans la liste d'écran, mais qu'il est obligatoire de revenir en arrière d'un certain nombre
    /// d'écran.
    /// </summary>
    [Obsolete]
    public class SmsScreen : AbstractScreen
    {
        private readonly Rectangle _previousArea;
        private TextBox _smsTextBox;
        private readonly int _nbrPageToGoBack;

        /// <summary>
        /// Constructeur principal
        /// </summary>
        /// <param name="backgroundImageEnSrc">Fond d'écran en anglais</param>
        /// <param name="sc">Configuration de présentation globale</param>
        /// <param name="nbrPageToGoBack">Nombre de page à revenir en arrière.</param>
        /// <param name="textBoxLocation">Emplacement de la boîte de texte.</param>
        /// <param name="previousLocation">Zone de retour et validation.</param>
        /// <param name="keyboard">Information sur le keyboard.</param>
        /// <param name="backgroundImageFrSrc">Fond d'écran en francais</param>
        public SmsScreen(string backgroundImageEnSrc, ScreenConfig sc, int nbrPageToGoBack,
            Rectangle textBoxLocation, Rectangle previousLocation, KeyboardControl keyboard = null, string backgroundImageFrSrc = null)
            : base(backgroundImageEnSrc, backgroundImageFrSrc ?? backgroundImageEnSrc, sc)
        {
            AddTextBoxs(textBoxLocation);
            AddKeyboard(keyboard);

            _previousArea = previousLocation;
            _nbrPageToGoBack = nbrPageToGoBack;
        }

        /// <inheritdoc/>
        public override void Recycle()
        {
            base.Recycle();

            _smsTextBox.Text = "";
        }

        private void AddKeyboard(KeyboardControl keyboard)
        {
            var key = keyboard ?? new KeyboardControl(0, 390, 1239, 335);
            
            key.Parent = PictureBox;

            key.BringToFront();
            key.Show();
        }

        private bool SetAnswersToQuestionnaire()
        {
            if (_smsTextBox.Text.Length <= 0) return true;

            if (StringUtil.IsValidPhoneNumber(_smsTextBox.Text))
            {
                if (_screenConfig.QuestionSmsNo != null)
                {
                    SaveAnswerToQuestion(new Answer{QuestionNo = _screenConfig.QuestionSmsNo.Value, Response = _smsTextBox.Text});
                }
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

        /// <inheritdoc/>
        protected override bool OnMouseDownHandler(int x, int y)
        {
            if (!base.OnMouseDownHandler(x, y)) return false;

            if (InArea(x, y, _previousArea))
            {
                if (!SetAnswersToQuestionnaire()) return false;
                Back(_nbrPageToGoBack);
                return false;
            }

            return true;
        }

        /// <inheritdoc/>
        public override void SetActivated()
        {
            base.SetActivated();

            _smsTextBox.Focus();
        }
    }
}
