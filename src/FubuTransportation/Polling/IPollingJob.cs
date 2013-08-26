namespace FubuTransportation.Polling
{
    public interface IPollingJob
    {
        bool IsRunning();
        void Start();
        void RunNow();
        void Stop();
        void ResetInterval(double interval);

    }
}