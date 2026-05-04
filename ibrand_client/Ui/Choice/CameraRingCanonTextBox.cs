using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Ionic.Zip;
using JetBrains.Annotations;
using NLog;
using NomadLogic.Common.Ui;
using NomadLogic.Common.Ui.Dialog;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Networking;
using NomadLogic.Common.Util.Win32;
using NomadLogic.Multimedia.Gpu.OpenCV;

namespace NomadLogic.iBrandDesktop.Ui.Choice
{


    public partial class CameraRingCanonTextBox : UserControl
    {
        protected enum CameraRingStatus
        {
            Waiting,
            Error,
            Good
        }

        // ReSharper disable once UnusedMember.Local
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        protected readonly object _lock = new object();
        private string _dirName;
        public override string Text => _dirName;

        private readonly Dictionary<string, string> _authorDict = new Dictionary<string, string>();
        private readonly List<string> _archivedMessages = new List<string>();
        private DirectoryInfo _tmpDirName;
        private SsdpUtil.CompleteFileReceptionProvider? _serverListeners;
        /// 0=Red, 1=Green, 2=Blue
        private readonly Bitmap[] _statusPictures = new Bitmap[3];


        public event Action PictureFileReady;

        public CameraRingCanonTextBox()
        {
            InitializeComponent();

            nbr_picture_label.Text = @"0";

            preview_button.Enabled = false;
            preview_button.Click += OnPreviewButtonClick;

            clear_button.Click += OnClearButtonClick;

            picStatus.Click += OnPicStatusClick;
            picStatus.SizeMode = PictureBoxSizeMode.StretchImage;
        }

        /// <inheritdoc />
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            InitStatusPictures();

            preview_button.Text = Resources.iBrand.UI_CameraRingChoice_Label_Preview;
            clear_button.Text = Resources.iBrand.UI_CameraRingChoice_Label_Clear;
            lblPictures.Text = Resources.iBrand.UI_CameraRingChoice_Label_Pictures;

            _tmpDirName = new DirectoryInfo(DirectoryUtil.GetLocalApplicationWorkDir() + GenerateUniqueId.GetId());
            _tmpDirName.Create();
            _serverListeners = SsdpUtil.PrepareAndLaunchCameraRingImageListener(_tmpDirName, OnListenFileReception);
        }

        private void InitStatusPictures()
        {
            if (_statusPictures[0] != null) return;
            _statusPictures[0] = FileUtil.LoadBitmap(DirectoryUtil.GetLocalNomadLogicAssetsDir() + "red_signal.png");
            _statusPictures[1] = FileUtil.LoadBitmap(DirectoryUtil.GetLocalNomadLogicAssetsDir() + "green_signal.png");
            _statusPictures[2] = FileUtil.LoadBitmap(DirectoryUtil.GetLocalNomadLogicAssetsDir() + "blue_signal.png");

            if (_statusPictures.Contains(null)) throw new System.Exception("Status pictures are missing. Check Common assets directory.");
        }

        /// <summary>
        /// Cette méthode est appellée à chaque réception de fichier. On enveloppe son traitement dans une nouvelle tâche pour
        /// ne pas empêche le fil d'exécution de réception de traiter d'autres demandes.
        /// </summary>
        /// <param name="zipFile"></param>
        private void OnListenFileReception(string zipFile)
        {
            Task.Factory.StartNew(() =>
            {
                if (!zipFile.EndsWith(".zip", StringComparison.InvariantCultureIgnoreCase) && !File.Exists(zipFile))
                {
                    var tmp = Resources.iBrand.UI_CameraRingChoice_InvalidIncomingZipFile + zipFile;
                    log.Warn(tmp);
                    UpdateStatusLabel(tmp, CameraRingStatus.Error);
                    return;
                }

                {
                    var tmpDirName = DirectoryUtil.GetLocalApplicationWorkDir() + GenerateUniqueId.GetId();

                    try
                    {
                        if (Directory.Exists(tmpDirName))
                        {
                            Directory.Delete(tmpDirName, true);
                        }

                        log.Trace("Receive new zip file. Extracting to {0}", tmpDirName);

                        using (var zip = ZipFile.Read(zipFile))
                        {

                            foreach (var e in zip)
                            {
                                e.Extract(tmpDirName);
                            }
                        }
                    }
                    catch (System.Exception e)
                    {
                        var tmp = Resources.iBrand.UI_CameraRingChoice_CouldNotExtractFilesFromZip + zipFile;
                        log.Error(e, tmp);
                        UpdateStatusLabel(tmp, CameraRingStatus.Error);
                        try
                        {
                            if (Directory.Exists(tmpDirName))
                                Directory.Delete(tmpDirName, true);

                            File.Delete(zipFile);
                        }
                        catch (System.Exception ex)
                        {
                            log.Error(ex, "Could not Deleting file or temporary directory.");
                        }
                        return;
                    }

                    // Étape deux, on traite les fichiers individuellement
                    var allGood = Directory.EnumerateFiles(tmpDirName).Aggregate(true, (current, fileFromZip) => current & OnFileReception(fileFromZip));

                    if (!allGood)
                    {
                        UpdateStatusLabel(Resources.iBrand.UI_CameraRingChoice_FileIncorrectlyReceived, CameraRingStatus.Error);
                    }
                    else if (CurrentPicturesCount >= (PictureNumberToBeValid?.Invoke() ?? int.MaxValue) &&
                        CurrentCameraCount >= (CameraNumberToBeValid?.Invoke() ?? int.MaxValue))
                    {
                        UpdateStatusLabel(Resources.iBrand.UI_CameraRingChoice_EnoughFilesReceived, CameraRingStatus.Good);

                        // On appel le next seulement si le delegate n,est pas null
                        PictureFileReady?.Invoke();

                    }
                    else
                    {
                        UpdateStatusLabel(null, CameraRingStatus.Waiting);
                    }

                    // Le répertoire de travail du ZIP est maintenant vide. On peut le détruire
                    FileUtil.DeleteFolderContent(tmpDirName);
                }
            }, TaskCreationOptions.LongRunning);
        }

        /// <summary>
        /// Le port  où l'on écoute pour des envois de fichiers. Si non actif ou initialisé, on retourne zéro.
        /// </summary>
        [PublicAPI]
        public int ListenTaskPort => _serverListeners?.TcpFileListenerService.Port ?? 0;

        /// <summary>
        /// Indique combien de caméras ont envoyé des photos.
        /// </summary>
        public virtual int CurrentCameraCount => CurrentPicturesCount;

        /// <summary>
        /// Indique combien de fichiers ont étés envoyés en tout.
        /// </summary>
        public virtual int 
            CurrentPicturesCount => _dirName == null ? 0 : Directory.GetFiles(_dirName, "*.jpg", SearchOption.TopDirectoryOnly).Length;

        /// <summary>
        /// Accepte une fonction qui donne le nombre d'images pour que le choix se considère valide.
        /// </summary>
        public Func<int> PictureNumberToBeValid { private get; set; }

        /// <summary>
        /// Accepte une fonction qui donne le nombre de caméra pour que le choix se considère valide.
        /// </summary>
        public Func<int> CameraNumberToBeValid { private get; set; }

        /// <summary>
        /// Pour afficher un message d'erreur.
        /// </summary>
        /// <param name="text">Le message</param>
        /// <param name="error">Pour indiquer qu'il s'agit d'un message d'erreur.</param>
        [PublicAPI]
        protected void ShowMessageBox(string text, bool error)
        {
            this.SafeCall(() =>
            {
                MsgBox.Show(this, text, Resources.iBrand.AppName, MessageBoxButtons.OK,
                    error ? MessageBoxIcon.Error : MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
            });
        }

        protected void SetNbrPicture(string nbrPicture)
        {
            this.SafeCall(() => { nbr_picture_label.Text = nbrPicture; });
        }

        protected void EnablePreviewButton(bool enable)
        {
            this.SafeCall(() => { preview_button.Enabled = enable; });
        }

        private void OnPreviewButtonClick(object sender, EventArgs args)
        {
            if (_dirName == null)
            {
                MsgBox.Show(this, Resources.iBrand.UI_NoPictureFileFound, Resources.iBrand.AppName, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                return;
            }

            ShowPreviewDialog();
        }

        private void ShowPreviewDialog()
        {
            this.SafeCall(() => { new MultiplePicturesPreviewDlg(_dirName).ShowDialog(this); });
        }

        private void OnClearButtonClick(object sender, EventArgs args)
        {
            if (_dirName == null)
            {
                MsgBox.Show(this, Resources.iBrand.UI_NoPictureFileFound, Resources.iBrand.AppName, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                return;
            }

            try
            {
                lock (_lock)
                {
                    SetNbrPicture("0");
                    ClearAllPictureFiles();
                    _authorDict.Clear();
                }
            }
            catch (System.Exception ex)
            {
                MsgBox.Show(this, ex.Message, Resources.iBrand.AppName, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
            }
        }

        /// <summary>
        /// Change la taille des éléments visuels de ce choix.
        /// </summary>
        /// <param name="size">La nouvelle taille à utiliser.</param>
        public void SetSize(Size size)
        {
            Size = size;

            var y = preview_button.Location.Y;
            const int SPACER = 5;
            
            calibrateButton.Location = new Point(size.Width - calibrateButton.Width - picStatus.Width - preview_button.Width - clear_button.Width - (4 * SPACER), y);
            preview_button.Location = new Point(size.Width - picStatus.Width - preview_button.Width - clear_button.Width - (3 * SPACER), y);
            clear_button.Location = new Point(size.Width - clear_button.Width - picStatus.Width - (2 * SPACER), y);
            picStatus.Location = new Point(size.Width - picStatus.Width - (1 * SPACER), y);
        }

        /// <summary>
        /// Réinitialise la réception de fichiers.
        /// </summary>
        public void Reset()
        {
            lock (_lock)
            {
                _authorDict.Clear();
                _dirName = null;
                _archivedMessages.Clear();
                UpdateStatusLabel(Resources.iBrand.UI_CameraRingChoice_WaitingForFiles, CameraRingStatus.Waiting);
                SetNbrPicture(0 + "/" + (PictureNumberToBeValid?.Invoke().ToString() ?? "???"));
                EnablePreviewButton(false);
            }
        }

        /// <summary>
        /// Vide le répertoire de réception.
        /// </summary>
        protected virtual void ClearAllPictureFiles()
        {
            if (!Directory.Exists(_dirName)) return;
            try
            {
                FileUtil.DeleteFolderContent(_dirName);
            }
            finally
            {
                _dirName = null;
            }
        }

        /// <summary>
        /// Gère la réception d'un nouveau fichier. Dans tout les cas, le fichier passé en paramètre n'existera plus. Soit il sera déplacé
        /// sous un nouveau nom, soit il sera détruit car invalide.
        /// Le fichier doit avoir un nom qui contient deux informations d'identification plus un nom aléatoire, sous le forme suivante:
        /// "author_serialnumber_seed.jpg", où "author" est l'information d'auteur par la caméra, "serial" est son S/N unique au matériel, et
        /// "seed" est n'importe quelle valeur.
        /// Le fichier renommé ne sera que "author.jpg". Si la valeur de "author" est un nombre, il sera placé sous la forme "000.jpg", pour
        /// permettre de les trier sans problème.
        /// </summary>
        /// <param name="fileReceived">Le nouveau fichier à traiter.</param>
        /// <returns>true si le fichier à été accepté.</returns>
        protected virtual bool OnFileReception(string fileReceived)
        {
            log.Trace("Now evaluating received file {0}", fileReceived);
            try
            {
                lock (_lock)
                {
                    CreateFileBundleDirectory();

                    var ids = StringUtil.ExtractUniqueIdsFromFileName(fileReceived);

                    if (ids.Length < 2)
                    {
                        log.Warn("Rejected file because it did not contain enough identification information in it's file name: {0}",fileReceived);
                        File.Delete(fileReceived);
                        return false;
                    }

                    if (!DoCheckAuthorUnicity(ids, fileReceived))
                    {
                        log.Trace("Not enough identifiers in file name. Rejected. name={0}", StringUtil.GetFileName(fileReceived));
                        return false;
                    }

                    DoCreateNewValidImageFileFromTemporaryFile(ids, fileReceived);

                    var nbrFiles = CurrentPicturesCount;
                    SetNbrPicture(nbrFiles + "/" + (PictureNumberToBeValid?.Invoke().ToString() ?? "???"));
                    EnablePreviewButton(nbrFiles > 0);
                }

            }
            catch (System.Exception ex)
            {
                var tmp = Resources.iBrand.UI_CameraRingChoice_ErrorDuringFileAnalisys + fileReceived;
                log.Error(ex, tmp);
                if (File.Exists(fileReceived))
                    File.Delete(fileReceived);
                UpdateStatusLabel(tmp, CameraRingStatus.Error);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Fait un transfert du fichier intrant dans le répertoire de réception des images valides, avec un nouveau nom
        /// représentant le code d'auteur. Comme il s'agit d'un transfert, le fichier original n'existera plus.
        /// Après le code d'auteur, on peut aussi ajouter des identifiants supplémentaires dans le nom de fichier.
        /// </summary>
        /// <param name="ids">Les identifiants du fichier original.</param>
        /// <param name="fileReceived">Le fichier reçu.</param>
        /// <param name="extraIdsToAdd">Identifiant optionnels supplémentaire à introduire dans le nom de fichier.</param>
        /// <returns>Le chemin du nouveau fichier valide, final.</returns>
        protected string DoCreateNewValidImageFileFromTemporaryFile(string[] ids, string fileReceived, params string[] extraIdsToAdd)
        {
            var authorValue = ids[0];

            if (int.TryParse(authorValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out var numberAuthor))
            {
                authorValue = FormatIntToFixedSize(numberAuthor);
            }

            var sb = new StringBuilder();
            sb.Append(_dirName);
            sb.Append(authorValue);
            foreach (var id in extraIdsToAdd)
            {
                sb.Append("_");
                sb.Append(id);
            }

            sb.Append(".jpg");

            var newFileName = sb.ToString();

            if (File.Exists(newFileName))
                File.Delete(newFileName);

            File.Move(fileReceived, newFileName);

            log.Trace("New file created for camera ring from incoming transfert: {0}", StringUtil.GetFileName(newFileName));

            return newFileName;
        }

        /// <summary>
        /// Prend un nombre entier en fait une chapine de caractère à taille fixe (met des zéros pour "padder") sur trois caractères.
        /// Ne prend pas de nombre au dessus de mille.
        /// </summary>
        /// <param name="number">La nombre à formatter.</param>
        /// <returns>Une chaîne représentant ce nombre.</returns>
        protected static string FormatIntToFixedSize(int number)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0:000}", number);
        }

        /// <summary>
        /// Vérifie si l'auteur est unique dans les fichiers reçus.
        /// </summary>
        /// <param name="ids">Les identifiants du fichier.</param>
        /// <param name="fileReceived">Le fichier reçu.</param>
        /// <returns>FALSE si l'auteur n'est pas unique. Le fichier sera détruit aussi si false.</returns>
        protected bool DoCheckAuthorUnicity(string[] ids, string fileReceived)
        {
            if (!_authorDict.TryGetValue(ids[0], out var knownSerial))
            {
                _authorDict.Add(ids[0], ids[1]);
            }
            else
            {
                if (knownSerial.Equals(ids[1])) return true;

                // Duplicata d'auteurs!
                log.Warn("Duplicate author information sent. Author already registered ({1}). Rejecting file: {0}", fileReceived, ids[0]);
                UpdateStatusLabel(Resources.iBrand.UI_CameraRingChoice_DuplicateAuthorDetected, CameraRingStatus.Error);
                File.Delete(fileReceived);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Met à jour le label de message avec un nouveau message, et archive le message d'erreur pour visualisation ultérieure.
        /// </summary>
        /// <param name="message">La message à afficher.</param>
        /// <param name="status">Le status à afficher.</param>
        protected void UpdateStatusLabel(string message, CameraRingStatus status)
        {
            var picIndex = 2; // 2 (bleu) par défaut.
            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (status)
            {
                case CameraRingStatus.Error:
                    picIndex = 0;
                    break;
                case CameraRingStatus.Good:
                    picIndex = 1;
                    break;
            }
            this.SafeCall(() =>
            {
                InitStatusPictures();
                picStatus.Image = _statusPictures[picIndex];
                Invalidate();
            });

            // Maintenant journalise le message, s'il y en a un.
            if (message == null) return;
            _archivedMessages.Add(DateTime.Now.ToShortTimeString() + "\t" + message);
            log.Debug(message);
        }

        /// <summary>
        /// Créer un répertoire temporaire où stoker les fichiers de ce répondant pour une question de CameraRing
        /// </summary>
        protected void CreateFileBundleDirectory()
        {
            if (_dirName != null) return;
            var picturesDir = GenerateUniqueId.GetId();

            _dirName =
                DirectoryUtil.GetLocalMediaDir(DirectoryUtil.NomadLogicMediaDirectoriesEnum.pictures) +
                picturesDir + Path.DirectorySeparatorChar;

            Directory.CreateDirectory(_dirName);

            // S'assure que si on a eu un répertoire existant (très très rare), les fichiers existants seront détruits.
            var files = Directory.EnumerateFiles(_dirName);
            foreach (var file in files)
            {
                File.Delete(file);
            }

            log.Trace("Created directory for cameraRing answer: {0}", _dirName);
        }

        /// <summary>
        /// Affiche un dialogue avec l'ensemble des messages archivé pour cette session.
        /// </summary>
        /// <param name="sender">Pas utilisé.</param>
        /// <param name="e">Pas utilisé.</param>
        private void OnPicStatusClick(object sender, EventArgs e)
        {
            var sb = new StringBuilder();

            // On met le status du server TCP ici, ainsi que son adresse.
            sb.Append(Resources.iBrand.UI_CameraRingChoice_ServerStatusString_01);
            sb.Append(_serverListeners == null ? Resources.iBrand.UI_CameraRingChoice_ServerStatusString_02 : 
                (!_serverListeners.Value.TcpFileListenerService.IsRunning ? Resources.iBrand.UI_CameraRingChoice_ServerStatusString_03 : 
                string.Format(Resources.iBrand.UI_CameraRingChoice_ServerStatusString_04, NetUtil.GetLocalIpAddress(), _serverListeners.Value.TcpFileListenerService.Port)));

            foreach (var message in _archivedMessages)
            {
                if (sb.Length != 0)
                {
                    sb.Append('\n');
                }
                sb.Append(message);
            }
            MessageBox.Show(sb.ToString(), Resources.iBrand.UI_CameraRingChoice_ShowRecentErrorMessages, MessageBoxButtons.OK, MessageBoxIcon.None);
            _archivedMessages.Clear();
        }

        private void calibrateButton_Click(object sender, EventArgs e)
        {
            this.SafeCall(() => { new ImageBundleCalibrationPickerDialog(_dirName, false).ShowDialog(this); });
        }
    }
}
