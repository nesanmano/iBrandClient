using System;
using System.Drawing;
using System.Windows.Forms;
using NLog;
using NomadLogic.Common.Util.Networking.QuestionnaireWebRequest.Emisys;
using NomadLogic.Common.Util.Win32;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic;

#pragma warning disable 618

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.custom.SCS
{
    // ReSharper disable once InconsistentNaming
    public class RfidScannerScreen_SCS201801 : RfidScannerScreen
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        public RfidScannerScreen_SCS201801(string backgroundImgEnSrc, string backgroundImgFrSrc, ScreenConfig sc, Rectangle? textboxLocation = null) : 
            base(backgroundImgEnSrc, backgroundImgFrSrc, sc, textboxLocation) { }

        protected override DialogResult ExecuteFacadeCustomCode(string cardUid, object res, Type returnType)
        {
            if (!(res is BasketStatusEnum basketStatus)) return DialogResult.Cancel;

            string errorMessage = null;

            switch (basketStatus)
            {
                case BasketStatusEnum.payment_pending:
                    // Erreur
                    errorMessage = $"Code d'erreur reçu lors de la tentative: {basketStatus}.\tVous pouvez ré-essayer l'impression.";
                    break;
                case BasketStatusEnum.payment_accept:
                    // Tout va bien.
                    IncrementEventInQuestionnaire(8);
                    break;
                case BasketStatusEnum.cancel:
                    // Erreur
                    errorMessage = $"Code d'erreur reçu lors de la tentative: {basketStatus}.\tL'impression à échouée. Veuillez contactez un représentant. Il va tenter de provoquer une impression manuellement.";
                    break;
                case BasketStatusEnum.InvalidState:
                    errorMessage = "Erreur lors de la configuration d'imprimante. Veuillez informer un représentant SVP.";
                    break;
                case BasketStatusEnum.FailedBasketCreation:
                    errorMessage = "Une erreur est survenue lors du contact du serveur de l'application RFID. Veuillez contacter un représentant pour vérifier l'état de la connexion Internet.";
                    break;
                case BasketStatusEnum.InvalidRequest:
                    errorMessage = "Une erreur de programmation est survenue. Veuillez contacter un représentant.";
                    break;
                case BasketStatusEnum.InvalidPrinterState:
                    errorMessage = "L'imprimante n'est pas correctement connectée ou sa configuration est inutilisable. Veuillez contacter un représentant.";
                    break;
                default:
                    throw new System.Exception("Code de retour inconnu.");
            }

            if (errorMessage != null)
            {
                FormUtil.SafeCall(this, () =>
                {
                    log.Error($"Code de retour de la tentative d'impression: {basketStatus}");
                    MessageBox.Show(this, errorMessage, @"iBrand", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                    ResetScanArea();
                });
                return DialogResult.Cancel;
            }

            return DialogResult.OK;
        }
    }
}
