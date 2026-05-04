namespace NomadLogic.iBrandDesktop.Util
{
    class Participant
    {
        private readonly int _score;
        private readonly double _time;
        private readonly string _nickName;

        public Participant(int score, double time, string nickName)
        {
            _score = score;
            _time = time;
            _nickName = nickName;
        }

        public int Score => _score;

        public double Time => _time;

        public string NickName => _nickName;
    }
}