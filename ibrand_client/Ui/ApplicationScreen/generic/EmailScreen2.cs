using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using JetBrains.Annotations;
using NLog;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Ui;
using NomadLogic.Common.Ui.Keyboard;
using NomadLogic.Common.Util;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic
{
    /// <summary>
    /// Écran pour permettre la saisie d'une réponse via une boîte de texte.
    /// TODO: Utiliser l'écran générique.
    /// </summary>
    [PublicAPI]
    [Obsolete]
    public class EmailScreen2 : AbstractScreen
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        
        private TextBox _mailTextBox;
        private Bitmap _checkmark;
        private IList<ScreenCheckOption> _zones = new List<ScreenCheckOption>();

        /// <summary>
        /// Constrcuteur principal.
        /// </summary>
        public EmailScreen2(string backgroundImageSrcEn, string backgroundImageSrcFr, ScreenConfig sc)
            : base(backgroundImageSrcEn, backgroundImageSrcFr, sc)
        { }

        private void OnLoad(object sender, EventArgs eventArgs)
        {
            if (NextPageOptions == null) NextPageOptions = ScreenChangeOptions.GenerateBasicNextOptions();
            if (BackPageOptions == null) BackPageOptions = ScreenChangeOptions.GenerateBasicBackOptions();
            
            AddTextBox(MailTextBoxLocation);
            
            if (ScreenCheckOptions != null)
            {
                if (_zones.Any())
                {
                    _checkmark = GetNoneLanguageBitmap("checkmark.png");
                }
            }

            AddKeyboardToControls(Keyboard);
        }

        /// <inheritdoc/>
        protected override void OnPaintEvent(Graphics g)
        {
            base.OnPaintEvent(g);
            PaintCheckBoxList(g, _zones, _checkmark);
        }

        /// Font Size global pour tout les éléments visuels de l'écran.
        /// TODO: Faire en sorte que cela peut être sur-chargé par une définition locale de l'élément.
        public int FontSize { get; set; } = 26;

        /// Location du textBox du courriel.
        public Rectangle? MailTextBoxLocation { get; set; }

        /// Le clavier à utiliser. Il faut donner cette valeur avant la première initialisation de la classe.
        public KeyboardControl Keyboard { get; set; }

        /// Les options (checkbox) d'écran à utiliser. Il faut donner ces valeurs avant la première initialisation de la classe.
        public IList<ScreenCheckOption> ScreenCheckOptions
        {
            get => _zones;
            set
            {
                _zones?.Clear();
                _zones = value;
            }
        }

        /// <summary>
        /// Si en plus de l'option de revenir en arrière, on peut avoir une zone optionnelle pour avancer.
        /// </summary>
        public Rectangle? NextPageZone { get; set; }

        /// <summary>
        /// Indique les options de changement de page sur l'action "Next".
        /// </summary>
        public ScreenChangeOptions NextPageOptions { get; set; }

        /// <summary>
        /// Si en plus de l'option de revenir en arrière, on peut avoir une zone optionnelle pour avancer.
        /// </summary>
        public Rectangle? BackPageZone { get; set; }
        
        /// <summary>
        /// Indique les options de changement de page sur l'action "Back".
        /// </summary>
        public ScreenChangeOptions BackPageOptions { get; set; }
        
        /// <summary>
        /// Zone optionnelle pour tout annuler.
        /// </summary>
        public Rectangle? CancelZone { get; set; }

        /// <inheritdoc />
        public override void Recycle()
        {
            base.Recycle();

            _mailTextBox.Text = "";

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < _zones.Count; i++)
            {
                var zone = _zones[i];
                zone.Recycle();
            }
        }

        private bool SetAnswersToQuestionnaire()
        {
            if (!AllMandatoryChecks(_zones))
            {
                var mBox = new MessageDialog();
                mBox.ShowMessage(Resources.iBrand.NotAllMandatoryCheckEnabled);
                return false;
            }

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

        /// TODO: Déplacer vers classe abstraite.
        private void AddTextBox(Rectangle? textBoxLocation)
        {
            if (textBoxLocation == null) return;
            
            _mailTextBox = new TextBox
            {
                BorderStyle = BorderStyle.FixedSingle,
                Location = textBoxLocation.Value.Location,
                Size = textBoxLocation.Value.Size,
                TabIndex = 1,
                Font = new Font(FontFamily.GenericSansSerif, FontSize)
            };
            _mailTextBox.TextChanged += OnTextChanged;

            Controls.Add(_mailTextBox);
            _mailTextBox.BringToFront();
        }

        /// <inheritdoc />
        protected override bool OnMouseDownHandler(int x, int y)
        {
            if (!base.OnMouseDownHandler(x, y)) return false;

            if (InArea(x, y, BackPageZone))
            {
                CheckPageChangeOptions(BackPageOptions);
                return false;
            }

            if (InArea(x, y, NextPageZone))
            {
                CheckPageChangeOptions(NextPageOptions);
                return false;
            }
            if (InArea(x, y, CancelZone))
            {
                FirstScreen();
                return false;
            }
            
            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < _zones.Count; i++)
            {
                var zone = _zones[i];
                if (!InArea(x, y, zone.Position)) continue;
                zone.Toggle();
                zone.SaveToAllModels(GetCurrentQuestionnaire(), zone.AnswerState);
                Refresh();
                break; // Normalement, les zones ne "overlap" pas...
            }

            return true;
        }

        private void CheckPageChangeOptions(ScreenChangeOptions nextPageOptions)
        {
            var tmp = nextPageOptions?.Evaluate(GetCurrentRespondentWithAnswers());

            if (tmp == null)
            {
                log.Warn(@"No page option found. Check your next page options.");
                Next();
                return;
            }

            if (tmp.ValidationTrigger && !SetAnswersToQuestionnaire())
            {
                return;
            }

            Next(tmp.Step);
        }

        /// <inheritdoc />
        public override void SetActivated()
        {
            base.SetActivated();
            _mailTextBox.Focus();
        }
    }
}
