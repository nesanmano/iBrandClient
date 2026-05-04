using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EmisysCardScannerLib;
using NLog;
using NUnit.Framework;
using PCSC;

namespace NomadLogic.Test.Common.RfidReader
{
    /// <summary>
    /// Classe pour tester le paquetage Nuget "pcsc-sharp" directement, sans passer par des classes internes à Nomad Logic.
    /// Voir ici pour une description du paquetage: "https://github.com/danm-de/pcsc-sharp"
    /// </summary>
    [SuppressMessage("ReSharper", "LocalizableElement")]
    public class SmartCardRawTest : TestBaseNunit
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Test de la librairie Windows PCSC
        /// </summary>
        [Test]
        public void PcscTestConnection()
        {
            var contextFactory = ContextFactory.Instance;
            using (var context = contextFactory.Establish(SCardScope.System))
            {
                Console.WriteLine("Currently connected readers: ");
                var readerNames = context.GetReaders();
                foreach (var readerName in readerNames)
                {
                    Console.WriteLine($"\t{readerName}");
                }
            }
        }

        /// <summary>
        /// Ce test n'est pas totalement automatique. Il va être FAIL si on ne détecte aucun lecteur,
        /// et si on ne lit aucune carte en quelques secondes.
        /// </summary>
        [Test]
        public void PcscReadCard()
        {
            var readerMr800 = new ReaderMR800();
            var ret1 = readerMr800.UpdateReadersStatus();
            if (ret1 == 0)
            {
                Assert.Ignore("No reader detected. Ignoring the test.");
            }

            var readerIndex = (short) readerMr800.ReaderName.TakeWhile(name => !name.Contains("MR800")).Count();
            var connectResult = readerMr800.ConnectReader(readerIndex);
            Assert.That(connectResult);

            Console.WriteLine("Reader connected.");

            // Création d'un Task pour retourner la première carte lue.
            var source = new CancellationTokenSource();
            var token = new CancellationTokenSource().Token;
            var t = new Task<bool>(() =>
            {
                const short GET_MIFAREONE = 0;
                var cardInfo = new byte[20];
                short receiveLen = 0;

                try
                {
                    while (!token.IsCancellationRequested)
                    {
                        Thread.Sleep(200);
                        var ret = readerMr800.GetData(GET_MIFAREONE, ref cardInfo, ref receiveLen);
                        log.Trace("Read data attempt: " + ret);
                        if (ret != ReaderMR800.SUCCESS) continue;

                        string cardUid = null;
                        // Reconstruction de la valeur de la carte en hexadecimal.
                        for (var i = 0; i < cardInfo[0] && i < cardInfo.Length - 1; i++)
                        {
                            var y = "00" + cardInfo[i + 1].ToString("X");
                            cardUid += y.Substring(y.Length - 2);
                        }

                        readerMr800.setText("Reading...", true, 1, 10);
                        //readerMr800.setLed((int) ReaderMR800.LED_COLOR.YELLOW, 100, true);
                        readerMr800.setBuzzer((int) ReaderMR800.BUZZER_BEEP.BEEP_READ);
                        Console.WriteLine("Card read : " + cardUid);
                        return true;
                    }
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine(e);
                    return false;
                }

                return false;
            }, token, TaskCreationOptions.LongRunning);

            t.Start();
            var waitResult = t.Wait(TimeSpan.FromSeconds(10));
            source.Cancel();
            
            readerMr800.DisconnectReader();
            Console.WriteLine("Disconnected reader.");
            Assert.IsTrue(waitResult);
        }
    }
}
