using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using NLog;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Services.Configuration;
using NomadLogic.Common.Util;
using NomadLogic.iBrandDesktop.Util.SubmitAction.BaseClasses;
using NomadLogic.iBrandDesktop.Util.SubmitAction.Generic.Worker;
using NomadLogic.Multimedia.Cli.Ffmpeg;

namespace NomadLogic.iBrandDesktop.Util.SubmitAction.Generic
{
    /// <summary>
    /// Utiliser cette classe comme submit action lorsque le projet necessite l'usage d'un fichier ffmpeg(ligne de commande ffmopeg).
    /// </summary>
    [PublicAPI]
    public class FfmpegVideoAndUploadToVimeoSubmitAction : AbstractCreateVideoAndSendSubmitAction
    {
        public const string FFMPEG_COMMAND_FILE = "ffmpeg_command_file";
        public const string FROM_BUNDLE = "ffmpeg_from_bundle";
        public const string QUESTION_BUNDLE_NO = "ffmpeg_question_bundle_no";
        public const string FRAME_RATE = "ffmpeg_bundle_frame_rate";
        public const string FILES_PER_SECOND = "ffmpeg_bundle_file_per_second";
        public const string ITERATION = "ffmpeg_bundle_iteration";
        public const string LOOPBACK = "ffmpeg_bundle_loopback";
        private string _ffmpegFile;
        private bool _fromBundle;
        private int _questionBundleNo;
        private uint _frameRate = 15;
        private double _filesPerSecond = 3;
        private int _iteration = 5;
        private bool _loopback = true;

        // ReSharper disable once UnusedMember.Local
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        
        /// <inheritdoc />
        protected override void ValidateConfigData()
        {
            base.ValidateConfigData();

            if (NoVideoEdit) return;

            if (_configData.ContainsKey(FROM_BUNDLE))
                _fromBundle = bool.Parse(_configData[FROM_BUNDLE]);

            if (_fromBundle)
            {
                if (!_configData.ContainsKey(QUESTION_BUNDLE_NO) || !int.TryParse(_configData[QUESTION_BUNDLE_NO], out _questionBundleNo))
                {
                    throw new System.Exception("Missing critical configuration in config file: " + QUESTION_BUNDLE_NO);
                }

                if (_configData.ContainsKey(FRAME_RATE)) 
                {
                    if (!uint.TryParse(_configData[FRAME_RATE], out _frameRate))
                        log.Warn("Could not parse ffmpeg frame rate :"+ _configData[FRAME_RATE]);
                }

                if (_configData.ContainsKey(FILES_PER_SECOND))
                {
                    if (!double.TryParse(_configData[FILES_PER_SECOND], out _filesPerSecond))
                        log.Warn("Could not parse ffmpeg files per seconds :" + _configData[FILES_PER_SECOND]);
                }

                if (_configData.ContainsKey(ITERATION))
                {
                    if (!int.TryParse(_configData[ITERATION], out _iteration))
                        log.Warn("Could not parse ffmpeg iteration :" + _configData[ITERATION]);
                }

                if (_configData.ContainsKey(LOOPBACK))
                {
                    if (!bool.TryParse(_configData[LOOPBACK], out _loopback))
                        log.Warn("Could not parse ffmpeg loopback :" + _configData[LOOPBACK]);
                }

            }
            else
            {
                if (!_configData.ContainsKey(FFMPEG_COMMAND_FILE))
                {
                    throw new System.Exception("Missing critical configuration in config file: " + FFMPEG_COMMAND_FILE);
                }

                _ffmpegFile = DirectoryUtil.GetLocalIbrandTerminalDir(TerminalInfo) + _configData[FFMPEG_COMMAND_FILE];
                if (!File.Exists(_ffmpegFile))
                {
                    _ffmpegFile = null;
                    throw new System.Exception("Critical configuration file is missing in questionnaire directory: " + _ffmpegFile);
                }
            }
        }

        protected override void AssignVideoCreationMethodDelegate(CreateVideoWorker worker)
        {
            worker.VideoCreationSettings.ScriptFileName = _ffmpegFile;
            
            worker.CreationMethod = (config, file, resultFile, dir, resp, db) =>
            {
                var answerMap = db.GetAnswersMap(resp.Id);
                return VideoCreation(resp, dir, answerMap, config, resultFile);
            };
        }

        /// <summary>
        /// Méthode de création du vidéo.
        /// </summary>
        /// <param name="resp">Le répondant.</param>
        /// <param name="baseDir">Le répertoire de base où chercher les fichiers vidéos.</param>
        /// <param name="answerMap">Le dictionnaire des réponses du répondant.</param>
        /// <param name="config">La configuration actuelle de l'application iBrand.</param>
        /// <param name="resultFile">Fichier résultat</param>
        /// <returns>Le fichier final créé sur disque.</returns>
        public bool VideoCreation(IRespondent resp, string baseDir, Dictionary<QuestionIdentifier, Answer> answerMap, NomadConfiguration config, FileInfo resultFile)
        {
            if (!_fromBundle) {
                return SubmitActionCreateVideoUtil.StandardFfmpegScriptCommand(config, new FileInfo(_ffmpegFile), resultFile, baseDir, resp, answerMap);
            }

            var bundleQi = new QuestionIdentifier(_questionBundleNo);
            // Création d'un vidéo à partir d'un répertoire.
            var responseBundle = answerMap.ContainsKey(bundleQi) ? answerMap[bundleQi] : null;
            if (responseBundle == null) throw new System.Exception("Invalid bundle answer for respondent");

            var workFileDir = DirectoryUtil.ExtractZipToLocalTemporaryDirectory(responseBundle.Response);
            var pics = workFileDir.GetFiles().ToList();
            FileUtil.SortFileCollection(pics);

            var tmpPath = Path.GetTempPath() + Path.DirectorySeparatorChar + GenerateUniqueId.GetId() + Path.DirectorySeparatorChar;
            var tmpDir = Directory.CreateDirectory(tmpPath);

            // TODO À enlever éventuellement quand la librarie FFMPEG sera capable de gérer des espaces dans sa tokenisation de fichiers externes.
            if (tmpDir.FullName.Contains(" "))
            {
                throw new System.Exception("FFMPEG curently cannot deal with spaces in files to use in a sequence of frame files. Change the temp directory of Windows to remove all spaces.");
            }
                
            var nbrDigit = FfmpegUtil.PrepareDirectoryForImageListVideoCreation(pics, tmpDir, _iteration, _loopback);

            if (nbrDigit < 0) throw new System.Exception("Could not prepare video frame files.");

            var mask = @"%0" + nbrDigit + @"d.jpg";
            FfmpegUtil.CreateVideoFromPictureFiles(config.FfmpegLocation, tmpDir, mask, resultFile.FullName, _filesPerSecond, _frameRate, 0);

            resultFile.Refresh();
            if (!resultFile.Exists || resultFile.Length < 100)
            {
                throw new System.Exception("Ffmpeg command did not create video from bundle.");
            }

            return true;
        }

    }
}
