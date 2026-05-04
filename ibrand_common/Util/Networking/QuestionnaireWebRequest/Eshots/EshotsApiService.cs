using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NomadLogic.Common.Services;

namespace NomadLogic.Common.Util.Networking.QuestionnaireWebRequest.Eshots
{
    public enum EshotsApiConfigState
    {
        CanSend,
        TodayInvalid,
        FullLoginRequired,
        ManualSetupRequired,
        RenewRequired
    }

    /// <summary>
    /// Contient, en mémoire, les informations de la dernière activation.
    /// Attention, même s'il l'initialisation est vraie, il se peut qu'il n'y ait aucune information disponible.
    /// S'il n'y a pas eu de synchronisation lors de cette activation, mais qu'il y avait des données en mémoire,
    /// comme par exemple une synchro à été faite le matin puis l'application a simplement été relancée,
    /// uniquement le jour courant est donné, car c'est la seule information (hormis le token d'identification) qui
    /// est requise pour envoyer des données à un projet Eshots.
    /// </summary>
    public class EshotsApiService : IService
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        private IList<EshotsCampaign> _fullData;
        private IList<EshotsEventDay> _cachedDays;

        public EshotsCampaign CurrentCampaign { get; set; }

        public EshotsEvent CurrentEvent { get; set; }

        public EshotsEventDay CurrentEventDay { get; set; }
        
        public virtual IDatabaseEshots LocalDatabase { get; set; }

        public DateTime? LastLogin { get; set; }

        public string LastUserName { get; set; }

        /// <summary>
        /// Si mis à vrai, force les usager à manuellement changer la date dans les options à chaque changement de jour.
        /// </summary>
        public bool ManualDaySelect { get; set; }
        
        public string RefreshToken { get; set; }

        public string AccessToken { get; set; }

        /// <summary>
        /// Classe pour envelopper la logique d'une initialisation paresseuse. Pour ne pas à charger ce service au départ du logiciel et uniquement au besoin.
        /// Vérifie l'état de la variable d'instance. Si elle est null, tente une première initialisation à partir du disque et
        /// intègre le résultat (si positif) dans le gestionnaire de services. Fait un no-op dans le cas où l'instane n'est pas nulle.
        /// </summary>
        /// <param name="instance">Le référence à tester.</param>
        /// <param name="database">La base de donnée à utiliser s'il créer la prmière instance du service.</param>
        /// <returns>Soit la même instance (si non nul au départ), sinon, la nouvelle instance créée.</returns>
        public static EshotsApiService InitialInitCheck(EshotsApiService instance, IDatabaseEshots database)
        {
            if (instance != null) return instance;

            log.Debug("First init of Eshot Service instance.");
            instance = new EshotsApiService
            {
                LocalDatabase = database
            };

            var t = ServicesContainer.AddService(instance);
            t.Wait();

            if (t.Result) return instance;
            log.Error("Could not initialize Eshots service from disk.");
            return null;
        }

        /// <summary>
        /// Cette propriété est importante à vérifier sur un base régulière, avant chaque envoi. Elle détecte les changements
        /// de jours et résume si des actions spécifiques doivent être faites avant de procéder avec des appels vers le serveur de Eshots.
        /// En gros, on ne devrait procéder que si cette méthode retourne "CanSend".
        /// On devrait ouvrir le UI de configuration quand on reçoit "ManualSetupRequired" pour choisir une nouvelle journée.
        /// Également quand 
        /// </summary>
        public virtual EshotsApiConfigState ApiState
        {
            get
            {
                if (CurrentEventDay == null) return EshotsApiConfigState.FullLoginRequired;

                if (LastLogin == null) return EshotsApiConfigState.FullLoginRequired;

                if (LastLogin.Value + TimeSpan.FromDays(1) < DateTime.Now) return EshotsApiConfigState.RenewRequired;

                // Cas rare. Le jour à changé naturellement depuis la dernière config.
                var todayNow = DateTime.Today;
                if (InstancedDate < todayNow)
                {
                    // Il faut forcer un changement de configuration via l'écran UI.
                    if (ManualDaySelect) return EshotsApiConfigState.ManualSetupRequired;
                    
                    // On tente de trouver une journées valide correspondant à aujourd'hui.
                    var tentativeDay = _cachedDays?.FirstOrDefault(edc => edc.Date == todayNow);
                    if (tentativeDay == null)
                    {
                        // Cas rare: soit on a atteint la dernière journée valide, soit on est tombé dans un trou entre les dates.
                        if (CurrentEventDay != null && CanUseLastConfig) return EshotsApiConfigState.CanSend;
                        return EshotsApiConfigState.TodayInvalid;
                    }

                    log.Info("Changement de jour détecté et sélection automatique permise. Changement effectué pour: " + tentativeDay);
                    CurrentEventDay = tentativeDay;
                    InstancedDate = todayNow;
                    return EshotsApiConfigState.CanSend;
                }

                // Vérifie si on peut utiliser une date AUTRE que celle d'aujourd'hui (forcer l'utilisation de "Today").
                if (CurrentEventDay.Date != InstancedDate && !CanUseLastConfig)
                    return EshotsApiConfigState.TodayInvalid;

                // Situation par défaut. Tout est beau.
                return EshotsApiConfigState.CanSend;
            }
        }

        /// <summary>
        /// Peux-t-on utiliser un autre jour que la date d'instance?
        /// </summary>
        public bool CanUseLastConfig { get; set; }

        /// <summary>
        /// Request past data when login in with the server.
        /// </summary>
        public bool RequestPastData { get; set; }

        /// <summary>
        /// Indique une date pour la création de l'instance. Permet de détecter un changement de jour (minuit passé).
        /// Note, changer cette valeur manuellement n'est utile que dans un contexte de tests.
        /// </summary>
        public DateTime InstancedDate = DateTime.Today;

        public void Dispose()
        {
            //
        }

        // Constructeur vide
        public EshotsApiService()
        {
            log.Trace("Eshots Service instance created.");
        }

        public virtual bool Init()
        {
            // Tente de retrouver la dernière configuration importante stockée sur disque. Habituellement cela est 

            if (LocalDatabase == null) return false;

            var lastDataInDb = LocalDatabase.GetLastEshotsConfiguration();
            
            if (lastDataInDb != null)
            {
                log.Debug("Last eShot Configuration read from disk: \n" + ClassUtil.ToStringPublicProperties(lastDataInDb, 1));

                LastUserName = lastDataInDb.LastUserName;
                LastLogin = lastDataInDb.ConfigurationDate;
                ManualDaySelect = lastDataInDb.DontForceRefresh;
                _cachedDays = lastDataInDb.EventDaysCached;
                CurrentEventDay = !ManualDaySelect ? _cachedDays?.FirstOrDefault(edc => edc.Date == InstancedDate) : null;
                CanUseLastConfig = lastDataInDb.AllowAllDays;
                RequestPastData = lastDataInDb.AllowPastData;
                AccessToken = lastDataInDb.AccessToken;
                RefreshToken = lastDataInDb.RefreshToken;
            }

            if (CurrentEventDay != null && !ManualDaySelect)
            {
                log.Debug("Automatic selection of current day enabled from disk: " + CurrentEventDay);
            }

            return true;
        }
        
        public IList<EshotsCampaign> GetFullData()
        {
            return _fullData;
        }

        public void SetFullData(IList<EshotsCampaign> campaignData, IList<EshotsEventDay> cachedDays)
        {
            _fullData = campaignData;
            _cachedDays = cachedDays;
        }

        public IList<EshotsEventDay> GetCachedDays()
        {
            return _cachedDays;
        }

        public void Clear()
        {
            CurrentCampaign = null;
            CurrentEvent = null;
            CurrentEventDay = null;
            ManualDaySelect = false;
            LastLogin = null;
            LastUserName = null;
            _cachedDays = null;
            LocalDatabase?.RegisterCurrentEshotsConfiguration(null); // Vide les données.
        }
    }
}