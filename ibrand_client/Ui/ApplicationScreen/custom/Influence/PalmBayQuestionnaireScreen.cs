using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using JetBrains.Annotations;
using NLog;
using NomadLogic.Common.Ui;
using NomadLogic.Common.Ui.Keyboard;
using NomadLogic.Common.Util;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.custom.Influence
{
    public class PalmBayQuestionnaireScreen : AbstractScreen
    {
        // ReSharper disable once UnusedMember.Local
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        private readonly Font _font = new Font(FontFamily.GenericSansSerif, 20);
        private readonly TextBox _emailTextbox;
        private readonly TextBox _phoneTextbox;
        private readonly TextBox _emailTextbox2;
        private readonly TextBox _phoneTextbox2;

        private IList<SectionSkipScreenData> _zones;

        private Bitmap _checkmark;

        /// La liste des zones Checkbox. Optionel.
        [PublicAPI, CanBeNull]
        public IList<ScreenCheckOption> CheckBoxZone { get; set; }

        protected override void OnLoad()
        {
            _checkmark = GetNoneLanguageBitmap("checkmark.png");
        }

        public PalmBayQuestionnaireScreen(string bkgEn, string bkgFr, ScreenConfig sc)
            : base(bkgEn, bkgFr, sc)
        {
            _emailTextbox = new TextBox();
            this.InitTextBox(_emailTextbox, new Rectangle(353, 155, 690, 30), _font, 1);

            _phoneTextbox = new TextBox();
            this.InitTextBox(_phoneTextbox, new Rectangle(353, 199, 690, 30), _font, 2);

            _emailTextbox2 = new TextBox();
            this.InitTextBox(_emailTextbox2, new Rectangle(353, 287, 690, 30), _font, 3);
            
            _phoneTextbox2 = new TextBox();
            this.InitTextBox(_phoneTextbox2, new Rectangle(353, 332, 690, 30), _font, 4);

            AddKeyboardToControls(new KeyboardControl(-1, 413, 1195, 273));
        }

        public override void Recycle()
        {
            base.Recycle();

            _emailTextbox.Text = "";
            _phoneTextbox.Text = "";
            _emailTextbox2.Text = "";
            _phoneTextbox2.Text = "";

            RecycleCheckBoxs(CheckBoxZone);
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
            if (string.IsNullOrWhiteSpace(_emailTextbox.Text) && 
                string.IsNullOrWhiteSpace(_phoneTextbox.Text) && 
                string.IsNullOrWhiteSpace(_emailTextbox2.Text) && 
                string.IsNullOrWhiteSpace(_phoneTextbox2.Text))
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
                    //SaveAnswerToQuestion(9, 1, _emailTextbox.Text.Trim());
                }
                else
                {
                    var mBox = new MessageDialog();
                    mBox.ShowMessage(_activeLg.Equals("en", StringComparison.InvariantCultureIgnoreCase) ?
                        "Invalid email 1." :
                        "Courriel 1 invalide.");
                    return false;
                }
            }
            // Validation email 2
            if (!string.IsNullOrWhiteSpace(_emailTextbox2.Text))
            {
                if (StringUtil.IsValidEmail(_emailTextbox2.Text))
                {
                    //SaveAnswerToQuestion(12, 1, _emailTextbox2.Text.Trim());
                }
                else
                {
                    var mBox = new MessageDialog();
                    mBox.ShowMessage(_activeLg.Equals("en", StringComparison.InvariantCultureIgnoreCase) ?
                        "Invalid email 2." :
                        "Courriel 2 invalide.");
                    return false;
                }
            }

            // Validation téléphone 
            if (!string.IsNullOrWhiteSpace(_phoneTextbox.Text))
            {
                if (StringUtil.IsValidPhoneNumber(_phoneTextbox.Text))
                {
                    //SaveAnswerToQuestion(10, 1, _phoneTextbox.Text.Trim());
                }
                else
                {
                    var mBox = new MessageDialog();
                    mBox.ShowMessage(_activeLg.Equals("en", StringComparison.InvariantCultureIgnoreCase) ?
                        "Invalid phone number 1." :
                        "Numéro de téléphone 1 invalide.");
                    return false;
                }
            }
            // Validation téléphone 2
            if (!string.IsNullOrWhiteSpace(_phoneTextbox2.Text))
            {
                if (StringUtil.IsValidPhoneNumber(_phoneTextbox2.Text))
                {
                    //SaveAnswerToQuestion(13, 1, _phoneTextbox2.Text.Trim());
                }
                else
                {
                    var mBox = new MessageDialog();
                    mBox.ShowMessage(_activeLg.Equals("en", StringComparison.InvariantCultureIgnoreCase) ?
                        "Invalid phone number 2." :
                        "Numéro de téléphone 2 invalide.");
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
            _emailTextbox.Focus();
        }

    }
}
