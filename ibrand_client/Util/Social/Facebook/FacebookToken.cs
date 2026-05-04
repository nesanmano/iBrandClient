namespace NomadLogic.iBrandDesktop.Util.Social.Facebook
{
    public class FacebookToken
    {
        private readonly string _pageId;
        private readonly string _token;
        
        public FacebookToken(string pageId, string token)
        {
            _pageId = pageId;
            _token = token;
        }

        public FacebookToken(string token)
        {
            _token = token;
        }

        public string PageId => _pageId;


        public string Token => _token;
    }
}
