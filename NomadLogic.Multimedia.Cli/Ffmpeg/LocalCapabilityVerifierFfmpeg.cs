using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using NomadLogic.Common.Services;
using NomadLogic.Common.Services.Configuration;
using NomadLogic.Common.Services.LocalCapabilities;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Json;

namespace NomadLogic.Multimedia.Cli.Ffmpeg
{
    public class LocalCapabilityVerifierFfmpeg : LocalCapabilityVerifierAbstract
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        
        protected override CapabilityStatusGroupEnum GetCapabilityEnumType()
        {
            return CapabilityStatusGroupEnum.Ffmpeg;
        }

        protected override void ExecuteDo(CapabilityStatus capStatus, List<CapabilityStatus> previousTests)
        {
            var config = ServicesContainer.GetService<NomadConfiguration>();
            
            // Si on a un rapport sur disque, on prend celui-là plutôt que de refaire un test.
            var path = new FileInfo(DirectoryUtil.GetLocalApplicationDir(DirectoryUtil.ApplicationNameEnum.Nomad) + "FfmpegCapabilitiesReport.json");
            if (path.Exists) {
                JsonUtil.UpdateFromJson(path, capStatus);
                return;
            }
            
            var ffmpegExecPath = config.FfmpegLocation;
            if (!File.Exists(ffmpegExecPath))
            {
                capStatus.Status = StatusEnum.Missing;
                capStatus.ErrorMessages.Add($"Executable is missing: '{ffmpegExecPath}'");
            }
            else
            {
                if (!CheckPreviousTests(previousTests, CapabilityStatusGroupEnum.Assets))
                {
                    log.Warn("Cannot evaluate FFMPEG capabilities because assets are missing.");
                    capStatus.ErrorMessages.Add("Assets are missing. Skipping FFMPEG transformation check. Check application logs for details.");
                    capStatus.Status = StatusEnum.Pending;
                    return;
                }
                
                // Essaie de faire du chroma keying sur un fichier d'image. Si le fichier résultant n'existe pas, c'est que
                // la version de FFMPEG est insuffisante.
                {
                    var currentFunction = new SubCapabilityStatus
                    {
                        FunctionName = "Chroma key"
                    };
                    capStatus.SubFunctionsCapabilities.Add(currentFunction);

                    var testFile = DirectoryUtil.GetLocalNomadLogicAssetsDir() + "not_keyed.jpg";
                    var tmpFile = DirectoryUtil.CreateLocalTemporaryFile(GenerateUniqueId.GetId() + ".jpg");
                    var fileInfo = new FileInfo(tmpFile);

                    currentFunction.Status = StatusEnum.Invalid;
                    try
                    {
                        var chroma = FfmpegChromaKeyInfo.NewInstanceChromaKeyGreen();
                        FfmpegUtil.ChromakeyImage(ffmpegExecPath, testFile, tmpFile, chroma);

                        if (!fileInfo.Exists || fileInfo.Length == 0)
                        {
                            capStatus.ErrorMessages.Add("FFmpeg could not chroma key a test image.");
                        }
                        else
                        {
                            currentFunction.Status = StatusEnum.Valid;
                        }
                    }
                    // ReSharper disable once EmptyGeneralCatchClause
                    catch
                    {
                        capStatus.ErrorMessages.Add("FFmpeg could not chroma key a test image. Exception during test.");
                    }
                    finally
                    {
                        fileInfo.Refresh();
                        if (fileInfo.Exists)
                        {
                            fileInfo.Delete();
                        }
                    }
                }

                // Tous les test ont réussis
                capStatus.Status = capStatus.SubFunctionsCapabilities.All(f => f.Status == StatusEnum.Valid) ? StatusEnum.Valid : StatusEnum.Invalid;
                
                // On écrit sur disque uniquement si le test est réussi.
                if (capStatus.Status == StatusEnum.Valid)
                {
                    JsonUtil.WriteJsonFile(path, capStatus);
                }
            }
        }
    }
}