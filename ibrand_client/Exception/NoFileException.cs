using System;

namespace NomadLogic.iBrandDesktop.Exception
{
    public class NoFileException : ApplicationException
    {
        public NoFileException(string message)
            : base(message)
        {
        }
    }
}
