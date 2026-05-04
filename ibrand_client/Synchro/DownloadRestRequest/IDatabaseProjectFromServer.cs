using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using NomadLogic.Common.Serveur;
using NomadLogic.iBrandDesktop.Synchro.JujuPrintstation;

namespace NomadLogic.iBrandDesktop.Synchro.DownloadRestRequest
{
    [PublicAPI]
    public interface IDatabaseProjectFromServer
    {
        int UpdateResourceFileData(int questionnaireId, ResourceFileToDownload fic);

        List<ResourceFileToDownload> GetResourceFiles(int tId);

        int ManageRessourceFiles(int questionnaireId, [NotNull] IEnumerable<ResourceFileToDownload> ressourceList);

        int InsertTerminal(TerminalInfo tdi);

        [Obsolete]
        int InsertPost(string token,int qId,string imgUrl,string imgFileLocalDest);

        [Obsolete]
        int GetTotalNumberOfPosts(int qId);

        [Obsolete]
        List<GalleryUnitElement> GetPosts(int qId,int startIndex, int endIndex);

        TerminalInfo GetTerminal(int terminalId);

        bool IsTerminalAlreadyInserted(int tId);

        int ResetTerminalMetadata([NotNull] TerminalInfo tdi);
    }
}