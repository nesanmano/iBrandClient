namespace GoPro.Hero
{
    public static class Configuration
    {
        public enum HttpRequestMode { Sync, Async };

        public static HttpRequestMode CommandRequestMode { get; set; }

        static Configuration()
        {
            CommandRequestMode = HttpRequestMode.Sync;
        }
    }
}
