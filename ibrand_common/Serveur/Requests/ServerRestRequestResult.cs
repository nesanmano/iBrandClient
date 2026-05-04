using System;

namespace NomadLogic.Common.Serveur.Requests
{
    public class ServerRestRequestResult<T>
    {
        public ServerRestRequestResultType Result { get; set; }
        
        public string MessageUi { get; set; }

        public T Data { get; set; }

        public Exception Error { get; set; }
    }

    public enum ServerRestRequestResultType
    {
        Error,
        Cancelled,
        Failed,
        Success,
        SuccessPartial,
        NoContent,
        Unauthorized
    }
}