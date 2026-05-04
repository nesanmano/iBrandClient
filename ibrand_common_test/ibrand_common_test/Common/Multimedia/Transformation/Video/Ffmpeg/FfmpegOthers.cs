using System.IO;
using NLog;
using NomadLogic.Common.Util;
using NomadLogic.Multimedia.Cli.Ffmpeg;
using NomadLogic.Multimedia.Gpu;
using NUnit.Framework;

namespace NomadLogic.Test.Common.Multimedia.Transformation.Video.Ffmpeg
{
    public class FfmpegOthers : TestBaseFfmpeg
    {
        // ReSharper disable once UnusedMember.Local
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        
        /// <summary>
        /// "Test" pour exécuter une opération de chroma keying sur une répertoire complet, avec un répertoire de sortie pour tous les résultats.
        /// </summary>
        [Test]
        public void ChromaKeyFullDirectory()
        {
            const string INPUT_DIR = @"I:\photo_statues\original\";
            Assert.IsTrue(Directory.Exists(INPUT_DIR));
            const string CHROMA_KEY_FILE = @"I:\photo_statues\FfmpegChromaKeyInfo.config";
            Assert.IsTrue(File.Exists(CHROMA_KEY_FILE));
            const string OUT_DIR = @"I:\photo_statues\keyed\";
            FileUtil.DeleteFolderContent(OUT_DIR);
            EnsureDirExists(OUT_DIR);

            var chroma = ChromaKeyManager.GetDefaultChromaConfigFfmpeg();

            foreach (var file in FileUtil.GetFilesByExtensionsOrderByName(new DirectoryInfo(INPUT_DIR), ".jpg"))
            {
                var outName = OUT_DIR + StringUtil.RemoveExtension(file.Name) + ".png";
                FfmpegUtil.ChromakeyImage(ExecLocationFfprobe, file.FullName, outName, chroma);
                Assert.IsTrue(File.Exists(outName));
            }
        }
    }
}
