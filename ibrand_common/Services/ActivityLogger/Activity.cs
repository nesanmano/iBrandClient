namespace NomadLogic.Common.Services.ActivityLogger
{
    public class Activity
    {
        public string Details { get; set; }
        public int TerminalId { get; set; }
        public ActivityLoggerActionType ActionType { get; set; }
    }
}