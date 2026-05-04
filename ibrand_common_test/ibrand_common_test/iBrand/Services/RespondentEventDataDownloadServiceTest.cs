using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NLog;
using NomadLogic.Common.Serveur;
using NomadLogic.Common.Services;
using NomadLogic.Common.Services.Configuration;
using NomadLogic.Common.Util.DateAndTime;
using NomadLogic.iBrandDesktop.Services.RespondentEventDataDownload;
using NomadLogic.iBrandDesktop.Services.RespondentEventDataDownload.DownloadWorkers;
using NomadLogic.Test.Common.Respondent;
using NUnit.Framework;
// ReSharper disable InconsistentNaming

namespace NomadLogic.Test.iBrand.Services
{
    public class RespondentEventDataDownloadServiceTest : TestBaseNunit
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        [TestCase(ServerConfigurationSwitch.Preprod, "nomad", 222, 286, 223, 287)]
        public async Task TestRespondentEventSyncDownload(ServerConfigurationSwitch server, string respondentUniqueId, int sourceQID, int sourceTID, int procQID, int procTID)
        {
            var nomadConfig = ServicesContainer.GetService<NomadConfiguration>();
            var conf = nomadConfig.ServerInformations[server];
            Assert.IsNotNull(conf);

            var outputDirPath = GetFullOutputDir();
            EnsureDirEmpty(outputDirPath);
            var outputDirInfo = new DirectoryInfo(outputDirPath);
            var tmpWorkDir = new DirectoryInfo(Path.Combine(outputDirPath, "tmp"));

            var successCalled = false;
            var errorCalled = false;
            var cancelledCalled= false;
            var dataSent = false;

            var callbackSuccess = new Action<RespondentBundleRequest>(request =>
            {
                successCalled = true;
                Console.Out.WriteLine("SUCCESS callback called!");
                log.Debug("Success called");
            });
            var callbackError = new Action<RespondentBundleRequest>(request =>
            {
                errorCalled = true;
                Console.Out.WriteLine("ERROR callback called!");
                log.Debug("Error called");
            });
            var callbackCancel = new Action<RespondentBundleRequest>(request =>
            {
                cancelledCalled = true;
                Console.Out.WriteLine("CANCELLED callback called!");
                log.Debug("Cancelled called");
            });

            var progressObserver = new Progress<RespondentBundleDownloadReport>();
            // Output console.
            var consoleTextFilePath = Path.Combine(outputDirPath, "console.log");
            var totalItemsToDownload = 0;
            RespondentBundleFinalData finalData;

            using (var writer = new StreamWriter(consoleTextFilePath, true))
            {
                progressObserver.ProgressChanged += async (sender, report) =>
                {
                    var message = "R -> " + report.ToStringQuickPreview();

                    await Console.Out.WriteLineAsync(message);

                    // ReSharper disable once AccessToDisposedClosure
                    await writer.WriteLineAsync(message);
                };

                var worker = new DownloadWorkerAqualand
                {
                    SourceQuestionnaireId = sourceQID,
                    SourceTerminalId = sourceTID,
                    ProcessedQuestionnaireId = procQID,
                    ProcessedTerminalId = procTID,
                    SourceDataKeyFilters = new List<string>(),
                    ProcessedDataKeyFilters = new List<string>(),
                    DateMinDefinition = new DateTimeCalculatorFixed {FixedDate = DateTime.Parse("2020-01-01", CultureInfo.InvariantCulture)},
                    DateMaxDefinition = new DateTimeCalculatorFixed {FixedDate = DateTime.Now},
                    TemporaryWorkDir = tmpWorkDir
                };

                var rbr = new RespondentBundleRequest(outputDirInfo, respondentUniqueId, worker, new MockQuestionnaire(),
                    callbackSuccess, callbackCancel, callbackError);

                using (var serviceRef = new RespondentEventDataDownloadService())
                {
                    serviceRef.ForcedServerConfiguration = conf;
                    serviceRef.AddObserver(progressObserver);

                    Assert.IsTrue(serviceRef.Init());
                    Assert.IsTrue(await serviceRef.StartLongRunning());

                    finalData = serviceRef.GetLastRespondentData();
                    Assert.IsNull(finalData, "Final data reference not null before first download request.");

                    serviceRef.SetRespondentToDownload(rbr);

                    finalData = serviceRef.GetLastRespondentData();
                    Assert.IsNotNull(finalData, "Final data reference should not be null.");
                    finalData.AllDataAvailiable += (sender, groups) =>
                    {
                        var data = finalData.GetFinalDataReference();
                        Assert.IsNotNull(data);
                        dataSent = true;
                        totalItemsToDownload = data.Values.Sum(group => group.Items.Sum(pair => pair.Value.IsDownloadableFile() ? (pair.Value.Downloaded ? 0 : 1) : 0));
                        Console.Out.WriteLine("** All Data received: {0} headers, {1} items left", data.Count, totalItemsToDownload);
                    };
                    finalData.DataIsNoLongerRelevant += (sender, groups) => { Console.Out.WriteLine("** Data irrelevance has been called."); };

                    // On attend
                    while (!successCalled && !errorCalled && !cancelledCalled)
                    {
                        await Task.Delay(100);
                    }

                    // Il ne devrait plus avoir d'exécution en cours dans le service. Le téléchargement est terminé.
                    Assert.IsTrue(serviceRef.Status == LongRunningStatus.Idle);

                    serviceRef.RemoveObserver(progressObserver);
                    await serviceRef.StopLongRunning();

                    var tmp = serviceRef.GetLastRespondentData();
                    Assert.IsNotNull(tmp);
                    Assert.AreSame(tmp, finalData);
                    
                    Assert.IsTrue(dataSent, "Raw data availiable event was not called.");

                    // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                    if (finalData != null)
                    {
                        var data = finalData.GetFinalDataReference();
                        totalItemsToDownload = data.Values.Sum(group => group.Items.Sum(pair => pair.Value.IsDownloadableFile() ? (pair.Value.Downloaded ? 0 : 1) : 0));
                        Console.Out.WriteLine("Data update: {0} headers, {1} items left", data.Count, totalItemsToDownload);
                        Assert.AreEqual(0, totalItemsToDownload, "Some items are still flagged as not downloaded.");
                        Assert.IsFalse(finalData.ErrorDetectedDuringProcess, "Flag for error is up.");
                    }

                    await Console.Out.WriteLineAsync($"Call statuses: Success={successCalled},Error={errorCalled},Cancel={cancelledCalled}");
                    Assert.IsTrue(successCalled);

                    // On assigne une nouvelle demande. Elle ne devrait pas être traitée (le loop est stoppé), mais cela va invalider la dernière requête, et on peut obtenir
                    // la référence vers la dernière requête.
                    serviceRef.SetRespondentToDownload(rbr);

                    Assert.IsFalse(finalData.DataAvailiable); // Les données ne sont plus disponibles.
                    Assert.IsTrue(finalData.DataUnusable); 
                    Assert.IsFalse(finalData.ErrorDetectedDuringProcess);

                    var newFinalData = serviceRef.GetLastRespondentData();

                    Assert.IsNotNull(newFinalData);
                    Assert.AreNotEqual(newFinalData, finalData);
                    
                    // vérifie que même si la nouvelle requête est "lancée", elle n'est pas encore prête car le téléchargement ne peut être amorcé.
                    Assert.IsFalse(newFinalData.DataAvailiable);
                    // Lers données ne sont pas contre pas encore viciée par une nouvelle demande. Mais pas disponible non plus.
                    Assert.IsFalse(newFinalData.DataUnusable);
                    Assert.IsFalse(newFinalData.ErrorDetectedDuringProcess);
                    Assert.IsNull(newFinalData.GetFinalDataReference()); //  Devrait retourner null.
                }
            }
        }
    }
}