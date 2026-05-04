using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using JetBrains.Annotations;
using NLog;
using NomadLogic.Common.Extentions;
using NomadLogic.Common.Services;
using NomadLogic.Common.Services.Configuration;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Json;
using NomadLogic.Multimedia;
using NomadLogic.Multimedia.Cli.ImageMagickCLI;
using NomadLogic.PrintStation.Contest;
using NomadLogic.PrintStation.Json;
using NomadLogic.PrintStation.Services;

namespace NomadLogic.PrintStation
{
    [PublicAPI]
    internal class PrintStationUtil
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        private PrintStationUtil()
        {
            // Classe statique.
        }

        /// <summary>
        /// Construit une image de la photo du répondant par dessus le cadre donnée par la configuration "UiFilesUtil.PHOTO_FRAME".
        /// Version anglaise.
        /// </summary>
        /// <param name="pictureSrc">L'image à utiliser comme base.</param>
        /// <param name="picture">Informations sur l'image et son Post.</param>
        /// <param name="saveFile">Le fichier final dans lequel écrire le résultat, habituellement à fin d'impression.</param>
        /// <returns>Le bitmap créé.</returns>
        public static Bitmap CreateFramedPhoto([NotNull] string pictureSrc, [NotNull]  PicturePostInfo picture, [NotNull]  string saveFile)
        {
            var frame = UiFilesUtil.GetFileName(UiFilesUtil.PhotoFrame, picture.UserId);
            return CreateFramedPhoto(pictureSrc, picture, saveFile, frame);
        }

        /// <summary>
        /// Construit une image de la photo du répondant par dessus le cadre donnée par la configuration "UiFilesUtil.PHOTO_FRAME".
        /// Version française.
        /// </summary>
        /// <param name="pictureSrc">L'image à utiliser comme base.</param>
        /// <param name="picture">Information sur l'image.</param>
        /// <param name="saveFile">Le fichier final dans lequel écrire le résultat, habituellement à fin d'impression.</param>
        /// <returns>Le bitmap créé.</returns>
        public static Bitmap CreateFramedPhotoFr([NotNull] string pictureSrc, [NotNull]  PicturePostInfo picture, [NotNull]  string saveFile)
        {
            var frame = UiFilesUtil.GetFileName(UiFilesUtil.PhotoFrameFr, picture.UserId);
            return CreateFramedPhoto(pictureSrc, picture, saveFile, frame);
        }

        /// <summary>
        /// Construit une image de la photo du répondant par dessus le cadre précis.
        /// </summary>
        /// <param name="image">L'image à utiliser comme base.</param>
        /// <param name="picture">Information sur l'image.</param>
        /// <param name="saveFile">Le fichier final dans lequel écrire le résultat, habituellement à fin d'impression.</param>
        /// <param name="frameFile">Le fichier du fond.</param>
        /// <returns>Le bitmap créé.</returns>
        public static Bitmap CreateFramedPhoto([NotNull] string image, [NotNull]  PicturePostInfo picture, [NotNull]  string saveFile, [NotNull]  string frameFile)
        {
            return CreateFramedPhoto(new List<string> { image }, picture, saveFile, frameFile);
        }

        /// <summary>
        /// Construit le cadre avec une multitude de fichier d'images.
        /// </summary>
        /// <param name="images">Les images à mettre sur le frame.</param>
        /// <param name="picture">L'information sur l'image.</param>
        /// <param name="saveFile">Le fichier final dans lequel écrire le résultat, habituellement à fin d'impression.</param>
        /// <param name="frameFile">Le fichier du fond.</param>
        /// <returns>Le bitmap créé.</returns>
        public static Bitmap CreateFramedPhoto([NotNull] List<string> images, [NotNull]  PicturePostInfo picture, [NotNull]  string saveFile, [NotNull]  string frameFile)
        {
            var runningConfig = RunningFeatureConfig.GetInstance();
            var configuration = ServicesContainer.GetService<NomadConfiguration>();

            if (!File.Exists(frameFile))
            {
                throw new FileNotFoundException(frameFile);
            }

            var frameBitmap = FileUtil.LoadBitmap(frameFile);

            var offsetPerc = runningConfig.GetConfiguration<float>(FeatureConfigEnum.PicturePadding);
            var baseOffset = (int)(Math.Min(frameBitmap.Width, frameBitmap.Height) * offsetPerc);
            var topOffsetExtra = runningConfig.GetConfiguration<int>(FeatureConfigEnum.PictureOffsetTop);
            var sideOffsetExtra = runningConfig.GetConfiguration<int>(FeatureConfigEnum.PictureOffsetSide) + configuration.PrintPictureOffsetX;
            var frameOverPictures = runningConfig.GetConfiguration<bool>(FeatureConfigEnum.PictureFrameOver);
            var cropImagesToSquare = runningConfig.GetConfiguration<bool>(FeatureConfigEnum.PictureCropSquare);

            Color? bkgColor = null;
            {
                var colorValues = runningConfig.GetConfiguration<string>(FeatureConfigEnum.PictureBackgroundColor);
                if (colorValues != null)
                {
                    try
                    {
                        bkgColor = StringUtil.ColorFromStringDefinition(colorValues);
                    }
                    catch (Exception e)
                    {
                        log.Warn(e, "Could not create background color from configs from server: PICTURE_BKG_COLOR={0}", colorValues);
                    }
                }
            }

            Rectangle? absoluteLocation = null;
            {
                var rectValues = runningConfig.GetConfiguration<string>(FeatureConfigEnum.PictureAbsolutePosition);
                if (rectValues != null)
                {
                    try
                    {
                        absoluteLocation = StringUtil.RectangleFromStringDefinition(rectValues);
                    }
                    catch (Exception e)
                    {
                        log.Warn(e, "Could not create absolute position from configs from server: PICTURE_ABSOLUTE={0}", rectValues);
                    }
                }
            }

            Rectangle? absoluteLocationProfilePicture = null;
            {
                var rectValues = runningConfig.GetConfiguration<string>(FeatureConfigEnum.PictureSocialUserImageAbsolutePosition);
                if (rectValues != null)
                {
                    try
                    {
                        absoluteLocationProfilePicture = StringUtil.RectangleFromStringDefinition(rectValues);
                    }
                    catch (Exception e)
                    {
                        log.Warn(e, "Could not create absolute position for social user picture from configs from server: PICTURE_SOCIAL_IMAGE_ABSOLUTE_POSITION={0}", rectValues);
                    }
                }
            }

            Point? absoluteLocationProfileUserName = null;
            {
                var pointValues = runningConfig.GetConfiguration<string>(FeatureConfigEnum.PictureSocialUserNameAbsolutePoint);
                if (pointValues != null)
                {
                    try
                    {
                        absoluteLocationProfileUserName = StringUtil.PointFromStringDefinition(pointValues);
                    }
                    catch (Exception e)
                    {
                        log.Warn(e, "Could not create absolute point for social user name from configs from server: PICTURE_SOCIAL_NAME_ABSOLUTE_POSITION={0}", pointValues);
                    }
                }
            }

            FontDefinitionInfo socialUserNameFont = null;
            {
                var fontDef = runningConfig.GetConfiguration<string>(FeatureConfigEnum.PictureSocialUserNameFontJson);
                if (fontDef != null)
                {
                    Exception error = null;
                    try
                    {
                        socialUserNameFont = new FontDefinitionInfo();
                        if (!JsonUtil.UpdateFromJson(fontDef, socialUserNameFont))
                        {
                            error = new Exception("JSON read failed");
                        }
                    }
                    catch (Exception e) { error = e; }

                    if (error != null)
                    {
                        socialUserNameFont = null;
                        log.Warn(error, "Could not create Font for social user name from configs from server: PICTURE_SOCIAL_NAME_FONT_JSON={0}", fontDef);
                    }
                }
            }

            var nbrCol = (int)Math.Ceiling(Math.Sqrt(images.Count));

            var maxBlockLenght = (int)((Math.Min(frameBitmap.Width, frameBitmap.Height) - (float)((nbrCol + 1) * baseOffset)) / nbrCol);
            var blockSize = absoluteLocation?.Size ?? new Size(maxBlockLenght, maxBlockLenght);

            var modFiles = new List<string>();

            Bitmap resultBitmap = null;

            try
            {
                // Création des images modifiées.
                foreach (var inFile in images)
                {
                    Bitmap imageOriginale = null;

                    try
                    {
                        imageOriginale = FileUtil.LoadBitmap(inFile);
                        
                        if (cropImagesToSquare && imageOriginale.Width != imageOriginale.Height)
                        {
                            var b = imageOriginale.ResizeToSquare();
                            imageOriginale.Dispose();
                            imageOriginale = b;
                        }

                        // RotateToNewSize picture if needed, and constraint to multiples of 90 below 360 (0, 90, 180, 270)
                        var rotation = runningConfig.GetConfiguration<int>(FeatureConfigEnum.PictureRotation);

                        rotation += 360;
                        rotation = rotation % 360;
                        if (rotation >= 270) { rotation = 270; }
                        else if (rotation >= 180) { rotation = 180; }
                        else if (rotation >= 90) { rotation = 90; }
                        else if (rotation > 0) { rotation = 0; }

                        Bitmap temp;
                        if (rotation != 0)
                        {
                            temp = imageOriginale.RotateToNewSize(rotation);
                            imageOriginale.Dispose();
                            imageOriginale = temp;
                        }

                        // Resize to block size
                        if (!blockSize.Equals(imageOriginale.Size))
                        {
                            temp = imageOriginale.Resize(blockSize.Width, blockSize.Height, PixelFormat.Format32bppArgb);
                            imageOriginale.Dispose();
                            imageOriginale = temp;
                        }

                        // See if we have a picture-only overlay.
                        var overlayFile = runningConfig.GetConfiguration<string>(FeatureConfigEnum.PictureOverlay);
                        // On tente de trouver le fichier d'overlay
                        if (overlayFile != null)
                        {
                            var overlayFilePath = DirectoryUtil.GetLocalPrintstationMediaDir(picture.UserId,
                                                                                             DirectoryUtil.NomadLogicPrintstationDirectoriesEnum.ui) + overlayFile;
                            if (!File.Exists(overlayFilePath))
                            {
                                log.Warn(
                                         "Overlay file found in feature config does not exists in user media files: {0}. Will ignore overlay attempt.",
                                         overlayFile);
                            }
                            else
                            {
                                // On applique le overlay
                                var overlayImage = FileUtil.LoadBitmap(overlayFilePath);

                                if (overlayImage.Width != imageOriginale.Width || overlayImage.Height != imageOriginale.Height)
                                {
                                    var tmp = overlayImage.Resize(imageOriginale.Width, imageOriginale.Height);
                                    overlayImage.Dispose();
                                    overlayImage = tmp;
                                }

                                imageOriginale.AddOverlay(overlayImage);

                                overlayImage.Dispose();
                            }
                        }

                        var tmpFilePath = DirectoryUtil.CreateLocalTemporaryFile(GenerateUniqueId.GetId() + ".png");
                        imageOriginale.Save(ImageFormat.Png, 9, tmpFilePath);
                        modFiles.Add(tmpFilePath);
                    }
                    catch (Exception e)
                    {
                        log.Error(e);
                        frameBitmap.Dispose();
                        throw;
                    }
                    finally { imageOriginale?.Dispose(); }
                }

                // Composite the images
                if (modFiles.Count == 1 && absoluteLocation.HasValue)
                {
                    using (var modFile = FileUtil.LoadBitmap(modFiles[0]))
                    {
                        resultBitmap = BitmapExtentions.CreateFramedPicture(frameBitmap, modFile, absoluteLocation.Value.X, absoluteLocation.Value.Y, frameOverPictures, bkgColor);
                    }
                }
                else
                {
                    resultBitmap = frameBitmap.PutMultiplePictureOnFrame(modFiles, offsetPerc, nbrCol, topOffsetExtra, sideOffsetExtra, frameOverPictures, bkgColor);
                }

                #region Image de profil et texte de nom d'usager du service social.
                // On place maintenant l'image et/ou le texte du nom d'usager si l'image, si ces options sont choisies.
                if (absoluteLocationProfilePicture != null)
                {
                    var profilePicturePath = picture.ProfilPictureSrc;
                    if (File.Exists(profilePicturePath)) // Il se peut que l'on n'ait pas d'image de profil.
                    {
                        var profilBmp = FileUtil.LoadBitmap(profilePicturePath);
                        if (!profilBmp.Size.Equals(absoluteLocationProfilePicture.Value.Size)) // Resize, si différent, avec Crop maximisé.
                        {
                            var tmp = profilBmp.Frame(absoluteLocationProfilePicture.Value.Width, absoluteLocationProfilePicture.Value.Height);
                            profilBmp.Dispose();
                            profilBmp = tmp;
                        }

                        resultBitmap.AddOverlay(profilBmp, absoluteLocationProfilePicture.Value.X, absoluteLocationProfilePicture.Value.Y);
                        profilBmp.Dispose();
                    }
                }

                // Représente plus un rectangle où l'on va dessiner le nom via un Brush de Texte qu'une image à proprement parler.
                if (absoluteLocationProfileUserName != null && !string.IsNullOrWhiteSpace(picture.UserName))
                {
                    var nameFontDef = socialUserNameFont ?? new FontDefinitionInfo();
                    var style = nameFontDef.ParseStyleValueToEnum();
                    var font = new Font(nameFontDef.FamilyName, nameFontDef.Size, style, nameFontDef.GraphicsUnit);
                    var brush = new SolidBrush(nameFontDef.Color);
                    
                    resultBitmap.AddTextToBitmap(picture.UserName, absoluteLocationProfileUserName.Value.X, absoluteLocationProfileUserName.Value.Y, false, brush, font);
                }

                #endregion

                // The overlay is over the photo
                var overlay = UiFilesUtil.GetFileName(UiFilesUtil.PhotoOverlay, picture.UserId);

                // Put the final overlay over the result if we have one
                if (overlay != null)
                {
                    using (var overlayBitmap = FileUtil.LoadBitmap(overlay))
                        resultBitmap.AddOverlay(overlayBitmap);
                }

                if (RunningFeatureConfig.GetInstance().GetConfiguration<bool>(FeatureConfigEnum.HasContest))
                {
                    var overlayWinner = UiFilesUtil.GetFileName(UiFilesUtil.PhotoOverlayWinner, picture.UserId);

                    // Put the overlay over the result if we have one
                    if (overlayWinner != null)
                    {
                        if (ContestUtil.GetInstance().IsLastEventWinner())
                        {
                            using (var overlayBitmap = FileUtil.LoadBitmap(overlayWinner))
                                resultBitmap.AddOverlay(overlayBitmap);
                        }
                    }
                }

                resultBitmap.SaveAsJpeg(95, saveFile);

                return resultBitmap;
            }
            catch (Exception e)
            {
                log.Error(e);
                resultBitmap?.Dispose();
                throw;
            }
            finally
            {
                // Destruction des fichiers temporaires.
                foreach (var file in modFiles) { try { File.Delete(file); } catch (Exception) { /*Die nice*/ } }
            }
        }

        /// <summary>
        /// Cette méthode transforme un image pour l'affichage en retournant un Bitmap, ainsi que sauvegarder une version imprimable sur disque en paramètre
        /// de sortie.
        /// </summary>
        /// <param name="user">L'usager Printstation qui commande la transformation.</param>
        /// <param name="pictureInfo">Information sur l'image Post choisie.</param>
        /// <param name="pictureOutPath">Le chemin de l'image modifiée.</param>
        /// <param name="angleRotation">La rotation à appliquer sur l'image, uniquement utilisé pour la version Bitmap (à des fins d'affichage à l'écran.)</param>
        /// <param name="createdFileTransformed">Le paramètre de sortie, qui indique le chemin sur disque de l'image créée.</param>
        /// <returns>Un bitmap de l'image créée, avec rotation en extra si autre que zéro degré.</returns>
        public static Bitmap TransformPicture(PrintStationUser user, PicturePostInfo pictureInfo, string pictureOutPath, double angleRotation, out string createdFileTransformed)
        {
            if (!File.Exists(pictureOutPath))
            {
                createdFileTransformed = null;
                return null;
            }

            try
            {
                Bitmap result;

                if (RunningFeatureConfig.GetInstance().GetConfiguration<bool>(FeatureConfigEnum.PictureNoModification))
                {
                    createdFileTransformed = pictureInfo.Src;
                    return FileUtil.LoadBitmap(pictureInfo.Src);
                }

                if (RunningFeatureConfig.GetInstance().GetConfiguration<bool>(FeatureConfigEnum.HasFrame))
                {
                    var pictureName = GenerateUniqueId.GetId() + ".jpg";

                    createdFileTransformed = DirectoryUtil.GetLocalPrintstationMediaDir(
                        pictureInfo.UserId, DirectoryUtil.NomadLogicPrintstationDirectoriesEnum.print) + pictureName;

                    result = CreateFramedPhoto(pictureOutPath, pictureInfo, createdFileTransformed);
                }
                else
                {
                    createdFileTransformed = null;
                    if (RunningFeatureConfig.GetInstance().GetConfiguration<bool>(FeatureConfigEnum.IsPictureSizeTwoBySix))
                    {
                        var pictureName = GenerateUniqueId.GetId() + ".jpg";
                        /*createdFileTransformed = DirectoryUtil.GetLocalIbrandQuestionnaireDataDir(user.EventId, user.FeatureId,
                            // On utilise le EventID et le Feature ID comme équivalence au projet/questionnaire. 
                            DirectoryUtil.ProjectDataDirectoriesEnum.pictures,
                            DirectoryUtil.ProjectDataDirectoriesModEnum.print) + pictureName;*/

                        result = Create2By6Photo(pictureOutPath, createdFileTransformed);
                    }
                    else
                    {
                        /* Image sans transformation spécifique: on imprime comme telle. */

                        // On teste maintenant s'il y a un crop de présentation
                        var usePictureCropInPrint = RunningFeatureConfig.GetInstance().GetConfiguration<bool>(FeatureConfigEnum.UsePicturePresentationCropInPrint);
                        
                        result = usePictureCropInPrint ? GetPresentationPictureFromFile(pictureOutPath) : FileUtil.LoadBitmap(pictureOutPath);
                        createdFileTransformed = pictureOutPath;
                    }
                }

                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (angleRotation == 0) return result;

                var tempBitmap = result.RotateToNewSize(angleRotation);
                result.Dispose();
                result = tempBitmap;

                return result;
            }
            catch (Exception ex)
            {
                createdFileTransformed = null;
                log.Warn(ex, "Could not Transform picture.");
            }

            return null;
        }

        /// <summary>
        /// Obtient l'image pour fin de présentation à l'écran à partir d'un fichier sur disque. On fait la vérification s'il n'y a pas
        /// de transformation de présentation requise durant cette méthode. Cela implique que l'image présentée n'est pas exactement la même
        /// que celle qui sera imprimée et/ou traitée dans un questionnaire.
        /// </summary>
        /// <param name="pictureFile">Le fichier d'image à lire.</param>
        /// <param name="desiredSize"></param>
        /// <param name="thumbDirectory"></param>
        /// <param name="fitInside"></param>
        /// <param name="color"></param>
        /// <returns>L'image de présentation du fichier donné.</returns>
        public static Bitmap GetPresentationPictureFromFile(string pictureFile, Size? desiredSize = null, DirectoryInfo thumbDirectory = null, bool fitInside = false, Color? color = null)
        {
            Bitmap result;
            Rectangle? cropRectangle = null;
            try
            {
                var rectangleValue = RunningFeatureConfig.GetInstance().GetConfiguration<string>(FeatureConfigEnum.PicturePresentationCrop);
                
                if (rectangleValue != null)
                {
                    var coords = StringUtil.GetDataFromDelimitedString(rectangleValue, ',');
                    cropRectangle = new Rectangle(coords[0], coords[1], coords[2], coords[3]);
                }
            }
            catch (Exception e)
            {
                log.Error(e, "Could not create crop rectangle with current {0}", RunningFeatureConfig.GetInstance()
                    .GetConfiguration<string>(FeatureConfigEnum.PicturePresentationCrop));
            }

            if (cropRectangle != null)
            {
                using (var bmp = FileUtil.LoadBitmap(pictureFile))
                {
                    result = bmp.Crop(cropRectangle.Value);
                    return result;
                }
            }

            // On tente ici de créer des thumbnails pour optimiser la vitesse de chargement.
            if (ImageMagickCliUtil.LoadImageFromThumbnail(new FileInfo(pictureFile), thumbDirectory, desiredSize, out result, fitInside, color)) return result;
            log.Warn("Could not load thumbnail of file: {0}", pictureFile);
            result = FileUtil.LoadBitmap(pictureFile);

            return result;
        }

        public static Bitmap Create2By6Photo(string pictureSrc, string saveFile)
        {
            var stream = new FileStream(pictureSrc, FileMode.Open);
            var picture = new Bitmap(stream);
            picture.SetResolution(72f, 72f);
            stream.Close();


            var dest = new Bitmap(picture.Width * 2, picture.Height);
            dest.SetResolution(72f, 72f);

            var g = Graphics.FromImage(dest);

            g.DrawImageUnscaled(picture, new Point(0, 0));

            var rotatedImage = picture.RotateToNewSize(180);

            g.DrawImageUnscaled(rotatedImage, new Point(picture.Width, 0));
            g.Dispose();

            dest.SaveAsJpeg(95, saveFile);

            picture.Dispose();
            rotatedImage.Dispose();
            return dest;
        }

        /// <summary>
        /// Créer un nouveau Post à partir d'un fichier d'image. On considère que le fichier vient d'un envoi direct de iBrand.
        /// </summary>
        /// <param name="elementId">Identifiant du fichier. (similaire à un identifiant venant du web)</param>
        /// <param name="file">Le chemin complet du fichier d'image</param>
        /// <param name="database">Une base de données pour y inscrire le Post.</param>
        /// <param name="user">L'usager qui sera sssocié au Post</param>
        public static void CreatePostFromFile(string elementId, string file, PrintStationDatabase database, PrintStationUser user)
        {
            var fileCreatedDate = File.GetCreationTime(file);

            var post = new Post(-1, elementId, file, null, "ibrand Direct",
                fileCreatedDate.ToString("yyyy-MM-dd HH:mm:ss"), true, false, -1, 1, -1, -1)
            {
                EventId = user.EventId,
                SocialAppServiceName = "iBrand Direct"
            };
            database.InsertIBrandPost(post);
        }
    }
}