using NomadLogic.Common.Respondent;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen;

namespace NomadLogic.PrintStation.Ui
{
    /// <summary>
    /// Interface pour les méthode supplémentaires nécessaires pour le PrintStation en extension sur un IAbstractScreen
    /// </summary>
    public interface IAbstractPrintStationScreen : IAbstractScreen
    {
        /// <summary>
        /// Va cherche l'image sélectionnée par l'usager.
        /// </summary>
        /// <returns>Les informations sur l'image.</returns>
        PicturePostInfo GetSelectedPicture();

        /// <summary>
        /// Source du répondant actuel.
        /// </summary>
        ICurrentRespondentSource CurrentRespondentSource { get; set; }
    }
}