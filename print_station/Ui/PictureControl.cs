using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using NLog;
using NomadLogic.Common.Extentions;
using NomadLogic.Common.Util;
using NomadLogic.PrintStation.Json;

namespace NomadLogic.PrintStation.Ui
{
    /// <summary>
    /// 
    /// </summary>
    public partial class PictureControl : UserControl
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        private readonly PicturePostInfo _picture;
        private readonly PrintStationUser _user;
        public PictureDialogCloseEvent OnDialogClosed;
        public EventHandler OnPictureClick;

        public PictureControl(PrintStationUser user, PicturePostInfo picture, int blockWidth, int blockHeight)
        {
            InitializeComponent();

            _user = user;
            _picture = picture;

            Initialize(picture, blockWidth, blockHeight);

            MouseDown += MouseDownHandler;
            picture_box.MouseDown += MouseDownHandler;
        }

        private void Initialize(PicturePostInfo picture, int blockWidth, int blockHeight)
        {
            picture_box.Location = new Point(0, 0);
            picture_box.Size = new Size(blockWidth, blockHeight);

            if (File.Exists(picture.Src))
            {
                var tbnDir = new DirectoryInfo(DirectoryUtil.GetLocalPrintstationMediaDir(_user.Id, DirectoryUtil.NomadLogicPrintstationDirectoriesEnum.thumbnails));
                var conf = RunningFeatureConfig.GetInstance();
                var viewMode = conf.GetConfiguration<int>(FeatureConfigEnum.GalleryImageViewMode);
                var bkgColorStr = conf.GetConfiguration<string>(FeatureConfigEnum.BackgroundColor);

                Color? bkgColor = null;
                if (bkgColorStr != null)
                {
                    if (!bkgColorStr.StartsWith("#"))
                    {
                        bkgColorStr = $"#{bkgColorStr}";
                    }
                    bkgColor = ColorTranslator.FromHtml(bkgColorStr);
                }

                var bitmap = PrintStationUtil.GetPresentationPictureFromFile(picture.Src, picture_box.Size, tbnDir, viewMode != 2, bkgColor);
                
                // Trois possibilités selon la valeur de la configuration.
                switch (viewMode)
                {
                    case 2:
                    {
                        var newBitmap = bitmap.Frame(blockWidth, blockHeight);
                        bitmap.Dispose();
                        bitmap = newBitmap;

                        picture_box.SizeMode = PictureBoxSizeMode.CenterImage;
                    }
                        break;
                    case 1:
                    {
                        var blockRatio = blockWidth/(float)blockHeight;
                        var imageRatio = bitmap.Width/(float)bitmap.Height;

                        int newWidth;
                        int newHeight;

                        if (blockRatio > imageRatio) // Plus étendu sur la largeur
                        {
                            newHeight = blockHeight;
                            newWidth = (int)((newHeight/(float) bitmap.Height) * bitmap.Width);
                        }
                        else if (blockRatio < imageRatio)
                        {
                            newWidth = blockWidth;
                            newHeight = (int)((newWidth / (float)bitmap.Width) * bitmap.Height);
                        }
                        else //  ratio égal
                        {
                            newHeight = blockHeight;
                            newWidth = blockWidth;
                        }

                        var newBitmap = bitmap.Resize(newWidth, newHeight);
                        bitmap.Dispose();
                        bitmap = newBitmap;

                        picture_box.SizeMode = PictureBoxSizeMode.CenterImage;
                    }
                        break;
                    default:
                        picture_box.SizeMode = PictureBoxSizeMode.StretchImage;
                        break;
                }

                picture_box.Image = bitmap;
            }

            Size = new Size(blockWidth, blockHeight);
        }

        /// <summary>
        /// Méthode pour agir sur un événement souris.
        /// </summary>
        /// <param name="sender">Envoyeur</param>
        /// <param name="e">Paramètres</param>
        public void MouseDownHandler(object sender, MouseEventArgs e)
        {
            OnPictureClick?.Invoke(this, EventArgs.Empty);

            if (RunningFeatureConfig.GetInstance().GetConfiguration<bool>(FeatureConfigEnum.HasQuestionnaire))
            {
                MainForm.GetInstance().ShowQuestionnaire(_picture);
            }
            else
            {
                new PictureDialog(_user, _picture).ShowDialog();
            }
            
            OnDialogClosed?.Invoke(_picture.PostId);
        }
    }
}