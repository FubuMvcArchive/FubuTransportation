using System;
using System.Threading.Tasks;
using FubuCore.Reflection;
using FubuTransportation.Polling;
using FubuTransportation.Runtime.Routing;
using FubuTransportation.ScheduledJobs.Execution;
using FubuTransportation.ScheduledJobs.Persistence;

namespace FubuTransportation.ScheduledJobs
{
    public interface IScheduledJob
    {
        Type JobType { get; }
        void Initialize(DateTimeOffset now, IJobExecutor executor, JobSchedule schedule);
        Accessor Channel { get; }
        TimeSpan Timeout { get;}
        IRoutingRule ToRoutingRule();
    }

    public interface IScheduledJob<T> where T : IJob
    {
        void Execute(IJobExecutor executor);
        Task<RescheduleRequest<T>> ToTask(IJob job, IJobRunTracker tracker);
    }
}