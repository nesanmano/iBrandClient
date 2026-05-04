using System;
using System.Net.Sockets;
using System.Security.Cryptography;
using NomadLogic.iBrandDesktop.Util;

namespace NomadLogic.iBrandDesktop.Synchro.Stream
{
    public class SecureNetworkStream : ISynchroStream
    {
        private readonly NetworkStream _mStream;
        private readonly ICryptoTransform _encryptor;
        private byte[] _bytesToSend;
        private bool _encrypt = true;

        public SecureNetworkStream(NetworkStream stream)
        {
            _mStream = stream;
            _encryptor = new PkcsKeyGenerator().Encryptor;
        }

        public void Write(byte[] buffer, int offset, int size)
        {
            if (_encrypt)
            {
                var toSend = AddBytes(buffer);

                var currentPosition = 0;
                var targetBytes = new byte[1024];

                while (toSend.Length - currentPosition >= _encryptor.InputBlockSize)
                {
                    // Transform the bytes from currentPosition in the
                    // sourceBytes array, writing the bytes to the targetBytes
                    // array.
                    var numBytesRead = _encryptor.TransformBlock(
                        toSend,
                        currentPosition,
                        _encryptor.InputBlockSize,
                        targetBytes,
                        currentPosition);

                    // Advance the current position in the sourceBytes array.
                    currentPosition += numBytesRead;
                }

                if (toSend.Length - currentPosition != 0)
                {
                    _bytesToSend = new byte[toSend.Length - currentPosition];
                    Array.Copy(toSend, currentPosition, _bytesToSend, 0, toSend.Length - currentPosition);
                }
                else
                    _bytesToSend = new byte[0];

                var encryptedBuffer = TrimArray(targetBytes, currentPosition);
                if (encryptedBuffer.Length == 0) return;
                WriteLenght(encryptedBuffer.Length);
                _mStream.Write(encryptedBuffer, 0, encryptedBuffer.Length);
                _mStream.Flush();
            }
            else
            {
                WriteLenght(buffer.Length);
                _mStream.Write(buffer, offset, size);
                _mStream.Flush();
            }
        }


        public void WriteFinal()
        {
            var encryptedBuffer = _encryptor.TransformFinalBlock(_bytesToSend, 0, _bytesToSend.Length);

            WriteLenght(encryptedBuffer.Length);
            _mStream.Write(encryptedBuffer, 0, encryptedBuffer.Length);
            _mStream.Flush();
            _encrypt = false;
        }

        public void CloseFile()
        {
            WriteLenght(0);
        }

        private byte[] AddBytes(byte[] buffer)
        {
            if (_bytesToSend == null)
                return buffer;

            var newBuffer = new byte[buffer.Length + _bytesToSend.Length];

            Array.Copy(_bytesToSend, 0, newBuffer, 0, _bytesToSend.Length);
            Array.Copy(buffer, 0, newBuffer, _bytesToSend.Length, buffer.Length);

            return newBuffer;
        }

        public void Close()
        {
            _mStream.Close();
        }

        public NetworkStream NetworkStream => _mStream;

        private void WriteLenght(int i)
        {
            var b = new byte[4];

            b[0] = (byte)((i >> 24) & 0xFF);
            b[1] = (byte)((i >> 16) & 0xFF);
            b[2] = (byte)((i >> 8) & 0xFF);
            b[3] = (byte)((i >> 0) & 0xFF);

            _mStream.Write(b, 0, b.Length);
        }


        public NetworkStream Stream => _mStream;

        private byte[] TrimArray(byte[] targetArray, int numBytes)
        {
            // Create a new array with the number of valid bytes.
            var returnedArray = new byte[numBytes];
            for (var j = 0; j < numBytes; j++)
            {
                returnedArray[j] = targetArray[j];
            }

            return returnedArray;
        }

        public void Dispose()
        {
            _mStream?.Dispose();
            _encryptor?.Dispose();
        }
    }
}
