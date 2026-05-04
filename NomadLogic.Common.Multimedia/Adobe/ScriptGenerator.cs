using System.IO;
using NLog;

namespace NomadLogic.Multimedia.Adobe
{
    public class ScriptGenerator
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        public static string _flashVideoExtension = ".flv";

        private readonly string _scriptFileName;
        private readonly string _projectName;
        private readonly string _template;
        
        public ScriptGenerator(string scriptFileName, string projectName, string template)
        {
            _scriptFileName = scriptFileName;
            _projectName = projectName;
            _template = template;
        }
        
        public void GenerateScriptFile(string pictureFileName, string introFileName, string outroFileName, string frameFileName, string outputFileName)
        {

            if (File.Exists(_scriptFileName))
                File.Delete(_scriptFileName);


            var scriptStream = CreateScriptFile(_scriptFileName);
            var writer = new StreamWriter(scriptStream);

            WriteAeProject(writer, _projectName);

            if (File.Exists(outputFileName))
            {
                return;
            }

            //WriteVideoSource(writer, backgroundName, 2);
            WriteImage(writer, pictureFileName, 2);
            WriteImage(writer, introFileName, 3);
            WriteImage(writer, outroFileName, 4);
            WriteImage(writer, frameFileName, 5); 


            WriteRenderPart(writer, outputFileName);
            WriteEndScript(writer);
            writer.Flush();
            writer.Close();
            scriptStream.Close();
        }

        private static FileStream CreateScriptFile(string fileName)
        {
            var fileStream = !File.Exists(fileName) ? File.Create(fileName) : new FileStream(fileName, FileMode.Open);

            return fileStream;
        }

        private void WriteAeProject(TextWriter scriptWriter, string aeProjectPath)
        {
            try
            {
                var content = "#target aftereffects\n" +
                                "app.purge( PurgeTarget.ALL_CACHES );\n" + //Empty the cache (better rendering performances)
                                "var projectToOpen = File(\"" + aeProjectPath + "\");\n" +
                                "if (projectToOpen.exists){\n" +
                                "var new_project = app.open(projectToOpen);\n" +
                                "var myComposition = app.project.item(1);\n";
                content = AdaptPath(content);
                scriptWriter.Write(content);
            }
            catch (IOException e)
            {
                log.Error(e);
            }
        }

        public bool WriteVideoSource(StreamWriter scriptWriter, string videoPath, int itemNbr)
        {
            try
            {
                var content = "videoImport = new File(\"" + videoPath + "\");\n" +
                "app.project.item(" + itemNbr + ").replace(videoImport);\n";
                content = AdaptPath(content);
                scriptWriter.Write(content);
                return true;
            }
            catch (IOException)
            {
                return false;
            }
        }

        private void WriteRenderPart(TextWriter scriptWriter, string endVideoName)
        {
            try
            {
                var content = "myProj = app.project;\n" +
                "myRenderQueue = myProj.renderQueue;\n" +
                "myQueueItem = myRenderQueue.items.add(myComposition);\n" +
                "myOM = myQueueItem.outputModule(1);\n" +
                "myOM.applyTemplate(\""+_template+"\");\n" +
                "myOM.file = new File(\"" + endVideoName + "\");\n" +
                "myRenderQueue.render();\n" +
                "myQueueItem.remove();\n" +
                "myProj.close(CloseOptions.DO_NOT_SAVE_CHANGES);\n";
                content = AdaptPath(content);
                scriptWriter.Write(content);
            }
            catch (IOException)
            {
            }
        }

        private void WriteEndScript(TextWriter scriptWriter)
        {
            try
            {
                var content = "}\n" +
                "else\n" +
                "{\n" +
                "alert(\"The project or template specified cannot be found.\");\n" +
                "}\n";
                content = AdaptPath(content);
                scriptWriter.Write(content);
            }
            catch (IOException)
            {
            }
        }

        public bool WriteText(StreamWriter scriptWriter, string text, int layer)
        {
            try
            {
                var content = "text = myComposition.layer(" + layer + ");\n" +
                "textSource = text.property(\"sourceText\");\n" +
                "textSource.setValue(\"" + text + "\");\n";
                content = AdaptPath(content);
                scriptWriter.Write(content);
                return true;
            }
            catch (IOException)
            {
                return false;
            }
        }

        private void WriteImage(TextWriter scriptWriter, string file, int item)
        {
            try
            {
                var content = "var openning = new File(\"" + file + "\");\n" +
                "app.project.item(" + item + ").replace(openning);\n";
                content = AdaptPath(content);
                scriptWriter.Write(content);
            }
            catch (IOException e)
            {
                log.Error(e);
            }
        }

        private static string AdaptPath(string _string)
        {
            return _string.Replace("\\", "/");
        }
    }
}
