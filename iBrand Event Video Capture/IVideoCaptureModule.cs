using System;
using NomadLogic.Common.Services;
using NomadLogic.Common.Services.StateWatcher;
using NomadLogic.Common.Ui.NomadScreen;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Json;
using NomadLogic.Multimedia.Video;

namespace NomadLogic.App.EventVideoCapture
{
    /// <summary>
    /// Regorupement d'interface pour homogéniser la hiérarchie des fonctions de base de chaque module requis par cette application.
    /// </summary>
    public interface IVideoCaptureModule : IDisposable, IExternalFileConfig, IJsonConfig, IInitialisable, IWatcherListUpdate, IReportObserver<VideoCaptureReport>
    {
        
    }
}