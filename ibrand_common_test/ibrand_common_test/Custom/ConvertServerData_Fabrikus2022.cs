using System;
using System.Collections.Generic;
using System.IO;
using Aspose.Cells;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Excel;
using NUnit.Framework;

namespace NomadLogic.Test.Custom
{
    // ReSharper disable once InconsistentNaming
    public class ConvertServerData_Fabrikus2022 : TestBaseNunit
    {
        [TestCase(@"C:\Users\Me\Documents\Projets\2022\NL-3 Fabrikus\sql reports\all_unlocked_respondants.csv", "Central Europe Standard Time")]
        public void ConvertDatabaseInfoXlsx(string filePath, string timeZoneOffsetId)
        {
            Assert.IsTrue(File.Exists(filePath));

            var newFileName = StringUtil.GetFileName(filePath); 
            newFileName = StringUtil.RemoveExtension(newFileName);

            var pathName = Path.Combine(StringUtil.GetDirName(filePath), "results");
            var newFilePath = Path.Combine(pathName, $"{DateTime.Now:yyyy_MM_dd}_" + newFileName+".xlsx");

            var highestTimeStampValue = long.MinValue;
            var lowestTimeStampValue = long.MaxValue;

            using (var stream = new StreamReader(filePath))
            {
                var wb = new Workbook();

                var sheet = wb.Worksheets[0];
                sheet.Name = "Données";
                // Headers
                var headers = new List<string> {"Database Respondent Id", "France Date","France Time","RFID"};
                ExcelUtil.GenerateStatsBlock(0,0, sheet.Cells, headers);

                var line = stream.ReadLine();
                var lineNbr = 0;
                while (!string.IsNullOrWhiteSpace(line))
                {
                    lineNbr++;
                    var items = line.Split(';');

                    var timePosix = long.Parse(items[1]);

                    if (timePosix > highestTimeStampValue)
                    {
                        highestTimeStampValue = timePosix;
                    }

                    if (timePosix < lowestTimeStampValue)
                    {
                        lowestTimeStampValue = timePosix;
                    }
                    
                    var (date, hour) = ExcelUtil.ExtractStringDateAndTime(timePosix, timeZoneOffsetId);
                    ExcelUtil.GenerateStatsBlock(lineNbr, 0, sheet.Cells, items[0], date, hour, items[2]);

                    line = stream.ReadLine();
                }
                
                wb.Worksheets.Add("Statistiques");
                sheet = wb.Worksheets[1];

                // Sauvegarde des informations temporelles
                var (minDate, minDateHour) = ExcelUtil.ExtractStringDateAndTime(lowestTimeStampValue, timeZoneOffsetId);
                var (maxDate, maxDateHour) = ExcelUtil.ExtractStringDateAndTime(highestTimeStampValue, timeZoneOffsetId);
                var data = new List<List<string>>
                {
                    new List<string> {"Données générées le:",DateTime.Now.ToLongDateString()},
                    new List<string> {"Ajustement des dates des données:",timeZoneOffsetId},
                    new List<string> {"Plus petite date:", $"{minDate} {minDateHour}", lowestTimeStampValue.ToString()},
                    new List<string> {"Plus grande date:", $"{maxDate} {maxDateHour}", highestTimeStampValue.ToString()}
                };
                ExcelUtil.GenerateStatsBlock(0,0, sheet.Cells, data);

                wb.Save(newFilePath, SaveFormat.Xlsx);
            }

            Assert.IsTrue(File.Exists(newFilePath));
        }
    }
}