using System.Collections.Generic;
using System.Threading;
using NLog;
using NomadLogic.Common.Services;
using NomadLogic.Common.Util.DateAndTime;
using NomadLogic.iBrandDesktop.Services.RespondentEventDataDownload;
using NomadLogic.Multimedia.Cli.ImageMagickCLI.JobScheduler;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers.ScreenDefinitions.MultimediaEventListing
{
    public class MultimediaEventListRawConverterBase : MultimediaEventListRawConverter
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        public MultimediaEventListRawConverterBase()
        {
            /* Vérification qu'un service critique est lancé.
             * TODO Probablement pas le meilleur endroit pour faire cette vérification.
             */
            var imageMagickService = ServicesContainer.GetService<ImageMagickJobSchedulerService>();
            if (imageMagickService != null) return;
            log.Debug("ImageMagickJobSchedulerService not started. Initializing service.");
            var tmp = new ImageMagickJobSchedulerService(); //  Ne requiert pas de configuration spéciale.

            var rez = ServicesContainer.AddService(tmp).GetAwaiter().GetResult();
            if (rez) return;
            log.Error("ImageMagickJobSchedulerService cannot initialize. Error.");
            throw new System.Exception("Could not initialize critical ImageMagickJobSchedulerService. Please investigate.");
        }

        protected override List<MultimediaEventGalleryRow> StartConversionExec(CancellationToken ct)
        {
            var rowList = new List<MultimediaEventGalleryRow>();
            
            log.Trace("Conversion of raw data to Gallery row started.");
            
            var nbrRowProcessed = 0;
            using (var iterRow = _rawData.GetEnumerator())
            {
                while (iterRow.MoveNext())
                {
                    nbrRowProcessed++;
                    var dynamicHeaderRaw = iterRow.Current.Key;
                    var fileGroupRaw = iterRow.Current.Value;

                    var dynamicHeaderDateCreation = dynamicHeaderRaw.CreationDate.ToString(DateUtil.Iso8601FormatString);
                    var datePresentationStr = dynamicHeaderRaw.CreationDate.ToString(_presentationData.CustomTimeFormat);

                    var rowFileDirectory = fileGroupRaw.LocalDirectory; // Contient tous les fichiers du groupe.
                    var megr = new MultimediaEventGalleryRow(_screenInst, _presentationData, dynamicHeaderRaw.CreationDate);

                    // On assigne les events
                    megr.SelectionChangedEvent += SelectionChangedEventRef;

                    var nbrItems = fileGroupRaw.Items.Count;
                    
                    var fileData = new RespondentBundleDataFile[nbrItems];
                    foreach (var itemFile in fileGroupRaw.Items.Values)
                    {
                        fileData[itemFile.OriginalDataIndex] = itemFile;
                    }
                    
                    // Pas de validation à faire sur le nombre d'élément.

                    var validItemList = new List<MultimediaEventGalleryRowItem>();

                    // On fait les associations, et on valide que ce sont des fichiers.
                    for (var i = 0; i < nbrItems; i++)
                    {
                        var ori = fileData[i];

                        var tmpFileItem = new MultimediaEventGalleryRowItem(datePresentationStr) // Type image par défaut.
                        {
                            WorkDirectory = rowFileDirectory,
                            // CannotBeSelected // Pas utilisé ici, on assume que tout les éléments sont sélectionnables.
                            LayoutIndex = i,
                            // FixedThumbnailImageFile // Pas utilisé. On n'a pas de thumbnail associé.
                            FixedMainData = ori.LocalFile.FullName,
                            OriginalDynamicValue = ori.OriginalDataValue,
                            OriginalDynamicGroupDataKey = dynamicHeaderRaw.ValuesKey,
                            OriginalDynamicGroupCreationDate = dynamicHeaderDateCreation
                        };

                        tmpFileItem.ChangeDetected += (sender, args) =>
                        {
                            megr.RowItemUpdate(sender, args);
                        };
                        
                        validItemList.Add(tmpFileItem);
                        log.Trace("Finished converting row item: Tags => principal='{0}'", ori.OriginalDataValue);
                    }
                    
                    // On assigne ces objets.
                    if (!megr.Init(validItemList.ToArray()))
                    {
                        log.Warn("Initialization for row failed.");
                    }
                    else
                    {
                        rowList.Add(megr);
                    }
                }
            }

            // On trie ensuite les rangées selon la date de création originale
            rowList.Sort();

            log.Trace("Conversion finished. Invoking populate action with result.");

            if (rowList.Count != nbrRowProcessed)
            {
                log.Warn("Discordant number of rows detected. Is {0} but should be {1}.", nbrRowProcessed, rowList.Count);
            }

            return rowList;
        }
    }
}