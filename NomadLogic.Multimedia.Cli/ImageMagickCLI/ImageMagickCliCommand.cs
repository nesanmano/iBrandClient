using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using NomadLogic.Common.Services;
using NomadLogic.Common.Services.Configuration;
using NomadLogic.Common.Util;

namespace NomadLogic.Multimedia.Cli.ImageMagickCLI
{
    /// <summary>
    /// Représente une seule ligne de commande avec paramètre sous forme de jetons.
    /// Formatté pour ImageMagick.
    /// </summary>
    public class ImageMagickCliCommand
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// La commande sous format texte avec jetons.
        /// </summary>
        public string CommandToken { get; internal set; }

        /// <summary>
        /// Liste des type des fichiers temporaires et leur identifiants contenus dans cette commande.
        /// </summary>
        public IDictionary<FileTokenType, ISet<FileTokenInfo>> TemporaryFileData { get; internal set; }
        
        [Obsolete("Use Async Version")]
        public void Execute(CliTmpFileDictionaryRefs fileRefs, Dictionary<string, string> inFiles)
        {
            try
            {
                // Les jetons de questionnaires en premier
                var tokenCommand =  StringUtil.CreateStringFromTokenizedPattern(CommandToken, inFiles);

                // Les jetons de fichiers en deuxième.
                var fullCommand = StringUtil.CreateStringFromTokenizedPatternTemporaryFiles(tokenCommand, fileRefs.OutRefs, fileRefs.TempRefs, fileRefs.InRefs);

                var config = ServicesContainer.GetService<NomadConfiguration>();
                if (log.IsDebugEnabled) log.Debug("ImageMagick command:\n\t" + CommandToken + "\n\t\"" + config.ImageMagickLocation + "\" " + fullCommand);

                LaunchProcessUtil.LaunchProcess(config.ImageMagickLocation, fullCommand);
            }
            catch (Exception e)
            {
                log.Error(e, "Error during tokenization or following command:\n\t" + CommandToken);
            }
        }

        public async Task ExecuteAsync(CliTmpFileDictionaryRefs fileRefs, Dictionary<string, string> inFiles, CancellationToken ct)
        {
            try
            {
                // Les jetons de questionnaires en premier
                var tokenCommand =  StringUtil.CreateStringFromTokenizedPattern(CommandToken, inFiles);

                // Les jetons de fichiers en deuxième.
                var fullCommand = StringUtil.CreateStringFromTokenizedPatternTemporaryFiles(tokenCommand, fileRefs.OutRefs, fileRefs.TempRefs, fileRefs.InRefs);

                var config = ServicesContainer.GetService<NomadConfiguration>();
                if (log.IsDebugEnabled) log.Debug("ImageMagick command:\n\t" + CommandToken + "\n\t\"" + config.ImageMagickLocation + "\" " + fullCommand);
                
                await LaunchProcessUtil.LaunchProcessAsync(ct, config.ImageMagickLocation, fullCommand);
            }
            catch (Exception e)
            {
                log.Error(e, "Error during tokenization or following command:\n\t" + CommandToken);
            }
        }

        public override string ToString()
        {
            return this.ToStringPublicProperties(1);
        }

    }
}