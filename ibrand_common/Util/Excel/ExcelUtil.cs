using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Aspose.Cells;
using JetBrains.Annotations;

namespace NomadLogic.Common.Util.Excel
{
    [PublicAPI]
    public static class ExcelUtil
    {
        public static (string, string) ExtractStringDateAndTime(long dateOff, string timeZoneId)
        {
            var dateTimeOff = DateTimeOffset.FromUnixTimeMilliseconds(dateOff);
            var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            var modTime = TimeZoneInfo.ConvertTimeFromUtc(dateTimeOff.UtcDateTime, timeZoneInfo);

            return (modTime.ToString("yyyy-MM-dd"), modTime.ToString("HH:mm:ss"));
        }

        /// <summary>
        /// Pour générer des lignes de données facilement dans une page Excel sans se soucier des indices...
        /// </summary>
        public static void GenerateStatsBlock(int startLine, int startColumn, Cells cells, IEnumerable<IList<string>> data)
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
        /// </summary>
        public static void GenerateStatsBlock(int startLine, int startColumn, Cells cells, params string[] line)
        {
            for (var i = 0; i < line.Length; i++)
            {
                cells[startLine, i+startColumn].PutValue(line[i]);
            }
        }

        /// <summary>
        /// Pour générer des lignes de données facilement dans une page Excel sans se soucier des indices...
        /// Variation pour une seule ligne.
        /// </summary>
        public static void GenerateStatsBlock(int startLine, int startColumn, Cells cells, IList<string> line)
        {
            for (var i = 0; i < line.Count; i++)
            {
                cells[startLine, i+startColumn].PutValue(line[i]);
            }
        }

        public static bool CreateExcelFileFromList<T>(IList<T> dataList, FileInfo exportFile, string sheetName = null, IExcelHeaderGroup headerGen = null) where T : IExcelItem
        {
            if (dataList == null || !dataList.Any())
            {
                return false;
            }

            if (exportFile.Exists)
            {
                exportFile.Delete();
                exportFile.Refresh();
            }

            var wb = new Workbook();

            var sheet = wb.Worksheets[0];
            sheet.Name = sheetName ?? "Data 1";

            // Création du header
            if (headerGen != null)
            {
                GenerateStatsBlock(0, 0, sheet.Cells, headerGen.ExportHeadersLine() as IList<string>);
            }

            var lineNbr = 0;
            // Listing des valeurs
            foreach (var item in dataList)
            {
                lineNbr++;
                GenerateStatsBlock(lineNbr, 0, sheet.Cells, item.ExportDataLine());
            }

            // Sauvegarde
            wb.Save(exportFile.FullName, SaveFormat.Xlsx);
            exportFile.Refresh();

            return exportFile.Exists;
        }
    }

    public interface IExcelItem
    {
        IList<string> ExportDataLine();
    }

    public interface IExcelHeaderGroup
    {
        IReadOnlyList<string> ExportHeadersLine();
    }
}