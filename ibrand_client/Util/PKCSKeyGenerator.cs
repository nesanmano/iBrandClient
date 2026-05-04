using System.Security.Cryptography;

namespace NomadLogic.iBrandDesktop.Util
{
    public class PkcsKeyGenerator
    {
        readonly byte[] _key = { 0x01, 0x23, 0x45, 0x67, 0x89, 0xab, 0xcd,
                                 0xef, 0x01, 0x23, 0x45, 0x67, 0x89, 0xab, 0xcd,
                                 0xef};

        readonly byte[] _iv = { 0x07, 0x06, 0x05, 0x04, 0x03, 0x02, 0x01, 0x00, 0x07, 0x06, 0x05, 0x04, 0x03, 0x02, 0x01, 0x00 };
        readonly RijndaelManaged _aes;

        public ICryptoTransform Encryptor => _aes.CreateEncryptor(_key, _iv);

        public PkcsKeyGenerator()
        {
            _aes = new RijndaelManaged();

            _aes.Mode = CipherMode.CFB;
            _aes.Padding = PaddingMode.PKCS7;
            _aes.BlockSize = 128;
        }
    }
}
