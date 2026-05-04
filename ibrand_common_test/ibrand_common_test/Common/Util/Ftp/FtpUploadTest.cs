using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.DateAndTime;
using NomadLogic.Common.Util.Json;
using NomadLogic.Common.Util.Networking.Ftp;
using NomadLogic.Test.Common.Respondent;
using NUnit.Framework;

namespace NomadLogic.Test.Common.Util.Ftp
{
    /// <summary>
    /// Cette classe aurait un grand besoin d'être nettoyée ou d'être re-créée comme classe de test correcte.
    /// </summary>
    [Obsolete]
    public class FtpUploadTest : TestBaseNunit
    {

        private static string _ipAddr;
        private static int _port;

        private static string _user;

        private static string _password;

        // ReSharper disable once NotAccessedField.Local
        private static string _filePatternToBackup;

        private static MockRespondent _respondent;

        private static FtpPool _ftp;
        private static string _rep;

        private static FtpConfigurationInfo _jsonObject;
        private static FtpConfigurationInfo _sample;

        private FtpPool ConnectToFtpHost()
        {
            var ftp = new FtpPool(_ipAddr, _port, _user, _password);

            _rep = "ftp://" + _ipAddr + ":" + _port;
            //var dirExist = FtpWebUtil.CheckIfDirectoryExists(_rep + "/" + DateTime.Now.Year);

            //Assert.True(dirExist); // si vrai veux dire qu'il a reussi a se connecter sans problème

            return ftp;
        }

        private void BackUpFtpMp4(string pid,string qid,string fileToSend,string subFolder, string modifiedFileName = null)
        {


            var ftpDirPath = _rep + "/" + DateTime.Now.Year + "/" + pid + "/" + qid + "/" + subFolder;
            _ftp.CheckFtpDirectoryPath(ftpDirPath);
           

            var ftpPath = _ftp.GetUploadPath(DateTime.Now.Year + "/" + pid + "/" + qid+ "/" + subFolder, fileToSend);

            if (modifiedFileName != null)
                ftpPath = _ftp.GetUploadPath(DateTime.Now.Year + "/" + pid + "/" + qid + "/" + subFolder, modifiedFileName);


            var regex = new Regex(@"\W\w{3,4}$");
            if(!regex.IsMatch(ftpPath))
                throw new Exception($"{ftpPath} is an in valid file path. Revise TokenizedFilesNames in ftp.config. The file path must termiante with a valid media extension");


            // On verifie si le fichier existe; car on veut eviter de uploader 2 fois
            /*
            if (!_ftp.FtpFileExist(ftpPath,out var err))
            {
                  var fi = new FileInfo(fileToSend);
                _ftp.UploadFile(fi, ftpPath);

                var res = _ftp.FtpFileExist(ftpPath, out err);
                if (res)
                {
                    Console.WriteLine(@"File uploaded to FTP: {0}", ftpPath);
                }

                Assert.True(res); //si la booleenne est vrai, le fic a ete envoye
            }
            else if( err == null)
            {
                Console.WriteLine(@"File already present on server: : {0}", ftpPath);
            }
            else
            {
                
                Console.WriteLine(err);
            }
            */

          
        }
        
       private int  ParseJsonAndConnectToFtp()
       {

           var file = BaseDir + "/ftp_test/ftp.config";
      
            _jsonObject = JsonUtil.CreateInstanceFromJsonConfigFile<FtpConfigurationInfo>(file);

            if (_jsonObject == null)
                throw new Exception("Unable to create instance of FtpConfigurationInfo. Verfy that your ftp.config file is a valid JSON.");

           var type = typeof(FtpConfigurationInfo);
           foreach (var property in type.GetProperties())
           {
               if(property.GetValue(_jsonObject) == null)
                   throw new Exception("One or more properties in the instance of FtpConfigurationInfo are NULL. Verify that your ftp.config file is a valid JSON.");
           }

            _ipAddr = _jsonObject.FtpServerIp;
            _port = int.Parse(_jsonObject.Port);

            _user = _jsonObject.Username;
            _password = _jsonObject.Password;

            _filePatternToBackup = _jsonObject.TokenizedFilesNames.ElementAt(0);

            Assert.IsNotNull(_jsonObject);
            Assert.AreEqual(_sample.FtpServerIp, _jsonObject.FtpServerIp);
            Assert.AreEqual(_sample.Username, _jsonObject.Username);
            Assert.AreEqual(_sample.Password, _jsonObject.Password);
            Assert.AreEqual(_sample.Port, _jsonObject.Port);
            Assert.GreaterOrEqual(_jsonObject.TokenizedFilesNames.Count, _sample.TokenizedFilesNames.Count);
            var regex = new Regex(@"{\w*.*\w}");
            Assert.True(regex.IsMatch(_jsonObject.TokenizedFilesNames.ElementAt(0)));
            return 0;
        }


        private void FileNameGenerator()
        {
            var dico = _respondent.MockAnswer.ToDictionary(a => a.QuestionIdentifier);

            foreach (var pattern in _jsonObject.TokenizedFilesNames)
            {
               


                var tokenizedFile =
                    StringUtil.CreateStringFromTokenizedPatternRespondent(pattern, dico, null, _respondent);
                var originalFileName = StringUtil.GetFileName(dico[new QuestionIdentifier(4)].Response);

                Console.WriteLine(@"File to look for {0}", originalFileName);

                var subFolder = "";
                if (tokenizedFile.Contains(".mp4"))
                    subFolder = "videos/";
                if (tokenizedFile.Contains(".jpg"))
                    subFolder = "pictures/";

                //var localFile = BaseDir + "ftp_test/" + subFolder + StringUtil.GetFileName(tokenizedFile);


                //subFolder = subFolder + tokenizedFile.Substring(0, tokenizedFile.Length - StringUtil.GetFileName(tokenizedFile).Length);

                /*Regex regex = new Regex(@"\/");
                if (regex.Matches(newFile).Count == 0)
                {
                    newFile = BaseDir + "ftp_test/" + subFolder + newFile;
                }*/

                var localFile = BaseDir + "ftp_test/" + subFolder +originalFileName;



                //fic en local
               
                if (!File.Exists(localFile))
                {
                    Assert.Fail();

                }

                //if (tokenizedFile.EndsWith(".mp4") || tokenizedFile.EndsWith(".jpg"))
                    //subFolder = subFolder + tokenizedFile.Substring(0, tokenizedFile.Length - StringUtil.GetFileName(tokenizedFile).Length);
               // else
                //{
                var regex = new Regex(@".*\/");
                if (regex.Matches(tokenizedFile).Count != 0)
                {
                    subFolder = subFolder + regex.Matches(tokenizedFile)[0].Value;
                }

                string modifiedFileName = null;

                //get filename modifier 
                /*Regex regex2 = new Regex(@".*.mp4");
                if (regex2.Matches(tokenizedFile).Count != 0)
                {
                    int modifierLength = regex2.Matches(tokenizedFile)[0].Value.Length; // + "ftp_test/" + subFolder + newFile;

                    modifiedFileName = originalFileName.Substring(0, originalFileName.Length - 4) + tokenizedFile.Substring(modifierLength) + ".mp4";
                }*/
                if(!StringUtil.GetFileName(tokenizedFile).Equals(originalFileName))
                {
                    modifiedFileName = StringUtil.GetFileName(tokenizedFile);
                }



                //fic en local
                var fileToSend = localFile;//BaseDir + "ftp_test/" + subFolder + newFile;
                Console.WriteLine(fileToSend);
                if (!File.Exists(fileToSend))
                {

                    continue;
                }


                BackUpFtpMp4(Convert.ToString(1050), Convert.ToString(978), fileToSend,subFolder, modifiedFileName);
            }


        }



        [Test]
        public void FinalTest()
        {
            if (ParseJsonAndConnectToFtp() == -1) Assert.Fail();
            _ftp = ConnectToFtpHost();

           FileNameGenerator();
           /*try
                {
                    FtpFileUpload.FileUploader(_jsonObject, 978, 925, _respondent,
                        GenerateAnswerMap(_respondent.MockAnswer));
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    // si lance une exception on assert erreur

                }
                finally
                {
                    Console.WriteLine(@"FTP operation finished");
                }*/
        }

        [Test]
        [OneTimeSetUp]
        public void SetupMockRespondent()
        {
            var respondent = new MockRespondent();
            var ans = new List<Answer>()
            {
                new Answer{QuestionNo = 2, ChoiceNo = 1, Response = "dkanderson@gmail.com"},
                new Answer{QuestionNo = 4, ChoiceNo = 1, Response = BaseDir+ "ftp_test/videos/golf_tournament.mp4"},
                new Answer{QuestionNo = 9, ChoiceNo = 1, Response = BaseDir+ "ftp_test/pictures/a37ff320641.jpg"},
                new Answer{QuestionNo = 5, ChoiceNo = 1, Response = "a37ff320641.jpg"},
                new Answer{QuestionNo = 7, ChoiceNo = 1, Response = "Anderson"},
                new Answer{QuestionNo = 6, ChoiceNo = 1, Response = "Dustin"},
                new Answer{QuestionNo = 8, ChoiceNo = 2, Response = BaseDir + "swing_green.mp4"} 
                // "C:\Users\Kugan Mano\Documents\Git\iBrand_Client_C_Sharp_Desktop\test_files\swing_green.mp4"
                // /2018/tes-2018-01/3/2018/07/17/Anderson_Dustin.mp4
            };

            respondent.MockAnswer = ans;

            respondent.Language = "en";
            respondent.Date = DateTime.Now.ToString(DateUtil.UtcTimezoneFormatString);
            _respondent = respondent;

            _sample = new FtpConfigurationInfo
            {
                FtpServerIp = "69.70.163.38",
                Username = "ibrand",
                Password = "1234",
                Port = "51218",
                TokenizedFilesNames = new List<string> { "{Q4}.mp4" }
            };
        }
    }
}
