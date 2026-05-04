using Newtonsoft.Json;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers
{
    [JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
    public enum UserErrorEnum
    {
        Mandatory,
        Empty,
        Validation,
        System
    }
}