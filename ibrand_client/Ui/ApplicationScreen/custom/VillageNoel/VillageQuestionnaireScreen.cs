using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using JetBrains.Annotations;
using NLog;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Ui;
using NomadLogic.Common.Ui.Keyboard;
using NomadLogic.Common.Util;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.custom.VillageNoel
{
    public class VillageQuestionnaireScreen : AbstractScreen
    {
        // ReSharper disable once UnusedMember.Local
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        private readonly Font _font = new Font(FontFamily.GenericSansSerif, 20);
        private readonly TextBox _fullNameTextbox;
        private readonly TextBox _emailTextbox;
        private readonly TextBox _phoneTextbox;

        private IList<SectionSkipScreenData> _zones;

        private Bitmap _checkmark;

        /// La liste des zones Checkbox. Optionel.
        [PublicAPI, CanBeNull]
        public IList<ScreenCheckOption> CheckBoxZone { get; set; }

        private void OnLoad(object sender, EventArgs e)
        {
            _checkmark = GetNoneLanguageBitmap("checkmark.png");
        }

        public VillageQuestionnaireScreen(string bkgEn, string bkgFr, ScreenConfig sc)
            : base(bkgEn, bkgFr, sc)
        {
            //PictureBox.MouseClick += MouseDownHandler;

            _fullNameTextbox = new TextBox();
            this.InitTextBox(_fullNameTextbox, new Rectangle(434, 178, 720, 40), _font, 1);

            _emailTextbox = new TextBox();
            this.InitTextBox(_emailTextbox, new Rectangle(434, 242, 720, 40), _font, 2);

            _phoneTextbox = new TextBox();
            this.InitTextBox(_phoneTextbox, new Rectangle(434, 308, 720, 40), _font, 3);

            AddKeyboardToControls(new KeyboardControl(-1, 387, 1375, 411));
        }

        public override void Recycle()
        {
            base.Recycle();

            _emailTextbox.Text = "";
            _phoneTextbox.Text = "";
            _fullNameTextbox.Text = "";

            RecycleCheckBoxs(CheckBoxZone);
        }

        protected override bool OnMouseDownHandler(int x, int y)
        {
            if (!base.OnMouseDownHandler(x, y)) return false;

            if (!this.EvaluateZonesAndValidation(x, y, ZoneList, CheckBoxZone, SetAnswersToQuestionnaire)) return false;

            return EvaluateCheckBoxsMouseDown(x, y, CheckBoxZone);
        }

        protected override void OnPaintEvent(Graphics g)
        {
            base.OnPaintEvent(g);

            PaintCheckBoxList(g, CheckBoxZone, _checkmark);
        }

        /// <summary>
        /// Assigne ou va chercher un tableau de définition de zones.
        /// Cette valeur ne peut jamais être vide ou nulle, et dans cette situation on lui assignera une valeur 
        /// par défaut (Tout l'écran, va à la prochaine page) la prochaine fois qu'on demandera sa valeur.
        /// </summary>
        [PublicAPI]
        public IList<SectionSkipScreenData> ZoneList
        {
            set => _zones = value;
            get => _zones ?? (_zones = new List<SectionSkipScreenData>
                       {
                           new SectionSkipScreenData
                           {
                               Zone = new Rectangle(new Point(0, 0), new Size(PictureBox.Image.Width, PictureBox.Image.Height)),
                               PageChangeOptions = ScreenChangeOptions.GenerateBasicNextOptions()
                           }
                       }
                   );
        }

        private bool SetAnswersToQuestionnaire()
        {

            if (string.IsNullOrWhiteSpace(_fullNameTextbox.Text))
            {
                var mBox = new MessageDialog();
                mBox.ShowMessage(_activeLg.Equals("en", StringComparison.InvariantCultureIgnoreCase) ? 
                    "You must enter your name." : 
                    "Vous devez entrer votre nom.");
                return false;
            }

            SaveAnswerToQuestion(new Answer { QuestionNo = 1, Response = _fullNameTextbox.Text.Trim()});
            
            if (string.IsNullOrWhiteSpace(_emailTextbox.Text) && string.IsNullOrWhiteSpace(_phoneTextbox.Text))
            {
                var mBox = new MessageDialog();
                mBox.ShowMessage(_activeLg.Equals("en", StringComparison.InvariantCultureIgnoreCase) ?
                    "You must enter at least one way to communicate with you." :
                    "Vous devez nous donner au moins une façon de communiquer avec vous.");
                return false;
            }

            // Validation email
            if (!string.IsNullOrWhiteSpace(_emailTextbox.Text))
            {
                if (StringUtil.IsValidEmail(_emailTextbox.Text))
                {
                    SaveAnswerToQuestion(new Answer{ QuestionNo = 2, Response = _emailTextbox.Text.Trim()});
                }
                else
                {
                    var mBox = new MessageDialog();
                    mBox.ShowMessage(_activeLg.Equals("en", StringComparison.InvariantCultureIgnoreCase) ?
                        "Invalid email." :
                        "Courriel invalide.");
                    return false;
                }
            }

            // Validation téléphone 
            if (!string.IsNullOrWhiteSpace(_phoneTextbox.Text))
            {
                if (StringUtil.IsValidPhoneNumber(_phoneTextbox.Text))
                {
                    SaveAnswerToQuestion(new Answer { QuestionNo = 3, Response = _phoneTextbox.Text.Trim()});
                }
                else
                {
                    var mBox = new MessageDialog();
                    mBox.ShowMessage(_activeLg.Equals("en", StringComparison.InvariantCultureIgnoreCase) ?
                        "Invalid phone number." :
                        "Numéro de téléphone invalide.");
                    return false;
                }
            }

            return true;
        }

        public override void Dispose()
        {
            base.Dispose();

            _checkmark?.Dispose();
            _checkmark = null;
        }

        public override void SetActivated()
        {
            base.SetActivated();
            _fullNameTextbox.Focus();
        }

    }
}
