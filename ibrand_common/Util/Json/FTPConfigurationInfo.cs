using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace NomadLogic.Common.Util.Json
{
    [Obsolete("Utiliser plutôt le version avec configuration de sécurité SSH.")]
    [PublicAPI]
    public class FtpConfigurationInfo : IJsonConfig
    {

        public string FtpServerIp { get; set; } = "69.70.163.38";

        public string Port { get; set; } = "51218";

        public string Username { get; set; } = "ibrand";

        public string Password { get; set; } = "1234";

        public IList<string> TokenizedFilesNames { get; set; }

        string IJsonConfig.DefaultConfigFileName => "ftp.config";


    }
}
