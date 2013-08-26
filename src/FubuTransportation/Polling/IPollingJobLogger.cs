using System;

namespace FubuTransportation.Polling
{
    public interface IPollingJobLogger
    {
        void Stopping(Type jobType);
        void Starting(IJob job);
        void Successful(IJob job);
        void Failed(IJob job, Exception ex);
        void FailedToSchedule(Type jobType, Exception exception);
    }
}