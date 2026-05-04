using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Threading;
using NLog;
using NomadLogic.Common;
using NomadLogic.Common.Extentions;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Services;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Networking.Ftp;
using NomadLogic.iBrandDesktop.Services;
using NomadLogic.iBrandDesktop.Util.SubmitAction.BaseClasses;

namespace NomadLogic.iBrandDesktop.Util.SubmitAction
{
    [SuppressMessage("ReSharper", "NotAccessedField.Local")]
    class IrisLunetteSubmitActionWorker : ISubmitActionWorker
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private readonly Respondent _respondent;
        private readonly IrisLunetteSubmitAction _action;
        private readonly int _respIdD;

        public IrisLunetteSubmitActionWorker(IrisLunetteSubmitAction action, Respondent resp, int respId)
        {
            _respondent = resp;
            _action = action;
            _respIdD = respId;
        }

        public void DoWork(CancellationToken? ct)
        {
            if (ct?.IsCancellationRequested ?? false) return;

            var db = ServicesContainer.GetService<IbrandDatabase>();

            try
            {
                /**
                 * On commence par la normalisation du fichier visuel, basé sur les réponses obtenue.
                 * Si la question deux est répondue, alors ce traitement à déjà eu lieu.
                 */
                var files = new string[2];

                files[0] = _respondent.GetAnswerText(2);
                files[1] = _respondent.GetAnswerText(7);

                if (files[0] == null)
                {
                    files[0] = CreateNormalizedImage(db, 1, 3, 4, 11, 5, 2, "_face", _action.CenterCoordinateFace);
                }
                if (files[1] == null)
                {
                    // On vérifie si la question de profil existe, car la photo de profil est optionnelle.
                    if (_respondent.GetAnswerText(6) != null)
                    {
                        files[1] = CreateNormalizedImage(db, 6, 8, 9, 12, 10, 7, "_profil", _action.CenterCoordinateProfil);
                    }
                }

                /**
                 * On fait maintenant la portion FTP, c'est à dire envoyer le fichier sur un FTP. 
                 */

                if (!File.Exists(files[0]))
                {
                    log.Error("Inexistent answer to upload for respondent: " + _respondent.Id);
                }
                else
                {

                    FileInfo[] fi = { new FileInfo(files[0]), files[1] == null ? null : new FileInfo(files[1]) };

                    var ftp = new FtpPool(_action.FtpAdress, _action.FtpPort, _action.FtpUser, _action.FtpPassword);
                    var rep = DateTime.Now.ToString("yyyy_MM_dd", CultureInfo.GetCultureInfo("fr-CA"));
                    if (_action.FtpDir != null)
                    {
                        rep = _action.FtpDir + "/" + rep;
                    }
                    var ftpPath = ftp.GetUploadPath(rep + "/face", files[0]);
                    if (!ftp.CheckFtpDirectoryPath(ftpPath))
                    {
                        throw new System.Exception("Invalid file path.");
                    }
                    //FtpWebUtil.UploadFile(fi[0], ftpPath, ftp.WebSetup);

                    if (File.Exists(files[1]))
                    {
                        ftpPath = ftp.GetUploadPath(rep + "/side", files[1]);
                        if (!ftp.CheckFtpDirectoryPath(ftpPath))
                        {
                            throw new System.Exception("Invalid file path.");
                        }
                        //FtpWebUtil.UploadFile(fi[1], ftpPath, ftp.WebSetup);
                    }

                    db.SetRespondentReadyToSync(_respIdD);

                }
            }
            catch (System.Exception ex)
            {
                log.Fatal(ex, "Error while executing treatment of respondent.");

                _action.UiSynchro?.AddUiMessage("Error while treating respondent.", 4);
            }
        }

        public ISubmitActionWorker NextWorkerStep { get; set; }

        private string CreateNormalizedImage(IbrandDatabase db, int questionImage, int questionPoint1, 
            int questionPoint2, int questionPointMedian, int questionDistance, int questionResult, string suffix, Point? center)
        {
            Bitmap normalized = null;
            Bitmap cropped = null;

            string resultFile;

            try
            {
                var imageFile = _respondent.GetAnswerText(questionImage);
                var point1Resp = _respondent.GetAnswerText(questionPoint1);
                var point2Resp = _respondent.GetAnswerText(questionPoint2);
                var point3Resp = _respondent.GetAnswerText(questionPointMedian);
                var occularDistanceResp = _respondent.GetAnswerText(questionDistance);

                var occDist = int.Parse(occularDistanceResp);

                var p1 = StringUtil.PointFromStringDefinition(point1Resp);
                var p2 = StringUtil.PointFromStringDefinition(point2Resp);
                Point? p3 = null;

                if (point3Resp != null)
                {
                    p3 = StringUtil.PointFromStringDefinition(point3Resp);
                }

                // On produit une image qui a reçue une rotation, un redimensionnement et une translation pour se normaliser par rapport à un point.
                normalized = new Bitmap(imageFile).NormalizeToStaticCoordinate(p1, p2, p3, occDist, center,
                    _action.PixelMilimeterValue, _action.AngleMinThreshold);

                cropped = normalized.Crop(550, 550);

                var finalFileName = DirectoryUtil.GetLocalIbrandTerminalDataDir(_action.TerminalInfo,
                                        DirectoryUtil.ProjectDataDirectoriesEnum.pictures, DirectoryUtil.ProjectDataDirectoriesModEnum.transformed) + GenerateUniqueId.GetId() + suffix + ".png";

                // DEBUG, à enlever
                //ImageUtil.SaveToImageFileWithOptions(normalized, ImageFormat.Png, 9, final_file_name + ".png");

                if (cropped.Save(ImageFormat.Png, 9, finalFileName))
                {
                    var answer = new Answer{QuestionType = QuestionTypeEnum.PhotoText, QuestionNo = questionResult, Response = finalFileName};
                    db.InsertAnswer(_respondent.Id, answer);
                    resultFile = finalFileName; // Pour la portion FTP du traitement.
                }
                else
                {
                    throw new System.Exception("Could not save image to file: " + finalFileName);
                }
            }
            finally
            {
                normalized?.Dispose();
                cropped?.Dispose();
            }
            return resultFile;
        }

        public ISubmitAction GetSubmitAction()
        {
            return _action;
        }

        public int GetRespId()
        {
            return _respIdD;
        }
    }
}