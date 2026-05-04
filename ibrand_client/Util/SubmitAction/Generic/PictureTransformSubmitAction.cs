using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using JetBrains.Annotations;
using NLog;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Services;
using NomadLogic.Common.Services.Configuration;
using NomadLogic.Common.Util;
using NomadLogic.iBrandDesktop.Util.SubmitAction.BaseClasses;

namespace NomadLogic.iBrandDesktop.Util.SubmitAction.Generic
{
    /// <summary>
    /// Classe de transformation de photo. Requiert un fichier de transformation appellé "picture_transformation_config.xml" dans le répertoire
    /// du questionnaire.
    /// </summary>
    [PublicAPI]
    public class PictureTransformSubmitAction : AbstractSubmitActionPictureTransformation
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        
        /// <inheritdoc />
        protected override void InstanciateSubmitActionPools()
        {
            AddSubmitActionWorkerPool("Transformation", 1);
        }

        /// <summary>
        /// Indique où mettre les fichiers modifiés, en plus du répertoire de base.
        /// </summary>
        public string DestinationFolder => _configData.ContainsKey("destination_folder") ? _configData["destination_folder"] : null;
        
        /// <summary>
        /// 
        /// </summary>
        public bool CopyToBackUpDirectory
        {
            get
            {
                if (_configData != null && _configData.ContainsKey("copy_to_backup"))
                {
                    return bool.Parse(_configData["copy_to_backup"]);
                }

                return false;
            }
        }

        /// <inheritdoc />
        protected override void ValidateConfigData()
        {
            if (!CopyToBackUpDirectory) return;
            var conf = ServicesContainer.GetService<NomadConfiguration>();
            if (DirectoryUtil.IsBackupDirAvailiable(conf)) return;

            var errText = "Back Up directory is not availiable. Please ensure this drive can be reached: " + conf.BackupDrive;
            log.Error(errText);
            MessageBox.Show(Resources.iBrand.Copyright, Resources.iBrand.BackupDirNotAvailiable + $@" ({conf.BackupDrive})");
        }

        /// <inheritdoc />
        public override IList<Bitmap> TransformPictures(IRespondent resp, bool preview)
        {
            return PictureTransformationController.TransformPictures(resp, preview);
        }

        /// <inheritdoc />
        protected override ISubmitActionWorker CreateWorker(Respondent resp, bool usePool = true)
        {
            return new Worker.PictureTransformSubmitActionWorker(this, resp, resp.Id);
        }
    }
}
