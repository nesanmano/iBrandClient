using System;
using System.Drawing;
using System.Windows.Forms;
using NLog;
using NomadLogic.Common;
using NomadLogic.Common.Util.Networking.QuestionnaireWebRequest.Emisys;
using NomadLogic.Common.Util.Win32;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic;

#pragma warning disable 618

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.custom.Diamond
{
    public class TdSpendWallRfidScannerScreen : RfidScannerScreen
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private int _qBudget;
        private int _spendQuestion;

        /// <summary>
        /// Valeur numérique pour déterminer si on est proche du total de dépense visé.
        /// </summary>
        public double EqualMargin { get; set; } = 0.1d;

        /// <summary>
        /// Cette sous-classe introduit une méthode personalisée pour calculer la balance des achats venant de la réponse de Emisys.
        /// </summary>
        /// <param name="backgroundImgEnSrc">Image de fond Anglais.</param>
        /// <param name="backgroundImgFrSrc">Image de fond Français.</param>
        /// <param name="sc">Configuration d'écran.</param>
        /// <param name="textboxLocation">Location de la boîte de scan.</param>
        /// <param name="questionBudget">Le numéro de question du budget. Cette question doit être un combo avec trois choix.</param>
        /// <param name="questionSpend">Le numéro de question du total dépensé.</param>
        public TdSpendWallRfidScannerScreen(string backgroundImgEnSrc, string backgroundImgFrSrc, ScreenConfig sc, Rectangle textboxLocation, int questionBudget, int questionSpend) :
            base(backgroundImgEnSrc, backgroundImgFrSrc, sc, textboxLocation)
        {
            _qBudget = questionBudget;
            _spendQuestion = questionSpend;
        }

        protected override DialogResult ExecuteFacadeCustomCode(string cardUid, object res, Type returnType)
        {
            if (!(res is FinalizeCardInfo responseFinalize)) return DialogResult.Cancel;

            if (responseFinalize.Result == 1 || responseFinalize.Result == 0)
            {
                var marge = responseFinalize.OrignalBalance * EqualMargin;
                var egal = Math.Abs(responseFinalize.CurrentBalance) <= marge;
                var spent = responseFinalize.OrignalBalance - responseFinalize.CurrentBalance;
                //var spendAnswer = new Answer(QuestionTypeEnum.Text, _spendQuestion, 1, -1, StringUtil.FormatFloat(0, spent));

                // Détermine si on est sous-budget (1), dans le mile (2) ou trop dépensé (3)
                //var budgetGoalAnswer = egal ? new Answer(QuestionTypeEnum.Combo, _qBudget, 1, 2) : new Answer(QuestionTypeEnum.Combo, _qBudget, 1, responseFinalize.CurrentBalance > 0 ? 1 : 3);

                //SaveAnswerToQuestion(budgetGoalAnswer);
                //SaveAnswerToQuestion(spendAnswer);
            }
            else
            {
                log.Warn("Unknown card ID: {0}", cardUid);
                this.SafeCall(() => {
                    var message = _activeLg.Equals(Constants.FR, StringComparison.InvariantCultureIgnoreCase) ?
                        $"Carte inconnue (code={responseFinalize.Result}, UID={cardUid}). Nous ne pouvons procéder à la vérification." :
                        $"Unknown Card (code={responseFinalize.Result}, UID={cardUid}). Cannot proceed further.";
                    MessageBox.Show(this, message, @"iBrand", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                    ResetScanArea();
                });
                return DialogResult.Cancel;
            }

            return DialogResult.OK;
        }
    }
}
