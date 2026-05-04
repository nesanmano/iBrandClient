
using System.IO;
using NomadLogic.Common.Util.Json;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers
{
    class ManualFlurryConfigLoader
    {
        private ManualFlurryConfig _manualFlurry;

        public void LoadConfig(string configFilePath)
        {
            _manualFlurry = JsonUtil.ReadJsonFile<ManualFlurryConfig>(new FileInfo(configFilePath));
            
        }


        public int GetMinShotsRequired()
        {
            return _manualFlurry.MinShotsRequired;
        }


        public int getMaxShotsRequired()
        {
            return _manualFlurry.MaxShotsRequired;
        }
    }
}
