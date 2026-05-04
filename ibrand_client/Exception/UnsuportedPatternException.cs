using System;

namespace NomadLogic.iBrandDesktop.Exception
{
    public class UnsuportedPatternException : ApplicationException
    {
        public UnsuportedPatternException()
            : base("Unsuported pattern.")
        {

        }
    }
}
