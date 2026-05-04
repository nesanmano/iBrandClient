using System.IO;
using NomadLogic.Common.Util;
using NomadLogic.iBrandDesktop.Util.SubmitAction;
using NomadLogic.Multimedia.Cli.Ffmpeg;
using NUnit.Framework;

namespace NomadLogic.Test.iBrand.Util.SubmitAction
{
    public class TestPosteCanadaSubmitActionSubmitAction : TestBaseNunit
    {

        /* Pour le test de la transformation du texte, voir ImageUtilTest.PutRotatedCustomFontTextOnImage() */

        [Test]
        public void CreateStarTrekVideo()
        {
            const string baseDir = @"C:\Users\François Coupal\Documents\projets\2016\Pub-2016-01 Star Trek Poste Canada\test\old assets\";

            const string bkg1 = baseDir + "bkg_a.jpg";
            Assert.IsTrue(File.Exists(bkg1));
            const string bkg2 = baseDir + "bkg_b.jpg";
            Assert.IsTrue(File.Exists(bkg2));
            const string sound = baseDir + "sound.mp3";
            Assert.IsTrue(File.Exists(sound));
            const string intro1 = baseDir + "intro.mp4";
            Assert.IsTrue(File.Exists(intro1));
            const string intro2 = baseDir + "outro.mp4";
            Assert.IsTrue(File.Exists(intro2));
            const string grain = baseDir + "grain overlay.png";
            Assert.IsTrue(File.Exists(grain));
            const string logo = baseDir + "overlay_logo.png";
            Assert.IsTrue(File.Exists(logo));
            var chroma = new FfmpegChromaKeyInfo()
            {
                Red = 104
                ,Green = 93
                ,Blue = 172
                ,Similarity = 0.1f
                ,Blend = 0.05f
            };

            var dir = new DirectoryInfo(@"C:\Users\François Coupal\Documents\projets\2016\PUB-2016-01 Star Trek Poste Canada\test\re-render");
            Assert.IsTrue(dir.Exists);

            var files = dir.GetFiles();

            foreach (var file in files)
            {
                var output = @"C:\Users\François Coupal\Documents\projets\2016\PUB-2016-01 Star Trek Poste Canada\test\re-render\renders\" + StringUtil.RemoveExtension(file.Name) + ".mp4";
                EnsureFileNotExists(output);
                
                PosteCanadaStarTrekSubmitAction.CreateStarTrekVideo(output, bkg1, bkg2, file.FullName, chroma, intro1,
                    intro2, grain, "1280x1920", "2840:2130:900:0", logo, 0.25f, 2.25f, 0.25f, 2.25f, 5.0f, 6.0f, sound, "");

                Assert.IsTrue(File.Exists(output));
            }
        }
    }
}