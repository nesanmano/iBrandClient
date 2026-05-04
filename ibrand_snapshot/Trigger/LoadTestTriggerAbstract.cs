using System;
using System.Threading;

namespace iBrand_SnapShot.Trigger
{
    internal class LoadTestTriggerAbstract : TriggerAbstract
    {
        private double[] _delays;
        private volatile bool _run = true;
        private Thread _t;

        public override void Start()
        {
            _t = new Thread(DoWork)
            {
                IsBackground = true,
                Name = "LoadTestTriggerAbstract"
            };
            _run = true;
            _t.Start();
        }

        public override void Stop()
        {
            if (_t == null || !_t.IsAlive) return;
            _run = false;
            _t.Interrupt();
        }

        public override void Dispose()
        {
            if (_t == null || !_t.IsAlive) return;
            _run = false;
            _t.Interrupt();
        }

        public override bool Init(string[] parameters)
        {
            if (parameters == null || parameters.Length < 1)
            {
                return false;
            }

            var tmp = new double[parameters.Length];
            try
            {
                for (var i = 0; i < parameters.Length; i++)
                {
                    tmp[i] = double.Parse(parameters[i]);
                }
                _delays = tmp;
            } catch(Exception) {
                return false;
            }

            return true;
        }

        private void DoWork()
        {
            var index = 0;
            var next = CalculateNext(ref index, DateTime.Now);

            while(_run)
            {
                try
                {
                    var now = DateTime.Now;
                    if (now.CompareTo(next) < 0)
                    {
                        var s = next.Subtract(now);
                        Thread.Sleep(s);
                    }
                    else
                    {
                        FireTriggerEvent();
                        next = CalculateNext(ref index, now);
                        var s = next.Subtract(now);
                        Console.Out.WriteLine("Next delay: " + s);
                        Thread.Sleep(s);
                    }
                }
                catch (ThreadInterruptedException)
                {
                    // Do nothing
                }
            }
        }

        private DateTime CalculateNext(ref int index, DateTime now)
        {
            var next = now.AddMinutes(_delays[index]);
            index++;
            if (index >= _delays.Length)
            {
                index = 0;
            }
            return next;
        }

        public override bool IsInitialized()
        {
            return _delays != null;
        }

        public override bool IsRunning()
        {
            return _t != null && _t.IsAlive;
        }
    }
}
