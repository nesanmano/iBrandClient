// ReSharper disable RedundantUsingDirective

using NLog;

//using ps = Photoshop;

namespace NomadLogic.Multimedia.Adobe
{
    /// <summary>
    /// Cette englobe les commandes nécessaires pour interfacer avec l'application Photoshop.
    /// La version supportée de Photoshop est: Photoshop CC 2015
    /// </summary>
    public abstract class PhotoshopUtils
    {
        
        // ReSharper disable once UnusedMember.Local
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        /*
        /// <summary>
        /// Exécute un script via Photoshop sur une image sur disque.
        /// Si on le désire, on peut demander de sauvegarder le document une fois le script fait dans un autre fichier.
        /// </summary>
        /// <param name="originalFile">Le fichier qui sert de base pour les modifications.</param>
        /// <param name="outputFile">Optionnel. Le fichier de sortie. Doit être différent du fichier original.</param>
        /// <param name="scriptFile">Le fichier de script Javascript pour l'opération.</param>
        /// <returns>Si une exception a eu lieu, on retourne une reférence vers cette dernière.</returns>
        public static System.Exception ExecuteScriptOnFile(FileInfo originalFile, FileInfo outputFile, FileInfo scriptFile)
        {
            if (originalFile == null || !originalFile.Exists || scriptFile == null || !scriptFile.Exists ||
                (outputFile != null && originalFile.FullName.Equals(outputFile.FullName)))
            {
                return new ArgumentException("Invalid arguments.");
            }

            var app = new ps.Application { DisplayDialogs = ps.PsDialogModes.psDisplayNoDialogs };
            var doc = app.Open(originalFile.FullName);

            try
            {
                // Exécution du script, qui fait un hello world
                // 1- Identification du code d'Action de script. Cette valeur est constante, mais on peut la chercher de façon programmatique.
                var scriptCode = app.StringIDToTypeID("AdobeScriptAutomation Scripts");
                // 2- Identification du code d'action pour activer une action. Peut aussi devenir une constante.
                var actionCode = app.CharIDToTypeID("jsCt");
                // Création de la classe qui englobe les détails de l'action à exécuter
                var actionDesc = new ps.ActionDescriptor();
                actionDesc.PutPath(actionCode, scriptFile.FullName);
                app.ExecuteAction(scriptCode, actionDesc, ps.PsDialogModes.psDisplayNoDialogs);
                
                SaveCurrentDocumentToFile(outputFile, doc);
            }
            catch (System.Exception e)
            {
                log.Error(e);
                return e;
            }
            finally
            {
                doc?.Close(ps.PsSaveOptions.psDoNotSaveChanges);
            }

            if (outputFile == null)
            {
                return null;
            }

            return !File.Exists(outputFile.FullName) ? new System.Exception("Output file is missing.") : null;
        }

        private static void SaveCurrentDocumentToFile(FileSystemInfo outputFile, ps.Document doc)
        {
            if (outputFile == null || doc == null) return;

            object options = new ps.JPEGSaveOptions();
            var ext = StringUtil.GetFileExtension(outputFile.FullName);
            if (ext.Equals("png", StringComparison.InvariantCultureIgnoreCase))
            {
                options = new ps.PNGSaveOptions();
            }

            if (outputFile.Exists)
            {
                outputFile.Delete();
            }

            doc.SaveAs(outputFile.FullName, options, true, ps.PsExtensionType.psLowercase);
        }

        /// <summary>
        /// Exécute une action Photoshop sur l'image donnée en paramètre.
        /// </summary>
        /// <param name="originalFile">Fichier original.</param>
        /// <param name="outputFile">Fichier résultant. Optionnel.</param>
        /// <param name="actionGroup">Groupe d'action (le contenant de l'action à exécuter)</param>
        /// <param name="actionName">Le nom de l'action à exécuter.</param>
        /// <returns>Null si tout va bien, sinon, une exception décrivant l'erreur.</returns>
        public static System.Exception ExecuteActionOnFile(FileInfo originalFile, FileInfo outputFile, string actionGroup, string actionName)
        {
            var app = new ps.Application { DisplayDialogs = ps.PsDialogModes.psDisplayNoDialogs };
            ps.Document doc = null;

            try
            {
                var iteDocs = app.Documents.GetEnumerator();
                while (iteDocs.MoveNext())
                {
                    var curDoc = iteDocs.Current as ps.Document;
                    curDoc?.Close(ps.PsSaveOptions.psDoNotSaveChanges);
                }

                doc = app.Open(originalFile.FullName);
                
                // Exécution de l'action
                var playEvent = app.CharIDToTypeID("Ply ");
                var playEventDescriptor = new ps.ActionDescriptor();
                var idnull = app.CharIDToTypeID("null");
                var actionReference = new ps.ActionReference();
                var actionRefId = app.CharIDToTypeID("Actn");
                actionReference.PutName(actionRefId, actionName);
                var idASet = app.CharIDToTypeID("ASet");
                actionReference.PutName(idASet, actionGroup);
                playEventDescriptor.PutReference(idnull, actionReference);
                app.ExecuteAction(playEvent, playEventDescriptor, ps.PsDialogModes.psDisplayNoDialogs);

                SaveCurrentDocumentToFile(outputFile, doc);
            }
            catch (System.Exception e)
            {
                log.Error(e, "Erreur dans l'exécution du groupe '{0}', action '{1}'.", actionGroup, actionName);
                return e;
            }
            finally
            {
                doc?.Close(ps.PsSaveOptions.psDoNotSaveChanges);
            }

            return null;
        }
        */
    }
}
