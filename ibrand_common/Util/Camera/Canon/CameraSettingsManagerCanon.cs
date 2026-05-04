using System;
using System.Collections.Generic;
using System.Linq;
using Canon.Eos.Framework;
using JetBrains.Annotations;
using NLog;
using NomadLogic.Common.Util.Camera.Canon.CameraProperty;

namespace NomadLogic.Common.Util.Camera.Canon
{
    /// <summary>
    /// Cette classe contient et analyse les configurations d'un groupement de caméra Canon.
    /// La raison est que même parmi le même modèle, certaines configurations n'ont pas les mêmes
    /// valeurs possibles entre certaines caméras. Cette classe conserve et analyse les configurations
    /// de toutes les caméras dans un ensemble cohérent, et permet de donner les valeurs communes à
    /// toutes les caméras dans l'ensemble.
    /// </summary>
    [PublicAPI]
    public class CameraSettingsManagerCanon : IDisposable
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private readonly Dictionary<string, CameraPropertiesGroup> _groupsDictionary = new Dictionary<string, CameraPropertiesGroup>();
        private readonly Dictionary<uint, ISet<string>> _propIdShared = new Dictionary<uint, ISet<string>>();

        /// <summary>
        /// Met à jour l'ensemble de propriété globale avec cette caméra. Si la caméra à déjà été initialisée, rien ne se produira.
        /// </summary>
        /// <param name="eosCamera">La caméra à mettre à jour.</param>
        /// <param name="setAsActive">Indique s'il faut indiquer que la caméra est active ou non.</param>
        public void UpdateSettingsForCamera(EosCamera eosCamera, bool setAsActive = true)
        {
            if (eosCamera == null) return;
            if (_groupsDictionary.ContainsKey(eosCamera.SerialNumber))
            {
                if (_groupsDictionary[eosCamera.SerialNumber].Active == setAsActive) return;

                _groupsDictionary[eosCamera.SerialNumber].Active = setAsActive;
                CalculteSharedProperties();
                return;
            }

            var group = new CameraPropertiesGroup(eosCamera, setAsActive);

            log.Trace(
                "Camera (serial={8}, Author={9}, Description={10}) properties initialized with the following properties: ISO={0}, AeMode={1}, Av={2}, ExposureComp={3}, ImageQuality={4}, MeteringMode={5}, Tv={6}, WhiteBalance={7}",
                group.Iso.SelectedValue,
                group.AeMode.SelectedValue,
                group.Av.SelectedValue,
                group.ExposureComp.SelectedValue,
                group.ImageQuality.SelectedValue,
                group.MeteringMode.SelectedValue,
                group.Tv.SelectedValue,
                group.WhiteBalance.SelectedValue,
                eosCamera.SerialNumber,
                eosCamera.Author,
                eosCamera.DeviceDescription);

            _groupsDictionary.Add(eosCamera.SerialNumber, group);
            if (setAsActive)
            {
                CalculteSharedProperties();
            }
        }

        /// <summary>
        /// Obtient le groupement de propriétés d'une caméra spécifique.
        /// </summary>
        /// <param name="cameraSerialNumber">Le numéro de série de la caméra voulue.</param>
        /// <returns>NULL si la caméra n'a pas été insérée dans le gestionnaire au préalable.</returns>
        public CameraPropertiesGroup GetPropertiesGroup(string cameraSerialNumber)
        {
            if (string.IsNullOrWhiteSpace(cameraSerialNumber)) return null;
            return !_groupsDictionary.ContainsKey(cameraSerialNumber) ? null : _groupsDictionary[cameraSerialNumber];
        }
        
        /// <summary>
        /// Obtient le groupement de propriétés d'une caméra spécifique.
        /// </summary>
        /// <param name="cam">La référence vers la caméra.</param>
        /// <returns>NULL si la caméra n'a pas été insérée dans le gestionnaire au préalable.</returns>
        public CameraPropertiesGroup GetPropertiesGroup(EosCamera cam)
        {
            return cam == null ? null : GetPropertiesGroup(cam.SerialNumber);
        }

        /// <summary>
        /// Calcule si un groupe de propriétés spécifique, via ses valeurs présentement sélectionnées, peut être
        /// propagé à tous les autres groupes présentement actif. Retourne vrai si les valeurs sélectionnées
        /// existent comme possibilités pour tous les groupes, pour toutes les propriétés.
        /// </summary>
        /// <param name="groupIdToPropagate">L'identifiant du groupe à analyser.</param>
        /// <returns>TRUE si une propagation aux autres groupes actif est possible.</returns>
        public bool IsCurrentCameraSettingsSharedByAll(string groupIdToPropagate)
        {
            _groupsDictionary.TryGetValue(groupIdToPropagate, out var currentGroup);

            return currentGroup != null && currentGroup.GetAllAbstractProperties().Aggregate(true,
                (current, props) => current & IsValueSharedByAllActiveGroups(props.PropertyId, props.SelectedValue));
        }

        /// <summary>
        /// Copie les valeurs sélectionnées d'un groupe spécifique à tous les autres groupes présentement actifs.
        /// cette méthode assume que c'est possible via une vérification préalable faite par l'appellant avec
        /// la méthode "IsCurrentCameraSettingsSharedByAll(string)".
        /// Sinon, rien ne sera fait et on retourn false.
        /// </summary>
        /// <param name="selectedCameraSerialNumber">Le groupe à prendre comme modèle pour la propagation.</param>
        /// <param name="cameraDictionary">Le dictionnaire de référence vers les instance valides de caméra, afin de les mettre à jour.</param>
        /// <returns>TRUE si la propagation s'est correctement effectuée.</returns>
        public bool PropagateGroupSelectionToAllActiveGroups(string selectedCameraSerialNumber, Dictionary<string, EosCamera> cameraDictionary)
        {
            _groupsDictionary.TryGetValue(selectedCameraSerialNumber, out var currentGroup);
            if (currentGroup == null) return false;

            if (!IsCurrentCameraSettingsSharedByAll(selectedCameraSerialNumber)) return false;


            foreach (var group in _groupsDictionary.Values)
            {
                if (group.Equals(currentGroup)) continue; // Le groupe de base ne se modifie pas lui-même.

                foreach (var prop in currentGroup.GetAllAbstractProperties())
                {
                    if (prop.SelectedValue == null) continue; // On ne met pas à jour une valeur nulle vers les autres.
                    cameraDictionary.TryGetValue(group.OwnerCameraSerial, out var camRef);
                    group.SetPropertyIntegerData(camRef, prop.PropertyId, prop.PropertyValue);
                }
            }

            return true;
        }

        /// <summary>
        /// Met à jour le status d'activité de la caméra, via son numéro de série.
        /// </summary>
        /// <param name="serial">Le numéro de série du groupe.</param>
        /// <param name="setActive">Le drapeau d'activité.</param>
        public void UpdateActiveSettingForCamera(string serial, bool setActive)
        {
            if (string.IsNullOrWhiteSpace(serial)) return;
            if (!_groupsDictionary.ContainsKey(serial)) return;
            if (_groupsDictionary[serial].Active == setActive) return;

            _groupsDictionary[serial].Active = setActive;
            CalculteSharedProperties();
        }

        /// <summary>
        /// Met à jour les propriétés partagées de tous les groupes actifs.
        /// </summary>
        private void CalculteSharedProperties()
        {
            _propIdShared.Clear();
            var activeGroups = _groupsDictionary.Values.Where(g => g.Active).ToList();

            switch (activeGroups.Count)
            {
                // Si aucun élément, le dictionnaire reste vide.
                case 0:
                    return;
                // Si un seul élément, on copie les valeurs.
                case 1:
                    var gr = activeGroups[0];
                    log.Trace("Adding properties for camera serial={0}, author={1}", gr.OwnerCameraSerial, gr.CameraAuthor);
                    foreach (var prop in gr.GetAllAbstractProperties())
                    {
                        var valuesSet = new HashSet<string>();
                        foreach (var val in prop.SupportedDisplayValues)
                        {
                            valuesSet.Add(val);
                        }

                        _propIdShared.Add(prop.PropertyId, valuesSet);
                        log.Trace("Property '{0}' added. Shared values: {1}", prop.GetType().Name, valuesSet.Count);
                    }
                    break;
                // Si plus que un élément, on insère juste les valeurs communes.
                default:
                    foreach (var group in activeGroups)
                    {
                        log.Trace("Adding properties for camera serial={0}, author={1}, type={2}", group.OwnerCameraSerial, group.CameraAuthor, group.Description);
                        foreach (var prop in group.GetAllAbstractProperties())
                        {
                            if (!_propIdShared.ContainsKey(prop.PropertyId))
                            {
                                var set = new SortedSet<string>();
                                foreach (var val in prop.SupportedDisplayValues)
                                {
                                    set.Add(val);
                                }
                                _propIdShared.Add(prop.PropertyId, set);
                                log.Trace("Property '{0}:{2}' added. Shared values: {1}", prop.GetType().Name, set.Count, prop.PropertyId);
                                continue;
                            }

                            var propSet = _propIdShared[prop.PropertyId];
                            var beforeIntersect = new string[propSet.Count];
                            propSet.CopyTo(beforeIntersect, 0);
                            var nbrBefore = propSet.Count;
                            propSet.IntersectWith(prop.SupportedDisplayValues);

                            if (!log.IsTraceEnabled) continue;
                            var diff = beforeIntersect.Except(propSet);
                            log.Trace("Property '{0}:{3}' added. Shared values: {1} => {2} removed=[{4}]",
                                prop.GetType().Name, nbrBefore, propSet.Count, prop.PropertyId, StringUtil.FormatEnumerable(diff, false, 0, ", "));
                        }
                    }

                    if (log.IsDebugEnabled)
                    {
                        var ite = _propIdShared.GetEnumerator();
                        while(ite.MoveNext()) log.Debug("PropIdShared: {0} -> [{1}]", ite.Current.Key, StringUtil.FormatEnumerable(ite.Current.Value, false, 0, ", "));
                        ite.Dispose();
                    }

                    break;
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _groupsDictionary.Clear();
            _propIdShared.Clear();
        }

        /// <summary>
        /// Vérifie si une valeur d'une propriété est partagée par tous les autres groupes actifs.
        /// </summary>
        /// <param name="propertyId">L'identifiant de la propriété.</param>
        /// <param name="propertyValue">La valeur de la propriété.</param>
        /// <returns>TRUE si la valeur de la propriété est partagée.</returns>
        public bool IsValueSharedByAllActiveGroups(uint propertyId, string propertyValue)
        {
            return propertyValue == null || _propIdShared.ContainsKey(propertyId) && _propIdShared[propertyId].Contains(propertyValue);
        }
    }
}