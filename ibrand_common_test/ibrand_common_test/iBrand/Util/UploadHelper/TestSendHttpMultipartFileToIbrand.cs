using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Text;
using System.Web.Script.Serialization;
using NomadLogic.Common.UploadHelper;
using NomadLogic.Common.Util;
using NUnit.Framework;

namespace NomadLogic.Test.iBrand.Util.UploadHelper
{
    /// <summary>
    /// Ensemble de test pour l'envoie d'un fichier vers le serveur iBrand. Requiert un serveur iBrand local pour s'effectuer.
    /// </summary>
    public class TestSendHttpMultipartFileToIbrand : TestBaseNunit
    {
        /// <summary>
        /// Test pour l'envoie d'un seul fichier vers le serveur local.
        /// </summary>
        [Test]
        public void TestSingleFileUpload()
        {
            //Test en local
            const string url = "http://localhost:8080/ibrand_app/UploadFileToQuestionnaire";

            // On va chercher un fichier au hasard dans le répertoire d'image de l'usager actuel.
            var picsJpeg = Directory.GetFiles(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "*.jpg");
            var pick = new Random().Next(picsJpeg.Length);
            UploadFile[] files = {
                new UploadFile(picsJpeg[pick], "source", "image/jpeg")
            };

            Debug.WriteLine("File selected: " + files[0].FileName);

            // Pas de paramètres d'extra
            var form = new NameValueCollection
            {
                {"questionnaire_no", "600"},
                {"project_no", "613"},
                {"directory", StringUtil.generateRandomString()}
            };

            // Envoi de la requête
            var req = (HttpWebRequest)WebRequest.Create(url);

            var response = HttpUploadHelper.Upload(req, files, form);

            // Conversion de la réponse (Json)
            Dictionary<string, string> dico = null;
            if (response.StatusCode == HttpStatusCode.OK)
            {
                string resp = null;
                using (var s = response.GetResponseStream())
                {
                    if (s != null)
                    {
                        using (var sr = new StreamReader(s))
                        {
                            resp = sr.ReadToEnd();
                        }
                    }
                }

                Debug.WriteLine("Response from server: " + resp);

                try
                {
                    var serializer = new JavaScriptSerializer();
                    Assert.IsNotNull(resp);
                    dico = serializer.Deserialize<Dictionary<string, string>>(resp);
                }
                catch (SerializationException e)
                {
                    Debug.WriteLine(e);
                }
            }
            else
            {
                Assert.Fail("Response code not ok.");
            }

            Assert.IsNotNull(dico);
            Assert.IsTrue(dico.Count == 1); // Ne contient qu'un fichier.

            var filename = Path.GetFileName(files[0].FileName);
            FileInfo tmp = null;
            if (filename != null)
            {
                Assert.IsTrue(dico.ContainsKey(filename)); // Contient le nom du fichier comme clé
                Assert.AreNotEqual(filename, dico[filename]); // Sa valeur est différente de la clé.
                {

                    var tmp1 = filename.Substring(filename.LastIndexOf('.'), filename.Length - filename.LastIndexOf('.'));
                    var tmp2 = dico[filename].Substring(dico[filename].LastIndexOf('.'), dico[filename].Length - dico[filename].LastIndexOf('.'));
                    Assert.IsTrue(tmp1.Equals(tmp2)); // Possède la même extension

                }

                // Et surtout, le fichier est accessible par référence web.
                tmp = new FileInfo(Path.GetTempPath() + filename);
                if (File.Exists(tmp.FullName))
                {
                    File.Delete(tmp.FullName);
                }

                using (var client = new WebClient())
                {
                    client.DownloadFile(dico[filename], tmp.FullName);
                }
            }
            else
            {
                Assert.Fail("File name is null.");
            }

            // Et que ce fichier téléchargé à exactement la même taille que l'original.
            Assert.IsTrue(File.Exists(tmp.FullName));
            var ori = new FileInfo(picsJpeg[pick]);
            Assert.IsTrue(File.Exists(ori.FullName));
            Assert.AreEqual(tmp.Length, ori.Length);
        }

        /// <summary>
        /// Test pour la structure JSON d'un fichier multi-part.
        /// </summary>
        [Test]
        public void TestGsonAnswer()
        {
            Dictionary<string, string> dico = null;
            var expectedData = new Dictionary<string, string>();

            string[][] values = {
                               new[] {"111.JPG", "http://localhost:8080/ibrand_app/projects/613/600/on1firp8v1_ptelj82uv2.JPG"},
                               new[] {"222.JPG", "http://localhost:8080/ibrand_app/projects/614/601/oueLw/jbf1j12uv1_ptelj82uv2.JPG"},
                               new[] {"333.JPG", "http://localhost:8080/ibrand_app/projects/615/602/on1firp80acNcCgNEueLw/jbf1j82uv2.JPG"}
                               };
            var sb = new StringBuilder();

            sb.Append('{');
            var first = true;
            foreach (var tab in values)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    sb.Append(',');
                }
                sb.Append('"');
                sb.Append(tab[0]);
                sb.Append('"');
                sb.Append(':');
                sb.Append('"');
                sb.Append(tab[1]);
                sb.Append('"');

                // On en profite pour construire le dictionnaire de comparaison.
                expectedData.Add(tab[0], tab[1]);
            }
            sb.Append('}');

            var gson = sb.ToString();

            Debug.WriteLine("GSON string: " + gson);

            try
            {
                var serializer = new JavaScriptSerializer();
                dico = serializer.Deserialize<Dictionary<string, string>>(gson);
            }
            catch (ArgumentException e)
            {
                Debug.WriteLine(e);
                Assert.Fail("Error during deserialization.");
            }

            // Dictionnaires pas immédiatement comparables, car n'implémentent pas ICollection. Ce qui est con...
            CollectionAssert.AreEquivalent(dico.ToList(), expectedData.ToList());
        }

        /// <summary>
        /// Test pour envoyer plusieurs fichiers à la fois.
        /// </summary>
        [Test]
        public void TestMultipleFileUpload()
        {
            Assert.Fail("Not yet implemented");
        }
    }
}
