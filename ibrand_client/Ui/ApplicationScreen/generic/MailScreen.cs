using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using JetBrains.Annotations;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Ui;
using NomadLogic.Common.Ui.Keyboard;
using NomadLogic.Common.Util;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic
{
    /// <summary>
    /// Écran pour saisir un courriel, d'où son nom. Il agit comme un écran, pseudo modal,
    /// en ce sens qu'il ne permet pas d'avancer dans la liste d'écran, mais qu'il est obligatoire de revenir en arrière d'un certain nombre
    /// d'écran.
    /// </summary>
    [PublicAPI, Obsolete("Utilisez la classe EmailScreen2 à la place de cette classe.")]
    public class MailScreen : AbstractScreen
    {
        private readonly Rectangle _previousArea;

        private TextBox _mailTextBox;
        private readonly int _nbrPageToGoBack;
        private readonly Bitmap _checkmark;
        private readonly List<ScreenCheckOption> _zones = new List<ScreenCheckOption>();

        /// <summary>
        /// Constrcuteur principal.
        /// </summary>
        public MailScreen(string backgroundImageSrcEn, ScreenConfig sc, int nbrPageToGoBack, Rectangle textBoxLocation, Rectangle previousLocation,
            KeyboardControl keyboard, IEnumerable<ScreenCheckOption> optInOptions = null, string backgroundImageSrcFr = null)
            : base(backgroundImageSrcEn, backgroundImageSrcFr ?? backgroundImageSrcEn, sc)
        {
            if (optInOptions != null)
            {
                _zones.AddRange(optInOptions);
                if (_zones.Any())
                {
                    _checkmark = GetNoneLanguageBitmap("checkmark.png");
                }
            }

            AddTextBox(textBoxLocation);
            AddKeyboard(keyboard);
            
            _previousArea = previousLocation;
            _nbrPageToGoBack = nbrPageToGoBack;
        }

        /// <inheritdoc/>
        protected override void OnPaintEvent(Graphics g)
        {
            base.OnPaintEvent(g);

            foreach (var zone in _zones.Where(zone => zone.AnswerState))
            {
                DrawImage(g, _checkmark, zone.CheckLocation);
            }
        }

        /// <summary>
        /// Indique, si vrai, s'il faut aller par en avant au lieu de revenir à un écran antérieur quand on active le bouton
        /// de confirmation.
        /// </summary>
        [Obsolete]
        public bool MoveForward { get; set; }

        /// <summary>
        /// Si en plus de l'option de revenir en arrière, on peut avoir une zone optionnelle pour avancer.
        /// </summary>
        public Rectangle? NextPageZone { get; set; }

        /// <summary>
        /// Indique les options de changement de page sur l'action "Next".
        /// </summary>
        public ScreenChangeOptions NextPageOptions { get; set; }

        /// <summary>
        /// Indique les options de changement de page sur l'action "Back".
        /// </summary>
        public ScreenChangeOptions BackPageOptions { get; set; }
        
        /// <summary>
        /// Si en plus de l'option de revenir en arrière, on peut avoir une zone optionnelle pour avancer.
        /// </summary>
        public Rectangle? CancelZone { get; set; }

        /// <summary>
        /// Indique que le courriel est obligatoire. Si on entre dans cette page, on doit la quitter avec un courriel valide.
        /// </summary>
        public bool MandatoryEmail { get; set; }

        /// <inheritdoc />
        public override void Recycle()
        {
            base.Recycle();

            _mailTextBox.Text = "";
            
            foreach (var zone in _zones)
            {
                zone.Recycle();
            }
        }

        private void AddKeyboard(KeyboardControl keyboard) {
            var key = keyboard ?? new KeyboardControl(0, 390, 1239, 335);
            
            key.Parent = PictureBox;

            key.BringToFront();
            key.Show();
        }

        private bool SetAnswersToQuestionnaire()
        {
            if (!AllMandatoryChecks(_zones))
            {
                var mBox = new MessageDialog();
                mBox.ShowMessage(Resources.iBrand.NotAllMandatoryCheckEnabled);
                return false;
            }

            if (_mailTextBox.Text.Length <= 0 && !MandatoryEmail) return true;

            if (StringUtil.IsValidEmail(_mailTextBox.Text))
            {
                if (_screenConfig.QuestionEmailNo != null)
                    SaveAnswerToQuestion(new Answer{QuestionNo = _screenConfig.QuestionEmailNo.Value, Response = _mailTextBox.Text});
                else
                {
                    var mBox = new MessageDialog();
                    mBox.ShowMessage(_activeLg.Equals("fr") 
                        ? "Le courriel ne peut être sauvegardé. Vérifiez les configurations du questionnaire." 
                        : "Email cannot be saved. Check questionnaire settings.");
                    return false;
                }
            }
            else
            {
                var mBox = new MessageDialog();
                mBox.ShowMessage(_activeLg.Equals("fr") ? "Adresse courriel non valide." : "Invalid email");
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

        /// <inheritdoc />
        protected override bool OnMouseDownHandler(int x, int y)
        {
            if (!base.OnMouseDownHandler(x, y)) return false;

            if (InArea(x, y, _previousArea))
            {
                if (!SetAnswersToQuestionnaire()) return false;

                var step = _nbrPageToGoBack;

                var tmp = NextPageOptions?.Evaluate(GetCurrentRespondentWithAnswers());
                if (tmp != null) step = tmp.Step;
                
#pragma warning disable 612
                if (!MoveForward)
#pragma warning restore 612
                {
                    Back(step);
                    return false;
                }

                Next(step);
                return false;
            }

            if (InArea(x, y, NextPageZone))
            {
                if (!SetAnswersToQuestionnaire()) return false;

                var step = 1;

                var tmp = NextPageOptions?.Evaluate(GetCurrentRespondentWithAnswers());
                if (tmp != null) step = tmp.Step;

                Next(step);
                return false;
            }

            if (InArea(x, y, CancelZone))
            {
                FirstScreen();
                return false;
            }

            EvaluateCheckBoxsMouseDown(x, y, _zones);

            return true;
        }

        /// <inheritdoc />
        public override void SetActivated()
        {
            base.SetActivated();
            _mailTextBox.Focus();
        }
    }
}
