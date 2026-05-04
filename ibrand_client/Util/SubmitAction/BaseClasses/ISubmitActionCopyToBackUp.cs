
namespace NomadLogic.iBrandDesktop.Util.SubmitAction.BaseClasses
{
    /// <summary>
    /// Interface pour l'action de sauvegarder un ou plusieurs fichiers vers le répertoire de sauvegarde spécifié dans la configuration de l'application.
    /// </summary>
    public interface ISubmitActionCopyToBackUp : ISubmitAction
    {
        /// <summary>
        /// Indique si le succès de la sauvegarde est optionel.
        /// </summary>
        bool OptionalBackup { get; }
        
        /// <summary>
        /// Liste de questions sur lesquelles faire la sauvegarde.
        /// </summary>
        int[] QuestionsFileToBackUp { get; }

        /// <summary>
        /// Si vrai, on ne met pas les fichiers dans le même répertoire, mais dans une arborescence de répertoires,
        /// définie par la propriété "DirectoryTreeToken" suite au répertoire de sauvegarde.
        /// </summary>
        bool UseDirectoryTree { get; }

        /// <summary>
        /// Le répertoire relatif où faire la sauvegarde, sous forme de d'arborescence avec des jetons.
        /// Les jetons sont:
        /// {P_ID} => remplacé par le numéro de projet.
        /// {Q_ID} => remplacé par le numéro du questionnaire.
        /// {R_ID} => remplacé par le numéro du répondant.
        /// {DATE} => La date actuelle, sous format "yyyy-MM-dd".
        /// {Q_NO} => Le numéro de question qui fait référence au fichier.
        /// Exemples: 
        /// "superevent\{DATE}\" pour séparer les fichiers par date.
        /// "{DATE}\{Q_NO}\" pour séparer les fichiers de différentes questions, par date.
        /// </summary>
        string DirectoryTreeToken { get; }
        
        /// <summary>
        /// Indique que l'on veut nommer le fichier de backup de façon différente, avec jetons, plutôt que d'utiliser
        /// le nom de fichier original.
        /// </summary>
        string FileNameToken { get; }
    }
}