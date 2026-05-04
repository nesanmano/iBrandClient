using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using NLog;
using NomadLogic.Common.Services;
using NomadLogic.Common.Services.Configuration;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Networking;
using NomadLogic.PrintStation.Services;

namespace NomadLogic.PrintStation
{
    /// <summary>
    ///     Classe qui envoie vers le serveur le nombre d'impression de façon récurrente.
    /// </summary>
    public class SendPrintCountTask
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private readonly PrintStationDatabase _database;
        private readonly Server _server;

        /// <summary>
        ///     Contructeur principal.
        /// </summary>
        /// <param name="database">La base de donnée à utiliser pour gérer les Posts.</param>
        /// <param name="config">La abstractConfiguration du système.</param>
        public SendPrintCountTask(PrintStationDatabase database, NomadConfiguration config)
        {
            _database = database;
            var server = new Server("192.95.36.173", "le-server", "8186", "8186");
        }

        /// <summary>
        ///     Lance la tâche, et ne peut être arrêtée que si le paramètre de Token d'arrêt est notifié d'un arrêt.
        ///     <br />Effectue l'envoi vers le serveur lors de l'appel initial, mais par la suite ne fera rien à moins qu'une
        ///     impression ait été enregistrée par le système par événement.
        ///     <br />Cette méthode ne PEUT PAS arrêter d'elle-même et donc doit absolument être encapsulée dans un Task C#.
        /// </summary>
        /// <param name="cancel">Le Token d'arrêt.</param>
        public void DoWork(CancellationToken cancel)
        {
            log.Trace("SendPrintCount task started.");

            var signaled = true;
            while (!cancel.IsCancellationRequested)
            {
                if (signaled)
                {
                    try
                    {
                        var printCounts = _database.GetPrintCountToSend();

                        foreach (var printCount in printCounts.Where(printCount => SendPrintCount(printCount.PostId, printCount.Identifier)))
                        {
                            _database.SetPrintCountSent(printCount.Id);
                        }

                        var printCountsSocialCam = _database.GetPrintCountSocialCamToSend();

                        foreach (var printCount in printCountsSocialCam.Where(printCount => SendPrintCountSocialCam(printCount.EventId, printCount.Identifier)))
                        {
                            _database.SetPrintCountSocialCamSent(printCount.Id);
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Error(ex);
                    }
                }

                signaled = false;

                // Attend indéfiniment soit un événement d'impression ou une demande d'arrêt de la tâche.
                while (!cancel.IsCancellationRequested)
                {
                    signaled = NewPrintEvent.GetInstance().NewItemEvent.WaitOne(100);
                    break;
                }
            }
            log.Trace("SendPrintCount task stopped.");
        }

        private bool SendPrintCount(int postId, string identifier)
        {
            var request = HttpUtil.GenerateRequest(_server, "print-station/AddPrintCount",
                "&post_id=" + postId + "&identifier=" + identifier);
            var response = (HttpWebResponse)request.GetResponse();

            if (response.StatusCode != HttpStatusCode.OK) return false;
            using (var s = response.GetResponseStream())
            {
                if (s == null) return false;
                using (var sr = new StreamReader(s))
                {
                    if (sr.ReadToEnd().Equals("ok"))
                        return true;
                }
            }
            return false;
        }

        private bool SendPrintCountSocialCam(int eventId, string identifier)
        {
            var request = HttpUtil.GenerateRequest(_server, "print-station/AddPrintCountSocialCam",
                "&event_id=" + eventId + "&identifier=" + identifier);
            var response = (HttpWebResponse)request.GetResponse();

            if (response.StatusCode != HttpStatusCode.OK) return false;
            using (var s = response.GetResponseStream())
            {
                if (s == null) return false;
                using (var sr = new StreamReader(s))
                {
                    if (sr.ReadToEnd().Equals("ok"))
                        return true;
                }
            }
            return false;
        }
    }
}