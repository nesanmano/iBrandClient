using System;
using System.Linq;
using Rssdp;

namespace NomadLogic.Common.Util.Networking
{
    /// <summary>
    /// Cette classe englobe l'utilitaire pour préparer à envoyer par broadcast l'information d'une machine via le protocole SSDP.
    /// Le cycle d'une instance est le suivant:
    /// 1- Création de l'instance
    /// 2- Initialisation des propriétées.
    /// 2b- Création et/ou assignation d'un fichier de description du service SSDP.
    /// 3- Appel de la méthode InitAndStart(), qui lance le service.
    /// 4- Pour stopper, soit détruire ou appeller la méthode StopPublishingSsdpService()
    /// </summary>
    public class SsdpServiceProvider
    {
        private SsdpDevicePublisher _publisher;

        /// <summary>
        /// Initialise via les propriétés et lance l'application de publication du service sur le réseau local.
        /// </summary>
        public void InitAndStart()
        {
            if (_publisher != null) return;

            _publisher = new SsdpDevicePublisher();
            _publisher.AddDevice(new SsdpRootDevice
            {
                CacheLifetime = CacheTime,
                DeviceTypeNamespace = Message01,
                DeviceType = Message02,
                Uuid = ServiceId,
                ModelNumber = "null3",
                FriendlyName = "null1",
                Manufacturer = "Nomad Logic Inc.",
                ModelName = "null2",
                DeviceVersion = 1,
                Location = new Uri(@"C:\"),
                ModelDescription = "nl"
            });
        }

        ~SsdpServiceProvider()
        {
            StopPublishingSsdpService();
        }

        public void StopPublishingSsdpService()
        {
            _publisher?.Dispose();
        }

        /// <summary>
        /// Indique si le service est présentement en train de rouler.
        /// </summary>
        public bool IsRunning => _publisher?.Devices.Any() ?? false;

        public string Message01 { get; set; } = "Message01";

        public string Message02 { get; set; } = "Message02";

        public string ServiceId { get; set; } = "NomadLogicTcpListener";
        
        public TimeSpan CacheTime { get; set; } = TimeSpan.FromMinutes(5);
        
    }
}
