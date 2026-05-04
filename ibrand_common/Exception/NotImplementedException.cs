namespace NomadLogic.iBrand.Transformation.Exception
{
    class NotImplementedException : System.Exception
    {

        public string ErrorMessage
        {
        get
        {
            return Message.ToString();
        }
        }

        public NotImplementedException(string errorMessage)
                           : base(errorMessage) {}

        public NotImplementedException(string errorMessage, System.Exception innerEx)
                           : base(errorMessage, innerEx) {}


    }
}
