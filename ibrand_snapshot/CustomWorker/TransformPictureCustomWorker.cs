using NomadLogic.iBrand.Common.Util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using NLog;
using NomadLogic.iBrand.Common;
using NomadLogic.iBrand.Common.Respondent;
using NomadLogic.iBrand.Common.Transformation.image;

namespace iBrand_SnapShot.CustomWorker
{
    internal class TransformPictureCustomWorker : CustomWorker<SnapShotCustomWorkerParameter, FileInfo>
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        private string _galleryDir;
        private string _archiveDir;
        private string _assetDir;

        public override void DoPreparationWork()
        {
            GetTransformationProperties();
        }

        public override void DoWork()
        {
            var picture = Parameter.WorkFile.FullName;
            var transformedPicture = picture + "_trans.jpg";

            log.Debug("TransformPictureCustomWorker DoWork");

            if (!File.Exists(transformedPicture))
            {
                try
                {
                    var controller = new TransformationsController();
                    controller.Initialize("picture_transformation_config.xml");

                    var respondent = new RespondentImpl("", _assetDir, 0); 
                    respondent.AddAnswer(new Answer(-1 , 1, 1, Parameter.WorkFile.FullName));

                    var transformation = controller.GetTransformation(respondent, false);
                    Bitmap transformedPictureBitmap = null;
                    transformedPictureBitmap = controller.TransformPicture(respondent, false);
                    transformedPictureBitmap.Save(transformedPicture, ImageFormat.Jpeg);
                    controller.Dispose();
                    GC.Collect();
                }
                catch (Exception ex)
                {
                    log.Fatal(ex, "Error while Transforming pictures");
                } 
            }

            if (File.Exists(transformedPicture))
            {

                try
                {
                    var tPict = _galleryDir + StringUtil.GetFileName(transformedPicture);
                    if (File.Exists(tPict))
                    {
                        File.Delete(tPict);
                    }
                    File.Move(transformedPicture, tPict);

                    var oPict = _archiveDir + StringUtil.GetFileName(picture);
                    if (File.Exists(oPict))
                    {
                        File.Delete(oPict);
                    }
                    File.Move(picture, oPict);
                }
                catch (Exception e)
                {
                    log.Info(e, "Problem Moving picture file.");
                }
            }
        }

        private void GetTransformationProperties()
        {
            var path = "config.txt";
            if (File.Exists(path))
            {
                var data = new Dictionary<string, string>();
                string temp;
                foreach (var row in File.ReadAllLines(path))
                {
                    var tab = row.Split('=');
                    var rest = string.Concat(tab.Skip(1).ToArray());
                    data.Add(tab[0], rest);
                }

                if (data.TryGetValue("gallery_dir", out temp))
                {
                    _galleryDir = temp;
                }


                
                if (data.TryGetValue("log_all", out temp))
                {
                    try
                    {
                        //log.en = bool.Parse(temp);
                    }
                    finally
                    {
                        //die nice
                    }
                }


                if (data.TryGetValue("archive_dir", out temp) && Directory.Exists(temp))
                {
                    try
                    {
                        _archiveDir = temp;
                    }
                    finally
                    {
                        //die nice
                    }
                }

                
                if (data.TryGetValue("asset_dir", out temp) && Directory.Exists(temp))
                {
                    try
                    {
                        _assetDir = temp;
                    }
                    finally
                    {
                        //die nice
                    }
                }
            }
            else
            {
                if (LoggerReference == null)
                {
                    log.Info("Could not find config file for picture transformation.");
                }
                else
                {
                    LoggerReference.LogEvent("Could not find config file for picture transformation.");
                }
            }
        }
    }
}
