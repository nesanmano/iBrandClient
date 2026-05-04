using System;
using System.IO;
using System.Net;
using NLog;
using NomadLogic.Common.Util;

namespace NomadLogic.iBrandDesktop.Util
{
    public class Ftp
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        public event ProgressBarMaxMin FileProgressBarMinMax;
        public event ProgressBarStep FileProgressBarStep;


        private static readonly string protocol = "ftp://";

        readonly string _address;
        readonly string _userName;
        readonly string _password;
        readonly string _dir;

        readonly string _fileToUpload;

        public event OnUploadFinishedDelegate OnUploadFinished;

        readonly Respondent _respondent;

        public Ftp(string address, string userName, string password, string dir, string fileToUpload, Respondent respondent)
        {
            _address = address;
            _userName = userName;
            _password = password;
            _dir = dir;
            _fileToUpload = fileToUpload;
            _respondent = respondent;
        }


        public void DoWork()
        {
            var success = false;

            FileStream fileStream = null;
            Stream requestStream = null;

            try
            {
                if (File.Exists(_fileToUpload))
                {
                    var fileInfo = new FileInfo(_fileToUpload);

                    // Create FtpWebRequest object from the Uri provided
                    string uploadPath;

                    if (_dir != null)
                        uploadPath = protocol + _address + "/" + _dir + "/" + StringUtil.GetFileName(_fileToUpload);
                    else
                        uploadPath = protocol + _address + "/" + StringUtil.GetFileName(_fileToUpload);

                    if (FtpFileExist(uploadPath))
                    {
                        var request = (FtpWebRequest)WebRequest.Create(uploadPath);
                        request.Credentials = new NetworkCredential(_userName, _password);
                        request.KeepAlive = false;

                        // Set timeout for 30 seconds
                        request.Timeout = 30000;
                        request.Method = WebRequestMethods.Ftp.DeleteFile;

                        var response = (FtpWebResponse)request.GetResponse();
                        response.Close();
                    }

                    var ftpWebRequest = (FtpWebRequest)WebRequest.Create(new Uri(uploadPath));

                    // Provide the WebPermission Credentials
                    ftpWebRequest.Credentials = new NetworkCredential(_userName, _password);

                    // By default KeepAlive is true, where the control connection is not closed
                    // after a command is executed.
                    ftpWebRequest.KeepAlive = false;

                    // Set timeout for 30 seconds
                    ftpWebRequest.Timeout = 30000;

                    // Specify the command to be executed.
                    ftpWebRequest.Method = WebRequestMethods.Ftp.UploadFile;

                    // Specify the data transfer type.
                    ftpWebRequest.UseBinary = true;

                    // Notify the server about the size of the uploaded file
                    ftpWebRequest.ContentLength = fileInfo.Length;

                    FireFileProgressBarMaxMin(0, (int)fileInfo.Length);

                    // The buffer size is Set to 2kb
                    var buffLength = 2048;

                    var buff = new byte[buffLength];

                    // Opens a file stream (System.IO.FileStream) to read the file to be uploaded
                    fileStream = fileInfo.OpenRead();

                    // Stream to which the file to be upload is written
                    requestStream = ftpWebRequest.GetRequestStream();

                    // Read from the file stream 2kb at a time
                    var contentLen = fileStream.Read(buff, 0, buffLength);

                    // Till Stream content ends
                    while (contentLen != 0)
                    {
                        // Write Content from the file stream to the FTP Upload Stream
                        requestStream.Write(buff, 0, contentLen);
                        FireFileProgressBarStep(contentLen);
                        contentLen = fileStream.Read(buff, 0, buffLength);
                    }

                    success = true;

                }
            }
            catch (System.Exception ex)
            {
                //MessageBox.Show(ex.Message, "Upload Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                log.Error(ex);
            }
            finally
            {
                // Close the file stream and the Request Stream
                if (requestStream != null)
                {
                    requestStream.Close();
                    requestStream.Dispose();
                }

                if (fileStream != null)
                {
                    fileStream.Close();
                    fileStream.Dispose();
                }
            }

            if (OnUploadFinished != null)
                OnUploadFinished(success, Convert.ToInt32(_respondent.Id));

        }

        public bool FtpFileExist(string fileName)
        {
            var wc = new WebClient();
            try
            {
                wc.Credentials = new NetworkCredential(_userName, _password);
                var fData = wc.DownloadData(fileName);
                if (fData.Length > -1)
                {
                    return true;
                }
                return false;
            }
            catch (System.Exception)
            {
                // Debug here?
            }
            return false;
        }

        protected void FireFileProgressBarMaxMin(int min, int max)
        {
            if (FileProgressBarMaxMinEvent != null)
                FileProgressBarMinMax(min, max);
        }

        protected void FireFileProgressBarStep(int value)
        {
            if (FileProgressBarStepEvent != null)
                FileProgressBarStep(value);
        }

        public ProgressBarMaxMin FileProgressBarMaxMinEvent
        {
            get
            {
                return FileProgressBarMinMax;
            }

            set
            {
                FileProgressBarMinMax = value;
            }

        }

        public ProgressBarStep FileProgressBarStepEvent
        {
            get
            {
                return FileProgressBarStep;
            }

            set
            {
                FileProgressBarStep = value;
            }
        }

    }
}
