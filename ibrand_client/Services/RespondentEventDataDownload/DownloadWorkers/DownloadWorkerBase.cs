using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Newtonsoft.Json;
using NLog;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Serveur;
using NomadLogic.Common.Serveur.Requests;
using NomadLogic.Common.Serveur.Structures;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.DateAndTime;
using NomadLogic.Common.Util.Json;

namespace NomadLogic.iBrandDesktop.Services.RespondentEventDataDownload.DownloadWorkers
{
    /// <summary>
    /// Ce téléchargeur va chercher dans un questionnaire les images d'un répondant spécifique.
    /// Il va tenter d'organiser les images en ordre de création lorsque possible (EG: Groupes dynamiques).
    /// Il n'y a pas de logique pour rendre certaines images indisponibles: on assume que tout le contenu référencé est valide à sélectionner.
    /// </summary>
    [PublicAPI]
    public class DownloadWorkerBase : IRespondentEventDataDownloadWorker
    {
        [JsonRequired]
        public int SourceQuestionnaireId { get; set; }

        public int SourceTerminalId { get; set; } = -1;

        [JsonRequired]
        public List<string> SourceDataKeyFilters { get; set; }
        public List<string> SourceQuestionsFilter { get; set; }

        /// <summary>
        /// Indique qu'il faut traiter les duplicats de timestamps (qui ne devraient pas exister) en les fusionnant avec un header existant.
        /// Par défaut, comme les fichiers sont souvent des duplicats, c'est à faux.
        /// </summary>
        public bool ProcessServerDuplicates { get; set; }

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
            try
            {
                var reportMaster = new RespondentBundleDownloadReport
                {
                    StartTime = DateTime.Now
                };

                if (ct.IsCancellationRequested)
                {
                    service.Logger.Trace("Canceled before worker started.");
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

                #region Téléchargement des données.
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
                        },
                        QuestionDescriptionsFilter = SourceQuestionsFilter
                    };

                    if (service.Logger.IsTraceEnabled)
                    {
                        service.Logger.Trace("Fetching data for respondent (UEI => {0}, IDs => {4}/{5}). Filters:\n\t\t- Dates Min='{1}', Max='{2}'\n\t\t- Key Filters={3}",
                            dr.GetRespondentUniqueId(), requestMinDate, requestMaxDate, 
                            StringUtil.FormatEnumerable(SourceDataKeyFilters, false, 0, ", "),
                            SourceQuestionnaireId, SourceTerminalId);
                    }

                    var requestDataResult = await ServerRestRequestsUtil.GetRespondentData(ct, currentServerConf, jujuToken, requestAParams);
                    if (requestDataResult == null || requestDataResult.Result != ServerRestRequestResultType.Success)
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

                    if (requestDataResult.Data == null)
                    {
                        service.Logger.Debug("No data was found for request.");
                        PrepareReport(service, reportMaster, message: null, errorMessage: "No data was found for request.");
                        respondentBundleFinalData.FlagAsEmpty();
                        dr.CallbackComplete?.Invoke(dr);
                        return;
                    }

                    var dataList = requestDataResult.Data.DynamicValuesFound ?? new List<RequestRespondentDataDynamicValue>();
                    service.Logger.Trace("Adding {0} elements to Data list.", dataList.Count);
                    reqOriginalData.AddRange(dataList);

                    // Ajout des informations des questions au questionnaire
                    // TODO Ajouter une option de mapping pour sauver des numéros de questions à d'autres questions (EX: q1 => q2)
                    // TODO Trouver une façon de convertir les images contenues dans les réponses vers des groupes dynamiques affichables dans la gallerie

                    var answersFromRequest = requestDataResult.Data.AnswersFound ?? new List<RequestRespondentDataAnswer>();
                    service.Logger.Trace("Parsing {0} answers.", answersFromRequest.Count);

                    var saveTo = dr.GetSaveObject;
                    if (saveTo == null) continue;

                    service.Logger.Debug("Save object detected. Saving answer data to current questionnaire.");
                    foreach (var ans in answersFromRequest)
                    {
                        var newAns = new Answer
                        {
                            QuestionIdentifier = new QuestionIdentifier(ans.QuestionNo, ans.ChoiceNo, ans.QuestionType),
                            Response = ans.TextValue
                        };
                        if (saveTo.SaveAnswerToQuestion(newAns))
                        {
                            service.Logger.Trace("Answer from server saved: {0}", newAns);
                        }
                        else
                        {
                            service.Logger.Warn("Could not save answer to current questionnaire. Review: {0}", ans);
                        }
                    }
                }
                #endregion

                PrepareReport(service, reportMaster, message: "Step 1/4 finished. (Server GET)");
                if (ct.IsCancellationRequested)
                {
                    service.Logger.Debug("Request cancelled.");
                    dr.CallbackCancelled?.Invoke(dr);
                    respondentBundleFinalData.FlagAsUnusable();
                    return;
                }

                #region Étape 2: Évaluer quels fichiers existent, et lesquels télécharger.
            
                foreach (var rod in reqOriginalData)
                {
                    var dataGroup = new RespondentBundleDataGroup
                    {
                        OriginalValueHeader = rod
                    };
                    var duplicate = !respondentBundleFinalData._fullData.TryAdd(rod, dataGroup);
                    var index = 0;

                    // TODO Ceci est un problème de BD sur le serveur: il devrait être impossible d'avoir deux "header dynamiques" avec exactement le même Timestamp.
                    // Mais malheureusement, le BD serveur contient ce genre d'information.... Donc, je fais le choix de FUSIONNER les deux.
                    // Cette étape est optionnelle car hautement subjective (c'est un work-around de bogue).
                    if (duplicate && ProcessServerDuplicates)
                    {
                        // On remplace l'instance par celle contenu dans la liste déjà établie.
                        dataGroup = respondentBundleFinalData._fullData[rod];
                        index = dataGroup.Items.Count - 1; // On commence l'indexage plus loin, passé les items existants.
                    }
                
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
            
                #endregion
            
                PrepareReport(service, reportMaster, message: $"Step 2/4 finished. Marshall data to gallery format");
                if (ct.IsCancellationRequested)
                {
                    service.Logger.Debug("Request cancelled.");
                    dr.CallbackCancelled?.Invoke(dr);
                    respondentBundleFinalData.FlagAsUnusable();
                    return;
                }

                #region Étape 3: Analyser les fichiers existants.

                // Comme il est très possible que les noms de fichiers n'aient aucun liens (alphanumérique) direct avec les jetons de téléchargement,
                // Il faut garder un lien entre les jetons et les fichiers reçus. Cela se fait avec un fichier local.
                var fileAssociationFile = new FileInfo(Path.Combine(workDir.FullName, "____fileAssoc.json"));
                Dictionary<string, string> fileAssicDico;
                if (!fileAssociationFile.Exists)
                {
                    fileAssicDico = new Dictionary<string, string>();
                    JsonUtil.WriteJsonFile(fileAssociationFile, fileAssicDico);
                    fileAssociationFile.Refresh();
                    service.Logger.Trace("Created file association dictionary: {0}", fileAssociationFile);
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
            
                PrepareReport(service, reportMaster, message: $"Step 3/4 finished. (Files to download identified: {filesToDownload.Count} remaining)");
                if (ct.IsCancellationRequested)
                {
                    service.Logger.Debug("Request cancelled.");
                    dr.CallbackCancelled?.Invoke(dr);
                    respondentBundleFinalData.FlagAsUnusable();
                    return;
                }

                #region Étape 4 : on télécharge les fichiers manquants.
            
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
                PrepareReport(service, reportMaster, message: "Respondent data fully synchronized on disk. 4/4");
                dr.CallbackComplete?.Invoke(dr);
            }
            catch (System.Exception e)
            {
                service.Logger.Error(e, "Exception during download sequence.");
                PrepareReport(service, null, null, "Exception detected. Download failed.");
                dr.CallbackError?.Invoke(dr);
            } 
        }
        
        private void PrepareReport(RespondentEventDataDownloadService service, RespondentBundleDownloadReport info, string message = null, string errorMessage = null)
        {
            info.Message = message;
            info.MessageError = errorMessage;
            service.TriggerReport(info);
        }
    }
}