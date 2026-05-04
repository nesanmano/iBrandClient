using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using JetBrains.Annotations;
using NLog;
using NomadLogic.Common.Services;
using NomadLogic.Common.Services.Configuration;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Networking.VimeoApi;
using NomadLogic.iBrandDesktop.Services;
using NomadLogic.iBrandDesktop.Util.SubmitAction.Generic.Worker;

// ReSharper disable InconsistentNaming

namespace NomadLogic.iBrandDesktop.Util.SubmitAction.BaseClasses
{
    /// <summary>
    /// Classe de SubmitAction pour gérer la quasi-totalité des opérations pour créer et gérer un fichier vidéo en Post-Submit.
    /// </summary>
    public abstract class AbstractCreateVideoAndSendSubmitAction : AbstractSubmitAction,
        ISubmitActionCopyToBackUp, ISubmitActionSendFilesToPrintstation, ISubmitActionSendFilesToFtp
    {
        // ReSharper disable once UnusedMember.Local
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        [PublicAPI]
        public const string NO_FTP_NOMAD = "skip_ftp_nomad";
        [PublicAPI]
        public const string NO_VIDEO_EDIT = "skip_video_edit";
        [PublicAPI]
        public const string VIDEO_FILE_NAME_FORMAT = "video_file_name_format";
        [PublicAPI]
        public const string QUESTION_VIDEO_FILE = "question_video_file";
        [PublicAPI]
        public const string QUESTION_VIDEO_FILE_SAVE = "question_video_file_save";
        [PublicAPI]
        public const string QUESTION_THUMBNAIL = "question_thumbnail";
        [PublicAPI]
        public const string THUMBNAIL_TIME = "thumbnail_time";
        [PublicAPI]
        public const string SHOW_ON_TV = "show_on_tv";
        [PublicAPI]
        public const string DELETE_MOV = "delete_mov";

        [PublicAPI]
        public const string QUESTION_VIMEO_IDS = "question_vimeo";
        [PublicAPI]
        public const string QUESTION_VIMEO_FILES = "question_vimeo_files";
        [PublicAPI]
        public const string VIMEO_ACCOUNT_ENUM = "vimeo_account_enum";
        [PublicAPI]
        public const string ALBUM = "album";

        [PublicAPI]
        public const string BKP_QUESTIONS = "question_no_backup";
        [PublicAPI]
        public const string BKP_DIR_TREE = "bkp_dir_tree";
        [PublicAPI]
        public const string BKP_FILE_NAME = "bkp_file_name";
        [PublicAPI]
        public const string BKP_OPTIONAL = "optional_backup";

        [PublicAPI]
        public const string ACTIVATE_SEND_TO_PS = "send_to_ps";
        [PublicAPI]
        public const string NBR_MIN_PRINTSTATION = "nbr_min_printstation";
        [PublicAPI]
        public const string FILE_QUESTIONS = "file_questions";
        [PublicAPI]
        public const string ANSWER_QUESTIONS = "answer_questions";
        [PublicAPI]
        public const string FILE_TO_SHOW_GALLERY = "file_to_show_gallery";

        [PublicAPI]
        public const string FTP_USER = "ftp_user";
        [PublicAPI]
        public const string FTP_PASS = "ftp_pass";
        [PublicAPI]
        public const string FTP_URL = "ftp_url";
        [PublicAPI]
        public const string FTP_PORT = "ftp_port";
        [PublicAPI]
        public const string FTP_DIR = "ftp_dir";
        [PublicAPI]
        public const string FTP_QUESTIONS = "ftp_questions";
        [PublicAPI]
        public const string FTP_OPTIONAL = "ftp_optional_files";
        [PublicAPI]
        public const string FTP_DESTROY = "ftp_destroy_files";
        [PublicAPI]
        public const string FTP_SYNCH_WITH_NO_FILE = "ftp_synch_with_no_file";

        [PublicAPI]
        public const string WEB_REQUEST_DELETE_USERS = "web_condensed_delete_used_respondent_number";

        private int[] _psFileQuestions;
        private int[] _psAnswerQuestions;

        public int? WebRequestDeleteUsedNumberQuestionNo { get; set; }
        public bool OptionalBackup { get; private set; } = true;
        public bool UseDirectoryTree => DirectoryTreeToken != null;
        public string DirectoryTreeToken { get; private set; }
        public int[] QuestionsVideoFiles { get; private set; }
        public int QuestionVideoSaveTo { get; private set; } = -1;
        public int[] QuestionsVimeoIds { get; private set; }

        // Peut être un duplicata de "QuestionsVideoFiles" si non-explicitement rempli.
        public int[] QuestionsVimeoFilesToSend { get; private set; }
        public int[] QuestionsThumbnails { get; private set; }
        public int[] QuestionsFileToBackUp { get; private set; }
        public string FileNameToken { get; private set; }
        
        /// <inheritdoc />
        public int MinPrintstationToSend { get; private set; }
        /// <inheritdoc />
        public int FileToShowGallery { get; private set; }
        /// <inheritdoc />
        public ReadOnlyCollection<int> FileQuestions => Array.AsReadOnly(_psFileQuestions);
        /// <inheritdoc />
        public ReadOnlyCollection<int> AnswerQuestions => Array.AsReadOnly(_psAnswerQuestions);
        /// <inheritdoc />
        public bool SendToPrintstationActivated { get; private set; }
        /// <inheritdoc />
        public string FtpUser { get; private set; }
        /// <inheritdoc />
        public string FtpPassword { get; private set; }
        /// <inheritdoc />
        public string FtpAdress { get; private set; }

        public int FtpPort { get; private set; }
        /// <inheritdoc />
        public string FtpDir { get; private set; }
        /// <inheritdoc />
        public int[] FtpQuestions { get; private set; }
        /// <inheritdoc />
        public bool FtpOptionalFiles { get; private set; }
        /// <inheritdoc />
        public bool FtpDestroyFiles { get; private set; }
        /// <inheritdoc />
        public bool FtpSynchRespondentWithMissingFiles { get; private set; }

        /// <summary>
        /// Indique comment le nom du fichier final doit être nomenclaturé.
        /// Est composé d'un nom (et d'une extension) avec de multiple jetons.
        /// Doit contenir au moins un jeton, sinon sera remplacé par le format de base: {RANDOM}_t.mp4
        /// Sera automatiquement ajouté "_t.mp4" si le nom de fichier, une fois transformé, ne se termine pas avec ça.
        /// </summary>
        [PublicAPI]
        public string VideoFileNameFormat { get; private set; }

        // Indique qu'il faut se débarrasser du fichier vidéo original après transformation.
        public bool DeleteOriginalMovieFile { get; private set; }

        /// <inheritdoc />
        public bool NoVideoEdit { get; private set; }

        /// <inheritdoc />
        public bool NoNomadFtp { get; private set; }


        protected override void InstanciateSubmitActionPools()
        {
            AddSubmitActionWorkerPool("CreateVideo", 1);
            AddSubmitActionWorkerPool("UploadToVimeo", 3);
            AddSubmitActionWorkerPool("BackUp", 3);
            AddSubmitActionWorkerPool("SendToPs", 3);
            AddSubmitActionWorkerPool("SendToFtp", 3);
        }
        
        public override void TransformNotReadyToSyncRespondents(CancellationToken? ct)
        {
            var db = ServicesContainer.GetService<IbrandDatabase>();
            var respondentList = db.GetRespondentsBySyncStatus(Convert.ToString(_terminalInfo.ProjectId));

            UiSynchro?.SetProgressBarMinMax(0, respondentList.Count);

            foreach (var respId in respondentList)
            {
                if (ct != null && ct.Value.IsCancellationRequested) break;

                var respondent = db.GetRespondent(respId);

                respondent.Answers = db.GetAnswers(respId);
                respondent.DynamicHeaders = db.GetDynamicHeaders(respId);

                var temp = CreateWorker(respondent, false);
                var step = 0;
                do
                {
                    UiSynchro?.AddUiMessage($"Doing step {++step} of SubmitAction", 4);
                    temp.DoWork(ct);
                    temp = temp.NextWorkerStep;
                } while (temp != null && ct != null && !ct.Value.IsCancellationRequested);

                UiSynchro?.AddProgressStep(1);
            }
        }

        /// <inheritdoc />
        protected override void ValidateConfigData()
        {
            #region config transformation video

            if (_configData.ContainsKey(NO_FTP_NOMAD))
            {
                NoNomadFtp = bool.Parse(_configData[NO_FTP_NOMAD]);
            }

            if (_configData.ContainsKey(NO_VIDEO_EDIT))
            {
                NoVideoEdit = bool.Parse(_configData[NO_VIDEO_EDIT]);
            }
            else
            {
                QuestionsVideoFiles = new int[0];
            }

            if (_configData.ContainsKey(QUESTION_VIDEO_FILE))
            {
                QuestionsVideoFiles = StringUtil.GetDataFromDelimitedString(_configData[QUESTION_VIDEO_FILE],
                    StringUtil.STRING_DELIMITER_CHARS);
            }
            else
            {
                QuestionsVideoFiles = new int[0];
            }

            if (!QuestionsVideoFiles.Any())
            {
                throw new System.Exception("Missing critical configuration in config file: " + QUESTION_VIDEO_FILE);
            }

            if (_configData.ContainsKey(QUESTION_VIDEO_FILE_SAVE))
            {
                if (int.TryParse(_configData[QUESTION_VIDEO_FILE_SAVE], out int qSave))
                {
                    QuestionVideoSaveTo = qSave;
                }
            }

            if (_configData.ContainsKey(QUESTION_THUMBNAIL))
            {
                QuestionsThumbnails = StringUtil.GetDataFromDelimitedString(_configData[QUESTION_THUMBNAIL],
                    StringUtil.STRING_DELIMITER_CHARS);
            }
            else
            {
                QuestionsThumbnails = new int[0];
            }
            
            if (!_configData.ContainsKey(THUMBNAIL_TIME))
            {
                _configData.Add(THUMBNAIL_TIME, "1.0");
            }
            if (!_configData.ContainsKey(SHOW_ON_TV))
            {
                _configData.Add(SHOW_ON_TV, "false");
                
            }
            if (!_configData.ContainsKey(DELETE_MOV))
            {
                _configData.Add(DELETE_MOV, "false");
            }
            else
            {
                DeleteOriginalMovieFile = bool.Parse(_configData[DELETE_MOV]);
            }

            #endregion

            #region config Vimeo

            if (_configData.ContainsKey(QUESTION_VIMEO_IDS))
            {
                QuestionsVimeoIds = StringUtil.GetDataFromDelimitedString(_configData[QUESTION_VIMEO_IDS],
                    StringUtil.STRING_DELIMITER_CHARS);
            }
            else
            {
                QuestionsVimeoIds = new int[0];
            }

            if (QuestionsVimeoIds.Any())
            {
                if (!_configData.ContainsKey(ALBUM))
                {
                    _configData.Add(ALBUM, "");
                }

                // Si vide, on prend les questions videos de base plus haut
                if (_configData.ContainsKey(QUESTION_VIMEO_FILES))
                {
                    QuestionsVimeoFilesToSend = StringUtil.GetDataFromDelimitedString(_configData[QUESTION_VIMEO_FILES],
                        StringUtil.STRING_DELIMITER_CHARS);
                }
                else
                {
                    QuestionsVimeoFilesToSend = QuestionsVideoFiles;
                }

                if (_configData.ContainsKey(VIMEO_ACCOUNT_ENUM))
                {
                    // On vérifie que l'enum est valide.
                    if (!Enum.TryParse(_configData[VIMEO_ACCOUNT_ENUM], true,
                        out VimeoAccountLogin _)) // variable initié mais inutilisée.
                    {
                        throw new SystemException(
                            $"Invalid enum name for vimeo account {_configData[VIMEO_ACCOUNT_ENUM]}. Valid representations are: [{StringUtil.FormatEnumerable(Enum.GetNames(typeof(VimeoAccountLogin)))}]");
                    }
                }
                else
                {
                    _configData.Add(VIMEO_ACCOUNT_ENUM, "");
                }
            }

            #endregion

            #region config Back-up
            if (_configData.ContainsKey(BKP_QUESTIONS))
            {
                QuestionsFileToBackUp =
                    StringUtil.GetDataFromDelimitedString(_configData[BKP_QUESTIONS],
                        StringUtil.STRING_DELIMITER_CHARS);
            }
            else
            {
                QuestionsFileToBackUp = new int[0];
            }

            if (_configData.ContainsKey(BKP_OPTIONAL))
            {
                OptionalBackup = bool.Parse(_configData[BKP_OPTIONAL]);
            }

            if (!OptionalBackup && !QuestionsFileToBackUp.Any()) throw new SystemException("No valid question(s) number set up for mandatory backup of files.");

            DirectoryTreeToken = _configData.ContainsKey(BKP_DIR_TREE) ? _configData[BKP_DIR_TREE] : null;

            FileNameToken = _configData != null && _configData.ContainsKey(BKP_FILE_NAME) ? _configData[BKP_FILE_NAME] : null;
            
            var conf = ServicesContainer.GetService<NomadConfiguration>();

            if (!DirectoryUtil.IsBackupDirAvailiable(conf) && !OptionalBackup)
            {
                var errText = "Back Up directory is not availiable. Please ensure this drive can be reached: " + conf.BackupDrive;
                log.Error(errText);
                MessageBox.Show(Resources.iBrand.Copyright, Resources.iBrand.BackupDirNotAvailiable + $@" ({conf.BackupDrive})");
            }
            #endregion

            #region config Printstation

            if (_configData.ContainsKey(ACTIVATE_SEND_TO_PS))
            {
                SendToPrintstationActivated = bool.Parse(_configData[ACTIVATE_SEND_TO_PS]);
            }

            _psAnswerQuestions = new int[0];
            _psFileQuestions = new int[0];
            if (SendToPrintstationActivated)
            {
                if (!_configData.ContainsKey(NBR_MIN_PRINTSTATION))
                {
                    throw new System.Exception("Missing critical configuration in config file: " + NBR_MIN_PRINTSTATION);
                }
                if (!_configData.ContainsKey(FILE_QUESTIONS))
                {
                    throw new System.Exception("Missing critical configuration in config file: " + FILE_QUESTIONS);
                }
                if (!_configData.ContainsKey(FILE_TO_SHOW_GALLERY))
                {
                    throw new System.Exception("Missing critical configuration in config file: " + FILE_TO_SHOW_GALLERY);
                }

                MinPrintstationToSend = int.Parse(_configData[NBR_MIN_PRINTSTATION]);
                if (MinPrintstationToSend < 0)
                {
                    throw new System.Exception("Cannot send files to a negative number of Printstations.");
                }

                FileToShowGallery = int.Parse(_configData[FILE_TO_SHOW_GALLERY]);

                var tmpListeQuestion = new List<int>();
                foreach (var t in _configData[FILE_QUESTIONS].Split(StringUtil.STRING_DELIMITER_CHARS).Where(t => !string.IsNullOrEmpty(t)))
                {
                    try
                    {
                        tmpListeQuestion.Add(int.Parse(t));
                    }
                    catch (System.Exception e)
                    {
                        log.Error(e, "Error parsing questions numbers. Must be in the format 'X;Y;Z', where only integers are used. Following string is inadequate: {0}",
                            _configData[FILE_QUESTIONS]);
                        throw;
                    }
                }

                _psFileQuestions = tmpListeQuestion.ToArray();
                if (_psFileQuestions.Length < 1)
                {
                    throw new System.Exception("At least one question is necessary to send to Printstations.");
                }

                if (_configData.ContainsKey(ANSWER_QUESTIONS))
                {
                    var tmpAnswerQuestions = new List<int>();
                    foreach (var t in _configData[ANSWER_QUESTIONS].Split(StringUtil.STRING_DELIMITER_CHARS).Where(t => !string.IsNullOrEmpty(t)))
                    {
                        try
                        {
                            tmpAnswerQuestions.Add(int.Parse(t));
                        }
                        catch (System.Exception e)
                        {
                            log.Error(e,
                                "Error parsing questions numbers. Must be in the format 'X;Y;Z', where only intergers are used. Following string is inadequate: {0}",
                                _configData[ANSWER_QUESTIONS]);
                            throw;
                        }
                    }

                    _psAnswerQuestions = tmpAnswerQuestions.ToArray();
                }
            }

            #endregion

            #region Send to FTP

            FtpQuestions = _configData.ContainsKey(FTP_QUESTIONS) ? 
                StringUtil.GetDataFromDelimitedString(_configData[FTP_QUESTIONS]) : 
                new int[0];
            
            if (FtpQuestions.Any())
            {
                // Obligatoires
                if (!_configData.ContainsKey(FTP_USER)) throw new System.Exception("Missing critical configuration in config file: " + FTP_USER);
                FtpUser = _configData[FTP_USER];

                if (!_configData.ContainsKey(FTP_PASS)) throw new System.Exception("Missing critical configuration in config file: " + FTP_PASS);
                FtpPassword = _configData[FTP_PASS];

                if (!_configData.ContainsKey(FTP_URL)) throw new System.Exception("Missing critical configuration in config file: " + FTP_URL);
                FtpAdress = _configData[FTP_URL];
                FtpPort = int.Parse(_configData[FTP_PORT]);

                // Non-obligatoire (ou avec défaut valide)
                if (_configData.ContainsKey(FTP_DESTROY))
                {
                    FtpDestroyFiles = bool.Parse(_configData[FTP_DESTROY]);
                }
                if (_configData.ContainsKey(FTP_SYNCH_WITH_NO_FILE))
                {
                    FtpSynchRespondentWithMissingFiles = bool.Parse(_configData[FTP_SYNCH_WITH_NO_FILE]);
                }
                if (_configData.ContainsKey(FTP_OPTIONAL))
                {
                    FtpOptionalFiles = bool.Parse(_configData[FTP_OPTIONAL]);
                }
                if (_configData.ContainsKey(FTP_DIR))
                {
                    FtpDir = _configData[FTP_OPTIONAL];
                }
            }

            #endregion

            #region video file name token

            if (_configData.ContainsKey(VIDEO_FILE_NAME_FORMAT))
            {
                VideoFileNameFormat = _configData[VIDEO_FILE_NAME_FORMAT];
            }

            #endregion

            if (_configData.ContainsKey(WEB_REQUEST_DELETE_USERS))
            {
                WebRequestDeleteUsedNumberQuestionNo = int.Parse(_configData[WEB_REQUEST_DELETE_USERS]);
            }
        }
        
        /// <inheritdoc />
        protected override ISubmitActionWorker CreateWorker(Respondent resp, bool usePool = true)
        {
            var stepList = new List<bool>
            {
                true, // transformation vidéo. (duh!)
                true, // On tente à chaque fois d'envoyer vers Vimeo.
                QuestionsFileToBackUp.Any(), // On tente un backup si on a au moins une question de backup
                SendToPrintstationActivated, // Vérifie si on envoi à un printstation.
                FtpQuestions.Any() // Si on envoie des fichiers sur un FTP
            };
            
            var stepInfo = new SubmitActionWorkerNextStepInfo(stepList,
                usePool && _submitActionWorkerPools.Any() ? _submitActionWorkerPools.ToArray() : null);

            #region création des settings vidéos
            var baseDir = new DirectoryInfo(GetTerminalDirectory);
            var outputDir = new DirectoryInfo(DirectoryUtil.GetLocalIbrandTerminalDataDir(TerminalInfo,
                DirectoryUtil.ProjectDataDirectoriesEnum.videos, DirectoryUtil.ProjectDataDirectoriesModEnum.none));
            var outputDirThumbnail = new DirectoryInfo(DirectoryUtil.GetLocalIbrandTerminalDataDir(TerminalInfo, 
                DirectoryUtil.ProjectDataDirectoriesEnum.videos, DirectoryUtil.ProjectDataDirectoriesModEnum.thumbnail));
            var questionsFichierNo = QuestionsVideoFiles;
            var questionsThumbnail = QuestionsThumbnails;
            var thumbTime = float.Parse(_configData[THUMBNAIL_TIME], CultureInfo.InvariantCulture);
            var showOnTv = bool.Parse(_configData[SHOW_ON_TV]);

            var settings = new SubmitVideoTransformationSetting
            {
                BaseDirectory = baseDir,
                OutputVideoDirectory = outputDir,
                OutputThumbnailDirectory = outputDirThumbnail,
                ThumbnailTime = thumbTime,
                ResultFileNamePattern = VideoFileNameFormat,
                CopyToShowOnTv = showOnTv
            };

            for (var i = 0; i < questionsFichierNo.Length; i++)
            {
                var vid = new VideoSetting
                {
                    OriginalVideoFileQuestionNo = questionsFichierNo[i],
                    OutputVideoFileQuestionNo = QuestionVideoSaveTo == -1 ? null : (int?)QuestionVideoSaveTo,
                    NoVideoEdition = NoVideoEdit,
                    DestroyOriginalMultimediaFile = DeleteOriginalMovieFile
                };

                ThumbnailSetting thumb = null;

                if (questionsThumbnail != null && questionsThumbnail.Length < i)
                {
                    thumb = new ThumbnailSetting
                    {
                        ThumbnailQuestionNo = questionsThumbnail[i]
                    };
                }

                settings.VideoQuestions.Add(new ValueTuple<VideoSetting, ThumbnailSetting>(vid, thumb));
            }
            #endregion

            // On commence dans cette classe par un worker de création vidéo.
            var worker = new CreateVideoWorker(this, resp, resp.Id, stepInfo)
            {
                VideoCreationSettings = settings
            };
            
            AssignVideoCreationMethodDelegate(worker);

            return worker;
        }

        protected abstract void AssignVideoCreationMethodDelegate(CreateVideoWorker worker);

    }
}