using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using NomadLogic.Common.Util;
using NomadLogic.iBrandDesktop.Datasource;

namespace NomadLogic.iBrandDesktop
{
    [PublicAPI]
    public class QuestionnaireInfo
    {
        public QuestionnaireInfo(int id, string name, string fileSrc, int projectId)
        {
            Id = id;
            Name = name;

            FileSrc = fileSrc;
            FileName = StringUtil.GetFileName(fileSrc);

            ProjectId = projectId;
        }

        [Obsolete]
        public void AddEvent(IbrandEventInfo e)
        {
            if (Events == null)
                Events = new List<IbrandEventInfo>();

            Events.Add(e);
        }

        [Obsolete]
        public List<IbrandEventInfo> Events { get; private set; }

        [Obsolete]
        public void AddDatasourceInfo(DatasourceInfo di)
        {
            if (DatasourceInfo == null)
                DatasourceInfo = new List<DatasourceInfo>();

            DatasourceInfo.Add(di);
        }

        [Obsolete]
        public List<DatasourceInfo> DatasourceInfo { get; private set; }

        [Obsolete]
        public List<FileToDownload> FilesToDownload { get; private set; }

        public int Id { get; }

        [Obsolete]
        public void AddFileToDownload(string fileSrc, string type)
        {
            if (FilesToDownload == null)
                FilesToDownload = new List<FileToDownload>();

            FilesToDownload.Add(new FileToDownload(fileSrc, type));
        }

        public int ProjectId { get; }

        //TerminalId is part of the this class. From Juju, Every client is treated as a terminal(end-point).
        public int TerminalID { get; set;}

        public string Name { get; }

        [Obsolete]
        public string FileSrc { get; }

        [Obsolete]
        public string FileName { get; }

        public override string ToString()
        {
            return Name;
        }
    }
}
