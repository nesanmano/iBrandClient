using NLog;

//using ps = Photoshop;

namespace NomadLogic.Test.Common.Multimedia.Transformation.Adobe
{
    public class PhotoshopIntegrationTest : TestBaseNunit
    {
        // ReSharper disable once UnusedMember.Local
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        
        /// <summary>
        /// Test simple pour appeller Photoshop, redimensionner l'image, sauvegarder le résultat, et fermer le document
        /// dans Photoshop.
        /// </summary>
       /* [TestMethod]
        public void PhotoshopCallTest()
        {
            var outDir = Directory.GetCurrentDirectory() + @"\result\" + GetCurrentMethodName();
            EnsureDirExists(outDir);

            var baseImage = BaseDir + Path.DirectorySeparatorChar + "intro.jpg";
            Assert.IsTrue(File.Exists(baseImage), "File does not exists: " + baseImage);
            var resultImage = outDir + Path.DirectorySeparatorChar + "result.png";
            EnsureFileNotExists(resultImage);

            // Ne peut pas prendre des chemins relatifs, alors in faut obtenir le chemin absolu.
            var fileIn = new FileInfo(baseImage);
            var fileOut = new FileInfo(resultImage);

            var app = new ps.Application();
            app.Preferences.RulerUnits = ps.PsUnits.psPixels;
            app.DisplayDialogs = ps.PsDialogModes.psDisplayNoDialogs;
            var iteDocs = app.Documents.GetEnumerator();
            while (iteDocs.MoveNext())
            {
                var curDoc = iteDocs.Current as ps.Document;
                curDoc?.Close(ps.PsSaveOptions.psDoNotSaveChanges);
            }
            var doc = app.Open(fileIn.FullName);

            try
            {
                doc.ResizeImage(200, 200);
                var options = new ps.PNGSaveOptions();
                doc.SaveAs(fileOut.FullName, options, true, ps.PsExtensionType.psLowercase);
            }
            catch (System.Exception e)
            {
                Assert.Fail("Exception: " + e.Message + "\n\t" + e.StackTrace);
            }
            finally
            {
                doc.Close(ps.PsSaveOptions.psDoNotSaveChanges);
            }

            Assert.IsTrue(File.Exists(resultImage), "Result file was not found");
        }

        /// <summary>
        /// Test simple pour appeller Photoshop, et exécuter un script.
        /// </summary>
        [TestMethod]
        public void PhotoshopCallScriptTest()
        {
            var outDir = Directory.GetCurrentDirectory() + @"\result\" + GetCurrentMethodName();
            EnsureDirExists(outDir);
            EnsureDirExists(BaseDir);

            var baseScript = BaseDir + Path.DirectorySeparatorChar + "new_hello_world.jsx";
            Assert.IsTrue(File.Exists(baseScript), "File does not exists: " + baseScript);
            var resultImage = outDir + Path.DirectorySeparatorChar + "result.png";
            EnsureFileNotExists(resultImage);

            // Ne peut pas prendre des chemins relatifs, alors in faut obtenir le chemin absolu.
            var fileIn = new FileInfo(baseScript);
            var fileOut = new FileInfo(resultImage);

            var app = new ps.Application { DisplayDialogs = ps.PsDialogModes.psDisplayNoDialogs };
            var iteDocs = app.Documents.GetEnumerator();
            while (iteDocs.MoveNext())
            {
                var curDoc = iteDocs.Current as ps.Document;
                curDoc?.Close(ps.PsSaveOptions.psDoNotSaveChanges);
            }
            ps.Document doc = null;

            try
            {
                // Exécution du script, qui fait un hello world
                // 1- Identification du code d'Action de script. Cette valeur est constante, mais on peut la chercher de façon programmatique.
                var scriptCode = app.StringIDToTypeID("AdobeScriptAutomation Scripts");
                // 2- Identification du code d'action pour activer une action. Peut aussi devenir une constante.
                var actionCode = app.CharIDToTypeID("jsCt");
                // Création de la classe qui englobe les détails de l'action à exécuter
                var actionDesc = new ps.ActionDescriptor();
                actionDesc.PutPath(actionCode, fileIn.FullName);
                app.ExecuteAction(scriptCode, actionDesc, ps.PsDialogModes.psDisplayNoDialogs);

                doc = app.ActiveDocument;

                var options = new ps.PNGSaveOptions();
                doc?.SaveAs(fileOut.FullName, options, true, ps.PsExtensionType.psLowercase);
            }
            catch (System.Exception e)
            {
                Assert.Fail("Exception: " + e.Message + "\n\t" + e.StackTrace);
            }
            finally
            {
                doc?.Close(ps.PsSaveOptions.psDoNotSaveChanges);
            }

            Assert.IsTrue(File.Exists(resultImage), "Result file was not found");
        }

        /// <summary>
        /// Test simple pour appeller Photoshop, et exécuter un script.
        /// </summary>
        [TestMethod]
        public void PhotoshopUtilScriptTest()
        {
            var outDir = Directory.GetCurrentDirectory() + @"\result\" + GetCurrentMethodName();
            EnsureDirExists(outDir);

            var inImage = BaseDir + Path.DirectorySeparatorChar + "image_01.jpg";
            Assert.IsTrue(File.Exists(inImage), "File does not exists: " + inImage);
            var baseScript = BaseDir + Path.DirectorySeparatorChar + "emboss_fade.jsx";
            Assert.IsTrue(File.Exists(baseScript), "File does not exists: " + baseScript);
            var baseScript2 = BaseDir + Path.DirectorySeparatorChar + "modify_file.jsx";
            Assert.IsTrue(File.Exists(baseScript2), "File does not exists: " + baseScript2);
            var resultImage = outDir + Path.DirectorySeparatorChar + "result.png";
            EnsureFileNotExists(resultImage);
            var resultImage2 = outDir + Path.DirectorySeparatorChar + "result2.png";
            EnsureFileNotExists(resultImage2);

            // Ne peut pas prendre des chemins relatifs, alors in faut obtenir le chemin absolu.
            var fileIn = new FileInfo(inImage);
            var fileOut = new FileInfo(resultImage);
            var fileOut2 = new FileInfo(resultImage2);
            var scriptIn = new FileInfo(baseScript);
            var scriptIn2 = new FileInfo(baseScript2);

            {
                var err = PhotoshopUtils.ExecuteScriptOnFile(fileIn, fileOut, scriptIn);
                Assert.IsNull(err, err?.Message);
                Assert.IsTrue(File.Exists(resultImage), "Result file was not found");
            }

            {
                var err = PhotoshopUtils.ExecuteScriptOnFile(fileIn, fileOut2, scriptIn2);
                Assert.IsNull(err, err?.Message);
                Assert.IsTrue(File.Exists(resultImage2), "Result file was not found");
            }
        }

        /// <summary>
        /// Test pour exécuter une action sur un fichier.
        /// </summary>
        [TestMethod]
        public void PhotoshopCallActionTest()
        {
            var outDir = Directory.GetCurrentDirectory() + @"\result\" + GetCurrentMethodName();
            EnsureDirExists(outDir);

            var inImage = BaseDir + Path.DirectorySeparatorChar + "image_01.jpg";
            Assert.IsTrue(File.Exists(inImage), "File does not exists: " + inImage);
            var resultImage = outDir + Path.DirectorySeparatorChar + "result.jpg";
            EnsureFileNotExists(resultImage);

            // Ne peut pas prendre des chemins relatifs, alors in faut obtenir le chemin absolu.
            var fileIn = new FileInfo(inImage);
            var fileOut = new FileInfo(resultImage);

            var err = PhotoshopUtils.ExecuteActionOnFile(fileIn, fileOut, "Default Actions", "Molten Lead");
            Assert.IsNull(err, err?.Message);

            Assert.IsTrue(File.Exists(resultImage), "Result file was not found");
        }

        /// <summary>
        /// Test d'exécution de l'action de "statue"
        /// </summary>
        [TestMethod]
        public void PhotoshopCallComplexActionTest()
        {
            var outDir = Directory.GetCurrentDirectory() + @"\result\" + GetCurrentMethodName();
            EnsureDirExists(outDir);

            //string[] inFiles = {"statue_model_01.png", "statue_model_02.png" , "statue_model_03.png" , "statue_model_04.png" };

            var inFiles = FileUtil.GetFilesByExtensionsOrderByName(new DirectoryInfo(@"C:\Users\François Coupal\Documents\projets\tests\images"), 50, ".jpg", ".png");

            var filter = new ChromaKeyHslFilter();

            foreach (var file in inFiles)
            {
                Assert.IsTrue(file.Exists, "File does not exists: " + file.FullName);
                var resultImage = outDir + Path.DirectorySeparatorChar + StringUtil.RemoveExtension(file.Name) + "_result.png";
                EnsureFileNotExists(resultImage);

                ImageUtil.RotateJpegToMatchExifData(file.FullName);

                using (var bmp = ImageUtil.LoadBitmapFromFile(file.FullName))
                {
                    using (var keyed = filter.Filter(bmp))
                    {
                        var tmpOutput = DirectoryUtil.CreateLocalTemporaryFile(GenerateUniqueId.GetId() + ".png");
                        ImageUtil.SaveToImageFileWithOptions(keyed, ImageFormat.Png, 8, tmpOutput);
                        //ImageUtil.SaveToJpegWithOptions(keyed, 90, tmpOutput);

                        // Ne peut pas prendre des chemins relatifs, alors in faut obtenir le chemin absolu via la classe FileInfo
                        var fileIn = new FileInfo(tmpOutput);
                        var fileOut = new FileInfo(resultImage);

                        var err = PhotoshopUtils.ExecuteActionOnFile(fileIn, fileOut, "Statue v3a", "statue v3a");
                        Assert.IsNull(err, err?.Message);
                        Assert.IsTrue(File.Exists(resultImage), "Result file was not found");
                    }
                }
            }
        }*/
    }
}
