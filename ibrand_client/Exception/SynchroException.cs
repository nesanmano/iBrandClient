using System;

namespace NomadLogic.iBrandDesktop.Exception
{
    class SynchroException : ApplicationException
    {

        public SynchroException(string message)
            : base(message)
        {
        }

        public SynchroException(string message, System.Exception e)
            : base(message, e)
        {
        }
    }
}
