using System;
using System.Linq;
using System.Windows.Forms;
using JetBrains.Annotations;
using NLog;
using NomadLogic.Common.Services;
using NomadLogic.Common.Services.Configuration;
using NomadLogic.Common.Util;
using NomadLogic.iBrandDesktop.Util.SubmitAction.BaseClasses;
using NomadLogic.iBrandDesktop.Util.SubmitAction.Generic.Worker;

namespace NomadLogic.iBrandDesktop.Util.SubmitAction.Generic
{
    [PublicAPI]
    public class CopyToBackUpSubmitAction : AbstractSubmitAction, ISubmitActionCopyToBackUp
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        protected override void InstanciateSubmitActionPools()
        {
            AddSubmitActionWorkerPool("BackupFilePool", 5);
        }

        public bool OptionalBackup { get; private set; } = true;
        public int[] QuestionsFileToBackUp { get; private set; }
        public bool UseDirectoryTree => DirectoryTreeToken != null;
        public string DirectoryTreeToken { get; private set; }
        public string FileNameToken { get; private set; }

        /// <summary>
        /// Cette classe vérifie si toutes les configurations externes critiques sont disponibles.
        /// </summary>
        protected override void ValidateConfigData()
        {
            var conf = ServicesContainer.GetService<NomadConfiguration>();

            if (!DirectoryUtil.IsBackupDirAvailiable(conf))
            {
                var errText = "Back Up directory is not availiable. Please ensure this drive can be reached: " + conf.BackupDrive;
                log.Error(errText);
                MessageBox.Show(Resources.iBrand.Copyright, Resources.iBrand.BackupDirNotAvailiable + $@" ({conf.BackupDrive})");
            }

            if (_configData != null && _configData.ContainsKey("question_no_backup"))
            {
                QuestionsFileToBackUp =  StringUtil.GetDataFromDelimitedString(_configData["question_no_backup"], StringUtil.STRING_DELIMITER_CHARS);
            }

            if (_configData != null && _configData.ContainsKey("optional_backup"))
            {
                OptionalBackup = bool.Parse(_configData["optional_backup"]);
            }

            if (!OptionalBackup && !QuestionsFileToBackUp.Any()) throw new SystemException("No valid question(s) number set up for mandatory backup of files.");

            DirectoryTreeToken = _configData != null && !_configData.ContainsKey("bkp_dir_tree") ? _configData["bkp_dir_tree"] : null;
            
            FileNameToken = _configData != null && !_configData.ContainsKey("bkp_dir_tree") ? _configData["bkp_dir_tree"] : null;
        }

        protected override ISubmitActionWorker CreateWorker(Respondent resp, bool usePool = true)
        {
            return new CopyToBackUpSubmitActionWorker(this, resp, resp.Id);
        }
    }
}
