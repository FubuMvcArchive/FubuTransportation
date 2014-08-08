using System;
using FubuTransportation.Polling;

namespace FubuTransportation.ScheduledJob
{
    public interface IScheduledJobLogger
    {
        void LogAndTimeExecution(IJob job, Action action);
        void LogNextScheduledRun(IJob job, DateTimeOffset nextTime);
    }
}