using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NomadLogic.Common.Services;
using NomadLogic.Common.Services.StateWatcher;
using NomadLogic.Common.Ui.NomadScreen;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Json;

namespace NomadLogic.Multimedia.Video
{
    public interface IVideoCaptureManager<T> 
        : IJsonConfig, IExternalFileConfig, IInitialisable, IDisposable, IWatcherListUpdate, IReportObserver<VideoCaptureReport> 
        where T : VideoCaptureSourceConfigurationBase
    {
        /// <summary>
        /// Configurations pour les paramètres de capture de chaque sources vidéos.
        /// </summary>
        VideoCaptureConfiguration<T> CaptureConfiguration { get; set; }

        /// <summary>
        /// Indique si le module roule.
        /// </summary>
        [JsonIgnore]
        bool IsRunning { get; }
        
        /// <summary>
        /// Callback quand le système de capture détecte un événement de coupure. Cela débloque la lecture (elle a besoin de cette confirmation pour fonctionner).
        /// </summary>
        void TriggerEvent();

        /// <summary>
        /// Méthode asynchrone qui fait tous le nécessaire pour la capture d'un vidéo.
        /// </summary>
        /// <param name="ct">Le jeton de cancellation.</param>
        /// <param name="baseDirectory">Le répertoire des fichiers résultats de l'opération. Assume que ce répertoire existe.</param>
        Task StartCapture(CancellationToken ct, DirectoryInfo baseDirectory);

        [JsonIgnore]
        TimeSpan DelayAfterTrigger { get; set; }

        /// <summary>
        /// Valide si tous les fichiers qui doivent être créés dans cette capture ont bel et bien été capturés.
        /// </summary>
        /// <param name="dirToCheck">Le répertoire à vérifier.</param>
        /// <returns>TRUE si tous les fichiers requis sont présents.</returns>
        bool Validate(DirectoryInfo dirToCheck);
    }
}