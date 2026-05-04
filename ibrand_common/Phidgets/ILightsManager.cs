using System;
using NomadLogic.Common.Services;
using NomadLogic.Common.Services.StateWatcher;
using NomadLogic.Common.Ui.NomadScreen;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Json;

namespace NomadLogic.Common.Phidgets
{
    public interface ILightsManager : IJsonConfig, IExternalFileConfig, IInitialisable, IDisposable, IWatcherListUpdate, IReportObserver<LightsReport>
    {

    }

    public class LightsReport : IProgressDeepCopy<LightsReport>
    {
        public string StatusMessage { get; set; }

        public LightsReport Copy()
        {
            return new LightsReport
            {
                StatusMessage = StatusMessage
            };
        }
    }
}