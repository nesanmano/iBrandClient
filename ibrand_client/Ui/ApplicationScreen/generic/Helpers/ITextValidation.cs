using Newtonsoft.Json;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers
{
    [JsonConverter(typeof(TextValidationConverter))]
    public interface ITextValidation
    {
        bool Validate(string text);
    }
}