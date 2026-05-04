using System;
using System.IO;
using System.Linq;
using NLog;
using NomadLogic.Common.Util.DateAndTime;
using NomadLogic.Common.Util.Networking.VimeoApi;
using NUnit.Framework;
using Assert = NUnit.Framework.Assert;

namespace NomadLogic.Test.Common.Util
{
    /// <summary>
    /// Classe de test pour le nouveau système de requête Vimeo.
    /// </summary>
    public class VimeoUtilTest : TestBaseNunit
    {
        // ReSharper disable once UnusedMember.Local
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        private const string TestAlbumName = "unitTestUpload";

        /// <summary>
        /// Teste la méthode pour obtenir les informations sur le compte usager.
        /// </summary>
        [TestCase("MainAccount")]
        [TestCase("InfoAccount")]
        [TestCase("TestAccount")]
        public void TestAccountInfoGet(string accountName)
        {
            Assert.IsTrue(Enum.TryParse<VimeoAccountLogin>(accountName, out var account));
            var test = VimeoApi.GetInstance(account);
            var result = test.GetAccountInfo();
            
            Assert.That(result != null);
            Console.Out.WriteLine("Account name: " + result.Name);
            Console.Out.WriteLine("Main link: " + result.Websites[0].Link);
            Console.Out.WriteLine("Created: " + result.CreatedTime.ToString(DateUtil.UtcTimezoneFormatString));
            // Juste pour vérifier que on reçu quelque chose de valide.
            Assert.That(result.UploadQuota.Space.Max == result.UploadQuota.Space.Free + result.UploadQuota.Space.Used);
        }

        [Test]
        public void TestAlternateAccountInfoGet()
        {
            const VimeoAccountLogin account = VimeoAccountLogin.InfoAccount;
            var test = VimeoApi.GetInstance(account);
            var result = test.GetAccountInfo();

            Assert.That(result != null, $"Could not get account information for account: {account}");
            Assert.That(result.Name.Equals("Frederic Simard Fournier"));
            Assert.That(result.Websites.Count == 0);
            Assert.AreEqual(new DateTime(2016, 07, 07, 10, 28, 25), result.CreatedTime);
            Assert.That(result.UploadQuota.Space.Max == result.UploadQuota.Space.Free + result.UploadQuota.Space.Used);
        }

        /// <summary>
        /// Test les opération CRUD sur un album (Create, Read, Update, Destroy).
        /// </summary>
        [Test]
        public void TestAlbumCrudMethod()
        {
            var test = VimeoApi.GetInstance();

            const string testAlbumName = "unitTest";

            // Album ne doit pas exister au départ.
            var resultListAlbums = test.ListAlbums();
            var albumFound = resultListAlbums.FirstOrDefault(a => a.Name.Equals(testAlbumName));
            if (albumFound != null)
            {
                test.DestroyAlbum(albumFound.Id);
                Assert.Fail("Album previously existed. Attempted to destroy it, then fail test afterwards.");
            }

            // Création de l'album
            var createdAlbum = test.CreateAlbum(testAlbumName);
            Assert.IsNotNull(createdAlbum);

            // Lecture de l'album
            var albumCheck = test.GetAlbum(createdAlbum.Id);
            Assert.IsNotNull(albumCheck);
            Assert.AreNotSame(createdAlbum, albumCheck);
            Assert.AreEqual(createdAlbum, albumCheck);

            // Mise à jour de l'album
            albumCheck.Description = "changed";
            Assert.That(test.UpdateAlbum(albumCheck));

            var albumModCheck = test.GetAlbum(albumCheck.Id); // Même identifiant d'album
            Assert.IsNotNull(albumModCheck);
            Assert.AreNotSame(albumCheck, albumModCheck);
            
            // NOTE: Pour une raison bizarre, le API ne change pas la date de modification après une modification.
            Assert.That(albumModCheck.Description.Equals("changed"));
            Assert.AreNotEqual(albumModCheck, createdAlbum);

            // Destruction de l'album
            Assert.That(test.DestroyAlbum(createdAlbum.Id));
            resultListAlbums = test.ListAlbums();
            Assert.That(!resultListAlbums.Any(a => a.Name.Equals(createdAlbum.Name)));
            var singleCheck = test.GetAlbum(createdAlbum.Id);
            Assert.IsNull(singleCheck);
        }

        /// <summary>
        /// Teste l'upload d'un vidéo vers un album, le listage de son contenu, la destruction des vidéos dans l'album.
        /// L'album en question n'est pas détruit, mais peut être vide.
        /// </summary>
        [Test]
        public void TestUploadVideoToAlbum()
        {
            AddVideoToTestAlbum("swing_green.mp4");
        }

        private void AddVideoToTestAlbum(string fileName)
        {
            var test = VimeoApi.GetInstance();

            // Album ne doit pas exister au départ.
            var resultListAlbums = test.ListAlbums();
            var albumFound = resultListAlbums.FirstOrDefault(a => a.Name.Equals(TestAlbumName));
            if (albumFound == null)
            {
                // Création de l'album
                albumFound = test.CreateAlbum(TestAlbumName);
                Assert.IsNotNull(albumFound);
            }

            // Upload un vidéo
            var videoFile = BaseDir + fileName;
            Assert.That(File.Exists(videoFile));

            var videoInfo = test.UploadVideo(new FileInfo(videoFile));
            Assert.IsNotNull(videoInfo);

            // Assigne le vidéo à l'album
            var successAssignToAlbum = test.AddVideoToAlbum(videoInfo, albumFound);
            Assert.That(successAssignToAlbum);
        }

        [Test]
        public void TestUploadVideoBigFile()
        {
            AddVideoToTestAlbum("original_green_120fps.mp4");
        }
    }
}
