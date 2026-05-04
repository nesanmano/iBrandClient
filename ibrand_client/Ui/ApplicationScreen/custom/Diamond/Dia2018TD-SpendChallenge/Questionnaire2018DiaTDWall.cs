using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using JetBrains.Annotations;
using NomadLogic.Common.Ui;
using NomadLogic.Common.Ui.Keyboard;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Win32;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.custom.Diamond
{
    class Questionnaire2018DiaTdWall : AbstractScreen
    {
        //private static readonly Logger log = LogManager.GetCurrentClassLogger();

        private TextBox _firstNameTextbox;
        private TextBox _lastNameTextbox;
        private TextBox _phoneTextbox;

        private IList<SectionSkipScreenData> _zones;

        private Bitmap _checkmark;

        /// La liste des zones Checkbox. Optionel.
        [PublicAPI, CanBeNull]
        public IList<ScreenCheckOption> CheckBoxZone { get; set; }

        private void OnLoad(object sender, EventArgs e)
        {
            _checkmark = GetNoneLanguageBitmap("checkmark4.png");
        }

        public Questionnaire2018DiaTdWall(string bkgEn, string bkgFr, ScreenConfig sc)
            : base(bkgEn, bkgFr, sc)
        {
            AddTextBoxs();
            AddKeyboard();
            Load += OnLoad;
        }

        public override void Recycle()
        {
            base.Recycle();

            this.SafeCall(() => { 
                _firstNameTextbox.Text = "";
                _lastNameTextbox.Text = "";
                _phoneTextbox.Text = "";
                RecycleCheckBoxs(CheckBoxZone);
            });
        }

        private void AddKeyboard()
        {
            var kb = new KeyboardControl(268, 513, 850, 261)
            {
                KeyPadding = -5
            };
            AddKeyboardToControls(kb);
        }

        private void AddTextBoxs()
        {
            _firstNameTextbox = new TextBox();
            this.InitTextBox(_firstNameTextbox, new Rectangle(367, 332, 251, 30), new Font(FontFamily.GenericSansSerif, 14));

            _lastNameTextbox = new TextBox();
            this.InitTextBox(_lastNameTextbox, new Rectangle(741, 332, 251, 30), new Font(FontFamily.GenericSansSerif, 14));

            _phoneTextbox = new TextBox();
            this.InitTextBox(_phoneTextbox, new Rectangle(470, 371, 251, 30), new Font(FontFamily.GenericSansSerif, 14));
        }

        protected override bool OnMouseDownHandler(int x, int y)
        {
            if (!base.OnMouseDownHandler(x, y)) return false;

            if (!EvaluateCheckBoxsMouseDown(x, y, CheckBoxZone)) return false;
            return this.EvaluateZonesAndValidation(x, y, ZoneList, CheckBoxZone, SetAnswersToQuestionnaire);
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
            MessageDialog mBox;

            // Validation nom
            if (!string.IsNullOrWhiteSpace(_firstNameTextbox.Text))
            {
                //SaveAnswerToQuestion(FirstNameQno, 1, _firstNameTextbox.Text.Trim());
            }
            else
            {
                mBox = new MessageDialog();
                mBox.ShowMessage(_activeLg.Equals("fr", StringComparison.InvariantCultureIgnoreCase) ? 
                    "Prénom invalide." :
                    "Invalid First name.");
                return false;
            }

            if (!string.IsNullOrWhiteSpace(_lastNameTextbox.Text))
            {
                //SaveAnswerToQuestion(LastNameQno, 1, _lastNameTextbox.Text.Trim());
            }
            else
            {
                mBox = new MessageDialog();
                mBox.ShowMessage(_activeLg.Equals("fr", StringComparison.InvariantCultureIgnoreCase) ?
                    "Nom de famille invalide." :
                    "Invalid Last name.");
                return false;
            }

            // Validation téléphone 
            if (!string.IsNullOrWhiteSpace(_phoneTextbox.Text) && StringUtil.IsValidPhoneNumber(_phoneTextbox.Text))
            {
                //SaveAnswerToQuestion(PhoneQno, 1, _phoneTextbox.Text.Trim());
            }
            else
            {
                mBox = new MessageDialog();
                mBox.ShowMessage(_activeLg.Equals("fr", StringComparison.InvariantCultureIgnoreCase) ?
                    "Numéro de téléphone invalide." :
                    "Invalid phone number.");
                return false;
            }

            return true;
        }

        public int LastNameQno { get; set; } = 3;

        public int PhoneQno { get; set; } = 6;

        public int FirstNameQno { get; set; } = 2;

        public override void Dispose()
        {
            base.Dispose();

            _checkmark?.Dispose();
            _checkmark = null;
        }

        public override void SetActivated()
        {
            base.SetActivated();
            _firstNameTextbox.Focus();
        }
    }
}
