using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using NLog;
using NomadLogic.Common.Extentions;
using NomadLogic.Common.Services;
using NomadLogic.Common.Ui.Dialog;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Json;
using NomadLogic.Common.Util.Print;
using NomadLogic.PrintStation.Contest;
using NomadLogic.PrintStation.Json;
using NomadLogic.PrintStation.Services;

namespace NomadLogic.PrintStation.Ui
{
    /// <summary>
    /// Classe pour afficher le résultat d'un Post à imprimer. Implique habituellement une transformation de l'image du post,
    /// qui sera de mettre le Frame en dessous de l'image.
    /// </summary>
    public partial class PictureDialog : Form
    {
        private const float IDEAL_HEIGHT_SIZE = 0.75f;
        private const float IDEAL_WIDTH_SIZE = 0.5f;
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private readonly PicturePostInfo _picture;
        private readonly PictureBox _closePictureBox;
        private readonly string _fileToPrint;
        private readonly Button _printButton;
        private readonly bool _hasPrint;
        private readonly PrintStationUser _user;

        /// <summary>
        /// Constructeur de base.
        /// </summary>
        /// <param name="user">L'usager PrintStation actuel.</param>
        /// <param name="picture">L'information relative à l'image à afficher.</param>
        public PictureDialog(PrintStationUser user, PicturePostInfo picture)
        {
            var conf = RunningFeatureConfig.GetInstance();
            InitializeComponent();

            _picture = picture;

            var border = conf.GetConfiguration<int>(FeatureConfigEnum.PictureDialogBorder);
            var border2X = border * 2;
            var pictureRotation = conf.GetConfiguration<int>(FeatureConfigEnum.PictureDialogRotation);
            var printZoneHeight = conf.GetConfiguration<int>(FeatureConfigEnum.PictureDialogPanelPrintHeight);
            var printButtonPadding = conf.GetConfiguration<int>(FeatureConfigEnum.PictureDialogPrintButtonPadding);
            var totalScreenWidth = Screen.PrimaryScreen.WorkingArea.Width;
            var totalScreenHeight = Screen.PrimaryScreen.WorkingArea.Height;
            _hasPrint = conf.GetConfiguration<bool>(FeatureConfigEnum.HasPrint);

            // Création du bouton du haut
            var printButtonFileName = UiFilesUtil.GetFileName(UiFilesUtil.PrintButton, _picture.UserId);

            if (!File.Exists(printButtonFileName))
            {
                throw new Exception("Print button image file do not exist: \n\t" + printButtonFileName);
            }
            
            var printImage = Image.FromFile(printButtonFileName);

            var candidateImageHeight = printImage.Height;
            if (printZoneHeight < candidateImageHeight + (2 * printButtonPadding))
            {
                candidateImageHeight = printZoneHeight - (2 * printButtonPadding);
            }

            _printButton = new Button
            {
                Size =
                    new Size((int)(candidateImageHeight / (printImage.Width / (float)printImage.Height)),
                        candidateImageHeight),
                Visible = true,
                Image = printImage,
                FlatStyle = FlatStyle.Flat
            };

            _printButton.FlatAppearance.BorderSize = 0;
            _printButton.MouseDown += MouseDownButtonHandler;

            // On recalcule la tailel de la zone pour la rendre le plus petit possible
            printZoneHeight = _printButton.Height + (2 * printButtonPadding);

            // On transforme l'image.
            var imageTrans = PrintStationUtil.TransformPicture(user, picture, _picture.Src, pictureRotation, out _fileToPrint);

            var ratio = imageTrans.Width / (float)imageTrans.Height;

            // On calcule la taille du contrôle à présenter selon sa taille ou un maximum dans l'écran.
            // On calcule la hauteur comme premier candidat car les images à imprimer sont souvent plus hautes que larges.
            var maxHeightCandidate = (int)(totalScreenHeight * IDEAL_HEIGHT_SIZE);
            if (maxHeightCandidate > imageTrans.Height + border2X + printZoneHeight)
            {
                maxHeightCandidate = imageTrans.Height + border2X + printZoneHeight;
            }

            var maxWidthCandidate = (int)((maxHeightCandidate - border2X - printZoneHeight) * ratio) + border2X;
            if (maxWidthCandidate > totalScreenWidth * IDEAL_WIDTH_SIZE)
            {
                // Image trop large, on recalcule la largeur maximale et le candidat de hauteur ensuite.
                maxWidthCandidate = (int)(totalScreenWidth * IDEAL_WIDTH_SIZE);
                maxHeightCandidate = (int)((maxWidthCandidate - border2X) / ratio) + border2X + printZoneHeight;
            }

            Size = new Size(maxWidthCandidate, maxHeightCandidate);
            Location = new Point((totalScreenWidth - maxWidthCandidate) / 2, (totalScreenHeight - maxHeightCandidate) / 2);

            picture_box.Width = maxWidthCandidate - border2X;
            picture_box.Height = maxHeightCandidate - border2X - printZoneHeight;
            picture_box.Location = new Point(border, printZoneHeight + border);

            // Resize de l'image au besoin.
            var tempBitmap = imageTrans.Resize(picture_box.Width, picture_box.Height);
            imageTrans.Dispose();
            imageTrans = tempBitmap;

            picture_box.Image = imageTrans;

            // On place le bouton d'impression maintenant que l'on a la taille finale de la forme.
            _printButton.Location = new Point((maxWidthCandidate - _printButton.Width) / 2, printButtonPadding);
            Controls.Add(_printButton);
            _printButton.BringToFront();

            // Création et placement du bouton de fermeture.
            var closeFileName = UiFilesUtil.GetFileName(UiFilesUtil.CloseButton, _picture.UserId);

            if (!File.Exists(closeFileName))
            {
                throw new Exception("Close button image file does not exists: " + closeFileName);
            }

            var closeBitmap = Image.FromFile(closeFileName);

            // L'image de close ne peut pas être plus grosse que le bouton d'impression.
            var closeHeight = closeBitmap.Height > printImage.Height ? printImage.Height : closeBitmap.Height;

            _closePictureBox = new PictureBox
            {
                Image = closeBitmap,
                Width = closeHeight * (closeBitmap.Width / closeBitmap.Height),
                Height = closeHeight
            };
            _closePictureBox.Location = new Point((maxWidthCandidate - _closePictureBox.Width) - printButtonPadding, printButtonPadding);
            Controls.Add(_closePictureBox);
            _closePictureBox.BringToFront();

            _closePictureBox.MouseDown += MouseDownButtonHandler;
            _user = user;
        }

        /// <summary>
        /// Méthode déléguée pour les événements souris.
        /// </summary>
        /// <param name="sender">L'origine de l'événement.</param>
        /// <param name="e">Les paramètres de l'événement.</param>
        public void MouseDownButtonHandler(object sender, MouseEventArgs e)
        {
            if (sender.Equals(_printButton))
            {
                if (RunningFeatureConfig.GetInstance().GetConfiguration<bool>(FeatureConfigEnum.HasContest))
                {
                    var contest = ContestUtil.GetInstance();

                    // init Params
                    var contestParamsFile = UiFilesUtil.GetFileName(UiFilesUtil.ContestParametersFile, _user.Id);
                    if (contestParamsFile == null)
                    {
                        log.Error($"No contest Ui File found for feature id={_user.FeatureId}, id name={UiFilesUtil.ContestParametersFile}");
                        MsgBox.Show(this, "No contest file. Check contest parameters", "Nomad Logic");
                    }
                    else
                    {
                        contest.Parameters = JsonUtil.ReadJsonFile<ContestParameters>(new FileInfo(contestParamsFile));

                        // Roll for winner
                        contest.NewEvent(_user, DateTime.Now);

                        if (contest.IsLastEventWinner())
                        {
                            new PopUpWinner(_picture.UserId, contest.Parameters).ShowDialog();
                        }
                    }
                }
                
                if (_hasPrint) Print();

                CloseDlg();
            }
            else if (sender.Equals(_closePictureBox))
            {
                CloseDlg();
            }

        }

        private void CloseDlg()
        {
            picture_box.Dispose();
            DialogResult = DialogResult.OK;

            Close();
        }

        /// <summary>
        /// Envoi l'image vers l'impression système et journalise le fait qu'une impression de ce Post à été demandé.
        /// </summary>
        public void Print()
        {
            Cursor.Current = Cursors.WaitCursor;
            string defaultPrinterName = null;
            try
            {
                defaultPrinterName = PrintUtil.GetCurrentDefaultPrinterName();

                var database = ServicesContainer.GetService<PrintStationDatabase>();

                if (_picture.ServerPostId != -1)
                    database.AddPrintCount(_picture.ServerPostId);
                else
                    database.AddPrintCountSocialCam(database.GetActiveUser().EventId);

                if (PrintUtil.Print(_fileToPrint, null, this))
                {
                    NewPrintEvent.GetInstance().NewItemEvent.Set();
                }
            }
            catch (Exception e)
            {
                log.Error(e, "Could not print image {0} to following printer: {1}", _fileToPrint, defaultPrinterName);
            }

            Cursor.Current = Cursors.Default;
        }
    }
}