using System.Collections.ObjectModel;

namespace NomadLogic.iBrandDesktop.Util.SubmitAction.BaseClasses
{
    /// <summary>
    /// Méthode pour traiter l'envoi vers un Printstation.
    /// </summary>
    public interface ISubmitActionSendFilesToPrintstation : ISubmitAction
    {
        /// <summary>
        /// Indique si l'étape d'envoi vers le printstation est optionelle.
        /// </summary>
        bool SendToPrintstationActivated { get; }

        /// <summary>
        /// Le nombre minimale de PrintStation qui doivent réussir à recevoir les images. Peut être de zéro.
        /// </summary>
        int MinPrintstationToSend { get; }
        
        /// <summary>
        /// Numéro de question qui contient l'image à afficher comme représentant de la gallerie.
        /// </summary>
        int FileToShowGallery { get; }

        /// <summary>
        /// Liste des numéros de questions qui contiennent des fichiers à envoyer.
        /// </summary>
        ReadOnlyCollection<int> FileQuestions { get; }

        /// <summary>
        /// Liste des numéros de question qu'il faut envoyer les réponses sous format JSON.
        /// </summary>
        ReadOnlyCollection<int> AnswerQuestions { get; }
    }
}