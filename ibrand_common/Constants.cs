using System;
using NomadLogic.Common.Util;

namespace NomadLogic.Common
{
    [Obsolete("Code Smell. Ne pas utiliser de classe statique pour ce genre d'information...")]
    public static class Constants
    {
        public const string DEFAULT_LOGO_FILE = "default_logo.bmp";

        public const string FR = "fr";
        public const string EN = "en";

        public static readonly string[] SUPPORTED_LANGUAGES = { FR, EN };

        public const int QUESTION_PAD = 4;
        public const int CHOICE_PAD = 4;
        public const int QUESTION_MARGIN = 3;
        public const int CHOICE_MARGIN = 3;
        public const int QUESTION_TEXT_MARGIN = 4;

        public const string TWITTER_APPS_ID = "ItX9dbau5llTA7H22wYJQ";
        public const string TWITTER_SECRET = "XLRlkhATdmWgPYBzY2PTi37Rddt0oIK6dlMDWas3gn8";

        public const int MAX_WIDTH = 500;

        // ReSharper disable once InconsistentNaming
        public static string APP_DIR => DirectoryUtil.GetLocalApplicationDir();
    }
}
