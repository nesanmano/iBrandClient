using System;

namespace NomadLogic.Common.Util.Trigger.LaserBeam
{
    internal class Program
    {
        public static void TestMain(string[] args)
        {
            var lbm = new LaserBeamManager("F310", 4);
            var charac = -1;

            lbm.Triggered += Evenement;
            if (!lbm.Init())
            {
                Console.Out.WriteLine("Impossible d'initialiser le gestionnaire du laser.");
                while (charac != 32 && charac != 13)
                {
                    charac = Console.Read();
                }
                return;
            }

            lbm.Start();
            Console.Out.WriteLine("Laser prêt.");

            while (charac != 32 && charac != 13) {
                charac = Console.Read();
            }

            lbm.Stop();
        }

        private static void Evenement()
        {
            Console.Out.WriteLine("Rayon coupé!");
        }
    }
}
