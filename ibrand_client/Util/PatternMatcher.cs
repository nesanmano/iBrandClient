using NomadLogic.Common.Util;
using NomadLogic.iBrandDesktop.Exception;

namespace NomadLogic.iBrandDesktop.Util
{
    public class PatternMatcher
    {
        public const int PostalCode = 1;
        public const int EmailAdr = 2;
        public const int Canon = 3;
        public const int Nikon = 4;
        public const int UsPostalCode = 5;
        public const int PhoneNumber = 6;
        public const int CanadaPostalCode = 7;
        public const int Integer = 8;
        public const int MinuteTime = 9;

        public static bool Match(int pattern, string text)
        {
            bool match;

            switch (pattern)
            {
                case PostalCode:
                    match = MatchPostalCode(text);
                    break;
                case EmailAdr:
                    match = MatchEmail(text);
                    break;
                case Canon:
                    match = MatchCanon(text);
                    break;
                case Nikon:
                    match = MatchNikon(text);
                    break;
                case UsPostalCode:
                    match = MatchUsPostalCode(text);
                    break;
                case CanadaPostalCode:
                    match = MatchCanadaPostalCode(text);
                    break;
                case PhoneNumber:
                    match = MatchPhoneNumber(text);
                    break;
                case Integer:
                    match = MatchInteger(text);
                    break;
                case MinuteTime:
                    match = MatchTime(text);
                    break;
                default:
                    throw new UnsuportedPatternException();
            }

            return match;
        }

        /// <summary>
        /// Exemples: 1:56, 23:12
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private static bool MatchTime(string text)
        {
            return StringUtil.IsValidMinuteTime(text);
        }

        //h3e 1e2 | h3e-1e2 | 12345 | 1234
        private static bool MatchPostalCode(string code)
        {
            if (StringUtil.IsValidCanadaPostalCode(code))
                return true;

            return StringUtil.IsValidUSPostalCode(code);
        }

        private static bool MatchCanadaPostalCode(string code)
        {
            return StringUtil.IsValidCanadaPostalCode(code);
        }

        //819 333 4545, 819-333-4545
        private static bool MatchPhoneNumber(string number)
        {
            return StringUtil.IsValidPhoneNumber(number);
        }

        private static bool MatchUsPostalCode(string code)
        {
            return StringUtil.IsValidUSPostalCode(code);
        }

        private static bool MatchInteger(string integer)
        {
            return StringUtil.IsInteger(integer);
        }

        //IMG_1234.jpg
        private static bool MatchCanon(string picture)
        {
            return StringUtil.IsValidCanonPictureNumber(picture);
        }

        //DSC_1234.jpg
        private static bool MatchNikon(string picture)
        {
            return StringUtil.IsValidNikonPictureNumber(picture);
        }

        private static bool MatchEmail(string emailAdr)
        {
            return StringUtil.IsValidEmail(emailAdr);
        }

        public static string GetPattern(int pattern)
        {
            string patternForm;

            switch (pattern)
            {
                case PostalCode:
                    patternForm = " H7Y 1A1, H7Y-1A1";
                    break;
                case EmailAdr:
                    patternForm = " name@domain.org";
                    break;
                case Canon:
                case Nikon:
                    patternForm = " 1234";
                    break;
                case UsPostalCode:
                    patternForm = " 12345";
                    break;
                case PhoneNumber:
                    patternForm = " 819 333 4545, 819-333-4545";
                    break;
                case CanadaPostalCode:
                    patternForm = " H2K 1X1";
                    break;
                case Integer:
                    patternForm = " 24";
                    break;
                case MinuteTime:
                    patternForm = "999:59";
                    break;
                default:
                    throw new UnsuportedPatternException();
            }

            return patternForm;
        }
    }
}
