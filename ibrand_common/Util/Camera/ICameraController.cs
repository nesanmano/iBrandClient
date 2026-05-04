using System;
using System.Windows.Forms;
using JetBrains.Annotations;

namespace NomadLogic.Common.Util.Camera
{
    /// <summary>
    /// Classe d'interface pour un controller de camera agnostique de la marque de la caméra.
    /// Un controller de caméra est capable de gérer tous les appels vers son API à travers une série de méthodes
    /// simples, desquelles on peut demander, en général:
    /// <ul>
    /// <li>Combien de caméras détecté par l'API sont présentes.</li>
    /// <li>Si l'API est prêt à recevoir des commandes de photos.</li>
    /// <li>Demander de prendre une ou plusieures photos.</li>
    /// <li>Quel est l'état, et si possible, la configuration de chaque appareil.</li>
    /// </ul>
    /// À noter que les implémentations de cette interface seront des Singletons.
    /// La plupart des communications de retour se font font via un système asynchrone de demandeur, ou chaque invocation de méthode asynchrone implique
    /// que le demandant se donne en paramètre via une interface, on recevra un résultat via un "callback" éventuel. Ces retours par méthode sont
    /// définies dans les interfaces impliquées.
    /// </summary>
    [PublicAPI]
    public interface ICameraController<TCameraConfig> : IDisposable where TCameraConfig : ICameraConfig
    {
        /// <summary>
        /// Initialise l'instance. En cas d'erreur, retourne false.
        /// Si l'instance est déjà initialisé, ne fait rien et retourne 'true'.
        /// </summary>
        /// <returns></returns>
        bool Init();

        /// <summary>
        /// Indique combiens de caméras sont présentement disponibles.
        /// </summary>
        /// <returns></returns>
        int NbrCameraReady();

        /// <summary>
        /// Retourne l'état actuel du controlleur, indiquant quel est l'état de son traitement interne.
        /// De façon plus générale, il indique s'il est en traitement (et quel est ce traitement).
        /// </summary>
        /// <returns></returns>
        CameraControllerStateEnum CurrentState();

        /// <summary>
        /// Indique que la classe demande de recevoir des mises à jour pour des images de visionnement continue (live view).
        /// Quand au moins un demandant de live view est enregistré, le contrôleur fait ce qu'il peut pour activer le plus souvent possible le
        /// live view entre ses opérations (photo ou vidéo), et envoyer le résultat de chaque prise d'image à ses écoutants.
        /// Le nombre d'écoutant peut être supérieur à un. Dès que le nombre d'écoutant est de zéro, le contrôleur coupe le live view.
        /// Le même écoutant ne peut être enregistré plusieures fois.
        /// </summary>
        /// <param name="listener">L'écoutant des trames de visionnement continu.</param>
        /// <returns>true si l'écoutant est présent dans la liste d'observateurs à la fin de l'appel de cette méthode.</returns>
        bool RegisterForLiveView(ILiveViewListener listener);

        /// <summary>
        /// Enlève l'écoutant de la liste d'observateurs de trames de vision continue. Si la liste d'observeurs devient vide à la suite de
        /// cet appel, désactive le live view.
        /// </summary>
        /// <param name="listener">L'écoutant à enlever.</param>
        /// <returns>true si l'écoutant ne fait plus partie de la liste d'observeurs.</returns>
        bool UnRegisterForLiveView(ILiveViewListener listener);

        /// <summary>
        /// Indique que au moins un écoutant de trames s'est enregistré et que le contrôleur tente de capturer des trames avec le live view.
        /// </summary>
        /// <returns>TRUE si au moins un écoutant est enregistré.</returns>
        bool IsLiveViewEnabled();

        /// <summary>
        /// Obtient la configuration de la caméra à l'index voulu dans le tableau de caméra disponibles. Retourne null si l'index est invalide.
        /// L'instance retournée est dissociée de la caméra et toute modification ne se réflètera pas sur la caméra. Pour forcer une modification
        /// de configuration, utiliser la méthode "SetCameraConfiguration(int, TCameraConfig>"
        /// </summary>
        /// <param name="cameraIndex">L'index de la caméra à vérifier.</param>
        /// <returns>La configuration actuelle de la caméra.</returns>
        TCameraConfig GetCameraConfiguration(int cameraIndex);

        /// <summary>
        /// Assigne une configuration à une caméra. Si l'index n'existe pas, ou si la configuration est invalide, retourne null.
        /// Si non-nul, retourne vrai si et seulement si la caméra a changé de configuration.
        /// </summary>
        /// <param name="cameraIndex">L'index de la caméra à modifier.</param>
        /// <param name="config">La configuration à assigner à la caméra.</param>
        /// <returns>TRUE si et seulement si, la caméra à changé de configuration. Dans le cas d'un configuration nulle ou invalide, retourne nul.</returns>
        bool? SetCameraConfiguration(int cameraIndex, TCameraConfig config);

        /// <summary>
        /// Lance une demande pour un nombre d'images à prendre, pour un nombre de caméra précis. En gros, si tout se passe bien, on retourne
        /// en une fois un résultat qui contiendra X images sur disque ou X est égal au nombre d'images demandées fois le nombre caméra requises.
        /// L'instance retournée n'est jamais nulle, mais il faut interroger l'objet de retour pour savoir si une erreur s'est produite.
        /// </summary>
        /// <param name="request">La structure qui contient les paramètres de la demande de photo.</param>
        /// <returns>TRUE si la demande est valide et que le traitement est en cours.</returns>
        bool TakePictures(IPictureRequest request);

        /// <summary>
        /// Démarre une prise vidéo, pour une durée indéterminée. Il faut invoquer la méthode "StopVideo(IVideoRequest)" avec la même requête
        /// pour stopper le vidéo (ou si un problème survient durant la prise du vidéo).
        /// </summary>
        /// <param name="request">La demande de vidéo.</param>
        /// <returns>TRUE si on démarré la prise d'un vidéo, faux sinon. Cela veut peut-être dire que l'on ne pouvait pas démarrer le vidéo à cause
        /// d'un état incompatible ou que le contrôlleur était déjà occupé.</returns>
        bool StartVideo(IVideoRequest request);

        /// <summary>
        /// Stoppe la demande vidéo par un demandant précis. Si la durée maximale d'un vidéo à été donné, cela coupera court à la prise vidéo quand même.
        /// </summary>
        /// <param name="request">La demande de vidéo.</param>
        /// <returns>TRUE si une demande vidéo en cours à été arrêtée.</returns>
        bool StopVideo(IVideoRequest request);

        /// <summary>
        /// Enregistre un écoutant pour être notifié de tous les nouveaux fichiers enregistrés par les caméras gérées par le gestionnaire.
        /// Il est obligatoire de s'enregistrer via cette méthode pour recevoir des images d'une caméra qui n'a pas été activée par
        /// la méthode "TakePictures", par déclencheur par exemple.
        /// Il est toujours possible de recevoir des images via la méthode TakePictures. Le contrôlleur n'enverra pas deux fois la même image au même demandant.
        /// </summary>
        /// <param name="listener">Le demandant pour réception d'images.</param>
        /// <returns>TRUE si l'instance fait partie de la liste d'écoutants à la fin de l'appel.</returns>
        bool RegisterForNewPictureEvent(IPictureListener listener);

        /// <summary>
        /// Enlève un écoutant pour être notifié de tous les nouveaux fichiers enregistrés par les caméras gérées par le gestionnaire.
        /// Si on s'enlève de cette liste et que l'on a fait une demande via TakePictures, aucune image ne sera envoyé à cet écoutant.
        /// </summary>
        /// <param name="listener">Le demandant pour réception d'images.</param>
        /// <returns>TRUE si l'instance ne fait plus partie de la liste d'écoutants à la fin de l'appel.</returns>
        bool UnRegisterForNewPictureEvent(IPictureListener listener);

        /// <summary>
        /// La forme principale d'affichage courante. Utilisée uniquement pour l'affichage de messages d'erreurs destinés à l'usager via
        /// des MessageBox. Si null, toutes les fonctions de ces messages d'erreurs ne seront pas utilisés.
        /// </summary>s
        Form MainFormWindow { get; set; }
    }
}
