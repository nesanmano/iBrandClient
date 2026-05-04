using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Threading;
using Ionic.Zip;
using NLog;
using NomadLogic.Common;
using NomadLogic.Common.Extentions;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Services;
using NomadLogic.Common.Services.Configuration;
using NomadLogic.Common.Util;
using NomadLogic.iBrandDesktop.Services;
using NomadLogic.iBrandDesktop.Util.SubmitAction.BaseClasses;
using NomadLogic.Multimedia.Cli.Ffmpeg;
using NomadLogic.Multimedia.Gpu.OpenCV.TransformFromJson;

namespace NomadLogic.iBrandDesktop.Util.SubmitAction
{
    public class KiaCameraRing : AbstractSubmitAction
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        protected override void InstanciateSubmitActionPools()
        {
            AddSubmitActionWorkerPool("transformation_kia", 1);
        }

        public override bool SubmitRespondent(Respondent respondent)
        {
            try
            {
                var db = ServicesContainer.GetService<IbrandDatabase>();

                var answerBarcode = respondent.Answers[0];

                var barcodes = answerBarcode.Response.Split(':');

                foreach (var barcodeAnswer in barcodes.Select(barcode => new Answer{QuestionType = QuestionTypeEnum.Text, QuestionNo = 16, Response = barcode}))
                {
                    respondent.Answers[0] = barcodeAnswer;
                    var respId = db.InsertRespondent(respondent, false);

                    // Il faut que ça soit une instance différente par Worker. Très important.
                    var tmpResp = db.GetRespondent(respId);
                    var temp = CreateWorker(tmpResp);
                    _submitActionWorkerPools[0].AddWorker(temp);
                }

                RespondentSaved();

                return true;
            }
            catch (System.Exception ex)
            {
                log.Fatal(ex.Message);
                return false;
            }
        }

        protected override ISubmitActionWorker CreateWorker(Respondent resp, bool usePool = true)
        {
            return new KiaCameraRingWorker(this, resp, resp.Id);
        }

        private class KiaCameraRingWorker : SubmitActionWorkerAbstract<KiaCameraRing>
        {
            private const int QUESTION_FIRST_PICTURE = 19;
            private const int QUESTION_VIDEO = 21;
            private const int QUESTION_BUNDLE = 18;
            private const string FICHIER_COMPRESS = "config_compress.bin";

            public KiaCameraRingWorker(KiaCameraRing action, Respondent resp, int respId) : base(action, resp, respId)
            {
                // Rien de plus
            }
            private void CreateVideoFile(IReadOnlyList<FileInfo> pictures, string resultFileName)
            {
                /*
                var transformation = new TransformationAvi();

                var compressionFile = DirectoryUtil.GetLocalIbrandQuestionnaireDir(_action.ProjectId, int.Parse(_action.QuestionnaireId)) + FICHIER_COMPRESS;
                try
                {
                    var files = new string[pictures.Count];

                    for (var i = 0; i < pictures.Count; i++)
                    {
                        files[i] = pictures[i].FullName;
                    }

                    transformation.TransformFilesToVideo(files, 100, resultFileName, "Kia 2016", compressionFile, null, 2, 15, false, null);
                }
                catch (System.Exception ex)
                {
                    log.Error(ex);
                    throw;
                }
                */
            }

            private static bool TransformsFromJson(IEnumerable<FileInfo> pictures, IRespondent respondent, string dirToSave, Size? cropTo)
            {
                var fileName = DirectoryUtil.GetLocalIbrandTerminalDir(respondent.ProjectId, respondent.QuestionnaireId, respondent.TerminalId) + "transforms.json";

                if (!File.Exists(fileName))
                {
                    log.Fatal("JSON file for pictures normalization is missing!");
                    return false;
                }

                var jsonSerializer = new DataContractJsonSerializer(typeof(FileBundleNormalizationTransforms));

                Stream fs = File.OpenRead(fileName);

                var objResponse = jsonSerializer.ReadObject(fs);
                var transforms = objResponse as FileBundleNormalizationTransforms;

                if (transforms == null)
                {
                    log.Fatal("Transform file not found or cannot be read as JSON: " + fileName);
                    return false;
                }

                var globalParameters = transforms.GlobalParameters;

                foreach (var picture in pictures)
                {
                    var temp = StringUtil.GetFileName(picture.FullName);
                    var number = temp.Substring(0, temp.IndexOf(".", StringComparison.Ordinal));

                    var pictureNumber = int.Parse(number);

                    var pictureTranform = transforms.PicturesTransform[pictureNumber - 1];

                    var resFactor = 0.37;

                    using (var image = FileUtil.LoadBitmap(picture.FullName, 72f))
                    {
                        var result = new Bitmap(image.Width, image.Height, PixelFormat.Format32bppArgb);
                        result.SetResolution(image.HorizontalResolution, image.VerticalResolution);
                        using (var graphics = Graphics.FromImage(result))
                        {
                            //Set the resize quality modes to high quality
                            graphics.CompositingQuality = CompositingQuality.HighQuality;
                            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                            graphics.SmoothingMode = SmoothingMode.HighQuality;

                            graphics.DrawImage(image, -(int)(pictureTranform.Offset.X * resFactor), -(int)(pictureTranform.Offset.Y * resFactor));
                            graphics.Dispose();
                        }

                        var rotatedResult = result.Rotate(-pictureTranform.Angle, (float)resFactor * 2587, (float)resFactor * 1368);

                        var scaledResult = result.Resize((int)(image.Width * pictureTranform.Scale), (int)(image.Height * pictureTranform.Scale));

                        var croppedResult = scaledResult.Crop((int)(globalParameters.MinCrop.X * resFactor), (int)(globalParameters.MinCrop.Y * resFactor));

                        var final = croppedResult;
                        if (cropTo != null)
                        {
                            final = croppedResult.Frame(cropTo.Value.Width, cropTo.Value.Height);
                        }

                        final.Save(ImageFormat.Jpeg, 90,
                                    dirToSave + Path.DirectorySeparatorChar +
                                    StringUtil.GetFileName(picture.FullName));
                        final.Dispose();

                        croppedResult.Dispose();
                        scaledResult.Dispose();
                        rotatedResult.Dispose();
                        result.Dispose();
                    }
                }

                return true;
            }

            public override void DoWork(CancellationToken? ct)
            {
                if (ct?.IsCancellationRequested ?? false) return;

                if (_action._stopped) return;

                try
                {
                    var db = ServicesContainer.GetService<IbrandDatabase>();

                    _respondent.Answers = db.GetAnswers(_respondent.Id);

                    var config = ServicesContainer.GetService<NomadConfiguration>();

                    var dirName = _respondent.GetAnswerText(QUESTION_BUNDLE, 1); //Question photo bundle

                    if (!Directory.Exists(dirName))
                    {
                        log.Warn("Bundle directory does not exists: " + dirName);
                        return;
                    }

                    var di = new DirectoryInfo(dirName);

                    var files = di.EnumerateFiles("*.jpg");

                    var orderedPictureList = files.ToList();

                    orderedPictureList.Sort(SortFunction);

                    var transformDir = dirName.Substring(0, dirName.Length - 1) + "_t";

                    Directory.CreateDirectory(transformDir);

                    string zippedFileName;
                    string[] transformedPictures;

                    if (TransformsFromJson(orderedPictureList, _respondent, transformDir, new Size(640, 480)))
                    {
                        zippedFileName = transformDir + ".zip";
                        transformedPictures = Directory.GetFiles(transformDir);
                    }
                    else
                    {
                        zippedFileName = dirName.Substring(0, dirName.Length - 1) + ".zip";
                        transformedPictures = Directory.GetFiles(dirName);
                        Directory.Delete(transformDir);
                        transformDir = null;
                    }

                    if (!File.Exists(zippedFileName))
                    {
                        var zip = new ZipFile();

                        zip.AddFiles(transformedPictures, "");
                        zip.Save(zippedFileName);
                    }

                    var photoBundleAnswer = new Answer{QuestionType = QuestionTypeEnum.PhotoBundle, QuestionNo = QUESTION_BUNDLE, Response = zippedFileName};

                    //Create Mov
                    var id = GenerateUniqueId.GetId();

                    var aviFileName =
                        DirectoryUtil.GetLocalMediaDir(DirectoryUtil.NomadLogicMediaDirectoriesEnum.videos) +
                         id + ".avi";

                    var movFileName =
                        DirectoryUtil.GetLocalMediaDir(DirectoryUtil.NomadLogicMediaDirectoriesEnum.videos) +
                        id + ".mov";


                    di = transformDir != null ? new DirectoryInfo(transformDir) : new DirectoryInfo(dirName);

                    files = di.EnumerateFiles();

                    var orderedList = files.ToList();
                    orderedList.Sort(SortFunction);

                    CreateVideoFile(orderedList, aviFileName);

                    FfmpegUtil.AviToMov(aviFileName, movFileName, config.FfmpegLocation);

                    var movAnswer = new Answer{QuestionType = QuestionTypeEnum.Video, QuestionNo = QUESTION_VIDEO, Response = movFileName};

                    var firstPictureName =
                        DirectoryUtil.GetLocalMediaDir(DirectoryUtil.NomadLogicMediaDirectoriesEnum.pictures) +
                        GenerateUniqueId.GetId() + ".jpg";

                    File.Copy(transformedPictures[0], firstPictureName);

                    var firstPictureAnswer = new Answer{QuestionType = QuestionTypeEnum.Photo, QuestionNo = QUESTION_FIRST_PICTURE, Response = firstPictureName};

                    CropAndMoveToDirectory(transformDir ?? dirName);

                    db.InsertAnswer(_respId, photoBundleAnswer);

                    db.InsertAnswer(_respId, movAnswer);

                    db.InsertAnswer(_respId, firstPictureAnswer);

                    db.SetRespondentReadyToSync(_respId);

                    _action.NewRespondentEventHandle?.Set();
                }
                catch (System.Exception ex)
                {
                    log.Fatal(ex, "Error while doing Kia Camera ring submit action.");

                    _action.UiSynchro?.AddUiMessage(
                        "Error while Treating camera ring pictures for respondent id=" + _respId, 4);
                }
                finally
                {
                    _action.OnWorkFinished(_respId);
                }
            }

            protected override ISubmitActionWorker NextWorkerInstance(SubmitActionWorkerNextStepInfo next)
            {
                throw new NotImplementedException();
            }

            protected override void DoWorkExecute(IbrandDatabase db, CancellationToken? ct)
            {
                throw new NotImplementedException();
            }

            private static int SortFunction(FileInfo file1, FileInfo file2)
            {
                var fileFullName1 = file1.FullName;

                var fileName1 = StringUtil.GetFileName(fileFullName1);
                var cameraNo = StringUtil.RemoveExtension(fileName1);

                var no1 = -1;

                if (StringUtil.IsInteger(cameraNo))
                    no1 = int.Parse(cameraNo);

                var fileFullName2 = file2.FullName;

                var fileName2 = StringUtil.GetFileName(fileFullName2);
                cameraNo = StringUtil.RemoveExtension(fileName2);

                var no2 = -1;

                if (StringUtil.IsInteger(cameraNo))
                    no2 = int.Parse(cameraNo);

                if (no1 != -1 && no2 != -1)
                    return no1.CompareTo(no2);

                return string.Compare(fileName1, fileName2, StringComparison.Ordinal);
            }

            /// <summary>
            /// Copie les fichiers vers le répertore de ShowPictureOnTv par défaut.
            /// </summary>
            /// <param name="picturesDir"></param>
            private static void CropAndMoveToDirectory(string picturesDir)
            {
                var files = Directory.GetFiles(picturesDir);

                var dir = DirectoryUtil.GetLocalShowPicturesOnTvPicturesDir(StringUtil.GetLastDirName(picturesDir));

                foreach (var file in files)
                {
                    var fileNameOnly = StringUtil.GetFileName(file);
                    var filePath = Path.Combine(dir, fileNameOnly);
                    File.Copy(file, filePath);
                }
            }
        }
    }
}
