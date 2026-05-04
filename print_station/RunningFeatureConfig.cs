using NomadLogic.PrintStation.Services;
using System;
using System.Linq;
using NomadLogic.Common.Services;

namespace NomadLogic.PrintStation
{
    /// <summary>
    /// Cette classe statique initialize et peut être utilisée pour accéder aux configuration de la PrintStation.
    /// <br/>Ces configurations viennent de la base de données, et possèdent (pour la plupart) une valeur par défaut.
    /// <br/>Il n'est requis que de mettre une valeur dans la base de donnée que pour modifier la valeur par défaut.
    /// </summary>
    public class RunningFeatureConfig
    {
        private static readonly NLog.Logger log = NLog.LogManager.GetCurrentClassLogger();
        private static RunningFeatureConfig _runningConfig;
        
        /// <summary>
        /// Obtient la configuration voulue.
        /// </summary>
        /// <typeparam name="T">Le type voulu.</typeparam>
        /// <param name="confEnum">La configuration à aller chercher.</param>
        /// <returns>La valeur donnée ou par défaut.</returns>
        public T GetConfiguration<T>(FeatureConfigEnum confEnum) where T : IConvertible
        {
            return FeatureConfigManager.GetConfigValue<T>(confEnum);
        }

        /// <summary>
        /// Assigne une valeur pour la configuration demandée. SI on donne NULL, cela enlève la valeur des configurations actuelles.
        /// </summary>
        /// <param name="confEnum">La configuration à modifier.</param>
        /// <param name="value">La valeur à donner. Sera convertie en String</param>
        /// <returns>Si la valeur a été mise à jour (ajout, modification ou destruction). Une valeur similaire écrasée retourne vrai.</returns>
        public bool SetConfiguration(FeatureConfigEnum confEnum, Object value)
        {
            return value == null ? FeatureConfigManager.RemoveConfigurationValue(confEnum) : FeatureConfigManager.SetConfigurationValue(confEnum, value.ToString());
        }

        /// <summary>
        /// La seule onstance de cette classe.
        /// </summary>
        /// <returns></returns>
        public static RunningFeatureConfig GetInstance()
        {
            return _runningConfig ?? (_runningConfig = new RunningFeatureConfig());
        }
        
        /// <summary>
        /// Va chercher dans la base de données les informations de configuration d'un événement (Feature) particulier.
        /// Vide les configurations au préalable.
        /// </summary>
        /// <param name="featureId">L'identifiant de l'événement à configurer.</param>
        public void Initialize(int featureId)
        {
            var database = ServicesContainer.GetService<PrintStationDatabase>();
            var fromDb = database.GetFeatureConfig(featureId);

            FeatureConfigManager.ClearConfigurations();
            foreach (var f in fromDb.Where(f => !FeatureConfigManager.SetConfigurationValue(f)))
            {
                log.Warn("Could not Set configuration value from database. Name={0} Value={1}", f.Name, f.Value);
            }
        }

        /// <summary>
        /// Vide la configuration actuelle de toute valeur.
        /// </summary>
        public void Clear()
        {
            FeatureConfigManager.ClearConfigurations();
        }
    }
}