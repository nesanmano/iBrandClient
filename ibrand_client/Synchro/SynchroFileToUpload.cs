using System;
using NomadLogic.Common;
using NomadLogic.Common.Util;

namespace NomadLogic.iBrandDesktop.Synchro
{
    public partial class UploadProjectData
    {
        public class SynchroFileToUpload
        {
            public SynchroFileToUpload(string localFileName, QuestionTypeEnum questionType, string respondentLg, int respondentId, bool autoClean)
            {
                LocalFileName = localFileName;

                string extension;
                string fileNameWithoutExt;

                if (localFileName.LastIndexOf(".", StringComparison.Ordinal) != -1)
                {
                    extension = localFileName.Substring(localFileName.LastIndexOf(".", StringComparison.Ordinal));
                    fileNameWithoutExt = LocalFileName.Substring(0, localFileName.LastIndexOf(".", StringComparison.Ordinal));
                }
                else
                {
                    extension = ".jpg";
                    fileNameWithoutExt = LocalFileName;
                }
                
                ServerFileName = StringUtil.GetFileName(fileNameWithoutExt) + extension;
                QuestionType = questionType;
                RespondentLanguage = respondentLg;
                RespondentId = respondentId;
                AutoClean = autoClean;
            }

            public SynchroFileToUpload(string localFileName, QuestionTypeEnum questionType, string serverFileName, bool autoClean)
            {
                LocalFileName = localFileName;
                ServerFileName = serverFileName;
                QuestionType = questionType;
                AutoClean = autoClean;
            }

            public string LocalFileName { get; }

            public string RespondentLanguage { get; }

            public int RespondentId { get; }

            public string ServerFileName { get; }

            public QuestionTypeEnum QuestionType { get; }

            public bool AutoClean { get; }
        }
    }
}