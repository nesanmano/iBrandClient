using System.IO;
using System.Windows.Forms;
using NLog;
using NomadLogic.Common.Services;
using NomadLogic.Common.Services.Configuration;
using NomadLogic.Common.Util;

namespace NomadLogic.iBrandDesktop.Ui
{
    class BackUp
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        readonly string _videoFile;
        private readonly int _pId;
        private readonly int _qId;
        private readonly int _tId;

        public BackUp(int projectId, int questionnaireId, int terminalId, string videoFile)
        {
            _videoFile = videoFile;
            _pId = projectId;
            _qId = questionnaireId;
            _tId = terminalId;
        }

        public void DoWork()
        {
            try
            {
                var configuration = ServicesContainer.GetService<NomadConfiguration>();

                //Save database file
                var backupDbFileName = DirectoryUtil.GetBackupDir(configuration) + StringUtil.GetFileName(configuration.Ibrand.DbIbrandFileName);

                if (File.Exists(backupDbFileName))
                    File.Delete(backupDbFileName);

                File.Copy(configuration.Ibrand.DbIbrandFileName, backupDbFileName);
                //Save video file
                if (!Directory.Exists(DirectoryUtil.GetBackupDir(configuration) + "videos"))
                    Directory.CreateDirectory(DirectoryUtil.GetBackupDir(configuration) + "videos");

                var backupVideoFileName = DirectoryUtil.GetBackupDir(configuration) + "videos" + Path.DirectorySeparatorChar + _videoFile;

                if (File.Exists(backupVideoFileName))
                    File.Delete(backupVideoFileName);

                File.Copy(DirectoryUtil.GetLocalIbrandTerminalDataDir(_pId, _qId, _tId, DirectoryUtil.ProjectDataDirectoriesEnum.videos, DirectoryUtil.ProjectDataDirectoriesModEnum.none)
                          + _videoFile, backupVideoFileName);

            }
            catch (System.Exception ex)
            {
                log.Fatal(ex);
                MessageBox.Show(@"Problems while saving backup.", @"iBrand (tm)", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
            }
        }
    }
}