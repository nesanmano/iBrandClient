using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;

namespace NomadLogic.Common.Util.Camera
{
    /// <summary>
    /// Classe définissant toutes les propriétés d'une prise d'images via le contrôleur de caméra.
    /// </summary>
    [PublicAPI]
    public interface IPictureRequest : ICameraRequest
    {
        /// <summary>
        /// Nombre minimal de caméra pour saisir des images en simultané.
        /// </summary>
        int MinCameraNbr { get; }

        /// <summary>
        /// Le demandant de la requête d'image.
        /// </summary>
        IPictureListener PictureListener { get; }

        /// <summary>
        /// Number of pictures to take per camera.
        /// </summary>
        int PictureToTakeNbr { get; }

        /// <summary>
        /// Le nombre de secondes entre chaque séquence de prise de photo.
        /// </summary>
        int CameraCountDown { get; }

        /// <summary>
        /// Liste des images prises, si tout s'est bien passé. S'il y a eu une erreur, et que l'ensemble de l'opération ne s'est pas
        /// finie correctement, cette valeur sera nulle.
        /// </summary>
        List<List<FileInfo>> PicturesTaken { get; set; }

        /// Assigne une étiquette pour la requête. Cette étiquette n'est utile que pour l'appellant.
        /// Est utilisé dans les circonstances où un même type de demande peut être utilisé dans un même contexte. Par exemple,
        /// si l'appellant peut utiliser une image A et une image B pour deux tâches différentes, avec le même écoutant.
        string RequestTag { get; set; }
    }
}