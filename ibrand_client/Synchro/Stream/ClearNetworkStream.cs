using System.Net.Sockets;

namespace NomadLogic.iBrandDesktop.Synchro.Stream
{
    public class ClearNetworkStream : ISynchroStream
    {
        public ClearNetworkStream(NetworkStream stream)
        {
            NetworkStream = stream;
        }

        public void Write(byte[] buffer, int offset, int size)
        {
            WriteLenght(buffer.Length);
            NetworkStream.Write(buffer, offset, size);
            NetworkStream.Flush();
        }

        public void WriteFinal()
        {
            //Do nothing
        }

        private void WriteLenght(int i)
        {
            var b = new byte[4];

            b[0] = (byte)((i >> 24) & 0xFF);
            b[1] = (byte)((i >> 16) & 0xFF);
            b[2] = (byte)((i >> 08) & 0xFF);
            b[3] = (byte)((i >> 00) & 0xFF);

            NetworkStream.Write(b, 0, b.Length);
        }

        public void CloseFile()
        {
            WriteLenght(0);
        }

        public NetworkStream NetworkStream { get; }

        public void Close()
        {
            NetworkStream.Close();
        }

        public void Dispose()
        {
            NetworkStream?.Dispose();
        }
    }
}
