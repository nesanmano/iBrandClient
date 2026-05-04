namespace NomadLogic.Common.Util.Camera
{
    /// <summary>
    /// Cette interface indique les informations de base qui doivent être commune à toutes les caméras de différents manufacturiers.
    /// </summary>
    public interface ICameraConfig
    {
        /// <summary>
        /// L'identifiant de la caméra, que l'on peut modifier à loisir.
        /// </summary>
        string Author { get; set; }
        /// <summary>
        /// Le niveau de batterie de la caméra.
        /// </summary>
        uint? BatteryLevel { get; set; }
        /// <summary>
        /// Les droits acquis sur les photos prise par cet appareil.
        /// </summary>
        string Copyright { get; set; }
        /// <summary>
        /// Description de l'appareil.
        /// </summary>
        string DeviceDescription { get; set; }
        /// <summary>
        /// Version du firmware de l'appareil.
        /// </summary>
        string FirmwareVersion { get; set; }
        /// <summary>
        /// Nom de produit de la caméra.
        /// </summary>
        string ProductName { get; set; }
        /// <summary>
        /// Numéro de série.
        /// </summary>
        string SerialNumber { get; set; }
    }
}