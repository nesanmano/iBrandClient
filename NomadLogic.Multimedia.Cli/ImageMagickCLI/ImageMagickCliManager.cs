using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using NLog;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Services;
using NomadLogic.Common.Services.Configuration;
using NomadLogic.Common.Util;
using static NomadLogic.Multimedia.Cli.ImageMagickCLI.ImageMagickCliUtil;

namespace NomadLogic.Multimedia.Cli.ImageMagickCLI
{
    /// <summary>
    /// Cette classe englobe une séquence de commandes vers le logicel Image Magick (https://www.imagemagick.org/script/index.php)
    /// préalablement installé sur le système. Comme FFMPEG, ce seront des appels logés via ligne de commande.
    /// Contrairement à FFMPEG par contre, il est presque impossible de tout faire en une commande. Les commandes d'une instance
    /// de transformation IM sont donc chaînées. Ces commandes sont "tokenisées" pour être adaptable facilement à des changements
    /// et pour être utilisées via un questionnaire et/ou répondant remplits.
    /// Une instance de cette classe est configurée via une série de commandes, et est "state-less", indiquant que sa méthode
    /// d'exécution principale ne conserve pas d'état intermédiaire et peut être réutilisée telle quelle avec de nouveaux
    /// paramètres.
    /// </summary>
    [PublicAPI]
    public class ImageMagickCliManager
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Initialise l'instance avec ses paramètres.
        /// Principalement, s'assure que l'exécutable de ImageMagick est présent, qu'il possède la bonne version.
        /// Et que les commandes lues dans le fichier d'exécution soient valides.
        /// </summary>
        /// <returns>VRAI si et seulement si l'instance est prête à exécuter les commandes.</returns>
        public bool Init(VersionInfo minversion = null)
        {
            var config = ServicesContainer.GetService<NomadConfiguration>();
            if (!TestExecutableLocation(config, minversion ?? new VersionInfo {Major = 7})) return false;

            // Lecture des commandes
            if (!File.Exists(CommandFile)) return false;
            var commandLines = File.ReadAllLines(CommandFile).Where(l => !l.Trim().StartsWith("#") && !string.IsNullOrWhiteSpace(l)).ToList();

            Commands = CreateCommands<ImageMagickCliCommandRespondent>(commandLines);

            if (Commands.Count == commandLines.Count) return true;

            log.Error("Could not create the same number of commands as is defined in file. Check file '{0}'\nExtracted command listing:\n\t{1}",
                CommandFile, StringUtil.FormatEnumerable(commandLines, true, 1, "\n\t"));
            Commands.Clear();
            return false;
        }

        /// <summary>
        /// Accès vers la liste des commandes générées.
        /// </summary>
        public List<ImageMagickCliCommandRespondent> Commands { get; private set; }

        /// <summary>
        /// Indique qu'il faut garder les fichiers temporaires sur disque. Principalement à des fins de tests.
        /// </summary>
        public bool DebugKeepTemporaryFiles { get; set; }

        /// <summary>
        /// Emplacement pour les fichiers temporaires. Si null, utilise celui par défaut sur la machine.
        /// </summary>
        public string WorkingDirectory { get; set; }
        
        /// <summary>
        /// Fichier qui contient la commande ImageMagick à exécuter.
        /// </summary>
        public string CommandFile { get; set; }

        /// <summary>
        /// Répertoire spécifique où sauvegarder les fichiers générés de type OUT.
        /// </summary>
        public string OutputDir { get; set; }

        /// <summary>
        /// Utilise l'identifiant du fichier de sortie comme nom, plutôt qu'un nom de fichier aléatoire. L'extension de fichier selon le type de sortie sera
        /// quand même mis à la fin.
        /// </summary>
        public bool UseOutpurIdAsName { get; set; }
        
        public override string ToString()
        {
            return ClassUtil.ToStringPublicProperties(this);
        }

        /// <summary>
        /// Commande d'exécution principale basé sur la configuration de cet objet.
        /// </summary>
        /// <param name="resp">Le répondant à analyser.</param>
        /// <param name="answers">Le dictionnaire de réponses.</param>
        /// <param name="baseFileDir">Le répertoire de base des fichiers.</param>
        /// <param name="ctsToken">Le jeton de configuration.</param>
        /// <param name="report">Si on veut un rapport détaillé lors de l'exécution.</param>
        /// <returns>Le résultat du script.</returns>
        public async Task<ImageMagickCliResult> ExecuteCommandOnRespondent(IRespondent resp, Dictionary<QuestionIdentifier, Answer> answers, 
            string baseFileDir, CancellationToken ctsToken, Progress<ImageMagickCliUiUpdate> report = null)
        {
            var baseOutputDir = new DirectoryInfo(string.IsNullOrWhiteSpace(OutputDir) ? DirectoryUtil.GetLocalApplicationWorkDir() : OutputDir);
            var refs = CreateFileRefs(Commands, baseOutputDir, WorkingDirectory, UseOutpurIdAsName);

            async Task Func(ImageMagickCliCommandRespondent com, CliTmpFileDictionaryRefs re, CancellationToken ct)
            {
                await com.Execute(refs, resp, baseFileDir, answers, ct);
            }

            return await ExecuteCommands(Commands, refs, Func, ctsToken, report, DebugKeepTemporaryFiles);
        }
    }
}