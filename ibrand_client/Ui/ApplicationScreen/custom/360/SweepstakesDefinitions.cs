using System;
using System.Collections.Generic;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.custom._360
{
    public class SweepstakesDefinitions
    {
        public IDictionary<int, IList<SweepstakesBlock>> BlockDefinitions { get; set; }
    }

    public class SweepstakesBlock
    {
        public DateTime MinDate { get; set; }
        public DateTime MaxDate { get; set; }
        public TimeSpan BlockTime { get; set; }
        public int MaxWinners { get; set; }
        public int CodePrix { get; set; }
        public char CodeSweepBlock { get; set; }
    }
}