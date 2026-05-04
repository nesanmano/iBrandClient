namespace NomadLogic.iBrand.Transformation.Exception
{
    class NoTransformationFoundException : System.Exception
    {

        public string ErrorMessage
        {
        get
        {
            return Message.ToString();
        }
        }

        public NoTransformationFoundException(string errorMessage)
                           : base(errorMessage) {}

        public NoTransformationFoundException(string errorMessage, System.Exception innerEx)
                           : base(errorMessage, innerEx) {}
    }
}
