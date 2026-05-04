using NLog;
using System;
using System.IO;
using System.Net;

// ReSharper disable InconsistentNaming

namespace sony_camera_api.Util
{
    public class SimpleLiveviewSlicer
    {

        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        /**
     * Payload data class. See also Camera Remote API specification document to
     * know the data structure.
     */
        public sealed class Payload
        {
            /** jpeg data container */
            public readonly byte[] jpegData;

            /** padding data container */
            public readonly byte[] paddingData;

            /**
             * Constructor
             */
            public Payload(byte[] jpeg, byte[] padding)
            {
                jpegData = jpeg;
                paddingData = padding;
            }
        }

        private static readonly int CONNECTION_TIMEOUT = 2000; // [msec]

        private Stream inputStream;

        private HttpWebResponse response;

        /**
         * Opens Liveview HTTP GET connection and prepares for reading Packet data.
         * 
         * @param liveviewUrl Liveview data url that is obtained by DD.xml or result
         *            of startLiveview API.
         * @throws IOException generic errors or exception.
         */
        public void Open(String liveviewUrl)
        {

            if (inputStream != null || response != null)
            {
                throw new Exception("Slicer is already open.");
            }


            var request = (HttpWebRequest)WebRequest.Create(liveviewUrl);
            request.AllowAutoRedirect = false;
            request.Method = "GET";
            request.ProtocolVersion = HttpVersion.Version11;
            request.Timeout = CONNECTION_TIMEOUT;

            response = (HttpWebResponse)request.GetResponse();

            if (response.StatusCode == HttpStatusCode.OK)
            {
                inputStream = response.GetResponseStream();
            }
        }

        /**
         * Closes the connection.
         * 
         * @throws IOException generic errors or exception.
         */
        public void Close()
        {
            try
            {
                if (inputStream != null)
                {
                    inputStream.Close();
                    inputStream = null;
                }

                if (response != null)
                {
                    response.Close();
                    response = null;
                }
            }
            catch (IOException e)
            {
                log.Error(e, "Close() IOException.");
            }


        }

        public Payload NextPayload()
        {

            Payload payload = null;

            while (inputStream != null && payload == null)
            {
                // Common Header
                var readLength = 1 + 1 + 2 + 4;
                var commonHeader = ReadBytes(inputStream, readLength);
                if (commonHeader == null || commonHeader.Length != readLength)
                {
                    throw new IOException("Cannot read stream for common header.");
                }

                if (commonHeader[0] != 0xFF)
                {
                    throw new IOException("Unexpected data format. (Start byte)");
                }

                switch (commonHeader[1])
                {
                    case 0x12:
                        // This is information header for streaming.
                        // skip this packet.
                        readLength = 4 + 3 + 1 + 2 + 118 + 4 + 4 + 24;
                        ReadBytes(inputStream, readLength);
                        break;
                    case 0x01:
                    case 0x11:
                        payload = ReadPayload();
                        break;
                }
            }
            return payload;
        }

        /**
         * Reads liveview stream and slice one Packet. If server is not ready for
         * liveview data, this API calling will be blocked until server returns next
         * data.
         * 
         * @return Payload data of sliced Packet
         * @throws IOException generic errors or exception.
         */
        public Payload ReadPayload()
        {

            if (inputStream != null)
            {
                // Payload Header
                var readLength = 4 + 3 + 1 + 4 + 1 + 115;
                var payloadHeader = ReadBytes(inputStream, readLength);
                if (payloadHeader == null || payloadHeader.Length != readLength)
                {
                    throw new IOException("Cannot read stream for payload header.");
                }
                if (payloadHeader[0] != 0x24 || payloadHeader[1] != 0x35
                        || payloadHeader[2] != 0x68
                        || payloadHeader[3] != 0x79)
                {
                    throw new IOException("Unexpected data format. (Start code)");
                }
                var jpegSize = bytesToInt(payloadHeader, 4, 3);
                var paddingSize = bytesToInt(payloadHeader, 7, 1);

                // Payload Data
                var jpegData = ReadBytes(inputStream, jpegSize);
                var paddingData = ReadBytes(inputStream, paddingSize);

                return new Payload(jpegData, paddingData);
            }
            return null;
        }

        /**
         * Converts byte array to int.
         * 
         * @param byteData
         * @param startIndex
         * @param count
         * @return
         */
        private static int bytesToInt(byte[] byteData, int startIndex, int count)
        {
            var ret = 0;
            for (var i = startIndex; i < startIndex + count; i++)
            {
                ret = (ret << 8) | (byteData[i] & 0xff);
            }
            return ret;
        }

        /**
         * Reads byte array from the indicated input stream.
         * 
         * @param in
         * @param length
         * @return
         * @throws IOException
         */
        private static byte[] ReadBytes(Stream input, int length)
        {

            var buffer = new byte[1024];
            using (var ms = new MemoryStream())
            {

                while (true)
                {
                    var trialReadlen = Math.Min(buffer.Length, length - (int)ms.Length);
                    if (trialReadlen == 0)
                        break;
                    var readlen = input.Read(buffer, 0, trialReadlen);
                    if (readlen < 0)
                    {
                        break;
                    }
                    ms.Write(buffer, 0, readlen);
                    if (length <= (int)ms.Length)
                    {
                        break;
                    }
                }
                var ret = ms.ToArray();
                return ret;
            }
        }
    }
}
