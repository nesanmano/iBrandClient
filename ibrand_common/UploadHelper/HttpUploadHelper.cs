using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Text;
using NLog;

namespace NomadLogic.Common.UploadHelper
{
    /// <summary>
    /// Classe d'aide pour envoyer des fichier par requête HTTP, en utilisant le protocole de "multipart".
    /// </summary>
    public class HttpUploadHelper
    {
        private static Logger log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Singleton
        /// </summary>
        private HttpUploadHelper()
        { }

        /// <summary>
        /// Similaire à Upload, mais renvoit la réponse sous forme de String. Utile par exemple si l'on veut analyser la réponse dans son ensemble,
        /// comme avec su sérialiseur Json par exemple.
        /// </summary>
        /// <param name="url">L'adresse à envoyer la requête.</param>
        /// <param name="files">Les fichiers à envoyer.</param>
        /// <param name="form">Les arguments de la requête (POST).</param>
        /// <returns>La réponse du serveur, sous forme de String.</returns>
        public static string Upload(string url, UploadFile[] files, NameValueCollection form)
        {
            HttpWebResponse resp = Upload((HttpWebRequest)WebRequest.Create(url), files, form);

            using (Stream s = resp.GetResponseStream())
            using (StreamReader sr = new StreamReader(s))
            {
                return sr.ReadToEnd();
            }
        }

        /// <summary>
        /// Envoit une requête HTTP MIME multipart à l'adresse voulue. On peut y associer des fichier (et c'est d'ailleur le principal attrait de
        /// cette classe) et des paramètres en POST.
        /// <br/>L'ensemble de la requête est encodée en UTF-8.
        /// </summary>
        /// <param name="req">La requête HTTP.</param>
        /// <param name="files">Les fichiers à envoyer.</param>
        /// <param name="form">Les paramètres de la requête.</param>
        /// <returns>Une réponse HTTP.</returns>
        public static HttpWebResponse Upload(HttpWebRequest req, UploadFile[] files, NameValueCollection form)
        {
            List<MimePart> mimeParts = new List<MimePart>();

            try
            {
                foreach (string key in form.AllKeys)
                {
                    StringMimePart part = new StringMimePart();

                    part.Headers["Content-Disposition"] = "form-data; name=\"" + key + "\"";
                    part.Headers["Content-Type"] = "text/plain";
                    String test = form[key]; ;
                    part.StringData = form[key];

                    mimeParts.Add(part);
                }

                int nameIndex = 0;

                foreach (UploadFile file in files)
                {
                    StreamMimePart part = new StreamMimePart();

                    if (string.IsNullOrEmpty(file.FieldName))
                        file.FieldName = "file" + nameIndex++;

                    part.Headers["Content-Disposition"] = "form-data; name=\"sendfile" + nameIndex + "\"; filename=\"" + file.FileName + "\"";
                    part.Headers["Content-Type"] = file.ContentType;

                    part.SetStream(file.Data);

                    mimeParts.Add(part);
                }

                string boundary = "----------" + DateTime.Now.Ticks.ToString("x");

                req.ContentType = "multipart/form-data; boundary=" + boundary;
                req.Method = "POST";
                req.KeepAlive = false;
                req.Timeout = 1000 * 60 * 20;//20 minutes

                long contentLength = 0;

                byte[] _footer = Encoding.UTF8.GetBytes("--" + boundary + "--\r\n");

                foreach (MimePart part in mimeParts)
                {
                    contentLength += part.GenerateHeaderFooterData(boundary);
                }

                req.ContentLength = contentLength + _footer.Length;

                byte[] buffer = new byte[8192];
                byte[] afterFile = Encoding.UTF8.GetBytes("\r\n");
                int read;

                using (Stream s = req.GetRequestStream())
                {
                    foreach (MimePart part in mimeParts)
                    {
                        s.Write(part.Header, 0, part.Header.Length);

                        while ((read = part.Data.Read(buffer, 0, buffer.Length)) > 0)
                            s.Write(buffer, 0, read);

                        part.Data.Dispose();

                        s.Write(afterFile, 0, afterFile.Length);
                    }

                    s.Write(_footer, 0, _footer.Length);
                }

                return (HttpWebResponse)req.GetResponse();
            }
            catch (Exception ex)
            {
                log.Fatal(ex);

                foreach (MimePart part in mimeParts)
                    if (part.Data != null)
                        part.Data.Dispose();

                throw ex;
            }
        }
    

    public static HttpWebResponse Upload(HttpWebRequest req, UploadFile file)
    {
        List<MimePart> mimeParts = new List<MimePart>();

        try
        {

                StreamMimePart streamPart = new StreamMimePart();

                if (string.IsNullOrEmpty(file.FieldName))
                    file.FieldName = "file";

                streamPart.Headers["Content-Disposition"] = "form-data; name=\"file\"; filename=\"" + file.FileName + "\"";
                streamPart.Headers["Content-Type"] = file.ContentType;

                streamPart.SetStream(file.Data);

                mimeParts.Add(streamPart);


            string boundary = "----------" + DateTime.Now.Ticks.ToString("x");

            req.ContentType = "multipart/form-data; boundary=" + boundary;
            req.Method = "POST";
            req.KeepAlive = false;
            req.Timeout = 1000 * 60 * 20;//20 minutes

            long contentLength = 0;

            byte[] _footer = Encoding.UTF8.GetBytes("--" + boundary + "--\r\n");

            foreach (MimePart part in mimeParts)
            {
                contentLength += part.GenerateHeaderFooterData(boundary);
            }

            req.ContentLength = contentLength + _footer.Length;

            byte[] buffer = new byte[8192];
            byte[] afterFile = Encoding.UTF8.GetBytes("\r\n");
            int read;

            using (Stream s = req.GetRequestStream())
            {
                foreach (MimePart part in mimeParts)
                {
                    s.Write(part.Header, 0, part.Header.Length);

                    while ((read = part.Data.Read(buffer, 0, buffer.Length)) > 0)
                        s.Write(buffer, 0, read);

                    part.Data.Dispose();

                    s.Write(afterFile, 0, afterFile.Length);
                }

                s.Write(_footer, 0, _footer.Length);
            }
            
                return (HttpWebResponse)req.GetResponse();
        }
        catch (Exception ex)
        {
            log.Fatal(ex);

            foreach (MimePart part in mimeParts)
                if (part.Data != null)
                    part.Data.Dispose();

            throw ex;
        }
    }
}
}