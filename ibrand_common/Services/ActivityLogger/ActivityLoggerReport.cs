namespace NomadLogic.Common.Services.ActivityLogger
{
    public class ActivityLoggerReport  : IProgressDeepCopy<ActivityLoggerReport>
    {
        public int NewLogEntriesSinceStart { get; set; }

        public ActivityLoggerReport Copy()
        {
            return new ActivityLoggerReport
            {
                NewLogEntriesSinceStart = NewLogEntriesSinceStart
            };
        }
    }
}