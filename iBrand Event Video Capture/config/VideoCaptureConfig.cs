using System;
using Newtonsoft.Json;
using NomadLogic.App.EventVideoCapture.Capture;
using NomadLogic.App.EventVideoCapture.Reader;
using NomadLogic.App.EventVideoCapture.Video.Workers;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Json;
using NomadLogic.Common.Util.RfidReader;
using NomadLogic.Common.Util.Trigger;

namespace NomadLogic.App.EventVideoCapture.config
{
    public class VideoCaptureConfig : IJsonConfig, IExternalFileConfig
    {
        public string ProjectDescription { get; set; } = "TEST Config (Empty)";
        public int NomadProjectId { get; set; } = -1;
        public int NomadQuestionnaireId { get; set; } = -1;
        public int NomadTerminalId { get; set; } = -1;

        [JsonIgnore]
        public string GetNomadIdsStr => $"{NomadProjectId}/{NomadQuestionnaireId}/{NomadTerminalId}";

        public string LocationKeyName { get; set; } = Environment.MachineName;

        /// <summary>
        /// Nom de la classe de travail individuelle.
        /// Doit implémenter la classe VideoTreatmentAbstract
        /// </summary>
        public string WorkerClassFullName { get; set; } = typeof(VideoTreatmentAquaparc2021).AssemblyQualifiedName;
        
        public string ReaderClassFullName { get; set; } = typeof(ReaderManagerStandAlone).AssemblyQualifiedName;

        public string TriggerClassFullName { get; set; } = typeof(TriggerManagerFormKeyboard).AssemblyQualifiedName;

        public string CaptureClassFullName { get; set; } = typeof(CaptureFfmpegScript).AssemblyQualifiedName;
        
        public string LightClassFullName { get; set; }
        
        public string DefaultConfigFileName { get; } = "video_trigger_app_config.json";
        
        public string ConfigFileName { get; set; }

        /// <summary>
        /// Tranche de temps maximal pour la capture d'un vidéo.
        /// Par défaut: 60 secondes.
        /// </summary>
        public TimeSpan MaxCaptureTimeAllowed { get; set; } = TimeSpan.FromSeconds(60);

        /// <summary>
        /// Délais entre réception du trigger et départ de la capture.
        /// </summary>
        public int DelayForCaptureAfterTrigger { get; set; } = 500;

        /// <summary>
        /// Délais entre un travailleur et sa ré-entrée quand un problème temporaire est détectée.
        /// </summary>
        public TimeSpan RequeueDelay { get; set; } = TimeSpan.FromSeconds(5);
    }
}