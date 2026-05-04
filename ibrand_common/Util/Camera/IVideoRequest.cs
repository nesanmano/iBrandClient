using System.IO;
using JetBrains.Annotations;

namespace NomadLogic.Common.Util.Camera
{
    [PublicAPI]
    public interface IVideoRequest : ICameraRequest
    {
        /// <summary>
        /// Le demandant de la requête d'image.
        /// </summary>
        IVideoListener VideoListener { get; }

        /// <summary>
        /// La durée maximale du vidéo, en secondes.
        /// </summary>
        double Duration { get; }

        /// <summary>
        /// Le nombre de secondes avant le départ du vidéo.
        /// </summary>
        int CountDown { get; }

        /// <summary>
        /// Le vidéo pris par la caméra.
        /// </summary>
        FileInfo VideoTaken { get; set; }
    }
}