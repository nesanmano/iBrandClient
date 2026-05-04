using System;

namespace NomadLogic.Common.PresentationScreens
{
    /// <summary>
    /// Interface de base pour une classe du système de présentation. Nommément, les méthode pour créer, valider et sauvegarder les
    /// informations sont indiqués ici, pour respecter le "life cycle" par contrat de ces éléments.
    /// </summary>
    public interface IPresentationElement : IDisposable {
        /// <summary>
        /// Méthode à exécuter pour vérifier que toutes les informations contenues sont valides. Retourne faux si l'état actuel de l'instance
        /// ne permet pas son utilisation. SI certains éléments sont instanciés dynamiquement basé sur les informations de base, c'est également ici que
        /// l'on le fait.
        /// </summary>
        /// <returns></returns>
        bool Initialize();
    }
}