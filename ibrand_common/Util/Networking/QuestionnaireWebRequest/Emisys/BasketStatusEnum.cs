// ReSharper disable InconsistentNaming
namespace NomadLogic.Common.Util.Networking.QuestionnaireWebRequest.Emisys
{
    public enum BasketStatusEnum
    {
        /// Création du panier confirmée.
        payment_pending,
        /// Commande de paiement confirmée.
        payment_accept,
        /// Commande de cancellation confirmée.
        cancel,
        /// Indique que la requête n'a pas être créée car il manquait des informations, ou que l'on sait que ça va foirer (internet inactif).
        InvalidState,
        /// Indique que la création du panier a échoué.
        FailedBasketCreation,
        /// Requête vers les serveurs n'a pas fonctionnée.
        InvalidRequest,
        /// Indique que l'imprimante n'est pas disponible.
        InvalidPrinterState
    }
}