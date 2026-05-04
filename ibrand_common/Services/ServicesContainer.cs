using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NLog;

namespace NomadLogic.Common.Services
{
    /// <summary>
    /// Classe statique de référence vers les ervices globaux d'une application. Cela permet de ne pas avoir à faire des singletons de chacune de ces classes.
    /// </summary>
    public static class ServicesContainer
    {
        private static readonly Dictionary<string, IService> services = new Dictionary<string, IService>();
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Ajout un service dans le dictionnaire, via sa classe d'instance.
        /// Le service sera initialisé, et si applicable, démarré.
        /// </summary>
        /// <param name="service">L'instance de service à insérer.</param>
        /// <param name="startAuto">Indique si on ne veut partir automatiquement les services de longue durées.</param>
        /// <returns>TRUE si le service a été ajouté (et partit si applicable).</returns>
        public static async Task<bool> AddService(IService service, bool startAuto = true)
        {
            var typeName = service?.GetType().FullName;
            if (typeName == null) return false;

            if (!service.Init())
            {
                log.Warn("Could not properly initialize service of type {0}.", typeName);
                service.Dispose();
                return false;
            }

            var allGood = true;
            if (startAuto && service is IAsyncLongRunning slr)
            {
                allGood = await slr.StartLongRunning();
            }

            if (!allGood)
            {
                log.Warn("Could not properly start service of type {0}.", typeName);
                service.Dispose();
                return false;
            }

            services.Add(typeName, service);
            return true;
        }

        /// <summary>
        /// Enlève un service du contenant. L'instance passée en paramètre sera disposée.
        /// </summary>
        /// <param name="service">L'instance du service à enlever.</param>
        public static async Task RemoveService(IService service)
        {
            var typeName = service?.GetType().FullName;
            if (typeName == null) return;

            services.Remove(typeName);
            if (service is IAsyncLongRunning longServ)
            {
                await longServ.StopLongRunning();
            }

            service.Dispose();
        }

        /// <summary>
        /// Vide le contenant. Idéalement il faut appeller cette méthode après avoir Stopper les services.
        /// </summary>
        public static void Dispose()
        {
            foreach (var service in services)
            {
                service.Value.Dispose();
                if (log.IsTraceEnabled) log.Trace("Service {0} has been disposed.", service.Value.GetType().Name);
            }
            services.Clear();
        }

        /// <summary>
        /// Obtient la référence d'une classe de service via son type. Cette classe doit étendre IService.
        /// retourne "default(T)", soit null, si le service n'a pas été initialisé.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T GetService<T>() where T : IService
        {
            var typ = typeof(T);
            if (typ == typeof(object)) return default;
            // ReSharper disable once AssignNullToNotNullAttribute
            var res = services.TryGetValue(typeof(T).FullName, out var service);
            if (res) return (T) service;

            var resRef = services.Values.FirstOrDefault(s => typ.IsInstanceOfType(s));
            return resRef != null ? (T)resRef : default;
            
        }

        public static async Task StopServices()
        {
            log.Trace("Stopping all services.");
            var servList = (from service in services.Values.Select(s => s as IAsyncLongRunning) where service != null select service.StopLongRunning()).ToArray();
            log.Trace("Waiting on {0} services.", servList.Length);
            await Task.WhenAll(servList);
            log.Trace("All services stopped.");
        }
    }
}
