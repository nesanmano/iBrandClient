using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using NomadLogic.Common.Ui;
using NomadLogic.Common.Ui.Keyboard;
using NomadLogic.Common.Util;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.custom.Diamond
{
    /// <summary>
    /// Écran spécifique pour le projet DIA-2017-01 car il est impossible d'ajouter la fonctionnalité voulue sans briser le comportement habituel
    /// de l'écran de courriel habituel. À savoir, revenir en arrière quand courriel vide, mais aller an avant si courriel valide.
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public class MailScreen_Dia201701 : AbstractScreen
    {
        private readonly Rectangle _previousArea;

        private TextBox _mailTextBox;
        private readonly int _nbrPageToGoBack;
        private readonly Bitmap _checkmark;
        private readonly List<ScreenCheckOption> _zones = new List<ScreenCheckOption>();

        /// <summary>
        /// Constrcuteur principal.
        /// </summary>
        public MailScreen_Dia201701(string backgroundImageSrcEn, ScreenConfig sc, int nbrPageToGoBack, Rectangle textBoxLocation, Rectangle previousLocation,
            KeyboardControl keyboard, IEnumerable<ScreenCheckOption> optInOptions = null, string backgroundImageSrcFr = null)
            : base(backgroundImageSrcEn, backgroundImageSrcFr ?? backgroundImageSrcEn, sc)
        {
            if (optInOptions != null)
            {
                _zones.AddRange(optInOptions);
                if (_zones.Any())
                {
                    // ReSharper disable once VirtualMemberCallInConstructor
                    _checkmark = GetNoneLanguageBitmap("checkmark.png");
                }
            }

            AddTextBox(textBoxLocation);
            AddKeyboard(keyboard);

            //PictureBox.MouseClick += MouseDownHandler;

            _previousArea = previousLocation;
            _nbrPageToGoBack = nbrPageToGoBack;
        }

        /// <inheritdoc/>
//        protected override void PaintEvent(object sender, PaintEventArgs e)
//        {
//            base.PaintEvent(sender, e);
//
//            foreach (var zone in _zones.Where(zone => zone.AnswerState))
//            {
//                e.Graphics.DrawImageUnscaled(_checkmark, zone.CheckLocation);
//            }
//        }
        
        /// <summary>
        /// Si en plus de l'option de revenir en arrière, on peut avoir une zone optionnelle pour avancer.
        /// </summary>
        public Rectangle? NextPageZone { get; set; }

        /// <summary>
        /// Si en plus de l'option de revenir en arrière, on peut avoir une zone optionnelle pour avancer.
        /// </summary>
        public Rectangle? CancelZone { get; set; }
        
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

            if (_mailTextBox.Text.Length <= 0) return true;

            if (StringUtil.IsValidEmail(_mailTextBox.Text))
            {
                //if (_screenConfig.QuestionEmailNo != null)
                //    SaveAnswerToQuestion(_screenConfig.QuestionEmailNo.Value, 1, _mailTextBox.Text);
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
        /*protected override void MouseDownHandler(object sender, MouseEventArgs e)
        {
            base.MouseDownHandler(sender, e);
            if (_flagForClosing) return;

            if (InArea(e.X, e.Y, _previousArea))
            {
                if (!SetAnswersToQuestionnaire()) return;

                if (_mailTextBox.TextLength == 0)
                {
                    Back(_nbrPageToGoBack);
                }
                else
                {
                    Next(_nbrPageToGoBack);
                }
            }
            else if (InArea(e.X, e.Y, NextPageZone))
            {
                if (!SetAnswersToQuestionnaire()) return;
                Next(1);
            }
            else if (InArea(e.X, e.Y, CancelZone)) { FirstScreen(); }
            else
            {
                // ReSharper disable once ForCanBeConvertedToForeach
                for (var i = 0; i < _zones.Count; i++)
                {
                    var zone = _zones[i];
                    if (!InArea(e.X, e.Y, zone.ClickZone)) continue;
                    zone.AnswerState = !zone.AnswerState;
                    SaveAnswerToQuestionCheck(zone.QuestionNo, zone.ChoiceNo, zone.AnswerState);
                    Refresh();
                    break; // Normalement, les zones ne "overlap" pas...
                }
            }
        }
*/

        /// <inheritdoc />
        public override void SetActivated()
        {
            base.SetActivated();
            _mailTextBox.Focus();
        }
    }
}
