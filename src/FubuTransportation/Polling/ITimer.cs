using System;

namespace FubuTransportation.Polling
{
    public interface ITimer
    {
        void Start(Action callback, double interval);
        void Restart();
        void Stop();

        bool Enabled { get; }
    }
}