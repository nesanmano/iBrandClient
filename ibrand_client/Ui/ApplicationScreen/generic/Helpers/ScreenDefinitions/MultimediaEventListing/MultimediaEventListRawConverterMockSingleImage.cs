using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using NomadLogic.Common.Util.DateAndTime;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers.ScreenDefinitions.MultimediaEventListing
{
    [PublicAPI]
    public class MultimediaEventListRawConverterMockSingleImage : MultimediaEventListRawConverter
    {
        protected override List<MultimediaEventGalleryRow> StartConversionExec(CancellationToken ct)
        {
            // TODO Ceci est une génération aléatoire pour test
            var rand = new Random();

            var rowList = new List<MultimediaEventGalleryRow>();

            var path = @"C:\Users\Me\Documents\git\iBrand_Client_C_Sharp_Desktop\test_files\";
            var imagesPossible = Directory.GetFiles(path, "*.jpg", SearchOption.TopDirectoryOnly);
            var maxIndex = imagesPossible.Length-1;
            for (var i = 0; i < 10 ; i++) // Dix rangées
            {
                var dateCreated = DateTime.Now - TimeSpan.FromMinutes(rand.Next(1, 300));
                var dateCreatedStr = dateCreated.ToString(_presentationData.CustomTimeFormat);
                var dgDate = dateCreated.ToString(DateUtil.Iso8601FormatString);
                
                var item = new MultimediaEventGalleryRow(_screenInst, _presentationData, dateCreated);

                var rowItems = new MultimediaEventGalleryRowItem[1];

                for (var r = 0; r <  1; r++)
                {
                    var imagePath = imagesPossible[rand.Next(0, maxIndex)];
                    var rowItem = new MultimediaEventGalleryRowItem(dateCreatedStr)
                    {
                        WorkDirectory = _workDirectory,
                        CannotBeSelected = false,
                        FixedMainData = imagePath,
                        OriginalDynamicValue = "mock",
                        OriginalDynamicGroupDataKey = "mock",
                        OriginalDynamicGroupCreationDate = dgDate
                    };
                    rowItems[r] = rowItem;
                }

                item.Init(rowItems);
                item.SelectionChangedEvent += SelectionChangedEventRef;
                rowList.Add(item);
            }
            rowList.Sort();

            return rowList;
        }
    }
}