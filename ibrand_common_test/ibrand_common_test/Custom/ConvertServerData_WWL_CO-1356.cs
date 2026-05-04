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
    public class ConvertServerData_WWL_CO_1356 : TestBaseNunit
    {
        #region Util Methods
        private static (string, string) ExtractStringDateAndTime(long dateOff, string timeZoneId)
        {
            var dateTimeOff = DateTimeOffset.FromUnixTimeMilliseconds(dateOff);
            var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            var modTime = TimeZoneInfo.ConvertTimeFromUtc(dateTimeOff.UtcDateTime, timeZoneInfo);

            return (modTime.ToString("yyyy-MM-dd"), modTime.ToString("HH:mm:ss"));
        }

        /// <summary>
        /// Pour générer des lignes de données facilement dans une page Excel sans se soucier des indices...
        /// </summary>
        private static void GenerateStatsBlock(int startLine, int startColumn, Cells cells, IEnumerable<IList<string>> data)
        {
            foreach (var line in data)
            {
                for (var i = 0; i < line.Count; i++)
                {
                    cells[startLine, i+startColumn].PutValue(line[i]);
                }
                startLine++;
            }
        }

        /// <summary>
        /// Pour générer des lignes de données facilement dans une page Excel sans se soucier des indices...
        /// Variation pour une seule ligne.
        /// </summary>
        private static void GenerateStatsBlock(int startLine, int startColumn, Cells cells, IList<string> line)
        {
            for (var i = 0; i < line.Count; i++)
            {
                cells[startLine, i+startColumn].PutValue(line[i]);
            }
        }

        /// <summary>
        /// Pour générer des lignes de données facilement dans une page Excel sans se soucier des indices...
        /// Variation avec un tuple précis.
        /// </summary>
        private static void GenerateStatsBlock(int startLine, int startColumn, Cells cells, IEnumerable<(string, int)> data)
        {
            foreach (var (item1, item2) in data)
            {
                cells[startLine, 0+startColumn].PutValue(item1);
                cells[startLine, 1+startColumn].PutValue(item2);
                startLine++;
            }
        }

        /// <summary>
        /// Pour générer des lignes de données facilement dans une page Excel sans se soucier des indices...
        /// Variation pour des paramètres de méthodes.
        /// </summary>
        private static void GenerateStatsBlock(int startLine, int startColumn, Cells cells, params string[] line)
        {
            for (var i = 0; i < line.Length; i++)
            {
                cells[startLine, i+startColumn].PutValue(line[i]);
            }
        }
        #endregion

        [TestCase(@"C:\Users\Me\Documents\Projets\2021\co-1356 WinterWonderLand\sql\results\all_image_data_wwl.csv", "Arab Standard Time")]
        public void ConvertDatabaseInfoXlsx(string filePath, string timeZoneOffsetId)
        {
            var baseDir = GetFullOutputDir();
            EnsureDirExists(baseDir);
            
            Assert.IsTrue(File.Exists(filePath));

            var newFileName = StringUtil.GetFileName(filePath); 
            newFileName = StringUtil.RemoveExtension(newFileName);
            var newFilePath = Path.Combine(baseDir, $"{DateTime.Now:yyyy_MM_dd}_" + newFileName+".xlsx");

            var highestTimeStampValue = long.MinValue;
            var lowestTimeStampValue = long.MaxValue;

            using (var stream = new StreamReader(filePath))
            {
                var wb = new Workbook();

                var sheet = wb.Worksheets[0];
                sheet.Name = "Données";
                // Headers
                var headers = new List<string> {"External Unique Identifier","Database Respondent Id", "Terminal name","Ryadh Date","Ryadh Time","Image file name"};
                GenerateStatsBlock(0,0, sheet.Cells, headers);

                var line = stream.ReadLine();
                var lineNbr = 0;
                while (!string.IsNullOrWhiteSpace(line))
                {
                    lineNbr++;
                    var items = line.Split(';');

                    var timePosix = long.Parse(items[3]);

                    if (timePosix > highestTimeStampValue)
                    {
                        highestTimeStampValue = timePosix;
                    }

                    if (timePosix < lowestTimeStampValue)
                    {
                        lowestTimeStampValue = timePosix;
                    }
                    
                    var (date, hour) = ExtractStringDateAndTime(timePosix, timeZoneOffsetId);
                    GenerateStatsBlock(lineNbr, 0, sheet.Cells, items[0],items[1],items[2],date,hour,items[4]);

                    line = stream.ReadLine();
                }
                
                wb.Worksheets.Add("Statistiques");
                sheet = wb.Worksheets[1];

                // Sauvegarde des informations temporelles
                var (minDate, minDateHour) = ExtractStringDateAndTime(lowestTimeStampValue, timeZoneOffsetId);
                var (maxDate, maxDateHour) = ExtractStringDateAndTime(highestTimeStampValue, timeZoneOffsetId);
                var data = new List<List<string>>
                {
                    new List<string> {"Données générées le:",DateTime.Now.ToLongDateString()},
                    new List<string> {"Ajustement des dates des données:",timeZoneOffsetId},
                    new List<string> {"Plus petite date:", $"{minDate} {minDateHour}", lowestTimeStampValue.ToString()},
                    new List<string> {"Plus grande date:", $"{maxDate} {maxDateHour}", highestTimeStampValue.ToString()}
                };
                GenerateStatsBlock(0,0, sheet.Cells, data);

                wb.Save(newFilePath, SaveFormat.Xlsx);
            }

            Assert.IsTrue(File.Exists(newFilePath));
        }

        [TestCase(@"C:\Users\François Coupal\Documents\projets\2021\co-1356 WinterWonderLand\sql\results\events_camera.csv", "Arab Standard Time")]
        public void ConvertDatabaseCameraEventsXlsx(string filePath, string timeZoneOffsetId)
        {
            var baseDir = GetFullOutputDir();
            EnsureDirExists(baseDir);
            
            Assert.IsTrue(File.Exists(filePath));

            var newFileName = StringUtil.GetFileName(filePath); 
            newFileName = StringUtil.RemoveExtension(newFileName);
            var newFilePath = Path.Combine(baseDir, $"{DateTime.Now:yyyy_MM_dd}_" + newFileName+".xlsx");

            var highestTimeStampValue = long.MinValue;
            var lowestTimeStampValue = long.MaxValue;

            using (var stream = new StreamReader(filePath))
            {
                var wb = new Workbook();

                var sheet = wb.Worksheets[0];
                sheet.Name = "Données";
                // Headers
                var headers = new List<string> {"External Unique Identifier","Date", "Hour", "Nbr association"};
                GenerateStatsBlock(0,0, sheet.Cells, headers);

                var listDaysAssoc = new List<(string, int)>();
                var lastDay = ("", 0);

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

                    var nbrAssoc = int.Parse(items[2]);
                    var (date, hour) = ExtractStringDateAndTime(timePosix, timeZoneOffsetId);
                    GenerateStatsBlock(lineNbr, 0, sheet.Cells, items[0],date,hour,nbrAssoc.ToString());

                    // Calcul du nombre total de events par jour.
                    if (lastDay.Item1 != date)
                    {
                        listDaysAssoc.Add(lastDay);
                        // Les tuples ne sont pas gérées avec des pointers d'instances, donc il faut les ajouter au moment où
                        // on veut s'en débarasser, car c'est une copie qui va dans la liste.
                        lastDay = (date, 0); 
                    }

                    lastDay.Item2 = lastDay.Item2+nbrAssoc;

                    line = stream.ReadLine();
                }
                listDaysAssoc.RemoveAt(0); // Il faut se débarasser du premier élément car il est "vide". Voir plus haut.

                // Sauvegarde des informations groupées par date
                wb.Worksheets.Add("Groupement par jour");
                sheet = wb.Worksheets[1];
                GenerateStatsBlock(0,0, sheet.Cells, "Date", "Number of associations");
                GenerateStatsBlock(1,0, sheet.Cells, listDaysAssoc);
                
                // Sauvegarde des informations temporelles
                wb.Worksheets.Add("Statistiques");
                sheet = wb.Worksheets[2];
                var (minDate, minDateHour) = ExtractStringDateAndTime(lowestTimeStampValue, timeZoneOffsetId);
                var (maxDate, maxDateHour) = ExtractStringDateAndTime(highestTimeStampValue, timeZoneOffsetId);
                var data = new List<List<string>>
                {
                    new List<string> {"Données générées le:",DateTime.Now.ToLongDateString()},
                    new List<string> {"Ajustement des dates des données:",timeZoneOffsetId},
                    new List<string> {"Plus petite date:", $"{minDate} {minDateHour}", lowestTimeStampValue.ToString()},
                    new List<string> {"Plus grande date:", $"{maxDate} {maxDateHour}", highestTimeStampValue.ToString()}
                };
                GenerateStatsBlock(0,0, sheet.Cells, data);

                wb.Save(newFilePath, SaveFormat.Xlsx);
            }

            Assert.IsTrue(File.Exists(newFilePath));
        }

        [TestCase(@"C:\Users\Me\Documents\Projets\2021\co-1356 WinterWonderLand\sql\results\12041_analisys.txt", "Eastern Standard Time")]
        public void ConvertDatabaseSingleUidAnalysisXlsx(string filePath, string timeZoneOffsetId)
        {
            var baseDir = GetFullOutputDir();
            EnsureDirExists(baseDir);
            
            Assert.IsTrue(File.Exists(filePath));

            var newFileName = StringUtil.GetFileName(filePath); 
            newFileName = StringUtil.RemoveExtension(newFileName);
            var newFilePath = Path.Combine(baseDir, $"{DateTime.Now:yyyy_MM_dd}_" + newFileName+".xlsx");

            var highestTimeStampValue = long.MinValue;
            var lowestTimeStampValue = long.MaxValue;

            using (var stream = new StreamReader(filePath))
            {
                var wb = new Workbook();

                var sheet = wb.Worksheets[0];
                sheet.Name = "Données";
                // Headers
                var headers = new List<string> {"External Unique Identifier","Date", "terminal", "image file"};
                GenerateStatsBlock(0,0, sheet.Cells, headers);

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
                    
                    var (date, hour) = ExtractStringDateAndTime(timePosix, timeZoneOffsetId);
                    GenerateStatsBlock(lineNbr, 0, sheet.Cells, items[0],date,hour,items[2], items[3]);

                    line = stream.ReadLine();
                }

                // Sauvegarde des informations temporelles
                wb.Worksheets.Add("Statistiques");
                sheet = wb.Worksheets[1];
                var (minDate, minDateHour) = ExtractStringDateAndTime(lowestTimeStampValue, timeZoneOffsetId);
                var (maxDate, maxDateHour) = ExtractStringDateAndTime(highestTimeStampValue, timeZoneOffsetId);
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