using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Newtonsoft.Json;
using NomadLogic.Common.Serveur;
using NomadLogic.Common.Serveur.Requests;
using NomadLogic.Common.Serveur.Structures;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.DateAndTime;
using NomadLogic.Common.Util.Json;

namespace NomadLogic.iBrandDesktop.Services.RespondentEventDataDownload.DownloadWorkers
{
    /// <summary>
    /// Logique pour le téléchargement des informations pour Aqualand, qui utilise deux questionnaires comme sources d'information:
    /// - Un (nommé A) qui est la sources des images brutes.
    /// - Un (nommé B) qui contient une copie des images qui sont actuellement choisies et peuvent être disponibles de l'extérieur.
    /// Avec les caractéristiques suivantes:
    /// - B est donc un sous-ensemble de A
    /// - Les images choisie (présentes dans le questionnaire B) doivent être affichées, mais pas sélectionnables.
    /// - En plus d'images, il y a aussi des vidéos Viméos (via ID externe: ex -> "45362527687")
    /// </summary>
    [PublicAPI]
    public class DownloadWorkerAqualand : IRespondentEventDataDownloadWorker
    {
        [JsonRequired]
        public int SourceQuestionnaireId { get; set; }
        [JsonRequired]
        public int SourceTerminalId { get; set; }
        [JsonRequired]
        public List<string> SourceDataKeyFilters { get; set; }
        [JsonRequired]
        public int ProcessedTerminalId { get; set; }
        [JsonRequired]
        public int ProcessedQuestionnaireId { get; set; }
        [JsonRequired]
        public List<string> ProcessedDataKeyFilters { get; set; }
        
        /// <summary>
        /// Si on veut utiliser un répertoire temporaire autre que celui système (débogage par exemple),
        /// on peut en spécifier un ici.
        /// </summary>
        public DirectoryInfo TemporaryWorkDir { get; set; }
        
        public DatetimeCalculator DateMinDefinition { get; set; } = new DateTimeCalculatorFromNow
        {
            StartFromDateOnly = true,
            ReduceByTimespan = TimeSpan.Zero
        };

        public DatetimeCalculator DateMaxDefinition { get; set; } = new DateTimeCalculatorFromNow
        {
            StartFromDateOnly = true,
            ReduceByTimespan = TimeSpan.FromDays(-1) // Deux négatifs deviennent une addition.
        };

        public bool Validate()
        {
            var checksOut = true;
            checksOut &= SourceQuestionnaireId > 0;
            checksOut &= SourceTerminalId > 0;
            checksOut &= ProcessedQuestionnaireId > 0;
            checksOut &= ProcessedTerminalId > 0;
            checksOut &= DateMinDefinition != null;
            checksOut &= DateMaxDefinition != null;

            return checksOut;
        }

        public async Task DoDownload(CancellationToken ct, 
            ServerConfiguration currentServerConf,
            RespondentEventDataDownloadService service, 
            RespondentBundleRequest dr, 
            RespondentBundleFinalData respondentBundleFinalData)
        {
            var reportMaster = new RespondentBundleDownloadReport
            {
                StartTime = DateTime.Now
            };

            if (ct.IsCancellationRequested)
            {
                dr.CallbackCancelled?.Invoke(dr);
                respondentBundleFinalData.FlagAsUnusable();
                return;
            }
            
            var workDir = dr.GetBaseDirectory();
            if (!workDir.Exists)
            {
                Directory.CreateDirectory(workDir.FullName);
                workDir.Refresh();
                if (!workDir.Exists)
                {
                    service.Logger.Warn("Could not create destination directory: " + workDir.FullName);
                    dr.CallbackError?.Invoke(dr);
                    respondentBundleFinalData.FlagAsUnusable();
                    return;
                }
            }
            service.Logger.Trace("Work directory: {0}", workDir.FullName);

            var login = await ServerRestRequestsUtil.Login(currentServerConf, ct);
            if (login == null || login.Result != ServerRestRequestResultType.Success)
            {
                PrepareReport(service, reportMaster, null, "Could not complete download request. Login to main server failed.");
                dr.CallbackError(dr);
                respondentBundleFinalData.FlagAsUnusable();
                return;
            }

            service.Logger.Trace("Download Respondent: login established.");

            var filesToDownload = new List<RespondentBundleDataFile>();
            var jujuToken = login.Data;
            var requestHasMore = true;
            var requestMinDate = DateMinDefinition.CalculateDate(DateTimeKind.Local);
            var requestMaxDate = DateMaxDefinition.CalculateDate(DateTimeKind.Local);
            respondentBundleFinalData._fullData = new ConcurrentDictionary<RequestRespondentDataDynamicValue, RespondentBundleDataGroup>();

            var reqOriginalData = new List<RequestRespondentDataDynamicValue>();

            #region Étape 1: on télécharge les infos du répondant source, questionnaire A.
            while (requestHasMore)
            {
                var requestAParams = new RequestGetRespondentDataPayload
                {
                    RespondentIdentification = new RespondentIdentificationStrategy
                    {
                        Strategy = new RespondentIdentificationStrategyDefinition
                        {
                            TerminalId = SourceTerminalId,
                            QuestionnaireId = SourceQuestionnaireId,
                            FailOnEmpty = false,
                            ExternalUniqueIdentifier = dr.GetRespondentUniqueId(),
                            StrategyType = RequestRespondentDataIdentificationStrategyEnum.ExternalIdentifier
                        }
                    },
                    DynamicValuesFilter = new RespondentDataDynamicValueRequestFilter
                    {
                        MinimalDate = requestMinDate,
                        MaximumDate = requestMaxDate,
                        DynamicKeysList = SourceDataKeyFilters
                    }
                };

                if (service.Logger.IsTraceEnabled)
                {
                    service.Logger.Trace("Fetching data for respondent (UEI => {0}, IDs => {4}/{5}). Filters:\n\t\t- Dates Min='{1}', Max='{2}'\n\t\t- Key Filters={3}",
                        dr.GetRespondentUniqueId(), requestMinDate, requestMaxDate, 
                        StringUtil.FormatEnumerable(SourceDataKeyFilters, false, 0, ", "),
                        SourceQuestionnaireId, SourceTerminalId);
                }

                var reqAres = await ServerRestRequestsUtil.GetRespondentData(ct, currentServerConf, jujuToken, requestAParams);
                if (reqAres == null || reqAres.Result != ServerRestRequestResultType.Success)
                {
                    service.Logger.Error("Failed to get respondent data. Aborting.");
                    PrepareReport(service, reportMaster, null, errorMessage: "Could not complete download request. Request for origin data failed.");
                    dr.CallbackError?.Invoke(dr);
                    respondentBundleFinalData.FlagAsUnusable();
                    return;
                }

                requestHasMore = false;
                /* TODO Pas de pagination
                requestHasMore = reqAres.Data.HasMore; // Continue ou arrête.
                requestMinDate = reqAres.Data.LastElementDate;
                if (requestHasMore) service.Logger.Debug("More original elements exists. Requesting new data.");
                */

                if (reqAres.Data == null)
                {
                    service.Logger.Debug("No data was found for request.");
                    PrepareReport(service, reportMaster, message: null, errorMessage: "No data was found for request.");
                    respondentBundleFinalData.FlagAsEmpty();
                    dr.CallbackComplete?.Invoke(dr);
                    return;
                }

                var dataList = reqAres.Data.DynamicValuesFound;
                service.Logger.Trace("Adding {0} elements to original Data list.", dataList.Count);
                reqOriginalData.AddRange(dataList);
            }
            #endregion

            PrepareReport(service, reportMaster, message: "Step 1/5 finished. (Server A GET)");
            if (ct.IsCancellationRequested)
            {
                service.Logger.Debug("Request cancelled.");
                dr.CallbackCancelled?.Invoke(dr);
                respondentBundleFinalData.FlagAsUnusable();
                return;
            }

            #region Étape 2: télécharge les infos du répondant cible, questionnaire B.

            var reqProcessedData = new List<RequestRespondentDataDynamicValue>();

            requestHasMore = true;
            while (requestHasMore)
            {
                var requestBParams = new RequestGetRespondentDataPayload
                {
                    RespondentIdentification = new RespondentIdentificationStrategy
                    {
                        Strategy = new RespondentIdentificationStrategyDefinition
                        {
                            TerminalId = ProcessedTerminalId,
                            QuestionnaireId = ProcessedQuestionnaireId,
                            FailOnEmpty = false,
                            ExternalUniqueIdentifier = dr.GetRespondentUniqueId(),
                            StrategyType = RequestRespondentDataIdentificationStrategyEnum.ExternalIdentifier
                        }
                    },
                    DynamicValuesFilter = new RespondentDataDynamicValueRequestFilter
                    {
                        MinimalDate = requestMinDate,
                        MaximumDate = requestMaxDate,
                        DynamicKeysList = ProcessedDataKeyFilters
                    }
                };
                var reqBres = await ServerRestRequestsUtil.GetRespondentData(ct, currentServerConf, jujuToken, requestBParams);
                if (reqBres == null || reqBres.Result != ServerRestRequestResultType.Success)
                {
                    PrepareReport(service, reportMaster, message: null, errorMessage: "Could not complete download request. Request for processed (B) data failed.");
                    dr.CallbackError?.Invoke(dr);
                    respondentBundleFinalData.FlagAsUnusable();
                    return;
                }

                requestHasMore = false;
                /* TODO Pas de pagination
                requestHasMore = reqAres.Data.HasMore; // Continue ou arrête.
                requestMinDate = reqAres.Data.LastElementDate;
                if (requestHasMore) service.Logger.Debug("More original elements exists. Requesting new data.");
                */
                if (reqBres.Data != null && (reqBres.Data?.DynamicValuesFound?.Any() ?? false))
                {
                    var dataList = reqBres.Data.DynamicValuesFound;
                    service.Logger.Trace("Adding {0} elements to processed Data list.", dataList.Count);
                    reqProcessedData.AddRange(dataList);
                }
                else
                {
                    service.Logger.Trace("No processed data found for this respondent. Skipping.");
                }
            }

            #endregion

            PrepareReport(service, reportMaster, message: "Step 2/5 finished. (Server B GET)");
            if (ct.IsCancellationRequested)
            {
                service.Logger.Debug("Request cancelled.");
                dr.CallbackCancelled?.Invoke(dr);
                respondentBundleFinalData.FlagAsUnusable();
                return;
            }

            #region Étape 3: Évaluer quels fichiers existent, et lesquels télécharger.

            // reqProcessed VS reqOrigin
            foreach (var rod in reqOriginalData)
            {
                var dataGroup = new RespondentBundleDataGroup
                {
                    OriginalValueHeader = rod
                };
                respondentBundleFinalData._fullData.TryAdd(rod, dataGroup);

                var index = 0;
                foreach (var item in rod.Values)
                {
                    dataGroup.Items.TryAdd(item, new RespondentBundleDataFile
                    {
                        OriginalDataValue = item,
                        OriginalDataIndex = index
                    });
                    index++;
                }
            }
            
            var nbrProcessedItems = 0;
            foreach (var rpd in reqProcessedData)
            {
                if (rpd.Values.Count < 4)
                {
                    service.Logger.Warn("Invalid processed data. Skipping. " + rpd.ToStringPublicProperties(1));
                    continue;
                }

                var creDateStr = rpd.Values[1];
                var modDateStr = rpd.Values[2];
                var success = true;
                //var creDate = DateTime.ParseExact(rpd.Values[1], DateUtil.UtcTimezoneFormatString, CultureInfo.InvariantCulture);
                //var modDate = DateTime.ParseExact(rpd.Values[2], DateUtil.UtcTimezoneFormatString, CultureInfo.InvariantCulture);
                success &= DateTime.TryParseExact(creDateStr, DateUtil.UtcTimezoneFormatString, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var creDate);
                success &= DateTime.TryParseExact(modDateStr, DateUtil.UtcTimezoneFormatString, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var modDate);

                if (!success)
                {
                    // Problème de formatage
                    service.Logger.Warn("Could not process date format. Skipping processed values:\n" + StringUtil.ConcatStringsMultiline(rpd.Values));
                    continue;
                }

                var keySearch = new RequestRespondentDataDynamicValue
                {
                    ValuesKey = rpd.Values[0],
                    CreationDate = creDate,
                    ModificationDate = modDate
                };

                if (!respondentBundleFinalData._fullData.TryGetValue(keySearch, out var keyValues))
                {
                    service.Logger.Warn("Processed data could not find original data. Skipping. =>\n" + rpd.ToStringPublicProperties(1));
                    continue;
                }
                
                // Pour le reste du tableau, on assigne les valeurs trouvées comme "achetés"
                for (var i = 3; i < rpd.Values.Count; i++)
                {
                    var processedItem = rpd.Values[i];
                    if (keyValues.Items.ContainsKey(processedItem))
                    {
                        keyValues.Items[processedItem].Processed = true;
                        nbrProcessedItems++;
                    }
                    else
                    {
                        service.Logger.Warn("No matching item value found for processed data ({0}) in following original data header: {1}", processedItem, keySearch.ToStringPublicProperties(1));
                    }
                }
            }
            
            service.Logger.Debug("{0} items were found already processed.", nbrProcessedItems);
            #endregion
            
            PrepareReport(service, reportMaster, message: $"Step 3/5 finished. (processed items elimination: {nbrProcessedItems} already processed on B server)");
            if (ct.IsCancellationRequested)
            {
                service.Logger.Debug("Request cancelled.");
                dr.CallbackCancelled?.Invoke(dr);
                respondentBundleFinalData.FlagAsUnusable();
                return;
            }

            #region Étape 4: Analyser les fichiers existants.

            // Comme il est très possible que les noms de fichiers n'aient aucun liens (alphanumérique) direct avec les jetons de téléchargement,
            // Il faut garder un lien entre les jetons et les fichiers reçus. Cela se fait avec un fichier local.
            var fileAssociationFile = new FileInfo(Path.Combine(workDir.FullName, "____fileAssoc.json"));
            Dictionary<string, string> fileAssicDico;
            if (!fileAssociationFile.Exists)
            {
                fileAssicDico = new Dictionary<string, string>();
                JsonUtil.WriteJsonFile(fileAssociationFile, fileAssicDico);
                fileAssociationFile.Refresh();
            }
            else
            {
                fileAssicDico = JsonUtil.ReadJsonFile<Dictionary<string, string>>(fileAssociationFile);
            }
            
            using (var iterVals = respondentBundleFinalData._fullData.Values.GetEnumerator())
            {
                while (iterVals.MoveNext())
                {
                    var currDataGroup = iterVals.Current;
                    if (currDataGroup == null) continue;

                    // Création d'un sous-répertoire pour ce groupe
                    var subDirPath = Path.Combine(workDir.FullName, currDataGroup.OriginalValueHeader.CreationDate.ToString(DateUtil.UtcTimezoneFormatString).Replace(':', '_'));
                    DirectoryInfo subDir;
                    if (!Directory.Exists(subDirPath))
                    {
                        subDir = Directory.CreateDirectory(subDirPath);
                        service.Logger.Trace("Created local subdirectory for event: {0}", subDirPath);
                    }
                    else
                    {
                        subDir = new DirectoryInfo(subDirPath);
                    }

                    currDataGroup.LocalDirectory = subDir;

                    using(var iter = currDataGroup.Items.Values.GetEnumerator())
                    {
                        while (iter.MoveNext())
                        {
                            var dataFile = iter.Current;
                            if (dataFile == null) continue;
                            if (!dataFile.IsDownloadableFile()) continue; //  N'est pas un fichier.
                            // On vérifie s'il existe un lien association déjà existant.
                            
                            dataFile.LocalDirectory = subDir;

                            if (fileAssicDico.TryGetValue(dataFile.OriginalDataValue, out var filePath))
                            {
                                if (!File.Exists(filePath))
                                {
                                    fileAssicDico.Remove(dataFile.OriginalDataValue);
                                    service.Logger.Warn("Previous association found no existing file. Deleting association: {0} => {1}", dataFile.OriginalDataValue, filePath);
                                    continue;
                                }

                                dataFile.Downloaded = true;
                                dataFile.LocalFile = new FileInfo(filePath);
                                service.Logger.Trace("File already downloaded. Assigning from association dictionary: {0}", filePath);
                            }
                            else
                            {
                                // N'existe pas, donc on ajoute à la liste de téléchargeemnt.
                                filesToDownload.Add(dataFile);
                            }
                        }
                    }
                }
            }
            
            #endregion
            
            PrepareReport(service, reportMaster, message: $"Step 4/5 finished. (Files to download identified: {filesToDownload.Count} remaining)");
            if (ct.IsCancellationRequested)
            {
                service.Logger.Debug("Request cancelled.");
                dr.CallbackCancelled?.Invoke(dr);
                respondentBundleFinalData.FlagAsUnusable();
                return;
            }

            #region Étape 5 : on télécharge les fichiers manquants.
            
            service.Logger.Debug("Preparing to download {0} files from server.", filesToDownload.Count);
            reportMaster.TotalFilesDownloaded = filesToDownload.Count;

            // On va diviser le travail en morceau, pour ne pas télécharger un fichier immense
            for (var currentFileIndex = 0; currentFileIndex < filesToDownload.Count ; currentFileIndex += 20)
            {
                var toTake = 20;
                if (currentFileIndex + toTake >= filesToDownload.Count - 1)
                {
                    toTake = filesToDownload.Count - currentFileIndex;
                }
                var nextBatch = filesToDownload.GetRange(currentFileIndex, toTake);

                service.Logger.Trace("Starting batch download for elements. Indexes => {0}/{1}.", currentFileIndex, currentFileIndex+toTake-1);

                // Construction de la requête.
                var tokenToDownload = new List<string>();
                foreach (var dv in nextBatch)
                {
                    if (!dv.IsDownloadableFile())
                    {
                        service.Logger.Warn("A value passed that was not a downloadable file. Discarding. Value: {0}", dv.OriginalDataValue);
                        continue;
                    }
                    tokenToDownload.Add(dv.OriginalDataValue);
                }
                
                var downloadParameters = new RequestDownloadZipFilePayload
                {
                    FilesToDownload = tokenToDownload
                };

                var tmpZipFile = new FileInfo(DirectoryUtil.CreateLocalTemporaryFile(StringUtil.GenerateGuidString() + ".zip"));

                // TODO Faire des Retry
                var downloadResultTask = ServerRestRequestsUtil.DownloadMultipleFiles(ct, currentServerConf, jujuToken, downloadParameters, tmpZipFile);
                tmpZipFile.Refresh();
                if (downloadResultTask == null || !downloadResultTask.Data || !tmpZipFile.Exists)
                {
                    service.Logger.Error("Could not download ZIP file.");
                    dr.CallbackError?.Invoke(dr);
                    respondentBundleFinalData.FlagAsUnusable();
                    return;
                }

                // Extraire le ZIP
                var tmpDir = DirectoryUtil.ExtractZipToLocalTemporaryDirectory(tmpZipFile.FullName, TemporaryWorkDir);
                if (tmpDir == null)
                {
                    service.Logger.Error("Could not extract ZIP file.");
                    dr.CallbackError?.Invoke(dr);
                    respondentBundleFinalData.FlagAsUnusable();
                    continue; //  TODO Faire mieux et plus robuste.
                }

                tmpDir.Refresh();
                var extractedFiles = tmpDir.GetFiles();
                // Le ZIP contient un fichier d'association.
                var jsonDictFile = extractedFiles.First(f => f.Name.Equals("association.json", StringComparison.InvariantCultureIgnoreCase));
                var fileDic = JsonUtil.ReadJsonFile<Dictionary<string, string>>(jsonDictFile);

                // On compare maintenant le contenu de ce dictionnaire, aux fichiers reçus, on déplace les fichiers et met à jour les infos des éléments des événements
                foreach (var rbdf in nextBatch)
                {
                    if (!fileDic.TryGetValue(rbdf.OriginalDataValue, out var dlFile))
                    {
                        service.Logger.Warn("File token not found in downloaded ZIP content: " + rbdf.OriginalDataValue);
                        rbdf.HasError = true;
                        continue;
                    }
                    // On déplace le fichier dans le bon répertoire.
                    var tmpFileInfo = new FileInfo(Path.Combine(tmpDir.FullName, dlFile));
                    var copyToPath = Path.Combine(rbdf.LocalDirectory.FullName, tmpFileInfo.Name);
                    if (!File.Exists(copyToPath))
                    {
                        try
                        {
                            File.Copy(tmpFileInfo.FullName, copyToPath);

                            if (!File.Exists(copyToPath))
                            {
                                service.Logger.Warn("File could not be moved: {0} -> {1}", dlFile, copyToPath);
                                rbdf.HasError = true;
                                continue;
                            }
                        }
                        catch (System.Exception e)
                        {
                            service.Logger.Error(e, "File could not be moved (with exception): {0} -> {1}", dlFile, copyToPath);
                            rbdf.HasError = true;
                            continue;
                        }
                    }
                    else
                    {
                        service.Logger.Warn("File did already exists. No copy necessary: " + copyToPath);
                    }

                    rbdf.Downloaded = true;
                    rbdf.LocalFile = new FileInfo(copyToPath);

                    reportMaster.TotalFilesDownloaded++;
                    reportMaster.TotalBytesDownloaded += tmpFileInfo.Length;

                    // Met à jour le registre
                    fileAssicDico.Add(rbdf.OriginalDataValue, copyToPath);
                    JsonUtil.WriteJsonFile(fileAssociationFile, fileAssicDico);
                    service.Logger.Trace("File added to event registry: " + copyToPath);

                    PrepareReport(service, reportMaster);
                }

                // Nettoyage du répertoire
                FileUtil.DeleteFolderContent(tmpDir.FullName);
            }

            #endregion

            respondentBundleFinalData.DataIsReady();

            // Opération terminée.
            PrepareReport(service, reportMaster, message: "Respondent data fully synchronized on disk. 5/5");
            dr.CallbackComplete?.Invoke(dr);   
        }
        
        private void PrepareReport(RespondentEventDataDownloadService service, RespondentBundleDownloadReport info, string message = null, string errorMessage = null)
        {
            info.Message = message;
            info.MessageError = errorMessage;
            service.TriggerReport(info);
        }
    }
}