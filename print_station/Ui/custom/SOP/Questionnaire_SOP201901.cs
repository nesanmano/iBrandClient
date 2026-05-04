using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using JetBrains.Annotations;
using NLog;
using NomadLogic.Common.Ui;
using NomadLogic.Common.Ui.Keyboard;
using NomadLogic.Common.Util;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers;
using NomadLogic.PrintStation.Ui.generic;

namespace NomadLogic.PrintStation.Ui.custom.SOP
{
    // ReSharper disable once InconsistentNaming
    public class Questionnaire_SOP201901 : AbstractPrintStationScreen
    {
        // ReSharper disable once UnusedMember.Local
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private readonly Rectangle _nextArea;
        private readonly Rectangle _cancelArea;

        private TextBox _emailTextbox;
        private TextBox _phoneTextbox;

        private IList<SectionSkipScreenData> _zones;

        private Bitmap _checkmark;

        /// La liste des zones Checkbox. Optionel.
        [PublicAPI, CanBeNull]
        public IList<ScreenCheckOption> CheckBoxZone { get; set; }
        
        public Questionnaire_SOP201901(string bkgEn, string bkgFr, PrintStationScreenConfig sc)
            : base(bkgEn, bkgFr, sc)
        {
            AddTextBoxs();
            AddKeyboard();

            _nextArea = new Rectangle(538, 361, 600, 64);
            _cancelArea = new Rectangle(1588, 0, 80, 80);
        }

        protected override void OnLoad()
        {
            _checkmark = GetNoneLanguageBitmap("checkmark.png");
        }

        public override void Recycle()
        {
            base.Recycle();

            _emailTextbox.Text = "";
            _phoneTextbox.Text = "";

            if (CheckBoxZone == null) return;
            foreach (var zone in CheckBoxZone)
            {
                zone.Recycle();
            }
        }

        private void AddKeyboard()
        {
            var kb = new KeyboardControl(-1, 440, 1676, 411)
            {
                KeyPadding = 1
            };
            AddKeyboardToControls(kb);
        }

        private void AddTextBoxs()
        {
            _emailTextbox = new TextBox();
            this.InitTextBox(_emailTextbox, new Rectangle(548, 157, 581, 45), new Font(FontFamily.GenericSansSerif, 20));
            _emailTextbox.BorderStyle = BorderStyle.None;
            _emailTextbox.Multiline = true;

            _phoneTextbox = new TextBox();
            this.InitTextBox(_phoneTextbox, new Rectangle(548, 251, 581, 45), new Font(FontFamily.GenericSansSerif, 20));
            _phoneTextbox.BorderStyle = BorderStyle.None;
            _phoneTextbox.Multiline = true;
        }

        protected override bool OnMouseDownHandler(int x, int y)
        {
            if (!base.OnMouseDownHandler(x, y)) return false;

            if (InArea(x,y, _cancelArea))
            {
                Close();
                return false;
            }

            if (InArea(x,y, _nextArea))
            {
                if (SetAnswersToQuestionnaire())
                {
                    Next();
                    return false;
                }
            }

            if (CheckBoxZone != null)
            {

                foreach (var zone in CheckBoxZone.Where(data => InArea(x, y, data.Position)))
                {
                    //var zone = _zonesCheckBox[i];
                    if (!InArea(x, y, zone.Position)) continue;
                    zone.Toggle();
                    zone.SaveToAllModels(GetCurrentQuestionnaire(), zone.AnswerState);
                    Refresh();
                    return true;
                }
            }
            
            return true;

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
            var pictureInfo = GetSelectedPicture();
            //SaveAnswerToQuestion(1, 1, pictureInfo.Src);

            // Validation téléphone 
            if (!string.IsNullOrWhiteSpace(_phoneTextbox.Text))
            {
                if (StringUtil.IsValidPhoneNumber(_phoneTextbox.Text))
                {
                    //SaveAnswerToQuestion(4, 1, _phoneTextbox.Text);
                }
                else
                {
                    var mBox = new MessageDialog();
                    mBox.ShowMessage(_activeLg.Equals("en", StringComparison.CurrentCultureIgnoreCase) ? 
                        "The Phone Number is invalid." : 
                        "Le numéro de téléphone est invalide.");
                    return false;
                }
            }


            // Validation email
            if (!string.IsNullOrWhiteSpace(_emailTextbox.Text))
            {
                if (StringUtil.IsValidEmail(_emailTextbox.Text))
                {
                    //SaveAnswerToQuestion(3, 1, _emailTextbox.Text);
                }
                else
                {
                    var mBox = new MessageDialog();
                    mBox.ShowMessage(_activeLg.Equals("en", StringComparison.CurrentCultureIgnoreCase) ?
                        "The email is invalid." :
                        "Le courriel est invalide.");
                    return false;
                }
            }

            return true;
        }

        protected override void OnPaintEvent(Graphics g)
        {
            base.OnPaintEvent(g);

            PaintCheckBoxList(g, CheckBoxZone, _checkmark);
           
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

            FillControlWithTextAnswer(_emailTextbox, 3);
            FillControlWithTextAnswer(_phoneTextbox, 4);

            _emailTextbox.Focus();

        }
    
    }
}
