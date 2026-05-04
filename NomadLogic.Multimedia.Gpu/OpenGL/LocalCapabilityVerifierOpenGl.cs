using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using JetBrains.Annotations;
using NLog;
using NomadLogic.Common.Services.LocalCapabilities;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Json;

namespace NomadLogic.Multimedia.Gpu.OpenGL
{
    [PublicAPI]
    public class LocalCapabilityVerifierOpenGl : LocalCapabilityVerifierAbstract, ILocalCapabilityVerifierForm
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        protected override CapabilityStatusGroupEnum GetCapabilityEnumType()
        {
            return CapabilityStatusGroupEnum.OpenGlAnalysis;
        }

        protected override void ExecuteDo(CapabilityStatus capStatus, List<CapabilityStatus> previousTests)
        {
            if (ParentForm == null)
            {
                capStatus.Status = StatusEnum.Invalid;
                if (capStatus.ErrorMessages == null)
                {
                    capStatus.ErrorMessages = new List<string>(); 
                }
                capStatus.ErrorMessages.Add("No parent form given. Test is invalid. Check initialization sequence of capabilities tests.");
                return;
            }
            
            var cts = new CancellationTokenSource();
            
            // S'il y a déjà un rapport écrit, on ne refait pas le test OpenGL.
            var path = new FileInfo(DirectoryUtil.GetLocalApplicationDir(DirectoryUtil.ApplicationNameEnum.Nomad) + "OpenGlAnalysisReport.json");
            if (path.Exists) {
                JsonUtil.UpdateFromJson(path, capStatus);
                return;
            }

            // Tests pour OpenCV
            var openGlExtensions = new OpenGLSubCapabilities();
            
            try
            {
                var finished = false;
                ParentForm.Invoke(new Action(() =>
                {
                    var test = new OpenGLCapabilitiesTestForm(openGlExtensions, cts, () =>
                    {
                        finished = true;
                    });
                    test.Show();
                }));
            
                // On attend sur le même thread que l'invocation UI termine.
                // ReSharper disable once ImplicitlyCapturedClosure
                var act = new Action(() =>
                {
                    while (!cts.IsCancellationRequested && !finished)
                    {
                        Thread.Sleep(100);
                    }
                });
                Task.Run(act, cts.Token).Wait(2000);
                
                capStatus.SubFunctionsCapabilities.Add(openGlExtensions);
                capStatus.Status = StatusEnum.Valid;
                JsonUtil.WriteJsonFile(path, capStatus); // Pour sauter l'analyse la prochaine fois, écrit le rapport de façon externe.
            }
            catch (Exception e)
            {
                openGlExtensions.Status = StatusEnum.Invalid;
                capStatus.SubFunctionsCapabilities.Add(openGlExtensions);
                capStatus.Status = StatusEnum.Invalid;
                capStatus.ErrorMessages.Add(e.Message);
            }
        }

        public Form ParentForm { get; set; }
    }
}