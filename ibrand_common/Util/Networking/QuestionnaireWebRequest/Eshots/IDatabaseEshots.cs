using System;

namespace NomadLogic.Common.Util.Networking.QuestionnaireWebRequest.Eshots
{
    /// <summary>
    /// Cette interface contient les commandes CRUD pour gérer la sauvegarde des informations de Eshots en mémoire.
    /// Cette couche de persistance peut être mémorielle ou solide (sur disque).
    /// </summary>
    public interface IDatabaseEshots : IDisposable
    {
        /// <summary>
        /// Redonne les informations de la dernière information de configuration donnée pour Eshots.
        /// Il n'y a qu'un seul paquet d'information à la fois. Une reconfiguration écrase les dernière données.
        /// </summary>
        /// <returns>La dernière confoguration. N'est jamais nul, mais peut être vide.</returns>
        EshotsDataConfiguration GetLastEshotsConfiguration();

        /// <summary>
        /// Enregistre les données sur disque. Si appellée avec un élément nul, vide ces données de la couche de persistance..
        /// </summary>
        /// <param name="data">Les données à enregistrer. Si nul, vide la cache sur disque.</param>
        /// <returns>TRUE si l'opération réussie.</returns>
        bool RegisterCurrentEshotsConfiguration(EshotsDataConfiguration data);
    }
}