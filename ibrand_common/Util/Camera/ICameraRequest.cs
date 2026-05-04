using System;
using System.IO;
using JetBrains.Annotations;

namespace NomadLogic.Common.Util.Camera
{
    [PublicAPI]
    public interface ICameraRequest
    {
        /// <summary>
        /// Indique s'il faut utiliser la LiveView durant la prise du vidéo.
        /// </summary>
        bool UseLiveView { get; }

        /// <summary>
        /// L'observateur qui écoute les mises à jour des images de pré-visualisation.
        /// </summary>
        ILiveViewListener LiveViewListener { get; }

        /// <summary>
        /// Indique si la requête a été annulée. Le vidéo sera stoppé, et on ne stockera pas le fichier résultant.
        /// </summary>
        bool Cancelled { get; set; }

        /// <summary>
        /// Indique qu'une erreur a été détectée à la fin du processus de prise de vidéo et que le résultat ne sera
        /// pas conforme au résultat attendu.
        /// </summary>
        bool ErrorDetected { get; set; }

        /// <summary>
        /// La référence pour l'écoutant des événements liés au décompte durant la prise de photo.
        /// Aide pour synchroniser une interface visuelle avec ce qui arrive durant l'exécution de la prise de photo.
        /// </summary>
        ICountDownEventListener CountDownEventListener { get; }

        /// <summary>
        /// Indique quel est le répertoire de sauvegarde voulu par la requête.
        /// Par défaut, cette valeur est: "DirectoryUtil.GetLocalMediaDir(DirectoryUtil.NomadLogicMediaDirectoriesEnum.pictures)"
        /// </summary>
        DirectoryInfo SaveDirectory { get; set; }

        /// Durée de temps maximal que la requête doit être exécutée dedans. Si la valeur temporelle est zéro ou négative, il n'y a pas
        /// d'attente maximale.
        TimeSpan Timeout { get; set; }
    }
}