using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers.ScreenDefinitions.MultimediaEventListing
{
    [PublicAPI]
    [JsonConverter(typeof(StringEnumConverter))]
    public enum MultimediaEventListingStateEnum
    {
        ProcessingData,
        WaitingForData,
        NoData,
        Ready,
        Error
    }
}