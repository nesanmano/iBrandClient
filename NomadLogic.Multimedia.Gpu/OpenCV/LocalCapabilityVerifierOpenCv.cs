using System.Collections.Generic;
using System.IO;
using System.Linq;
using Emgu.CV;
using NLog;
using NomadLogic.Common.Services.LocalCapabilities;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Json;

namespace NomadLogic.Multimedia.Gpu.OpenCV
{
    public class LocalCapabilityVerifierOpenCv : LocalCapabilityVerifierAbstract
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        
        protected override CapabilityStatusGroupEnum GetCapabilityEnumType()
        {
            return CapabilityStatusGroupEnum.OpenCv;
        }

        protected override void ExecuteDo(CapabilityStatus capStatus, List<CapabilityStatus> previousTests)
        {
            // Si on a un rapport sur disque, on prend celui-là plutôt que de refaire un test.
            var path = new FileInfo(DirectoryUtil.GetLocalApplicationDir(DirectoryUtil.ApplicationNameEnum.Nomad) + "OpenCvCapabilitiesReport.json");
            if (path.Exists) {
                JsonUtil.UpdateFromJson(path, capStatus);
                return;
            }

            if (!CheckPreviousTests(previousTests, CapabilityStatusGroupEnum.Assets))
            {
                log.Warn("Cannot evaluate OpenCv capabilities because assets are missing or are not tested.");
                capStatus.ErrorMessages.Add("Assets are missing Skipping OpenCv check. Check application logs for details.");
                capStatus.Status = StatusEnum.Pending;
                return;
            }
            
            try
            {
                // ReSharper disable once UnusedVariable
                var mat = new Mat();
                capStatus.Status = StatusEnum.Valid;
            }
            catch (System.Exception e)
            {
                log.Error(e);
                capStatus.Status = StatusEnum.Invalid;
                capStatus.ErrorMessages.Add("Failed to load OpenCV basic library. Check application logs for details.");
            }

            if (capStatus.Status == StatusEnum.Invalid)
            {
                JsonUtil.WriteJsonFile(path, capStatus);
                return;
            }

            // Tente de faire une détection de formes
            {
                var currentFunction = new SubCapabilityStatus
                {
                    FunctionName = "Shape detection"
                };
                capStatus.SubFunctionsCapabilities.Add(currentFunction);

                var testFile = DirectoryUtil.GetLocalNomadLogicAssetsDir() + "next_arrow.png";
                OpenCvUtil.DetectShapesInImage(new FileInfo(testFile), false, true, false);
                currentFunction.Status = StatusEnum.Valid;
            }

            capStatus.Status = capStatus.SubFunctionsCapabilities.All(f => f.Status == StatusEnum.Valid) ? StatusEnum.Valid : StatusEnum.Invalid;
            JsonUtil.WriteJsonFile(path, capStatus);
        }
    }
}