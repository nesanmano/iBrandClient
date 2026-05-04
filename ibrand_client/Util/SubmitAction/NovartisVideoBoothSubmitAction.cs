using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using NLog;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Services;
using NomadLogic.Common.Services.Configuration;
using NomadLogic.Common.Util;
using NomadLogic.iBrandDesktop.Util.SubmitAction.BaseClasses;

namespace NomadLogic.iBrandDesktop.Util.SubmitAction
{
    /// <summary>
    /// Classe de transformation de photo. Requiert un fichier de transformation appellé "picture_transformation_config.xml" dans le répertoire
    /// du questionnaire.
    /// </summary>
    // ReSharper disable once ClassNeverInstantiated.Global
    public class NovartisVideoBoothSubmitAction : AbstractSubmitActionPictureTransformation
    {
        // ReSharper disable once UnusedMember.Local
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        protected override void InstanciateSubmitActionPools()
        {
            AddSubmitActionWorkerPool("Transformation", 1);
        }

        protected override void ValidateConfigData()
        {
            var conf = ServicesContainer.GetService<NomadConfiguration>();
            if (DirectoryUtil.IsBackupDirAvailiable(conf)) return;

            var errText = "Back Up directory is not availiable. Please ensure this drive can be reached: " + conf.BackupDrive;
            //log.Error(errText);
            MessageBox.Show(Resources.iBrand.BackupDirNotAvailiable + $@" ({conf.BackupDrive})", Resources.iBrand.Copyright);
            throw new System.Exception(errText);
        }

        public override IList<Bitmap> TransformPictures(IRespondent resp, bool preview)
        {
            return PictureTransformationController.TransformPictures(resp, preview);
        }

        protected override ISubmitActionWorker CreateWorker(Respondent resp, bool usePool = true)
        {
            return new NovartisVideoBoothSubmitActionWorker(this, resp, resp.Id);
        }
    }
}
