using System;
using System.Drawing;
using NLog;
using NomadLogic.Common.Ui;
using NomadLogic.Common.Util.Win32;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.custom.Diamond
{
    /// <summary>
    /// Cette écran permet de choisir le mode de vie du joueur et de lui attribuer un montant abitraire correspondant à son mode de vie.
    /// </summary>
    class LifeStyleModeScreen : AbstractScreen
    {
        // ReSharper disable once UnusedMember.Local
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private readonly Rectangle _nextAreaStartingAndPlanning;
        private readonly Rectangle _nextAreaGrowingAndBuilding;
        private readonly Rectangle _nextAreaMaintainingAndManaging;
        private readonly Rectangle _cancelArea;

        private readonly Rectangle _helpStartingAndPlanning;
        private readonly Rectangle _helpGrowingAndBuilding;
        private readonly Rectangle _helpMaintainingAndManaging;

        public readonly double[] _millenium = {400.00,450.00,500.00,550.00,600.00,650.00,700.00,750.00,800.00};
        public readonly double[] _xGen = {600.00,650.00,700.00,750.00,800.00,850.00,900.00,950.00,1000.00};
        public readonly double[] _boomers = { 800.00, 850.00, 900.00, 950.00, 1000.00, 1050.00,1100.00,1150.00,1200.00};

        private bool _lockSelection;

        public LifeStyleModeScreen(string bkgEn, string bkgFr, ScreenConfig sc)
            : base(bkgEn, bkgFr, sc)
        {
            _cancelArea = new Rectangle(58, 704, 73, 66);

            _nextAreaStartingAndPlanning = new Rectangle(192, 300, 300, 300);
            _nextAreaGrowingAndBuilding = new Rectangle(493, 300, 300, 300);
            _nextAreaMaintainingAndManaging = new Rectangle(797, 300, 300, 300);

            _helpStartingAndPlanning = new Rectangle(321, 640, 45, 45);
            _helpGrowingAndBuilding = new Rectangle(620, 640, 45, 45);
            _helpMaintainingAndManaging = new Rectangle(927, 640, 45, 45);
        }

        public int StartingAndPlanning { get; set; }

        public int GrowingAndBuilding { get; set; }

        public int MaintainingAndManaging { get; set; }


        public override void Recycle()
        {
            base.Recycle();

            this.SafeCall(() => {
                _lockSelection = false;
            });
        }

        /// <inheritdoc />
        protected override bool OnMouseDownHandler(int x, int y)
        {
            if (!base.OnMouseDownHandler(x, y)) return false;

            if (InArea(x, y, _nextAreaStartingAndPlanning))
            {
                if (!_lockSelection)
                {
                    //SaveAnswerToQuestion(LifeStyleQno, 1, StartingAndPlanning);

                    GetBudget(_millenium);
                    Next(4);
                }
                else
                {
                    ShowAlert();
                }
                return false;
            }

            if (InArea(x, y, _nextAreaGrowingAndBuilding))
            {
                if (!_lockSelection)
                {
                    //SaveAnswerToQuestion(LifeStyleQno, 1, GrowingAndBuilding);

                    GetBudget(_xGen);
                    Next(4);
                }
                else
                {
                    ShowAlert();
                }

                return false;
            }

            if(InArea(x, y, _nextAreaMaintainingAndManaging))
            {
                if (!_lockSelection)
                {
                    //SaveAnswerToQuestion(LifeStyleQno, 1, MaintainingAndManaging);

                    GetBudget(_boomers);
                    Next(4);
                }
                else
                {
                    ShowAlert();
                }

                return false;
            }

            if (InArea(x, y, _helpStartingAndPlanning))
            {
                Next(1);
                return false;
            }

            if (InArea(x, y, _helpGrowingAndBuilding))
            {
                Next(2);
                return false;
            }

            if (InArea(x, y, _helpMaintainingAndManaging))
            {
                Next(3);
                return false;
            }

            if (InArea(x, y, _cancelArea))
            {
                FirstScreen();
                return false;
            }

            return true;
        }

        public int LifeStyleQno { get; set; } = 10;

        /// <summary>
        /// Sauvegarde dans la question 11 un montant aléatoire choisi dans une liste.
        /// </summary>
        /// <param name="amountsArr"></param>
        public void GetBudget(double[] amountsArr)
        {
            var rnd = new Random();
            var rand = rnd.Next(0, amountsArr.Length);
            
            //SaveAnswerToQuestion(BudgetQno, 1, amountsArr[rand].ToString(CultureInfo.InvariantCulture));
        }

        public int BudgetQno { get; set; } = 11;

        private void ShowAlert()
        {
            var lang = GetCurrentRespondentWithAnswers().Language;

            var msg = lang.Equals("fr", StringComparison.InvariantCultureIgnoreCase) ? "Vous avez déjà choisi un style de vie." : "You have already selected your life style";

            var mBox = new MessageDialog();
            mBox.ShowMessage(msg);
        }
    }
}
