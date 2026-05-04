using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Services;
using NomadLogic.Common.Services.Configuration;
using NomadLogic.Common.Util;

namespace NomadLogic.Multimedia.Cli.ImageMagickCLI
{
    /// <summary>
    /// Extension d'un jeton de ligne de commande de ImageMagick, pour utiliser des jetons avec un répondant et dictionnaire de réponses.
    /// </summary>
    public class ImageMagickCliCommandRespondent : ImageMagickCliCommand
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        public Task Execute(CliTmpFileDictionaryRefs fileRefs, IRespondent resp, string baseDirectory, Dictionary<QuestionIdentifier, Answer> answers, CancellationToken cancellationToken)
        {
            try
            {
                // Les jetons de questionnaires en premier
                var tokenCommand =  StringUtil.CreateStringFromTokenizedPatternRespondent(CommandToken, answers, null, resp, baseDirectory);
                // Les jetons de fichiers en deuxième.
                var fullCommand = StringUtil.CreateStringFromTokenizedPatternTemporaryFiles(tokenCommand, fileRefs.OutRefs, fileRefs.TempRefs, fileRefs.InRefs);

                var config = ServicesContainer.GetService<NomadConfiguration>();
                if (log.IsDebugEnabled) log.Debug("ImageMagick command:\n\t" + CommandToken + "\n\t\"" + config.ImageMagickLocation + "\" " + fullCommand);

                var t = new Task(() => LaunchProcessUtil.LaunchProcess(config.ImageMagickLocation, fullCommand), cancellationToken);
                t.Start();
                return t;
            }
            catch (System.Exception e)
            {
                log.Error("Error during tokenization or following command:\n\t" + CommandToken);
                return Task.FromException(e);
            }
        }
    }
}