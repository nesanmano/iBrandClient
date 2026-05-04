using GoPro.Hero;

namespace NomadLogic.Common.Util.Camera.GoProCam
{
    /// <summary>
    /// Classe de configuration pour un contrôleur GoPro.
    /// </summary>
    public class CameraConfigGoPro : ICameraConfig
    {
        /// <summary>
        /// Un code de deux caractères fournit par la caméra.
        /// </summary>
        public string Author { get; set; }
        /// <summary>
        /// Une valeur 0-255 du niveau de la batterie.
        /// </summary>
        public uint? BatteryLevel { get; set; }
        /// <summary>
        /// Aucun pour une GoPro.
        /// </summary>
        public string Copyright { get; set; }
        /// <summary>
        /// Aucune information de description pour une GoPro
        /// </summary>
        public string DeviceDescription { get; set; }
        /// <summary>
        /// La version du firmware de la caméra, format "Major.Minor.Build.Revision"
        /// </summary>
        public string FirmwareVersion { get; set; }
        /// <summary>
        /// Un chiffre identifiant le modèle de la caméra.
        /// </summary>
        public string ProductName { get; set; }
        /// <summary>
        /// Pas un numéro de série comme tel, mais on utilise l'addresse MAC pour un identifiant unique.
        /// </summary>
        public string SerialNumber { get; set; }

        /// <summary>
        /// La classe de configuration étendue de la caméra.
        /// Direct de l'API.
        /// </summary>
        public CameraExtendedSettings ExtendedSettings { get; set; }
        /// <summary>
        /// L'état générale de la caméra.
        /// Direct de l'API.
        /// </summary>
        public BacpacStatus BacpackStatus { get; set; }
        /// <summary>
        /// Informations fixes de la caméra.
        /// Direct de l'API.
        /// </summary>
        public BacpacInformation BacpackInformation { get; set; }
    }
}