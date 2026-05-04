using System;
using System.Drawing;
using System.IO;
using NLog;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Json;
using NUnit.Framework;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen;

namespace NomadLogic.Test.iBrand.Ui.ApplicationScreen
{
    class SimpleProjectConfigTest : TestBaseNunit
    {
        // ReSharper disable once UnusedMember.Local
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        private Point? Point { get; set; }
        [Test]
        public void ParseConfiguration()
        {

            var configFile = BaseDir + "simpleProjectConfig.json";

            var t = Point.HasValue;
            Console.WriteLine(Point.Value.X);

            FileInfo fic = new FileInfo(configFile);

            if(!fic.Exists)
                Assert.Fail();


            SimpleProjectConfig config = JsonUtil.ReadJsonFile<SimpleProjectConfig>(fic);


            Assert.IsTrue(config !=null);

            Rectangle? rec;
            int[] vals;

            foreach (string s in config.HomeScreen.Keys)
            {
                rec = null;   
                Assert.IsTrue(rec == null);

                config.HomeScreen.TryGetValue(s, out vals);

                if(vals?.Length>0)
                {
                    rec = new Rectangle((int)vals.GetValue(0),(int)vals.GetValue(1),(int)vals.GetValue(2),(int)vals.GetValue(3));
                    Assert.IsTrue(rec != null);
                }
            }


            foreach (string s in config.Preview.Keys)
            {
                rec = null;   
                Assert.IsTrue(rec == null);

                config.Preview.TryGetValue(s, out vals);

                if(vals?.Length>0)
                {
                    rec = new Rectangle((int)vals.GetValue(0),(int)vals.GetValue(1),(int)vals.GetValue(2),(int)vals.GetValue(3));
                    Assert.IsTrue(rec != null);
                }
            }

         
        }
    }
}
