using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Json;
using NomadLogic.Common.Util.Print;
using NUnit.Framework;

namespace NomadLogic.Test.Common.Util
{
    /// <summary>
    /// Classe pour tester l'impression
    /// </summary>
    [SuppressMessage("ReSharper", "UnusedMember.Local")]
    public class PrintTest : TestBaseNunit
    {
        private const string PRINTER_NAME_DS40 = "DS40";
        private const string PRINTER_NAME_FOXIT_PDF = "Foxit Reader PDF Printer";
        
        [Test]
        public void PrintJobTestSynch()
        {
            var printerName = PrintUtil.GetPrinterNamesList().FirstOrDefault(printer =>
            {
                var test = new PrinterSettings {PrinterName = printer};
                return test.IsDefaultPrinter;
            });
            Assert.IsNotNull(printerName, "No default printer.");

            var lineFile = BaseDir + Path.DirectorySeparatorChar + "carte_hockey_lines.png";
            Assert.IsTrue(File.Exists(lineFile));

            var job = PrintUtil.CreatePrintJob(printerName);
            Assert.IsFalse(job.IsReadyToPrint, "PrintJob should not be ready to print right after creation.");

            job.AddActionDrawBitmap(lineFile);
            Assert.IsTrue(job.IsReadyToPrint, "PrintJob should be ready to print after page action addition.");

            job.DebugPrintAreaForInstanceBitmaps = true;

            // Exécution synchrone.

            var printStartedCheck = false;
            job.PrintStarted += (sender, args) => printStartedCheck = true;
            var printFinishedCheck = false;
            job.PrintFinished += (sender, args) => printFinishedCheck = true;
            var lastPagePrinted = 0;
            job.PagePrinted += (sender, i) => lastPagePrinted = i;

            job.DoWork(null);

            Assert.IsTrue(printStartedCheck, "Print start event did not go off.");
            Assert.IsTrue(printFinishedCheck, "Print finish event did not go off.");
            Assert.AreEqual(1, lastPagePrinted);
        }

        [Test]
        public void PrintJobTestAsynch()
        {
            var printerName = PrintUtil.GetCurrentDefaultPrinterName();
            Assert.IsNotNull(printerName, "No default printer.");

            var job = PrintUtil.CreatePrintJob(printerName);
            Assert.IsFalse(job.IsReadyToPrint, "PrintJob should not be ready to print right after creation.");

            job.AddActionDraw(e =>
            {
                var ps = e.PageSettings;
                var g = e.Graphics;

                var printableArea = ps.PrintableArea;

                const double space = 0.15;

                var sizeX = (int) (printableArea.Width*space);
                var sizeY = (int) (printableArea.Height*space);

                for (double i = 0; i < 1.0;)
                {
                    var x = printableArea.X + (int) (printableArea.Width*i);
                    var y = printableArea.Y + (int) (printableArea.Height*i);
                    g.DrawRectangle(Pens.Black, x, y, sizeX, sizeY);
                    i += space*1.1;
                }

                g.DrawRectangle(Pens.Black,
                    printableArea.X,
                    printableArea.Y,
                    (!ps.Landscape ? printableArea.Width : printableArea.Height) - 1,
                    (!ps.Landscape ? printableArea.Height : printableArea.Width) - 1);
            });
            Assert.IsTrue(job.IsReadyToPrint, "PrintJob should be ready to print after page action addition.");

            job.DebugPrintAreaForInstanceBitmaps = true;

            // Exécution Asynchrone.

            var printStartedCheck = false;
            job.PrintStarted += (sender, args) => printStartedCheck = true;
            var printFinishedCheck = false;
            job.PrintFinished += (sender, args) => printFinishedCheck = true;
            var lastPagePrinted = 0;
            job.PagePrinted += (sender, i) => lastPagePrinted = i;

            var t = Task.Run(() => job.DoWork(null));
            Task.WaitAll(t);

            Assert.IsTrue(printStartedCheck, "Print start event did not go off.");
            Assert.IsTrue(printFinishedCheck, "Print finish event did not go off.");
            Assert.AreEqual(1, lastPagePrinted);
        }

        [Test]
        [SuppressMessage("ReSharper", "LocalizableElement")]
        public void DetectPrinterCurrentSettings()
        {
            var printers = PrintUtil.GetPrinterNamesList().ToList();
            Assert.IsTrue(printers.Contains(PRINTER_NAME_FOXIT_PDF));

            var pSettings = new PrinterSettings
            {
                PrinterName = PRINTER_NAME_FOXIT_PDF,
                Copies = 1
            };

            Console.Write(PrintUtil.ToString(pSettings, 1));
        }

        [Test]
        [SuppressMessage("ReSharper", "LocalizableElement")]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public void ForcePrintSmallCenteredImage()
        {
            var originalImageFile = BaseDir + Path.DirectorySeparatorChar + "image_01.jpg";
            //var originalImageFile = BaseDir + Path.DirectorySeparatorChar + "frame_5x7.jpg";
            //var originalImageFile = BaseDir + Path.DirectorySeparatorChar + "frame_4x6.png";
            Assert.IsTrue(File.Exists(originalImageFile));
            var outputDir = OutputBaseDir + GetCurrentMethodName() + Path.DirectorySeparatorChar;
            EnsureDirExists(outputDir);

            var printers = PrintUtil.GetPrinterNamesList().ToList();
            Assert.IsTrue(printers.Contains(PRINTER_NAME_FOXIT_PDF));

            var job = PrintUtil.CreatePrintJob(PRINTER_NAME_FOXIT_PDF, true);

            var pSettings = job.PrinterSettings;

            var pageSettings = pSettings.DefaultPageSettings;

            // On force l'impression par défaut sur du 6x4
            var paper = PrintUtil.FindPaperSizeFromPrinterSettings(pSettings, "5x7");
            //var paper = PrintUtil.FindPaperSizeFromPrinterSettings(pSettings, "4x6");
            Assert.IsNotNull(paper);

            pageSettings.PaperSize = paper;

            job.AddActionDrawBitmap(originalImageFile, new PrintJobImageOption
            {
                Scaled = true,
                CropMaximizedScale = false,
                UsePageResolution = false,
                VerticalAlignment = PrintJobImageVerticalAlignment.Center,
                HorizontalAlignment = PrintJobImageHorizontalAlignment.Center,
                IgnoreMargins = true,
                LandScape = true,
                DetectOrientation = true
            });

            //job.DebugPrintAreaForInstanceBitmaps = true;

            job.DoWork(null);
        }

        [Test]
        public void CreateAndRecreatePrintJobs()
        {
            var outputDir = OutputBaseDir + GetCurrentMethodName() + Path.DirectorySeparatorChar;
            EnsureDirExists(outputDir);
            var originalConfigFile = new FileInfo(outputDir + "print.config");
            EnsureFileNotExists(originalConfigFile.FullName);

            var printers = PrintUtil.GetPrinterNamesList().ToList();

            var printIndex = new Random().Next(printers.Count);

            var originalJob = PrintUtil.CreatePrintJob(printers[printIndex], true, "test");

            var pSettings = originalJob.PrinterSettings;

            var pageSettings = pSettings.DefaultPageSettings;

            var paperIndex = new Random().Next(pSettings.PaperSizes.Count);
            pageSettings.PaperSize = pSettings.PaperSizes[paperIndex];

            var pji = originalJob.CreatePrintJobInfo();

            JsonUtil.WriteJsonFile(originalConfigFile, pji);
            Assert.IsTrue(File.Exists(originalConfigFile.FullName));

            var pjiFile = JsonUtil.ReadJsonFile<PrintJobInfo>(originalConfigFile);

            Assert.IsTrue(pji.Equals(pjiFile));

            var fileJob = PrintUtil.CreatePrintJob(pjiFile);

            Assert.IsTrue(originalJob.Equals(fileJob));
        }
    }
}