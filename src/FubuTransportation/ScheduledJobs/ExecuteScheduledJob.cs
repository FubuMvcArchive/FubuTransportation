using System;
using FubuCore;
using FubuTransportation.Polling;

namespace FubuTransportation.ScheduledJobs
{
    public class ExecuteScheduledJob<T> where T : IJob
    {
        public TimeSpan Timeout = 5.Minutes();
    }
}