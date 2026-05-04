using System.Collections.Generic;

namespace NomadLogic.Common.Services.StateWatcher
{
    /// <summary>
    /// Cette interface existe pour indiquer qu'une instance peut analyser et s'ajouter au bon endroit pour un surveillant de capacité système.
    /// </summary>
    public interface IWatcherListUpdate
    {
        void UpdateWatcherList(IList<ExternalStateWatcher> watcherList);
    }
}