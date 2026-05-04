using System;
using JetBrains.Annotations;
using NomadLogic.Common.Questionnaire;
using NomadLogic.Common.Util;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers
{
    /// <summary>
    /// Représente une instance capable de surveiller une source de données pour une Gallerie. Le résultat de
    /// ces sources est toujours une liste de fichiers.
    /// </summary>
    /// <typeparam name="T">Le type de présentation.</typeparam>
    [PublicAPI]
    public interface IGallerySource<T> : IDisposable
    {
        /// <summary>
        /// Initialize l'instance. S'assure quelle possède toutes les informations nécessaire pour
        /// son exécution correcte.
        /// </summary>
        /// <returns>True si l'initialisation est complète que l'on peut lancer les mises à jour.</returns>
        bool Init();

        /// <summary>
        /// Débute la lecture et la mise à jour des sources de fichiers.
        /// </summary>
        /// <returns>True si l'opération est lancée.</returns>
        bool StartUpdates();

        /// <summary>
        /// Stoppe la mise à jour de la source.
        /// </summary>
        /// <returns>True si l'opération de msie à jour est stoppée.</returns>
        bool StopUpdates();

        /// <summary>
        /// Ajoute un observateur sur les mises à jours d'éléments.
        /// Si déjà présent, ne sera pas ajouté à nouveau.
        /// </summary>
        /// <param name="monitor">L'instance à ajouter.</param>
        /// <returns>TRUE si le moniteur a été ajouté à la liste.</returns>
        bool AddElementMonitor(IElementMonitor<ComparableFileInfo> monitor);

        /// <summary>
        /// Retire un moniteur de la liste de mises à jour.
        /// </summary>
        /// <param name="monitor">Le moniteur à retirer.</param>
        /// <returns>True si le moniteur était dans la liste, et a été retiré avec succès.</returns>
        bool RemoveElementMonitor(IElementMonitor<ComparableFileInfo> monitor);

        /// <summary>
        /// Enlève tous les moniteurs de la liste de mises à jour.
        /// </summary>
        void ClearAllElementMonitors();
        
        /// <summary>
        /// Classe décoratrice qui se charge de sauvegarder un élément dans un questionnaire.
        /// </summary>
        ISaveToQuestionnaireFormatter<T> SaveFormatter { get; set; }
    }
}