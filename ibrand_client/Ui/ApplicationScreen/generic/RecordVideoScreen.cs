using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Canon.Eos.Framework;
using Canon.Eos.Framework.Eventing;
using JetBrains.Annotations;
using NLog;
using NomadLogic.Common;
using NomadLogic.Common.Extentions;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Services;
using NomadLogic.Common.Services.Configuration;
using NomadLogic.Common.Ui.Dialog;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Win32;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers;
using NomadLogic.Multimedia;
using NomadLogic.Multimedia.Cli.Ffmpeg;
using Timer = System.Windows.Forms.Timer;

#pragma warning disable 612
#pragma warning disable 618

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic
{
    /// <summary>
    ///     Classe pour enregistrer un vidéo via une caméra T3i, de façon la plus configurable possible.
    /// </summary>
    [PublicAPI]
    public class RecordVideoScreen : AbstractScreen
    {
        public delegate void StateChangedHandler(object sender, RecordVideoScreenStates args);

        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private readonly BitmapModificationFilter? _bmf;
        private readonly Rectangle? _cancelArea;
        private readonly CanonCameraManager _canonManager;

        private readonly bool _compressFile;
        private readonly Rectangle? _doneArea;
        private readonly IList<string> _fileNames;
        private readonly object _lock = new object();

        private readonly PictureBox _panelPreview;

        private readonly Rectangle? _startArea;
        private readonly object _stateLock = new object();
        private readonly Rectangle? _stopArea;
        private readonly int _timeBeforeRecordDefault;

        private readonly Timer _timer;
        
        /// <summary>
        /// Temps d'enregistrement du vidéo.
        /// </summary>
        protected int TimeToRecordDefault { get; set; }
        private Bitmap _doneEnabledButton;

        /// Cette liste ne sera jamais null, mais peut être vide.
        private IList<AnimatedImageSequenceEnumTrigger<RecordVideoScreenStates>> _enumSequences =
            new List<AnimatedImageSequenceEnumTrigger<RecordVideoScreenStates>>();

        private IList<RecordVideoScreenStates> _listPreviewShowStates = new List<RecordVideoScreenStates>
        {
            RecordVideoScreenStates.StandBy,
            RecordVideoScreenStates.PreRecording,
            RecordVideoScreenStates.Recording
        };

        private Bitmap _startEnabledButton;

        private RecordVideoScreenStates _state = RecordVideoScreenStates.ScreenInactive;

        /// Séquence d'image de l'enregistrement.
        [Obsolete] private AnimatedImageSequence _staticSequence;

        private Bitmap _stopEnabledButton;

        private int _timeBeforeRecord;
        private int _timeToRecord;

        /// Séquence d'image de l'enregistrement.
        [Obsolete] private AnimatedImageSequence _transferSequence;

        protected string _videoFileName;

        /// <summary>
        ///     Constructeur principal pour un écran d'enregistrement vidéo.
        /// </summary>
        /// <param name="sc">Les paramètres généraux de l'écran.</param>
        /// <param name="backgroundImgEnSrc">L'image de fond anglais.</param>
        /// <param name="backgroundImgFrSrc">L'image de fond français.</param>
        /// <param name="preview">
        ///     Optionnel. La taille et position de l'élément de prévisualisation. Si null, on ne fait pas de "preview".
        ///     Si la valeur de X est de zéro, on calcule selon le centre de l'écran.
        /// </param>
        /// <param name="beforeCountdown">Le nombre de seconde avant le début de l'enregistrement.</param>
        /// <param name="maxTimeCountdown">Le nombre de seconde maximal de l'enregistrement.</param>
        /// <param name="bmf">Les paramètre de modification des images.</param>
        /// <param name="startZone">La zone de démarrage de l'enregistrement vidéo.</param>
        /// <param name="cancelZone">Zone pour tout arrêter et revenir à l'écran principal.</param>
        /// <param name="stopZone">La zone pour stopper l'enregistrement vidéo.</param>
        /// <param name="doneZone">La zone pour passer à la page suivante, une fois enregistré.</param>
        /// <param name="compressFile">
        ///     Indique s'il faut faire une compression vidéo. Normalement TRUE, mais si on sait que l'on peut le faire plus tard, dans
        ///     un processus dédié (comme un SubmitAction), il est préférable de ne pas faire cette opération immédiatement, car cela relentit le processus.
        /// </param>
        /// <param name="fileNames">Noms des fichiers de boutons, si on ne prend pas ceux par défaut.</param>
        public RecordVideoScreen(ScreenConfig sc, string backgroundImgEnSrc, string backgroundImgFrSrc, Rectangle? preview,
            int beforeCountdown, int maxTimeCountdown, BitmapModificationFilter? bmf, Rectangle? startZone,
            Rectangle? cancelZone, Rectangle? stopZone, Rectangle? doneZone, bool compressFile, IList<string> fileNames = null)
            : base(backgroundImgEnSrc, backgroundImgFrSrc ?? backgroundImgEnSrc, sc)
        {
            _timeBeforeRecordDefault = beforeCountdown;
            TimeToRecordDefault = maxTimeCountdown;
            _timeBeforeRecord = _timeBeforeRecordDefault;
            _timeToRecord = TimeToRecordDefault;
            _compressFile = compressFile;
            _fileNames = fileNames;

            if (preview != null)
            {
                _panelPreview = new PictureBox
                {
                    Location = new Point(preview.Value.X < 1 ? (PictureBox.Size.Width - preview.Value.Width) / 2 : preview.Value.X, preview.Value.Y),
                    Size = preview.Value.Size,
                    BackColor = Color.White
                };
                Controls.Add(_panelPreview);
                _panelPreview.BringToFront();
                _panelPreview.Visible = false;
            }

            _timer = new Timer {Interval = 1000};
            _timer.Tick += OnTimerEventRecord;

            _bmf = bmf;

            _startArea = startZone;
            _stopArea = stopZone;
            _doneArea = doneZone;
            _cancelArea = cancelZone;

            RecordingStateChanged += OnRecordingStateChanged;

            _canonManager = CanonCameraManager.GetInstance();
            if (_panelPreview != null)
            {
                _canonManager.LiveViewUpdate += HandlePictureUpdate;
            }
            _canonManager.FileCreatedOnCamera += OnFileCreatedOnCamera;
            _canonManager.FileSavedOnHost += OnFileSavedOnHost;
        }

        /// Indique les états où la prévisualisation est visible à l'écran.
        /// Par défaut, la visualisation est active dans l'état "Pre-Recording", "Stand-By" et "Recording".
        /// S'arrange pour ne jamais être nul, mais peut être vide. Ce qui désactive la visualisation.
        /// Il n'y a aucune visualisation dans tous les cas si on ne spécifie pas une zone de prévisualisation avec le constructeur.
        /// L'état "ScreenInactive" n'affiche jamais le preview quoi qu'il arrive.
        [PublicAPI]
        public IList<RecordVideoScreenStates> PreviewOnStates
        {
            get => _listPreviewShowStates;
            set => _listPreviewShowStates = value ?? new List<RecordVideoScreenStates>();
        }

        /// Configuration pour démarrer un vidéo VLC durant le décompte.
        /// Prend en considération les configurations Nomad pour localiser VLC, mais le fichier vidéo est local au questionnaire.
        public VlcVideoCountdownInfo StartVideoDuringCountdown { get; set; }

        /// Contient une liste de séquences animées, qui sont utilisées et affichées uniquement quand un certain état est actif.
        /// Si on fournit des états à une séquence (vide par défaut), l'affichage de la séquence se fera uniquement lors
        /// des états énumérés. Si vide, la séquence se fear de façon constante.
        /// Une liste vide est permise, mais pas la valeur nulle. Une liste contenant une séquence nulle ou vide sera aussi refusée.
        /// Toute erreur lance une exception lors de l'assignation.
        /// L'ancienne liste est désassignée et disposée sur chaque élément.
        [PublicAPI]
        public IList<AnimatedImageSequenceEnumTrigger<RecordVideoScreenStates>> ImageSequences
        {
            get => _enumSequences;
            set
            {
                if (value == null || value.Any(l => l.ImageSequence == null))
                    throw new InvalidEnumArgumentException(@"Cannot accept list with invalid (null) sequence.");

                // Déassignation de la liste actuelle
                foreach (var seq in _enumSequences)
                {
                    seq.ImageSequence.StopSequence();
                    seq.ImageSequence.UpdateDraw -= InvokeRefresh;
                    seq.ImageSequence.Dispose();
                }

                _enumSequences = value;
                foreach (var seq in _enumSequences) seq.ImageSequence.UpdateDraw += InvokeRefresh;
            }
        }

        [PublicAPI]
        [Obsolete]
        public AnimatedImageSequence StaticImageSequence
        {
            get => _staticSequence;
            set
            {
                if (_staticSequence != null)
                {
                    _staticSequence.StopSequence();
                    _staticSequence.UpdateDraw -= InvokeRefresh;
                    _staticSequence.Dispose();
                    _staticSequence = null;
                }

                _staticSequence = value;
                _staticSequence.UpdateDraw += InvokeRefresh;
            }
        }

        [PublicAPI]
        [Obsolete]
        public AnimatedImageSequence TransferImageSequence
        {
            get => _transferSequence;
            set
            {
                if (value == null) throw new System.Exception("Image sequence cannot be null.");

                if (_transferSequence != null)
                {
                    _transferSequence.StopSequence();
                    _transferSequence.UpdateDraw -= InvokeRefresh;
                    _transferSequence.Dispose();
                    _transferSequence = null;
                }

                _transferSequence = value;
                _transferSequence.UpdateDraw += InvokeRefresh;
            }
        }

        /// <summary>
        ///     si non nul force l'enregistrement de la fichier video a une question precise,
        ///     en remplacant la fonctionnalite de ScreenConfig
        /// </summary>
        [PublicAPI]
        public int? ForceSaveVideoToQuestionNo { get; set; }

        /// <summary>
        ///     Décorateur pour l'affichage du décompte. Si null alors de "OnLoad", il sera créé avec une version de base.
        /// </summary>
        [PublicAPI]
        public ICountdownDecorator CountdownDecorator { get; set; }

        /// <summary>
        ///     Paramètres de création d'un thumbnail à partir du vidéo traité.
        /// </summary>
        [PublicAPI]
        public ThumbnailCreationParameters ThumbnailCreationParameters { get; set; }

        /// <summary>
        ///     Indique que si on compresse le vidéo, on enlève aussi l'audio (si true).
        /// </summary>
        [PublicAPI]
        public bool RemoveAudioFromVideo { get; set; }

        /// <summary>
        ///     Indique le nombre de page à revenir en arrière dans le cas que cette valeur est non-nulle.
        ///     Si null, effectue une cancellation du questionnaire.
        /// </summary>
        [PublicAPI]
        public int? CancelBackPage { get; set; } = 1;

        /// <summary>
        ///     Indique, si mis à TRUE, qu'il faut partir immédiatement la séquence de saisie vidéo.
        ///     Il est préférable d'avoir un décompte dans le décorateur du timer pour laisser un peu de temps à l'usager de se préparer.
        /// </summary>
        [PublicAPI]
        public bool AutomaticStart { get; set; }

        public event StateChangedHandler RecordingStateChanged;

        private void OnRecordingStateChanged(object sender, RecordVideoScreenStates args)
        {
            UpdateVisiblePreview();
        }

        /// À utiliser quand on veut changer l'état de l'instance.
        /// Gère tous les lancements d'événements appropriés.
        [PublicAPI]
        protected void ChangeRecordingState(RecordVideoScreenStates newState)
        {
            lock (_stateLock)
            {
                if (_state == newState) return;
                log.Debug($"Record screen state change: '{_state}' to '{newState}'");
                _state = newState;
                RecordingStateChanged?.Invoke(this, newState);
            }
        }

        /// <inheritdoc />
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (UseCountdownDecorator)
            {
                if (CountdownDecorator == null)
                {
                    // Positions par défaut.
                    var numberTopCenterPoint = new Point(PictureBox.Width / 2, 100 + 30 + (_panelPreview?.Height ?? 0));
                    var messageTopCenterPoint = new Point(PictureBox.Width / 2, numberTopCenterPoint.Y + 20);

                    CountdownDecorator = new CountdownDecoratorBasicImages
                    {
                        MaxTimeCountdown = 30,
                        GetImageFileMethod = GetBitmap,
                        GetImageFileNoneLanguageMethod = GetNoneLanguageBitmap,
                        UpperCenterPointMessage = messageTopCenterPoint,
                        UpperCenterPointNumber = numberTopCenterPoint
                    };
                }

                if (CountdownDecorator.GetImageFileMethod == null)
                    CountdownDecorator.GetImageFileMethod = GetBitmap;
                if (CountdownDecorator.GetImageFileNoneLanguageMethod == null)
                    CountdownDecorator.GetImageFileNoneLanguageMethod = GetNoneLanguageBitmap;
            }

            InitBitmaps();
        }

        /// Utiliser pour activer l'obsolète countdown decorator.
        public bool UseCountdownDecorator { get; set; }

        /// <inheritdoc />
        public override bool ChangeLanguage(string language)
        {
            var change = base.ChangeLanguage(language);
            if (!change) return false;
            InitBitmaps();
            return true;
        }

        private void InitBitmaps()
        {
            this.SafeCall(() =>
            {
                lock (_lock)
                {
                    log.Trace("Record screen bitmaps redo. Initialising to {0}", _activeLg);
                    CountdownDecorator?.Init(_bmf);

                    TransferImageSequence?.ClearCurrentImages();
                    TransferImageSequence?.CreateImages(_activeLg);
                    StaticImageSequence?.ClearCurrentImages();
                    StaticImageSequence?.CreateImages(_activeLg, GetCurrentQuestionnaire(), GetCurrentRespondentWithAnswers());

                    foreach (var seq in _enumSequences)
                    {
                        seq.ImageSequence.ClearCurrentImages();
                        seq.ImageSequence.CreateImages(_activeLg, GetCurrentQuestionnaire(), GetCurrentRespondentWithAnswers());
                    }

                    /* TODO Ce fonctionnement est exactement le contraire de ce qui se fait sur les autres écrans, à savoir, les image d'extra sont pour bloquer, et non pour activer comme ici.*/
                    if (_startArea != null) _startEnabledButton = ObtainModifiedPicture(_startEnabledButton, 0, "start_enable.png");

                    if (_stopArea != null) _stopEnabledButton = ObtainModifiedPicture(_stopEnabledButton, 1, "stop_enable.png");

                    if (_doneArea != null) _doneEnabledButton = ObtainModifiedPicture(_doneEnabledButton, 2, "save_enable.png");
                }
            }, err => { log.Error(err, "Could not create image files for screen."); });
        }

        private Bitmap ObtainModifiedPicture(Bitmap originalBmp, int index, string defaultName)
        {
            originalBmp?.Dispose();
            var name = _fileNames.GetArrayValueWithDefault(index, defaultName);
            var bmp = File.Exists(name) ? FileUtil.LoadBitmap(name) : GetBitmap(name);
            return ImageUtil.ModifyPicture(bmp, ColorSubstitutionFilterGroup.Icon, _bmf);
        }

        private void HandlePictureUpdate(object sender, EosImageEventArgs e)
        {
            if (!IsDisposed && _activated) UpdatePreviewPicture(e.GetImage());
        }

        private void OnFileSavedOnHost(object sender, EosFileEventArgs e)
        {
            if (IsDisposed || !_activated) return;

            Task.Run(() =>
            {
                var config = ServicesContainer.GetService<NomadConfiguration>();
                try
                {
                    log.Debug($"File saved on host from camera: {e.FilePath}");
                    if (_compressFile)
                    {
                        _videoFileName =
                            GetTerminalDirectoryDataFile( GenerateUniqueId.GetId() + ".mp4",
                                DirectoryUtil.ProjectDataDirectoriesEnum.videos,
                                DirectoryUtil.ProjectDataDirectoriesModEnum.none) ;

                        log.Trace("Compressing video file.");
                        FfmpegUtil.CompressVideo(config.FfmpegLocation, e.FilePath, _videoFileName, 24, RemoveAudioFromVideo);

                        if (!File.Exists(_videoFileName)) throw new System.Exception("Could not transform video to mp4.");

                        // On détruit le fichier original (plus besoin).
                        if (File.Exists(e.FilePath)) File.Delete(e.FilePath);
                    }
                    else
                    {
                        _videoFileName =
                            GetTerminalDirectoryDataFile(StringUtil.GetFileName(e.FilePath),
                                DirectoryUtil.ProjectDataDirectoriesEnum.videos,
                                DirectoryUtil.ProjectDataDirectoriesModEnum.none);
                        File.Move(e.FilePath, _videoFileName); // Si aucune compression, on déplace (pas copier) le fichier.
                    }

                    log.Trace($"File moved to: {_videoFileName}");

                    if (ForceSaveVideoToQuestionNo != null)
                        SaveAnswerToQuestion(new Answer {QuestionNo = ForceSaveVideoToQuestionNo.Value, Response = _videoFileName});
                    else if (_screenConfig.QuestionVideoNo != null)
                        SaveAnswerToQuestion(new Answer {QuestionNo = _screenConfig.QuestionVideoNo.Value, Response = _videoFileName});

                    if (ThumbnailCreationParameters == null) return;

                    var tmp = DirectoryUtil.CreateLocalTemporaryFile(GenerateUniqueId.GetId() + ".jpg");
                    FfmpegUtil.ExtractSingleFrame(config.FfmpegLocation, _videoFileName, ThumbnailCreationParameters.ThumbnailTime, tmp);
                    if (File.Exists(tmp))
                    {
                        var img = FileUtil.LoadBitmap(tmp);

                        var resize = ThumbnailCreationParameters.CropSize;
                        if (resize != null)
                        {
                            var tmpImage = img.Frame(resize.Value.Width, resize.Value.Height);
                            img.Dispose();
                            img = tmpImage;
                        }

                        var overlay = ThumbnailCreationParameters.OverlayFile;
                        if (overlay != null && File.Exists(overlay))
                            using (var overlayImg = FileUtil.LoadBitmap(overlay))
                            {
                                img.AddOverlay(overlayImg);
                            }

                        // Chemin final
                        var finalFile =
                            GetTerminalDirectoryDataFile(StringUtil.GetFileName(tmp),
                                DirectoryUtil.ProjectDataDirectoriesEnum.videos, DirectoryUtil.ProjectDataDirectoriesModEnum.thumbnail)
                            ;

                        img.Save(90, finalFile);

                        SaveAnswerToQuestion(new Answer {QuestionNo = ThumbnailCreationParameters.QuestionThumbnailNo, Response = finalFile});

                        img.Dispose();
                        File.Delete(tmp);
                        log.Trace($"Video thumbnail created: '{finalFile}'");
                    }
                    else
                    {
                        this.SafeCall(() =>
                            {
                                MsgBox.Show(_activeLg.Equals(Constants.FR)
                                    ? "N'a pas pu créer le fichier de Thumbnail."
                                    : "Could not create thumbnail file.");
                            });
                        log.Error("Could not extract Thumbnail. Check parameters.");
                    }
                }
                catch (System.Exception ex)
                {
                    log.Error(ex);
                }
                finally
                {
                    TransferImageSequence?.StopSequence();
                    this.SafeCall(Refresh);
                    VideoFileReadyOnDisk();
                }
            });
        }

        private void OnFileCreatedOnCamera(object sender, IntPtr dirItem, EosCamera camera)
        {
            if (IsDisposed || !_activated) return;

            Invoke(new Action(() =>
            {
                _timer.Stop();
                ChangeRecordingState(RecordVideoScreenStates.Transfering);
                TransferImageSequence?.StartSequence();
                Refresh();

                log.Trace("New file on camera detected.");

                try
                {
                    var dir = DirectoryUtil.GetLocalMediaDir(DirectoryUtil.NomadLogicMediaDirectoriesEnum.videos);
                    var fileName = dir + GenerateUniqueId.GetId() + ".MOV";

                    if (File.Exists(fileName)) File.Delete(fileName);
                    camera.SavePicture(dirItem, fileName);
                }
                catch (System.Exception ex)
                {
                    log.Error(ex);
                    ChangeRecordingState(RecordVideoScreenStates.StandBy);
                }
            }));
        }

        /// Active ou désactive la visibilité du preview selon l'état actuel.
        private void UpdateVisiblePreview()
        {
            if (_panelPreview == null) return;

            this.SafeCall(() =>
            {
                if (_listPreviewShowStates.Contains(_state))
                {
                    _panelPreview.Visible = true;
                    _canonManager.StartLiveView();
                }
                else
                {
                    _panelPreview.Visible = false;
                    _canonManager.StopLiveView();
                }

                Refresh();
            });
        }

        public RotateFlipType? PreviewImageRotation { get; set; }

        private void UpdatePreviewPicture(Image image)
        {
            if (_panelPreview == null || _panelPreview.Visible == false) return;
            if (InvokeRequired)
            {
                Invoke(new Action(() => UpdatePreviewPicture(image)));
            }
            else
            {
                var finalBitmap = image.ResizeForView(_panelPreview.Width, _panelPreview.Height);
                finalBitmap.RotateFlip(PreviewImageRotation ?? RotateFlipType.RotateNoneFlipX);
                _panelPreview.Image = finalBitmap;
                Refresh();
            }
        }

        private void VideoFileReadyOnDisk()
        {
            Invoke(new Action(() =>
            {
                Cursor.Current = Cursors.Default;
                ChangeRecordingState(RecordVideoScreenStates.VideoFileReady);
                if (_doneArea == null)
                {
                    Task.Run(() =>
                    {
                        log.Trace("Video file ready. Automatic next called.");
                        SuspendTimer();
                        Thread.Sleep(500);
                        PlayImageSequences(false);
                        Next();
                    });
                }
            }));
        }

        private void PlayImageSequences(bool play)
        {
            foreach (var seq in _enumSequences)
            {
                if (play) seq.ImageSequence.StartSequence();
                else seq.ImageSequence.StopSequence();
                this.SafeCall(Refresh);
            }
        }

        /// <inheritdoc />
        protected override bool OnMouseDownHandler(int x, int y)
        {
            if (!base.OnMouseDownHandler(x, y)) return false;

            if ((_state == RecordVideoScreenStates.VideoFileReady && _doneArea != null || _state == RecordVideoScreenStates.StandBy) &&
                InArea(x, y, _startArea))
            {
                StartVideoSequence();
                return false;
            }

            if (_state == RecordVideoScreenStates.VideoFileReady && InArea(x,y, _doneArea))
            {
                ChangeRecordingState(RecordVideoScreenStates.ScreenInactive);
                ResetTimer();

                StaticImageSequence?.StopSequence();
                PlayImageSequences(false);
                Next();
                return false;
            }

            if (InArea(x, y, _stopArea))
            {
                if (_state == RecordVideoScreenStates.Recording)
                {
                    _timer.Stop();
                    ResumeTimer();
                    StopRecording();
                }
                else if (_state == RecordVideoScreenStates.PreRecording)
                {
                    _timer.Stop();
                    ResumeTimer();
                    ChangeRecordingState(RecordVideoScreenStates.StandBy);
                }

                return false;
            }

            if (InArea(x, y, _cancelArea))
            {
                Cursor.Current = Cursors.WaitCursor;
                ChangeRecordingState(RecordVideoScreenStates.ScreenInactive);

                StopRecording(); // au cas où

                StaticImageSequence?.StopSequence();
                PlayImageSequences(false);

                if (CancelBackPage == null)
                {
                    ResetTimer();
                    FirstScreen();
                }
                else
                {
                    if (CancelBackPage != null) Back(CancelBackPage.Value);
                }

                return false;
            }

            return true;
        }

        protected virtual void StartVideoSequence()
        {
            this.SafeCall(() =>
            {
                Cursor.Current = Cursors.Default;

                if (File.Exists(_videoFileName))
                {
                    log.Trace("Deleting previous video file collected.");
                    File.Delete(_videoFileName);
                }

                ChangeRecordingState(RecordVideoScreenStates.PreRecording);

                _timeBeforeRecord = _timeBeforeRecordDefault;
                _timeToRecord = TimeToRecordDefault;
                _timer.Start();

                SuspendTimer(); // Le timer d'inactivité.
                Refresh();
            });
        }

        /// Tente de stopper la caméra. Si la caméra n'enregistrait pas, où si une erreur survient, on met l'état en standby.
        /// Sinon, cela va provoquer une séquence d'événements (donc, va éventuellement appeler FileOnCamera).
        protected void StopRecording(bool error = false)
        {
            _timer.Stop();
            try
            {
                Cursor.Current = Cursors.WaitCursor;
                if (_canonManager.StopRecord()) return;

                // En cas d'erreur on revient en standby.
                log.Trace("Stop command failed. Reverting to stand-by state.");
                Cursor.Current = Cursors.Default;
                ChangeRecordingState(RecordVideoScreenStates.StandBy);
            }
            catch (System.Exception ex)
            {
                try
                {
                    // ReSharper disable once LocalizableElement
                    MessageBox.Show(ex.Message + "\n\n" + ex);

                    if (File.Exists(_videoFileName)) File.Delete(_videoFileName);
                }
                catch (System.Exception e)
                {
                    log.Error(e);
                }
                finally
                {
                    log.Trace("Error duting stop command. Reverting to stand-by state.");
                    // En cas d'erreur on revient en standby.
                    ChangeRecordingState(RecordVideoScreenStates.StandBy);
                    error = true;
                }
            }
            finally
            {
                OnVideoRecordStop(error);
            }
        }

        protected virtual void OnVideoRecordStop(bool error = false)
        {
            // Rien de plus
        }

        /// <inheritdoc />
        protected override void OnPaintEvent(Graphics g)
        {
            base.OnPaintEvent(g);

            lock (_lock)
            {
                TransferImageSequence?.Paint(g);
                StaticImageSequence?.Paint(g);

                foreach (var seq in _enumSequences) if (seq.EnumFlags.Contains(_state)) seq.ImageSequence.Paint(g);

                if (CountdownDecorator != null)
                {
                    if (_state == RecordVideoScreenStates.Recording || _state == RecordVideoScreenStates.PreRecording)
                        CountdownDecorator.Draw(g, _timeBeforeRecord, _timeToRecord,
                            _state == RecordVideoScreenStates.Recording);
                }

                if (_state == RecordVideoScreenStates.VideoFileReady && _doneArea != null && _doneEnabledButton != null)
                    DrawImage(g, _doneEnabledButton, _doneArea.Value.Location);

                if ((_state == RecordVideoScreenStates.StandBy || _state == RecordVideoScreenStates.VideoFileReady && _doneArea != null) && _startArea != null && _startEnabledButton != null)
                    DrawImage(g, _startEnabledButton, _startArea.Value.Location);

                if (_stopArea != null && (_state == RecordVideoScreenStates.Recording || _state == RecordVideoScreenStates.PreRecording) && _stopEnabledButton != null)
                    DrawImage(g, _stopEnabledButton, _stopArea.Value.Location);
            }
        }

        /// <inheritdoc />
        public override void Recycle()
        {
            base.Recycle();

            log.Trace("Recycling.");
            _timer.Stop();

            _videoFileName = null;
            TransferImageSequence?.StopSequence();
            StaticImageSequence?.StopSequence();
            PlayImageSequences(false);

            ChangeRecordingState(RecordVideoScreenStates.ScreenInactive);
        }

        /// <inheritdoc />
        public override void SetActivated()
        {
            base.SetActivated();

            log.Trace("Activating.");

            UpdateImageSequences();

            lock (_stateLock)
            {
                StaticImageSequence?.StartSequence();
                
                if (_panelPreview != null)
                {
                    _panelPreview.BringToFront();
                    _panelPreview.Image?.Dispose();
                    _panelPreview.Image = null;
                }

                if (!_canonManager.IsCameraConnected())
                {
                    MessageBox.Show(Resources.iBrand.CameraNotConnected, Resources.iBrand.AppName, MessageBoxButtons.OK);
                    PlayImageSequences(false);
                    Back();
                    return;
                }
            }

            PlayImageSequences(true);

            Cursor.Current = Cursors.Default;

            if (AutomaticStart)
                StartVideoSequence();
            else
                ChangeRecordingState(RecordVideoScreenStates.StandBy);
        }

        private void UpdateImageSequences()
        {
            // Pour chaque sequence qui contient un générateur dépendant de l'état du questionnaire
            foreach (var seq in _enumSequences.Where(s => s.ImageSequence.ImageGenerators.Any(g => g is AnimatedImageSequenceQuestionnaire)))
            {
                seq.ImageSequence.ClearCurrentImages();
                seq.ImageSequence.CreateImages(_activeLg, GetCurrentQuestionnaire(), GetCurrentRespondentWithAnswers());
            }
        }
        
        private void OnTimerEventRecord(object sender, EventArgs e)
        {
            lock (_stateLock)
            {
                if (_timeBeforeRecord > 0)
                {
                    _timeBeforeRecord--;

                    if (StartVideoDuringCountdown?.StartTime == _timeBeforeRecord)
                    {
                        var config = ServicesContainer.GetService<NomadConfiguration>();

                        var error = false;

                        var execPath = config.VlcLocation;
                        if (string.IsNullOrWhiteSpace(execPath))
                        {
                            log.Warn("No Vlc location exists. Check configuration file.");
                            this.SafeCall(() =>
                            {
                                MessageBox.Show(_activeLg.Equals("en", StringComparison.InvariantCultureIgnoreCase) ? 
                                    "Vlc executable configuration does not exists. Please check configurations." : 
                                    "L'exécutable de Vlc n'est pas mentionné dans les configurations.", 
                                    @"iBrand App.", MessageBoxButtons.OK);
                            });
                            error = true;
                        }

                        if (!File.Exists(execPath))
                        {
                            log.Warn("Vlc location does not exists. Check configuration file. Location=" + execPath);
                            this.SafeCall(() =>
                            {
                                MessageBox.Show(_activeLg.Equals("en", StringComparison.InvariantCultureIgnoreCase) ?
                                        "Vlc executable does not exists. Please check configurations." :
                                        "L'exécutable de Vlc n'existe pas. Vérifiez les configurations.",
                                    @"iBrand App.", MessageBoxButtons.OK);
                            });
                            error = true;
                        }

                        if (!error)
                        {
                            var file = StartVideoDuringCountdown?.GetFileToPlay(_activeLg);

                            if (file == null)
                            {
                                log.Warn($"No valid file name in configuration: lg={_activeLg}, files ={StringUtil.FormatEnumerable(StartVideoDuringCountdown?.FilesToPlay?.GetEnumerator())}");
                                this.SafeCall(() =>
                                {
                                    MessageBox.Show(_activeLg.Equals("en", StringComparison.InvariantCultureIgnoreCase) ?
                                            "No valid video files set for project." :
                                            "Pas de fichier vidéo valide associé au projet.",
                                        @"iBrand App.", MessageBoxButtons.OK);
                                });
                            }
                            else
                            {
                                var filePath = GetTerminalDirectory() + file;
                                LaunchProcessUtil.LaunchProcess(execPath, $"\"{filePath}\"", null, null, false);
                            }
                        }
                    }
                }
                else
                {
                    if (_timeToRecord > 0)
                    {
                        if (!(_state == RecordVideoScreenStates.PreRecording || _state == RecordVideoScreenStates.Recording))
                        {
                            // erreur, on stoppe
                            log.Error("Wrong state {0} in timer sequence. Aborting.", _state);
                            StopRecording();
                        }

                        if (_state != RecordVideoScreenStates.Recording)
                        {
                            if (_canonManager.StartRecord())
                            {
                                OnRecordStart();
                                ChangeRecordingState(RecordVideoScreenStates.Recording);
                            }
                            else
                            {
                                _timer.Stop();
                                OnVideoRecordStop(true);
                                ChangeRecordingState(RecordVideoScreenStates.StandBy);
                                this.SafeCall(() =>
                                {
                                    MessageBox.Show(_activeLg.Equals("en", StringComparison.InvariantCultureIgnoreCase) ?
                                            "Camera was unable to execute record command." :
                                            "La caméra n'a pas été capable d'exécuter la commande d'enregistrement.",
                                        @"iBrand App.", MessageBoxButtons.OK);
                                });
                            }
                        }

                        _timeToRecord--;
                    }
                    else
                    {
                        StopRecording();
                    }
                }
            }

            Refresh();
        }

        /// <summary>
        /// Exécution de code additionnel quand on démarre la prise vidéo.
        /// </summary>
        protected virtual void OnRecordStart()
        {
            // Rien par défaut
        }

        public override void Dispose()
        {
            base.Dispose();

            log.Trace("Disposing.");
            TransferImageSequence?.Dispose();
            StaticImageSequence?.Dispose();

            foreach (var seq in _enumSequences) seq.ImageSequence.Dispose();
        }

        ~RecordVideoScreen()
        {
            RecordingStateChanged -= OnRecordingStateChanged;
        }
    }
}