using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace NomadLogic.iBrandDesktop.Ui.Choice
{
    public partial class ChoiceTimerTextBox : UserControl
    {
        protected delegate void SetTextDelegate(string text);
        protected delegate void EnableButtonDelegate(bool enable);
        public delegate void TimeChangedDelegate(string time, bool reset);

        public event TimeChangedDelegate OnTimerChange;

        private int _time;
        private int _countDownValue;

        private int _maxTime;
        private int _countDown;

        private readonly Timer _timer;

        private readonly Stopwatch _stopWatch;

        private bool _isRunning;

        public ChoiceTimerTextBox()
        {
            InitializeComponent();

            text_box.Enabled = false;

            reset_button.Enabled = false;
            stop_button.Enabled = false;

            reset_button.Click += OnResetButtonClick;
            start_button.Click += OnStartButtonClick;
            stop_button.Click += OnStopButtonClick;


            _timer = new Timer();
            _timer.Interval = 1000; //1 sec
            _timer.Tick += OnTimerEventt;

            _stopWatch = new Stopwatch();
        }

        public void OnResetButtonClick(object sender, EventArgs args)
        {
            _time = 0;

            _countDownValue = _countDown;

            SetTextBoxText(_countDown.ToString());

            EnableStartButton(true);

            if (OnTimerChange != null)
            {
                OnTimerChange(_countDown.ToString(), true);
            }

            _isRunning = false;
        }

        public void OnStartButtonClick(object sender, EventArgs args)
        {
            SetTextBoxText(_countDown.ToString());
            EnableStartButton(false);
            EnableResetButton(false);
            EnableStopButton(true);

            _countDownValue = _countDown;

            _time = 0;
            _timer.Start();
            _isRunning = true;
        }

        public void OnStopButtonClick(object sender, EventArgs args)
        {    
            _timer.Stop();

            if (_stopWatch.IsRunning)
            {
                _stopWatch.Stop();

                var t = TimeSpan.FromMilliseconds(_stopWatch.ElapsedMilliseconds);

                SetTextBoxText(t.ToString(@"ss\.fff"));
            }

            _isRunning = false;

            EnableStopButton(false);

            EnableResetButton(true);
        }

        public bool Running => _isRunning;

        public void OnTimerEventt(object sender, EventArgs e)
        {
            string timerValue;
            
            if (_countDownValue != 0)
            {
                _countDownValue--;

                timerValue = _countDownValue.ToString();
            }
            else
            {
                if (!_stopWatch.IsRunning)
                    _stopWatch.Restart();
                
                _time++;

                timerValue = _time.ToString();
                if (_time == _maxTime)
                {
                    OnStopButtonClick(this, EventArgs.Empty);
                }
            }

            SetTextBoxText(timerValue);

            if (OnTimerChange != null)
                OnTimerChange(timerValue, false);
        }

        public void EnableResetButton(bool enable)
        {
            if (reset_button.InvokeRequired)
            {
                // we were called on a worker thread
                // marshal the call to the user interface thread
                Invoke(new EnableButtonDelegate(EnableResetButton), enable);
                return;
            }

            reset_button.Enabled = enable;

        }

        public void EnableStartButton(bool enable)
        {
            if (start_button.InvokeRequired)
            {
                // we were called on a worker thread
                // marshal the call to the user interface thread
                Invoke(new EnableButtonDelegate(EnableStartButton), enable);
                return;
            }

            start_button.Enabled = enable;

        }

        public void EnableStopButton(bool enable)
        {
            if (stop_button.InvokeRequired)
            {
                // we were called on a worker thread
                // marshal the call to the user interface thread
                Invoke(new EnableButtonDelegate(EnableStopButton), enable);
                return;
            }

            stop_button.Enabled = enable;

        }

        public void SetTextBoxText(string text)
        {
            if (text_box.InvokeRequired)
            {
                // we were called on a worker thread
                // marshal the call to the user interface thread
                Invoke(new SetTextDelegate(SetTextBoxText), text);
                return;
            }

            text_box.Text = text;
            text_box.Invalidate();

        }

        public override string Text => text_box.Text;

        public void SetMaxTime(int maxTime)
        {
            _maxTime = maxTime;
        }

        public void SetCountDown(int countDown)
        {
            _countDown = countDown;
        }

        public void SetSize(Size size)
        {
            Size = size;

            text_box.Width = size.Width - start_button.Width - stop_button.Width - reset_button.Width - 15;

            var initialLocation = start_button.Location;

            start_button.Location = new Point(text_box.Width + 5, initialLocation.Y);

            stop_button.Location = new Point(text_box.Width + start_button.Width + 10, initialLocation.Y);

            reset_button.Location = new Point(text_box.Width + start_button.Width + stop_button.Width + 15, initialLocation.Y);
        }

        public void Reset()
        {
            text_box.Text = "";
        }

        public new bool Enabled
        {
            get
            {
                return base.Enabled;
            }
            set
            {
                text_box.Enabled = false;
                start_button.Enabled = value;
                base.Enabled = value;

                text_box.BackColor = Color.LightGray;
                Reset();
            }
        }
    }
}
