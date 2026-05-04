using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using NomadLogic.Common.Util.RfidReader;
using NUnit.Framework;

namespace NomadLogic.Test.Common.RfidReader
{
    /// <summary>
    /// tests pour le manager de lecteur RFID
    /// </summary>
    public class Mr800RfidReaderTest : TestBaseNunit
    {
        [Test]
        [SuppressMessage("ReSharper", "AccessToDisposedClosure")]
        public void MR800Reader_ConnectionTest()
        {
            var reader = new MR800_RfidReader();
            reader.HardWareDisconnected += (sender, args) =>
            {
                Assert.Fail("Error during usage.");
            };

            if (!reader.RefreshReader())
            {
                Assert.Ignore("No reader detected. Ignoring test.");
            }

            var t = new Task(() =>
            {
                Assert.IsFalse(reader.IsConnected());

                Assert.IsFalse(reader.DisconnectReader());

                Assert.IsTrue(reader.ConnectReader());
                Thread.Sleep(3000);

                Assert.IsTrue(reader.IsConnected());

                Assert.IsTrue(reader.DisconnectReader());
                Thread.Sleep(2000);
            });
            t.Start();
            t.Wait();
            
            reader.Dispose();
        }

        [Test]
        [SuppressMessage("ReSharper", "AccessToDisposedClosure")]
        public void Mr800_SetLedsTest()
        {
            using (var reader = new MR800_RfidReader())
            {
                reader.HardWareDisconnected += (sender, args) =>
                {
                    Assert.Fail("Error during usage.");
                };

                if (!reader.RefreshReader())
                {
                    Assert.Ignore("No reader detected. Ignoring test.");
                }
                
                var t = new Task(() =>
                {
                    Assert.IsTrue(reader.ConnectReader());
                
                    reader.SetLed(MR800_RfidReader.LedColors.Blue, 2, true);
                    Thread.Sleep(5000);
                    reader.SetLed(MR800_RfidReader.LedColors.Yellow, 15, true); // Seule commande qui faut ce qu'on lui demande
                    Thread.Sleep(5000);
                    reader.SetLed(MR800_RfidReader.LedColors.Green, 2, true);
                    Thread.Sleep(5000);
                    reader.SetLed(MR800_RfidReader.LedColors.Red, 2, true);
                    Thread.Sleep(5000);

                    Assert.IsTrue(reader.DisconnectReader());
                    Thread.Sleep(500);
                });
                t.Start();
                t.Wait();
            }
        }

        [Test]
        [SuppressMessage("ReSharper", "AccessToDisposedClosure")]
        public void Mr800_ReadTest()
        {
            var reader = new MR800_RfidReader
            {
                TimeBetweenRead = TimeSpan.FromSeconds(2),
                TimeBetweenSameCardRead = TimeSpan.FromSeconds(2) // On peut lire plusieures fois la même carte dans ce test.
            };

            if (!reader.RefreshReader())
            {
                Assert.Ignore("No reader detected. Ignoring test.");
            }

            var cardReadCount = 0;
            var invalidCardReadCount = 0;
            reader.CardRead += (sender, args) =>
            {
                Console.WriteLine(@"Card read : " + args?.CardUid);
                cardReadCount++;
            };
            reader.InvalidCardRead += (sender, args) =>
            {
                Console.WriteLine(@"Invalid Card read : " + args?.CardUid);
                invalidCardReadCount++;
            };
            reader.HardWareDisconnected += (sender, args) =>
            {
                Assert.Fail("Error during usage.");
            };

            var t = new Task(() =>
            {
                Assert.IsFalse(reader.IsConnected());

                Assert.IsFalse(reader.DisconnectReader());

                Assert.IsTrue(reader.ConnectReader());
                Thread.Sleep(TimeSpan.FromSeconds(10)); // Le test dure 10 secondes.

                Assert.IsTrue(reader.DisconnectReader());
            });
            t.Start();
            t.Wait(TimeSpan.FromSeconds(11));

            reader.Dispose();

            Console.WriteLine(@"Total cards read : " + cardReadCount);
            Console.WriteLine(@"Total invalid cards read : " + invalidCardReadCount);
            Assert.That(cardReadCount > 0);
            Assert.That(invalidCardReadCount == 0);
        }
    }
}
