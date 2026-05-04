using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading;
using Ionic.Zip;
using JetBrains.Annotations;
using NLog;
using NomadLogic.Common;
using NomadLogic.Common.Extentions;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Services;
using NomadLogic.Common.Services.Configuration;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.DateAndTime;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers.ImageTransforms.Base;
using NomadLogic.Multimedia;
using NomadLogic.Multimedia.Cli.Ffmpeg;
using NomadLogic.Multimedia.Cli.ImageMagickCLI;
using NomadLogic.Multimedia.Gpu;

#pragma warning disable 612

// ReSharper disable NonReadonlyMemberInGetHashCode

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers.ImageTransforms
{
    /// <summary>
    /// Classe statique pour isoler les opérations de transformations sur des séries d'image à partir d'écrans de Photobooth.
    /// Le but est de réutiliser ces fonctions sans être contraint par un système d'héritage, et d'avoir accès facilement à toutes les options de création
    /// pour toutes les étapes.
    /// Le but est aussi d'aider à une éventuelle refactorisation des ces méthodes, et de découpler la classe de "ScreenConfig", qui ne devrait plus être
    /// la seule source de configuration des étapes de transformation.
    /// </summary>
    public static class ImageListTransform
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Lance les possibilités de transformations pour une série d'images prises au préalable.
        /// </summary>
        /// <param name="screen">L'écran qui appelle la fonction.</param>
        /// <param name="imageFiles">La liste des fichiers d'images.</param>
        /// <param name="sc">L'objet de configuration global.</param>
        /// <param name="allOptions">Options de transformations.</param>
        /// <param name="customAction">Action personnalisé.</param>
        [PublicAPI]
        public static void TransformImageList(IAbstractScreen screen, IList<string> imageFiles, ScreenConfig sc,
            ImageListTransformOptions allOptions = null, Action<IAbstractScreen, IList<string>, ScreenConfig> customAction = null)
        {
            if (imageFiles == null || !imageFiles.Any() || screen == null || sc == null) return;

            CreateChromaKeyPicture(screen, imageFiles, sc, allOptions?.ChromaOptions);

            CreateAnimatedGif(screen, imageFiles, sc, allOptions?.GifOptions);

            CreatePictureToPrint(screen, imageFiles, sc, allOptions?.PrintOptions);

            CreateSocialWallPicture(screen, imageFiles, sc, allOptions?.SocialWallPictureOptions);

            CreatePictureBundle(screen, imageFiles, sc, allOptions?.BundleOptions);

            CreateVideo(screen, imageFiles, sc, allOptions?.VideoOptions);

            CreateBaseBackgroundPictures(screen, imageFiles, sc, allOptions?.BaseBackgroundOptions);

            CreateFirstPictureWithChromaKey(screen, imageFiles, sc, allOptions?.FirstPictureChromaKeyOptions);

            CreatePicturesImageMagick(screen, sc, allOptions?.ImageMagickOptions);

            CreateDisplayOnTvPicture(screen, imageFiles, sc, allOptions?.DisplayOnTvPictureOptions);

            CreateFirstPicture(screen, imageFiles, sc, allOptions?.FirstPictureOptions);

            customAction?.Invoke(screen, imageFiles, sc);  
        }

        private static void CreatePicturesImageMagick(IAbstractScreen screen, ScreenConfig sc, 
            IList<ImageListTransformOptionsImageMagick> optionsArg, string saveDirPath = null)
        {
            var optL = optionsArg ?? new List<ImageListTransformOptionsImageMagick>();

            var savePath = string.IsNullOrWhiteSpace(saveDirPath) ?
                new DirectoryInfo(DirectoryUtil.GetLocalIbrandTerminalDataDir(sc.ProjectId, sc.QuestionnaireId, sc.TerminalId,
                    DirectoryUtil.ProjectDataDirectoriesEnum.pictures, DirectoryUtil.ProjectDataDirectoriesModEnum.transformed)) :
                // ReSharper disable once AssignNullToNotNullAttribute
                new DirectoryInfo(saveDirPath);
            var questDir = DirectoryUtil.GetLocalIbrandTerminalDir(sc.ProjectId, sc.QuestionnaireId, sc.TerminalId);

            var resp = screen.GetCurrentRespondentWithAnswers();

            try
            {
                foreach (var opt in optL)
                {
                    if (opt.Condition != null && !opt.Condition.Evaluate(resp)) continue;

                    var cmdFile = questDir + opt.CommandFile;
                    if (!File.Exists(cmdFile))
                    {
                        log.Error("No command file found, cannot continue transformation: " + cmdFile);
                        continue;
                    }

                    var manager = new ImageMagickCliManager
                    {
                        OutputDir = savePath.FullName,
                        CommandFile = cmdFile,
                        WorkingDirectory = savePath.FullName
                    };
                    if (!manager.Init())
                    {
                        throw new System.Exception("Could not initialize ImageMagick manager: " + manager);
                    }

                    var answers = screen.GetCurrentQuestionnaire().GetAnswersDictionary();
                    var cts = new CancellationTokenSource();

                    Progress<ImageMagickCliUiUpdate> prog = null;

                    if (log.IsDebugEnabled)
                    {
                        prog = new Progress<ImageMagickCliUiUpdate>(update =>
                        {
                            log.Debug(update.BasicLineReport());
                        });
                    }
                   

                    var t = manager.ExecuteCommandOnRespondent(resp, answers, questDir, cts.Token, prog);
                    t.Wait((int) opt.Timeout.TotalMilliseconds, cts.Token);

                    if (!t.IsCompleted || t.IsCanceled)
                    {
                        cts.Cancel();
                        log.Debug("Image magick transform failed.");
                        continue;
                    }

                    var result = t.IsCompleted && !t.IsCanceled ? t.Result : null;

                    log.Debug("ImageMagick transformation duration: " + result?.TotalTime.ToStringHourMinSec(true, true));

                    if (result == null || !result.Success)
                    {
                        log.Error("Could not complete transformation: " + manager.CommandFile);
                        continue;
                    }

                    foreach (var key in opt.ResultMapSaveModels.Keys)
                    {
                        if (!result.ResultFiles.ContainsKey(key))
                        {
                            log.Warn("Result mapping does not contain a reference to OUT file tagged: {0}", key);
                            continue;
                        }

                        var file = result.ResultFiles[key];
                        var model = opt.ResultMapSaveModels[key];

                        model.SaveData(screen, file.FullName);
                    }
                }
            }
            catch (System.Exception e)
            {
                log.Error(e, "Error while executing ImageMagickCLI commands.");
            }
        }

        private static void CreateChromaKeyPicture(IAbstractScreen screen, IList<string> imageFiles, ScreenConfig sc, 
            ImageListTransformOptionsChroma optionsArg, string saveDirPath = null)
        {
            var opt = optionsArg ?? new ImageListTransformOptionsChroma();

            var savePath = string.IsNullOrWhiteSpace(saveDirPath) ?
                new DirectoryInfo(DirectoryUtil.GetLocalIbrandTerminalDataDir(sc.ProjectId, sc.QuestionnaireId, sc.TerminalId,
                    DirectoryUtil.ProjectDataDirectoriesEnum.pictures, DirectoryUtil.ProjectDataDirectoriesModEnum.transformed)) :
                // ReSharper disable once AssignNullToNotNullAttribute
                new DirectoryInfo(saveDirPath);

            var resp = screen.GetCurrentRespondentWithAnswers();

            foreach (var trans in opt.Transforms)
            {
                if (trans.Condition != null && !trans.Condition.Evaluate(resp)) continue;

                var filename = savePath.FullName + GenerateUniqueId.GetId() + (trans.CanvasLayer.CanvasFileType == ImageFileTypeEnum.Png ? ".png" : ".jpg");

                using (var canvas = new Bitmap(trans.CanvasLayer.Size.Width, trans.CanvasLayer.Size.Height,
                    trans.CanvasLayer.CanvasFileType == ImageFileTypeEnum.Png
                        ? PixelFormat.Format32bppPArgb
                        : PixelFormat.Format24bppRgb))
                {
                    canvas.SetResolution(300, 300);
                    using (var g = Graphics.FromImage(canvas))
                    {
                        // Si on utilise une image comme canevas
                        if (trans.CanvasLayer.FileName != null)
                        {
                            var path = FileUtil.FindFilePaths(new DirectoryInfo(DirectoryUtil.GetLocalIbrandTerminalDir(sc.ProjectId, sc.QuestionnaireId, sc.TerminalId)), trans.CanvasLayer.FileName);
                            if (path?.Exists ?? false)
                            {
                                using (var canvasBmp = FileUtil.LoadBitmap(path.FullName))
                                {
                                    g.DrawImage(canvasBmp, 0, 0, new Rectangle(0, 0, canvasBmp.Width, canvasBmp.Height), GraphicsUnit.Pixel);
                                }
                            }
                        }
                            
                        // Traite l'image du répondant.
                        var imageFile = imageFiles[0];

                        if (trans.PictureLayer.UseChromaKey)
                        {
                            var tmpOut = DirectoryUtil.CreateLocalTemporaryFile();
                            var nomadConfig = ServicesContainer.GetService<NomadConfiguration>();
                            var result = ChromaKeyManager.ChromaKey(imageFile, tmpOut, nomadConfig);
                            if (!result)
                            {
                                throw new System.Exception("Could not key following file: " + imageFile);
                            }
                            imageFile = tmpOut;
                        }

                        Image bmp;
                        if (trans.PictureLayer.Crop && trans.PictureLayer.Scale != null)
                        {
                            var rect = trans.PictureLayer.Scale.Value;
                            bmp = FileUtil.LoadBitmap(imageFile);
                            var tmp = bmp;
                            var res = bmp.ResizeForView(rect.Width, rect.Height);
                            bmp = res;
                            tmp.Dispose();
                        }
                        else
                        {
                            bmp = FileUtil.LoadBitmap(imageFile, 300, trans.PictureLayer.Scale);
                        }

                        // Calcul de la position
                        var posX = trans.PictureLayer.Position.X;
                        var posY = trans.PictureLayer.Position.Y;
                        if (trans.PictureLayer.Center)
                        {
                            posX -= bmp.Width / 2;
                            posY -= bmp.Height / 2;
                        }

                        g.DrawImage(bmp, posX, posY, new Rectangle(0, 0, bmp.Width, bmp.Height), GraphicsUnit.Pixel);
                        bmp.Dispose();

                        // Si on utilise une image superposée
                        if (trans.Frame?.FilePath != null)
                        {
                            var path = FileUtil.FindFilePaths(new DirectoryInfo(DirectoryUtil.GetLocalIbrandTerminalDir(sc.ProjectId, sc.QuestionnaireId, sc.TerminalId)), trans.Frame.FilePath);
                            if (path?.Exists ?? false)
                            {
                                using (var frameBmp = FileUtil.LoadBitmap(path.FullName, 300, trans.Frame.Scale))
                                {
                                    g.DrawImage(frameBmp, trans.Frame.Position.X, trans.Frame.Position.Y, new Rectangle(0, 0, frameBmp.Width, frameBmp.Height), GraphicsUnit.Pixel);
                                }
                            }
                        }
                    }

                    if (trans.CanvasLayer.CanvasFileType == ImageFileTypeEnum.Png)
                    {
                        canvas.Save(ImageFormat.Png, 9, filename);
                    }
                    else
                    {
                        canvas.Save(ImageFormat.Jpeg, 90, filename);
                    }
                }

                screen.SaveAnswerToQuestion(new Answer{ QuestionNo = trans.SaveToQuestion, Response = filename});
            }

        }

        /// <summary>
        /// Créer une ou plusieures images basées avec l'image dite "base background", qui est une référence vers un fichier d'image voulu global à l'application.
        /// </summary>
        /// <param name="screen">Écran Photobooth appelant.</param>
        /// <param name="imageFiles">Fichiers d'images.</param>
        /// <param name="sc">Configuration d'écran variable.</param>
        /// <param name="optionsArg">Options de transformation.</param>
        /// <param name="finalFileDirPath">Force l'usage d'un chemin de fichier précis. DOit être un répertoire.</param>
        private static void CreateBaseBackgroundPictures(IAbstractScreen screen, IList<string> imageFiles, ScreenConfig sc,
            ImageListTransformOptionsBaseBackground optionsArg = null, string finalFileDirPath = null)
        {
            var opt = optionsArg ?? new ImageListTransformOptionsBaseBackground();

            if (opt.BasePictureTransforms == null || !opt.BasePictureTransforms.Any()) return;

            var baseDir = string.IsNullOrWhiteSpace(finalFileDirPath) ? 
                new DirectoryInfo(DirectoryUtil.GetLocalIbrandTerminalDataDir(sc.ProjectId, sc.QuestionnaireId, sc.TerminalId,
                DirectoryUtil.ProjectDataDirectoriesEnum.pictures, DirectoryUtil.ProjectDataDirectoriesModEnum.transformed)) : 
                // ReSharper disable once AssignNullToNotNullAttribute
                new DirectoryInfo(finalFileDirPath);

            var tmp = DirectoryUtil.GetLocalNomadLogicDir() + "BasePictureFileInfo.txt";
            if (!File.Exists(tmp))
            {
                throw new System.Exception("No base picture configuration found.");
            }
            var basePictLocation = File.ReadAllText(tmp);
          
            if (!File.Exists(basePictLocation))
            {
                throw new System.Exception("Base picture file invalid: " + basePictLocation);
            }

            foreach (var trans in opt.BasePictureTransforms)
            {
                using (var basePict = FileUtil.LoadBitmap(basePictLocation, 300, trans.BaseBackground.Scale))
                {
                    var filename = baseDir.FullName + GenerateUniqueId.GetId() + (trans.Canvas.CanvasFileType == ImageFileTypeEnum.Png ? ".png" : ".jpg");

                    using (var canvas = new Bitmap(trans.Canvas.Size.Width, trans.Canvas.Size.Height,
                        trans.Canvas.CanvasFileType == ImageFileTypeEnum.Png
                            ? PixelFormat.Format32bppPArgb
                            : PixelFormat.Format24bppRgb))
                    {
                        canvas.SetResolution(300, 300);
                        using (var g = Graphics.FromImage(canvas))
                        {
                            // Si on utilise une image de fond.
                            if (trans.Frame.FilePath != null)
                            {
                                using (var frameBmp =
                                    FileUtil.LoadBitmap(trans.Frame.FilePath, 300, trans.Frame.Scale))
                                {
                                    g.DrawImage(frameBmp, trans.Frame.Position.X, trans.Frame.Position.Y, new Rectangle(0, 0, frameBmp.Width, frameBmp.Height), GraphicsUnit.Pixel);
                                }
                            }

                            // Dessine l'image de base sur le canevas
                            g.DrawImage(basePict, trans.BaseBackground.Position.X, trans.BaseBackground.Position.Y, new Rectangle(0, 0, basePict.Width, basePict.Height), GraphicsUnit.Pixel);

                            // Traite l'image du répondant.
                            var imageFile = imageFiles[0];

                            if (trans.Picture.UseChromaKey)
                            {
                                var tmpOut = DirectoryUtil.CreateLocalTemporaryFile();
                                var nomadConfig = ServicesContainer.GetService<NomadConfiguration>();
                                var result = ChromaKeyManager.ChromaKey(imageFile, tmpOut, nomadConfig);
                                if (!result)
                                {
                                    throw new System.Exception("Could not key following file: " + imageFile);
                                }
                                imageFile = tmpOut;
                            }

                            using (var bmp = FileUtil.LoadBitmap(imageFile, 300, trans.Picture.Scale))
                            {
                                g.DrawImage(bmp, trans.Picture.Position.X, trans.Picture.Position.Y, new Rectangle(0, 0, bmp.Width, bmp.Height), GraphicsUnit.Pixel);
                            }

                            foreach (var overlay in trans.Overlays)
                            {
                                // Si on utilise un overlay
                                if (overlay.FilePath != null)
                                {
                                    using (var overlayBmp = FileUtil.LoadBitmap(overlay.FilePath, 300, overlay.Scale))
                                    {
                                        g.DrawImage(overlayBmp, overlay.Position.X, overlay.Position.Y, new Rectangle(0, 0, overlayBmp.Width, overlayBmp.Height), GraphicsUnit.Pixel);
                                    }
                                }
                            }
                        }

                        if (trans.Canvas.CanvasFileType == ImageFileTypeEnum.Png)
                        {
                            canvas.Save(ImageFormat.Png, 9, filename);
                        }
                        else
                        {
                            canvas.Save(ImageFormat.Jpeg, 90, filename);
                        }
                    }

                    screen.SaveAnswerToQuestion(new Answer{QuestionNo = trans.SaveToQuestionNo, Response = filename});
                }
                
            }
        }
        
        /// <summary>
        /// Créer une image destinée à être une sorte de "thumbnail" pour le répondant. Cette image sera toujours basée sur la première de la liste.
        /// </summary>
        /// <param name="screen">Écran Photobooth appelant.</param>
        /// <param name="imageFiles">Fichiers d'images.</param>
        /// <param name="sc">Configuration d'écran variable.</param>
        /// <param name="optionsArg">Options de transformation.</param>
        /// <param name="finalFilePath">Force l'usage d'un chemin de fichier précis.</param>
        [PublicAPI]
        public static void CreateFirstPicture(IAbstractScreen screen, IList<string> imageFiles, ScreenConfig sc,
            ImageListTransformOptionsFirstPicture optionsArg = null, string finalFilePath = null)
        {
            var opt = optionsArg ?? new ImageListTransformOptionsFirstPicture();
            var toSave = opt.SaveDataModel ?? null;
            if (toSave == null) return;

            CreateFirstPicture(screen, sc, imageFiles, null, opt);
        }

        /// <summary>
        /// Créer une image chromatise destinée à être une sorte de "thumbnail" pour le répondant. Cette image sera toujours basée sur la première de la liste.
        /// </summary>
        /// <param name="screen">Écran Photobooth appelant.</param>
        /// <param name="imageFiles">Fichiers d'images.</param>
        /// <param name="sc">Configuration d'écran variable.</param>
        /// <param name="optionsArg">Options de transformation.</param>
        /// <param name="finalFilePath">Force l'usage d'un chemin de fichier précis.</param>
        [PublicAPI]
        public static void CreateFirstPictureWithChromaKey(IAbstractScreen screen, IList<string> imageFiles,
            ScreenConfig sc,
            ImageListTransformOptionsFirstPictureChromaKey optionsArg = null, string finalFilePath = null)
        {
            var opt = optionsArg ?? new ImageListTransformOptionsFirstPictureChromaKey();
            var toSave = opt.SaveToQuestion ?? -1;
            if (toSave == -1) return;

            var pictureName = GenerateUniqueId.GetId() + ".png";
            var outputFilePath = finalFilePath ?? DirectoryUtil.GetLocalIbrandTerminalDataDir(sc.ProjectId, sc.QuestionnaireId, sc.TerminalId,
                                     DirectoryUtil.ProjectDataDirectoriesEnum.pictures, DirectoryUtil.ProjectDataDirectoriesModEnum.transformed) + pictureName;

            //var resp = screen.GetCurrentRespondentWithAnswers();

            //CreateFirstPictureWithChromaKey(resp, imageFiles, outputFilePath, opt);
            var nomadConfig = ServicesContainer.GetService<NomadConfiguration>();
            var result = ChromaKeyManager.ChromaKey(imageFiles[0], outputFilePath, nomadConfig);
            if (!result || !File.Exists(outputFilePath))
            {
                throw new System.Exception("Could not key following file: " + imageFiles[0]);
            }

            // Sauvegarde dans le questionnaire.
            screen.SaveAnswerToQuestion(new Answer{QuestionNo = toSave, Response = outputFilePath});
        }

        /// <summary>
        /// Créer une image destinée à être affichée par l'application DisplayOnTv. Est souvent générée en marge du questionnaire.
        /// Cette image sera toujours basée sur la première de la liste.
        /// </summary>
        /// <param name="screen">Écran Photobooth appelant.</param>
        /// <param name="imageFiles">Fichiers d'images.</param>
        /// <param name="sc">Configuration d'écran variable.</param>
        /// <param name="optionsArg">Options de transformation.</param>
        /// <param name="finalFilePath">Force l'usage d'un chemin de fichier précis.</param>
        [PublicAPI]
        public static void CreateDisplayOnTvPicture(IAbstractScreen screen, IList<string> imageFiles, ScreenConfig sc,
            ImageListTransformOptionsDisplayOnTv optionsArg = null, string finalFilePath = null)
        {
            if (imageFiles == null || !imageFiles.Any() || optionsArg == null) return;
            var opt = optionsArg;
            var toSave = opt.SaveToQuestion ?? -1;
            if (toSave == -1) return;

            var pictureName = GenerateUniqueId.GetId() + ".jpg";
            var outputFilePath = finalFilePath ?? DirectoryUtil.GetLocalShowPicturesOnTvPicturesDir() + pictureName;
            var outputFilePathQuest = finalFilePath ?? DirectoryUtil.GetLocalIbrandTerminalDataDir(sc.ProjectId, sc.QuestionnaireId, sc.TerminalId,
                                          DirectoryUtil.ProjectDataDirectoriesEnum.pictures, DirectoryUtil.ProjectDataDirectoriesModEnum.socialwall) + pictureName;

            var resp = screen.GetCurrentRespondentWithAnswers();

            if (opt.Condition != null && !opt.Condition.Evaluate(resp)) return;

            // 0- Ouverture préalable de l'image principale
            using (var picture = FileUtil.LoadBitmap(opt.InputFromQuestion == null
                ? imageFiles[0]
                : resp.GetAnswerText(opt.InputFromQuestion.Value)))
            {
                // Calcul des valeurs de position (avec fallback quand valeurs précises sont inexistantes).
                var modPictureSizeX = opt.NewSize?.Width ?? picture.Width;
                var modPictureSizeY = opt.NewSize?.Height ?? picture.Height;
                var canvasSizeX = opt.CanvasSize?.Width ?? modPictureSizeX;
                var canvasSizeY = opt.CanvasSize?.Height ?? modPictureSizeY;
                var picturePosX = opt.NewPos?.X ?? 0;
                var picturePosY = opt.NewPos?.Y ?? 0;
                var overlayPosX = opt.OverlayPos?.X ?? 0;
                var overlayPosY = opt.OverlayPos?.Y ?? 0;
                var overlaySizeX = opt.OverlayNewSize?.Width ?? -1;
                var overlaySizeY = opt.OverlayNewSize?.Height ?? -1;

                // 0- Création du canevas et de sa couleur de base
                using (var canevas = new Bitmap(canvasSizeX, canvasSizeY))
                {
                    canevas.SetResolution(300, 300);
                    using (var g = Graphics.FromImage(canevas))
                    {
                        var brush = new SolidBrush(opt.CanvasBaseColor ?? Color.Transparent);
                        g.FillRectangle(brush, 0, 0, canvasSizeX, canvasSizeY);

                        // 1- Ajout du background si existant.
                        {
                            if (opt.BackgroundImage != null)
                            {
                                if (File.Exists(opt.BackgroundImage))
                                {
                                    Bitmap bkg = null;
                                    try
                                    {
                                        bkg = FileUtil.LoadBitmap(opt.BackgroundImage);

                                        // Redimensionnement de l'image principale, si nécessaire.
                                        if (bkg.Width != canvasSizeX || bkg.Height != canvasSizeY)
                                        {
                                            var tmp = bkg.Frame(canvasSizeX, canvasSizeY);
                                            bkg.Dispose();
                                            bkg = tmp;
                                        }

                                        g.DrawImage(bkg, 0, 0, new Rectangle(0, 0, bkg.Width, bkg.Height),
                                            GraphicsUnit.Pixel);
                                    }
                                    finally
                                    {
                                        // ReSharper disable once ConstantConditionalAccessQualifier
                                        bkg?.Dispose();
                                    }
                                }
                                else
                                {
                                    log.Warn(
                                        "Background image for Display On TV image transformation does not exists. Skipping step. Check configurations: "
                                        + opt.BackgroundImage);
                                }
                            }
                        }

                        // 2- Ajout de l'image principale
                        {
                            using (var image = picture.Width != modPictureSizeX || picture.Height != modPictureSizeY ? 
                                    picture.Frame(modPictureSizeX, modPictureSizeY) : 
                                    new Bitmap(picture))
                            {
                                if (opt.NewRotateFlip.HasValue)
                                {
                                    image.RotateFlip(opt.NewRotateFlip.Value);
                                }

                                g.DrawImage(image, picturePosX, picturePosY, new Rectangle(0, 0, image.Width, image.Height), GraphicsUnit.Pixel);
                            }
                        }

                        // 3- Ajout d'un overlay, si existant.
                        var overFile = opt.OverlaySelector?.GetImageFrame(resp, 0);
                        if (overFile != null && File.Exists(overFile))
                        {
                            Bitmap overlay = null;
                            try
                            {
                                overlay = FileUtil.LoadBitmap(overFile);

                                // Redimensionnement de l'image principale, si nécessaire.
                                if (overlaySizeX != -1 && overlaySizeY != -1)
                                {
                                    if (overlay.Width != overlaySizeX || overlay.Height != overlaySizeY)
                                    {
                                        var tmp = overlay.Frame(overlaySizeX, overlaySizeY);
                                        overlay.Dispose();
                                        overlay = tmp;
                                    }
                                }

                                g.DrawImage(overlay, overlayPosX, overlayPosY,
                                    new Rectangle(0, 0, overlay.Width, overlay.Height), GraphicsUnit.Pixel);
                            }
                            finally
                            {
                                // ReSharper disable once ConstantConditionalAccessQualifier
                                overlay?.Dispose();
                            }
                        }

                        // 4- Ajout du texte, si existant.
                        if (opt.FontDefinition != null && opt.QuestionNoToFetchText.HasValue &&
                            opt.QuestionNoToFetchText.Value > 0)
                        {
                            var fd = opt.FontDefinition;

                            //4a- Charger le font spécial, si inconnu
                            if (!opt.CustomFontCollection.Families.Any() && fd.FontFile != null &&
                                File.Exists(fd.FontFile)) // Si vide
                            {
                                opt.CustomFontCollection.AddFontFile(fd.FontFile);
                            }

                            // Variables pour le texte
                            var text = resp.GetAnswerText(opt.QuestionNoToFetchText.Value);
                            if (!string.IsNullOrWhiteSpace(text))
                            {
                                if (opt.TextCase == 1)
                                {
                                    text = text.ToUpperInvariant();
                                }

                                if (opt.TextCase == 2)
                                {
                                    text = text.ToLowerInvariant();
                                }

                                var fontFamilly = opt.CustomFontCollection.Families.First(fontCheck =>
                                    fontCheck.Name.Contains(fd.FamilyName));
                                var textPosX = opt.TextPos?.X ?? 0;
                                var textPosY = opt.TextPos?.Y ?? 0;
                                var brushFont = new SolidBrush(fd.Color);
                                var font = new Font(fontFamilly, fd.Size, fd.ParseStyleValueToEnum(), fd.GraphicsUnit);

                                canevas.AddTextToBitmap(text, textPosX, textPosY, opt.TextCentered, brushFont, font,
                                    opt.TextRotation, opt.TextMaxPixelPerLine, opt.TextLineOffset ?? 0);
                            }
                            else
                            {
                                throw new System.Exception(
                                    $"No text in question {opt.QuestionNoToFetchText.Value} for respondent.");
                            }
                        }
                    }

                    // Sauvegarde du canevas
                    canevas.Save(opt.JpegSaveQuality, outputFilePathQuest);

                }
            }

            // Copie dans le répertoire du ShowOnTv.
            File.Copy(outputFilePathQuest, outputFilePath);

            // Sauvegarde dans le questionnaire.
            screen.SaveAnswerToQuestion(new Answer {QuestionNo = toSave, Response = outputFilePathQuest});
        }

        /// <summary>
        /// Créer une image destinée à être une sorte de "thumbnail" pour le répondant. Cette image sera toujours basée sur la première de la liste.
        /// </summary>
        /// <param name="screen">L'écran qui appelle.</param>
        /// <param name="sc">Configuration globale</param>
        /// <param name="imageFiles">Fichiers d'images.</param>
        /// <param name="optionsArg">Options de transformation.</param>
        /// <param name="finalFilePath">Force l'usage d'un chemin de fichier précis.</param>
        [PublicAPI]
        public static void CreateFirstPicture(IAbstractScreen screen, ScreenConfig sc, IList<string> imageFiles, string finalFilePath,
            ImageListTransformOptionsFirstPicture optionsArg)
        {
            if (imageFiles == null || !imageFiles.Any()) return;
            var options = optionsArg ?? new ImageListTransformOptionsFirstPicture();
            var toSave = options.SaveDataModel;

            var pictureName = GenerateUniqueId.GetId() + ".jpg";
            var outputFilePathQuest = finalFilePath ?? DirectoryUtil.GetLocalIbrandTerminalDataDir(sc.ProjectId, sc.QuestionnaireId, sc.TerminalId,
                                          DirectoryUtil.ProjectDataDirectoriesEnum.pictures, DirectoryUtil.ProjectDataDirectoriesModEnum.firstpicture) + pictureName;

            var resp = screen.GetCurrentRespondentWithAnswers();

            if (!options.Condition?.Evaluate(resp) ?? false) return;

            // 0- Ouverture préalable de l'image principale
            using (var picture = FileUtil.LoadBitmap(options.InputFromQuestion == null
                ? imageFiles[0]
                : resp.GetAnswerText(options.InputFromQuestion.Value)))
            {
                // Calcul des valeurs de position (avec fallback quand valeurs précises sont inexistantes).
                var modPictureSizeX = options.NewSize?.Width ?? picture.Width;
                var modPictureSizeY = options.NewSize?.Height ?? picture.Height;
                var canvasSizeX = options.CanvasSize?.Width ?? modPictureSizeX;
                var canvasSizeY = options.CanvasSize?.Height ?? modPictureSizeY;
                var picturePosX = options.NewPos?.X ?? 0;
                var picturePosY = options.NewPos?.Y ?? 0;
                var overlayPosX = options.OverlayPos?.X ?? 0;
                var overlayPosY = options.OverlayPos?.Y ?? 0;

                // 1- Création du canevas et de sa couleur de base
                using (var canevas = new Bitmap(canvasSizeX, canvasSizeY))
                {
                    canevas.SetResolution(300, 300);
                    using (var g = Graphics.FromImage(canevas))
                    {
                        var brush = new SolidBrush(options.CanvasBaseColor ?? Color.Transparent);
                        g.FillRectangle(brush, 0, 0, canvasSizeX, canvasSizeY);

                        // 1- Ajout du background si existant.
                        {
                            if (options.BackgroundImage != null)
                            {
                                if (File.Exists(options.BackgroundImage))
                                {
                                    Bitmap bkg = null;
                                    try
                                    {
                                        bkg = FileUtil.LoadBitmap(options.BackgroundImage);

                                        // Redimensionnement de l'image principale, si nécessaire.
                                        if (bkg.Width != canvasSizeX || bkg.Height != canvasSizeY)
                                        {
                                            var tmp = bkg.Frame(canvasSizeX, canvasSizeY);
                                            bkg.Dispose();
                                            bkg = tmp;
                                        }

                                        g.DrawImage(bkg, 0, 0, new Rectangle(0, 0, bkg.Width, bkg.Height),
                                            GraphicsUnit.Pixel);
                                    }
                                    finally
                                    {
                                        // ReSharper disable once ConstantConditionalAccessQualifier
                                        bkg?.Dispose();
                                    }
                                }
                                else
                                {
                                    log.Warn(
                                        "Background image for First picture image transformation does not exists. Skipping step. Check configurations: "
                                        + options.BackgroundImage);
                                }
                            }
                        }

                        // 2- Ajout de l'image principale
                        {
                            using (var image = picture.Width != modPictureSizeX || picture.Height != modPictureSizeY ?
                                picture.Frame(modPictureSizeX, modPictureSizeY) :
                                new Bitmap(picture))
                            {
                                if (options.NewRotateFlip.HasValue)
                                {
                                    image.RotateFlip(options.NewRotateFlip.Value);
                                }

                                g.DrawImage(image, picturePosX, picturePosY, new Rectangle(0, 0, image.Width, image.Height), GraphicsUnit.Pixel);
                            }
                        }

                        // 3- Ajout d'un overlay, si existant.
                        var overFile = options.OverlaySelector?.GetImageFrame(resp, 0);
                        if (overFile != null)
                        {
                            if (File.Exists(overFile))
                            {
                                using (var over = FileUtil.LoadBitmap(overFile))
                                {
                                    g.DrawImage(over, overlayPosX, overlayPosY,
                                        new Rectangle(0, 0, over.Width, over.Height), GraphicsUnit.Pixel);
                                }
                            }
                            else
                            {
                                log.Warn("Overlay file does not exists. Check configuration: filePath=" + overFile);
                            }
                        }

                        // 4- Ajout du texte, si existant.
                        if (options.FontDefinition != null && options.QuestionNoToFetchText.HasValue && options.QuestionNoToFetchText.Value > 0)
                        {
                            var fd = options.FontDefinition;

                            //4a- Charger le font spécial, si inconnu
                            if (!options.CustomFontCollection.Families.Any() && fd.FontFile != null && File.Exists(fd.FontFile)) // Si vide
                            {
                                options.CustomFontCollection.AddFontFile(fd.FontFile);
                            }

                            // Variables pour le texte
                            var text = resp.GetAnswerText(options.QuestionNoToFetchText.Value);
                            if (!string.IsNullOrWhiteSpace(text))
                            {
                                if (options.TextCase == 1)
                                {
                                    text = text.ToUpperInvariant();
                                }
                                if (options.TextCase == 2)
                                {
                                    text = text.ToLowerInvariant();
                                }

                                var fontFamilly = options.CustomFontCollection.Families.First(fontCheck => fontCheck.Name.Contains(fd.FamilyName));
                                var textPosX = options.TextPos?.X ?? 0;
                                var textPosY = options.TextPos?.Y ?? 0;
                                var brushFont = new SolidBrush(fd.Color);
                                var font = new Font(fontFamilly, fd.Size, fd.ParseStyleValueToEnum(), fd.GraphicsUnit);

                                canevas.AddTextToBitmap(text, textPosX, textPosY, options.TextCentered, brushFont, font, 
                                    options.TextRotation, options.TextMaxPixelPerLine, options.TextLineOffset ?? 0);
                            }
                            else
                            {
                                throw new System.Exception($"No text in question {options.QuestionNoToFetchText.Value} for respondent.");
                            }
                        }
                    }

                    // Sauvegarde du canevas
                    canevas.Save(options.JpegSaveQuality, outputFilePathQuest);
                }
            }


            // Sauvegarde dans le questionnaire.
            if (toSave != null)
            {
                toSave.SaveData(screen.GetCurrentQuestionnaire(), outputFilePathQuest);
            }
        }


        /// <summary>
        /// Créer une image chromatise destinée à être une sorte de "thumbnail" pour le répondant. Cette image sera toujours basée sur la première de la liste.
        /// </summary>
        /// <param name="respondent">Le répondant, avec réponses, requis pour exécuter la transformation.</param>
        /// <param name="imageFiles">Fichiers d'images.</param>
        /// <param name="options">Options de transformation.</param>
        /// <param name="finalFilePath">Force l'usage d'un chemin de fichier précis.</param>
        [PublicAPI]
        [Obsolete]
        public static void CreateFirstPictureWithChromaKey(IRespondent respondent, IList<string> imageFiles, string finalFilePath,
            ImageListTransformOptionsFirstPictureChromaKey options)
        {
            if (imageFiles == null || !imageFiles.Any()) return;

            var nomadConfig = ServicesContainer.GetService<NomadConfiguration>();
            var result = ChromaKeyManager.ChromaKey(imageFiles[0], finalFilePath, nomadConfig);
            if (!result)
            {
                throw new System.Exception("Could not key following file: " + imageFiles[0]);
            }

            /*
            // 0- Ouverture préalable de l'image principale
            using (var picture = FileUtil.LoadBitmap(imageFiles[0]))
            {
                // Calcul des valeurs de position (avec fallback quand valeurs précises sont inexistantes).
                var modPictureSizeX = options.NewSize?.Width ?? picture.Width;
                var modPictureSizeY = options.NewSize?.Height ?? picture.Height;
                var canvasSizeX = options.CanvasSize?.Width ?? modPictureSizeX;
                var canvasSizeY = options.CanvasSize?.Height ?? modPictureSizeY;
                var picturePosX = options.NewPos?.X ?? 0;
                var picturePosY = options.NewPos?.Y ?? 0;
                var overlayPosX = options.OverlayPos?.X ?? 0;
                var overlayPosY = options.OverlayPos?.Y ?? 0;

                // 1- Création du canevas et de sa couleur de base
                using (var canevas = new Bitmap(canvasSizeX, canvasSizeY))
                {
                    using (var g = Graphics.FromImage(canevas))
                    {
                        var brush = new SolidBrush(options.CanvasBaseColor ?? Color.Transparent);
                        g.FillRectangle(brush, 0, 0, canvasSizeX, canvasSizeY);

                        // 2- Ajout de l'image principale
                        {
                            Bitmap image = null;
                            try
                            {
                                

                                // Traite l'image du répondant.
                                var imageFile = imageFiles[0];


                                //if (options.UseChromaKey)
                                //{
                                    var tmpOut = DirectoryUtil.CreateLocalTemporaryFile();
                                    var nomadConfig = ServicesContainer.GetService<NomadConfiguration>();
                                    var result = ChromaKeyManager.ChromaKey(imageFile, tmpOut, nomadConfig);
                                    if (!result)
                                    {
                                        throw new System.Exception("Could not key following file: " + imageFile);
                                    }
                                    imageFile = tmpOut;
                                //}


                                using (var bmp = FileUtil.LoadBitmap(imageFile, 300, options.NewSize))
                                {
                                    g.DrawImage(bmp, picturePosX, picturePosY, new Rectangle(0, 0, bmp.Width, bmp.Height), GraphicsUnit.Pixel);
                                }

                                image = FileUtil.LoadBitmap(imageFile, 300, new Size(2095,2095));
                                g.DrawImage(image, picturePosX, picturePosY);

                                // Redimensionnement de l'image principale, si nécessaire.
                                if (image.Width != modPictureSizeX || image.Height != modPictureSizeY)
                                {
                                    var tmp = image.Frame(modPictureSizeX,
                                        modPictureSizeY);
                                    image.Dispose();
                                    image = tmp;
                                }


                                if (options.NewRotateFlip.HasValue)
                                {
                                    image.RotateFlip(options.NewRotateFlip.Value);
                                }

                                
                            }
                            finally
                            {
                                // ReSharper disable once ConstantConditionalAccessQualifier
                                image?.Dispose();
                            }
                        }

                        // 3- Ajout d'un overlay, si existant.
                        var overFile = options.OverlaySelector?.GetImageFrame(respondent, 0);
                        if (overFile != null && File.Exists(overFile))
                        {
                            using (var over = FileUtil.LoadBitmap(overFile))
                            {
                                g.DrawImage(over, overlayPosX, overlayPosY);
                            }
                        }

                        // 4- Ajout du texte, si existant.
                        if (options.FontDefinition != null && options.QuestionNoToFetchText.HasValue && options.QuestionNoToFetchText.Value > 0)
                        {
                            var fd = options.FontDefinition;

                            //4a- Charger le font spécial, si inconnu
                            if (!options.CustomFontCollection.Families.Any() && fd.FontFile != null && File.Exists(fd.FontFile)) // Si vide
                            {
                                options.CustomFontCollection.AddFontFile(fd.FontFile);
                            }

                            // Variables pour le texte
                            var text = respondent.GetAnswerText(options.QuestionNoToFetchText.Value);
                            if (!string.IsNullOrWhiteSpace(text))
                            {
                                if (options.TextCase == 1)
                                {
                                    text = text.ToUpperInvariant();
                                }
                                if (options.TextCase == 2)
                                {
                                    text = text.ToLowerInvariant();
                                }

                                var fontFamilly = options.CustomFontCollection.Families.First(fontCheck => fontCheck.Name.Contains(fd.FamilyName));
                                var textPosX = options.TextPos?.X ?? 0;
                                var textPosY = options.TextPos?.Y ?? 0;
                                var brushFont = new SolidBrush(fd.Color);
                                var font = new Font(fontFamilly, fd.Size, fd.ParseStyleValueToEnum(), fd.GraphicsUnit);

                                canevas.AddTextToBitmap(text, textPosX, textPosY, options.TextCentered, brushFont, font,
                                    options.TextRotation, options.TextMaxPixelPerLine, options.TextLineOffset ?? 0);
                            }
                            else
                            {
                                throw new System.Exception($"No text in question {options.QuestionNoToFetchText.Value} for respondent.");
                            }
                        }
                    }

                    // Sauvegarde du canevas
                    canevas.Save(options.PngSaveQuality, finalFilePath);
                }
            }

    */
        }


        /// <summary>
        /// Créer une image destinée à être affichée sur un mur social. Elle est composée comme une impression de GIF, c'est à dire que les images
        /// listée sont affichées en mosaïque sur la surface de l'image finale.
        /// </summary>
        /// <param name="screen">Écran Photobooth appelant.</param>
        /// <param name="imageFiles">Fichiers d'images.</param>
        /// <param name="sc">Configuration d'écran variable.</param>
        /// <param name="options">Options de transformation.</param>
        [PublicAPI]
        public static void CreateSocialWallPicture(IAbstractScreen screen, IList<string> imageFiles, ScreenConfig sc,
            ImageListTransformOptionsSocialWall options = null)
        {
            if (imageFiles == null || !imageFiles.Any() || screen == null || sc == null) return;

            var opt = options ?? new ImageListTransformOptionsSocialWall();
            var toSave = opt.SaveToQuestion ?? sc.QuestionPictureSocialWallNo ?? -1;
            if (toSave == -1) return;

            var pictureName = GenerateUniqueId.GetId() + ".jpg";

            var outputFilePath = DirectoryUtil.GetLocalIbrandTerminalDataDir(sc.ProjectId, sc.QuestionnaireId, sc.TerminalId,
                    DirectoryUtil.ProjectDataDirectoriesEnum.pictures, DirectoryUtil.ProjectDataDirectoriesModEnum.socialwall) + pictureName;

            using (var finalImage = new Bitmap(opt.FinalSize.Width, opt.FinalSize.Height))
            {
                finalImage.SetResolution(opt.ImageResolution, opt.ImageResolution);

                using (var g = Graphics.FromImage(finalImage))
                {

                    Bitmap frame = null;
                    // Ajout d'un overlay, si existant.
                    var imageFile = opt.PrintFrameSelector?.GetImageFrame(screen.GetCurrentRespondentWithAnswers(), 0);
                    if (File.Exists(imageFile)) { frame = FileUtil.LoadBitmap(imageFile); }

                    if (frame != null)
                    {
                        if (!opt.FrameOverPicture)
                        {
                            g.DrawImage(frame, 0, 0, new Rectangle(0, 0, frame.Width, frame.Height), GraphicsUnit.Pixel);
                        }
                        else if (opt.BackgroundColor.HasValue)
                        {
                            g.FillRectangle(new SolidBrush(opt.BackgroundColor.Value), 0, 0, frame.Width, frame.Height); //  Couleur optionnelle
                        }
                    }

                    var count = 0;
                    var row = 0;
                    var mod = 0;

                    var nbrCol = (int)Math.Ceiling(Math.Sqrt(imageFiles.Count));

                    foreach (var fileName in imageFiles)
                    {
                        var image = FileUtil.LoadBitmap(fileName);

                        if (!image.Size.Equals(opt.SubImageSize))
                        {
                            var tmp = image.Frame(opt.SubImageSize.Width, opt.SubImageSize.Height);
                            image.Dispose();
                            image = tmp;
                        }

                        g.DrawImage(image, new Point(
                            opt.Padding + ((mod * image.Width) + (mod * opt.Padding)),
                            opt.Padding + ((row * image.Height) + (row * opt.Padding))));
                        image.Dispose();

                        count++;
                        mod = count % nbrCol;
                        if (mod == 0) row++;
                    }

                    if (frame != null)
                    {
                        if (opt.FrameOverPicture)
                        {
                            g.DrawImage(frame, 0, 0, new Rectangle(0, 0, frame.Width, frame.Height), GraphicsUnit.Pixel);
                        }
                        frame.Dispose();
                    }
                }

                finalImage.Save(opt.JpegSaveQuality, outputFilePath);
            }

            screen.SaveAnswerToQuestion(new Answer {QuestionNo = toSave, Response = outputFilePath});
        }

        /// <summary>
        /// Créer un réponse qui correspond à un groupe d'image (un Bundle). En clair, un fichier zip.
        /// </summary>
        /// <param name="screen">Écran Photobooth appelant.</param>
        /// <param name="imageFiles">Fichiers d'images.</param>
        /// <param name="sc">Configuration d'écran variable.</param>
        /// <param name="options">Options de transformation.</param>
        [PublicAPI]
        public static void CreatePictureBundle(IAbstractScreen screen, IList<string> imageFiles, ScreenConfig sc,
            ImageListTransformOptionsBundle options = null)
        {
            if (imageFiles == null || !imageFiles.Any() || screen == null || sc == null) return;

            var opt = options ?? new ImageListTransformOptionsBundle();
            var toSave = opt.SaveToQuestion ?? sc.QuestionPhotoBundleNo ?? -1;
            if (toSave == -1) return;

            var dirName = GenerateUniqueId.GetId();

            var dirAbsolutePath = DirectoryUtil.GetLocalApplicationWorkDir() + dirName + Path.DirectorySeparatorChar;

            Bitmap overlayBitmap = null;
            try {
                Directory.CreateDirectory(dirAbsolutePath);

                var compteur = 1;

                var overlay = opt.OverlaySelector?.GetImageFrame(screen.GetCurrentRespondentWithAnswers());

                if (overlay != null)
                    overlayBitmap = FileUtil.LoadBitmap(overlay);

                foreach (var fileName in imageFiles) {
                    var newName = dirAbsolutePath + compteur + ".jpg";

                    if (opt.NewSize.HasValue) {
                        using (var image = FileUtil.LoadBitmap(fileName)) {
                            using (var resized = image.Frame(opt.NewSize.Value.Height, opt.NewSize.Value.Height)) {
                                if (overlayBitmap != null)
                                {
                                    resized.AddOverlay(overlayBitmap);
                                }
                                resized.Save(opt.JpegSaveQuality, newName);
                            }
                        }
                    }
                    else {
                        if (overlayBitmap != null)
                        {
                            using (var image = FileUtil.LoadBitmap(fileName))
                            {
                                image.AddOverlay(overlayBitmap);
                                image.Save(opt.JpegSaveQuality, newName);
                            }
                        }
                        else
                        {
                            File.Copy(fileName, newName);
                        }
                    }

                    compteur++;
                }

                var pictures = Directory.GetFiles(dirAbsolutePath);

                var zippedFileName = DirectoryUtil.GetLocalIbrandTerminalDataDir(sc.ProjectId, sc.QuestionnaireId, sc.TerminalId,
                                                                                      DirectoryUtil.ProjectDataDirectoriesEnum.pictures,
                                                                                      DirectoryUtil.ProjectDataDirectoriesModEnum.bundle) + dirName + ".zip";

                if (!File.Exists(zippedFileName))
                {
                    var zip = new ZipFile();
                    zip.AddFiles(pictures, "");
                    zip.Save(zippedFileName);
                }

                screen.SaveAnswerToQuestion(new Answer {QuestionNo = toSave, Response = zippedFileName, QuestionType = QuestionTypeEnum.PhotoBundle});
            }
            finally
            {
                if (Directory.Exists(dirAbsolutePath)) FileUtil.DeleteFolderContent(dirAbsolutePath);

                overlayBitmap?.Dispose();
            }
        }

        /// <summary>
        /// Créer un vidéo à partir de la séquence d'images.
        /// </summary>
        /// <param name="screen">Écran Photobooth appelant.</param>
        /// <param name="imageFiles">Fichiers d'images.</param>
        /// <param name="sc">Configuration d'écran variable.</param>
        /// <param name="options">Options de transformation.</param>
        [PublicAPI]
        public static void CreateVideo(IAbstractScreen screen, IList<string> imageFiles, ScreenConfig sc,
            ImageListTransformOptionsVideo options = null)
        {
            if (imageFiles == null || !imageFiles.Any() || screen == null || sc == null) return;

            var opt = options ?? new ImageListTransformOptionsVideo();
            var toSave = opt.SaveToQuestion ?? sc.QuestionVideoNo ?? -1;
            if (toSave == -1) return;

            // On redimensionne les images au besoin.
            var videoFrames = new List<FileInfo>();
            var modFiles = new List<FileInfo>(); // Détruire les fichiers modifiés une fois le traitement fait.
            var workDir = new DirectoryInfo(Path.GetTempPath() + GenerateUniqueId.GetId() + Path.DirectorySeparatorChar);

            try
            {
                workDir.Create();
                foreach (var fileName in imageFiles)
                {
                    if (opt.NewSize.HasValue)
                    {
                        var modName = DirectoryUtil.CreateLocalTemporaryFile(GenerateUniqueId.GetId() + ".png");
                        using (var image = FileUtil.LoadBitmap(fileName))
                        {
                            using (var b = image.Frame(opt.NewSize.Value.Width, opt.NewSize.Value.Height))
                            {
                                if (!b.Save(ImageFormat.Png, 5, modName)) { throw new System.Exception("Could not save temporary file"); }
                            }
                        }
                        var file = new FileInfo(modName);
                        videoFrames.Add(file);
                        modFiles.Add(file);
                    }
                    else { videoFrames.Add(new FileInfo(fileName)); }
                }

                // On créer le vidéo.
                var maskSpace = FfmpegUtil.PrepareDirectoryForImageListVideoCreation(videoFrames, workDir, opt.Iterations, opt.Loopback);

                var videoFileNameFinal = DirectoryUtil.GetLocalIbrandTerminalDataDir(sc.ProjectId, sc.QuestionnaireId, sc.TerminalId,
                                                                                          DirectoryUtil.ProjectDataDirectoriesEnum.videos,
                                                                                          DirectoryUtil.ProjectDataDirectoriesModEnum.none) + GenerateUniqueId.GetId() + ".mp4";

                var config = ServicesContainer.GetService<NomadConfiguration>();

                var mask = $@"%{maskSpace}d.png";
                FfmpegUtil.CreateVideoFromPictureFiles(config.FfmpegLocation, workDir, mask, videoFileNameFinal, opt.FilesPerSecond, opt.FrameRate, 0);

                if (!File.Exists(videoFileNameFinal)) { log.Error("Could not create video file: files={0}, mask={1}, config={2}", videoFrames, mask, opt); }
                else { screen.SaveAnswerToQuestion(new Answer{QuestionNo = toSave, Response = videoFileNameFinal}); }
            }
            finally
            {
                // On nettoie les fichiers et répertoires temporaires.
                foreach (var file in modFiles)
                {
                    file.Delete();
                }
                FileUtil.DeleteFolderContent(workDir.FullName);
            }
        }

        /// <summary>
        /// Créer une image GIF à partir de la séquence d'image.
        /// </summary>
        /// <param name="screen">Écran Photobooth appelant.</param>
        /// <param name="imageFiles">Fichiers d'images.</param>
        /// <param name="sc">Configuration d'écran variable.</param>
        /// <param name="options">Options de transformation.</param>
        [PublicAPI]
        public static void CreateAnimatedGif(IAbstractScreen screen, IList<string> imageFiles, ScreenConfig sc,
            ImageListTransformOptionsGif options = null)
        {
            if (imageFiles == null || !imageFiles.Any() || screen == null || sc == null) return;

            var opt = options ?? new ImageListTransformOptionsGif();
            var toSave = opt.SaveToQuestion ?? sc.QuestionGifNo ?? -1;
            if (toSave == -1) return;

            var overlay = opt.OverlaySelector?.GetImageFrame(screen.GetCurrentRespondentWithAnswers());

            var finalGif = DirectoryUtil.GetLocalIbrandTerminalDataDir(sc.ProjectId, sc.QuestionnaireId, sc.TerminalId,
                DirectoryUtil.ProjectDataDirectoriesEnum.pictures, DirectoryUtil.ProjectDataDirectoriesModEnum.gif) + GenerateUniqueId.GetId() + ".gif";

            if (ImageUtil.CreateGif(imageFiles, finalGif, opt.Repeat, opt.Loopback, opt.Quality, opt.Delay, overlay, opt.Size, opt.Crop))
            {
                screen.SaveAnswerToQuestion(new Answer{QuestionNo = toSave, Response = finalGif});
            }
        }

        /// <summary>
        /// Créer une image d'impression à partir de la séquence d'images.
        /// </summary>
        /// <param name="screen">Écran Photobooth appelant.</param>
        /// <param name="imageFiles">Fichiers d'images.</param>
        /// <param name="sc">Configuration d'écran variable.</param>
        /// <param name="options">Options de transformation.</param>
        /// <param name="specificCreatedImagePath">Chemin précis où l'on veut mettre le fichier résultant.</param>
        [PublicAPI]
        public static void CreatePictureToPrint(IAbstractScreen screen, IList<string> imageFiles, ScreenConfig sc,
            ImageListTransformOptionsPrint options = null, string specificCreatedImagePath = null)
        {
            if (imageFiles == null || !imageFiles.Any() || screen == null || sc == null) return;

            var opt = options ?? new ImageListTransformOptionsPrint();
            var toSave = opt.SaveToQuestion ?? sc.QuestionPictureToPrintNo ?? -1;

            var frame = opt.PrintFrameSelector?.GetImageFrame(screen.GetCurrentRespondentWithAnswers());
            if (frame == null) return;

            var configuration = ServicesContainer.GetService<NomadConfiguration>();

            var finalImagePath = specificCreatedImagePath ??
                                 DirectoryUtil.GetLocalIbrandTerminalDataDir(sc.ProjectId, sc.QuestionnaireId, sc.TerminalId,
                DirectoryUtil.ProjectDataDirectoriesEnum.pictures, DirectoryUtil.ProjectDataDirectoriesModEnum.print) + GenerateUniqueId.GetId() + ".jpg";

            var imagesToPrint = imageFiles;
            var nbrCol = (int)Math.Ceiling(Math.Sqrt(imageFiles.Count));

            if (opt.UseOnlyFirstPicture)
            {
                nbrCol = 1;
                imagesToPrint = new List<string> { imageFiles.First() };
            }

            using (var frameBitmap = FileUtil.LoadBitmap(frame))
            {

                Bitmap final = null;

                try
                {
                    if (opt.UseFullFramePicture)
                    {
                        using (var firstPicture = FileUtil.LoadBitmap(imageFiles.First()))
                        {
                            using (var resized = firstPicture.Frame(frameBitmap.Width, frameBitmap.Height))
                            {
                                final = BitmapExtentions.CreateFramedPicture(frameBitmap, resized, 
                                    opt.OffsetExtraSide, opt.OffsetExtraTop, opt.FrameOver, opt.BackgroundColor, opt.RotateFlip);
                            }
                        }
                    }
                    else
                    {
                        final = frameBitmap.PutMultiplePictureOnFrame(imagesToPrint, opt.OffsetPerc, nbrCol,
                            opt.OffsetExtraTop, opt.OffsetExtraSide + configuration.PrintPictureOffsetX,
                            opt.FrameOver, opt.BackgroundColor, opt.RotateFlip);
                    }

                    if (final.Save(opt.JpegSaveQuality, finalImagePath))
                    {
                        if (toSave > 0)
                        {
                            screen.SaveAnswerToQuestion(new Answer {QuestionNo = toSave, Response = finalImagePath});
                        }
                    }
                    else
                    {
                        log.Error("Could not save print image with following options: file={0}, options={1}", finalImagePath, opt);
                    }
                }
                finally
                {
                    final?.Dispose();
                }
            }
        }
    }
}
