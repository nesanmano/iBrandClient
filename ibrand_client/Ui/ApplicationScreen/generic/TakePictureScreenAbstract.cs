using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using Canon.Eos.Framework;
using Canon.Eos.Framework.Eventing;
using JetBrains.Annotations;
using NLog;
using NomadLogic.Common;
using NomadLogic.Common.Extentions;
using NomadLogic.Common.Questionnaire;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Json;
using NomadLogic.Common.Util.Win32;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers.ImageTransforms;
using NomadLogic.iBrandDesktop.Util;
using NomadLogic.Multimedia;
using Timer = System.Timers.Timer;

#pragma warning disable 618

// ReSharper disable VirtualMemberCallInContructor

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic
{
    /// <summary>
    /// Classe de base pour les prises de photo. Contient les éléments de base pour l'affichage ainsi que les méthodes de callback pour
    /// les Événements de caméra.
    /// </summary>
    [PublicAPI]
    public abstract class TakePictureScreenAbstract : AbstractScreen
    {
        // ReSharper disable once UnusedMember.Local
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        /// Texte de base pour l'image de Freeze.
        protected static readonly Dictionary<string, string> _baseTextFreeze =
            new Dictionary<string, string> { { Constants.FR, "SOURIEZ" }, { Constants.EN, "SMILE" } };

        /// Référence vers l'image UI affichée juste avant une prise photo. à l'intérieur du panneau de prévisualisation.
        protected Bitmap _freezeBitmap;

        /// Référence vers le panneau de prévisuation. Peut être nul si on n'utilise pas la prévisualisation.
        protected PictureBox _panelPreview;

        /// Référence interne vers les images de base d'affichage, qui ne doivent pas être "garbage collected" quand on itère dans l'affichage du panneau de prévisualisation.
        protected readonly List<Image> _safeBitmaps = new List<Image>();

        /// Référence vers les définitions d'images de superpositions.
        protected List<PreviewOverlayInfo> _previewOverlayInfos = new List<PreviewOverlayInfo>();

        /// Référence vers le manager de caméra.
        protected CanonCameraManager _canonManager;

        /// Liste des fichiers pris par la classe.
        protected IList<string> _picturesTaken = new List<string>();

        /// Cette variable est une sémaphone multi-thread pour indiquer que la classe travaille en arrière plan (soit attends une photo, soit est en train
        /// de transformer quelque chose). Si mis à vrai, ne devrait pas réagir sur certains événements, comme le UI.
        protected volatile bool _backgroundWorkActive;

        private long _lastDebugImageTime;

        /// Dimensions de la zone de prévisualisation.
        private Rectangle? _panelPreviewInfo;

        private bool _loadImages;

        private readonly string _baseDir;

        /// <summary>
        /// Séquence d'images optionnelle quand on fait la transformation.
        /// </summary>
        protected AnimatedImageSequence _creatingSequence;

        private ImageListTransformOptions _imageListTransform;

        /// Bloque l'usage du "Live view de la caméra"
        public bool DisableLiveView { get; set; }

        /// <summary>
        /// Zone pour annuler le répondant.
        /// </summary>
        public Rectangle? CancelZone { get; set; }

        private Timer _canonTimer = new Timer
        {
            AutoReset = true,
            Interval = 1 * 1000
        };

        /// <summary>
        /// Indique que l'on veut être notifié par "pop-up" modal les événements où la caméra se connecte et déconnecte.
        /// </summary>
        public bool ShowCameraStatusMessages { get; set; }

        /// <summary>
        /// Indique que l'on veut cacher le symbole de la caméra active dans l'écran.
        /// </summary>
        public bool HideCameraStatusSymbol { get; set; }

        /// <summary>
        /// Endroit où l'on veut dessiner le symbole d'activité de la caméra.
        /// </summary>
        public Point? CameraStatusSymbolPoint { get; set; }

        // Indique que l'on avait perdu le nombre requis de caméra après l'initialisation intiale de l'écran.
        // Provoque l'affichage de messages à l'usager.
        private bool _cameraLost;
        private string _nextCameraMessageStatus;
        private Image _cameraSymbolActive;
        private Image _cameraSymbolInactive;
        private Point _cameraSymbolPoint;

        private protected TakePictureScreenAbstract(string backgroundImgEnSrc, string backgroundImgFrSrc, ScreenConfig sc,
            Rectangle? previewRectangle)
            : base(backgroundImgEnSrc, backgroundImgFrSrc ?? backgroundImgEnSrc, sc)
        {
            _baseDir = DirectoryUtil.GetLocalIbrandTerminalDir(sc.ProjectId, sc.QuestionnaireId, sc.TerminalId);

            _panelPreviewInfo = previewRectangle;

            if (_panelPreviewInfo != null)
            {
                PrepareFreezeBitmapDefinition();
            }
        }

        protected override bool OnMouseDownHandler(int x, int y)
        {
            if (!base.OnMouseDownHandler(x, y)) return false;
            if (_backgroundWorkActive) return false;

            if (InArea(x, y, CancelZone))
            {
                FirstScreen();
                return false;
            }

            return true;
        }

        /// <inheritdoc />
        protected override void OnLoad()
        {
            base.OnLoad();

            _canonManager = CanonCameraManager.GetInstance();
            _canonManager.FileCreatedOnCamera += OnFileCreatedOnCamera;
            _canonManager.FileSavedOnHost += OnFileSavedOnHost;
            _canonManager.CameraAdded += (sender, args) =>
            {
                var tmp = _canonManager.CameraCount();
                if (!_cameraLost || tmp <= 0) return;
                log.Warn("New camera connection.");
                _nextCameraMessageStatus = "New camera detected. You can now take pictures.";
                _cameraLost = false;
            };
            _canonManager.CameraShutdown += (sender, args) =>
            {
                var tmp = _canonManager.CameraCount();
                if (tmp != 0) return;
                log.Warn("Lost camera connection.");
                _cameraLost = true;
                _nextCameraMessageStatus = "Camera connection lost. Unable to take pictures.";
            };

            _canonTimer.Enabled = true;
            _canonTimer.Elapsed += CanonTimerOnElapsed;
            
            // On tente de charge le configuration locale du overlayPreview
            var configFilePath =
                DirectoryUtil.GetLocalIbrandTerminalDir(_screenConfig.ProjectId, _screenConfig.QuestionnaireId, _screenConfig.TerminalId) +
                new PreviewOverlayInfo().DefaultConfigFileName;
            if (File.Exists(configFilePath))
            {
                var obj = JsonUtil.ReadJsonFile<PreviewOverlayHelper>(new FileInfo(configFilePath));
                var tmp = obj?.PreviewOverlayInfoList;
                if (tmp != null && tmp.Any())
                {
                    LivePreviewOverlays = tmp;
                    if (obj.PreviewRectangle != null)
                    {
                        _panelPreviewInfo = obj.PreviewRectangle;
                        PrepareFreezeBitmapDefinition();
                    }
                }
            }

            _cameraSymbolPoint = CameraStatusSymbolPoint ?? new Point(0, Height - 50);

            InitBitmap();
            if (_panelPreviewInfo == null) return;
            var loc = _panelPreviewInfo.Value.X < 0
                ? new Point((PictureBox.Width - _panelPreviewInfo.Value.Width) / 2, _panelPreviewInfo.Value.Y)
                : _panelPreviewInfo.Value.Location;

            _panelPreview = new PictureBox
            {
                Location = loc,
                Size = _panelPreviewInfo.Value.Size,
                BackColor = Color.Black
            };

            Controls.Add(_panelPreview);
            _panelPreview.BringToFront();
            _canonManager.LiveViewUpdate += HandlePictureUpdate;
        }

        private void CanonTimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            if (!_activated) return;

            var tmpStr = _nextCameraMessageStatus;
            _nextCameraMessageStatus = null;

            if (tmpStr != null && ShowCameraStatusMessages)
            {
                Invoke(new Action(() => {
                    MessageBox.Show(this, tmpStr, Resources.iBrand.AppName,
                        MessageBoxButtons.OK, _cameraLost ? MessageBoxIcon.Error : MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1);
                }));
            }

            SetActivated();
        }

        private void PrepareFreezeBitmapDefinition()
        {
            if (_panelPreviewInfo != null)
            {
                FreezeBitmapDefinition = new ImageGeneratorFromText
                {
                    ProceduralImageTextInfo = new ProceduralImageTextInfo
                    {
                        BitmapSize = _panelPreviewInfo.Value.Size,
                        Color = Color.White,
                        BackgroundColor = Color.Black
                    },
                    Texts = _baseTextFreeze
                };
            }
        }

        /// Type de rotation à effectuer sur les images venant de la caméra, pour la prévisualisation. NULL par défaut, ce qui sera
        /// considéré comme RotateNoneFlipX. En gros, si le sujet bouge son bras gauche, on voit la côté gauche bouger à l'écran.
        public RotateFlipType? PreviewImageRotation { get; set; }

        /// Pour fin de déboguage seulement. Sauvegarde les images de prévisualisation (une image toutes les 3 secondes) dans ce répertoire si non nul. Nul par défaut.
        public string DebugImageDir { get; set; }

        /// <summary>
        /// Liste optionnelle (nul par défaut) d'une liste de fichiers à mettre par dessus les images du preview. Utilisée pour
        /// faire une pseudo-transformation avec la prévisualisation.
        /// </summary>
        public IList<PreviewOverlayInfo> LivePreviewOverlays
        {
            get => _previewOverlayInfos?.AsReadOnly();
            set
            {
                if (_previewOverlayInfos != null)
                {
                    foreach (var bmp in _previewOverlayInfos.Select(f => f.LoadedImageFile))
                    {
                        bmp?.Dispose();
                    }

                    _previewOverlayInfos.Clear();
                }
                else
                {
                    _previewOverlayInfos = new List<PreviewOverlayInfo>();
                }

                if (value != null)
                {
                    _previewOverlayInfos.AddRange(value);
                }

                _loadImages = true;
            }
        }

        /// <summary>
        /// Contient l'ensemble des options de transformations avec des fichiers d'image.
        /// Si cette variable n'est pas explicitement donnée, on tente de lire un fichier dans le répertoire du projet, qu'il existe ou non, et on met à jour
        /// la configuration selon le contenu de ce fichier.
        /// </summary>
        public ImageListTransformOptions ImageListTranformOptions
        {
            get
            {
                if (_imageListTransform != null) return _imageListTransform;

                var transform = new ImageListTransformOptions();
                JsonUtil.UpdateFromJson(
                    new FileInfo(
                        DirectoryUtil.GetLocalIbrandTerminalDir(_screenConfig.ProjectId,
                            _screenConfig.QuestionnaireId, _screenConfig.TerminalId) + transform.DefaultConfigFileName),
                    transform);
                return transform;
            }
            set => _imageListTransform = value;
        }

        /// <summary>
        /// Définition de l'image du "freeze" qui appraît quand la caméra se prépare à prendre une photo.
        /// Par défaut est générez avec un message de "souriez" blanc sur noir.
        /// Mettre à null on ne veut pas voir apparaître ce message.
        /// Ne fera rien si on n'a pas de zone de prévisualisation.
        /// </summary>
        public IImageGenerator FreezeBitmapDefinition { get; set; }

        /// <summary>
        /// Initialise les images de visualisation générales utilisée par la classe.
        /// </summary>
        protected virtual void InitBitmap()
        {
            if (_creatingSequence != null)
            {
                _creatingSequence.ClearCurrentImages();
                _creatingSequence.CreateImages(_activeLg);
            }

            if (_cameraSymbolActive == null)
            {
                var fileLoc = DirectoryUtil.GetLocalNomadLogicAssetsDir() + "green_signal.png";
                _cameraSymbolActive = FileUtil.LoadImage(fileLoc);
            }

            if (_cameraSymbolInactive == null)
            {
                var fileLoc = DirectoryUtil.GetLocalNomadLogicAssetsDir() + "red_signal.png";
                _cameraSymbolInactive = FileUtil.LoadImage(fileLoc);
            }

            CreateFreezeBmp();
        }

        private void CreateFreezeBmp()
        {
            if (FreezeBitmapDefinition == null) return;
            _safeBitmaps.Remove(_freezeBitmap);
            _freezeBitmap?.Dispose();
            _freezeBitmap = FreezeBitmapDefinition.GenerateImage(_activeLg);
            _safeBitmaps.Add(_freezeBitmap);
        }

        /// <inheritdoc />
        public override bool ChangeLanguage(string language)
        {
            var change = base.ChangeLanguage(language);
            if (!change) return false;

            CreateFreezeBmp();

            if (_creatingSequence == null) return true;

            _creatingSequence.ClearCurrentImages();
            _creatingSequence.CreateImages(_activeLg);
            return true;
        }

        /// <inheritdoc />
        public override void SetActivated()
        {
            base.SetActivated();

            if (_canonManager == null)
            {
                log.Error("No Working Camera Manager exists upon activation.");
                return;
            }

            //SuspendTimer();

            if (_canonManager.IsCameraConnected())
            {
                if (_panelPreview != null && !DisableLiveView) _canonManager.StartLiveView();

                SetPreviewPicture(_panelPreview, _safeBitmaps, null);
                _picturesTaken.Clear();

                SetActivatedCustom();
            }

            Invoke(new Action(Refresh));
}

        /// <summary>
        /// Permet aux classes enfants d'ajouter un peut de code pour leur activation, en conservant la logique de la méthode parent.
        /// Basé sur le patron de conception "Template method".
        /// </summary>
        protected abstract void SetActivatedCustom();

        /// <inheritdoc />
        public override void Recycle()
        {
            base.Recycle();
            _picturesTaken.Clear();
            SetPreviewPicture(_panelPreview, _safeBitmaps, null);
        }

        private static void OnFileCreatedOnCamera(object sender, IntPtr dirItem, EosCamera camera)
        {
            camera.SavePicture(dirItem,
                DirectoryUtil.GetLocalMediaDir(DirectoryUtil.NomadLogicMediaDirectoriesEnum.pictures) +
                GenerateUniqueId.GetId() + ".jpg");
        }

        private void HandlePictureUpdate(object sender, EosImageEventArgs e)
        {
            if (!IsDisposed && _activated)
            {
                UpdatePicture(e.GetImage(), false);
            }
        }

        private void OnFileSavedOnHost(object sender, EosFileEventArgs e)
        {
            if (!_activated) return;

            try
            {
                BitmapExtentions.MatchExifData(e.FilePath);
                OnFileSavedOnHostDo(e.FilePath);
            }
            catch (System.Exception ex)
            {
                log.Error(ex);
                MessageBox.Show(WorkingArea.Container, ex.Message, Resources.iBrand.AppName, MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Méthode d'exécution quand une image venant de la caméra est sauvegardée sur le disque.
        /// </summary>
        /// <param name="filePath">Le chemin absolu de l'image sauvegardée.</param>
        protected abstract void OnFileSavedOnHostDo(string filePath);

        /// <summary>
        /// Met à jour l'image de pré-visualisation.
        /// </summary>
        /// <param name="image">L'iamge à utiliser comme base.</param>
        /// <param name="force">Force le remplacement de l'ancienne image.</param>
        protected void UpdatePicture(Image image, bool force)
        {
            this.SafeCall(() => { OnUpdatePictureDo(image, force); });
        }

        /// <summary>
        /// Méthode d'exécution de la mise à jour de l'image de prévisualisation
        /// </summary>
        /// <param name="image">L'image à utiliser comme base.</param>
        /// <param name="force">Force le remplacement de cette image.</param>
        protected abstract void OnUpdatePictureDo(Image image, bool force);

        /// <inheritdoc />
        public override void Dispose()
        {
            base.Dispose();
            if (_canonManager.IsLiveView() && !DisableLiveView)
                _canonManager.StopLiveView();
        }

        /// <summary>
        /// Exécute les transformations sur images, incluant les sauvegardes dans des questions du questionnaire, sur un fil séparé.
        /// Active et désactive la séquence d'image de création, et passe automatiquement au prochain écran une fois complété.
        /// </summary>
        /// <param name="pictures">La liste de images à gérer.</param>
        /// <param name="ct">Jeton de cancellation optionnel.</param>
        protected void LaunchTransformationSequence(IList<string> pictures, CancellationToken? ct = null)
        {
            SetPreviewPicture(_panelPreview, _safeBitmaps, null);
            _creatingSequence?.StartSequence();

            if (ct.HasValue)
            {
                Task.Run(() => { LaunchTransformationSequenceDo(pictures); }, ct.Value);
            }
            else
            {
                Task.Run(() => { LaunchTransformationSequenceDo(pictures); });
            }
        }

        /// <summary>
        /// Sauvegarde l'image prise par la caméra, avant transformations, dans le questionnaire.
        /// </summary>
        public DataModelBase SaveOriginalImage { get; set; }

        // transforma image a utilise pour lance la transfo
        private void LaunchTransformationSequenceDo(IList<string> pictures)
        {
            System.Exception error = null;
            try
            {
                _backgroundWorkActive = true;
                if (SaveOriginalImage != null)
                {
                    var copyPath = DirectoryUtil.GetLocalIbrandTerminalDataDir(_screenConfig.ProjectId,
                                       _screenConfig.QuestionnaireId, _screenConfig.TerminalId,
                                       DirectoryUtil.ProjectDataDirectoriesEnum.pictures,
                                       DirectoryUtil.ProjectDataDirectoriesModEnum.none) + GenerateUniqueId.GetId() + ".jpg";

                    File.Copy(pictures[0], copyPath);
                    SaveOriginalImage.SaveData(GetCurrentQuestionnaire(), copyPath);
                }

                ImageListTransform.TransformImageList(this, pictures, _screenConfig, ImageListTranformOptions);
            }
            catch (System.Exception e)
            {
                log.Error(e);
                error = e;
            }
            finally
            {
                _creatingSequence?.StopSequence();
                _backgroundWorkActive = false;
            }

            if (error != null)
            {
                var result = MessageBox.Show(
                    @"There was an error during transformation. Do you wish to continue?\nMessage:\n" + error.Message,
                    @"iBrand TM", MessageBoxButtons.YesNo, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                if (result == DialogResult.Yes) Next();
            }
            else
            {
                Next();
            }
        }

        /// <summary>
        /// Lance la génération d'une image de prévisualisation selon les paramètres actuel.
        /// </summary>
        /// <param name="image">L'image de base à la prévisualisation.</param>
        protected void HandlePreviewImage(Image image)
        {
            if (DebugImageDir != null)
            {
                var now = DateTime.Now.Ticks;
                if (_lastDebugImageTime + (3 * 10000000) < now)
                {
                    _lastDebugImageTime = now;
                    var file = DebugImageDir + Path.DirectorySeparatorChar + GenerateUniqueId.GetId() + ".jpg";
                    image.Save(80, file);
                }
            }

            image.RotateFlip(PreviewImageRotation ?? RotateFlipType.RotateNoneFlipX);

            if (_previewOverlayInfos.Count == 0)
            {
                // On affiche l'image telle quelle.
                if (_panelPreview != null)
                {
                    SetPreviewPicture(_panelPreview, _safeBitmaps,
                        image.ResizeForView(_panelPreview.Width, _panelPreview.Height));
                }
            }
            else
            {
                // On effectue une légère transformation pour placer une image par-dessus

                // Initialisation paresseuse.
                if (_loadImages)
                {
                    using (var ite = _previewOverlayInfos.GetEnumerator())
                    {
                        while (ite.MoveNext())
                        {
                            var poi = ite.Current;
                            if (poi == null) continue;

                            var filePath = poi.FileName;

                            if (!File.Exists(filePath)) // Check full path
                            {
                                filePath = _baseDir + poi.FileName;
                                if (!File.Exists(filePath)) // Check relative path
                                {
                                    log.Warn("Overlay preview file does not exists. Review files: " + filePath);
                                    continue;
                                }
                                    
                            }
                            poi.LoadedImageFile = FileUtil.LoadBitmap(filePath);
                        }

                        _loadImages = false;
                    }
                }

                // Selection selon condition.
                // Une condition nulle est toujours vraie
                Bitmap bmp = null;
                PreviewOverlayInfo info = null;

                var resp = GetCurrentRespondentWithAnswers();
                if (resp != null)
                {
                    foreach (var poi in _previewOverlayInfos)
                    {
                        var condition = poi.Condition;
                        if (condition != null && !condition.Evaluate(resp)) continue;
                        bmp = poi.LoadedImageFile;
                        info = poi;
                        break;
                    }
                }

                // Appel de fonction, avec possibilite de null si on a pas trouve de condition viable
                var overlayPosition = info?.Position;
                var originalRectangle = info?.OriginalConstraint;

                if (_panelPreview != null)
                {
                    SetPreviewPicture(_panelPreview, _safeBitmaps,
                        image.ResizeForView(_panelPreview.Width, _panelPreview.Height, bmp, overlayPosition, originalRectangle));
                }
            }
        }

        /// <summary>
        /// Séquence d'image optionnelle qui joue lors de la transformation.
        /// </summary>
        public AnimatedImageSequence CreatingImageSequence
        {
            get => _creatingSequence;
            set
            {
                if (_creatingSequence != null)
                {
                    _creatingSequence.StopSequence();
                    _creatingSequence.UpdateDraw -= InvokeRefresh;
                    _creatingSequence.Dispose();
                    _creatingSequence = null;
                }

                _creatingSequence = value;
                if (_creatingSequence != null)
                {
                    _creatingSequence.UpdateDraw += InvokeRefresh;
                }
            }
        }

        protected override void OnPaintEvent(Graphics g)
        {
            base.OnPaintEvent(g);
            _creatingSequence?.Paint(g);

            ScreenUtil.DrawImage(g, _cameraLost ? _cameraSymbolInactive : _cameraSymbolActive, _cameraSymbolPoint);
        }
    }
}