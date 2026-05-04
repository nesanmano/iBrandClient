using NomadLogic.Common.Respondent;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers {

    /// <summary>
    /// Cette interface cache l'implémentation de sélection d'un fichier d'image. En général, l'appellant qui a besoin de ce genre de sélecteur
    /// à accès à un répondant contenant des réponses à un questionnaire, ainsi que l'étape (ou indice) du traitement qu'il est en train d'effectuer.
    /// </summary>
    public interface IImageSelector {
        /// <summary>
        /// Obtient le chemin absolu d'un fichier d'image sélectionné d'après l'information d'un répondant et/ou d'un indice
        /// de sélection (la Xième image présentement traitée).
        /// L'algorithme de sélection dépend de la classe d'implémentation.
        /// </summary>
        /// <param name="respondent">Le répondent, et ses réponses.</param>
        /// <param name="index">L'indice du traitement en cours. Nul par défaut.</param>
        /// <returns>Le chemin vers le fichier trouvé. Si null, aucune image ne correspond à la recherche.</returns>
        string GetImageFrame(IRespondent respondent, int? index = null);
    }
}