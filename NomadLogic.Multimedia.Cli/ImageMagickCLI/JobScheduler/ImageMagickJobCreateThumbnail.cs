using System;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NLog;
// ReSharper disable LocalizableElement

namespace NomadLogic.Multimedia.Cli.ImageMagickCLI.JobScheduler
{
    public class ImageMagickJobCreateThumbnail : ImageMagickJobDefinition
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private readonly FileInfo _origFile;
        private readonly DirectoryInfo _workDir;
        private readonly Size _wantedSize;
        private readonly bool _fitInside;
        private readonly Color _baseColor;

        public ImageMagickJobCreateThumbnail(Action<object> callback, FileInfo originalFile, DirectoryInfo workDirectory, Size wantedSize, bool fitInside = false, Color? baseColor = null) : base(callback)
        {
            _origFile = originalFile;
            if (_origFile == null || !_origFile.Exists) { throw new ArgumentException("Original file for thumbnail generation must be valid.", nameof(originalFile)); }
            _workDir = workDirectory;
            if (_workDir == null || !_workDir.Exists) { throw new ArgumentException("Working directory thumbnail generation must be valid.", nameof(workDirectory)); }
            _wantedSize = wantedSize;
            if (wantedSize.Equals(Size.Empty)) { throw new ArgumentException("Size was not specified.", nameof(wantedSize)); }
            _fitInside = fitInside;
            _baseColor = baseColor ?? Color.White;
        }

        protected override async Task<bool> ExecuteJob(CancellationToken ct)
        {
            Result = await ImageMagickCliUtil.LoadImageFromThumbnailAsync(ct,
                _origFile,
                _workDir,
                _wantedSize,
                _fitInside,
                _baseColor);

            if (Result != null) return true;
            
            ErrorMessage = $"Could not create thumbnail image from following file: {_origFile}";
            log.Error(ErrorMessage);
            return false;
        }
    }
}