using System;
using System.Drawing;
using System.Windows.Forms;
using JetBrains.Annotations;
using NLog;
using NomadLogic.Common.Ui.Keyboard;
using NomadLogic.Common.Util;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers;

// ReSharper disable InconsistentNaming

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.custom.Diamond
{
    /// <summary>
    /// Écran de sélection générique avec différentes zones.
    /// </summary>
    [PublicAPI]
    public class BackgroundSelectionScreen_Dia201901 : BackgroundSelectionScreen
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        private TextBox _customQuote;

        /// <summary>
        /// Constructeur par défaut.
        /// </summary>
        /// <param name="backgroundImgEnSrc">Fond anglais</param>
        /// <param name="backgroundImgFrSrc">Fond français</param>
        /// <param name="sc">Configuration</param>
        public BackgroundSelectionScreen_Dia201901(string backgroundImgEnSrc, string backgroundImgFrSrc, ScreenConfig sc)
            : base(backgroundImgEnSrc, backgroundImgFrSrc, sc)
        { }

        public override bool HasAnswer => 
            (_selectedOption?.AnswersUponSelection != null && 
            _selectedOption?.AnswersUponSelection?.Count > 0) ||
            !string.IsNullOrWhiteSpace(_customQuote.Text);

        protected override void OnLoad()
        {
            base.OnLoad();

            _customQuote = new TextBox
            {
                Multiline = true
            };
            var font = new Font(FontFamily.GenericSansSerif, 20);
            ScreenUtil.InitTextBox(this, _customQuote, new Rectangle(274, 700, 569, 100), font, 1);
            _customQuote.GotFocus += OnCustomQuoteOnGotFocus;
            _customQuote.TextChanged += OnCustomQuoteOnGotFocus;

            var key = new UsaKeyboardControl(-1, 809, 1000, 493)
            {
                KeyPadding = -5
                
            };
            key.CustomLabelInfo.Font = StaticCustomFontRepository.CreateCustomFont(new FontDefinitionInfo
            {
                Size = 25
            });

            AddKeyboardToControls(key);
        }

        private void OnCustomQuoteOnGotFocus(object sender, EventArgs args)
        {
            // On a sélectionné le texte, on annule la sélection de quote
            _selectedOption = null;
            Refresh();
        }

        protected override bool OnMouseDownHandler(int x, int y)
        {
            if (InArea(x, y, NextArea) && HasAnswer && !string.IsNullOrWhiteSpace(_customQuote.Text)) // Si a l'intérieur du "Next"
            {
                //SaveAnswerToQuestion(5, 1, _customQuote.Text);
            }
            else if (IsQuestionAnswered(5))
            {
                ResetQuestionAnswers(5);
            }

            if (!base.OnMouseDownHandler(x, y))
            {
                // Si quelque chose à changé dans les options de quote
                if (_selectedOption == null) return false;
                // On vide le texte.
                _customQuote.TextChanged -= OnCustomQuoteOnGotFocus;
                _customQuote.Text = "";
                _customQuote.TextChanged += OnCustomQuoteOnGotFocus;

                return false;
            }

            return true;
        }


        public override void Recycle()
        {
            base.Recycle();
            _selectedOption = null;
            _customQuote.Text = "";
        }
    }
}
