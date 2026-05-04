using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using JetBrains.Annotations;
using NLog;
using NomadLogic.Common.Ui.Keyboard;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers;
using NomadLogic.PrintStation.Ui.generic;

namespace NomadLogic.PrintStation.Ui.custom.Carte_Hockey
{
    class OptInScreen : AbstractPrintStationScreen
    {

       // ReSharper disable once UnusedMember.Local
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private readonly Rectangle _nextArea;
        private readonly Rectangle _cancelArea;



        private IList<SectionSkipScreenData> _zones;

        private Bitmap _checkmark;

        /// La liste des zones Checkbox. Optionel.
        [PublicAPI, CanBeNull]
        public IList<ScreenCheckOption> CheckBoxZone { get; set; }

        private void OnLoad(object sender, EventArgs e)
        {
            _checkmark = GetNoneLanguageBitmap("check mark.png");
        }
        
        public OptInScreen(string bkgEn, string bkgFr, PrintStationScreenConfig sc)
            : base(bkgEn, bkgFr, sc)
        {
          
            _nextArea = new Rectangle(265, 236, 765, 98);
            _cancelArea = new Rectangle(1169,1,111,92);

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
            var kb = new KeyboardControl(29,335,1221,267)
            {
                KeyPadding = -5
                
            };
            AddKeyboardToControls(kb);
        }

        

        
        protected override bool OnMouseDownHandler(int x, int y)
        {
            //base.MouseDownHandler(sender, e);

            if (InArea(x, y, _nextArea))
            {

                int tmp = 0;

                Next(1);
                return true;
            }

            if (InArea(x, y, _cancelArea))
            {
                //FirstScreen();
                log.Trace("Cancel Area clicked");
                Close();
                return false;
            }

            if(CheckBoxZone != null)
            {
                foreach (var data in CheckBoxZone.Where(data => InArea(x, y, data.Position)))
                {

                    if (!InArea(x, y, data.Position)) continue;
                    if (!base.OnMouseDownHandler(x, y)) return false;

                    if (!EvaluateCheckBoxsMouseDown(x, y, CheckBoxZone)) return false;
                    return this.EvaluateZonesAndValidation(x, y, ZoneList, CheckBoxZone);

                 
                    log.Trace("Next Area clicked");

                }
            }




            

            return false;

        }

        protected override void OnPaintEvent(Graphics g)
        {
            base.OnPaintEvent(g);

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
