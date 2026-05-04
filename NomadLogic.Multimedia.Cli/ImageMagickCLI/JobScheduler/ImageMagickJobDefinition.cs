using System;
using System.Threading;
using System.Threading.Tasks;
using NLog;

namespace NomadLogic.Multimedia.Cli.ImageMagickCLI.JobScheduler
{
    public abstract class ImageMagickJobDefinition
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private readonly Action<ImageMagickJobDefinition> _callBackAction;

        public object Result { get; set; }
        
        public TimeSpan MaxDelay { get; set; } = TimeSpan.FromSeconds(10);
        public string ErrorMessage { get; set; }

        protected ImageMagickJobDefinition(Action<object> callback)
        {
            _callBackAction = callback;
        }

        protected abstract Task<bool> ExecuteJob(CancellationToken ct);

        internal bool Execute()
        {
            log.Trace("Executing ImageMagick job.");
            var cts = new CancellationTokenSource(MaxDelay);
            var rez = ExecuteJob(cts.Token).GetAwaiter().GetResult();
            if (!rez)
            {
                log.Warn($"Job did not execute properly: {ErrorMessage}");
            }
            _callBackAction?.Invoke(this);

            return rez;
        }
    }
}