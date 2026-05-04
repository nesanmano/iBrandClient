namespace NomadLogic.Common.Util.Networking.QuestionnaireWebRequest.Emisys
{
    /// <summary>
    /// Cette classe enumere les differents statuts possible pour les UID mock 
    /// Ces statuts ne sont valables que pour la methode Finalize et Init card
    /// </summary>
    public static class EmisysStatusEnum
    {
        public static  string[] _statusFinalize = { "test_success","test_error","test_inactive"};


        // pour la methode init

        public static  string[] _statusInit = {"test_success","test_unchanged","test_error","test_new_user"};

    }
}
