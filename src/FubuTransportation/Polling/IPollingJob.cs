using System;

namespace FubuTransportation.Polling
{
    public interface IPollingJob : IDisposable
    {
        bool IsRunning();
        void Start();
        void RunNow();
        void Stop();
        void ResetInterval(double interval);
    }
}