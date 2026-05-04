using System.Collections.Generic;

namespace NomadLogic.iBrandDesktop
{
    public class Synchronization
    {
        public Synchronization(string synchroId, string ppc, long no)
        {
            SynchroId = synchroId;
            Ppc = ppc;
            No = no;
        }

        public List<int> RespondentsToSync { get; set; }

        public string SynchroId { get; }

        public string Ppc { get; }

        public long No { get; }
    }
}
