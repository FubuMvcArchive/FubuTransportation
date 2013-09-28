using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using FubuCore;

namespace FubuTransportation.Scheduling
{
    public class ThreadScheduler : IScheduler
    {
        private readonly Thread[] _threads;
        private bool _stopped = true;

        public int ThreadCount { get { return _threads.Length; }}

        public IEnumerable<Thread> Threads {get { return _threads.Where(x => x != null).ToArray(); }}

        public ThreadScheduler(int threadCount)
        {
            _threads = new Thread[threadCount];
        }

        public void Start(Action action, bool shouldLoop)
        {
            _stopped = false;
            for (int i = 0; i < ThreadCount; ++i)
            {
                var thread = new Thread(() => listen(action, shouldLoop))
                {
                    IsBackground = true,
                    Name = "FubuTransportation Receiving Thread",
                };
                thread.Start();
                _threads[i] = thread;
            }
        }

        private void listen(Action action, bool shouldLoop)
        {
            do
            {
                action();
            } while (!_stopped && shouldLoop);
        }

        public static IScheduler Default()
        {
            return new ThreadScheduler(1);
        }

        public void Dispose()
        {
            if (_stopped)
                return;

            _stopped = true;

            foreach (var thread in _threads)
            {
                if (!thread.Join(5.Seconds().Milliseconds))
                {
                    thread.Abort();
                }
            }
        }

        public override string ToString()
        {
            return "ThreadScheduler Count: " + _threads.Length;
        }
    }
}