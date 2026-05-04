namespace NomadLogic.iBrandDesktop.Util
{
    public class ZipCodeEntry
    {
        private readonly string _city;
        private readonly string _state;
        
        public ZipCodeEntry(string city, string state)
        {
            _city = city;
            _state = state;
        }

        public string City => _city;

        public string State => _state;
    }
}
