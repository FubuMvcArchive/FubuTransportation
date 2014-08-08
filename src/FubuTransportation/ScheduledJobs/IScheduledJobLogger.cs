using System;
using FubuTransportation.Polling;

namespace FubuTransportation.ScheduledJobs
{
    public interface IScheduledJobLogger
    {
        void LogAndTimeExecution(IJob job, Action action);
        void LogNextScheduledRun(IJob job, DateTimeOffset nextTime);
    }
}