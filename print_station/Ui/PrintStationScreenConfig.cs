using NomadLogic.iBrandDesktop.Ui.ApplicationScreen;
using NomadLogic.PrintStation.Json;

namespace NomadLogic.PrintStation.Ui
{
    public class PrintStationScreenConfig : ScreenConfig
    {
        public PrintStationScreenConfig(int projetId, int questionnaireId, int terminalId, PrintStationUser user) : base(projetId, questionnaireId, terminalId)
        {
            PrintStationUser = user;
        }

        public PrintStationUser PrintStationUser { get; set; }
    }
}