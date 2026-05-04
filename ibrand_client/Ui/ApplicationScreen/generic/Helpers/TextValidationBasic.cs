using NomadLogic.Common.Util;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers
{
    public class TextValidationBasic : ITextValidation
    {
        public TextValidationBasicEnum Option = TextValidationBasicEnum.None;

        public bool Validate(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return false;

            switch (Option)
            {
                case TextValidationBasicEnum.Email: return StringUtil.IsValidEmail(text);
                case TextValidationBasicEnum.Sms: return StringUtil.IsValidPhoneNumber(text);
                case TextValidationBasicEnum.Name: return true;
                case TextValidationBasicEnum.Integer: return int.TryParse(text, out _);
                case TextValidationBasicEnum.Float: return float.TryParse(text, out _);
                case TextValidationBasicEnum.None: return true;
                case TextValidationBasicEnum.PostalCodeCanada: return StringUtil.IsValidCanadaPostalCode(text);
                case TextValidationBasicEnum.PostalCodeUs: return StringUtil.IsValidUSPostalCode(text);
                default: return true;
            }
        }

        public enum TextValidationBasicEnum
        {
            None,
            Email,
            Sms,
            Name,
            Integer,
            PostalCodeCanada,
            PostalCodeUs,
            Float
        }
    }
}