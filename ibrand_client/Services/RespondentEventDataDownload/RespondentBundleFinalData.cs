using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using NomadLogic.Common.Serveur.Structures;
using NomadLogic.Common.Util;

namespace NomadLogic.iBrandDesktop.Services.RespondentEventDataDownload
{
    /// <summary>
    /// Cette classe regroupe les données et informations reliées à une requête de téléchargement.
    /// On a besoin de cette classe pour quelques raisons principale:
    /// - La classe qui lance le téléchargement n'est pas nécéssairement celle qui veut recevoir les données. Exemple: le premier écran part le téléchargement, l'écran 3 reçoit.
    /// - Le téléchargement peut être si rapide qu'il a terminé avant que l'on demande les données, auquel cas un simple "event" ne fait pas l'affaire.
    /// - Cette classe encapsule tout ce qui est nécessaire: les données, les événements, l'état actuel. Ce qui libère pas mal de points d'accès.
    /// - Libère le service de certains événements lié aux répondants, et de se charger uniquement des événements reliés au service au niveau général.
    /// </summary>
    [PublicAPI]
    public class RespondentBundleFinalData : IFileSelector
    {
        private bool _unusable;
        internal ConcurrentDictionary<RequestRespondentDataDynamicValue, RespondentBundleDataGroup> _fullData;
        private bool _dataUnlocked;
        private bool _empty;

        /// <summary>
        /// Constructeur seulement accessible du service.
        /// </summary>
        internal RespondentBundleFinalData()
        {
            // Nothing
        }

        /// <summary>
        /// Est lancé quand toutes les données sont lancées sur disque.
        /// </summary>
        public event EventHandler AllDataAvailiable;

        /// <summary>
        /// Quand cet événement est lancé, indique que le contenu ne doit plus être utilisé car une nouvelle requête à été lancée.
        /// </summary>
        public event EventHandler DataIsNoLongerRelevant;

        public bool ErrorDetectedDuringProcess { get; internal set; }

        /// <summary>
        /// Indique que les données sont suffisemment mûres pour être utilisée. Indique aussi que les données sont toujours d'actualité et représente bel et bien le dernier téléchargement en lice.
        /// </summary>
        public bool DataAvailiable => _dataUnlocked && !_unusable;

        /// <summary>
        /// Indique que les données sont désormais inutilisable car une nouvelle requête de téléchargement existe.
        /// </summary>
        public bool DataUnusable
        {
            get => _unusable;
            internal set => _unusable = value;
        }

        /// <summary>
        /// Si mis à vrai, indique qu'il n'y avait simplement aucune donnée côté serveur à aller chercher pour l'identifiant au moment de l'appel.
        /// Si on fait la requête, le contenu retourné par "GetFinalDataReference()" sera non-nul, mais vide.
        /// </summary>
        public bool IsEmpty => _empty;

        /// <summary>
        /// Permet à un acteur externe à obtenir la structure de données. Si ces données ne sont pas encore prêtes, cette méthode retourne null.
        /// Si la requête est désormais trop vieille, on ne retourne plus les données non plus.
        /// </summary>
        public IReadOnlyDictionary<RequestRespondentDataDynamicValue, RespondentBundleDataGroup> GetFinalDataReference()
        {
            if (_unusable) return null;
            if (!_dataUnlocked) return null;

            return _fullData;
        }

        /// <summary>
        /// Quand on appelle cette méthode, c'est pour signifier que les données de cette requête ne sont plus utilisable. En gros, qu'une nouvelle requêtre à été faite et qu'il faut
        /// se débarrasser de celle-ci. Ou alors qu'une erreur à été détectée et qu'il ne faut pas se fier sur son contenu.
        /// </summary>
        internal void FlagAsUnusable(bool errorDetected = false)
        {
            _unusable = true; // Point de non retour.
            ErrorDetectedDuringProcess = errorDetected;
            DataIsNoLongerRelevant?.InvokeFireAndForget();
        }

        // Débloque les appellants externes à avoir accès aux données.
        internal void DataIsReady()
        {
            _dataUnlocked = true;
            AllDataAvailiable?.InvokeFireAndForget();
        }

        /// <summary>
        /// Si on appelle ceci, cela indique qu'il n'y a aucune donnée à aller chercher. Ceci est un état valide.
        /// Les causes sont multiples, mais généralement, cela représente que aucune donnée n'existe (encore) pour l'identifiant demandé.
        /// Paradoxalement, cela veut dire que les données "sont disponible", pour respecter les lancement d'événements.
        /// </summary>
        internal void FlagAsEmpty()
        {
            _empty = true;
            DataIsReady();
        }

        public bool IsAllFilesReady()
        {
            if (_unusable) return false;
            if (!_dataUnlocked) return false;

            foreach (var vals in _fullData.Values)
            {
                foreach (var rbdf in vals.Items.Values)
                {
                    if (!rbdf.IsDownloadableFile()) continue;
                    if (!rbdf.Downloaded) return false;

                    if (!rbdf.LocalFile.Exists) return false;
                }
            }

            return true;
        }

        public List<FileInfo> ObtainFiles()
        {
            if (_unusable) return null;
            if (!_dataUnlocked) return null;

            var resultList = new List<FileInfo>();

            foreach (var vals in _fullData.Values)
            {
                resultList.AddRange(from rbdf in vals.Items.Values where rbdf.IsDownloadableFile() where rbdf.Downloaded select rbdf.LocalFile);
            }

            return resultList;
        }
    }
}