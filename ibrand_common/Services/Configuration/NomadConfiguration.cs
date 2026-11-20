using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using NomadLogic.Common.Serveur;
using NomadLogic.Common.Services.LocalCapabilities;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Json;

namespace NomadLogic.Common.Services.Configuration
{
    /// <summary>
    /// Configurations utiles pour toutes les applications de NomadLogic.
    /// </summary>
    [DataContract]
    [PublicAPI]
    public class NomadConfiguration : IService
    {
        /// <summary>
        /// Lit sur disque la location habituelle du fichier de configuration. Si le fichier n'existe pas, on en configure un nouveau et il sera
        /// écrit sur disque.
        /// Le résultat de la lecture de ce fichier ou d'une nouvvelle instance sera retournée, et initialisée avec le type d'application qui a demandé l'appel.
        /// </summary>
        /// <param name="applicationThatCalls">L'énumération de l'application qui appelle.</param>
        /// <returns>L'instance de configuration.</returns>
        public static NomadConfiguration GetConfigurationInstanceFromDisk(DirectoryUtil.ApplicationNameEnum applicationThatCalls)
        {
            NomadConfiguration nomadConfig;
            var nomadConfigFileInfo = new FileInfo(DirectoryUtil.GetLocalApplicationDir(DirectoryUtil.ApplicationNameEnum.Nomad) + "nomadlogic.config");
            if (!nomadConfigFileInfo.Exists)
            {
                nomadConfig = new NomadConfiguration();
                nomadConfig.InitWithDefaultValue();
                SaveConfigurationInstanceToDisk(nomadConfig);
            }
            else
            {
                nomadConfig = JsonUtil.ReadJsonFile<NomadConfiguration>(nomadConfigFileInfo);
            }
            nomadConfig.ApplicationName = applicationThatCalls;
            return nomadConfig;
        }

        /// <summary>
        /// Sauvegarde l'instance actuelle sur disque.
        /// </summary>
        /// <param name="configuration"></param>
        public static void SaveConfigurationInstanceToDisk(NomadConfiguration configuration)
        {
            if (configuration == null) return;
            var configFile = new FileInfo(DirectoryUtil.GetLocalApplicationDir(DirectoryUtil.ApplicationNameEnum.Nomad) + "nomadlogic.config");
            JsonUtil.WriteJsonFile(configFile, configuration);
        }

        /// <summary>
        /// Configurations propres à l'application Ibrand
        /// </summary>
        [DataMember]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public IbrandConfiguration Ibrand { get; set; }

        /// <summary>
        /// Code de langue active pour notre application.
        /// </summary>
        [DataMember]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public string PreferedLanguage { get; set; } = "en";
        
        /// <summary>
        /// Indique s'il faut envoyer automatiquement les répondants une fois validés.
        /// </summary>
        [DataMember]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public bool LiveSynchro { get; set; } = true;

        /// <summary>
        /// Détruit les fichiers associés à un répondant après un synchronisation réussie.
        /// </summary>
        [DataMember]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public bool DeletePictureAfterSynchro { get; set; }

        /// <summary>
        /// Le nom du matériel sur lequel roule l'application. Ne peut pas être modifié.
        /// </summary>
        [DataMember]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public string DeviceName => "Desktop";

        /// Location de l'exécutable FFMPEG
        [DataMember]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public string FfmpegLocation { get; set; } = 
            DirectoryUtil.GetLocalNomadLogicDir() + 
            DirectoryUtil.ApplicationNameEnum.Nomad + Path.DirectorySeparatorChar +
            "lib" + Path.DirectorySeparatorChar + 
            "ffmpeg"+ Path.DirectorySeparatorChar + 
            "bin"+ Path.DirectorySeparatorChar + "ffmpeg.exe";

        /// Location de l'exécutable FFPROBE
        [DataMember]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public string FfprobeLocation { get; set; } = 
            DirectoryUtil.GetLocalNomadLogicDir() + 
            DirectoryUtil.ApplicationNameEnum.Nomad + Path.DirectorySeparatorChar +
            "lib" + Path.DirectorySeparatorChar + 
            "ffmpeg"+ Path.DirectorySeparatorChar + 
            "bin"+ Path.DirectorySeparatorChar + "ffprobe.exe";
        
        /// <summary>
        /// Location ddes librairies dynamiques de VLC
        /// </summary>
        [DataMember]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public string VlcLibsLocation { get; set; } =
            DirectoryUtil.GetLocalNomadLogicDir() + 
            DirectoryUtil.ApplicationNameEnum.Nomad + Path.DirectorySeparatorChar +
            "lib" + Path.DirectorySeparatorChar +
            "vlc_dot_net" + Path.DirectorySeparatorChar;
        
        /// <summary>
        /// Nom de l'ordinateur.
        /// </summary>
        [DataMember]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public string PcName => Environment.MachineName;

        /// <summary>
        /// Lettre du répertoire où écrire les fichiers destinés à être mis en back-up.
        /// </summary>
        [DataMember]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public string BackupDrive { get; set; } = "F:";

        /// <summary>
        /// Nom de l'application qui a instancié cette instance. C'est en somme le nom de l'application actuelle.
        /// </summary>
        [DataMember]
        [JsonIgnore]
        public DirectoryUtil.ApplicationNameEnum ApplicationName { get; set; }

        /// <summary>
        /// Nom de la caméra de laquelle il faut faire des captures vidéo.
        /// </summary>
        [DataMember]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public string WebCamName { get; set; }

        /// <summary>
        /// Nom du device de capture audio.
        /// </summary>
        [DataMember]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public string AudioInputName { get; set; }

        /// <summary>
        /// Déplacement de l'imagesélectionner lors de la composition d'image d'impression.
        /// Sert à contrebalancer l'incertitude liée au couteau de certaines imprimantes quand il faut couper le papier.
        /// Certaines décales légèrement, et ce'est une configuration purement locale.
        /// </summary>
        [DataMember]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public int PrintPictureOffsetX { get; set; }

        /// <summary>
        /// Dit aux application qui utilisent un clavier visuel s'ils doivent oui ou non l'afficher.
        /// </summary>
        [DataMember]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public bool ShowKeyboard { get; set; }

        /// <summary>
        /// Contient un lien vers le rapport de capacités matérielles.
        /// </summary>
        [DataMember]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public CapabilitiesReport CapabilitiesReport { get; set; }

        /// <summary>
        /// Indique s'il faut utiliser Ffmpeg au lieu de fonction plus avancée comme OpenCv dans le traitement d'éléments multimédia.
        /// </summary>
        [DataMember]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public bool ForceUsageOfFfmpeg { get; set; }

        /// <summary>
        /// Indique l'endroit où l'exécutable de VLC se situe. Est nul par défaut, et doit être manuellement
        /// entré si on ré-initialise le fichier de configuration.
        /// </summary>
        [DataMember]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public string VlcLocation { get; set; }

        /// <summary>
        /// Indique l'endroit où l'exécutable de Image Magick se situe.
        /// </summary>
        [DataMember]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public string ImageMagickLocation { get; set; } = @"C:\Program Files\ImageMagick-7.0.7-Q16\magick.exe";

        /// <summary>
        /// Configuration pour parler au serveur principal.
        /// </summary>
        [DataMember]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public IDictionary<ServerConfigurationSwitch, ServerConfiguration> ServerInformations { get; set; }

        /*
        [DataMember]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        */
        [JsonIgnore]
        public ServerConfiguration CurrentServerConfiguration
        {
            get
            {
                if (!ServerInformations?.ContainsKey(CurrentServerConfigurationKey) ?? true)
                {
                    InitServerConfig();
                }

                if (ServerInformations.ContainsKey(CurrentServerConfigurationKey))
                {
                    return ServerInformations[CurrentServerConfigurationKey];
                }

                return new ServerConfiguration
                {
                    Identifier = new ServerUserIdentifier
                    {
                        Password = "Unknown",
                        Username = "Unknown"
                    },
                    Protocol = ServerConfigurationProtocol.Http,
                    Port = 0,
                    Address = "Unknown",
                    LocalPPC = "Unknown"
                };
            }
        }

        [DataMember]
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public ServerConfigurationSwitch CurrentServerConfigurationKey { get; set; } = ServerConfigurationSwitch.Prod;

        // TODO À enlever OPC
        [Obsolete]
        public PrintstationConfiguration Printstation { get; set; }

        /// <summary>
        /// Taille du dialogue de mot de passe. Par défaut: 1000x600
        /// </summary>
        [DataMember]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public Size PasswordDialogSize { get; set; } = new Size(1000, 600);

        /// <summary>
        /// Ratio de la taille de fonte par rapport à l'espace disponible du dialogue de mot de passe.
        /// </summary>
        [DataMember]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public float PasswordDialogKeyboardFontRatio { get; set; } = 0.075f;

        /// <summary>
        /// Répertoire de base pour les installation manuelles.
        /// </summary>
        [DataMember]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public string ManualInstallInputDir { get; set; } = DirectoryUtil.GetLocalNomadLogicDir() + "install" + Path.DirectorySeparatorChar;

        /// <summary>
        /// Nom (pas le chemin) de la base de données pour stocker les informations d'activité.
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        [DataMember]
        public string DbActivityFileName { get; set; } = "activity_db.db3";

        /// <summary>
        /// Teste si un groupe de tests pour capacités matérielles est valide.
        /// </summary>
        /// <param name="group">Le groupe à tester.</param>
        /// <returns>TRUE si le groupe est valide. Il faut bien sûr avoir créé le rapport auparavant.</returns>
        public bool IsCapabilityReportGroupValid(CapabilityStatusGroupEnum group)
        {
            return CapabilitiesReport?.Groups.Find(cs => cs.Name == group && cs.Status == StatusEnum.Valid) != null;
        }

        /// <summary>
        /// Initialize avec des configurations de base.
        /// </summary>
        public void InitWithDefaultValue()
        {
            Ibrand = new IbrandConfiguration();
            Ibrand.InitWithDefaultValue();

            InitServerConfig();
        }

        // TODO Trouver une autre manière de stocker l'Information de login...
        private void InitServerConfig()
        {
            if (ServerInformations != null && ServerInformations.Any()) return;

            ServerInformations = new Dictionary<ServerConfigurationSwitch, ServerConfiguration>
            {
                {
                    ServerConfigurationSwitch.Prod, new ServerConfiguration
                    {
                        Address = "msl-juju-backend-gudqamb3fxa2h5dw.canadacentral-01.azurewebsites.net",
                        Protocol = ServerConfigurationProtocol.Http,
                        Port = 80,
                        LocalPPC = PcName,
                        Identifier = new ServerUserIdentifier
                        {
                            Username = "devTeam@nomadlogic.com",
                            Password = "Bmkop877-"
                        }
                    }
                },
                {
                    ServerConfigurationSwitch.Preprod, new ServerConfiguration
                    {
                        Address = "192.95.36.173",
                        Protocol = ServerConfigurationProtocol.Http,
                        Port = 8081,
                        LocalPPC = PcName,
                        Identifier = new ServerUserIdentifier
                        {
                            Username = "devTeam@nomadlogic.com",
                            Password = "Bmkop877-"
                        }
                    }
                },
                {
                    ServerConfigurationSwitch.Dev, new ServerConfiguration
                    {
                        Address = "192.95.36.173",
                        Protocol = ServerConfigurationProtocol.Http,
                        Port = 8081,
                        LocalPPC = PcName,
                        Identifier = new ServerUserIdentifier
                        {
                            Username = "root",
                            Password = "Bmkop877-"
                        }
                    }
                },
                {
                    ServerConfigurationSwitch.Local, new ServerConfiguration
                    {
                        Address = "192.95.36.173",
                        Protocol = ServerConfigurationProtocol.Http,
                        Port = 8081,
                        LocalPPC = PcName,
                        Identifier = new ServerUserIdentifier
                        {
                            Username = "devTeam@nomadlogic.com",
                            Password = "Bmkop877-"
                        }
                    }
                },
                {
                    ServerConfigurationSwitch.DevAWS, new ServerConfiguration
                    {
                        Address = "juju.nomadlogic-app.com",
                        Protocol = ServerConfigurationProtocol.Https,
                        Port = 443,
                        LocalPPC = PcName,
                        Identifier = new ServerUserIdentifier
                        {
                            Username = "devTeam@nomadlogic.com",
                            Password = "1234"
                        }
                    }
                }
            };
        }

        /// <inheritdoc />
        public void Dispose()
        {
            // ne fait rien
        }

        public bool Init()
        {
            return true; // ne fait rien
        }

        public override string ToString()
        {
            return this.ToStringPublicProperties();
        }
    }
}
