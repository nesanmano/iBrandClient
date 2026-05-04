using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using NomadLogic.Common.Phidgets;
using NomadLogic.Common.Util.Trigger;
using NomadLogic.Multimedia.Video;
using NUnit.Framework;

namespace NomadLogic.Test.Common.Phidgets
{
    public class PhidgetTriggerTest : TestBaseNunit
    {
        [Test]
        [SuppressMessage("ReSharper", "AccessToDisposedClosure")]
        public async Task PhidgetInitAndShortTest()
        {
            var manager = new PhidgetTriggerManager
            {
                TriggerDeactivationTime = TimeSpan.FromMilliseconds(50)
            };

            var readyCheck = false;
            var eventFired = 0;
            var errorFired = 0;
            
            manager.TriggerReady += () =>
            {
                Assert.IsTrue(manager.IsRunning);
                Console.Out.WriteLine("Phidget digital input Serial connected to: " + manager.GetDigitalInputSerial());
                readyCheck = true;
            };

            manager.Triggered += () =>
            {
                eventFired++;
                var delayFromNext = manager.GetNextPossibleTriggerTime().Milliseconds;
                if (delayFromNext < 0) delayFromNext = 0; // Pas de valeur négative. 0 veut dire disponible dès maintenant.
                Console.Out.WriteLine($"Delay from next trigger: {delayFromNext} ms");
            };

            manager.TriggerError += (sender, s) =>
            {
                errorFired++;
            };

            IProgress<TriggerManagerReport> observer = new Progress<TriggerManagerReport>(s =>
            {
                Console.Out.WriteLine("Mesage received: " + s);
            });

            manager.AddObserver(observer);

            Assert.IsTrue(manager.Init());

            await Task.Delay(8000);

            manager.Dispose();

            Assert.IsTrue(readyCheck, "Ready Event never fired.");
            await Console.Out.WriteLineAsync($"{eventFired} triggers event were detected.");
            await Console.Out.WriteLineAsync($"{errorFired} errors event were detected.");
        }
    }
}