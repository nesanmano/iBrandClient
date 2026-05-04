namespace NomadLogic.App.EventVideoCapture.Video
{
    public class VideoTreatmentResult
    {
        /// <summary>
        /// Indique que le traitement est complet et final.
        /// </summary>
        public bool Complete { get; set; }

        public VideoTreatmentResultEnum State { get; set; }

        /// <summary>
        /// Message à afficher dans la console de l'application.
        /// </summary>
        public string UiMessage { get; set; }
    }

    public enum VideoTreatmentResultEnum
    {
        Unknown,
        Complete,
        ServerDidNotReceive,
        InvalidFile,
        ServerUnreachable,
        ThirdPartyFileUploadFailed,
        FileSystemBusy,
        ThirdPartyAuthFailed,
        VideoTransformationFailed,
        RejectedByThirdParty
    }
}