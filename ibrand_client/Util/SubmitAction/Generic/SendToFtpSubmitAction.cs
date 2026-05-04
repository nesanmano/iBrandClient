using System;
using NLog;
using NomadLogic.iBrandDesktop.Util.SubmitAction.BaseClasses;
using NomadLogic.iBrandDesktop.Util.SubmitAction.Generic.Worker;

namespace NomadLogic.iBrandDesktop.Util.SubmitAction.Generic
{
    /// <summary>
    /// Classe pour envoyer un fichier vers un serveur FTP. On peut avoir des optiosn pour detruire le fichier une fois le transfert confirmé, ainsi que pour indiquer que cette
    /// opération est optionnelle si une réponse n'est pas répondue.
    /// <br/>Les options dans le fichier de propriétés sont:
    /// <br/><b>ftp_user</b>: L'utilisateur FTP
    /// <br/><b>ftp_pass</b>: Le mot de passe de l'usager
    /// <br/><b>ftp_url</b>: L'adresse ftp, avec port de communication.
    /// <br/><b>question</b>: Le numéro de question qui contient le fichier à envoyer.
    /// <br/><b>destroy</b>: Booléen. Si l'envoi FTP est fonctionnel, alors on détruit le fichier après le transfert. FALSE par défaut
    /// <br/><b>optional</b>: Booléen. Si vrai, alors on ne génère pas d'erreur si la réponse n'existe pas. Par contre, on en génère une si le fichier n'existe pas.
    /// <br/><b>synch_with_error</b>: Booléen. Si vrai, on met le répondant à synchroniser même si l'envoi FTP n'a pas réussi.
    /// <br/><b>compress_video_file</b>: Booléean. Si vrai, on tente de compresser le fichier avec FFMPEG. Ne pas mettre à vrai si les fichier n'est pas un fichier vidéo valide.
    /// que la question à une réponse. FALSE par défaut.
    /// </summary>
    public class SendToFtpSubmitAction : AbstractSubmitAction
    {
        // ReSharper disable once UnusedMember.Local
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        protected override void InstanciateSubmitActionPools()
        {
            AddSubmitActionWorkerPool("SendFtp", 5);
        }

        protected new void ValidateConfigData()
        {
            if (FtpUser == null) throw new SystemException("No ftp user in config file.");
            if (FtpPassword == null) throw new SystemException("No ftp password in config file.");
            if (FtpAdress == null) throw new SystemException("No ftp adress in config file.");
            if (FtpQuestion < 1) throw new SystemException("No ftp question in config file.");
        }

        public string FtpUser
        {
            get
            {
                if (_configData != null && _configData.ContainsKey("ftp_user"))
                {
                    return _configData["ftp_user"];
                }

                return null;
            }
        }

        public string FtpPassword
        {
            get
            {
                if (_configData != null && _configData.ContainsKey("ftp_pass"))
                {
                    return _configData["ftp_pass"];
                }

                return null;
            }
        }

        public string FtpAdress
        {
            get
            {
                if (_configData != null && _configData.ContainsKey("ftp_url"))
                {
                    return _configData["ftp_url"];
                }

                return null;
            }
        }

        public int FtpPort
        {
            get
            {
                if (_configData != null && _configData.ContainsKey("ftp_port"))
                {
                    return int.Parse(_configData["ftp_port"]);
                }

                return -1;
            }
        }

        public int FtpQuestion
        {
            get
            {
                if (_configData != null && _configData.ContainsKey("question"))
                {
                    return int.Parse(_configData["question"]);
                }

                return -1;
            }
        }

        public bool Optional
        {
            get
            {
                if (_configData != null && _configData.ContainsKey("optional"))
                {
                    return bool.Parse(_configData["optional"]);
                }

                return false;
            }
        }

        public bool DestroyFile
        {
            get
            {
                if (_configData != null && _configData.ContainsKey("destroy"))
                {
                    return bool.Parse(_configData["destroy"]);
                }

                return false;
            }
        }

        protected override ISubmitActionWorker CreateWorker(Respondent resp, bool usePool = true)
        {
            return new SendToFtpSubmitActionWorkerLegacy(this, resp, resp.Id);
        }

        public bool SynchRespondentWithFtpError
        {
            get
            {
                if (_configData != null && _configData.ContainsKey("synch_with_error"))
                {
                    return bool.Parse(_configData["synch_with_error"]);
                }

                return false;
            }
        }

        public bool CompressVideoFile 
        {
            get
            {
                if (_configData != null && _configData.ContainsKey("compress_video_file"))
                {
                    return bool.Parse(_configData["compress_video_file"]);
                }

                return false;
            }
        }

        public string FtpDir
        {
            get
            {
                if (_configData != null && _configData.ContainsKey("ftp_dir"))
                {
                    return _configData["ftp_dir"];
                }

                return null;
            }
        }
    }
}
