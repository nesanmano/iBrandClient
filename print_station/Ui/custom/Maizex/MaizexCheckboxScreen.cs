using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using JetBrains.Annotations;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers;
using NomadLogic.PrintStation.Ui.generic;

namespace NomadLogic.PrintStation.Ui.custom.Maizex
{
    class MaizexCheckboxScreen : AbstractPrintStationScreen
    {


        private Bitmap _checkmark;

        private readonly Rectangle _closeArea;
        private readonly Rectangle _nextArea;
        private readonly Rectangle _optInArea1;

        private bool _optInChecked1;

        /// La liste des zones Checkbox. Optionel.
        [PublicAPI, CanBeNull]
        public IList<ScreenCheckOption> CheckBoxZone { get; set; }


        private IList<SectionSkipScreenData> _zones;

        public MaizexCheckboxScreen(string backgroundImgEnSrc, string backgroundImgFrSrc, PrintStationScreenConfig sc) : base(backgroundImgEnSrc, backgroundImgFrSrc, sc)
        {

            _checkmark = GetNoneLanguageBitmap("checkmark.png");
            _checkmark.SetResolution(300, 300);

            _nextArea = new Rectangle(new Point(555, 187), new Size(170, 61));
            _closeArea = new Rectangle(new Point(1191, 2), new Size(88, 82));

            _optInArea1 = new Rectangle(184, 529, 25, 25);



        }




        private bool SetAnswersToQuestionnaire()
        {

            //SaveAnswerToQuestion(1, 1, GetSelectedPicture().Src);

            SaveAnswerToQuestionCheck(5, 1, _optInChecked1);


            return true;
        }



        protected override bool OnMouseDownHandler(int x, int y)
        {
            if (!base.OnMouseDownHandler(x, y)) return false;


            if (InArea(x, y, _closeArea))
            {
                Close();
                return true;
            }


            if (InArea(x, y, _nextArea))
            {
                if (SetAnswersToQuestionnaire()) Next();
                return true;
            }
            if (InArea(x, y, _optInArea1))
            {
                _optInChecked1 = !_optInChecked1;
                EvaluateCheckBoxsMouseDown(x, y, CheckBoxZone);
                Refresh();
                return true;
            }
            

            if (!this.EvaluateZonesAndValidation(x, y, ZoneList, CheckBoxZone)) return false;

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
            if (CheckBoxZone != null)
            {
                foreach (var zone in CheckBoxZone.Where(zone => zone.AnswerState))
                {
                    DrawImage(g, _checkmark, zone.CheckLocation);
                }
            }

        }

        public override void Dispose()
        {
            base.Dispose();

            _checkmark?.Dispose();
            _checkmark = null;
        }

        public override void Recycle()
        {
            base.Recycle();

            _optInChecked1 = false;
            RecycleCheckBoxs(CheckBoxZone);
        }
    }
}
