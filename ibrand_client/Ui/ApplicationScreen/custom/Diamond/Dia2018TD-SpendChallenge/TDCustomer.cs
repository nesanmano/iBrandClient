using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using JetBrains.Annotations;
using NLog;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.custom.Diamond
{

    // Cette class n'est pas utilisée
    class TdCustomer : AbstractScreen
    {

        // ReSharper disable once UnusedMember.Local
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private readonly Rectangle _cancelArea;



        private IList<SectionSkipScreenData> _zones;

        private Bitmap _checkmark;

        /// La liste des zones Checkbox. Optionel.
        [PublicAPI, CanBeNull]
        public IList<ScreenCheckOption> CheckBoxZone { get; set; }

        private void OnLoad(object sender, EventArgs e)
        {
            _checkmark = GetNoneLanguageBitmap("checkmark.png");
        }

        public TdCustomer(string bkgEn, string bkgFr, ScreenConfig sc)
            : base(bkgEn, bkgFr, sc)
        {
            _cancelArea = new Rectangle(67, 704, 56, 56);
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

        protected override bool OnMouseDownHandler(int x, int y)
        {
            if (!base.OnMouseDownHandler(x, y)) return false;

            if (ZoneList != null)
            { 
                foreach (var data in ZoneList.Where(data => InArea(x, y, data.Zone)))
                {
                   var steps = data.PageChangeOptions.Evaluate(GetCurrentRespondentWithAnswers());

                    //SaveAnswerToQuestion(9, 1,-1);

                    Next(steps?.Step ?? 1);
                    return false;
                }
            }
            
            if (InArea(x, y, _cancelArea))
            {
                FirstScreen();
                return false;
            }

            return true;
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

      
        public override void Dispose()
        {
            base.Dispose();

            _checkmark?.Dispose();
            _checkmark = null;
        }
    
    }
}
