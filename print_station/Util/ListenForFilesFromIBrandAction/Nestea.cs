using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Ionic.Zip;
using NLog;
using NomadLogic.Common.Extentions;
using NomadLogic.Common.Services;
using NomadLogic.Common.Services.Configuration;
using NomadLogic.Common.Util;
using NomadLogic.Multimedia.Cli.Ffmpeg;

// ReSharper disable StringIndexOfIsCultureSpecific.1

namespace NomadLogic.PrintStation.Util.ListenForFilesFromIBrandAction
{
    public class Nestea : AbstractAction
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private const string FICHIER_COMPRESS = "config_compress.bin";

        public override void DoWork()
        {
            if (!File.Exists(FileName)) return;

            // Validation du fichier: si ce n'est pas un fichier ZIP, on détruit le fichier, pour ne pas avoir à le refuser une nouvelle fois.
            if (!FileName.EndsWith(".zip") && !FileName.EndsWith(".ZIP"))
            {
                File.Delete(FileName);
                return;
            }

            // Étape 1: création du répertoire de travail, et extraction du fichier.
            var dirName = DirectoryUtil.GetLocalApplicationWorkDir() +
                          StringUtil.RemoveExtension(StringUtil.GetFileName(FileName));

            try
            {
                if (Directory.Exists(dirName))
                {
                    Directory.Delete(dirName, true);
                }

                using (var zip = ZipFile.Read(FileName))
                {
                    foreach (var e in zip)
                    {
                        e.Extract(dirName);
                    }
                }
            }
            catch (Exception e)
            {
                log.Error(e, "Could not extract files from zip.");
                File.Delete(FileName);
                return;
            }

            var id = GenerateUniqueId.GetId(); // Identifiant de ce groupe de fichiers

            var tmpGif = dirName + Path.DirectorySeparatorChar + id + ".gif";
            var tmpPng = dirName + Path.DirectorySeparatorChar + id + ".png";
            var tmpAvi = dirName + Path.DirectorySeparatorChar + id + ".avi";
            var tmp3Gp = dirName + Path.DirectorySeparatorChar + id + ".3gp";

            try
            {
                var gifFileName =
                    DirectoryUtil.GetLocalPrintstationMediaDir(User.Id,
                        DirectoryUtil.NomadLogicPrintstationDirectoriesEnum.pictures) + id + ".gif";
                var pngFileName =
                    DirectoryUtil.GetLocalPrintstationMediaDir(User.Id,
                        DirectoryUtil.NomadLogicPrintstationDirectoriesEnum.pictures) + id + ".png";
                var threegpFileName =
                    DirectoryUtil.GetLocalPrintstationMediaDir(User.Id,
                        DirectoryUtil.NomadLogicPrintstationDirectoriesEnum.pictures) + id + ".3gp";

                if (File.Exists(gifFileName)) File.Delete(gifFileName);
                if (File.Exists(pngFileName)) File.Delete(pngFileName);
                if (File.Exists(threegpFileName)) File.Delete(threegpFileName);

                string overlayFile;

                if (dirName.IndexOf("_Q6C1S1") != -1)
                    overlayFile =
                        DirectoryUtil.GetLocalPrintstationMediaDir(User.Id,
                            DirectoryUtil.NomadLogicPrintstationDirectoriesEnum.ui) + "overlay_fr.png";
                else
                    overlayFile =
                        DirectoryUtil.GetLocalPrintstationMediaDir(User.Id,
                            DirectoryUtil.NomadLogicPrintstationDirectoriesEnum.ui) + "overlay_en.png";

                using (var overlayBitmap = FileUtil.LoadBitmap(overlayFile))
                {
                    ResizeAndAddOverlay(dirName, overlayBitmap);

                    CreateVideoFile(dirName, tmpAvi, overlayFile);

                    var configuration = ServicesContainer.GetService<NomadConfiguration>();

                    FfmpegUtil.AviTo3Gp(tmpAvi, tmp3Gp, configuration.FfmpegLocation);

                    if (!File.Exists(tmp3Gp))
                    {
                        log.Error("Could not create 3GP video file.");
                    }

                    //Create Gif
                    CreateAnimatedGifAndSaveFirstFile(dirName, tmpGif, tmpPng);

                    if (!File.Exists(tmpGif))
                    {
                        log.Error("Could not create GIF file.");
                    }

                    var files = Directory.GetFiles(dirName);
                    if (files.Length > 0)
                    {
                        var file = files[0];
                        PrintStationUtil.CreatePostFromFile(pngFileName, file, Database, User);
                    }
                }

                File.Move(tmp3Gp, threegpFileName);
                File.Move(tmpGif, gifFileName);
                File.Move(tmpPng, pngFileName);
            }
            catch (Exception e)
            {
                log.Error(e, "Could create post from zip.");
            }
            finally
            {
                // Au cas où les fichiers temporaires existent toujours
                File.Delete(tmp3Gp);
                File.Delete(tmpAvi);
                File.Delete(tmpPng);
                File.Delete(tmpGif);
                // Destruction du fichier original et du répertoire temporaire.
                File.Delete(FileName);
                Directory.Delete(dirName, true);
            }
        }

        private static void CreateVideoFile(string dir, string resultFileName, string overlayBitmap)
        {
            /*
            var transformation = new TransformationAvi();
            var files = Directory.GetFiles(dir, "*.png");
            if (files.Length <= 0) return;
            var compressionFile = DirectoryUtil.GetLocalApplicationDir() + FICHIER_COMPRESS;
            try
            {
                transformation.TransformFilesToVideo(files, 500, resultFileName, "Nestea", compressionFile,
                    overlayBitmap, 2, 30, false, null);
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
            */
        }

        private static void ResizeAndAddOverlay(string dir, Bitmap overlayBitmap)
        {
            var files = FileUtil.GetFiles(dir, "*.jpg|*.jpeg");

            if (files.Length < 2)
            {
                throw new Exception("Not enough image files in ZIP.");
            }

            foreach (var fileName in files)
            {
                Bitmap image = null;
                Bitmap b = null;

                try
                {
                    image = FileUtil.LoadBitmap(fileName);

                    if (image.Width != 720)
                    {
                        var tempImage = image.Resize(720, 480);
                        image.Dispose();
                        image = tempImage;
                    }

                    b = image.Resize(image.Height, image.Height);

                    b.AddOverlay(overlayBitmap);
                    b.Save(ImageFormat.Png, 9, StringUtil.RemoveExtension(fileName) + ".png");
                }
                finally
                {
                    image?.Dispose();
                    b?.Dispose();
                }
            }
        }

        private static void CreateAnimatedGifAndSaveFirstFile(string dir, string resultFileName, string pngFileName)
        {
            /*
            var a = new AnimatedGifEncoder();
            a.Start(resultFileName);
            a.SetDelay(350);
            //-1:no repeat,0:always repeat
            a.SetRepeat(0);
            a.SetQuality(1);

            var files = Directory.GetFiles(dir, "*.png");

            var firstFileSaved = false;

            foreach (var image in files.Select(Image.FromFile))
            {
                if (!firstFileSaved)
                {
                    //image.Save(jpg_file_name, ImageFormat.Png);
                    var b = new Bitmap(image);
                    ImageUtil.SaveToImageFileWithOptions(b, ImageFormat.Png, 9, pngFileName);
                    b.Dispose();
                    firstFileSaved = true;
                }

                a.AddFrame(image);


                image.Dispose();
                image.Dispose();
            }

            a.Finish();
            */
        }
    }
}