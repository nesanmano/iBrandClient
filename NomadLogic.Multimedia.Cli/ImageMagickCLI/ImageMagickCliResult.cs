using System;
using System.Collections.Generic;
using System.IO;

namespace NomadLogic.Multimedia.Cli.ImageMagickCLI
{
    /// <summary>
    /// Résultat complet pour l'exécution d'une série de commandes ImageMagick.
    /// </summary>
    public class ImageMagickCliResult
    {
        public bool Success { get; internal set; }
        public TimeSpan TotalTime { get; internal set; }
        public IReadOnlyDictionary<string, FileInfo> ResultFiles { get; internal set; }
    }
}