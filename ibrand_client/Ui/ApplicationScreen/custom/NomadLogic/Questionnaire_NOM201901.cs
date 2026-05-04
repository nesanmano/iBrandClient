using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using JetBrains.Annotations;
using NomadLogic.Common.Ui;
using NomadLogic.Common.Ui.Keyboard;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Win32;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.custom.NomadLogic
{
    public class QuestionnairePcq201901 : AbstractScreen
    {
        //private static readonly Logger log = LogManager.GetCurrentClassLogger();

        private TextBox _firstNameTextbox;
        private TextBox _lastNameTextbox;
        private TextBox _emailTextbox;
        private TextBox _phoneTextbox;

        private IList<SectionSkipScreenData> _zones;

        private Bitmap _checkmark;

        /// La liste des zones Checkbox. Optionel.
        [PublicAPI, CanBeNull]
        public IList<ScreenCheckOption> CheckBoxZone { get; set; }

        public QuestionnairePcq201901(string bkgEn, string bkgFr, ScreenConfig sc)
            : base(bkgEn, bkgFr, sc)
        {
            AddTextBoxs();
            AddKeyboard();
        }

        protected override void OnLoad()
        {
            _checkmark = GetNoneLanguageBitmap("checkmark.png");
        }

        public override void Recycle()
        {
            base.Recycle();

            FormUtil.SafeCall(this, () => { 
                _firstNameTextbox.Text = "";
                _lastNameTextbox.Text = "";
                _emailTextbox.Text = "";
                _phoneTextbox.Text = "";
                RecycleCheckBoxs(CheckBoxZone);
            });
        }

        private void AddKeyboard()
        {
            var kb = new KeyboardControl(120, 605, 1415, 361)
            {
                KeyPadding = -5
            };
            AddKeyboardToControls(kb);
        }

        private void AddTextBoxs()
        {
            _firstNameTextbox = new TextBox();
            this.InitTextBox(_firstNameTextbox, new Rectangle(-1, -1, 0, 0), new Font(FontFamily.GenericSansSerif, 18));
            _firstNameTextbox.BorderStyle = BorderStyle.None;
            _firstNameTextbox.Multiline = true;

            _lastNameTextbox = new TextBox();
            this.InitTextBox(_lastNameTextbox, new Rectangle(-1, -1, 0, 0), new Font(FontFamily.GenericSansSerif, 18));
            _lastNameTextbox.BorderStyle = BorderStyle.None;
            _lastNameTextbox.Multiline = true;

            _emailTextbox = new TextBox();
            this.InitTextBox(_emailTextbox, new Rectangle(554, 261, 570, 39), new Font(FontFamily.GenericSansSerif, 18));
            _emailTextbox.BorderStyle = BorderStyle.None;
            _emailTextbox.Multiline = true;

            _phoneTextbox = new TextBox();
            this.InitTextBox(_phoneTextbox, new Rectangle(554, 355, 576, 39), new Font(FontFamily.GenericSansSerif, 18));
            _phoneTextbox.BorderStyle = BorderStyle.None;
            _phoneTextbox.Multiline = true;
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

            // Validation prénom
            if (!string.IsNullOrWhiteSpace(_firstNameTextbox.Text))
            {
                //SaveAnswerToQuestion(2, 1, _firstNameTextbox.Text.Trim());
            }
//            else
//            {
//                mBox = new MessageDialog();
//                mBox.ShowMessage(_activeLg.Equals("fr", StringComparison.InvariantCultureIgnoreCase) ? 
//                    "Prénom invalide." :
//                    "Invalid First name.");
//                return false;
//            }

            // Validation nom
            if (!string.IsNullOrWhiteSpace(_lastNameTextbox.Text))
            {
                //SaveAnswerToQuestion(1, 1, _lastNameTextbox.Text.Trim());
            }
//            else
//            {
//                mBox = new MessageDialog();
//                mBox.ShowMessage(_activeLg.Equals("fr", StringComparison.InvariantCultureIgnoreCase) ?
//                    "Nom de famille invalide." :
//                    "Invalid Last name.");
//                return false;
//            }

            // Validation email 
            if (!string.IsNullOrWhiteSpace(_emailTextbox.Text)) { 
                if (StringUtil.IsValidEmail(_emailTextbox.Text))
                {
                    //SaveAnswerToQuestion(3, 1, _emailTextbox.Text.Trim());
                }
                else
                {
                    mBox = new MessageDialog();
                    mBox.ShowMessage(
                        "Courriel invalide.");
                    return false;
                }
            }

            // Validation téléphone 
            if (!string.IsNullOrWhiteSpace(_phoneTextbox.Text))
            {
                if (StringUtil.IsValidPhoneNumber(_phoneTextbox.Text))
                {
                    //SaveAnswerToQuestion(4, 1, _phoneTextbox.Text.Trim());
                }
                else
                {
                    mBox = new MessageDialog();
                    mBox.ShowMessage("Numéro de téléphone invalide.");
                    return false;
                }
            }

            if (!CheckBoxZone?.Any(zone => zone.AnswerState) ?? true)
            {
                mBox = new MessageDialog();
                mBox.ShowMessage("Vous devez avoir au moins une façon de communiquer avec vous.");
                return false;
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

            FillControlWithTextAnswer(_lastNameTextbox, 1);
            FillControlWithTextAnswer(_firstNameTextbox, 2);
            FillControlWithTextAnswer(_emailTextbox, 3);
            FillControlWithTextAnswer(_phoneTextbox, 4);

            _firstNameTextbox.Focus();
        }
    }
}
