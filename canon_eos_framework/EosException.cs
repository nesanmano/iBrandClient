using System;
using Canon.Eos.Framework.Helper;

namespace Canon.Eos.Framework
{
    public class EosException : Exception
    {
        internal EosException(long eosErrorCode)
        {
            EosErrorCode = (EosErrorCode)eosErrorCode;
        }

        internal EosException(long eosErrorCode, string message)
            : base(message) 
        {
            EosErrorCode = (EosErrorCode)eosErrorCode;
        }

        internal EosException(long eosErrorCode, string message, Exception innerException)
            : base(message, innerException) 
        {
            EosErrorCode = (EosErrorCode)eosErrorCode;
        }

        public EosErrorCode EosErrorCode { get; }

        public string EosErrorCodeMessage
        {
            get { return Util.ConvertCamelCasedStringToFriendlyString(EosErrorCode.ToString()) + "."; }
        }

        public override string Message
        {
            get { return string.Format("{0}{1}{2}", EosErrorCodeMessage, Environment.NewLine, base.Message); }
        }
    }
}
