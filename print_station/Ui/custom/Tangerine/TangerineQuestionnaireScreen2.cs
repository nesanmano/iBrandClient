using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using JetBrains.Annotations;
using NLog;
using NomadLogic.Common.Ui;
using NomadLogic.Common.Ui.Keyboard;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers;
using NomadLogic.PrintStation.Ui.generic;

namespace NomadLogic.PrintStation.Ui.custom.Tangerine
{
        class TangerineQuestionnaireScreen2 : AbstractPrintStationScreen
    {
        // ReSharper disable once UnusedMember.Local
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private readonly Rectangle _nextArea;
        private readonly Rectangle _cancelArea;


        private TextBox _nomTextBox;
        private TextBox _prenomTextBox;
        private TextBox _postalCodeTextbox;
        private TextBox _emailTextbox;
        private TextBox _phoneTextbox;


        private IList<SectionSkipScreenData> _zones;

        private Bitmap _checkmark;

        /// La liste des zones Checkbox. Optionel.
        [PublicAPI, CanBeNull]
        public IList<ScreenCheckOption> CheckBoxZone { get; set; }

        private void OnLoad(object sender, EventArgs e)
        {
            _checkmark = GetNoneLanguageBitmap("checkmark.png");
        }
        
        public TangerineQuestionnaireScreen2(string bkgEn, string bkgFr, PrintStationScreenConfig sc)
            : base(bkgEn, bkgFr, sc)
        {

            // AddTextBoxs();
            //AddKeyboard();

            _nextArea = new Rectangle(1184,352,86,86);
            _cancelArea = new Rectangle(1186,0,93,75);

            Load += OnLoad;
        }

        public override void Recycle()
        {
            base.Recycle();

            if (CheckBoxZone == null) return;
            foreach (var zone in CheckBoxZone)
            {
                zone.Recycle();
            }
        }

        private void AddKeyboard()
        {
            var kb = new KeyboardControl(29,300,1221,240)
            {
                KeyPadding = 1
                
            };
            AddKeyboardToControls(kb);
        }

        private void AddTextBoxs()
        {


            _nomTextBox = new TextBox();
            this.InitTextBox(_nomTextBox, new Rectangle(367, 21,545,34),new Font(FontFamily.GenericSansSerif,14));

            _prenomTextBox = new TextBox();
            this.InitTextBox(_prenomTextBox, new Rectangle(367, 64, 545, 34), new Font(FontFamily.GenericSansSerif, 14));

            _postalCodeTextbox = new TextBox();
            this.InitTextBox(_postalCodeTextbox, new Rectangle(367, 188, 545, 34), new Font(FontFamily.GenericSansSerif, 14));

            _emailTextbox = new TextBox();
            this.InitTextBox(_emailTextbox, new Rectangle(367, 148, 545, 34), new Font(FontFamily.GenericSansSerif, 14));
            
            _phoneTextbox = new TextBox();
            this.InitTextBox(_phoneTextbox, new Rectangle(367, 104, 545, 34), new Font(FontFamily.GenericSansSerif, 14));
        }

        
        protected override bool OnMouseDownHandler(int x, int y)
        {
            if (!base.OnMouseDownHandler(x, y)) return false;

            if (InArea(x,y, _cancelArea))
            {
                Close();
                return false;
            }

            if (InArea(x, y, _nextArea))
            {
                if (SetAnswersToQuestionnaire())
                {
                    Next();
                    return true;
                }
                return false;
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

            if (ZoneList == null) return true;
            
            foreach (var data in ZoneList.Where(data => InArea(x, y, data.Zone)))
            {
                if (!data.SkipValidation && !AllMandatoryChecks(CheckBoxZone))
                {
                    var mBox = new MessageDialog();
                    mBox.ShowMessage("Mandatory check boxes are not ticked");                         
                    return true;
                }

                if (data.PageSkip == 2)
                {
                    if(SetAnswersToQuestionnaire())
                        Next(data.PageSkip);

                }
                else
                {
                    Next(data.PageSkip);
                }
                
                return true;
            }
            

            return true;

        }

        private bool SetAnswersToQuestionnaire()
        {
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

        

        protected override void OnPaintEvent(Graphics g)
        {
            base.OnPaintEvent(g);

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
            
        }
    
    }
}
