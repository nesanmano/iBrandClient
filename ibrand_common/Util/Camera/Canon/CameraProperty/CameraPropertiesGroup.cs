using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Canon.Eos.Framework;
using NLog;

namespace NomadLogic.Common.Util.Camera.Canon.CameraProperty
{
    /// <summary>
    /// Cette camréa regroupe les comfigurations de propriétés valides pour une instance de caméra, 
    /// identifiée par numéro de série. Vu que l'identification est faite par numéro de série,
    /// une fois créé, on peut conserver ces modifications en mémoire même si une caméra se débranche.
    /// </summary>
    public class CameraPropertiesGroup
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private readonly Dictionary<uint, AbstractCameraProperty> _propertiesById = new Dictionary<uint, AbstractCameraProperty>();
        private ReadOnlyCollection<AbstractCameraProperty> _listAbstractProperties;

        /// <summary>
        /// Constrcuteur principal.
        /// </summary>
        /// <param name="camera">La caméra à analyser pour créer le groupe.</param>
        /// <param name="setAsActive">Indique s'il faut mettre la caméra active dès le départ.</param>
        public CameraPropertiesGroup(EosCamera camera, bool setAsActive = true)
        {
            OwnerCameraSerial = camera.SerialNumber;
            AeMode = new AeMode(camera);
            ExposureComp = new ExposureComp(camera);
            ImageQuality = new ImageQuality(camera);
            MeteringMode = new MeteringMode(camera);
            Iso = new Iso(camera);
            WhiteBalance = new WhiteBalance(camera);
            Tv = new Tv(camera);
            Av = new Av(camera);
            Active = setAsActive;
            CameraAuthor = camera.Author;
            Description = camera.DeviceDescription;

            _propertiesById.Add(AeMode.PropertyId, AeMode);
            _propertiesById.Add(ExposureComp.PropertyId, ExposureComp);
            _propertiesById.Add(ImageQuality.PropertyId, ImageQuality);
            _propertiesById.Add(MeteringMode.PropertyId, MeteringMode);
            _propertiesById.Add(Iso.PropertyId, Iso);
            _propertiesById.Add(WhiteBalance.PropertyId, WhiteBalance);
            _propertiesById.Add(Tv.PropertyId, Tv);
            _propertiesById.Add(Av.PropertyId, Av);
        }

        /// <summary>
        /// L'identifiant série de la caméra à qui appartient ce groupe de configuration.
        /// </summary>
        public string OwnerCameraSerial { get; }

        public AeMode AeMode { get; }

        public Av Av { get; }

        public ExposureComp ExposureComp { get; }

        public ImageQuality ImageQuality { get; }

        public Iso Iso { get; }

        public MeteringMode MeteringMode { get; }

        public Tv Tv { get; }

        public WhiteBalance WhiteBalance { get; }

        /// <summary>
        /// Indique qu'un groupe de propriété est actif (I.E. cette caméra est détectée par le manager).
        /// </summary>
        public bool Active { get; set; }

        /// <summary>
        /// L'auteur de la caméra, pour facilement l'identifier par l'usager.
        /// </summary>
        public string CameraAuthor { get; }

        /// <summary>
        /// Description du matériel, venant du matériel lui-même.
        /// </summary>
        public string Description { get; }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return OwnerCameraSerial.Equals(obj);
        }

        /// <summary>
        /// COmparateur d'égalité avec type forcé.
        /// </summary>
        /// <param name="other">La référence testée.</param>
        /// <returns>TRUE si égal. Cela se fait unqiuement sur le numéro de série.</returns>
        public bool Equals(CameraPropertiesGroup other)
        {
            return string.Equals(OwnerCameraSerial, other.OwnerCameraSerial);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return OwnerCameraSerial?.GetHashCode() ?? 0;
        }

        /// <summary>
        /// Retourne une liste énumérable de toutes les propriétés de ce groupe.
        /// </summary>
        /// <returns>Une énumération des propriétés de ce groupe.</returns>
        public IReadOnlyCollection<AbstractCameraProperty> GetAllAbstractProperties()
        {
            return _listAbstractProperties ??
                   (_listAbstractProperties =
                       new ReadOnlyCollection<AbstractCameraProperty>(new List<AbstractCameraProperty>
                       {
                           AeMode,
                           Av,
                           ExposureComp,
                           ImageQuality,
                           Iso,
                           MeteringMode,
                           Tv,
                           WhiteBalance
                       }));
        }

        /// <summary>
        /// Met à jour une valeur de propriété via son identifiant numérique.
        /// </summary>
        /// <param name="camera">Optionel. Une référence d'objet caméra à mettre à jour en même temps.</param>
        /// <param name="propPropertyId">L'identifiant de la propriété.</param>
        /// <param name="propPropertyValue">La nouvelle valeur de la propriété.</param>
        public void SetPropertyIntegerData(EosCamera camera, uint propPropertyId, uint propPropertyValue)
        {
            _propertiesById.TryGetValue(propPropertyId, out var prop);
            try
            {
                prop?.UpdateSelectedValue(camera, propPropertyValue);
            }
            catch (Exception err)
            {
                log.Warn(err,
                    "Error during property set (serial={0}, property={1}, new value={2})",
                    OwnerCameraSerial, prop, propPropertyValue);
            }
        }
    }
}