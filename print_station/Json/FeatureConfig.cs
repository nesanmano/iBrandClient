using System;
using System.Runtime.Serialization;

namespace NomadLogic.PrintStation.Json
{
    /// <summary>
    /// Classe voulue comme le résultat d'une désérialisation JSON pour la configuration d'un événement PrintStation.
    /// Une instance de cette classe est une configuration précise.
    /// </summary>
    [DataContract]
    public class FeatureConfig
    {
        /// <summary>
        /// Constructeur complet.
        /// </summary>
        /// <param name="name">Nom (clé) de la configuration</param>
        /// <param name="value">Valeur de la configuration.</param>
        public FeatureConfig(string name, string value)
        {
            Name = name;
            Value = value;
        }

        /// <summary>
        /// Obtient la valeur clé de cette configuration.
        /// </summary>
        [DataMember(Name = "name")]
        public string Name { get; set; }

        /// <summary>
        /// Obtient la valeur de la configuration.
        /// </summary>
        [DataMember(Name = "value")]
        public string Value { get; set; }

        /// <summary>
        /// Méthode de test d'égalité.
        /// </summary>
        /// <param name="o">L'objet en comparaison.</param>
        /// <returns>true si l'objet est égal à l'instance.</returns>
        public override bool Equals(object o)
        {
            if (this == o) return true;

            if (!(o is FeatureConfig)) return false;
            var f = (FeatureConfig) o;
            return f.Name.Equals(Name, StringComparison.InvariantCultureIgnoreCase);
        }

        /// <summary>
        /// Génère un Hash.
        /// </summary>
        /// <returns>Valeur entière du hash.</returns>
        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
    }
}