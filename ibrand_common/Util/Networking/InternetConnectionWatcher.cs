using System.Threading.Tasks;
using NomadLogic.Common.Services.StateWatcher;

namespace NomadLogic.Common.Util.Networking
{
    /// <summary>
    /// Simple surveillant de la connection filaire sur le système.
    /// </summary>
    public class InternetConnectionWatcher : ExternalStateWatcher
    {
        public override string GetDescription()
        {
            return @"Internet (RJ-45 cable detected)";
        }

        protected override ServiceStateWatcherControl CreateWinFormStatusControl(int availiableWidth)
        {
            return new ServiceStateWatcherControl(this, availiableWidth);
        }

        public override void DisposeDo()
        {
            // No-Op
        }

        protected override bool InitDo()
        {
            LaunchCheck().GetAwaiter().GetResult();
            return true;
        }

        protected override Task<WatcherStatusEnum> LaunchCheckDo()
        {
            return Task.FromResult(NetUtil.IsConnectedToInternet() ? WatcherStatusEnum.LastCheckSuccessfull : WatcherStatusEnum.LastCheckFailed);
        }

        public override string GetStatusDescription()
        {
            switch (Status)
            {
                case WatcherStatusEnum.LastCheckSuccessfull:
                    return "Cable found.";
                case WatcherStatusEnum.LastCheckFailed:
                    return "Cable disconnected.";
                default:
                    return base.GetStatusDescription();
            }
        }
    }
}