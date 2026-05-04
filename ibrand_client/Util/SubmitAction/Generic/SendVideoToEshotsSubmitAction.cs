using System.IO;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Json;
using NomadLogic.iBrandDesktop.Util.SubmitAction.BaseClasses;
using NomadLogic.iBrandDesktop.Util.SubmitAction.Generic.Worker;

namespace NomadLogic.iBrandDesktop.Util.SubmitAction.Generic
{
    public class SendVideoToEshotsSubmitAction : SendMultimediaToEshotsSubmitActionBase
    {
        protected override ISubmitActionWorker CreateWorker(Respondent resp, bool usePool = true)
        {
            var worker = new SendVideoToEshotsWorker(this, resp, resp.Id);

            if (VideoSettingsFile == null) return worker;

            var scriptFile = new FileInfo(GetTerminalDirectory + VideoSettingsFile);

            var settings = JsonUtil.ReadJsonFile<SubmitVideoTransformationSetting>(scriptFile);

            settings.BaseDirectory = new DirectoryInfo(GetTerminalDirectory);
            settings.OutputVideoDirectory = new DirectoryInfo(DirectoryUtil.GetLocalIbrandTerminalDataDir(TerminalInfo, DirectoryUtil.ProjectDataDirectoriesEnum.videos, DirectoryUtil.ProjectDataDirectoriesModEnum.none));
            settings.OutputThumbnailDirectory= new DirectoryInfo(DirectoryUtil.GetLocalIbrandTerminalDataDir(TerminalInfo, DirectoryUtil.ProjectDataDirectoriesEnum.videos, DirectoryUtil.ProjectDataDirectoriesModEnum.thumbnail));

            worker.VideoCreationSettings = settings;
            worker.VideoCreationMethod = SubmitActionCreateVideoUtil.StandardFfmpegScriptCommand;

            return worker;
        }

        public string VideoSettingsFile
        {
            get
            {
                if (_configData != null && _configData.ContainsKey("video_settings_json"))
                {
                    return _configData["video_settings_json"];
                }

                return null;
            }
        }

    }
}
