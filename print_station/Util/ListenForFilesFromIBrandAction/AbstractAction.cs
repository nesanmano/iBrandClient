using NomadLogic.Common.Services;
using NomadLogic.PrintStation.Json;
using NomadLogic.PrintStation.Services;

namespace NomadLogic.PrintStation.Util.ListenForFilesFromIBrandAction
{
    public abstract class AbstractAction
    {
        protected PrintStationDatabase Database;
        protected string FileName;
        protected PrintStationUser User;

        public virtual void Init(string fileName, PrintStationUser user)
        {
            FileName = fileName;
            User = user;
            Database = ServicesContainer.GetService<PrintStationDatabase>();
        }

        public virtual void DoWork()
        {
        }
    }
}