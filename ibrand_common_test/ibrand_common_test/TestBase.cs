using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NomadLogic.Common.Services;
using NomadLogic.Common.Services.Configuration;
using NomadLogic.Common.Util;

namespace NomadLogic.Test
{
    /// <summary>
    /// Classe de base pour des tests.
    /// </summary>
    ///
    public abstract class TestBase
    {
        protected const string BASE_DIR = @"..\..\..\test_files";
        protected const string EXEC = @"C:\Program Files (x86)\ffmpeg\bin\ffmpeg.exe";
        
        protected string BaseDir
        {
            get {
                var info = new FileInfo(BASE_DIR + Path.DirectorySeparatorChar + "image_01.jpg");
                if (info.Exists)
                {
                    return new DirectoryInfo(BASE_DIR).FullName;
                }

                const string baseDir = @"..\" + BASE_DIR;
                Assert.IsTrue(Directory.Exists(baseDir), Directory.GetCurrentDirectory());
                return new DirectoryInfo(baseDir).FullName;
            }
        }

        protected string OutputBaseDir
        {
            get
            {
                var outputDir = DirectoryUtil.GetLocalApplicationDir() + @"results\";
                return outputDir;
            }
        }

        protected static void InitConfigurationAndNlog() {
            var config = new NomadConfiguration();
            config.InitWithDefaultValue();
            config.ApplicationName = DirectoryUtil.ApplicationNameEnum.Test;
            config.PreferedLanguage = "en";

            ServicesContainer.AddService(config);

            LogUtil.InitNLog();
        }

        protected static void StopNlog()
        {
            LogUtil.StopNLog();
        }

        /// <summary>
        /// Obtient le nom de la méthode courante.
        /// Voir: https://msdn.microsoft.com/en-us/library/system.runtime.compilerservices.callermembernameattribute.aspx
        /// </summary>
        /// <param name="memberName">Automatiquement donné par le framework.</param>
        /// <returns>Le nom de la méthode courante (l'appellante de cette méthode).</returns>
        protected string GetCurrentMethodName([System.Runtime.CompilerServices.CallerMemberName] string memberName = "")
        {
            return memberName;
        }

        /// <summary>
        /// S'assure qu'un répertoire existe, avec assertion.
        /// </summary>
        /// <param name="dir">Le répertoire à vérifier.</param>
        protected virtual void EnsureDirExists(string dir)
        {
            if (!Directory.Exists(dir))
            {
                Assert.IsNotNull(Directory.CreateDirectory(dir));
            }
            Assert.IsTrue(Directory.Exists(dir));
        }

        /// <summary>
        /// S'assure qu'un fichier n'existe pas. SI oui, on le détruit, puis on asserte que le fichier n'existe pas.
        /// </summary>
        /// <param name="file">Le fichier à "vider".</param>
        protected virtual void EnsureFileNotExists(string file)
        {
            if (File.Exists(file))
            {
                File.Delete(file);
            }
            Assert.IsFalse(File.Exists(file));
        }
    }
}
