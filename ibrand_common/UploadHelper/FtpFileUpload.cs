using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using NLog;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Json;
using NomadLogic.Common.Util.Networking.Ftp;
using WinSCP;

namespace NomadLogic.Common.UploadHelper
{
    public class FtpFileUpload
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        private static int UploadFileToFtp(FtpConfigurationInfo ftpInfo, int pid, int qid, string fileToSend,
            string subFolder, string modifiedFileName = null)
        {
            var options = new SessionOptions
            {
                Protocol = Protocol.Ftp,
                HostName = ftpInfo.FtpServerIp + ":" + ftpInfo.Port,
                UserName = ftpInfo.Username,
                Password = ftpInfo.Password
            };

            var ftpDirPath = DateTime.Now.Year + "/" + pid + "/" + qid + "/" + subFolder;
            var ftpPath = ftpDirPath + "/" + (modifiedFileName ?? fileToSend);

            var regex = new Regex(@"\W\w{3,4}$");
            if (!regex.IsMatch(ftpPath))
                throw new Exception(
                    $"{fileToSend} is an in valid file path. Revise TokenizedFilesNames in ftp.config. The file path must termiante with a valid media extension");

            using (var session = new Session())
            {
                session.Open(options);

                FtpWebUtil.CreateDirectory(session, ftpDirPath);
                
                // On verify l'existance du fichier sur le FTP
                if (!FtpWebUtil.CheckIfExists(session, ftpPath))
                {
                    var fi = new FileInfo(fileToSend);
                    if (FtpWebUtil.UploadFile(session, ftpPath, fi))
                    {
                        log.Debug(@"File uploaded to FTP: {0}", ftpPath);
                        return 1;
                    }

                    log.Debug("Could not send file to server: : {0}", ftpPath);
                    return 0;
                }

                log.Debug("File already present on server: : {0}", ftpPath);
                return 0;
            }
        }


        public static void FileUploader(FtpConfigurationInfo ftpInfo, int pid, int qid, int terminalId, IRespondent respondent,
            Dictionary<QuestionIdentifier, Answer> answerMap, QuestionIdentifier vimeoQNo)
        {
            foreach (var pattern in ftpInfo.TokenizedFilesNames)
            {
                var tokenizedFile = StringUtil.CreateStringFromTokenizedPatternRespondent(pattern, answerMap, null, respondent);
                var originalFileName = StringUtil.GetFileName(answerMap[vimeoQNo].Response);


                log.Trace(@"File to look for {0}", originalFileName);

                var subFolder = "";
                if (tokenizedFile.Contains(".mp4"))
                    subFolder = "videos/";
                if (tokenizedFile.Contains(".jpg"))
                    subFolder = "pictures/";


                var localFile = DirectoryUtil.GetLocalIbrandTerminalDir(pid, qid, terminalId) + subFolder + originalFileName;


                var regex = new Regex(@".*\/");
                if (regex.Matches(tokenizedFile).Count != 0)
                    subFolder += regex.Matches(tokenizedFile)[0].Value;

                // get the new file name for FTP transfer if available
                string modifiedFileName = null;
                if (!StringUtil.GetFileName(tokenizedFile).Equals(originalFileName))
                    modifiedFileName = StringUtil.GetFileName(tokenizedFile);

                log.Trace(@"File to send over FTP {0}", localFile);
                //fic en local; ici on verfie que le fichier tokenize existe ou pas.
                //le cas ou on a pas de ficher a envoyer sera gerer dans le worker de Vimeo(pour l'envoi Vimeo. On gere le cas ou le questionnaire n'a pas de question Vimeo)
                var fileToSend = localFile;
                Console.WriteLine(fileToSend);
                if (!File.Exists(fileToSend))
                    throw new Exception(
                        $"Ibrand couldn't find this file:{fileToSend}. The configuration file may not be correct. Verify that the tokens are right.");


                if (UploadFileToFtp(ftpInfo, pid, qid, fileToSend, subFolder, modifiedFileName) == -1)
                    throw new Exception(
                        "Upload to FTP Server Failed. This is due to a lack in the infrastructure. We'll try again later eg: network down, server not reachable,insufficient rights ... ");
            }
        }
    }
}