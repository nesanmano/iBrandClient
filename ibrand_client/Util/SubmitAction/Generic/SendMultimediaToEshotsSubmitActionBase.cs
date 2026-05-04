using System;
using NomadLogic.iBrandDesktop.Util.SubmitAction.BaseClasses;

namespace NomadLogic.iBrandDesktop.Util.SubmitAction.Generic
{
    public abstract class SendMultimediaToEshotsSubmitActionBase : AbstractSubmitAction
    {
        protected override void InstanciateSubmitActionPools()
        {
            AddSubmitActionWorkerPool("SendFileToEshots", 1);
        }

        protected override void ValidateConfigData()
        {
            if (DebugDoNotSendToEshots) return;
            if (OriginalFileQuestion < 1) throw new SystemException("No original file question in config file.");
            if (EventDaysIdQuestion < 1) throw new SystemException("No event days id question in config file.");

            // On a besoin soit du courriel, soit d'un identifiant eShots. Si on a aucund es deux, on invalide.
            if (EshotsIdQuestion == -1 && EmailIdQuestion == -1 && EshotsEventTokenQuestion == -1)  
                throw new SystemException("No identifier set. At least onr identifier ID question must be set.");
        }
        
        public bool DebugDoNotSendToEshots
        {
            get
            {
                if (_configData != null && _configData.ContainsKey("debug_no_sending"))
                {
                    return bool.Parse(_configData["debug_no_sending"]);
                }

                return false;
            }
        }

        public int OriginalFileQuestion
        {
            get
            {
                if (_configData != null && _configData.ContainsKey("original_file_qno"))
                {
                    return int.Parse(_configData["original_file_qno"]);
                }

                return -1;
            }
        }
        
        public int EshotsEventTokenQuestion
        {
            get
            {
                if (_configData != null && _configData.ContainsKey("eshots_eventtokenid_qno"))
                {
                    return int.Parse(_configData["eshots_eventtokenid_qno"]);
                }

                return -1;
            }
        }

        public int EshotsIdQuestion
        {
            get
            {
                if (_configData != null && _configData.ContainsKey("eshotsid_qno"))
                {
                    return int.Parse(_configData["eshotsid_qno"]);
                }

                return -1;
            }
        }

        public int EventDaysIdQuestion
        {
            get
            {
                if (_configData != null && _configData.ContainsKey("eventday_qno"))
                {
                    return int.Parse(_configData["eventday_qno"]);
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

        public int EmailIdQuestion
        {
            get
            {
                if (_configData != null && _configData.ContainsKey("emailid_qno"))
                {
                    return int.Parse(_configData["emailid_qno"]);
                }

                return -1;
            }
        }
    }
}