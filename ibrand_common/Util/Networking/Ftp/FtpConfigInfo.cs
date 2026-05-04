using System;
using System.IO;
using Newtonsoft.Json;

namespace NomadLogic.Common.Util.Networking.Ftp
{
    /// <summary>
    /// Classe journalisable pour les configurations standard d'un système FTP.
    /// </summary>
    public class FtpConfigInfo
    {
        /// <summary>
        /// URL de connection. Doit inclure le protocole ("ftp" ou "sftp")
        /// </summary>
        [JsonRequired] public string Url { get; set; } = "sftp://127.0.0.1";

        /// <summary>
        /// Nom d'usager pour la connexion.
        /// </summary>
        [JsonRequired] public string User { get; set; } = "local_test";

        /// <summary>
        /// Mot de passe de l'usager.
        /// </summary>
        [JsonRequired] public string Password { get; set; } = "1234";

        /// <summary>
        /// Temps de recherche maximal (incluant transfert).
        /// </summary>
        public TimeSpan MaxSearchTime { get; set; } = TimeSpan.FromSeconds(5);

        /// <summary>
        /// Port de connexion. Habituellement 22 pour SFTP et 21 pour FTP. 22 par défaut.
        /// </summary>
        public int Port { get; set; } = 22;

        /// <summary>
        /// Pour des connexions sécurisée via SSH.
        /// Chemin complet ou relatif (juste le nom) pour le fichier qui contient la clé privée.
        /// </summary>
        public string SshPrivateKeyFilePath { get; set; }

        /// <summary>
        /// Pour des connexions sécurisée via SSH.
        /// Chemin complet ou relatif pour le fichier qui contient la clé privée.
        /// Variante pour référence directe sur fichier.
        /// </summary>
        [JsonIgnore]
        public FileInfo SshPrivateKeyFileInfo { get; set; }

        /// <summary>
        /// Empreinte publique du serveur distant. Si cette empreinte est différente, la connexion sera refusé de notre côté.
        /// </summary>
        public string SshHostFingerprint { get; set; }

        /// <summary>
        /// Contenu texte de la clé SSH, au besoin.
        /// </summary>
        public string SshPrivateKeyData { get; set; }
    }
}