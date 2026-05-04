using System;
using System.IO;
using System.Threading.Tasks;
using NomadLogic.Common.Services;
using NomadLogic.Common.Services.ActivityLogger;
using NomadLogic.Common.Services.Configuration;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Excel;
using NomadLogic.iBrandDesktop.Services;
using NUnit.Framework;

namespace NomadLogic.Test.Common.Services
{
    public class TestActivityLogger : TestBaseNunit
    {
        [Test]
        public async Task TestActivityLogAdd()
        {
            const int NBR_ACT = 10;
            
            var outDir = GetFullOutputDir();
            var testDatabase = outDir + "test_db.db3";
            var excelExportFile = new FileInfo(outDir + "excel.xlsx");
            EnsureDirEmpty(outDir);
            
            var serv = ServicesContainer.GetService<ActivityLoggerService>();
            if (serv == null)
            {
                serv = new ActivityLoggerService(testDatabase);
                Assert.IsTrue(await ServicesContainer.AddService(serv));
                Assert.IsTrue(await serv.StartLongRunning());
                Assert.IsTrue(serv.Status == LongRunningStatus.Working || serv.Status == LongRunningStatus.Idle);
            }

            var dateStart = DateTime.Now;

            var logStat = -1;
            var logMin = -1;
            var locker = new object();

            var prog = new Progress<ActivityLoggerReport>();
            prog.ProgressChanged += (sender, report) =>
            {
                lock (locker)
                {
                    logStat = report.NewLogEntriesSinceStart;
                }
            };

            serv.AddObserver(prog);
            serv.SendLastReport();

            for (var i = 0; i < NBR_ACT; i++)
            {
                serv.LogActivity(new Activity
                {
                    TerminalId = 0,
                    ActionType = ActivityLoggerActionType.Debug,
                    Details = "test data"
                });
            }

            var maxWait = DateTime.Now + TimeSpan.FromSeconds(5);

            // On attend que toutes les activitées soient entrées.
            while (DateTime.Now < maxWait || logStat < logMin)
            {
                await Task.Delay(100);
            }

            Assert.That(logMin <= logStat, "Not all activity was logged.");

            // On teste le rapport.

            var data = serv.GetActivityReport(0, dateStart, DateTime.Now, ActivityLoggerActionType.Debug);

            foreach (var item in data)
            {
                Console.Out.WriteLine(StringUtil.FormatEnumerable(item.ExportDataLine()));
            }
            
            Assert.AreEqual(NBR_ACT, data.Count);

            ExcelUtil.CreateExcelFileFromList(data, excelExportFile, "Test Report", data[0]);
            excelExportFile.Refresh();

            Assert.IsTrue(excelExportFile.Exists);
        }
    }
}