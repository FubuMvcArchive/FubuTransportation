using System;
using System.Threading.Tasks;
using FubuTransportation.Polling;

namespace FubuTransportation.ScheduledJobs
{
    public interface IJobExecutor
    {
        Task<JobExecutionRecord> Execute<T>(TimeSpan timeout) where T : IJob;
        void Schedule<T>(IScheduledJob<T> job, DateTimeOffset nextTime) where T : IJob;

        DateTimeOffset Now();
    }
}