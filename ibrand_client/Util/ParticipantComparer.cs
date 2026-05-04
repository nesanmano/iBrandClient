using System.Collections.Generic;

namespace NomadLogic.iBrandDesktop.Util
{
    class ParticipantComparer : IComparer<Participant>
    {
        public int Compare(Participant p1, Participant p2)
        {
            if (p1.Score < p2.Score)
                return 1;
            if (p1.Score > p2.Score)
                return -1;
            if (p1.Time > p2.Time)
                return 1;
            if (p1.Time < p2.Time)
                return -1;

            return 0;
        }
    }
}