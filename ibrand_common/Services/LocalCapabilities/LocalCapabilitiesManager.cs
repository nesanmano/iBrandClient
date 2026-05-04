using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using JetBrains.Annotations;
using NomadLogic.Common.Services.Configuration;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Json;

namespace NomadLogic.Common.Services.LocalCapabilities
{
    /// <summary>
    /// Cette classe contient des méthodes pour analyser et donner sous forme de rapport les fonctionnalités
    /// manquantes d'un système pour exécuter certaines opérations spécifiques.
    /// Par exemple, les capacités à exécuter du code OpenGL ou DirectX (capacités de la carte graphique)
    /// ou la localistion de certaines exécutables externes, comme FFMPEG.
    /// </summary>
    public static class LocalCapabilitiesManager
    {
        /// <summary>
        /// Fait un test de capacités système et retourne un rapport détaillé du résultat.
        /// Va également générer un fichier spécial pour décrire ce résultat sur disque.
        /// Chaque évocation de cette méthode va mettre à jour ce rapport.
        /// Les tests se font de façon asynchrone, et on peut donner un callBack pour réagir sur le résultat.
        /// </summary>
        public static async void AnalyseCapabilities(
            [NotNull] IEnumerable<ILocalCapabilityVerifier> tests, 
            [NotNull] NomadConfiguration config, 
            Action<CapabilitiesReport> callbackResult)
        {
            var report = new CapabilitiesReport();
            config.CapabilitiesReport = report;

            await Task.Run(() =>
            {
                foreach (var test in tests)
                {
                    test.VerifyCapability(report);
                }
            });
            
            WriteReportToDisk(report);
            callbackResult?.Invoke(report);
        }
        
        private static void WriteReportToDisk(CapabilitiesReport report)
        {
            var path = new FileInfo(DirectoryUtil.GetLocalApplicationDir(DirectoryUtil.ApplicationNameEnum.Nomad) + "capabilitiesReport.json");
            JsonUtil.WriteJsonFile(path, report);
        }
    }
}