using System;
using System.Net.Sockets;

namespace NomadLogic.iBrandDesktop.Synchro.Stream
{
    public interface ISynchroStream : IDisposable
    {
        void Write(byte[] buffer, int offset, int size);
        void WriteFinal();
        void CloseFile();
        void Close();
        NetworkStream NetworkStream
        {
            get;
        }
    }
}
