using System;
using System.Timers;

namespace FubuTransportation.Polling
{
    public class DefaultTimer : ITimer
    {
        private readonly Timer _timer;
        private Action _callback;

        public DefaultTimer()
        {
            _timer = new Timer { AutoReset = false };
            _timer.Elapsed += elapsedHandler;
        }

        public void Start(Action callback, double interval)
        {
            _callback = callback;

            _timer.Interval = interval;
            _timer.Start();
        }

        public void Stop()
        {
            _timer.Stop();
            _timer.Enabled = false;
        }

        public bool Enabled { get { return _timer.Enabled; } }

        public void Restart()
        {
            _timer.Start();
        }

        private void elapsedHandler(object sender, ElapsedEventArgs eventArgs)
        {
            if (_callback == null) return;
            _callback();

            // TODO -- harden this w/ errors?  Or handle it in the chains?
            _timer.Start();
        }
    }
}