namespace NomadLogic.iBrandDesktop.Report
{
    public interface IReportItem
    {
        /// <summary>
        /// Définition de l'en-tête d'une question.
        /// </summary>
        string HeaderDefinition();

        /// <summary>
        /// Valeur de la colonne, en formattant l'information pertinente.
        /// </summary>
        /// <returns>L'objet créé.</returns>

        object GenerateData();
    }
}